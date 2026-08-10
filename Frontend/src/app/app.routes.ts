import { Routes } from '@angular/router';

import { LoginComponent } from '@app/features/auth/login/login.component';
import { LayoutComponent } from '@app/shared/layout/layout.component';
import { authGuard } from '@app/core/guards/auth.guard';
import { roleGuard } from '@app/core/guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login'
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('@app/features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
        canActivate: [authGuard]
      },
      {
        path: 'branches',
        loadComponent: () => import('@app/features/branch/branch.component').then((m) => m.BranchComponent),
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Admin'] }
      },
      {
        path: 'departments',
        loadComponent: () => import('@app/features/department/department.component').then((m) => m.DepartmentComponent),
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Admin'] }
      },
      {
        path: 'positions',
        loadComponent: () => import('@app/features/position/position.component').then((m) => m.PositionComponent),
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Admin'] }
      },
      {
        path: 'users',
        loadComponent: () => import('@app/features/users/users.component').then((m) => m.UsersComponent),
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Admin', "HR"] }
      },
      {
        path: 'projects',
        loadComponent: () => import('@app/features/project/project.component').then((m) => m.ProjectComponent),
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Admin', "Manager", "Employee"] }
      }
    ]
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('@app/shared/components/unauthorized/unauthorized.component').then((m) => m.UnauthorizedComponent)
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
