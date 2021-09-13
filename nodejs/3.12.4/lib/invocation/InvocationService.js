"use strict";
/*
 * Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
Object.defineProperty(exports, "__esModule", { value: true });
var assert = require("assert");
var Promise = require("bluebird");
var BitsUtil_1 = require("../BitsUtil");
var HazelcastError_1 = require("../HazelcastError");
var Util_1 = require("../Util");
var ClientMessage = require("../ClientMessage");
var EXCEPTION_MESSAGE_TYPE = 109;
var MAX_FAST_INVOCATION_COUNT = 5;
var PROPERTY_INVOCATION_RETRY_PAUSE_MILLIS = 'hazelcast.client.invocation.retry.pause.millis';
var PROPERTY_INVOCATION_TIMEOUT_MILLIS = 'hazelcast.client.invocation.timeout.millis';
var PROPERTY_CLEAN_RESOURCES_MILLIS = 'hazelcast.client.internal.clean.resources.millis';
/**
 * A request to be sent to a hazelcast node.
 */
var Invocation = /** @class */ (function () {
    function Invocation(client, request, timeoutMillis) {
        this.invokeCount = 0;
        this.client = client;
        this.invocationService = client.getInvocationService();
        this.deadline = timeoutMillis === undefined
            ? Date.now() + this.invocationService.getInvocationTimeoutMillis()
            : Date.now() + timeoutMillis;
        this.request = request;
    }
    Invocation.isRetrySafeError = function (err) {
        return err instanceof HazelcastError_1.IOError
            || err instanceof HazelcastError_1.HazelcastInstanceNotActiveError
            || err instanceof HazelcastError_1.RetryableHazelcastError;
    };
    Invocation.prototype.shouldRetry = function (err) {
        if (Invocation.isRetrySafeError(err)) {
            return true;
        }
        if (err instanceof HazelcastError_1.TargetDisconnectedError) {
            return this.request.isRetryable() || this.invocationService.redoOperationEnabled();
        }
        return false;
    };
    /**
     * @returns {boolean}
     */
    Invocation.prototype.hasPartitionId = function () {
        return this.hasOwnProperty('partitionId') && this.partitionId >= 0;
    };
    Invocation.prototype.isAllowedToRetryOnSelection = function (err) {
        return (this.connection == null && this.address == null) || !(err instanceof HazelcastError_1.IOError);
    };
    return Invocation;
}());
exports.Invocation = Invocation;
/**
 * Sends requests to appropriate nodes. Resolves waiting promises with responses.
 */
var InvocationService = /** @class */ (function () {
    function InvocationService(hazelcastClient) {
        this.correlationCounter = 1;
        this.eventHandlers = new Map();
        this.pending = new Map();
        this.client = hazelcastClient;
        this.logger = this.client.getLoggingService().getLogger();
        var config = hazelcastClient.getConfig();
        if (config.networkConfig.smartRouting) {
            this.doInvoke = this.invokeSmart;
        }
        else {
            this.doInvoke = this.invokeNonSmart;
        }
        this.invocationRetryPauseMillis = config.properties[PROPERTY_INVOCATION_RETRY_PAUSE_MILLIS];
        this.invocationTimeoutMillis = config.properties[PROPERTY_INVOCATION_TIMEOUT_MILLIS];
        this.cleanResourcesMillis = config.properties[PROPERTY_CLEAN_RESOURCES_MILLIS];
        this.redoOperation = config.networkConfig.redoOperation;
        this.isShutdown = false;
    }
    InvocationService.prototype.start = function () {
        this.cleanResourcesTask = this.scheduleCleanResourcesTask(this.cleanResourcesMillis);
    };
    InvocationService.prototype.shutdown = function () {
        var _this = this;
        if (this.isShutdown) {
            return;
        }
        this.isShutdown = true;
        if (this.cleanResourcesTask !== undefined) {
            Util_1.cancelRepetitionTask(this.cleanResourcesTask);
        }
        this.pending.forEach(function (invocation) {
            _this.notifyError(invocation, new HazelcastError_1.ClientNotActiveError('Client is shutting down.'));
        });
    };
    InvocationService.prototype.redoOperationEnabled = function () {
        return this.redoOperation;
    };
    InvocationService.prototype.invoke = function (invocation) {
        invocation.deferred = Util_1.DeferredPromise();
        var newCorrelationId = this.correlationCounter++;
        invocation.request.setCorrelationId(newCorrelationId);
        this.doInvoke(invocation);
        return invocation.deferred.promise;
    };
    /**
     * Invokes given invocation on specified connection.
     * @param connection
     * @param request
     * @param handler called with values returned from server for this invocation.
     * @returns
     */
    InvocationService.prototype.invokeOnConnection = function (connection, request, handler) {
        var invocation = new Invocation(this.client, request);
        invocation.connection = connection;
        if (handler) {
            invocation.handler = handler;
        }
        return this.invoke(invocation);
    };
    /**
     * Invokes given invocation on the node that owns given partition.
     * @param request
     * @param partitionId
     * @param timeoutMillis optional override for the invocation timeout
     * @returns
     */
    InvocationService.prototype.invokeOnPartition = function (request, partitionId, timeoutMillis) {
        var invocation = new Invocation(this.client, request, timeoutMillis);
        invocation.partitionId = partitionId;
        return this.invoke(invocation);
    };
    /**
     * Invokes given invocation on the host with given address.
     * @param request
     * @param target
     * @returns
     */
    InvocationService.prototype.invokeOnTarget = function (request, target) {
        var invocation = new Invocation(this.client, request);
        invocation.address = target;
        return this.invoke(invocation);
    };
    /**
     * Invokes given invocation on any host.
     * Useful when an operation is not bound to any host but a generic operation.
     * @param request
     * @returns
     */
    InvocationService.prototype.invokeOnRandomTarget = function (request) {
        return this.invoke(new Invocation(this.client, request));
    };
    InvocationService.prototype.getInvocationTimeoutMillis = function () {
        return this.invocationTimeoutMillis;
    };
    InvocationService.prototype.getInvocationRetryPauseMillis = function () {
        return this.invocationRetryPauseMillis;
    };
    /**
     * Removes the handler for all event handlers with a specific correlation id.
     * @param id correlation id
     */
    InvocationService.prototype.removeEventHandler = function (id) {
        if (this.eventHandlers.hasOwnProperty('' + id)) {
            this.eventHandlers.delete(id);
        }
    };
    /**
     * Extract codec specific properties in a protocol message and resolves waiting promise.
     * @param buffer
     */
    InvocationService.prototype.processResponse = function (buffer) {
        var _this = this;
        var clientMessage = new ClientMessage(buffer);
        var correlationId = clientMessage.getCorrelationId();
        var messageType = clientMessage.getMessageType();
        if (clientMessage.hasFlags(BitsUtil_1.BitsUtil.LISTENER_FLAG)) {
            setImmediate(function () {
                var invocation = _this.eventHandlers.get(correlationId);
                if (invocation !== undefined) {
                    invocation.handler(clientMessage);
                }
            });
            return;
        }
        var pendingInvocation = this.pending.get(correlationId);
        if (pendingInvocation === undefined) {
            if (!this.isShutdown) {
                this.logger.warn('InvocationService', 'Found no registration for invocation id ' + correlationId);
            }
            return;
        }
        var deferred = pendingInvocation.deferred;
        if (messageType === EXCEPTION_MESSAGE_TYPE) {
            var remoteError = this.client.getErrorFactory().createErrorFromClientMessage(clientMessage);
            this.notifyError(pendingInvocation, remoteError);
        }
        else {
            this.pending.delete(correlationId);
            deferred.resolve(clientMessage);
        }
    };
    InvocationService.prototype.scheduleCleanResourcesTask = function (periodMillis) {
        var _this = this;
        return Util_1.scheduleWithRepetition(function () {
            _this.pending.forEach(function (invocation) {
                var connection = invocation.sendConnection;
                if (connection === undefined) {
                    return;
                }
                if (!connection.isAlive()) {
                    _this.notifyError(invocation, new HazelcastError_1.TargetDisconnectedError('Target member disconnected.'));
                }
            });
        }, periodMillis, periodMillis);
    };
    InvocationService.prototype.invokeSmart = function (invocation) {
        var _this = this;
        var invocationPromise;
        invocation.invokeCount++;
        if (invocation.hasOwnProperty('connection')) {
            invocationPromise = this.send(invocation, invocation.connection);
        }
        else if (invocation.hasPartitionId()) {
            invocationPromise = this.invokeOnPartitionOwner(invocation, invocation.partitionId);
        }
        else if (invocation.hasOwnProperty('address')) {
            invocationPromise = this.invokeOnAddress(invocation, invocation.address);
        }
        else {
            invocationPromise = this.invokeOnOwner(invocation);
        }
        invocationPromise.catch(function (err) {
            _this.notifyError(invocation, err);
        });
    };
    InvocationService.prototype.invokeNonSmart = function (invocation) {
        var _this = this;
        var invocationPromise;
        invocation.invokeCount++;
        if (invocation.hasOwnProperty('connection')) {
            invocationPromise = this.send(invocation, invocation.connection);
        }
        else {
            invocationPromise = this.invokeOnOwner(invocation);
        }
        invocationPromise.catch(function (err) {
            _this.notifyError(invocation, err);
        });
    };
    InvocationService.prototype.invokeOnOwner = function (invocation) {
        var owner = this.client.getClusterService().getOwnerConnection();
        if (owner == null) {
            return Promise.reject(new HazelcastError_1.IOError('Unisocket client\'s owner connection is not available.'));
        }
        return this.send(invocation, owner);
    };
    InvocationService.prototype.invokeOnAddress = function (invocation, address) {
        var _this = this;
        return this.client.getConnectionManager().getOrConnect(address).then(function (connection) {
            return _this.send(invocation, connection);
        }).catch(function (e) {
            _this.logger.debug('InvocationService', e);
            throw new HazelcastError_1.IOError(address.toString() + ' is not available.', e);
        });
    };
    InvocationService.prototype.invokeOnPartitionOwner = function (invocation, partitionId) {
        var _this = this;
        var ownerAddress = this.client.getPartitionService().getAddressForPartition(partitionId);
        return this.client.getConnectionManager().getOrConnect(ownerAddress).then(function (connection) {
            return _this.send(invocation, connection);
        }).catch(function (e) {
            _this.logger.debug('InvocationService', e);
            throw new HazelcastError_1.IOError(ownerAddress.toString() + '(partition owner) is not available.', e);
        });
    };
    InvocationService.prototype.send = function (invocation, connection) {
        assert(connection != null);
        if (this.isShutdown) {
            return Promise.reject(new HazelcastError_1.ClientNotActiveError('Client is shutdown.'));
        }
        this.registerInvocation(invocation);
        return connection.write(invocation.request.getBuffer()).then(function () {
            invocation.sendConnection = connection;
        });
    };
    InvocationService.prototype.notifyError = function (invocation, error) {
        var correlationId = invocation.request.getCorrelationId();
        if (this.rejectIfNotRetryable(invocation, error)) {
            this.pending.delete(correlationId);
            return;
        }
        this.logger.debug('InvocationService', 'Retrying(' + invocation.invokeCount + ') on correlation-id=' + correlationId, error);
        if (invocation.invokeCount < MAX_FAST_INVOCATION_COUNT) {
            this.doInvoke(invocation);
        }
        else {
            setTimeout(this.doInvoke.bind(this, invocation), this.getInvocationRetryPauseMillis());
        }
    };
    /**
     * Determines if an error is retryable. The given invocation is rejected with approprate error if the error is not retryable.
     * @param invocation
     * @param error
     * @returns `true` if invocation is rejected, `false` otherwise
     */
    InvocationService.prototype.rejectIfNotRetryable = function (invocation, error) {
        if (!this.client.getLifecycleService().isRunning()) {
            invocation.deferred.reject(new HazelcastError_1.ClientNotActiveError('Client is not active.', error));
            return true;
        }
        if (!invocation.isAllowedToRetryOnSelection(error)) {
            invocation.deferred.reject(error);
            return true;
        }
        if (!invocation.shouldRetry(error)) {
            invocation.deferred.reject(error);
            return true;
        }
        if (invocation.deadline < Date.now()) {
            this.logger.trace('InvocationService', 'Error will not be retried because invocation timed out');
            invocation.deferred.reject(new HazelcastError_1.InvocationTimeoutError('Invocation ' + invocation.request.getCorrelationId() + ')'
                + ' reached its deadline.', error));
            return true;
        }
    };
    InvocationService.prototype.registerInvocation = function (invocation) {
        var message = invocation.request;
        var correlationId = message.getCorrelationId();
        if (invocation.hasPartitionId()) {
            message.setPartitionId(invocation.partitionId);
        }
        else {
            message.setPartitionId(-1);
        }
        if (invocation.hasOwnProperty('handler')) {
            this.eventHandlers.set(correlationId, invocation);
        }
        this.pending.set(correlationId, invocation);
    };
    return InvocationService;
}());
exports.InvocationService = InvocationService;
