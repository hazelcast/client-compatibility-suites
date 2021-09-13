import { ListenerMessageCodec } from '../ListenerMessageCodec';
import { ClientConnection } from './ClientConnection';
export declare class ClientEventRegistration {
    readonly serverRegistrationId: string;
    readonly correlationId: number;
    readonly subscriber: ClientConnection;
    readonly codec: ListenerMessageCodec;
    constructor(serverRegistrationId: string, correlationId: number, subscriber: ClientConnection, codec: ListenerMessageCodec);
    toString(): string;
}
