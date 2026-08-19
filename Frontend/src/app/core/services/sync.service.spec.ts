import { TestBed } from '@angular/core/testing';
import { describe, beforeEach, afterEach, expect, it, vi } from 'vitest';
import { of, throwError } from 'rxjs';

import { SyncService } from './sync.service';
import { OfflineQueueService } from './offline-queue.service';
import { UserApiService } from './api-service/user-api.service';
import { ToastService } from './toast.service';

describe('SyncService', () => {
  let service: SyncService;

  let queueService: {
    getPendingUserRequests: ReturnType<typeof vi.fn>;
    markAsSynced: ReturnType<typeof vi.fn>;
  };

  let userApiService: {
    createUser: ReturnType<typeof vi.fn>;
  };

  let toastService: {
    show: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    queueService = {
      getPendingUserRequests: vi.fn(),
      markAsSynced: vi.fn()
    };

    userApiService = {
      createUser: vi.fn()
    };

    toastService = {
      show: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        SyncService,
        {
          provide: OfflineQueueService,
          useValue: queueService
        },
        {
          provide: UserApiService,
          useValue: userApiService
        },
        {
          provide: ToastService,
          useValue: toastService
        }
      ]
    });

    service = TestBed.inject(SyncService);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('Creation', () => {
    it('should create the service', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('syncPendingRequests', () => {
    it('should get pending user requests from the queue', async () => {
      queueService.getPendingUserRequests.mockResolvedValue([]);
      await service.syncPendingRequests();
      expect(queueService.getPendingUserRequests).toHaveBeenCalledTimes(1);
    });

    it('should do nothing when there are no pending requests', async () => {
      queueService.getPendingUserRequests.mockResolvedValue([]);

      await service.syncPendingRequests();

      expect(userApiService.createUser).not.toHaveBeenCalled();
      expect(toastService.show).not.toHaveBeenCalled();
      expect(queueService.markAsSynced).not.toHaveBeenCalled();
    });

    it('should create a pending user successfully', async () => {
      const request = {
        id: 1,
        operation: 'create-user',
        payload: {
          firstName: 'Harsh',
          lastName: 'Donda',
          email: 'harsh@test.com',
          password: 'Password123',
          branchId: 'branch-1',
          departmentId: 'department-1',
          positionId: 'position-1',
          roleId: 'role-1'
        },
        status: 'pending',
        createdAt: '2026-08-18T10:00:00.000Z'
      };

      queueService.getPendingUserRequests.mockResolvedValue([
        request
      ]);

      userApiService.createUser.mockReturnValue(
        of({
          isSuccess: true,
          data: 'User created successfully',
          statusCode: 200
        })
      );

      queueService.markAsSynced.mockResolvedValue(undefined);
      const location = window.location;
      await service.syncPendingRequests();

      expect(userApiService.createUser).toHaveBeenCalledTimes(1);
      expect(userApiService.createUser).toHaveBeenCalledWith(request.payload);

      expect(toastService.show).toHaveBeenCalledWith('User created successfully');

      expect(queueService.markAsSynced).toHaveBeenCalledWith(request.id);
    });

    it('should process multiple create-user requests', async () => {
      const request1 = {
        id: 1,
        operation: 'create-user',
        payload: {
          firstName: 'Harsh',
          lastName: 'Donda',
          email: 'harsh@test.com'
        },
        status: 'pending',
        createdAt: '2026-08-18T10:00:00.000Z'
      };

      const request2 = {
        id: 2,
        operation: 'create-user',
        payload: {
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@test.com'
        },
        status: 'pending',
        createdAt: '2026-08-18T11:00:00.000Z'
      };

      queueService.getPendingUserRequests.mockResolvedValue([
        request1,
        request2
      ]);

      userApiService.createUser
        .mockReturnValueOnce(of({
          isSuccess: true,
          data: 'User 1 created',
          statusCode: 200
        }))
        .mockReturnValueOnce(of({
          isSuccess: true,
          data: 'User 2 created',
          statusCode: 200
        }));

      queueService.markAsSynced.mockResolvedValue(undefined);

      await service.syncPendingRequests();

      expect(userApiService.createUser).toHaveBeenCalledTimes(2);

      expect(toastService.show).toHaveBeenCalledTimes(2);

      expect(queueService.markAsSynced).toHaveBeenCalledTimes(2);
      expect(queueService.markAsSynced).toHaveBeenNthCalledWith(1, 1);
      expect(queueService.markAsSynced).toHaveBeenNthCalledWith(2, 2);
    });

    it('should stop processing when createUser fails', async () => {
      const request1 = {
        id: 1,
        operation: 'create-user',
        payload: {
          firstName: 'Harsh',
          lastName: 'Donda',
          email: 'harsh@test.com'
        },
        status: 'pending',
        createdAt: '2026-08-18T10:00:00.000Z'
      };

      const request2 = {
        id: 2,
        operation: 'create-user',
        payload: {
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@test.com'
        },
        status: 'pending',
        createdAt: '2026-08-18T11:00:00.000Z'
      };

      queueService.getPendingUserRequests.mockResolvedValue([
        request1,
        request2
      ]);

      userApiService.createUser.mockReturnValue(
        throwError(() => new Error('API error'))
      );

      await service.syncPendingRequests();

      expect(userApiService.createUser).toHaveBeenCalledTimes(1);
      expect(toastService.show).not.toHaveBeenCalled();
      expect(queueService.markAsSynced).not.toHaveBeenCalled();
    });

    it('should stop processing when markAsSynced fails', async () => {
      const request = {
        id: 1,
        operation: 'create-user',
        payload: {
          firstName: 'Harsh',
          lastName: 'Donda',
          email: 'harsh@test.com'
        },
        status: 'pending',
        createdAt: '2026-08-18T10:00:00.000Z'
      };

      queueService.getPendingUserRequests.mockResolvedValue([request]);

      userApiService.createUser.mockReturnValue(
        of({
          isSuccess: true,
          data: 'User created successfully',
          statusCode: 200
        })
      );

      queueService.markAsSynced.mockRejectedValue(new Error('Delete failed'));

      await service.syncPendingRequests();

      expect(userApiService.createUser).toHaveBeenCalledTimes(1);
      expect(toastService.show).toHaveBeenCalledWith('User created successfully');
      expect(queueService.markAsSynced).toHaveBeenCalledWith(request.id);
    });

    it('should ignore requests with unsupported operations', async () => {
      const request = {
        id: 1,
        operation: 'unknown-operation',
        payload: {
          someData: 'test'
        },
        status: 'pending',
        createdAt: '2026-08-18T10:00:00.000Z'
      };

      queueService.getPendingUserRequests.mockResolvedValue([request]);

      await service.syncPendingRequests();

      expect(userApiService.createUser).not.toHaveBeenCalled();
      expect(toastService.show).not.toHaveBeenCalled();
      expect(queueService.markAsSynced).not.toHaveBeenCalled();
    });

    it('should continue safely when an unsupported operation is present', async () => {
      const request1 = {
        id: 1,
        operation: 'unknown-operation',
        payload: {},
        status: 'pending',
        createdAt: '2026-08-18T10:00:00.000Z'
      };

      const request2 = {
        id: 2,
        operation: 'create-user',
        payload: {
          firstName: 'Harsh',
          lastName: 'Donda',
          email: 'harsh@test.com'
        },
        status: 'pending',
        createdAt: '2026-08-18T11:00:00.000Z'
      };

      queueService.getPendingUserRequests.mockResolvedValue([
        request1,
        request2
      ]);

      userApiService.createUser.mockReturnValue(
        of({
          isSuccess: true,
          data: 'User created successfully',
          statusCode: 200
        })
      );

      queueService.markAsSynced.mockResolvedValue(undefined);
      await service.syncPendingRequests();

      expect(userApiService.createUser).toHaveBeenCalledTimes(1);
      expect(userApiService.createUser).toHaveBeenCalledWith(request2.payload);
      expect(queueService.markAsSynced).toHaveBeenCalledWith(2);
    });
  });
});
