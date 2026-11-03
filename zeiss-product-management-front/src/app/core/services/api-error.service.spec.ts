import { HttpErrorResponse } from '@angular/common/http';
import { describe, expect, it } from 'vitest';
import { ApiErrorService } from './api-error.service';

describe('ApiErrorService', () => {
  const service = new ApiErrorService();

  it('preserves structured entity and property validation errors', () => {
    const error = service.normalize(new HttpErrorResponse({
      status: 400,
      statusText: 'Bad Request',
      error: {
        message: 'One or more validation errors occurred.',
        errors: [{
          entity: 'Product',
          property: 'Name',
          code: 'product.name.required',
          message: 'Product name is required.'
        }]
      }
    }));

    expect(error.details).toEqual([{
      entity: 'Product',
      property: 'Name',
      code: 'product.name.required',
      message: 'Product name is required.'
    }]);
  });

  it('retains string errors from older API responses as general errors', () => {
    const error = service.normalize(new HttpErrorResponse({
      status: 400,
      statusText: 'Bad Request',
      error: { message: 'Request failed.', errors: ['Invalid request.'] }
    }));

    expect(error.details).toEqual([{
      entity: 'Request',
      property: null,
      code: 'request.error',
      message: 'Invalid request.'
    }]);
  });
});
