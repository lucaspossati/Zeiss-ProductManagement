import { ChangeDetectionStrategy, Component, DestroyRef, effect, input, output, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiValidationError } from '../../../shared/models/api-response.model';
import { Product, ProductPayload } from '../models/product.model';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.sass',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductFormComponent {
  readonly product = input<Product | null>(null);
  readonly saving = input(false);
  readonly validationErrors = input<readonly ApiValidationError[]>([]);
  readonly saved = output<ProductPayload>();
  readonly cancelled = output<void>();

  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly dismissedServerProperties = signal<ReadonlySet<string>>(new Set());
  readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    description: ['', Validators.maxLength(1000)],
    price: [0, [Validators.required, Validators.min(0.01)]],
    availableStock: [
      0,
      [Validators.required, Validators.min(0), Validators.max(2147483647), Validators.pattern(/^\d+$/)]
    ]
  });

  constructor() {
    effect(() => {
      const product = this.product();
      this.form.reset({
        name: product?.name ?? '',
        description: product?.description ?? '',
        price: product?.price ?? 0,
        availableStock: product?.availableStock ?? 0
      });
    });

    effect(() => {
      this.validationErrors();
      this.dismissedServerProperties.set(new Set());
    });

    this.form.controls.name.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.dismissFieldErrors('name'));
    this.form.controls.description.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.dismissFieldErrors('description'));
    this.form.controls.price.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.dismissFieldErrors('price'));
    this.form.controls.availableStock.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.dismissFieldErrors('availableStock'));
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const description = value.description.trim();
    this.saved.emit({
      name: value.name.trim(),
      description: description.length > 0 ? description : null,
      price: value.price,
      availableStock: value.availableStock
    });
  }

  cancel(): void {
    this.cancelled.emit();
  }

  fieldErrors(property: string): readonly ApiValidationError[] {
    const normalizedProperty = this.normalizeProperty(property);
    if (this.dismissedServerProperties().has(normalizedProperty)) {
      return [];
    }

    return this.validationErrors().filter(
      error => error.property !== null && this.normalizeProperty(error.property) === normalizedProperty
    );
  }

  generalErrors(): readonly ApiValidationError[] {
    const supportedProperties = new Set(['name', 'description', 'price', 'availablestock']);
    return this.validationErrors().filter(
      error => error.property === null || !supportedProperties.has(this.normalizeProperty(error.property))
    );
  }

  private normalizeProperty(property: string): string {
    return property.replace(/[^a-z]/gi, '').toLowerCase();
  }

  private dismissFieldErrors(property: string): void {
    const normalizedProperty = this.normalizeProperty(property);
    this.dismissedServerProperties.update(dismissed => new Set([...dismissed, normalizedProperty]));
  }
}
