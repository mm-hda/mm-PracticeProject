import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Pipe, PipeTransform, Signal, signal } from '@angular/core';
import { Router, provideRouter } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { of, Subject } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { NavbarComponent } from './navbar.component';
import { AuthApiService } from '@app/core/services/api-service/auth-api.service';
import { AuthService } from '@app/core/services/auth.service';
import { StorageService } from '@app/core/services/storage.service';
import { LanguageService } from '@app/core/services/language.service';

@Pipe({ name: 'translate', standalone: true })
class MockTranslatePipe implements PipeTransform {
  public transform(value: string): string {
    return value;
  }
}

interface MockUser {
  userId: string;
  name: string;
  email: string;
  role: string;
}

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let router: Router;

  let authApiServiceMock: {
    logout: ReturnType<typeof vi.fn>;
  };

  let authServiceMock: {
    currentUser: Signal<MockUser | null>;
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

  const mockUser: MockUser = {
    userId: '1',
    name: 'Harsh Donda',
    email: 'harsh@test.com',
    role: 'Admin'
  };

  const createComponent = (): void => {
    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  };

  beforeEach(async () => {
    authApiServiceMock = {
      logout: vi.fn()
    };

    authServiceMock = {
      currentUser: signal(mockUser),
      clearCurrentUser: vi.fn()
    };

    storageServiceMock = {
      getItem: vi.fn().mockReturnValue(mockUser),
      removeItem: vi.fn()
    };

    languageServiceMock = {
      getCurrentLanguage: vi.fn(() => 'en-US'),
      changeLanguage: vi.fn()
    };

    const translateServiceMock = {
      currentLang: signal('en-US'),
      defaultLang: signal('en-US'),
      onLangChange: new Subject().asObservable(),
      onTranslationChange: new Subject().asObservable(),
      onDefaultLangChange: new Subject().asObservable(),
      get: vi.fn((key: string | string[]) => of(key)),
      instant: vi.fn((key: string | string[]) => key),
      stream: vi.fn((key: string | string[]) => of(key)),
      getStreamOnTranslationChange: vi.fn((key: string | string[]) => of(key)),
      use: vi.fn(() => of({})),
      setDefaultLang: vi.fn()
    } as unknown as TranslateService;

    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        provideRouter([]),
        { provide: AuthApiService, useValue: authApiServiceMock },
        { provide: AuthService, useValue: authServiceMock },
        { provide: StorageService, useValue: storageServiceMock },
        { provide: LanguageService, useValue: languageServiceMock },
        { provide: TranslateService, useValue: translateServiceMock }
      ]
    })
      .overrideComponent(NavbarComponent, {
        remove: { imports: [TranslatePipe] },
        add: { imports: [MockTranslatePipe] }
      })
      .compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  describe('Creation', () => {
    beforeEach(() => createComponent());

    it('should create', () => {
      expect(component).toBeTruthy();
    });
  });

  describe('User information', () => {
    beforeEach(() => createComponent());

    it('should load user information from storage', () => {
      expect(storageServiceMock.getItem).toHaveBeenCalledWith('auth_user');
      expect(component.userName).toBe('Harsh Donda');
      expect(component.email).toBe('harsh@test.com');
      expect(component.role).toBe('Admin');
    });

    it('should display username', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Harsh Donda');
    });

    it('should display email', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('harsh@test.com');
    });

    it('should handle missing user information', () => {
      storageServiceMock.getItem.mockReturnValue(null);
      createComponent();
      expect(component.userName).toBe('');
      expect(component.email).toBe('');
      expect(component.role).toBe('');
    });
  });

  describe('Language', () => {
    beforeEach(() => createComponent());

    it('should initialize selected language', () => {
      expect(languageServiceMock.getCurrentLanguage).toHaveBeenCalled();
      expect(component.selectedLanguage).toBe('en-US');
    });

    it('should change application language to Hindi', () => {
      const event = { target: { value: 'hi-IN' } } as unknown as Event;
      component.changeLanguage(event);
      expect(component.selectedLanguage).toBe('hi-IN');
      expect(languageServiceMock.changeLanguage).toHaveBeenCalledWith('hi-IN');
    });

    it('should change application language to Gujarati', () => {
      const event = { target: { value: 'gu-IN' } } as unknown as Event;
      component.changeLanguage(event);
      expect(component.selectedLanguage).toBe('gu-IN');
      expect(languageServiceMock.changeLanguage).toHaveBeenCalledWith('gu-IN');
    });
  });

  describe('Logout', () => {
    beforeEach(() => createComponent());

    it('should logout, clear user, clear storage and navigate to login', () => {
      component.logout();
      expect(authApiServiceMock.logout).toHaveBeenCalled();
      expect(authServiceMock.clearCurrentUser).toHaveBeenCalled();
      expect(storageServiceMock.removeItem).toHaveBeenCalledWith('auth_user');
      expect(storageServiceMock.removeItem).toHaveBeenCalledWith('branches');
      expect(storageServiceMock.removeItem).toHaveBeenCalledWith('departments');
      expect(storageServiceMock.removeItem).toHaveBeenCalledWith('positions');
      expect(storageServiceMock.removeItem).toHaveBeenCalledWith('roles');
      expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });
  });

  describe('Role-based navigation', () => {
    it('should display all navigation items for Admin', () => {
      const adminUser = { ...mockUser, role: 'Admin' };
      authServiceMock.currentUser = signal(adminUser);
      storageServiceMock.getItem.mockReturnValue(adminUser);
      createComponent();

      const compiled = fixture.nativeElement as HTMLElement;

      expect(compiled.textContent).toContain('NAVBAR.BRANCHES');
      expect(compiled.textContent).toContain('NAVBAR.DEPARTMENTS');
      expect(compiled.textContent).toContain('NAVBAR.POSITIONS');
      expect(compiled.textContent).toContain('NAVBAR.USERS');
      expect(compiled.textContent).toContain('NAVBAR.PROJECTS');
    });

    it('should display only users navigation for HR', () => {
      const hrUser = { ...mockUser, role: 'HR' };
      authServiceMock.currentUser = signal(hrUser);
      storageServiceMock.getItem.mockReturnValue(hrUser);
      createComponent();

      const compiled = fixture.nativeElement as HTMLElement;

      expect(compiled.textContent).toContain('NAVBAR.USERS');
      expect(compiled.textContent).not.toContain('NAVBAR.BRANCHES');
      expect(compiled.textContent).not.toContain('NAVBAR.DEPARTMENTS');
      expect(compiled.textContent).not.toContain('NAVBAR.POSITIONS');
      expect(compiled.textContent).not.toContain('NAVBAR.PROJECTS');
    });

    it('should display only projects navigation for Manager', () => {
      const managerUser = { ...mockUser, role: 'Manager' };
      authServiceMock.currentUser = signal(managerUser);
      storageServiceMock.getItem.mockReturnValue(managerUser);
      createComponent();

      const compiled = fixture.nativeElement as HTMLElement;

      expect(compiled.textContent).toContain('NAVBAR.PROJECTS');
      expect(compiled.textContent).not.toContain('NAVBAR.USERS');
      expect(compiled.textContent).not.toContain('NAVBAR.BRANCHES');
      expect(compiled.textContent).not.toContain('NAVBAR.DEPARTMENTS');
      expect(compiled.textContent).not.toContain('NAVBAR.POSITIONS');
    });

    it('should display only projects navigation for Employee', () => {
      const employeeUser = { ...mockUser, role: 'Employee' };
      authServiceMock.currentUser = signal(employeeUser);
      storageServiceMock.getItem.mockReturnValue(employeeUser);
      createComponent();

      const compiled = fixture.nativeElement as HTMLElement;

      expect(compiled.textContent).toContain('NAVBAR.PROJECTS');
      expect(compiled.textContent).not.toContain('NAVBAR.USERS');
      expect(compiled.textContent).not.toContain('NAVBAR.BRANCHES');
      expect(compiled.textContent).not.toContain('NAVBAR.DEPARTMENTS');
      expect(compiled.textContent).not.toContain('NAVBAR.POSITIONS');
    });

    it('should not display role-specific navigation for unknown role', () => {
      const unknownUser = { ...mockUser, role: 'Unknown' };
      authServiceMock.currentUser = signal(unknownUser);
      storageServiceMock.getItem.mockReturnValue(unknownUser);
      createComponent();

      const compiled = fixture.nativeElement as HTMLElement;

      expect(compiled.textContent).not.toContain('NAVBAR.BRANCHES');
      expect(compiled.textContent).not.toContain('NAVBAR.DEPARTMENTS');
      expect(compiled.textContent).not.toContain('NAVBAR.POSITIONS');
      expect(compiled.textContent).not.toContain('NAVBAR.USERS');
      expect(compiled.textContent).not.toContain('NAVBAR.PROJECTS');
    });
  });

  describe('User interactions', () => {
    beforeEach(() => createComponent());

    it('should change language when select value changes', () => {
      const select = fixture.nativeElement.querySelector('select') as HTMLSelectElement;
      select.value = 'hi-IN';
      select.dispatchEvent(new Event('change'));
      fixture.detectChanges();

      expect(languageServiceMock.changeLanguage).toHaveBeenCalledWith('hi-IN');
      expect(component.selectedLanguage).toBe('hi-IN');
    });

    it('should logout when logout button is clicked', () => {
      const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
      button.click();

      expect(authApiServiceMock.logout).toHaveBeenCalled();
      expect(authServiceMock.clearCurrentUser).toHaveBeenCalled();
      expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });
  });
});
