import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { MessagingService } from '../../services/messaging.service';

const POLL_INTERVAL_MS = 10_000;

@Component({
  selector: 'app-priority-inbox',
  templateUrl: './priority-inbox.component.html',
  styleUrl: './priority-inbox.component.css',
  standalone: false
})
export class PriorityInboxComponent implements OnInit, OnDestroy {
  protected readonly messaging = inject(MessagingService);
  private pollTimer: ReturnType<typeof setInterval> | null = null;

  ngOnInit(): void {
    this.messaging.loadPriorityConversations();
    this.pollTimer = setInterval(
      () => this.messaging.loadPriorityConversations(),
      POLL_INTERVAL_MS
    );
  }

  ngOnDestroy(): void {
    if (this.pollTimer !== null) clearInterval(this.pollTimer);
  }

  protected refresh(): void {
    this.messaging.loadPriorityConversations();
  }
}
