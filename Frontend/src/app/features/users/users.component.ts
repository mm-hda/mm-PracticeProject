import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { finalize, map } from 'rxjs';

import { getStatusCodeMessage } from '@app/core/config/status-code-messages';

import {
  UserResponse,
  CreateUserRequest,
  paginationRequest,
  userFilterRequest
} from '@app/core/models/userModels/user.model';

import { BranchResponse } from '@app/core/models/branchModels/branch.model';
import { DepartmentResponse } from '@app/core/models/departmentModels/department.model';
import { PositionResponse } from '@app/core/models/positionModels/position.model';
import { roleResponse } from '@app/core/models/roleModels/role.model';

import { UserApiService } from '@app/core/services/api-service/user-api.service';
import { BranchApiService } from '@app/core/services/api-service/branch-api.service';
import { DepartmentApiService } from '@app/core/services/api-service/department-api.service';
import { PositionApiService } from '@app/core/services/api-service/position-api.service';
import { RoleApiService } from '@app/core/services/api-service/role-api.service';

import { StorageService } from '@app/core/services/storage.service';
import { ToastService } from '@app/core/services/toast.service';
import { AuthService } from '@app/core/services/auth.service';
import { OfflineQueueService } from '@app/core/services/offline-queue.service';
import { TranslatePipe } from '@ngx-translate/core';

type UserModalType = | 'add' | 'detail' | null;
type PendingUserResponse = UserResponse & {
  syncStatus?: 'pending' | 'synced';
};

@Component({
  standalone: true,
  selector: 'app-users',
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './users.component.html',
})
export class UsersComponent implements OnInit {

  private readonly userApiService = inject(UserApiService);
  private readonly roleApiService = inject(RoleApiService);
  private readonly branchApiService = inject(BranchApiService);
  private readonly departmentApiService = inject(DepartmentApiService);
  private readonly positionApiService = inject(PositionApiService);
  private readonly authService = inject(AuthService);
  private readonly offlineQueueService = inject(OfflineQueueService);

  private readonly storageService = inject(StorageService);
  private readonly toastService = inject(ToastService);

  public readonly users = signal<PendingUserResponse[]>([]);

  public readonly currentUser = this.authService.currentUser;

  public readonly roles = signal<roleResponse[]>([]);
  public readonly branches = signal<BranchResponse[]>([]);
  public readonly departments = signal<DepartmentResponse[]>([]);
  public readonly positions = signal<PositionResponse[]>([]);
  private readonly pageCache = signal<Map<number, UserResponse[]>>(new Map());
  public readonly selectedUser = signal<UserResponse | null>(null);
  public readonly DepartmentPositions = signal<PositionResponse[]>([]);
  public readonly draftUser = signal<CreateUserRequest | null>(null);


  public readonly isPageLoading = signal(false);
  public readonly isModalLoading = signal(false);
  public readonly isSubmitting = signal(false);

  public readonly currentPage = signal(1);
  public readonly pageSize = signal(5);

  public readonly totalPages = signal(0);
  public readonly totalRecords = signal(0);

  private readonly activeModal = signal<UserModalType>(null);

  public readonly isAddModalOpen = computed(() => this.activeModal() === 'add');

  public readonly isDetailModalOpen = computed(() => this.activeModal() === 'detail');

  public readonly filterForm = new FormGroup({
    searchTerm: new FormControl<string>(''),
    roleId: new FormControl<string>(''),
    branchId: new FormControl<string>(''),
    departmentId: new FormControl<string>(''),
    positionId: new FormControl<string>('')
  });

  public readonly userForm = new FormGroup({
    firstName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    lastName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(6)] }),
    dob: new FormControl<string | null>(null),
    branchId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    departmentId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    positionId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    roleId: new FormControl('', { nonNullable: true, validators: [Validators.required] })
  });

  public ngOnInit(): void {
    this.loadLookupData();
    this.loadUsers();
  }

  public loadUsers(): void {

    const page = this.currentPage();

    const cachedPage = this.pageCache().get(page);

    if (cachedPage) { this.users.set(cachedPage); return; }

    this.isPageLoading.set(true);

    const request: paginationRequest = {
      pageNumber: page,
      pageSize: this.pageSize()
    };

    this.userApiService
      .getAllUsers(request)
      .pipe(finalize(() => this.isPageLoading.set(false)))
      .subscribe({
        next: async response => {
          const users = response.data ?? [];
          const pendingRequests = await this.offlineQueueService.getPendingUserRequests();

          const pendingUsers = pendingRequests.map(request => {
            const payload = request.payload as CreateUserRequest;

            return {
              userId: `pending-${request.id}`,
              name: `${payload.firstName} ${payload.lastName}`,
              email: payload.email,
              syncStatus: 'pending'
            } as PendingUserResponse;
          });

          const mergedUsers = [
            ...pendingUsers,
            ...users
          ];

          this.users.set(mergedUsers);

          this.pageCache.update(cache => {
            const newCache = new Map(cache);
            newCache.set(page, mergedUsers);
            return newCache;
          });
          this.totalPages.set(response.meta?.totalPages ?? 0);
          this.totalRecords.set(response.meta?.totalRecords ?? 0);
        },
        error: error => { this.users.set([]); this.toastService.show(getStatusCodeMessage(error.statusCode)); }
      });
  }

  public loadLookupData(): void {

    const cachedRoles = this.storageService.getItem<roleResponse[]>('roles');

    if (cachedRoles?.length) {
      this.roles.set(cachedRoles);
    } else {
      this.roleApiService.getAllRoles().subscribe({
        next: response => {
          this.roles.set(response.data ?? []);
          this.storageService.setItem('roles', response.data ?? []);
        }
      });
    }

    const cachedBranches = this.storageService.getItem<BranchResponse[]>('branches');

    if (cachedBranches?.length) {
      this.branches.set(cachedBranches);
    } else {
      this.branchApiService.getAllBranches().subscribe({
        next: response => {
          this.branches.set(response.data ?? []);
          this.storageService.setItem('branches', response.data ?? []);
        }
      });
    }

    const cachedDepartments = this.storageService.getItem<DepartmentResponse[]>('departments');

    if (cachedDepartments?.length) {
      this.departments.set(cachedDepartments);
    } else {
      this.departmentApiService.getAllDepartments().subscribe({
        next: response => {
          this.departments.set(response.data ?? []);
          this.storageService.setItem('departments', response.data ?? []);
        }
      });
    }

    const cachedPositions = this.storageService.getItem<PositionResponse[]>('positions');

    if (cachedPositions?.length) {
      this.positions.set(cachedPositions);
    } else {
      this.positionApiService.getAllPositions()
        .subscribe({
          next: response => {
            this.positions.set(response.data ?? []);
            this.storageService.setItem('positions', response.data ?? []);
          }
        });
    }
  }

  public filterPositions(): void {
    const selectedDepartmentId = this.userForm.controls.departmentId.value;

    this.DepartmentPositions.set(this.positions().filter(position => position.departmentId === selectedDepartmentId));
  };

  public searchUsers(): void {
    const searchTerm = this.filterForm.controls.searchTerm.value?.trim();

    if (!searchTerm) {
      this.loadUsers();
      return;
    }

    this.isPageLoading.set(true);

    this.userApiService
      .getUserBySearch({ searchTerm })
      .pipe(finalize(() => this.isPageLoading.set(false)))
      .subscribe({
        next: response => { this.users.set(response.data ?? []); },

        error: error => {
          this.users.set([]);
          this.toastService.show(getStatusCodeMessage(error.statusCode));
        }
      });
  }

  public applyFilters(): void {

    const request: userFilterRequest = {
      roleId: this.filterForm.controls.roleId.value || undefined,

      branchId: this.filterForm.controls.branchId.value || undefined,

      departmentId: this.filterForm.controls.departmentId.value || undefined,

      positionId: this.filterForm.controls.positionId.value || undefined
    };

    this.isPageLoading.set(true);

    this.userApiService
      .getUsersByFilter(request)
      .pipe(finalize(() => this.isPageLoading.set(false)))
      .subscribe({
        next: response => { this.users.set(response.data ?? []); },

        error: error => {
          this.users.set([]);
          this.toastService.show(getStatusCodeMessage(error.statusCode));
        }
      });
  }

  public resetFilters(): void {

    this.filterForm.reset({
      searchTerm: '',
      roleId: '',
      branchId: '',
      departmentId: '',
      positionId: ''
    });

    this.currentPage.set(1);

    this.loadUsers();
  }

  public nextPage(): void {
    if (this.currentPage() >= this.totalPages()) {
      return;
    }
    this.currentPage.update(page => page + 1);
    this.loadUsers();
  }

  public previousPage(): void {
    if (this.currentPage() <= 1) {
      return;
    }
    this.currentPage.update(page => page - 1);
    this.loadUsers();
  }

  public openAddModal(): void {

    const draft = this.draftUser();

    if (draft) {
      this.userForm.patchValue(draft);
    } else {
      this.userForm.reset();
    }

    this.activeModal.set('add');
  }

  public openDetailModal(user: UserResponse): void {
    this.selectedUser.set(user);
    this.activeModal.set('detail');
  }

  public closeModals(): void {
    this.draftUser.set(
      this.userForm.getRawValue() as CreateUserRequest
    );

    this.activeModal.set(null);
    this.selectedUser.set(null);
    this.userForm.reset();
  }

  public async createUser(): Promise<void> {

    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    const request: CreateUserRequest = {
      firstName: this.userForm.controls.firstName.value.trim(),
      lastName: this.userForm.controls.lastName.value.trim(),
      email: this.userForm.controls.email.value.trim(),
      password: this.userForm.controls.password.value,
      dob: this.userForm.controls.dob.value,
      branchId: this.userForm.controls.branchId.value,
      departmentId: this.userForm.controls.departmentId.value,
      positionId: this.userForm.controls.positionId.value,
      roleId: this.userForm.controls.roleId.value
    };

    if (!navigator.onLine) {
      await this.offlineQueueService.addRequest(
        'create-user',
        request
      );

      this.users.update(users => [
        {
          userId: crypto.randomUUID(),
          name: `${request.firstName} ${request.lastName}`,
          email: request.email,
          syncStatus: 'pending'
        } as PendingUserResponse,
        ...users
      ]);

      this.toastService.show('Internet unavailable. User queued for sync.');

      this.closeModals();

      return;
    }

    this.isSubmitting.set(true);

    this.userApiService
      .createUser(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: response => {
          this.draftUser.set(null);
          this.toastService.show(getStatusCodeMessage(response.statusCode));
          this.closeModals();
          this.loadUsers();
        },

        error: error => {
          this.toastService.show(getStatusCodeMessage(error.statusCode));
        }
      });

    this.storageService.removeItem('departments');
    this.storageService.removeItem('positions');
    this.storageService.removeItem('branches');
    this.storageService.removeItem('roles');

    this.loadLookupData();
  }
}
