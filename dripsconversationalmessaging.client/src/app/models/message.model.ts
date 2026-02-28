import { Intent } from './intent.model';

export interface Message {
  id: string;
  conversationId: string;
  body: string;
  sender: string;
  intent: Intent;
  receivedAt: string;
}
