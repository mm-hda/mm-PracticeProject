import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { describe, beforeEach, afterEach, expect, it } from 'vitest';

import { RoleApiService } from './role-api.service';

import { apiEndpoints } from '@app/core/config/api-endpoints';
import { ServiceResponse } from '@app/core/models/service-response.model';
import { roleResponse } from '@app/core/models/roleModels/role.model';

describe('RoleApiService', () => {
  let service: RoleApiService;
  let httpTestingController: HttpTestingController;

  const roleUrl = apiEndpoints.role;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        RoleApiService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(RoleApiService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  describe('Creation', () => {
    it('should create the service', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('getAllRoles', () => {
    it('should send GET request to get all roles', () => {
      const mockRoles: roleResponse[] = [
        {
          id: 'role-1',
          name: 'Admin'
        },
        {
          id: 'role-2',
          name: 'HR'
        },
        {
          id: 'role-3',
          name: 'Manager'
        },
        {
          id: 'role-4',
          name: 'Employee'
        }
      ];

      const mockResponse: ServiceResponse<roleResponse[]> = {
        isSuccess: true,
        data: mockRoles,
        statusCode: 200
      };

      service.getAllRoles().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${roleUrl}/GetAllRoles`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });
});
