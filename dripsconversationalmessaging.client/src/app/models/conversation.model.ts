import { Message } from './message.model';

export interface Conversation {
  id: string;
  contactPhone: string;
  isHighPriority: boolean;
  isOptedOut: boolean;
  createdAt: string;
  messages: Message[];
}
