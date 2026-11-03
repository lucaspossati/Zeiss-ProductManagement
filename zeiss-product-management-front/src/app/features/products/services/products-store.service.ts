import { DestroyRef, Injectable, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, map, of, startWith, Subject, switchMap, tap } from 'rxjs';
import { ApiRequestError } from '../../../shared/models/api-request-error';
import { ProductsApiService } from './products-api.service';
import { Product } from '../models/product.model';

export interface ProductQuery {
  page: number;
  pageSize: number;
  search: string;
  minStock: number | null;
  maxStock: number | null;
}

type ProductLoadResult =
  | { kind: 'success'; products: Product[]; totalCount: number }
  | { kind: 'failure'; error: unknown };

@Injectable()
export class ProductsStore {
  private readonly api = inject(ProductsApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly refreshRequests = new Subject<void>();

  readonly products = signal<Product[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly query = signal<ProductQuery>({
    page: 1,
    pageSize: 10,
    search: '',
    minStock: null,
    maxStock: null,
  });

  constructor() {
    this.refreshRequests
      .pipe(
        startWith(undefined),
        map(() => this.query()),
        tap(() => {
          this.loading.set(true);
          this.errorMessage.set(null);
        }),
        switchMap((query) =>
          this.api
            .getProducts(
              query.page,
              query.pageSize,
              query.search,
              query.minStock,
              query.maxStock,
            )
            .pipe(
              map(
                (response): ProductLoadResult => ({
                  kind: 'success',
                  products: response.data.items,
                  totalCount: response.data.totalCount,
                }),
              ),
              catchError((error: unknown) =>
                of<ProductLoadResult>({ kind: 'failure', error }),
              ),
            ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((result) => {
        this.loading.set(false);

        if (result.kind === 'failure') {
          this.errorMessage.set(this.getErrorMessage(result.error));
          return;
        }

        this.products.set(result.products);
        this.totalCount.set(result.totalCount);
      });
  }

  setSearch(search: string): void {
    this.updateQuery({ search, page: 1 });
  }

  setStockRange(minStock: number | null, maxStock: number | null): void {
    this.updateQuery({ minStock, maxStock, page: 1 });
  }

  setPage(page: number, pageSize: number): void {
    this.updateQuery({ page, pageSize });
  }

  refresh(): void {
    this.refreshRequests.next();
  }

  private updateQuery(update: Partial<ProductQuery>): void {
    this.query.update((query) => ({ ...query, ...update }));
    this.refreshRequests.next();
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof ApiRequestError) {
      return error.message;
    }

    return error instanceof Error
      ? error.message
      : 'An unexpected error occurred while loading products.';
  }
}
