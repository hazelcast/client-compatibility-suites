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
var JsonStringDeserializationPolicy_1 = require("./JsonStringDeserializationPolicy");
var StringSerializationPolicy_1 = require("./StringSerializationPolicy");
var SerializationConfig = /** @class */ (function () {
    function SerializationConfig() {
        this.defaultNumberType = 'double';
        this.isBigEndian = true;
        this.dataSerializableFactories = {};
        this.dataSerializableFactoryConfigs = {};
        this.portableFactories = {};
        this.portableFactoryConfigs = {};
        this.portableVersion = 0;
        this.customSerializers = [];
        this.customSerializerConfigs = {};
        this.globalSerializer = null;
        this.globalSerializerConfig = null;
        this.jsonStringDeserializationPolicy = JsonStringDeserializationPolicy_1.JsonStringDeserializationPolicy.EAGER;
        this.stringSerializationPolicy = StringSerializationPolicy_1.StringSerializationPolicy.STANDARD;
    }
    return SerializationConfig;
}());
exports.SerializationConfig = SerializationConfig;
//# sourceMappingURL=SerializationConfig.js.map