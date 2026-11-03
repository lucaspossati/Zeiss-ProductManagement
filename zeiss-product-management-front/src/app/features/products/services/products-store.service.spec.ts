import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { API_BASE_URL } from '../../../core/configuration/api-base-url.token';
import { apiErrorInterceptor } from '../../../core/interceptors/api-error.interceptor';
import { Product } from '../models/product.model';
import { ProductsStore } from './products-store.service';

const product: Product = {
  productIdentifier: 123456,
  name: 'ZEISS Lens',
  description: null,
  price: 99.95,
  availableStock: 12,
  createdAtUtc: '2026-01-01T00:00:00Z',
  updatedAtUtc: '2026-01-01T00:00:00Z'
};

describe('ProductsStore', () => {
  let store: ProductsStore;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiErrorInterceptor])),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: 'https://localhost:7068/api' },
        ProductsStore
      ]
    });

    store = TestBed.inject(ProductsStore);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('loads the initial page and stores its products', () => {
    const request = http.expectOne((req) => req.url === 'https://localhost:7068/api/products');
    expect(request.request.params.get('page')).toBe('1');
    expect(request.request.params.get('pageSize')).toBe('10');

    request.flush({
      success: true,
      message: 'Products retrieved',
      data: {
        items: [product],
        currentPage: 1,
        pageSize: 10,
        totalCount: 1,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false
      },
      errors: []
    });

    expect(store.products()).toEqual([product]);
    expect(store.totalCount()).toBe(1);
    expect(store.loading()).toBe(false);
  });

  it('shows API errors instead of hiding failed requests', () => {
    const request = http.expectOne((req) => req.url === 'https://localhost:7068/api/products');
    request.flush(
      { success: false, message: 'Database unavailable', data: null, errors: [] },
      { status: 503, statusText: 'Service Unavailable' }
    );

    expect(store.errorMessage()).toBe('Database unavailable');
    expect(store.loading()).toBe(false);
  });

  it('reloads products when the stock range changes', () => {
    const initialRequest = http.expectOne((req) => req.url === 'https://localhost:7068/api/products');
    initialRequest.flush({
      success: true,
      message: 'Products retrieved',
      data: {
        items: [],
        currentPage: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 0,
        hasPreviousPage: false,
        hasNextPage: false
      },
      errors: []
    });

    store.setStockRange(5, 20);

    const filteredRequest = http.expectOne((req) => req.url === 'https://localhost:7068/api/products');
    expect(filteredRequest.request.params.get('page')).toBe('1');
    expect(filteredRequest.request.params.get('minStock')).toBe('5');
    expect(filteredRequest.request.params.get('maxStock')).toBe('20');
    filteredRequest.flush({
      success: true,
      message: 'Products retrieved',
      data: {
        items: [product],
        currentPage: 1,
        pageSize: 10,
        totalCount: 1,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false
      },
      errors: []
    });

    expect(store.products()).toEqual([product]);
  });
});
