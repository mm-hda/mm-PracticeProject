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
import { finalize } from 'rxjs';

import { getStatusCodeMessage } from '@app/core/config/status-code-messages';
import {
  DepartmentResponse,
  DepartmentUserResponse,
  CreateDepartmentRequest,
  UpdateDepartmentRequest
} from '@app/core/models/departmentModels/department.model';
import { DepartmentApiService } from '@app/core/services/api-service/department-api.service';
import { ToastService } from '@app/core/services/toast.service';
import {
  TableColumn,
  GenericTableComponent
} from '@app/shared/components/table/generic-table.component';
import { PositionApiService } from '@app/core/services/api-service/position-api.service';
import { PositionResponse } from '@app/core/models/positionModels/position.model';
import { StorageService } from '@app/core/services/storage.service';

type DepartmentModalType = | 'add' | 'edit' | 'detail' | 'employees' | null;

@Component({
  standalone: true,
  selector: 'app-department',
  imports: [CommonModule, ReactiveFormsModule, GenericTableComponent],
  templateUrl: './department.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})

export class DepartmentComponent implements OnInit {
  private readonly departmentApiService = inject(DepartmentApiService);

  private readonly toastService = inject(ToastService);
  private readonly positionApiService = inject(PositionApiService);
  private readonly storageService = inject(StorageService);

  public readonly departments = signal<DepartmentResponse[]>([]);

  public readonly departmentEmployees = signal<DepartmentUserResponse[]>([]);
  public readonly departmentPositions = signal<PositionResponse[]>([]);

  public readonly selectedDepartment = signal<DepartmentResponse | null>(null);

  public readonly isPageLoading = signal(false);

  public readonly isModalLoading = signal(false);

  public readonly isSubmitting = signal(false);

  private readonly activeModal = signal<DepartmentModalType>(null);

  public readonly userColumns: TableColumn[] = [
    { key: 'name', title: 'Name' },
    { key: 'email', title: 'Email' },
    { key: 'branchName', title: 'Branch' },
    { key: 'positionName', title: 'Position' },
    { key: 'roleName', title: 'Role' }
  ];

  public readonly positionColumns: TableColumn[] = [
    { key: 'name', title: 'Position Name' },
    { key: 'totalUsers', title: 'Total Users' }
  ];

  public readonly isAddModalOpen = computed(() => this.activeModal() === 'add');

  public readonly isEditModalOpen = computed(() => this.activeModal() === 'edit');

  public readonly isDetailModalOpen = computed(() => this.activeModal() === 'detail');

  public readonly isEmployeesModalOpen = computed(() => this.activeModal() === 'employees');

  public readonly departmentForm = new FormGroup({
    id: new FormControl<string>('', { nonNullable: true }),

    name: new FormControl<string>('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(100)
      ]
    })
  });

  public ngOnInit(): void {
    this.loadDepartments();
  }

  public loadDepartments(): void {
    this.isPageLoading.set(true);

    const cachedDepartments = this.storageService.getItem<DepartmentResponse[]>('departments');

    if (cachedDepartments?.length) {
      this.departments.set(cachedDepartments);
      this.isPageLoading.set(false);
      return;
    }

    this.departmentApiService
      .getAllDepartments()
      .pipe(finalize(() => this.isPageLoading.set(false)))
      .subscribe({
        next: (response) => {
          this.departments.set(response.data ?? []);
          this.storageService.setItem('departments', response.data ?? []);
        },

        error: (error) => {
          this.departments.set([]);

          this.toastService.show(getStatusCodeMessage(error.statusCode));
          this.toastService.show(getStatusCodeMessage(error.statusCode));
        }
      });
  }

  public openAddModal(): void {
    this.departmentForm.reset({
      id: '',
      name: ''
    });

    this.selectedDepartment.set(null);
    this.departmentEmployees.set([]);

    this.activeModal.set('add');
  }

  public openEditModal(department: DepartmentResponse): void {
    this.departmentForm.reset({
      id: department.id,
      name: department.name ?? ''
    });

    this.selectedDepartment.set(null);
    this.departmentEmployees.set([]);
    this.departmentPositions.set([]);

    this.activeModal.set('edit');
  }

  public openDetailModal(department: DepartmentResponse): void {

    this.selectedDepartment.set(department);
    this.departmentEmployees.set([]);

    this.activeModal.set('detail');

    this.isModalLoading.set(true);

    this.positionApiService.getPositionByDepartment(department.id)
      .pipe(finalize(() => this.isModalLoading.set(false)))
      .subscribe({
        next: (response) => { this.departmentPositions.set(response.data ?? []); },
        error: (error) => {
          this.departmentPositions.set([]);
          this.toastService.show(getStatusCodeMessage(error.statusCode));
        }
      });
  }

  public openEmployeesModal(departmentId: string): void {
    this.selectedDepartment.set(null);
    this.departmentEmployees.set([]);

    this.activeModal.set('employees');

    this.isModalLoading.set(true);

    this.departmentApiService.getDepartmentEmployees(departmentId)
      .pipe(finalize(() => this.isModalLoading.set(false)))
      .subscribe({
        next: (response) => { this.departmentEmployees.set(response.data ?? []); },

        error: (error) => {
          this.departmentEmployees.set([]);

          this.toastService.show(getStatusCodeMessage(error.statusCode));
        }
      });
  }

  public createDepartment(): void {

    if (this.departmentForm.invalid) {
      this.departmentForm.markAllAsTouched();
      return;
    }

    const request: CreateDepartmentRequest = {
      name: this.departmentForm.controls.name.value.trim()
    };

    this.isSubmitting.set(true);

    this.departmentApiService.createDepartment(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.toastService.show(getStatusCodeMessage(response.statusCode));

          if (response.statusCode !== 710) { return; }

          this.storageService.removeItem('departments');

          this.closeModals();
          this.loadDepartments();
        },

        error: (error) => { this.toastService.show(getStatusCodeMessage(error.statusCode)); }
      });
  }

  public updateDepartment(): void {
    if (this.departmentForm.invalid) {
      this.departmentForm.markAllAsTouched();
      return;
    }

    const departmentId = this.departmentForm.controls.id.value;

    if (!departmentId) {
      this.toastService.show('Department id is missing.');
      return;
    }

    const request: UpdateDepartmentRequest = {
      id: departmentId,
      name: this.departmentForm.controls.name.value.trim()
    };

    this.isSubmitting.set(true);

    this.departmentApiService.updateDepartment(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.toastService.show(getStatusCodeMessage(response.statusCode));

          if (response.statusCode !== 711) { return; }

          const newDepartment: DepartmentResponse = {
            id: request.id,
            name: request.name,
            totalPositions: 0,
            totalUsers: 0
          };

          const updatedDepartments = [
            ...this.departments().filter((d) => d.id !== request.id),
            newDepartment
          ];

          this.departments.set(updatedDepartments);

          this.storageService.setItem('departments', updatedDepartments);

          this.closeModals();
          this.loadDepartments();
        },

        error: (error) => { this.toastService.show(getStatusCodeMessage(error.statusCode)); }
      });
  }

  public closeModals(): void {
    this.activeModal.set(null);

    this.isModalLoading.set(false);
    this.isSubmitting.set(false);

    this.selectedDepartment.set(null);
    this.departmentEmployees.set([]);

    this.departmentForm.reset({ id: '', name: '' });
  }

  public controlInvalid(controlName: 'name'): boolean {
    const control = this.departmentForm.controls[controlName];

    return (control.invalid && (control.dirty || control.touched));
  }
}
