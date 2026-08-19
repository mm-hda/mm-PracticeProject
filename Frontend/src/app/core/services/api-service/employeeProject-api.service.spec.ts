import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { describe, beforeEach, afterEach, expect, it } from 'vitest';

import { EmployeeProjectApiService } from './employeeProject-api.service';

import { apiEndpoints } from '@app/core/config/api-endpoints';
import { ServiceResponse } from '@app/core/models/service-response.model';
import {
  CreateEmployeeProjectRequest,
  EmployeeProjectResponse
} from '@app/core/models/employeeProjectModels/employeeProject.model';

describe('EmployeeProjectApiService', () => {
  let service: EmployeeProjectApiService;
  let httpTestingController: HttpTestingController;

  const employeeProjectUrl = apiEndpoints.employeeProject;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        EmployeeProjectApiService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(EmployeeProjectApiService);
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

  describe('CreateEmployeeProject', () => {
    it('should send POST request with employee project data', () => {
      const requestData: CreateEmployeeProjectRequest = {
        userId: 'user-123',
        projectId: 'project-123'
      };

      const mockResponse: ServiceResponse<string> = {
        isSuccess: true,
        data: 'Employee project created successfully',
        statusCode: 200
      };

      service.CreateEmployeeProject(requestData).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${employeeProjectUrl}/CreateEmployeeProject`
      );

      expect(request.request.method).toBe('POST');
      expect(request.request.body).toEqual(requestData);

      request.flush(mockResponse);
    });
  });

  describe('DeleteEmployeeProject', () => {
    it('should send DELETE request with employee project id', () => {
      const employeeProjectId = 'employee-project-123';

      const mockResponse: ServiceResponse<string> = {
        isSuccess: true,
        data: 'Employee project deleted successfully',
        statusCode: 200
      };

      service.DeleteEmployeeProject(employeeProjectId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${employeeProjectUrl}/RemoveEmployeeProject/${employeeProjectId}`
      );

      expect(request.request.method).toBe('DELETE');

      request.flush(mockResponse);
    });
  });

  describe('GetEmployeeProjectsByUserId', () => {
    it('should send GET request with user id', () => {
      const userId = 'user-123';

      const mockProjects: EmployeeProjectResponse[] = [
        {
          id: 'employee-project-123',
          userId: 'user-123',
          userName: 'Harsh Donda',
          userEmail: 'harsh@test.com',
          roleName: 'Developer',
          projectId: 'project-123',
          projectName: 'HRMS Project',
          assignedDate: new Date('2026-01-15')
        }
      ];

      const mockResponse: ServiceResponse<EmployeeProjectResponse[]> = {
        isSuccess: true,
        data: mockProjects,
        statusCode: 200
      };

      service.GetEmployeeProjectsByUserId(userId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${employeeProjectUrl}/GetUserProjectsByUserId/${userId}`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });
});
