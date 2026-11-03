export interface Product {
  productIdentifier: number;
  name: string;
  description: string | null;
  price: number;
  availableStock: number;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface ProductPayload {
  name: string;
  description: string | null;
  price: number;
  availableStock: number;
}

export type StockOperation = 'add' | 'decrement';
