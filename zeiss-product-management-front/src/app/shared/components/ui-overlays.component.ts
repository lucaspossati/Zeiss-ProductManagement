import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-ui-overlays',
  standalone: true,
  imports: [],
  template: `
    @if (message(); as toast) {
      <div class="toast-container">
        <div
          class="toast"
          [class.toast-error]="toast.severity === 'error'"
          [attr.role]="toast.severity === 'error' ? 'alert' : 'status'"
          aria-live="polite"
        >
          <span class="toast-icon">
            <i [class]="toast.severity === 'error' ? 'pi pi-exclamation-circle' : 'pi pi-check-circle'" aria-hidden="true"></i>
          </span>
          <span class="toast-copy">
            <strong>{{ toast.summary }}</strong>
            <span>{{ toast.detail }}</span>
          </span>
          <button class="toast-dismiss" type="button" aria-label="Dismiss notification" (click)="dismiss.emit()">
            <i class="pi pi-times" aria-hidden="true"></i>
          </button>
        </div>
      </div>
    }
  `,
  styleUrl: './ui-overlays.component.sass',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UiOverlaysComponent {
  readonly message = input<{
    severity: 'success' | 'error';
    summary: string;
    detail: string;
  } | null>(null);
  readonly dismiss = output<void>();
}
