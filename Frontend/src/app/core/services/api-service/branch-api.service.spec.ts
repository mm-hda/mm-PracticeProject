import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { describe, beforeEach, afterEach, expect, it } from 'vitest';

import { BranchApiService } from './branch-api.service';

import { apiEndpoints } from '@app/core/config/api-endpoints';
import { ServiceResponse } from '@app/core/models/service-response.model';
import {
  BranchResponse,
  BranchUserResponse,
  CreateBranchRequest,
  UpdateBranchRequest
} from '@app/core/models/branchModels/branch.model';

describe('BranchApiService', () => {
  let service: BranchApiService;
  let httpTestingController: HttpTestingController;

  const branchUrl = apiEndpoints.branch;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        BranchApiService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(BranchApiService);
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

  describe('getAllBranches', () => {
    it('should send GET request to get all branches', () => {
      const mockBranches: BranchResponse[] = [
        {
          id: '1',
          name: 'Main Branch'
        },
        {
          id: '2',
          name: 'Second Branch'
        }
      ] as BranchResponse[];

      const mockResponse: ServiceResponse<BranchResponse[]> = {
        isSuccess: true,
        data: mockBranches,
        statusCode: 200
      };

      service.getAllBranches().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${branchUrl}/GetAllBranches`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });

  describe('getBranchById', () => {
    it('should send GET request with branch id', () => {
      const branchId = 'branch-123';

      const mockBranch: BranchResponse = {
        id: branchId,
        name: 'Main Branch'
      } as BranchResponse;

      const mockResponse: ServiceResponse<BranchResponse> = {
        isSuccess: true,
        data: mockBranch,
        statusCode: 200
      };

      service.getBranchById(branchId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${branchUrl}/GetBranchById/${branchId}`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });

  describe('getBranchEmployees', () => {
    it('should send GET request with branch id', () => {
      const branchId = 'branch-123';

      const mockEmployees: BranchUserResponse[] = [
        {
          userId: 'user-1',
          name: 'Harsh Donda',
          email: 'harsh@test.com'
        },
        {
          userId: 'user-2',
          name: 'John Doe',
          email: 'john@test.com'
        }
      ] as BranchUserResponse[];

      const mockResponse: ServiceResponse<BranchUserResponse[]> = {
        isSuccess: true,
        data: mockEmployees,
        statusCode: 200
      };

      service.getBranchEmployees(branchId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${branchUrl}/GetBranchUsers/${branchId}`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });

  describe('createBranch', () => {
    it('should send POST request with branch data', () => {
      const branchRequest: CreateBranchRequest = {
        name: 'New Branch'
      } as CreateBranchRequest;

      const mockResponse: ServiceResponse<string> = {
        isSuccess: true,
        data: 'Branch created successfully',
        statusCode: 200
      };

      service.createBranch(branchRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${branchUrl}/CreateBranch`
      );

      expect(request.request.method).toBe('POST');

      expect(request.request.body).toEqual(branchRequest);

      request.flush(mockResponse);
    });
  });

  describe('updateBranch', () => {
    it('should send PUT request with branch data', () => {
      const branchRequest: UpdateBranchRequest = {
        id: 'branch-123',
        name: 'Updated Branch'
      } as UpdateBranchRequest;

      const mockResponse: ServiceResponse<string> = {
        isSuccess: true,
        data: 'Branch updated successfully',
        statusCode: 200
      };

      service.updateBranch(branchRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${branchUrl}/UpdateBranch`
      );

      expect(request.request.method).toBe('PUT');

      expect(request.request.body).toEqual(branchRequest);

      request.flush(mockResponse);
    });
  });
});
