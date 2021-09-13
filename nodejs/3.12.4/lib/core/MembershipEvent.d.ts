import { Member } from './Member';
/**
 * Membership event fired when a new member is added to the cluster and/or when a member leaves the cluster
 * or when there is a member attribute change.
 */
export declare class MembershipEvent {
    /**
     * the removed or added member.
     */
    member: Member;
    /**
     * the membership event type.
     */
    eventType: number;
    /**
     * the members at the moment after this event.
     */
    members: Member[];
    constructor(member: Member, eventType: number, members: Member[]);
}
