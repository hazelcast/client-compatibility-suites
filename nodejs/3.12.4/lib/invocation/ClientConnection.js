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
var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var safe_buffer_1 = require("safe-buffer");
var events_1 = require("events");
var BitsUtil_1 = require("../BitsUtil");
var BuildInfo_1 = require("../BuildInfo");
var HazelcastError_1 = require("../HazelcastError");
var Address = require("../Address");
var Util_1 = require("../Util");
var FROZEN_ARRAY = Object.freeze([]);
var PROPERTY_PIPELINING_ENABLED = 'hazelcast.client.autopipelining.enabled';
var PROPERTY_PIPELINING_THRESHOLD = 'hazelcast.client.autopipelining.threshold.bytes';
var PROPERTY_NO_DELAY = 'hazelcast.client.socket.no.delay';
var PipelinedWriter = /** @class */ (function (_super) {
    __extends(PipelinedWriter, _super);
    function PipelinedWriter(socket, threshold) {
        var _this = _super.call(this) || this;
        _this.queue = [];
        _this.scheduled = false;
        _this.socket = socket;
        _this.threshold = threshold;
        return _this;
    }
    PipelinedWriter.prototype.write = function (buffer, resolver) {
        var _this = this;
        if (this.error) {
            // if there was a write error, it's useless to keep writing to the socket
            return process.nextTick(function () { return resolver.reject(_this.error); });
        }
        this.queue.push({ buffer: buffer, resolver: resolver });
        this.schedule();
    };
    PipelinedWriter.prototype.schedule = function () {
        var _this = this;
        if (!this.scheduled) {
            this.scheduled = true;
            // nextTick allows queue to be processed on the current event loop phase
            process.nextTick(function () { return _this.process(); });
        }
    };
    PipelinedWriter.prototype.process = function () {
        var _this = this;
        if (this.error) {
            return;
        }
        var buffers = [];
        var resolvers = [];
        var totalLength = 0;
        while (this.queue.length > 0 && totalLength < this.threshold) {
            var item = this.queue.shift();
            var data = item.buffer;
            totalLength += data.length;
            buffers.push(data);
            resolvers.push(item.resolver);
        }
        if (totalLength === 0) {
            this.scheduled = false;
            return;
        }
        // coalesce buffers and write to the socket: no further writes until flushed
        var merged = buffers.length === 1 ? buffers[0] : safe_buffer_1.Buffer.concat(buffers, totalLength);
        this.socket.write(merged, function (err) {
            if (err) {
                _this.handleError(err, resolvers);
                return;
            }
            _this.emit('write');
            for (var _i = 0, resolvers_1 = resolvers; _i < resolvers_1.length; _i++) {
                var r = resolvers_1[_i];
                r.resolve();
            }
            if (_this.queue.length === 0) {
                // will start running on the next message
                _this.scheduled = false;
                return;
            }
            // setImmediate allows IO between writes
            setImmediate(function () { return _this.process(); });
        });
    };
    PipelinedWriter.prototype.handleError = function (err, sentResolvers) {
        this.error = new HazelcastError_1.IOError(err);
        for (var _i = 0, sentResolvers_1 = sentResolvers; _i < sentResolvers_1.length; _i++) {
            var r = sentResolvers_1[_i];
            r.reject(this.error);
        }
        // no more items can be added now
        var q = this.queue;
        this.queue = FROZEN_ARRAY;
        for (var _a = 0, q_1 = q; _a < q_1.length; _a++) {
            var it = q_1[_a];
            it.resolver.reject(this.error);
        }
    };
    return PipelinedWriter;
}(events_1.EventEmitter));
exports.PipelinedWriter = PipelinedWriter;
var DirectWriter = /** @class */ (function (_super) {
    __extends(DirectWriter, _super);
    function DirectWriter(socket) {
        var _this = _super.call(this) || this;
        _this.socket = socket;
        return _this;
    }
    DirectWriter.prototype.write = function (buffer, resolver) {
        var _this = this;
        this.socket.write(buffer, function (err) {
            if (err) {
                resolver.reject(new HazelcastError_1.IOError(err));
                return;
            }
            _this.emit('write');
            resolver.resolve();
        });
    };
    return DirectWriter;
}(events_1.EventEmitter));
exports.DirectWriter = DirectWriter;
var FrameReader = /** @class */ (function () {
    function FrameReader() {
        this.chunks = [];
        this.chunksTotalSize = 0;
        this.frameSize = 0;
    }
    FrameReader.prototype.append = function (buffer) {
        this.chunksTotalSize += buffer.length;
        this.chunks.push(buffer);
    };
    FrameReader.prototype.read = function () {
        if (this.chunksTotalSize < BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES) {
            return null;
        }
        if (this.frameSize === 0) {
            this.frameSize = this.readFrameSize();
        }
        if (this.chunksTotalSize < this.frameSize) {
            return null;
        }
        var frame = this.chunks.length === 1 ? this.chunks[0] : safe_buffer_1.Buffer.concat(this.chunks, this.chunksTotalSize);
        if (this.chunksTotalSize > this.frameSize) {
            if (this.chunks.length === 1) {
                this.chunks[0] = frame.slice(this.frameSize);
            }
            else {
                this.chunks = [frame.slice(this.frameSize)];
            }
            frame = frame.slice(0, this.frameSize);
        }
        else {
            this.chunks = [];
        }
        this.chunksTotalSize -= this.frameSize;
        this.frameSize = 0;
        return frame;
    };
    FrameReader.prototype.readFrameSize = function () {
        if (this.chunks[0].length >= BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES) {
            return this.chunks[0].readInt32LE(0);
        }
        var readChunksSize = 0;
        for (var i = 0; i < this.chunks.length; i++) {
            readChunksSize += this.chunks[i].length;
            if (readChunksSize >= BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES) {
                var merged = safe_buffer_1.Buffer.concat(this.chunks.slice(0, i + 1), readChunksSize);
                return merged.readInt32LE(0);
            }
        }
        throw new Error('Detected illegal internal call in FrameReader!');
    };
    return FrameReader;
}());
exports.FrameReader = FrameReader;
var ClientConnection = /** @class */ (function () {
    function ClientConnection(client, address, socket) {
        var _this = this;
        this.heartbeating = true;
        this.startTime = Date.now();
        var enablePipelining = client.getConfig().properties[PROPERTY_PIPELINING_ENABLED];
        var pipeliningThreshold = client.getConfig().properties[PROPERTY_PIPELINING_THRESHOLD];
        var noDelay = client.getConfig().properties[PROPERTY_NO_DELAY];
        socket.setNoDelay(noDelay);
        this.client = client;
        this.socket = socket;
        this.address = address;
        this.localAddress = new Address(socket.localAddress, socket.localPort);
        this.lastReadTimeMillis = 0;
        this.closedTime = 0;
        this.connectedServerVersionString = null;
        this.connectedServerVersion = BuildInfo_1.BuildInfo.UNKNOWN_VERSION_ID;
        this.writer = enablePipelining ? new PipelinedWriter(socket, pipeliningThreshold) : new DirectWriter(socket);
        this.writer.on('write', function () {
            _this.lastWriteTimeMillis = Date.now();
        });
        this.reader = new FrameReader();
    }
    /**
     * Returns the address of local port that is associated with this connection.
     * @returns
     */
    ClientConnection.prototype.getLocalAddress = function () {
        return this.localAddress;
    };
    /**
     * Returns the address of remote node that is associated with this connection.
     * @returns
     */
    ClientConnection.prototype.getAddress = function () {
        return this.address;
    };
    ClientConnection.prototype.setAddress = function (address) {
        this.address = address;
    };
    ClientConnection.prototype.write = function (buffer) {
        var deferred = Util_1.DeferredPromise();
        this.writer.write(buffer, deferred);
        return deferred.promise;
    };
    ClientConnection.prototype.setConnectedServerVersion = function (versionString) {
        this.connectedServerVersionString = versionString;
        this.connectedServerVersion = BuildInfo_1.BuildInfo.calculateServerVersionFromString(versionString);
    };
    ClientConnection.prototype.getConnectedServerVersion = function () {
        return this.connectedServerVersion;
    };
    /**
     * Closes this connection.
     */
    ClientConnection.prototype.close = function () {
        this.socket.end();
        this.closedTime = Date.now();
    };
    ClientConnection.prototype.isAlive = function () {
        return this.closedTime === 0;
    };
    ClientConnection.prototype.isHeartbeating = function () {
        return this.heartbeating;
    };
    ClientConnection.prototype.setHeartbeating = function (heartbeating) {
        this.heartbeating = heartbeating;
    };
    ClientConnection.prototype.isAuthenticatedAsOwner = function () {
        return this.authenticatedAsOwner;
    };
    ClientConnection.prototype.setAuthenticatedAsOwner = function (asOwner) {
        this.authenticatedAsOwner = asOwner;
    };
    ClientConnection.prototype.getStartTime = function () {
        return this.startTime;
    };
    ClientConnection.prototype.getLastReadTimeMillis = function () {
        return this.lastReadTimeMillis;
    };
    ClientConnection.prototype.getLastWriteTimeMillis = function () {
        return this.lastWriteTimeMillis;
    };
    ClientConnection.prototype.toString = function () {
        return this.address.toString();
    };
    /**
     * Registers a function to pass received data on 'data' events on this connection.
     * @param callback
     */
    ClientConnection.prototype.registerResponseCallback = function (callback) {
        var _this = this;
        this.socket.on('data', function (buffer) {
            _this.lastReadTimeMillis = Date.now();
            _this.reader.append(buffer);
            var frame = _this.reader.read();
            while (frame !== null) {
                callback(frame);
                frame = _this.reader.read();
            }
        });
        this.socket.on('error', function (e) {
            if (e.code === 'EPIPE' || e.code === 'ECONNRESET') {
                _this.client.getConnectionManager().destroyConnection(_this.address);
            }
        });
    };
    return ClientConnection;
}());
exports.ClientConnection = ClientConnection;
