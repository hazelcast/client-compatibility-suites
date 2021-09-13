"use strict";
/*
 * Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
var Address = require("../Address");
var Util_1 = require("../Util");
var https_1 = require("https");
var URL = require("url");
/**
 * Discovery service that discover nodes via hazelcast.cloud
 * https://coordinator.hazelcast.cloud/cluster/discovery?token=<TOKEN>
 */
var HazelcastCloudDiscovery = /** @class */ (function () {
    function HazelcastCloudDiscovery(endpointUrl, connectionTimeoutInMillis) {
        this.endpointUrl = endpointUrl;
        this.connectionTimeoutInMillis = connectionTimeoutInMillis;
    }
    HazelcastCloudDiscovery.createUrlEndpoint = function (properties, cloudToken) {
        var cloudBaseUrl = properties[HazelcastCloudDiscovery.CLOUD_URL_BASE_PROPERTY];
        return cloudBaseUrl + this.CLOUD_URL_PATH + cloudToken;
    };
    HazelcastCloudDiscovery.prototype.discoverNodes = function () {
        return this.callService().catch(function (e) {
            throw e;
        });
    };
    HazelcastCloudDiscovery.prototype.callService = function () {
        var _this = this;
        var deferred = Util_1.DeferredPromise();
        var url = URL.parse(this.endpointUrl);
        var endpointUrlOptions = {
            host: url.host,
            path: url.path,
        };
        var dataAsAString = '';
        https_1.get(endpointUrlOptions, function (res) {
            res.setEncoding('utf8');
            res.on('data', function (chunk) {
                dataAsAString += chunk;
            });
            res.on('end', function () {
                deferred.resolve(_this.parseResponse(dataAsAString));
            });
        }).on('error', function (e) {
            deferred.reject(e);
        });
        return deferred.promise;
    };
    HazelcastCloudDiscovery.prototype.parseResponse = function (data) {
        var jsonValue = JSON.parse(data);
        var privateToPublicAddresses = new Map();
        for (var _i = 0, jsonValue_1 = jsonValue; _i < jsonValue_1.length; _i++) {
            var value = jsonValue_1[_i];
            var privateAddress = value[HazelcastCloudDiscovery.PRIVATE_ADDRESS_PROPERTY];
            var publicAddress = value[HazelcastCloudDiscovery.PUBLIC_ADDRESS_PROPERTY];
            var publicAddr = Util_1.AddressHelper.createAddressFromString(publicAddress.toString());
            privateToPublicAddresses.set(new Address(privateAddress, publicAddr.port).toString(), publicAddr);
        }
        return privateToPublicAddresses;
    };
    /**
     * Internal client property to change base url of cloud discovery endpoint.
     * Used for testing cloud discovery.
     */
    HazelcastCloudDiscovery.CLOUD_URL_BASE_PROPERTY = 'hazelcast.client.cloud.url';
    HazelcastCloudDiscovery.CLOUD_URL_PATH = '/cluster/discovery?token=';
    HazelcastCloudDiscovery.PRIVATE_ADDRESS_PROPERTY = 'private-address';
    HazelcastCloudDiscovery.PUBLIC_ADDRESS_PROPERTY = 'public-address';
    return HazelcastCloudDiscovery;
}());
exports.HazelcastCloudDiscovery = HazelcastCloudDiscovery;
//# sourceMappingURL=HazelcastCloudDiscovery.js.map