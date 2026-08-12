import {
  ComponentFixture,
  TestBed
} from '@angular/core/testing';

import { signal } from '@angular/core';
import {
  Router,
  provideRouter
} from '@angular/router';

import {
  beforeEach,
  describe,
  expect,
  it,
  vi
} from 'vitest';

import { NavbarComponent } from './navbar.component';

import { AuthApiService } from '@app/core/services/api-service/auth-api.service';
import { AuthService } from '@app/core/services/auth.service';
import { StorageService } from '@app/core/services/storage.service';
import { LanguageService } from '@app/core/services/language.service';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let router: Router;

  let authApiServiceMock: {
    logout: ReturnType<typeof vi.fn>;
  };

  let authServiceMock: {
    currentUser: ReturnType<typeof signal>;
    clearCurrentUser: ReturnType<typeof vi.fn>;
  };

  let storageServiceMock: {
    getItem: ReturnType<typeof vi.fn>;
    removeItem: ReturnType<typeof vi.fn>;
  };

  let languageServiceMock: {
    getCurrentLanguage: ReturnType<typeof vi.fn>;
    changeLanguage: ReturnType<typeof vi.fn>;
  };

  const mockUser = {
    userId: '1',
    name: 'Harsh Donda',
    email: 'harsh@test.com',
    role: 'Admin'
  };

  beforeEach(async () => {
    authApiServiceMock = {
      logout: vi.fn()
    };

    authServiceMock = {
      currentUser: signal(null),
      clearCurrentUser: vi.fn()
    };

    storageServiceMock = {
      getItem: vi.fn((key: string) => {
        if (key === 'auth_user') {
          return mockUser;
        }

        return null;
      }),
      removeItem: vi.fn()
    };

    languageServiceMock = {
      getCurrentLanguage: vi.fn(() => 'en'),
      changeLanguage: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        provideRouter([]),
        {
          provide: AuthApiService,
          useValue: authApiServiceMock
        },
        {
          provide: AuthService,
          useValue: authServiceMock
        },
        {
          provide: StorageService,
          useValue: storageServiceMock
        },
        {
          provide: LanguageService,
          useValue: languageServiceMock
        }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);

    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load user information from storage', () => {
    expect(storageServiceMock.getItem)
      .toHaveBeenCalledWith('auth_user');

    expect(component.userName)
      .toBe('Harsh Donda');

    expect(component.email)
      .toBe('harsh@test.com');

    expect(component.role)
      .toBe('Admin');
  });

  it('should initialize selected language', () => {
    expect(languageServiceMock.getCurrentLanguage)
      .toHaveBeenCalled();

    expect(component.selectedLanguage)
      .toBe('en');
  });

  it('should change application language', () => {
    const event = {
      target: {
        value: 'hi-IN'
      }
    } as unknown as Event;

    component.changeLanguage(event);

    expect(component.selectedLanguage)
      .toBe('hi-IN');

    expect(languageServiceMock.changeLanguage)
      .toHaveBeenCalledWith('hi-IN');
  });

  it('should logout and clear storage', () => {
    component.logout();

    expect(authApiServiceMock.logout)
      .toHaveBeenCalled();

    expect(authServiceMock.clearCurrentUser)
      .toHaveBeenCalled();

    expect(storageServiceMock.removeItem)
      .toHaveBeenCalledWith('auth_user');

    expect(storageServiceMock.removeItem)
      .toHaveBeenCalledWith('branches');

    expect(storageServiceMock.removeItem)
      .toHaveBeenCalledWith('departments');

    expect(storageServiceMock.removeItem)
      .toHaveBeenCalledWith('positions');

    expect(storageServiceMock.removeItem)
      .toHaveBeenCalledWith('roles');

    expect(router.navigate)
      .toHaveBeenCalledWith(['/login']);
  });
});
