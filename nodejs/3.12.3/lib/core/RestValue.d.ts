import { IdentifiedDataSerializable, IdentifiedDataSerializableFactory } from '../serialization/Serializable';
import { DataInput, DataOutput } from '../serialization/Data';
export declare const REST_VALUE_FACTORY_ID = -37;
export declare const REST_VALUE_CLASS_ID = 1;
export declare class RestValue implements IdentifiedDataSerializable {
    value: string;
    contentType: string;
    getClassId(): number;
    getFactoryId(): number;
    readData(input: DataInput): any;
    writeData(output: DataOutput): void;
}
export declare class RestValueFactory implements IdentifiedDataSerializableFactory {
    create(type: number): IdentifiedDataSerializable;
}
