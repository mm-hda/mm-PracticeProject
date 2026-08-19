import { TestBed } from '@angular/core/testing';
import { describe, beforeEach, expect, it, vi } from 'vitest';

import { AuthService } from './auth.service';
import { StorageService } from './storage.service';
import { TokenDto } from '../models/authModels/token.model';

describe('AuthService', () => {
  let service: AuthService;
  let storageService: {
    getItem: ReturnType<typeof vi.fn>;
    setItem: ReturnType<typeof vi.fn>;
    removeItem: ReturnType<typeof vi.fn>;
  };

  const mockUser: TokenDto = {
    userId: 'user-123',
    name: 'Harsh Donda',
    email: 'harsh@test.com',
    role: 'Admin',
    branch: 'Main Branch'
  };

  beforeEach(() => {
    storageService = {
      getItem: vi.fn(),
      setItem: vi.fn(),
      removeItem: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        {
          provide: StorageService,
          useValue: storageService
        }
      ]
    });

    service = TestBed.inject(AuthService);
  });

  describe('Creation', () => {
    it('should create the service', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('Constructor', () => {
    it('should remain unauthenticated when no user exists in storage', () => {
      expect(storageService.getItem).toHaveBeenCalledWith('auth_user');

      expect(service.currentUser()).toBeNull();
      expect(service.isAuthenticated()).toBe(false);
    });

    it('should set the current user when a user exists in storage', () => {
      storageService.getItem.mockReturnValue(mockUser);

      TestBed.resetTestingModule();

      TestBed.configureTestingModule({
        providers: [
          AuthService,
          {
            provide: StorageService,
            useValue: storageService
          }
        ]
      });

      service = TestBed.inject(AuthService);

      expect(storageService.getItem).toHaveBeenCalledWith('auth_user');

      expect(service.currentUser()).toEqual(mockUser);
      expect(service.isAuthenticated()).toBe(true);
    });
  });

  describe('setCurrentUser', () => {
    it('should store the user and update the current user signal', () => {
      service.setCurrentUser(mockUser);

      expect(storageService.setItem).toHaveBeenCalledWith(
        'auth_user',
        mockUser
      );

      expect(service.currentUser()).toEqual(mockUser);
      expect(service.isAuthenticated()).toBe(true);
    });

    it('should replace the existing current user with the new user', () => {
      const firstUser: TokenDto = {
        userId: 'user-1',
        name: 'User One',
        email: 'user1@test.com',
        role: 'Employee',
        branch: 'Branch 1'
      };

      const secondUser: TokenDto = {
        userId: 'user-2',
        name: 'User Two',
        email: 'user2@test.com',
        role: 'Manager',
        branch: 'Branch 2'
      };

      service.setCurrentUser(firstUser);

      expect(service.currentUser()).toEqual(firstUser);
      expect(service.isAuthenticated()).toBe(true);

      service.setCurrentUser(secondUser);

      expect(storageService.setItem).toHaveBeenLastCalledWith(
        'auth_user',
        secondUser
      );

      expect(service.currentUser()).toEqual(secondUser);
      expect(service.isAuthenticated()).toBe(true);
    });
  });

  describe('clearCurrentUser', () => {
    it('should remove the user from storage and clear the current user', () => {
      service.setCurrentUser(mockUser);

      expect(service.isAuthenticated()).toBe(true);

      service.clearCurrentUser();

      expect(storageService.removeItem).toHaveBeenCalledWith('auth_user');

      expect(service.currentUser()).toBeNull();
      expect(service.isAuthenticated()).toBe(false);
    });

    it('should remain unauthenticated when clearCurrentUser is called without a logged-in user', () => {
      expect(service.currentUser()).toBeNull();
      expect(service.isAuthenticated()).toBe(false);

      service.clearCurrentUser();

      expect(storageService.removeItem).toHaveBeenCalledWith('auth_user');

      expect(service.currentUser()).toBeNull();
      expect(service.isAuthenticated()).toBe(false);
    });
  });

  describe('currentUser', () => {
    it('should expose the current user as a readonly signal', () => {
      expect(service.currentUser()).toBeNull();
      service.setCurrentUser(mockUser);
      expect(service.currentUser()).toEqual(mockUser);
    });
  });

  describe('isAuthenticated', () => {
    it('should return false when current user is null', () => {
      expect(service.currentUser()).toBeNull();
      expect(service.isAuthenticated()).toBe(false);
    });

    it('should return true when current user exists', () => {
      service.setCurrentUser(mockUser);

      expect(service.currentUser()).toEqual(mockUser);
      expect(service.isAuthenticated()).toBe(true);
    });

    it('should return false again after current user is cleared', () => {
      service.setCurrentUser(mockUser);
      expect(service.isAuthenticated()).toBe(true);
      service.clearCurrentUser();
      expect(service.isAuthenticated()).toBe(false);
    });
  });
});
