import { describe, beforeEach, afterEach, expect, it } from 'vitest';

import { StorageService } from './storage.service';

describe('StorageService', () => {
  let service: StorageService;

  beforeEach(() => {
    service = new StorageService();
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
  });

  describe('Creation', () => {
    it('should create the service', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('setItem', () => {
    it('should store an encrypted value in localStorage', () => {
      const key = 'test-key';
      const value = {
        name: 'Harsh Donda',
        email: 'harsh@test.com'
      };

      service.setItem(key, value);

      const storedValue = localStorage.getItem(key);

      expect(storedValue).not.toBeNull();
      expect(storedValue).not.toBe(JSON.stringify(value));
    });
  });

  describe('getItem', () => {
    it('should return null when the key does not exist', () => {
      const result = service.getItem('missing-key');

      expect(result).toBeNull();
    });

    it('should decrypt and return an object', () => {
      const value = {
        userId: 'user-123',
        name: 'Harsh Donda',
        email: 'harsh@test.com',
        role: 'Admin'
      };

      service.setItem('auth_user', value);
      const result = service.getItem<typeof value>('auth_user');
      expect(result).toEqual(value);
    });

    it('should return null when stored data is invalid', () => {
      localStorage.setItem('invalid-data', 'invalid-encrypted-value');
      const result = service.getItem('invalid-data');
      expect(result).toBeNull();
    });

    it('should return null when encrypted data cannot be parsed as JSON', () => {
      localStorage.setItem('invalid-json', 'not-valid-encrypted-data');
      const result = service.getItem('invalid-json');
      expect(result).toBeNull();
    });
  });

  describe('removeItem', () => {
    it('should remove the specified item from localStorage', () => {
      service.setItem('test-key', {
        name: 'Harsh'
      });

      expect(localStorage.getItem('test-key')).not.toBeNull();
      service.removeItem('test-key');
      expect(localStorage.getItem('test-key')).toBeNull();
    });

    it('should not throw when removing a key that does not exist', () => {
      expect(() => {
        service.removeItem('missing-key');
      }).not.toThrow();

      expect(localStorage.getItem('missing-key')).toBeNull();
    });
  });

  describe('clear', () => {
    it('should clear all items from localStorage', () => {
      service.setItem('user', {
        name: 'Harsh'
      });

      service.setItem('role', 'Admin');

      service.setItem('language', 'en-US');

      expect(localStorage.getItem('user')).not.toBeNull();
      expect(localStorage.getItem('role')).not.toBeNull();
      expect(localStorage.getItem('language')).not.toBeNull();

      service.clear();

      expect(localStorage.getItem('user')).toBeNull();
      expect(localStorage.getItem('role')).toBeNull();
      expect(localStorage.getItem('language')).toBeNull();
    });

    it('should work when localStorage is already empty', () => {
      localStorage.clear();

      expect(() => { service.clear(); }).not.toThrow();

      expect(localStorage.length).toBe(0);
    });
  });
});
