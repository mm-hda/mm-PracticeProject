import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { describe, beforeEach, afterEach, expect, it, vi } from 'vitest';

import { AuthApiService } from './auth-api.service';
import { StorageService } from '../storage.service';
import { AuthService } from '../auth.service';

import { apiEndpoints } from '@app/core/config/api-endpoints';
import { LoginRequest } from '@app/core/models/authModels/login-request.model';
import { TokenDto } from '@app/core/models/authModels/token.model';
import { ServiceResponse } from '@app/core/models/service-response.model';

describe('AuthApiService', () => {
  let service: AuthApiService;
  let httpTestingController: HttpTestingController;

  let storageServiceMock: {
    getItem: ReturnType<typeof vi.fn>;
  };

  let authServiceMock: {
    clearCurrentUser: ReturnType<typeof vi.fn>;
  };

  const authUrl = apiEndpoints.auth;

  beforeEach(() => {
    storageServiceMock = { getItem: vi.fn() };

    authServiceMock = { clearCurrentUser: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        AuthApiService,
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: StorageService,
          useValue: storageServiceMock
        },
        {
          provide: AuthService,
          useValue: authServiceMock
        }
      ]
    });

    service = TestBed.inject(AuthApiService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => { httpTestingController.verify(); });

  describe('Creation', () => {
    it('should create the service', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('login', () => {
    it('should send POST request to login endpoint', () => {
      const payload: LoginRequest = {
        email: 'harsh@test.com',
        password: 'Password@123'
      };

      const mockToken: TokenDto = {
        userId: '1',
        name: 'Harsh Donda',
        email: 'harsh@test.com',
        role: 'Admin',
        branch: 'IT'
      };

      const mockResponse: ServiceResponse<TokenDto> = {
        isSuccess: true,
        data: mockToken,
        statusCode: 200
      };

      service.login(payload).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${authUrl}/Login`
      );

      expect(request.request.method).toBe('POST');

      expect(request.request.body).toEqual(payload);

      expect(request.request.withCredentials).toBe(true);

      request.flush(mockResponse);
    });
  });

  describe('logout', () => {
    it('should send logout request with stored user email', () => {
      storageServiceMock.getItem.mockReturnValue({
        email: 'harsh@test.com'
      });

      service.logout();

      expect(storageServiceMock.getItem).toHaveBeenCalledWith('auth_user');

      const request = httpTestingController.expectOne(
        `${authUrl}/logout`
      );

      expect(request.request.method).toBe('POST');

      expect(request.request.body).toEqual({ email: 'harsh@test.com' });

      expect(request.request.withCredentials).toBe(true);

      request.flush(null);

      expect(authServiceMock.clearCurrentUser).toHaveBeenCalledTimes(1);
    });

    it('should send undefined email when user is not available', () => {
      storageServiceMock.getItem.mockReturnValue(null);

      service.logout();

      expect(storageServiceMock.getItem).toHaveBeenCalledWith('auth_user');

      const request = httpTestingController.expectOne(
        `${authUrl}/logout`
      );

      expect(request.request.method).toBe('POST');
      expect(request.request.body).toEqual({ email: undefined });
      expect(request.request.withCredentials).toBe(true);
      request.flush(null);
      expect(authServiceMock.clearCurrentUser).toHaveBeenCalledTimes(1);
    });

    it('should clear current user when logout request fails', () => {
      storageServiceMock.getItem.mockReturnValue({
        email: 'harsh@test.com'
      });

      service.logout();

      const request = httpTestingController.expectOne(
        `${authUrl}/logout`
      );

      request.flush(
        {
          message: 'Logout failed'
        },
        {
          status: 500,
          statusText: 'Internal Server Error'
        }
      );

      expect(authServiceMock.clearCurrentUser).toHaveBeenCalledTimes(1);
    });
  });

  describe('refreshToken', () => {
    it('should send POST request to refresh-token endpoint', () => {
      const mockResponse = {
        isSuccess: true,
        data: {
          userId: '1',
          name: 'Harsh Donda',
          email: 'harsh@test.com',
          role: 'Admin',
          branch: 'IT'
        },
        statusCode: 200
      };

      service.refreshToken().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${authUrl}/refresh-token`
      );

      expect(request.request.method).toBe('POST');
      expect(request.request.body).toEqual({});
      expect(request.request.withCredentials).toBe(true);
      request.flush(mockResponse);
    });
  });
});
