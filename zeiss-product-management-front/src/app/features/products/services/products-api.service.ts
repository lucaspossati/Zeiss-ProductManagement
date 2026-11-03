import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/configuration/api-base-url.token';
import { ApiResponse, PaginatedResponse } from '../../../shared/models/api-response.model';
import { Product, ProductPayload } from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductsApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  getProducts(
    page: number,
    pageSize: number,
    search: string,
    minStock: number | null,
    maxStock: number | null
  ): Observable<ApiResponse<PaginatedResponse<Product>>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    if (minStock !== null) {
      params = params.set('minStock', minStock);
    }

    if (maxStock !== null) {
      params = params.set('maxStock', maxStock);
    }

    return this.http.get<ApiResponse<PaginatedResponse<Product>>>(
      `${this.apiBaseUrl}/products`,
      { params }
    );
  }

  createProduct(payload: ProductPayload): Observable<ApiResponse<Product>> {
    return this.http.post<ApiResponse<Product>>(`${this.apiBaseUrl}/products`, payload);
  }

  updateProduct(identifier: number, payload: ProductPayload): Observable<ApiResponse<Product>> {
    return this.http.put<ApiResponse<Product>>(`${this.apiBaseUrl}/products/${identifier}`, payload);
  }

  deleteProduct(identifier: number): Observable<void> {
    return this.http.delete<void>(`${this.apiBaseUrl}/products/${identifier}`);
  }

  addStock(identifier: number, quantity: number): Observable<ApiResponse<Product>> {
    return this.http.post<ApiResponse<Product>>(
      `${this.apiBaseUrl}/products/${identifier}/add-to-stock/${quantity}`,
      {}
    );
  }

  decrementStock(identifier: number, quantity: number): Observable<ApiResponse<Product>> {
    return this.http.post<ApiResponse<Product>>(
      `${this.apiBaseUrl}/products/${identifier}/decrement-stock/${quantity}`,
      {}
    );
  }
}
