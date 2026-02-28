import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessagingService } from '../../services/messaging.service';

@Component({
  selector: 'app-ingest-form',
  templateUrl: './ingest-form.component.html',
  styleUrl: './ingest-form.component.css',
  standalone: false
})
export class IngestFormComponent {
  private readonly fb = inject(FormBuilder);
  protected readonly messaging = inject(MessagingService);

  protected readonly form: FormGroup = this.fb.group({
    contactPhone: ['', [Validators.required, Validators.pattern(/^\+?\d{7,15}$/)]],
    sender: ['', Validators.required],
    body: ['', [Validators.required, Validators.maxLength(1000)]]
  });

  protected submit(): void {
    if (this.form.invalid) return;
    this.messaging.ingestMessage(this.form.getRawValue());
  }
}
