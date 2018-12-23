export interface Message {
    id: string;
    senderId: number;
    senderKnownAs: string;
    senderPhotoUrl: string;
    recipientId: number;
    recipientKnownAs: string;
    recipientPhotoUrl: string;
    content: string;
    messageSent: Date;
    isRead: boolean;
    dateRead?: Date;
}
