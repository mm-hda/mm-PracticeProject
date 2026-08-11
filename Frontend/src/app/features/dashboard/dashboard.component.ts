import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { BranchApiService } from '@app/core/services/api-service/branch-api.service';
import { DepartmentApiService } from '@app/core/services/api-service/department-api.service';
import { PositionApiService } from '@app/core/services/api-service/position-api.service';
import { RoleApiService } from '@app/core/services/api-service/role-api.service';
import { StorageService } from '@app/core/services/storage.service';
import { AuthService } from '@app/core/services/auth.service';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  standalone: true,
  selector: 'app-dashboard',
  imports: [TranslatePipe],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent {
  public readonly isPageLoading = signal(false);
  public readonly branchService = inject(BranchApiService);
  public readonly departmentService = inject(DepartmentApiService);
  public readonly positionService = inject(PositionApiService);
  public readonly rolesService = inject(RoleApiService);
  public readonly storeService = inject(StorageService);
  public readonly AuthService = inject(AuthService);

  public readonly currentUser = this.AuthService.currentUser;

  public ngOnInit(): void {
    this.loadLocalData();
  }

  public loadLocalData(): void {
    if (this.currentUser()?.role === 'Employee') {
      return;
    }
    if (this.currentUser()?.role === 'Manager') {
      return;
    }
    this.storeService.removeItem('branches');
    this.storeService.removeItem('departments');
    this.storeService.removeItem('positions');
    this.storeService.removeItem('roles');

    this.branchService.getAllBranches().subscribe({
      next: (response) => {
        this.storeService.setItem('branches', response.data);
      },
    });

    this.departmentService.getAllDepartments().subscribe({
      next: (response) => {
        this.storeService.setItem('departments', response.data);
      },
    });

    this.positionService.getAllPositions().subscribe({
      next: (response) => {
        this.storeService.setItem('positions', response.data);
      },
    });

    this.rolesService.getAllRoles().subscribe({
      next: (response) => {
        this.storeService.setItem('roles', response.data);
      },
    });
  }
}
