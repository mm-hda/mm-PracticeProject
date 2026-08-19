import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { describe, beforeEach, afterEach, expect, it } from 'vitest';

import { PositionApiService } from './position-api.service';

import { apiEndpoints } from '@app/core/config/api-endpoints';
import { ServiceResponse } from '@app/core/models/service-response.model';
import {
  CreatePositionRequest,
  PositionResponse,
  PositionUserResponse,
  UpdatePositionRequest
} from '../../models/positionModels/position.model';

describe('PositionApiService', () => {
  let service: PositionApiService;
  let httpTestingController: HttpTestingController;

  const positionUrl = apiEndpoints.position;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        PositionApiService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(PositionApiService);
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

  describe('getAllPositions', () => {
    it('should send GET request to get all positions', () => {
      const mockPositions: PositionResponse[] = [
        {
          id: 'position-1',
          name: 'Software Developer',
          departmentId: 'department-1',
          departmentName: 'IT',
          totalUsers: 10
        },
        {
          id: 'position-2',
          name: 'HR Manager',
          departmentId: 'department-2',
          departmentName: 'HR',
          totalUsers: 5
        }
      ];

      const mockResponse: ServiceResponse<PositionResponse[]> = {
        isSuccess: true,
        data: mockPositions,
        statusCode: 200
      };

      service.getAllPositions().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${positionUrl}/GetAllPositions`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });

  describe('getPositionById', () => {
    it('should send GET request with position id', () => {
      const positionId = 'position-123';

      const mockPosition: PositionResponse = {
        id: positionId,
        name: 'Software Developer',
        departmentId: 'department-1',
        departmentName: 'IT',
        totalUsers: 10
      };

      const mockResponse: ServiceResponse<PositionResponse> = {
        isSuccess: true,
        data: mockPosition,
        statusCode: 200
      };

      service.getPositionById(positionId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${positionUrl}/GetPositionById/${positionId}`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });

  describe('getPositionEmployees', () => {
    it('should send GET request with position id', () => {
      const positionId = 'position-123';

      const mockEmployees: PositionUserResponse[] = [
        {
          userId: 'user-1',
          name: 'Harsh Donda',
          email: 'harsh@test.com',
          dob: '2003-01-01',
          branchName: 'Main Branch',
          departmentName: 'IT',
          positionName: 'Software Developer',
          roleName: 'Employee'
        },
        {
          userId: 'user-2',
          name: 'John Doe',
          email: 'john@test.com',
          dob: null,
          branchName: 'Main Branch',
          departmentName: 'IT',
          positionName: 'Software Developer',
          roleName: 'Employee'
        }
      ];

      const mockResponse: ServiceResponse<PositionUserResponse[]> = {
        isSuccess: true,
        data: mockEmployees,
        statusCode: 200
      };

      service.getPositionEmployees(positionId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${positionUrl}/GetPositionUsers/${positionId}`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });

  describe('createPosition', () => {
    it('should send POST request with position data', () => {
      const positionRequest: CreatePositionRequest = {
        name: 'Software Developer',
        departmentId: 'department-123'
      };

      const mockResponse: ServiceResponse<string> = {
        isSuccess: true,
        data: 'Position created successfully',
        statusCode: 200
      };

      service.createPosition(positionRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${positionUrl}/CreatePosition`
      );

      expect(request.request.method).toBe('POST');
      expect(request.request.body).toEqual(positionRequest);

      request.flush(mockResponse);
    });
  });

  describe('updatePosition', () => {
    it('should send PUT request with position data', () => {
      const positionRequest: UpdatePositionRequest = {
        id: 'position-123',
        name: 'Senior Software Developer',
        departmentId: 'department-123'
      };

      const mockResponse: ServiceResponse<string> = {
        isSuccess: true,
        data: 'Position updated successfully',
        statusCode: 200
      };

      service.updatePosition(positionRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${positionUrl}/UpdatePosition`
      );

      expect(request.request.method).toBe('PUT');
      expect(request.request.body).toEqual(positionRequest);

      request.flush(mockResponse);
    });
  });

  describe('getPositionByDepartment', () => {
    it('should send GET request with department id', () => {
      const departmentId = 'department-123';

      const mockPositions: PositionResponse[] = [
        {
          id: 'position-1',
          name: 'Software Developer',
          departmentId,
          departmentName: 'IT',
          totalUsers: 10
        },
        {
          id: 'position-2',
          name: 'Senior Software Developer',
          departmentId,
          departmentName: 'IT',
          totalUsers: 5
        }
      ];

      const mockResponse: ServiceResponse<PositionResponse[]> = {
        isSuccess: true,
        data: mockPositions,
        statusCode: 200
      };

      service.getPositionByDepartment(departmentId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${positionUrl}/GetPositionsByDepartment/${departmentId}`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });
});
