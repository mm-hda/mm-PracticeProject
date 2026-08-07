import {
  HttpErrorResponse,
  HttpInterceptorFn,
  HttpResponse
} from '@angular/common/http';
import { tap } from 'rxjs';

import { ServiceResponse } from '@app/core/models/service-response.model';
import { TokenDto } from '@app/core/models/authModels/token.model';
import { StorageService } from '../services/storage.service';
import { inject } from '@angular/core';

export const authInterceptor: HttpInterceptorFn = (request, next) => {

  const storageService = inject(StorageService);

  const authRequest = request.clone({ withCredentials: true });

  return next(authRequest).pipe(
    tap({
      next: (event) => {

        if (!(event instanceof HttpResponse)) {
          return;
        }

        const body = event.body as ServiceResponse<TokenDto> | null;

        if (body?.statusCode === 713 && body.data) {
          const user = {
            name: body.data.name ?? '',
            email: body.data.email ?? '',
            role: body.data.role ?? '',
            userId: body.data.userId
          }

          storageService.setItem('auth_user', user);
        }
      },

      error: (_error: HttpErrorResponse) => {
        return;
      }
    })
  );
};
