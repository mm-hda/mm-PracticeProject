import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { describe, beforeEach, afterEach, expect, it } from 'vitest';

import { ProjectApiService } from './project-api.service';

import { apiEndpoints } from '@app/core/config/api-endpoints';
import { ServiceResponse } from '@app/core/models/service-response.model';
import {
  projectCreateRequest,
  projectResponse,
  projectUpdateRequest,
  ProjectUserResponse
} from '@app/core/models/projectModels/project.model';

describe('ProjectApiService', () => {
  let service: ProjectApiService;
  let httpTestingController: HttpTestingController;

  const projectUrl = apiEndpoints.project;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ProjectApiService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(ProjectApiService);
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

  describe('createProject', () => {
    it('should send POST request with project data', () => {
      const projectRequest: projectCreateRequest = {
        name: 'HRMS Project',
        description: 'Human Resource Management System',
        startDate: '2026-01-01',
        endDate: '2026-12-31',
        projectManagerId: 'manager-123'
      };

      const mockResponse: ServiceResponse<string> = {
        isSuccess: true,
        data: 'Project created successfully',
        statusCode: 200
      };

      service.createProject(projectRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${projectUrl}/CreateProject`
      );

      expect(request.request.method).toBe('POST');
      expect(request.request.body).toEqual(projectRequest);

      request.flush(mockResponse);
    });
  });

  describe('getAllProjects', () => {
    it('should send GET request to get all projects', () => {
      const mockProjects: projectResponse[] = [
        {
          id: 'project-1',
          name: 'HRMS Project',
          description: 'Human Resource Management System',
          startDate: '2026-01-01',
          endDate: '2026-12-31',
          projectManagerId: 'manager-123',
          projectManagerName: 'Harsh Donda',
          totalUsers: 10
        },
        {
          id: 'project-2',
          name: 'Payroll Project',
          description: 'Payroll Management System',
          startDate: '2026-02-01',
          endDate: '2026-11-30',
          projectManagerId: 'manager-456',
          projectManagerName: 'John Doe',
          totalUsers: 5
        }
      ];

      const mockResponse: ServiceResponse<projectResponse[]> = {
        isSuccess: true,
        data: mockProjects,
        statusCode: 200
      };

      service.getAllProjects().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${projectUrl}/GetAllProjects`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });

  describe('updateProject', () => {
    it('should send PUT request with project data', () => {
      const projectRequest: projectUpdateRequest = {
        id: 'project-123',
        name: 'Updated HRMS Project',
        description: 'Updated project description',
        startDate: '2026-01-15',
        endDate: '2026-12-31',
        projectManagerId: 'manager-123'
      };

      const mockResponse: ServiceResponse<string> = {
        isSuccess: true,
        data: 'Project updated successfully',
        statusCode: 200
      };

      service.updateProject(projectRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${projectUrl}/UpdateProject`
      );

      expect(request.request.method).toBe('PUT');
      expect(request.request.body).toEqual(projectRequest);

      request.flush(mockResponse);
    });
  });

  describe('getProjectEmployees', () => {
    it('should send GET request with project id', () => {
      const projectId = 'project-123';

      const mockEmployees: ProjectUserResponse[] = [
        {
          UserId: 'user-1',
          name: 'Harsh Donda',
          email: 'harsh@test.com',
          dob: '2003-01-01',
          branchName: 'Main Branch',
          departmentName: 'IT',
          positionName: 'Software Developer',
          roleName: 'Employee'
        },
        {
          UserId: 'user-2',
          name: 'John Doe',
          email: 'john@test.com',
          dob: null,
          branchName: 'Main Branch',
          departmentName: 'IT',
          positionName: 'Senior Developer',
          roleName: 'Employee'
        }
      ];

      const mockResponse: ServiceResponse<ProjectUserResponse[]> = {
        isSuccess: true,
        data: mockEmployees,
        statusCode: 200
      };

      service.getProjectEmployees(projectId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${projectUrl}/GetProjectEmployees/${projectId}`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });

  describe('getProjectsByManagerId', () => {
    it('should send GET request with manager id', () => {
      const managerId = 'manager-123';

      const mockProjects: projectResponse[] = [
        {
          id: 'project-1',
          name: 'HRMS Project',
          description: 'Human Resource Management System',
          startDate: '2026-01-01',
          endDate: '2026-12-31',
          projectManagerId: managerId,
          projectManagerName: 'Harsh Donda',
          totalUsers: 10
        }
      ];

      const mockResponse: ServiceResponse<projectResponse[]> = {
        isSuccess: true,
        data: mockProjects,
        statusCode: 200
      };

      service.getProjectsByManagerId(managerId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${projectUrl}/GetProjectsByManagerId/${managerId}`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });

  describe('getEmployeeProjects', () => {
    it('should send GET request with user id', () => {
      const userId = 'user-123';

      const mockProjects: projectResponse[] = [
        {
          id: 'project-1',
          name: 'HRMS Project',
          description: 'HRMS Project',
          startDate: '2026-01-01',
          endDate: '2026-12-31',
          projectManagerId: 'manager-123',
          projectManagerName: 'Harsh Donda',
          totalUsers: 10
        }
      ];

      const mockResponse: ServiceResponse<projectResponse[]> = {
        isSuccess: true,
        data: mockProjects,
        statusCode: 200
      };

      service.getEmployeeProjects(userId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${projectUrl}/GetEmployeeProjects/${userId}`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });
});
