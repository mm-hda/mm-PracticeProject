import { describe, beforeEach, afterEach, expect, it, vi } from 'vitest';

import { OfflineQueueService } from './offline-queue.service';
import { db, PendingRequest } from '../offline/app-db';

describe('OfflineQueueService', () => {
  let service: OfflineQueueService;

  beforeEach(() => {
    service = new OfflineQueueService();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('Creation', () => {
    it('should create the service', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('addRequest', () => {
    it('should add a pending request to the database', async () => {
      const addSpy = vi.spyOn(db.pendingRequests, 'add').mockResolvedValue(1);

      const operation = 'create-user';

      const payload = {
        name: 'Harsh Donda',
        email: 'harsh@test.com'
      };

      await service.addRequest(operation, payload);
      expect(addSpy).toHaveBeenCalledTimes(1);

      expect(addSpy).toHaveBeenCalledWith({
        operation,
        payload,
        status: 'pending',
        createdAt: expect.any(String)
      });
    });

    it('should store the provided operation correctly', async () => {
      const addSpy = vi.spyOn(db.pendingRequests, 'add').mockResolvedValue(1);
      await service.addRequest('create-user', {
        name: 'Harsh'
      });

      const request = addSpy.mock.calls[0][0];
      expect(request.operation).toBe('create-user');
    });

    it('should store the provided payload correctly', async () => {
      const addSpy = vi.spyOn(db.pendingRequests, 'add').mockResolvedValue(1);
      const payload = {
        userId: 'user-123',
        name: 'Harsh Donda'
      };

      await service.addRequest('create-user', payload);
      const request = addSpy.mock.calls[0][0];
      expect(request.payload).toEqual(payload);
    });

    it('should set the status to pending', async () => {
      const addSpy = vi.spyOn(db.pendingRequests, 'add').mockResolvedValue(1);
      await service.addRequest('create-user', { name: 'Harsh' });
      const request = addSpy.mock.calls[0][0];
      expect(request.status).toBe('pending');
    });

    it('should create a valid createdAt timestamp', async () => {
      const addSpy = vi
        .spyOn(db.pendingRequests, 'add')
        .mockResolvedValue(1);

      await service.addRequest('create-user', { name: 'Harsh' });

      const request = addSpy.mock.calls[0][0];

      expect(request.createdAt).toEqual(expect.any(String));
      expect(new Date(request.createdAt).toISOString()).toBe(request.createdAt);
    });

    it('should propagate the database error when adding fails', async () => {
      const error = new Error('Database error');
      vi.spyOn(db.pendingRequests, 'add').mockRejectedValue(error);
      await expect(service.addRequest('create-user', { name: 'Harsh' })).rejects.toThrow('Database error');
    });
  });

  describe('markAsSynced', () => {
    it('should delete the request using the provided id', async () => {
      const deleteSpy = vi.spyOn(db.pendingRequests, 'delete').mockResolvedValue(undefined);
      const id = 123;

      await service.markAsSynced(id);
      expect(deleteSpy).toHaveBeenCalledTimes(1);
      expect(deleteSpy).toHaveBeenCalledWith(id);
    });

    it('should work with different request ids', async () => {
      const deleteSpy = vi.spyOn(db.pendingRequests, 'delete').mockResolvedValue(undefined);

      await service.markAsSynced(1);
      await service.markAsSynced(999);

      expect(deleteSpy).toHaveBeenNthCalledWith(1, 1);
      expect(deleteSpy).toHaveBeenNthCalledWith(2, 999);
    });

    it('should propagate the database error when deleting fails', async () => {
      const error = new Error('Delete failed');
      vi.spyOn(db.pendingRequests, 'delete').mockRejectedValue(error);
      await expect(service.markAsSynced(123)).rejects.toThrow('Delete failed');
    });
  });

  describe('getPendingUserRequests', () => {
    it('should search for create-user operation and return matching requests', async () => {
      const mockRequests: PendingRequest[] = [
        {
          id: 1,
          operation: 'create-user',
          payload: {
            name: 'Harsh Donda',
            email: 'harsh@test.com'
          },
          status: 'pending',
          createdAt: '2026-08-18T10:00:00.000Z'
        },
        {
          id: 2,
          operation: 'create-user',
          payload: {
            name: 'John Doe',
            email: 'john@test.com'
          },
          status: 'pending',
          createdAt: '2026-08-18T11:00:00.000Z'
        }
      ];

      const toArray = vi.fn().mockResolvedValue(mockRequests);
      const equals = vi.fn().mockReturnValue({ toArray });
      const where = vi.spyOn(db.pendingRequests, 'where').mockReturnValue({ equals } as any);
      const result = await service.getPendingUserRequests();

      expect(where).toHaveBeenCalledWith('operation');
      expect(equals).toHaveBeenCalledWith('create-user');
      expect(toArray).toHaveBeenCalledTimes(1);
      expect(result).toEqual(mockRequests);
    });

    it('should return an empty array when there are no create-user requests', async () => {
      const toArray = vi.fn().mockResolvedValue([]);

      const equals = vi.fn().mockReturnValue({ toArray });

      vi.spyOn(db.pendingRequests, 'where').mockReturnValue({ equals } as any);

      const result = await service.getPendingUserRequests();
      expect(result).toEqual([]);
      expect(equals).toHaveBeenCalledWith('create-user');
      expect(toArray).toHaveBeenCalledTimes(1);
    });

    it('should return all matching create-user requests', async () => {
      const mockRequests: PendingRequest[] = [
        {
          id: 1,
          operation: 'create-user',
          payload: {
            name: 'User One'
          },
          status: 'pending',
          createdAt: '2026-08-18T10:00:00.000Z'
        },
        {
          id: 2,
          operation: 'create-user',
          payload: {
            name: 'User Two'
          },
          status: 'failed',
          createdAt: '2026-08-18T11:00:00.000Z'
        },
        {
          id: 3,
          operation: 'create-user',
          payload: {
            name: 'User Three'
          },
          status: 'syncing',
          createdAt: '2026-08-18T12:00:00.000Z'
        }
      ];

      const toArray = vi.fn().mockResolvedValue(mockRequests);
      const equals = vi.fn().mockReturnValue({ toArray });

      vi.spyOn(db.pendingRequests, 'where').mockReturnValue({ equals } as any);

      const result = await service.getPendingUserRequests();

      expect(result).toHaveLength(3);
      expect(result).toEqual(mockRequests);
    });

    it('should propagate the database error when fetching requests fails', async () => {
      const error = new Error('Fetch failed');

      const toArray = vi.fn().mockRejectedValue(error);
      const equals = vi.fn().mockReturnValue({ toArray });

      vi.spyOn(db.pendingRequests, 'where').mockReturnValue({ equals } as any);
      await expect(service.getPendingUserRequests()).rejects.toThrow('Fetch failed');
      expect(equals).toHaveBeenCalledWith('create-user');
      expect(toArray).toHaveBeenCalledTimes(1);
    });
  });
});
