import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiRequestError } from '../../shared/models/api-request-error';
import { ApiValidationError } from '../../shared/models/api-response.model';

interface ApiErrorBody {
  entity?: unknown;
  property?: unknown;
  code?: unknown;
  message?: unknown;
  errors?: unknown;
}

function isApiErrorBody(value: unknown): value is ApiErrorBody {
  return typeof value === 'object' && value !== null;
}

function toValidationError(value: unknown): ApiValidationError | null {
  if (typeof value === 'string') {
    return { entity: 'Request', property: null, code: 'request.error', message: value };
  }

  if (!isApiErrorBody(value) || typeof value.message !== 'string') {
    return null;
  }

  return {
    entity: typeof value.entity === 'string' ? value.entity : 'Request',
    property: typeof value.property === 'string' ? value.property : null,
    code: typeof value.code === 'string' ? value.code : 'request.error',
    message: value.message
  };
}

@Injectable({ providedIn: 'root' })
export class ApiErrorService {
  normalize(error: HttpErrorResponse): ApiRequestError {
    const body = error.error;
    const details = isApiErrorBody(body) && Array.isArray(body.errors)
      ? body.errors
        .map(toValidationError)
        .filter((detail): detail is ApiValidationError => detail !== null)
      : [];
    const message = isApiErrorBody(body) && typeof body.message === 'string'
      ? body.message
      : error.status === 0
        ? 'The API could not be reached. Check the backend URL and network connection.'
        : `The request failed with status ${error.status}.`;

    return new ApiRequestError(message, error.status, details);
  }
}
