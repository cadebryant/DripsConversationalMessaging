import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { catchError, finalize, EMPTY } from 'rxjs';
import { Conversation } from '../models/conversation.model';
import { IngestMessageRequest } from '../models/ingest-message-request.model';
import { Message } from '../models/message.model';

@Injectable({ providedIn: 'root' })
export class MessagingService {
  private readonly _conversations = signal<Conversation[]>([]);
  private readonly _loadingPriority = signal(false);
  private readonly _submitting = signal(false);
  private readonly _priorityError = signal<string | null>(null);
  private readonly _ingestError = signal<string | null>(null);
  private readonly _lastMessage = signal<Message | null>(null);

  readonly conversations = this._conversations.asReadonly();
  readonly loadingPriority = this._loadingPriority.asReadonly();
  readonly submitting = this._submitting.asReadonly();
  readonly priorityError = this._priorityError.asReadonly();
  readonly ingestError = this._ingestError.asReadonly();
  readonly lastMessage = this._lastMessage.asReadonly();

  constructor(private readonly http: HttpClient) {}

  loadPriorityConversations(): void {
    this._loadingPriority.set(true);
    this._priorityError.set(null);
    this.http.get<Conversation[]>('/api/conversations/priority').pipe(
      catchError(err => {
        this._priorityError.set(err.message ?? 'Failed to load conversations.');
        return EMPTY;
      }),
      finalize(() => this._loadingPriority.set(false))
    ).subscribe(conversations => this._conversations.set(conversations));
  }

  ingestMessage(request: IngestMessageRequest): void {
    this._submitting.set(true);
    this._ingestError.set(null);
    this._lastMessage.set(null);
    this.http.post<Message>('/api/messages/ingest', request).pipe(
      catchError(err => {
        this._ingestError.set(err.error?.detail ?? err.message ?? 'Failed to send message.');
        return EMPTY;
      }),
      finalize(() => this._submitting.set(false))
    ).subscribe(message => this._lastMessage.set(message));
  }
}
