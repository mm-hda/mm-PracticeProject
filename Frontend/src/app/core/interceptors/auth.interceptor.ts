import {
  HttpErrorResponse,
  HttpInterceptorFn,
  HttpResponse
} from '@angular/common/http';

import { inject } from '@angular/core';
import { Router } from '@angular/router';

import {
  catchError,
  switchMap,
  tap,
  throwError
} from 'rxjs';

import { ServiceResponse } from '@app/core/models/service-response.model';
import { TokenDto } from '@app/core/models/authModels/token.model';
import { StorageService } from '../services/storage.service';
import { AuthApiService } from '../services/api-service/auth-api.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const storageService = inject(StorageService);
  const authService = inject(AuthApiService);
  const router = inject(Router);

  const authRequest = request.clone({
    withCredentials: true
  });

  return next(authRequest).pipe(
    tap({
      next: (event) => {
        if (!(event instanceof HttpResponse)) { return; }

        const body = event.body as ServiceResponse<TokenDto> | null;

        if (body?.statusCode === 713 && body.data) {
          storageService.setItem('auth_user', {
            name: body.data.name ?? '',
            email: body.data.email ?? '',
            role: body.data.role ?? '',
            userId: body.data.userId
          });
        }
      }
    }),

    catchError((error: HttpErrorResponse) => {
      if (
        request.url.includes('refresh-token') ||
        request.url.includes('login')
      ) {
        return throwError(() => error);
      }

      if (error.status !== 401) {
        return throwError(() => error);
      }

      return authService.refreshToken().pipe(
        switchMap(() => {
          const retryRequest = request.clone({ withCredentials: true });
          return next(retryRequest);
        }),

        catchError((refreshError) => {
          storageService.removeItem('auth_user');
          router.navigate(['/login']);
          return throwError(() => refreshError);
        })
      );
    })
  );
};
