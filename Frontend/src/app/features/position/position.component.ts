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
  DepartmentResponse
} from '@app/core/models/departmentModels/department.model';
import {
  CreatePositionRequest,
  PositionResponse,
  PositionUserResponse,
  UpdatePositionRequest
} from '@app/core/models/positionModels/position.model';
import { DepartmentApiService } from '@app/core/services/api-service/department-api.service';
import { PositionApiService } from '@app/core/services/api-service/position-api.service';
import { ToastService } from '@app/core/services/toast.service';
import {
  GenericTableComponent,
  TableColumn
} from '@app/shared/components/table/generic-table.component';
import { StorageService } from '@app/core/services/storage.service';

type PositionModalType = | 'add' | 'edit' | 'detail' | 'employees' | null;

@Component({
  standalone: true,
  selector: 'app-position',
  imports: [CommonModule, ReactiveFormsModule, GenericTableComponent],
  templateUrl: './position.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PositionComponent implements OnInit {
  private readonly positionApiService = inject(PositionApiService);
  private readonly departmentApiService = inject(DepartmentApiService);
  private readonly toastService = inject(ToastService);
  private readonly storageService = inject(StorageService);

  public readonly positions = signal<PositionResponse[]>([]);
  public readonly departments = signal<DepartmentResponse[]>([]);
  public readonly positionEmployees = signal<PositionUserResponse[]>([]);
  public readonly positionEmployeesTableData = computed(
    () => this.positionEmployees() as unknown as Record<string, unknown>[]
  );
  public readonly selectedPosition = signal<PositionResponse | null>(null);

  public readonly isPageLoading = signal(false);
  public readonly isModalLoading = signal(false);
  public readonly isSubmitting = signal(false);

  private readonly activeModal = signal<PositionModalType>(null);

  public readonly userColumns: TableColumn[] = [
    { key: 'name', title: 'Name' },
    { key: 'email', title: 'Email' },
    { key: 'branchName', title: 'Branch' },
    { key: 'roleName', title: 'Role' }
  ];

  public readonly isAddModalOpen = computed(() => this.activeModal() === 'add');
  public readonly isEditModalOpen = computed(() => this.activeModal() === 'edit');
  public readonly isDetailModalOpen = computed(() => this.activeModal() === 'detail');
  public readonly isEmployeesModalOpen = computed(() => this.activeModal() === 'employees');

  public readonly positionForm = new FormGroup({
    id: new FormControl<string>('', { nonNullable: true }),
    name: new FormControl<string>('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(100)
      ]
    }),
    departmentId: new FormControl<string>('', {
      nonNullable: true,
      validators: [
        Validators.required
      ]
    })
  });

  public ngOnInit(): void {
    this.loadPositions();
  }

  public loadPositions(): void {
    this.isPageLoading.set(true);

    const cachedPositions = this.storageService.getItem<PositionResponse[]>('positions');

    if (cachedPositions?.length) {
      this.positions.set(cachedPositions);
      this.isPageLoading.set(false);
      return;
    }

    this.positionApiService
      .getAllPositions()
      .pipe(finalize(() => this.isPageLoading.set(false)))
      .subscribe({
        next: (response) => {
          this.positions.set(response.data ?? []);
          this.storageService.setItem('positions', response.data ?? []);
        },

        error: (error) => {

          this.positions.set([]);

          this.toastService.show(getStatusCodeMessage(error.statusCode));
        }
      });
  }

  public loadDepartments(): void {
    this.isModalLoading.set(true);

    const cachedDepartments = this.storageService.getItem<DepartmentResponse[]>('departments');

    if (cachedDepartments?.length) {
      this.departments.set(cachedDepartments);
      this.isModalLoading.set(false);
      return;
    }

    this.departmentApiService
      .getAllDepartments()
      .pipe(finalize(() => this.isModalLoading.set(false)))
      .subscribe({
        next: (response) => { this.departments.set(response.data ?? []); },

        error: (error) => {
          this.departments.set([]);
          this.storageService.removeItem('departments');

          this.toastService.show(getStatusCodeMessage(error.statusCode));
        }
      });
  }

  public openAddModal(): void {
    this.positionForm.reset({
      id: '',
      name: '',
      departmentId: ''
    });

    this.selectedPosition.set(null);
    this.positionEmployees.set([]);

    this.activeModal.set('add');

    this.loadDepartments();
  }

  public openEditModal(position: PositionResponse): void {
    this.positionForm.reset({
      id: position.id,
      name: position.name ?? '',
      departmentId: this.getPositionDepartmentId(position)
    });

    this.selectedPosition.set(position);
    this.positionEmployees.set([]);

    this.activeModal.set('edit');

    this.loadDepartments();
  }

  public openDetailModal(position: PositionResponse): void {
    this.selectedPosition.set(position);
    this.positionEmployees.set([]);

    this.activeModal.set('detail');
  }

  public openEmployeesModal(positionId: string): void {
    this.selectedPosition.set(null);
    this.positionEmployees.set([]);

    this.activeModal.set('employees');

    this.isModalLoading.set(true);

    this.positionApiService.getPositionEmployees(positionId)
      .pipe(finalize(() => this.isModalLoading.set(false)))
      .subscribe({
        next: (response) => { this.positionEmployees.set(response.data ?? []); },

        error: (error) => {
          this.positionEmployees.set([]);

          this.toastService.show(getStatusCodeMessage(error.statusCode));
        }
      });
  }

  public createPosition(): void {
    if (this.positionForm.invalid) {
      this.positionForm.markAllAsTouched();
      return;
    }

    const request: CreatePositionRequest = {
      name: this.positionForm.controls.name.value.trim(),
      departmentId: this.positionForm.controls.departmentId.value
    };

    this.isSubmitting.set(true);

    this.positionApiService.createPosition(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.toastService.show(getStatusCodeMessage(response.statusCode));

          if (response.statusCode !== 704) { return; }

          this.storageService.removeItem('positions');

          this.closeModals();
          this.loadPositions();
        },

        error: (error) => { this.toastService.show(getStatusCodeMessage(error.statusCode)); }
      });
  }

  public updatePosition(): void {
    if (this.positionForm.invalid) {
      this.positionForm.markAllAsTouched();
      return;
    }

    const positionId = this.positionForm.controls.id.value;

    if (!positionId) {
      this.toastService.show('Position id is missing.');
      return;
    }

    const request: UpdatePositionRequest = {
      id: positionId,
      name: this.positionForm.controls.name.value.trim(),
      departmentId: this.positionForm.controls.departmentId.value
    };

    this.isSubmitting.set(true);

    this.positionApiService.updatePosition(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.toastService.show(getStatusCodeMessage(response.statusCode));

          if (response.statusCode !== 706) { return; }

          const updatedPosition: PositionResponse = {
            id: positionId,
            name: request.name,
            departmentId: request.departmentId,
            totalUsers: this.selectedPosition()?.totalUsers ?? 0
          };

          const updatedPositions = [
            ...this.positions().filter((p) => p.id !== positionId),
            updatedPosition
          ];

          this.positions.set(updatedPositions);

          this.storageService.setItem('positions', updatedPositions);

          this.closeModals();
          this.loadPositions();
        },

        error: (error) => { this.toastService.show(getStatusCodeMessage(error.statusCode)); }
      });
  }

  public closeModals(): void {
    this.activeModal.set(null);

    this.isModalLoading.set(false);
    this.isSubmitting.set(false);

    this.selectedPosition.set(null);
    this.positionEmployees.set([]);

    this.positionForm.reset({
      id: '',
      name: '',
      departmentId: ''
    });
  }

  public controlInvalid(controlName: 'name' | 'departmentId'): boolean {
    const control = this.positionForm.controls[controlName];

    return (control.invalid && (control.dirty || control.touched));
  }

  private getPositionDepartmentId(position: PositionResponse): string {
    const positionValue = position as unknown as {
      departmentId?: string;
      departmentID?: string;
    };

    return positionValue.departmentId ?? positionValue.departmentID ?? '';
  }
}
