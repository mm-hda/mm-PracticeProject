import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { apiEndpoints } from '@app/core/config/api-endpoints';
import { ServiceResponse } from '@app/core/models/service-response.model';
import { LoginRequest } from '@app/core/models/authModels/login-request.model';
import { TokenDto } from '@app/core/models/authModels/token.model';
import { StorageService } from './storage.service';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly authUrl = apiEndpoints.auth;
  private readonly storageService = inject(StorageService);
  private readonly authService = inject(AuthService);

  public constructor() { }

  login(payload: LoginRequest): Observable<ServiceResponse<TokenDto>> {
    return this.http.post<ServiceResponse<TokenDto>>(`${this.authUrl}/Login`, payload, {
      withCredentials: true
    });
  }

  logout(): void {
    var email = this.storageService.getItem<{ email: string }>('auth_user')?.email;
    localStorage.clear();
    this.http.post(`${this.authUrl}/logout`, { email }, { withCredentials: true }).subscribe(
      {
        complete: () => {
          this.authService.clearCurrentUser();
        },
        error: (error) => {
          this.authService.clearCurrentUser();
        }
      }
    );
  }
}
