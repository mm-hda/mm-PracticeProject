import { TestBed } from '@angular/core/testing';
import { describe, it, expect, vi, beforeEach } from 'vitest';

import { App } from './app';

import { SyncService } from './core/services/sync.service';
import { LanguageService } from './core/services/language.service';

describe('App', () => {
  let syncServiceMock: {
    syncPendingRequests: ReturnType<typeof vi.fn>;
  };

  let languageServiceMock: {
    getCurrentLanguage: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    syncServiceMock = {
      syncPendingRequests: vi.fn()
    };

    languageServiceMock = {
      getCurrentLanguage: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        {
          provide: SyncService,
          useValue: syncServiceMock
        },
        {
          provide: LanguageService,
          useValue: languageServiceMock
        }
      ]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);

    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should call syncPendingRequests when app is created', () => {
    TestBed.createComponent(App);

    expect(syncServiceMock.syncPendingRequests).toHaveBeenCalled();
  });

  it('should call getCurrentLanguage when app is created', () => {
    TestBed.createComponent(App);

    expect(languageServiceMock.getCurrentLanguage).toHaveBeenCalled();
  });

  it('should call syncPendingRequests and getCurrentLanguage exactly once', () => {
    TestBed.createComponent(App);

    expect(syncServiceMock.syncPendingRequests).toHaveBeenCalledTimes(1);
    expect(languageServiceMock.getCurrentLanguage).toHaveBeenCalledTimes(1);
  });
});
