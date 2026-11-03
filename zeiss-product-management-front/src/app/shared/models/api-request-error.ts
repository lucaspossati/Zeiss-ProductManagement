import { ApiValidationError } from './api-response.model';

export class ApiRequestError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly details: readonly ApiValidationError[] = []
  ) {
    super(message);
    this.name = 'ApiRequestError';
  }
}
