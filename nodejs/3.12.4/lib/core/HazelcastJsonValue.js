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
var Util_1 = require("../Util");
/**
 * HazelcastJsonValue is a wrapper for JSON formatted strings. It is preferred
 * to store HazelcastJsonValue instead of Strings for JSON formatted strings.
 * Users can run predicates and use indexes on the attributes of the underlying
 * JSON strings.
 *
 * HazelcastJsonValue is queried using Hazelcast's querying language.
 * See {@link https://github.com/hazelcast/hazelcast-nodejs-client#77-distributed-query Distributed Query section}.
 *
 * In terms of querying, numbers in JSON strings are treated as either
 * Long or Double in the Java side. Strings, booleans and null
 * are treated as their Java counterparts.
 *
 * HazelcastJsonValue keeps given string as it is. Strings are not
 * checked for being valid. Ill-formatted json strings may cause false
 * positive or false negative results in queries.
 *
 * Null values are not allowed.
 */
var HazelcastJsonValue = /** @class */ (function () {
    /**
     * Creates a HazelcastJsonValue from given string.
     * @param jsonString a non null Json string
     */
    function HazelcastJsonValue(jsonString) {
        Util_1.assertString(jsonString);
        this.jsonString = jsonString;
    }
    /**
     * Returns unaltered string that was used to create this object.
     * @return original string
     */
    HazelcastJsonValue.prototype.toString = function () {
        return this.jsonString;
    };
    return HazelcastJsonValue;
}());
exports.HazelcastJsonValue = HazelcastJsonValue;
