/// <reference types="node" />
/// <reference types="bluebird" />
import { Buffer } from 'safe-buffer';
import * as Promise from 'bluebird';
import * as net from 'net';
import { EventEmitter } from 'events';
import HazelcastClient from '../HazelcastClient';
import Address = require('../Address');
export declare class PipelinedWriter extends EventEmitter {
    private readonly socket;
    private queue;
    private error;
    private scheduled;
    private readonly threshold;
    constructor(socket: net.Socket, threshold: number);
    write(buffer: Buffer, resolver: Promise.Resolver<void>): void;
    private schedule();
    private process();
    private handleError(err, sentResolvers);
}
export declare class DirectWriter extends EventEmitter {
    private readonly socket;
    constructor(socket: net.Socket);
    write(buffer: Buffer, resolver: Promise.Resolver<void>): void;
}
export declare class FrameReader {
    private chunks;
    private chunksTotalSize;
    private frameSize;
    append(buffer: Buffer): void;
    read(): Buffer;
    private readFrameSize();
}
export declare class ClientConnection {
    private address;
    private readonly localAddress;
    private lastReadTimeMillis;
    private lastWriteTimeMillis;
    private heartbeating;
    private readonly client;
    private readonly startTime;
    private closedTime;
    private connectedServerVersionString;
    private connectedServerVersion;
    private authenticatedAsOwner;
    private readonly socket;
    private readonly writer;
    private readonly reader;
    constructor(client: HazelcastClient, address: Address, socket: net.Socket);
    /**
     * Returns the address of local port that is associated with this connection.
     * @returns
     */
    getLocalAddress(): Address;
    /**
     * Returns the address of remote node that is associated with this connection.
     * @returns
     */
    getAddress(): Address;
    setAddress(address: Address): void;
    write(buffer: Buffer): Promise<void>;
    setConnectedServerVersion(versionString: string): void;
    getConnectedServerVersion(): number;
    /**
     * Closes this connection.
     */
    close(): void;
    isAlive(): boolean;
    isHeartbeating(): boolean;
    setHeartbeating(heartbeating: boolean): void;
    isAuthenticatedAsOwner(): boolean;
    setAuthenticatedAsOwner(asOwner: boolean): void;
    getStartTime(): number;
    getLastReadTimeMillis(): number;
    getLastWriteTimeMillis(): number;
    toString(): string;
    /**
     * Registers a function to pass received data on 'data' events on this connection.
     * @param callback
     */
    registerResponseCallback(callback: Function): void;
}
