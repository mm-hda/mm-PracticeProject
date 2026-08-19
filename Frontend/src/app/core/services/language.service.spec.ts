import { TestBed } from '@angular/core/testing';
import { describe, beforeEach, afterEach, expect, it, vi } from 'vitest';
import { TranslateService } from '@ngx-translate/core';

import { LanguageService } from './language.service';

describe('LanguageService', () => {
  let service: LanguageService;

  const translateService = {
    use: vi.fn()
  };

  beforeEach(() => {
    localStorage.clear();
    vi.clearAllMocks();

    TestBed.configureTestingModule({
      providers: [
        LanguageService,
        {
          provide: TranslateService,
          useValue: translateService
        }
      ]
    });

    service = TestBed.inject(LanguageService);
  });

  afterEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });

  describe('Creation', () => {
    it('should create the service', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('Constructor', () => {
    it('should use saved language when language exists in localStorage', () => {
      localStorage.setItem('app-language', 'hi-IN');

      TestBed.resetTestingModule();

      TestBed.configureTestingModule({
        providers: [
          LanguageService,
          {
            provide: TranslateService,
            useValue: translateService
          }
        ]
      });

      service = TestBed.inject(LanguageService);

      expect(translateService.use).toHaveBeenCalledWith('hi-IN');
    });

    it('should use Gujarati when browser language starts with gu', () => {
      vi.spyOn(navigator, 'language', 'get').mockReturnValue('gu-IN');

      TestBed.resetTestingModule();

      TestBed.configureTestingModule({
        providers: [
          LanguageService,
          {
            provide: TranslateService,
            useValue: translateService
          }
        ]
      });

      service = TestBed.inject(LanguageService);

      expect(translateService.use).toHaveBeenCalledWith('gu-IN');
    });

    it('should use Hindi when browser language starts with hi', () => {
      vi.spyOn(navigator, 'language', 'get').mockReturnValue('hi-IN');

      TestBed.resetTestingModule();

      TestBed.configureTestingModule({
        providers: [
          LanguageService,
          {
            provide: TranslateService,
            useValue: translateService
          }
        ]
      });

      service = TestBed.inject(LanguageService);

      expect(translateService.use).toHaveBeenCalledWith('hi-IN');
    });

    it('should use English when browser language is neither Gujarati nor Hindi', () => {
      vi.spyOn(navigator, 'language', 'get').mockReturnValue('en-US');

      TestBed.resetTestingModule();

      TestBed.configureTestingModule({
        providers: [
          LanguageService,
          {
            provide: TranslateService,
            useValue: translateService
          }
        ]
      });

      service = TestBed.inject(LanguageService);

      expect(translateService.use).toHaveBeenCalledWith('en-US');
    });
  });

  describe('changeLanguage', () => {
    it('should save the selected language and use it', () => {
      service.changeLanguage('gu-IN');

      expect(localStorage.getItem('app-language')).toBe('gu-IN');
      expect(translateService.use).toHaveBeenCalledWith('gu-IN');
    });

    it('should change language to Hindi', () => {
      service.changeLanguage('hi-IN');

      expect(localStorage.getItem('app-language')).toBe('hi-IN');
      expect(translateService.use).toHaveBeenCalledWith('hi-IN');
    });

    it('should change language to English', () => {
      service.changeLanguage('en-US');

      expect(localStorage.getItem('app-language')).toBe('en-US');
      expect(translateService.use).toHaveBeenCalledWith('en-US');
    });
  });

  describe('getCurrentLanguage', () => {
    it('should return Gujarati when saved language starts with gu', () => {
      localStorage.setItem('app-language', 'gu-IN');

      expect(service.getCurrentLanguage()).toBe('gu-IN');
    });

    it('should return Hindi when saved language starts with hi', () => {
      localStorage.setItem('app-language', 'hi-IN');

      expect(service.getCurrentLanguage()).toBe('hi-IN');
    });

    it('should return English when saved language is neither Gujarati nor Hindi', () => {
      localStorage.setItem('app-language', 'en-US');

      expect(service.getCurrentLanguage()).toBe('en-US');
    });

    it('should use browser language when no saved language exists', () => {
      localStorage.removeItem('app-language');

      vi.spyOn(navigator, 'language', 'get').mockReturnValue('gu-IN');

      expect(service.getCurrentLanguage()).toBe('gu-IN');
    });

    it('should return Hindi when browser language is Hindi and no saved language exists', () => {
      localStorage.removeItem('app-language');

      vi.spyOn(navigator, 'language', 'get').mockReturnValue('hi-IN');

      expect(service.getCurrentLanguage()).toBe('hi-IN');
    });

    it('should return English when browser language is neither Gujarati nor Hindi and no saved language exists', () => {
      localStorage.removeItem('app-language');

      vi.spyOn(navigator, 'language', 'get').mockReturnValue('en-US');

      expect(service.getCurrentLanguage()).toBe('en-US');
    });
  });
});
