import { describe, beforeEach, afterEach, expect, it, vi } from 'vitest';

import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    vi.useFakeTimers();
    service = new ToastService();
  });

  afterEach(() => {
    service.clear();
    vi.clearAllTimers();
    vi.useRealTimers();
  });

  describe('Creation', () => {
    it('should create the service', () => {
      expect(service).toBeTruthy();
    });

    it('should have null message initially', () => {
      expect(service.message()).toBeNull();
    });
  });

  describe('show', () => {
    it('should set the provided message', () => {
      service.show('User created successfully');
      expect(service.message()).toBe('User created successfully');
    });

    it('should replace the existing message with a new message', () => {
      service.show('First message');
      expect(service.message()).toBe('First message');
      service.show('Second message');
      expect(service.message()).toBe('Second message');
    });

    it('should clear the message after 3 seconds', () => {
      service.show('User created successfully');
      expect(service.message()).toBe('User created successfully');
      vi.advanceTimersByTime(2999);
      expect(service.message()).toBe('User created successfully');
      vi.advanceTimersByTime(1);
      expect(service.message()).toBeNull();
    });

    it('should call clear after 3 seconds', () => {
      const clearSpy = vi.spyOn(service, 'clear');
      service.show('Test message');
      expect(clearSpy).not.toHaveBeenCalled();
      vi.advanceTimersByTime(3000);
      expect(clearSpy).toHaveBeenCalledTimes(1);
    });

    it('should show an empty message when an empty string is provided', () => {
      service.show('');
      expect(service.message()).toBe('');
    });
  });

  describe('clear', () => {
    it('should clear the current message', () => {
      service.show('User created successfully');
      expect(service.message()).toBe('User created successfully');
      service.clear();
      expect(service.message()).toBeNull();
    });

    it('should keep the message null when clear is called without a message', () => {
      expect(service.message()).toBeNull();
      service.clear();
      expect(service.message()).toBeNull();
    });

    it('should clear the active timeout', () => {
      const clearTimeoutSpy = vi.spyOn(globalThis, 'clearTimeout');
      service.show('Test message');
      service.clear();
      expect(clearTimeoutSpy).toHaveBeenCalledTimes(1);
    });

    it('should prevent the timeout from clearing a new message after clear', () => {
      service.show('First message');
      service.clear();
      service.show('Second message');
      vi.advanceTimersByTime(2999);

      expect(service.message()).toBe('Second message');
      vi.advanceTimersByTime(1);
      expect(service.message()).toBeNull();
    });
  });

  describe('show and clear interaction', () => {
    it('should clear the previous timeout when clear is called before 3 seconds', () => {
      service.show('First message');
      vi.advanceTimersByTime(1000);
      service.clear();

      expect(service.message()).toBeNull();
      vi.advanceTimersByTime(2000);
      expect(service.message()).toBeNull();
    });

    it('should display the latest message when show is called multiple times', () => {
      service.show('Message 1');
      vi.advanceTimersByTime(1000);
      service.show('Message 2');

      expect(service.message()).toBe('Message 2');
      vi.advanceTimersByTime(2000);
      expect(service.message()).toBeNull();
    });
  });
});
