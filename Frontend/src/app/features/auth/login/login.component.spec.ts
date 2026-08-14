import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Pipe, PipeTransform } from '@angular/core';
import { provideRouter, Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { of, Subject, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { LoginComponent } from './login.component';
import { AuthApiService } from '@app/core/services/api-service/auth-api.service';
import { ToastService } from '@app/core/services/toast.service';
import { AuthService } from '@app/core/services/auth.service';
import { getStatusCodeMessage } from '@app/core/config/status-code-messages';

@Pipe({ name: 'translate', standalone: true })
class MockTranslatePipe implements PipeTransform {
  public transform(value: string): string {
    return value;
  }
}

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let router: Router;

  let authApiServiceMock: {
    login: ReturnType<typeof vi.fn>;
  };

  let toastServiceMock: {
    show: ReturnType<typeof vi.fn>;
  };

  let authServiceMock: {
    setCurrentUser: ReturnType<typeof vi.fn>;
  };

  const mockUser = {
    userId: '1',
    name: 'Harsh Donda',
    email: 'harsh@test.com',
    role: 'Admin'
  };

  beforeEach(async () => {
    authApiServiceMock = { login: vi.fn() };
    toastServiceMock = { show: vi.fn() };
    authServiceMock = { setCurrentUser: vi.fn() };

    const translateServiceMock = {
      currentLang: 'en-US',
      defaultLang: 'en-US',
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
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        { provide: AuthApiService, useValue: authApiServiceMock },
        { provide: ToastService, useValue: toastServiceMock },
        { provide: AuthService, useValue: authServiceMock },
        { provide: TranslateService, useValue: translateServiceMock }
      ]
    }).overrideComponent(LoginComponent, {
      remove: { imports: [TranslatePipe] },
      add: { imports: [MockTranslatePipe] }
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('Form validation', () => {
    it('should reject empty form', () => {
      component['submit']();

      expect(component['form'].invalid).toBe(true);
      expect(authApiServiceMock.login).not.toHaveBeenCalled();
    });

    it('should reject invalid email', () => {
      component['form'].setValue({
        email: 'invalid-email',
        password: 'Password@123'
      });

      component['submit']();

      expect(component['form'].controls.email.invalid).toBe(true);
      expect(authApiServiceMock.login).not.toHaveBeenCalled();
    });

    it('should reject empty email', () => {
      component['form'].setValue({
        email: '',
        password: 'Password@123'
      });

      component['submit']();

      expect(component['form'].controls.email.invalid).toBe(true);
      expect(authApiServiceMock.login).not.toHaveBeenCalled();
    });

    it('should reject empty password', () => {
      component['form'].setValue({
        email: 'harsh@test.com',
        password: ''
      });

      component['submit']();

      expect(component['form'].controls.password.invalid).toBe(true);
      expect(authApiServiceMock.login).not.toHaveBeenCalled();
    });

    it('should reject password shorter than 6 characters', () => {
      component['form'].setValue({
        email: 'harsh@test.com',
        password: 'Pass1'
      });

      component['submit']();

      expect(component['form'].controls.password.invalid).toBe(true);
      expect(authApiServiceMock.login).not.toHaveBeenCalled();
    });

    it('should reject password longer than 20 characters', () => {
      component['form'].setValue({
        email: 'harsh@test.com',
        password: 'Password1234567890123'
      });

      component['submit']();

      expect(component['form'].controls.password.invalid).toBe(true);
      expect(authApiServiceMock.login).not.toHaveBeenCalled();
    });

    it('should accept valid email and password', () => {
      component['form'].setValue({
        email: 'harsh@test.com',
        password: 'Password@123'
      });

      expect(component['form'].valid).toBe(true);
    });
  });

  describe('Unit tests', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should submit valid login credentials', () => {
      component['form'].setValue({
        email: 'harsh@gmail.com',
        password: 'Password@123'
      });

      authApiServiceMock.login.mockReturnValue(of({
        statusCode: 713,
        data: mockUser
      }));

      component['submit']();

      expect(authApiServiceMock.login).toHaveBeenCalledWith({
        email: 'harsh@gmail.com',
        password: 'Password@123'
      });
    });

    it('should handle successful login', () => {
      component['form'].setValue({
        email: 'harsh@test.com',
        password: 'Password@123'
      });

      authApiServiceMock.login.mockReturnValue(of({
        statusCode: 713,
        data: mockUser
      }));

      component['submit']();
      var expectedMessage = getStatusCodeMessage(713);
      expect(toastServiceMock.show).toHaveBeenCalledWith(expectedMessage);
      expect(authServiceMock.setCurrentUser).toHaveBeenCalledWith(mockUser);
      expect(router.navigateByUrl).toHaveBeenCalledWith('/dashboard');
    });

    it('should handle failed login', () => {
      component['form'].setValue({
        email: 'harsh@test.com',
        password: 'Password@123'
      });

      authApiServiceMock.login.mockReturnValue(
        throwError(() => ({
          error: { statusCode: 401 },
          status: 401
        }))
      );

      component['submit']();
      var expectedMessage = getStatusCodeMessage(401);
      expect(toastServiceMock.show).toHaveBeenCalledWith(expectedMessage);
      expect(authServiceMock.setCurrentUser).not.toHaveBeenCalled();
      expect(router.navigateByUrl).not.toHaveBeenCalled();
    });

    it('should toggle password visibility', () => {
      expect(component['hidePassword']()).toBe(true);

      component['togglePassword']();

      expect(component['hidePassword']()).toBe(false);
    });
  });

  describe('Integration tests', () => {
    it('should display validation message for invalid email', () => {
      const emailInput = fixture.nativeElement.querySelector('#email') as HTMLInputElement;

      emailInput.dispatchEvent(new Event('blur'));
      fixture.detectChanges();

      expect(fixture.nativeElement.textContent).toContain('AUTH.EMAIL_VALIDATION');
    });

    it('should submit form through template', () => {
      component['form'].setValue({
        email: 'harsh@test.com',
        password: 'Password@123'
      });

      authApiServiceMock.login.mockReturnValue(of({
        statusCode: 713,
        data: mockUser
      }));

      const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;

      form.dispatchEvent(new Event('submit'));
      fixture.detectChanges();

      expect(authApiServiceMock.login).toHaveBeenCalledWith({
        email: 'harsh@test.com',
        password: 'Password@123'
      });
    });

    it('should navigate to dashboard after successful login', () => {
      component['form'].setValue({
        email: 'harsh@test.com',
        password: 'Password@123'
      });

      authApiServiceMock.login.mockReturnValue(of({
        statusCode: 713,
        data: mockUser
      }));

      const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;

      form.dispatchEvent(new Event('submit'));
      fixture.detectChanges();

      expect(router.navigateByUrl).toHaveBeenCalledWith('/dashboard');
    });
  });
});
