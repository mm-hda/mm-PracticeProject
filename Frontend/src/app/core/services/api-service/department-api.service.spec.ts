import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { describe, beforeEach, afterEach, expect, it } from 'vitest';

import { DepartmentApiService } from './department-api.service';

import { apiEndpoints } from '@app/core/config/api-endpoints';
import { ServiceResponse } from '@app/core/models/service-response.model';
import {
  DepartmentResponse,
  DepartmentUserResponse,
  CreateDepartmentRequest,
  UpdateDepartmentRequest
} from '@app/core/models/departmentModels/department.model';

describe('DepartmentApiService', () => {
  let service: DepartmentApiService;
  let httpTestingController: HttpTestingController;

  const departmentUrl = apiEndpoints.department;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        DepartmentApiService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(DepartmentApiService);
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

  describe('getAllDepartments', () => {
    it('should send GET request to get all departments', () => {
      const mockDepartments: DepartmentResponse[] = [
        {
          id: '1',
          name: 'IT',
          totalPositions: 5,
          totalUsers: 20
        },
        {
          id: '2',
          name: 'HR',
          totalPositions: 3,
          totalUsers: 10
        }
      ];

      const mockResponse: ServiceResponse<DepartmentResponse[]> = {
        isSuccess: true,
        data: mockDepartments,
        statusCode: 200
      };

      service.getAllDepartments().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${departmentUrl}/GetAllDepartments`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });

  describe('getDepartmentById', () => {
    it('should send GET request with department id', () => {
      const departmentId = 'department-123';

      const mockDepartment: DepartmentResponse = {
        id: departmentId,
        name: 'IT',
        totalPositions: 5,
        totalUsers: 20
      };

      const mockResponse: ServiceResponse<DepartmentResponse> = {
        isSuccess: true,
        data: mockDepartment,
        statusCode: 200
      };

      service.getDepartmentById(departmentId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${departmentUrl}/GetDepartmentById/${departmentId}`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });

  describe('getDepartmentEmployees', () => {
    it('should send GET request with department id', () => {
      const departmentId = 'department-123';

      const mockEmployees: DepartmentUserResponse[] = [
        {
          userId: 'user-1',
          name: 'Harsh Donda',
          email: 'harsh@test.com',
          dob: '2003-01-01',
          branchName: 'Main Branch',
          departmentName: 'IT',
          positionName: 'Developer',
          roleName: 'Employee'
        },
        {
          userId: 'user-2',
          name: 'John Doe',
          email: 'john@test.com',
          dob: null,
          branchName: 'Main Branch',
          departmentName: 'IT',
          positionName: 'Manager',
          roleName: 'Manager'
        }
      ];

      const mockResponse: ServiceResponse<DepartmentUserResponse[]> = {
        isSuccess: true,
        data: mockEmployees,
        statusCode: 200
      };

      service.getDepartmentEmployees(departmentId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${departmentUrl}/GetDepartmentEmployees/${departmentId}`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });

  describe('createDepartment', () => {
    it('should send POST request with department data', () => {
      const departmentRequest: CreateDepartmentRequest = {
        name: 'Information Technology'
      };

      const mockResponse: ServiceResponse<string> = {
        isSuccess: true,
        data: 'Department created successfully',
        statusCode: 200
      };

      service.createDepartment(departmentRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${departmentUrl}/CreateDepartment`
      );

      expect(request.request.method).toBe('POST');

      expect(request.request.body).toEqual(departmentRequest);

      request.flush(mockResponse);
    });
  });

  describe('updateDepartment', () => {
    it('should send PUT request with department data', () => {
      const departmentRequest: UpdateDepartmentRequest = {
        id: 'department-123',
        name: 'Updated IT Department'
      };

      const mockResponse: ServiceResponse<string> = {
        isSuccess: true,
        data: 'Department updated successfully',
        statusCode: 200
      };

      service.updateDepartment(departmentRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${departmentUrl}/UpdateDepartment`
      );

      expect(request.request.method).toBe('PUT');

      expect(request.request.body).toEqual(departmentRequest);

      request.flush(mockResponse);
    });
  });
});
