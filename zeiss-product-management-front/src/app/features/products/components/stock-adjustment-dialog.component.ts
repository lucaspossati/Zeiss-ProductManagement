import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  effect,
  ElementRef,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ApiValidationError } from '../../../shared/models/api-response.model';
import { Product, StockOperation } from '../models/product.model';

@Component({
  selector: 'app-stock-adjustment-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule],
  templateUrl: './stock-adjustment-dialog.component.html',
  styleUrl: './stock-adjustment-dialog.component.sass',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StockAdjustmentDialogComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly quantityErrorsDismissed = signal(false);
  private readonly dialogElement =
    viewChild<ElementRef<HTMLDialogElement>>('stockDialog');

  readonly visible = input(false);
  readonly visibleChange = output<boolean>();
  readonly product = input<Product | null>(null);
  readonly operation = input<StockOperation>('add');
  readonly saving = input(false);
  readonly validationErrors = input<readonly ApiValidationError[]>([]);
  readonly submitted = output<number>();
  readonly quantity = new FormControl(1, {
    nonNullable: true,
    validators: [
      Validators.required,
      Validators.min(1),
      Validators.max(2147483647),
      Validators.pattern(/^\d+$/),
    ],
  });

  constructor() {
    effect(() => {
      if (!this.visible()) {
        return;
      }

      this.product();
      this.operation();
      this.quantity.reset(1);
    });

    effect(() => {
      const dialog = this.dialogElement()?.nativeElement;
      if (!dialog) {
        return;
      }

      if (this.visible() && !dialog.open) {
        dialog.showModal();
      } else if (!this.visible() && dialog.open) {
        dialog.close();
      }
    });

    effect(() => {
      this.validationErrors();
      this.quantityErrorsDismissed.set(false);
    });

    this.quantity.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.quantityErrorsDismissed.set(true));
  }

  submit(): void {
    if (this.quantity.invalid) {
      this.quantity.markAsTouched();
      return;
    }

    this.submitted.emit(this.quantity.value);
  }

  cancelDialog(event?: Event): void {
    event?.preventDefault();
    this.visibleChange.emit(false);
  }

  onDialogClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.visibleChange.emit(false);
    }
  }

  onDialogClosed(): void {
    if (this.visible()) {
      this.visibleChange.emit(false);
    }
  }

  quantityErrors(): readonly ApiValidationError[] {
    if (this.quantityErrorsDismissed()) {
      return [];
    }

    return this.validationErrors().filter(
      (error) =>
        error.property?.replace(/[^a-z]/gi, '').toLowerCase() === 'quantity',
    );
  }

  generalErrors(): readonly ApiValidationError[] {
    return this.validationErrors().filter(
      (error) =>
        error.property?.replace(/[^a-z]/gi, '').toLowerCase() !== 'quantity',
    );
  }
}
