import { CommonModule } from '@angular/common';
import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthApiService } from '@app/core/services/api-service/auth-api.service';
import { ToastService } from '@app/core/services/toast.service';
import { getStatusCodeMessage } from '@app/core/config/status-code-messages';
import { LoginRequest } from '@app/core/models/authModels/login-request.model';
import { AuthService } from '@app/core/services/auth.service';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './login.component.html',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly toastService = inject(ToastService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly isSubmitting = signal(false);
  protected readonly hidePassword = signal(true);

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  protected submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const payload: LoginRequest = this.form.getRawValue();
    this.isSubmitting.set(true);

    this.authApi
      .login(payload)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.toastService.show(getStatusCodeMessage(response.statusCode));

          if (response.statusCode !== 713) {
            return;
          }

          if (!response.data) {
            return;
          }

          this.authService.setCurrentUser(response.data);

          this.router.navigateByUrl('/dashboard');
        },
        error: (error: HttpErrorResponse) => {
          const statusCode = error.error?.statusCode ?? error.status;

          this.toastService.show(getStatusCodeMessage(statusCode));
        },
      });
  }

  protected togglePassword(): void {
    this.hidePassword.update((value) => !value);
  }

  protected controlInvalid(name: 'email' | 'password'): boolean {
    const control = this.form.controls[name];
    return control.invalid && (control.dirty || control.touched);
  }
}
