/**
 * Using this policy, one can control the
 * serialization type of strings.
 */
export declare enum StringSerializationPolicy {
    /**
     * Strings are serialized and deserialized
     * according to UTF-8 standard (RFC 3629).
     *
     * May lead to server-side compatibility
     * issues with IMDG 3.x for 4 byte characters,
     * like less common CJK characters and emoji.
     */
    STANDARD = 0,
    /**
     * 4 byte characters are represented as
     * 6 bytes during serialization/deserialization
     * (non-standard UTF-8). Provides full compatibility
     * with IMDG 3.x members and other clients.
     */
    LEGACY = 1,
}
