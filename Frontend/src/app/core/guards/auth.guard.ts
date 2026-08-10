import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {

  const authService = inject(AuthService);
  const router = inject(Router);

  console.log('authGuard: isAuthenticated', authService.isAuthenticated());

  if (authService.isAuthenticated()) {
    console.log('authGuard: user is authenticated');
    return true;
  }

  console.log('authGuard: user is not authenticated');
  return router.createUrlTree(['/login']);
};
