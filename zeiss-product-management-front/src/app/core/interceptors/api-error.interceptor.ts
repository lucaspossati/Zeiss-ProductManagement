import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ApiErrorService } from '../services/api-error.service';

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) => {
  const apiErrorService = inject(ApiErrorService);

  return next(request).pipe(
    catchError((error: unknown) =>
      throwError(() => error instanceof HttpErrorResponse ? apiErrorService.normalize(error) : error)
    )
  );
};
