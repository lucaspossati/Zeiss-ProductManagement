import { CurrencyPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  ElementRef,
  inject,
  signal,
  viewChild
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { UiOverlaysComponent } from '../../../shared/components/ui-overlays.component';
import { ApiRequestError } from '../../../shared/models/api-request-error';
import { ApiValidationError } from '../../../shared/models/api-response.model';
import { StockStatusPipe } from '../../../shared/pipes/stock-status.pipe';
import { Product, ProductPayload, StockOperation } from '../models/product.model';
import { ProductsApiService } from '../services/products-api.service';
import { ProductsStore } from '../services/products-store.service';
import { ProductFormComponent } from '../components/product-form.component';
import { StockAdjustmentDialogComponent } from '../components/stock-adjustment-dialog.component';

@Component({
  selector: 'app-products-page',
  standalone: true,
  host: { class: 'inventory-page-host' },
  imports: [
    CurrencyPipe,
    RouterLink,
    ReactiveFormsModule,
    UiOverlaysComponent,
    ProductFormComponent,
    StockAdjustmentDialogComponent,
    StockStatusPipe
  ],
  providers: [ProductsStore],
  templateUrl: './products-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductsPageComponent {
  readonly store = inject(ProductsStore);
  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly filtersForm = new FormGroup({
    minStock: new FormControl<number | null>(null, Validators.min(0)),
    maxStock: new FormControl<number | null>(null, Validators.min(0))
  });
  readonly productDialogVisible = signal(false);
  readonly editingProduct = signal<Product | null>(null);
  readonly savingProduct = signal(false);
  readonly stockDialogVisible = signal(false);
  readonly stockProduct = signal<Product | null>(null);
  readonly stockOperation = signal<StockOperation>('add');
  readonly savingStock = signal(false);
  readonly filterError = signal<string | null>(null);
  readonly productValidationErrors = signal<readonly ApiValidationError[]>([]);
  readonly stockValidationErrors = signal<readonly ApiValidationError[]>([]);
  readonly pendingDeleteProduct = signal<Product | null>(null);
  readonly deleteConfirmationVisible = signal(false);
  readonly toastMessage = signal<{
    severity: 'success' | 'error';
    summary: string;
    detail: string;
  } | null>(null);
  readonly productDialogElement = viewChild<ElementRef<HTMLDialogElement>>('productDialog');
  readonly deleteDialogElement = viewChild<ElementRef<HTMLDialogElement>>('deleteDialog');
  readonly pagination = computed(() => {
    const { page, pageSize } = this.store.query();
    const total = this.store.totalCount();

    return {
      start: total === 0 ? 0 : (page - 1) * pageSize + 1,
      end: Math.min(page * pageSize, total),
      totalPages: Math.max(1, Math.ceil(total / pageSize))
    };
  });
  readonly pageMetrics = computed(() => {
    const products = this.store.products();

    return {
      units: products.reduce((total, product) => total + product.availableStock, 0),
      lowStock: products.filter(product => product.availableStock < 5).length,
      stockValue: products.reduce((total, product) => total + product.price * product.availableStock, 0)
    };
  });

  private readonly api = inject(ProductsApiService);
  private readonly destroyRef = inject(DestroyRef);
  private toastTimeout: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    effect(() => {
      const dialog = this.productDialogElement()?.nativeElement;
      if (!dialog) {
        return;
      }

      if (this.productDialogVisible() && !dialog.open) {
        dialog.showModal();
      } else if (!this.productDialogVisible() && dialog.open) {
        dialog.close();
      }
    });

    effect(() => {
      const dialog = this.deleteDialogElement()?.nativeElement;
      if (!dialog) {
        return;
      }

      if (this.deleteConfirmationVisible() && !dialog.open) {
        dialog.showModal();
      } else if (!this.deleteConfirmationVisible() && dialog.open) {
        dialog.close();
      }
    });

    this.searchControl.valueChanges.pipe(
      debounceTime(250),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((search) => this.store.setSearch(search));

    this.filtersForm.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged((previous, current) =>
        previous.minStock === current.minStock && previous.maxStock === current.maxStock
      ),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => this.applyStockFilters());
  }

  private applyStockFilters(): void {
    if (this.filtersForm.invalid) {
      this.filterError.set('Stock values must be zero or greater.');
      return;
    }

    const { minStock, maxStock } = this.filtersForm.getRawValue();
    if (minStock !== null && maxStock !== null && minStock > maxStock) {
      this.filterError.set('Minimum stock cannot be greater than maximum stock.');
      return;
    }

    this.filterError.set(null);
    this.store.setStockRange(minStock, maxStock);
  }

  clearFilters(): void {
    this.filtersForm.reset({ minStock: null, maxStock: null }, { emitEvent: false });
    this.filterError.set(null);
    this.store.setStockRange(null, null);
  }

  onPageChange(first: number, rows: number): void {
    this.store.setPage(Math.floor(first / rows) + 1, rows);
  }

  setPageSize(event: Event): void {
    if (!(event.target instanceof HTMLSelectElement)) {
      return;
    }

    const pageSize = Number(event.target.value);
    if ([10, 25, 50].includes(pageSize)) {
      this.store.setPage(1, pageSize);
    }
  }

  previousPage(): void {
    const { page, pageSize } = this.store.query();
    if (page > 1) {
      this.store.setPage(page - 1, pageSize);
    }
  }

  nextPage(): void {
    const { page, pageSize } = this.store.query();
    if (page < this.pagination().totalPages) {
      this.store.setPage(page + 1, pageSize);
    }
  }

  openCreateDialog(): void {
    this.editingProduct.set(null);
    this.productValidationErrors.set([]);
    this.productDialogVisible.set(true);
  }

  openEditDialog(product: Product): void {
    this.editingProduct.set(product);
    this.productValidationErrors.set([]);
    this.productDialogVisible.set(true);
  }

  closeProductDialog(): void {
    this.productDialogVisible.set(false);
    this.editingProduct.set(null);
    this.productValidationErrors.set([]);
  }

  onProductDialogCancel(event: Event): void {
    event.preventDefault();
    this.closeProductDialog();
  }

  onProductDialogClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.closeProductDialog();
    }
  }

  onProductDialogClosed(): void {
    if (this.productDialogVisible()) {
      this.closeProductDialog();
    }
  }

  saveProduct(payload: ProductPayload): void {
    this.productValidationErrors.set([]);
    const product = this.editingProduct();
    const request = product
      ? this.api.updateProduct(product.productIdentifier, payload)
      : this.api.createProduct(payload);

    this.savingProduct.set(true);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (response) => {
        this.savingProduct.set(false);
        this.closeProductDialog();
        this.showToast('success', product ? 'Product updated' : 'Product created', response.message);
        this.store.refresh();
      },
      error: (error: unknown) => {
        this.savingProduct.set(false);
        if (error instanceof ApiRequestError) {
          this.productValidationErrors.set(error.details);
        }
        this.showError('Unable to save product', error);
      }
    });
  }

  confirmDelete(product: Product): void {
    this.pendingDeleteProduct.set(product);
    this.deleteConfirmationVisible.set(true);
  }

  cancelDelete(): void {
    this.deleteConfirmationVisible.set(false);
    this.pendingDeleteProduct.set(null);
  }

  deletePendingProduct(): void {
    const product = this.pendingDeleteProduct();
    this.cancelDelete();
    if (product) {
      this.deleteProduct(product);
    }
  }

  onDeleteDialogCancel(event: Event): void {
    event.preventDefault();
    this.cancelDelete();
  }

  onDeleteDialogClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.cancelDelete();
    }
  }

  onDeleteDialogClosed(): void {
    if (this.deleteConfirmationVisible()) {
      this.cancelDelete();
    }
  }

  openStockDialog(product: Product, operation: StockOperation): void {
    this.stockProduct.set(product);
    this.stockOperation.set(operation);
    this.stockValidationErrors.set([]);
    this.stockDialogVisible.set(true);
  }

  adjustStock(quantity: number): void {
    this.stockValidationErrors.set([]);
    const product = this.stockProduct();
    if (!product) {
      return;
    }

    const operation = this.stockOperation();
    const request = operation === 'add'
      ? this.api.addStock(product.productIdentifier, quantity)
      : this.api.decrementStock(product.productIdentifier, quantity);

    this.savingStock.set(true);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (response) => {
        this.savingStock.set(false);
        this.stockDialogVisible.set(false);
        this.showToast('success', 'Stock updated', response.message);
        this.store.refresh();
      },
      error: (error: unknown) => {
        this.savingStock.set(false);
        if (error instanceof ApiRequestError) {
          this.stockValidationErrors.set(error.details);
        }
        this.showError('Unable to update stock', error);
      }
    });
  }

  private deleteProduct(product: Product): void {
    this.api.deleteProduct(product.productIdentifier)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.showToast('success', 'Product deleted', `${product.name} was removed.`);
          this.store.refresh();
        },
        error: (error: unknown) => this.showError('Unable to delete product', error)
      });
  }

  private showToast(severity: 'success' | 'error', summary: string, detail: string): void {
    if (this.toastTimeout !== null) {
      clearTimeout(this.toastTimeout);
    }

    this.toastMessage.set({ severity, summary, detail });
    this.toastTimeout = setTimeout(() => {
      this.toastMessage.set(null);
      this.toastTimeout = null;
    }, 5000);
  }

  private showError(summary: string, error: unknown): void {
    const detail = error instanceof ApiRequestError
      ? error.details.length > 0
        ? error.details.map(validationError => validationError.message).join(' ')
        : error.message
      : error instanceof Error
        ? error.message
        : 'An unexpected error occurred.';

    this.showToast('error', summary, detail);
  }
}
