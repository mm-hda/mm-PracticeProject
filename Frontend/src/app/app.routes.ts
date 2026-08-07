import { Routes } from '@angular/router';

import { LoginComponent } from '@app/features/auth/login/login.component';
import { LayoutComponent } from '@app/shared/layout/layout.component';

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
        loadComponent: () => import('@app/features/dashboard/dashboard.component').then((m) => m.DashboardComponent)
      },
      {
        path: 'branches',
        loadComponent: () => import('@app/features/branch/branch.component').then((m) => m.BranchComponent)
      },
      {
        path: 'departments',
        loadComponent: () => import('@app/features/department/department.component').then((m) => m.DepartmentComponent)
      },
      {
        path: 'positions',
        loadComponent: () => import('@app/features/position/position.component').then((m) => m.PositionComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('@app/features/users/users.component').then((m) => m.UsersComponent)
      },
      {
        path: 'projects',
        loadComponent: () => import('@app/features/project/project.component').then((m) => m.ProjectComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
