import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { describe, beforeEach, afterEach, expect, it } from 'vitest';

import { UserApiService } from './user-api.service';

import { apiEndpoints } from '@app/core/config/api-endpoints';
import { ServiceResponse } from '@app/core/models/service-response.model';
import {
  UserResponse,
  CreateUserRequest,
  userFilterRequest,
  searchUserRequest,
  paginationRequest,
  updateUserRequest
} from '../../models/userModels/user.model';
import { ManagerResponse } from '../../models/projectModels/project.model';

describe('UserApiService', () => {
  let service: UserApiService;
  let httpTestingController: HttpTestingController;

  const userUrl = apiEndpoints.user;
  const authUrl = apiEndpoints.auth;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        UserApiService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(UserApiService);
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

  describe('getAllUsers', () => {
    it('should send GET request with pagination parameters', () => {
      const paginationRequest: paginationRequest = {
        pageNumber: 1,
        pageSize: 10
      };

      const mockUsers: UserResponse[] = [
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
          branchName: 'Second Branch',
          departmentName: 'HR',
          positionName: 'HR Manager',
          roleName: 'HR'
        }
      ];

      const mockResponse: ServiceResponse<UserResponse[]> = {
        isSuccess: true,
        data: mockUsers,
        statusCode: 200
      };

      service.getAllUsers(paginationRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${userUrl}/GetAllUsers?pageNumber=1&pageSize=10`
      );

      expect(request.request.method).toBe('GET');
      expect(request.request.params.get('pageNumber')).toBe('1');
      expect(request.request.params.get('pageSize')).toBe('10');

      request.flush(mockResponse);
    });
  });

  describe('getUserById', () => {
    it('should send GET request with user id', () => {
      const userId = 'user-123';

      const mockUser: UserResponse = {
        userId,
        name: 'Harsh Donda',
        email: 'harsh@test.com',
        dob: '2003-01-01',
        branchName: 'Main Branch',
        departmentName: 'IT',
        positionName: 'Software Developer',
        roleName: 'Employee'
      };

      const mockResponse: ServiceResponse<UserResponse> = {
        isSuccess: true,
        data: mockUser,
        statusCode: 200
      };

      service.getUserById(userId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${userUrl}/GetUserById/${userId}`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });

  describe('getUserBySearch', () => {
    it('should send GET request with search term', () => {
      const searchRequest: searchUserRequest = {
        searchTerm: 'Harsh'
      };

      const mockUsers: UserResponse[] = [
        {
          userId: 'user-1',
          name: 'Harsh Donda',
          email: 'harsh@test.com',
          dob: '2003-01-01',
          branchName: 'Main Branch',
          departmentName: 'IT',
          positionName: 'Software Developer',
          roleName: 'Employee'
        }
      ];

      const mockResponse: ServiceResponse<UserResponse[]> = {
        isSuccess: true,
        data: mockUsers,
        statusCode: 200
      };

      service.getUserBySearch(searchRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${userUrl}/GetUserBySearch?searchTerm=Harsh`
      );

      expect(request.request.method).toBe('GET');
      expect(request.request.params.get('searchTerm')).toBe('Harsh');

      request.flush(mockResponse);
    });

    it('should send empty search term when searchTerm is undefined', () => {
      const searchRequest: searchUserRequest = {};

      const mockResponse: ServiceResponse<UserResponse[]> = {
        isSuccess: true,
        data: [],
        statusCode: 200
      };

      service.getUserBySearch(searchRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${userUrl}/GetUserBySearch?searchTerm=`
      );

      expect(request.request.method).toBe('GET');
      expect(request.request.params.get('searchTerm')).toBe('');

      request.flush(mockResponse);
    });
  });

  describe('getUsersByFilter', () => {
    it('should send POST request with filter data', () => {
      const filterRequest: userFilterRequest = {
        roleId: 'role-123',
        branchId: 'branch-123',
        departmentId: 'department-123',
        positionId: 'position-123'
      };

      const mockUsers: UserResponse[] = [
        {
          userId: 'user-1',
          name: 'Harsh Donda',
          email: 'harsh@test.com',
          dob: '2003-01-01',
          branchName: 'Main Branch',
          departmentName: 'IT',
          positionName: 'Software Developer',
          roleName: 'Employee'
        }
      ];

      const mockResponse: ServiceResponse<UserResponse[]> = {
        isSuccess: true,
        data: mockUsers,
        statusCode: 200
      };

      service.getUsersByFilter(filterRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${userUrl}/GetUsersByFilter`
      );

      expect(request.request.method).toBe('POST');
      expect(request.request.body).toEqual({
        request: filterRequest
      });

      request.flush(mockResponse);
    });
  });

  describe('createUser', () => {
    it('should send POST request with user data to register endpoint', () => {
      const userRequest: CreateUserRequest = {
        firstName: 'Harsh',
        lastName: 'Donda',
        email: 'harsh@test.com',
        password: 'Password@123',
        dob: '2003-01-01',
        branchId: 'branch-123',
        departmentId: 'department-123',
        positionId: 'position-123',
        roleId: 'role-123'
      };

      const mockResponse: ServiceResponse<string> = {
        isSuccess: true,
        data: 'User created successfully',
        statusCode: 200
      };

      service.createUser(userRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${authUrl}/Register`
      );

      expect(request.request.method).toBe('POST');
      expect(request.request.body).toEqual(userRequest);

      request.flush(mockResponse);
    });
  });

  describe('getManagers', () => {
    it('should send GET request to get managers', () => {
      const mockManagers: ManagerResponse[] = [
        {
          userId: 'manager-1',
          name: 'Harsh Donda',
          email: 'harsh@test.com'
        },
        {
          userId: 'manager-2',
          name: 'John Doe',
          email: 'john@test.com'
        }
      ];

      const mockResponse: ServiceResponse<ManagerResponse[]> = {
        isSuccess: true,
        data: mockManagers,
        statusCode: 200
      };

      service.getManagers().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${userUrl}/GetManagers`
      );

      expect(request.request.method).toBe('GET');

      request.flush(mockResponse);
    });
  });

  describe('updateUser', () => {
    it('should send PUT request with user id and update data', () => {
      const userId = 'user-123';

      const updateRequest: updateUserRequest = {
        userId,
        firstName: 'Harsh',
        lastName: 'Donda Updated',
        email: 'harsh.updated@test.com',
        dob: '2003-01-01',
        branchId: 'branch-123',
        departmentId: 'department-123',
        positionId: 'position-123',
        roleId: 'role-123'
      };

      const mockResponse: ServiceResponse<string> = {
        isSuccess: true,
        data: 'User updated successfully',
        statusCode: 200
      };

      service.updateUser(userId, updateRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const request = httpTestingController.expectOne(
        `${userUrl}/UpdateUser/${userId}`
      );

      expect(request.request.method).toBe('PUT');
      expect(request.request.body).toEqual(updateRequest);

      request.flush(mockResponse);
    });
  });
});
