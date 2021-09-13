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
/* tslint:disable:no-bitwise */
var safe_buffer_1 = require("safe-buffer");
var assert = require("assert");
var Long = require("long");
var BitsUtil_1 = require("../BitsUtil");
var HeapData_1 = require("./HeapData");
var OUTPUT_BUFFER_INITIAL_SIZE = 1024;
var MASK_1BYTE = (1 << 8) - 1;
var MASK_2BYTE = (1 << 16) - 1;
var MASK_4BYTE = (1 << 32) - 1;
var ObjectDataOutput = /** @class */ (function () {
    function ObjectDataOutput(service, isBigEndian, isStandardUTF) {
        this.buffer = safe_buffer_1.Buffer.allocUnsafe(OUTPUT_BUFFER_INITIAL_SIZE);
        this.service = service;
        this.bigEndian = isBigEndian;
        this.standardUTF = isStandardUTF;
        this.pos = 0;
    }
    ObjectDataOutput.prototype.clear = function () {
        this.buffer = safe_buffer_1.Buffer.allocUnsafe(this.buffer.length);
        this.pos = 0;
    };
    ObjectDataOutput.prototype.isBigEndian = function () {
        return this.bigEndian;
    };
    ObjectDataOutput.prototype.position = function (newPosition) {
        var oldPos = this.pos;
        if (Number.isInteger(newPosition)) {
            this.pos = newPosition;
        }
        return oldPos;
    };
    ObjectDataOutput.prototype.toBuffer = function () {
        return this.buffer.slice(0, this.pos);
    };
    ObjectDataOutput.prototype.write = function (byte) {
        if (safe_buffer_1.Buffer.isBuffer(byte)) {
            this.ensureAvailable(byte.length);
            byte.copy(this.buffer, this.pos);
            this.pos += byte.length;
        }
        else {
            this.ensureAvailable(BitsUtil_1.BitsUtil.BYTE_SIZE_IN_BYTES);
            BitsUtil_1.BitsUtil.writeUInt8(this.buffer, this.pos, byte & MASK_1BYTE);
            this.pos += BitsUtil_1.BitsUtil.BYTE_SIZE_IN_BYTES;
        }
    };
    ObjectDataOutput.prototype.writeBoolean = function (val) {
        this.write(val ? 1 : 0);
    };
    ObjectDataOutput.prototype.writeBooleanArray = function (val) {
        this.writeArray(this.writeBoolean, val);
    };
    ObjectDataOutput.prototype.writeByte = function (byte) {
        this.write(byte);
    };
    ObjectDataOutput.prototype.writeByteArray = function (bytes) {
        this.writeArray(this.writeByte, bytes);
    };
    ObjectDataOutput.prototype.writeBytes = function (bytes) {
        var len = (bytes != null) ? bytes.length : 0;
        for (var i = 0; i < len; i++) {
            this.write(bytes.charCodeAt(i));
        }
    };
    ObjectDataOutput.prototype.writeChar = function (char) {
        this.ensureAvailable(BitsUtil_1.BitsUtil.CHAR_SIZE_IN_BYTES);
        BitsUtil_1.BitsUtil.writeUInt16(this.buffer, this.pos, char.charCodeAt(0), this.isBigEndian());
        this.pos += BitsUtil_1.BitsUtil.CHAR_SIZE_IN_BYTES;
    };
    ObjectDataOutput.prototype.writeCharArray = function (chars) {
        this.writeArray(this.writeChar, chars);
    };
    ObjectDataOutput.prototype.writeChars = function (chars) {
        var len = (chars != null) ? chars.length : BitsUtil_1.BitsUtil.NULL_ARRAY_LENGTH;
        this.writeInt(len);
        for (var i = 0; i < len; i++) {
            this.writeChar(chars.charAt(i));
        }
    };
    ObjectDataOutput.prototype.writeData = function (data) {
        var buf = (data != null) ? data.toBuffer() : null;
        var len = (buf != null) ? buf.length : BitsUtil_1.BitsUtil.NULL_ARRAY_LENGTH;
        this.writeInt(len);
        for (var i = 0; i < len; i++) {
            this.write(buf[i]);
        }
    };
    ObjectDataOutput.prototype.writeDouble = function (double) {
        this.ensureAvailable(BitsUtil_1.BitsUtil.DOUBLE_SIZE_IN_BYTES);
        BitsUtil_1.BitsUtil.writeDouble(this.buffer, this.pos, double, this.isBigEndian());
        this.pos += BitsUtil_1.BitsUtil.DOUBLE_SIZE_IN_BYTES;
    };
    ObjectDataOutput.prototype.writeDoubleArray = function (doubles) {
        this.writeArray(this.writeDouble, doubles);
    };
    ObjectDataOutput.prototype.writeFloat = function (float) {
        this.ensureAvailable(BitsUtil_1.BitsUtil.FLOAT_SIZE_IN_BYTES);
        BitsUtil_1.BitsUtil.writeFloat(this.buffer, this.pos, float, this.isBigEndian());
        this.pos += BitsUtil_1.BitsUtil.FLOAT_SIZE_IN_BYTES;
    };
    ObjectDataOutput.prototype.writeFloatArray = function (floats) {
        this.writeArray(this.writeFloat, floats);
    };
    ObjectDataOutput.prototype.writeInt = function (int) {
        this.ensureAvailable(BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES);
        BitsUtil_1.BitsUtil.writeInt32(this.buffer, this.pos, int, this.isBigEndian());
        this.pos += BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES;
    };
    ObjectDataOutput.prototype.writeIntBE = function (int) {
        this.ensureAvailable(BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES);
        BitsUtil_1.BitsUtil.writeInt32(this.buffer, this.pos, int, true);
        this.pos += BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES;
    };
    ObjectDataOutput.prototype.writeIntArray = function (ints) {
        this.writeArray(this.writeInt, ints);
    };
    ObjectDataOutput.prototype.writeLong = function (long) {
        this.ensureAvailable(BitsUtil_1.BitsUtil.LONG_SIZE_IN_BYTES);
        if (this.isBigEndian()) {
            BitsUtil_1.BitsUtil.writeInt32(this.buffer, this.pos, long.high, true);
            this.pos += BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES;
            BitsUtil_1.BitsUtil.writeInt32(this.buffer, this.pos, long.low, true);
            this.pos += BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES;
        }
        else {
            BitsUtil_1.BitsUtil.writeInt32(this.buffer, this.pos, long.low, false);
            this.pos += BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES;
            BitsUtil_1.BitsUtil.writeInt32(this.buffer, this.pos, long.high, false);
            this.pos += BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES;
        }
    };
    ObjectDataOutput.prototype.writeLongArray = function (longs) {
        this.writeArray(this.writeLong, longs);
    };
    ObjectDataOutput.prototype.writeObject = function (object) {
        this.service.writeObject(this, object);
    };
    ObjectDataOutput.prototype.writeShort = function (short) {
        this.ensureAvailable(BitsUtil_1.BitsUtil.SHORT_SIZE_IN_BYTES);
        BitsUtil_1.BitsUtil.writeInt16(this.buffer, this.pos, short, this.isBigEndian());
        this.pos += BitsUtil_1.BitsUtil.SHORT_SIZE_IN_BYTES;
    };
    ObjectDataOutput.prototype.writeShortArray = function (shorts) {
        this.writeArray(this.writeShort, shorts);
    };
    ObjectDataOutput.prototype.writeUTF = function (val) {
        if (this.standardUTF) {
            this.writeUTFStandard(val);
        }
        else {
            this.writeUTFLegacy(val);
        }
    };
    ObjectDataOutput.prototype.writeUTFArray = function (val) {
        this.writeArray(this.writeUTF, val);
    };
    ObjectDataOutput.prototype.writeZeroBytes = function (count) {
        for (var i = 0; i < count; i++) {
            this.write(0);
        }
    };
    ObjectDataOutput.prototype.available = function () {
        return this.buffer == null ? 0 : this.buffer.length - this.pos;
    };
    ObjectDataOutput.prototype.ensureAvailable = function (size) {
        if (this.available() < size) {
            var newBuffer = safe_buffer_1.Buffer.allocUnsafe(this.pos + size);
            this.buffer.copy(newBuffer, 0, 0, this.pos);
            this.buffer = newBuffer;
        }
    };
    ObjectDataOutput.prototype.writeArray = function (func, arr) {
        var len = (arr != null) ? arr.length : BitsUtil_1.BitsUtil.NULL_ARRAY_LENGTH;
        this.writeInt(len);
        if (len > 0) {
            var boundFunc = func.bind(this);
            arr.forEach(boundFunc);
        }
    };
    ObjectDataOutput.prototype.writeUTFStandard = function (val) {
        var len = (val != null) ? val.length : BitsUtil_1.BitsUtil.NULL_ARRAY_LENGTH;
        this.writeInt(len);
        if (len === BitsUtil_1.BitsUtil.NULL_ARRAY_LENGTH) {
            return;
        }
        var byteLen = safe_buffer_1.Buffer.byteLength(val, 'utf8');
        this.ensureAvailable(byteLen);
        this.buffer.write(val, this.pos, this.pos + byteLen, 'utf8');
        this.pos += byteLen;
    };
    ObjectDataOutput.prototype.writeUTFLegacy = function (val) {
        var len = (val != null) ? val.length : BitsUtil_1.BitsUtil.NULL_ARRAY_LENGTH;
        this.writeInt(len);
        this.ensureAvailable(len * 3);
        for (var i = 0; i < len; i++) {
            var ch = val.charCodeAt(i);
            if (ch <= 0x007F) {
                this.writeByte(ch);
            }
            else if (ch <= 0x07FF) {
                this.write(0xC0 | ch >> 6 & 0x1F);
                this.write(0x80 | ch & 0x3F);
            }
            else {
                this.write(0xE0 | ch >> 12 & 0x0F);
                this.write(0x80 | ch >> 6 & 0x3F);
                this.write(0x80 | ch & 0x3F);
            }
        }
    };
    return ObjectDataOutput;
}());
exports.ObjectDataOutput = ObjectDataOutput;
var PositionalObjectDataOutput = /** @class */ (function (_super) {
    __extends(PositionalObjectDataOutput, _super);
    function PositionalObjectDataOutput() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    PositionalObjectDataOutput.prototype.pwrite = function (position, byte) {
        if (safe_buffer_1.Buffer.isBuffer(byte)) {
            byte.copy(this.buffer, position);
        }
        else {
            this.buffer[position] = byte;
        }
    };
    PositionalObjectDataOutput.prototype.pwriteBoolean = function (position, val) {
        this.pwrite(position, val ? 1 : 0);
    };
    PositionalObjectDataOutput.prototype.pwriteByte = function (position, byte) {
        this.pwrite(position, byte);
    };
    PositionalObjectDataOutput.prototype.pwriteChar = function (position, char) {
        BitsUtil_1.BitsUtil.writeUInt16(this.buffer, position, char.charCodeAt(0), this.isBigEndian());
    };
    PositionalObjectDataOutput.prototype.pwriteDouble = function (position, double) {
        BitsUtil_1.BitsUtil.writeDouble(this.buffer, position, double, this.isBigEndian());
    };
    PositionalObjectDataOutput.prototype.pwriteFloat = function (position, float) {
        BitsUtil_1.BitsUtil.writeFloat(this.buffer, position, float, this.isBigEndian());
    };
    PositionalObjectDataOutput.prototype.pwriteInt = function (position, int) {
        BitsUtil_1.BitsUtil.writeInt32(this.buffer, position, int, this.isBigEndian());
    };
    PositionalObjectDataOutput.prototype.pwriteIntBE = function (position, int) {
        BitsUtil_1.BitsUtil.writeInt32(this.buffer, position, int, true);
    };
    PositionalObjectDataOutput.prototype.pwriteLong = function (position, long) {
        if (this.isBigEndian()) {
            BitsUtil_1.BitsUtil.writeInt32(this.buffer, position, long.high, true);
            BitsUtil_1.BitsUtil.writeInt32(this.buffer, position + BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES, long.low, true);
        }
        else {
            BitsUtil_1.BitsUtil.writeInt32(this.buffer, position, long.low, false);
            BitsUtil_1.BitsUtil.writeInt32(this.buffer, position + BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES, long.high, false);
        }
    };
    PositionalObjectDataOutput.prototype.pwriteShort = function (position, short) {
        BitsUtil_1.BitsUtil.writeInt16(this.buffer, position, short, this.isBigEndian());
    };
    return PositionalObjectDataOutput;
}(ObjectDataOutput));
exports.PositionalObjectDataOutput = PositionalObjectDataOutput;
var ObjectDataInput = /** @class */ (function () {
    function ObjectDataInput(buffer, offset, serializationService, isBigEndian, isStandardUTF) {
        this.buffer = buffer;
        this.offset = offset;
        this.service = serializationService;
        this.bigEndian = isBigEndian;
        this.standardUTF = isStandardUTF;
        this.pos = this.offset;
    }
    ObjectDataInput.prototype.isBigEndian = function () {
        return this.bigEndian;
    };
    ObjectDataInput.prototype.position = function (newPosition) {
        var oldPos = this.pos;
        if (Number.isInteger(newPosition)) {
            this.pos = newPosition;
        }
        return oldPos;
    };
    ObjectDataInput.prototype.read = function (pos) {
        this.assertAvailable(BitsUtil_1.BitsUtil.BYTE_SIZE_IN_BYTES, pos);
        if (pos === undefined) {
            return BitsUtil_1.BitsUtil.readUInt8(this.buffer, this.pos++);
        }
        else {
            return BitsUtil_1.BitsUtil.readUInt8(this.buffer, pos);
        }
    };
    ObjectDataInput.prototype.readBoolean = function (pos) {
        return this.read(pos) === 1;
    };
    ObjectDataInput.prototype.readBooleanArray = function (pos) {
        return this.readArray(this.readBoolean, pos);
    };
    ObjectDataInput.prototype.readByte = function (pos) {
        return this.read(pos);
    };
    ObjectDataInput.prototype.readByteArray = function (pos) {
        return this.readArray(this.readByte, pos);
    };
    ObjectDataInput.prototype.readChar = function (pos) {
        this.assertAvailable(BitsUtil_1.BitsUtil.CHAR_SIZE_IN_BYTES);
        var readBytes;
        if (pos === undefined) {
            readBytes = BitsUtil_1.BitsUtil.readUInt16(this.buffer, this.pos, this.isBigEndian());
            this.pos += BitsUtil_1.BitsUtil.CHAR_SIZE_IN_BYTES;
        }
        else {
            readBytes = BitsUtil_1.BitsUtil.readUInt16(this.buffer, pos, this.isBigEndian());
        }
        return String.fromCharCode(readBytes);
    };
    ObjectDataInput.prototype.readCharArray = function (pos) {
        return this.readArray(this.readChar, pos);
    };
    ObjectDataInput.prototype.readData = function () {
        var bytes = this.readByteArray();
        var data = bytes === null ? null : new HeapData_1.HeapData(safe_buffer_1.Buffer.from(bytes));
        return data;
    };
    ObjectDataInput.prototype.readDouble = function (pos) {
        this.assertAvailable(BitsUtil_1.BitsUtil.DOUBLE_SIZE_IN_BYTES, pos);
        var ret;
        if (pos === undefined) {
            ret = BitsUtil_1.BitsUtil.readDouble(this.buffer, this.pos, this.isBigEndian());
            this.pos += BitsUtil_1.BitsUtil.DOUBLE_SIZE_IN_BYTES;
        }
        else {
            ret = BitsUtil_1.BitsUtil.readDouble(this.buffer, pos, this.isBigEndian());
        }
        return ret;
    };
    ObjectDataInput.prototype.readDoubleArray = function (pos) {
        return this.readArray(this.readDouble, pos);
    };
    ObjectDataInput.prototype.readFloat = function (pos) {
        this.assertAvailable(BitsUtil_1.BitsUtil.FLOAT_SIZE_IN_BYTES, pos);
        var ret;
        if (pos === undefined) {
            ret = BitsUtil_1.BitsUtil.readFloat(this.buffer, this.pos, this.isBigEndian());
            this.pos += BitsUtil_1.BitsUtil.FLOAT_SIZE_IN_BYTES;
        }
        else {
            ret = BitsUtil_1.BitsUtil.readFloat(this.buffer, pos, this.isBigEndian());
        }
        return ret;
    };
    ObjectDataInput.prototype.readFloatArray = function (pos) {
        return this.readArray(this.readFloat, pos);
    };
    ObjectDataInput.prototype.readInt = function (pos) {
        this.assertAvailable(BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES, pos);
        var ret;
        if (pos === undefined) {
            ret = BitsUtil_1.BitsUtil.readInt32(this.buffer, this.pos, this.isBigEndian());
            this.pos += BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES;
        }
        else {
            ret = BitsUtil_1.BitsUtil.readInt32(this.buffer, pos, this.isBigEndian());
        }
        return ret;
    };
    ObjectDataInput.prototype.readIntArray = function (pos) {
        return this.readArray(this.readInt, pos);
    };
    ObjectDataInput.prototype.readLong = function (pos) {
        this.assertAvailable(BitsUtil_1.BitsUtil.LONG_SIZE_IN_BYTES, pos);
        var first;
        var second;
        if (pos === undefined) {
            first = BitsUtil_1.BitsUtil.readInt32(this.buffer, this.pos, this.isBigEndian());
            this.pos += BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES;
            second = BitsUtil_1.BitsUtil.readInt32(this.buffer, this.pos, this.isBigEndian());
            this.pos += BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES;
        }
        else {
            first = BitsUtil_1.BitsUtil.readInt32(this.buffer, pos, this.isBigEndian());
            second = BitsUtil_1.BitsUtil.readInt32(this.buffer, pos + BitsUtil_1.BitsUtil.INT_SIZE_IN_BYTES, this.isBigEndian());
        }
        if (this.isBigEndian()) {
            return new Long(second, first);
        }
        else {
            return new Long(first, second);
        }
    };
    ObjectDataInput.prototype.readLongArray = function (pos) {
        return this.readArray(this.readLong, pos);
    };
    ObjectDataInput.prototype.readObject = function () {
        return this.service.readObject(this);
    };
    ObjectDataInput.prototype.readShort = function (pos) {
        this.assertAvailable(BitsUtil_1.BitsUtil.SHORT_SIZE_IN_BYTES, pos);
        var ret;
        if (pos === undefined) {
            ret = BitsUtil_1.BitsUtil.readInt16(this.buffer, this.pos, this.isBigEndian());
            this.pos += BitsUtil_1.BitsUtil.SHORT_SIZE_IN_BYTES;
        }
        else {
            ret = BitsUtil_1.BitsUtil.readInt16(this.buffer, pos, this.isBigEndian());
        }
        return ret;
    };
    ObjectDataInput.prototype.readShortArray = function (pos) {
        return this.readArray(this.readShort, pos);
    };
    ObjectDataInput.prototype.readUnsignedByte = function (pos) {
        return this.read(pos);
    };
    ObjectDataInput.prototype.readUnsignedShort = function (pos) {
        return this.readChar(pos).charCodeAt(0);
    };
    ObjectDataInput.prototype.readUTF = function (pos) {
        if (this.standardUTF) {
            return this.readUTFStandard(pos);
        }
        else {
            return this.readUTFLegacy(pos);
        }
    };
    ObjectDataInput.prototype.readUTFArray = function (pos) {
        return this.readArray(this.readUTF, pos);
    };
    ObjectDataInput.prototype.reset = function () {
        this.pos = 0;
    };
    ObjectDataInput.prototype.skipBytes = function (count) {
        this.pos += count;
    };
    ObjectDataInput.prototype.readCopy = function (other, numBytes) {
        this.assertAvailable(numBytes, this.pos);
        this.buffer.copy(other, 0, this.pos, this.pos + numBytes);
        this.pos += numBytes;
    };
    ObjectDataInput.prototype.available = function () {
        return this.buffer.length - this.pos;
    };
    ObjectDataInput.prototype.readArray = function (func, pos) {
        var backupPos = this.pos;
        if (pos !== undefined) {
            this.pos = pos;
        }
        var len = this.readInt();
        var arr = [];
        for (var i = 0; i < len; i++) {
            arr.push(func.call(this));
        }
        if (pos !== undefined) {
            this.pos = backupPos;
        }
        return arr;
    };
    ObjectDataInput.prototype.assertAvailable = function (numOfBytes, pos) {
        if (pos === void 0) { pos = this.pos; }
        assert(pos >= 0);
        assert(pos + numOfBytes <= this.buffer.length);
    };
    ObjectDataInput.prototype.readUTFLegacy = function (pos) {
        var len = this.readInt(pos);
        var readingIndex = this.addOrUndefined(pos, 4);
        if (len === BitsUtil_1.BitsUtil.NULL_ARRAY_LENGTH) {
            return null;
        }
        var result = '';
        var leadingByte;
        for (var i = 0; i < len; i++) {
            var charCode = void 0;
            leadingByte = this.readByte(readingIndex) & MASK_1BYTE;
            readingIndex = this.addOrUndefined(readingIndex, 1);
            var b = leadingByte & 0xFF;
            switch (b >> 4) {
                /* tslint:disable:no-switch-case-fall-through */
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    charCode = leadingByte;
                    break;
                case 12:
                case 13:
                    var first = (b & 0x1F) << 6;
                    var second = this.readByte(readingIndex) & 0x3F;
                    readingIndex = this.addOrUndefined(readingIndex, 1);
                    charCode = first | second;
                    break;
                case 14:
                    var first2 = (b & 0x0F) << 12;
                    var second2 = (this.readByte(readingIndex) & 0x3F) << 6;
                    readingIndex = this.addOrUndefined(readingIndex, 1);
                    var third2 = this.readByte(readingIndex) & 0x3F;
                    readingIndex = this.addOrUndefined(readingIndex, 1);
                    charCode = (first2 | second2 | third2);
                    break;
                default:
                    throw new RangeError('Malformed UTF8 string');
            }
            result += String.fromCharCode(charCode);
        }
        return result;
    };
    ObjectDataInput.prototype.readUTFStandard = function (pos) {
        var len = this.readInt(pos);
        var readPos = this.addOrUndefined(pos, 4) || this.pos;
        if (len === BitsUtil_1.BitsUtil.NULL_ARRAY_LENGTH) {
            return null;
        }
        // max char size in UTF-8 is 4 bytes, see RFC3629
        // TODO: change to `maxByteLen = len;` in future when string serialization in client protocol changes
        var maxByteLen = len * 4;
        var available = this.available();
        var readByteLen = maxByteLen > available ? available : maxByteLen;
        var readStr = this.buffer.toString('utf8', readPos, readPos + readByteLen);
        var result = readStr.substring(0, len);
        if (pos === undefined) {
            var realByteLen = safe_buffer_1.Buffer.byteLength(result, 'utf8');
            this.pos += realByteLen;
        }
        return result;
    };
    ObjectDataInput.prototype.addOrUndefined = function (base, adder) {
        if (base === undefined) {
            return undefined;
        }
        else {
            return base + adder;
        }
    };
    return ObjectDataInput;
}());
exports.ObjectDataInput = ObjectDataInput;
//# sourceMappingURL=ObjectData.js.map