import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';

import { getStatusCodeMessage } from '@app/core/config/status-code-messages';
import {
  BranchResponse,
  BranchUserResponse,
  CreateBranchRequest,
  UpdateBranchRequest
} from '@app/core/models/branchModels/branch.model';
import { BranchApiService } from '@app/core/services/api-service/branch-api.service';
import { ToastService } from '@app/core/services/toast.service';
import { TableColumn, GenericTableComponent } from '@app/shared/components/table/generic-table.component';
import { StorageService } from '@app/core/services/storage.service';
import { TranslatePipe } from '@ngx-translate/core';

type BranchModalType = 'add' | 'edit' | 'detail' | 'employees' | null;

@Component({
  standalone: true,
  selector: 'app-branch',
  imports: [CommonModule, ReactiveFormsModule, GenericTableComponent, TranslatePipe],
  templateUrl: './branch.component.html',
})
export class BranchComponent implements OnInit {
  private readonly branchApiService = inject(BranchApiService);
  private readonly toastService = inject(ToastService);
  private readonly storageService = inject(StorageService);

  public readonly branches = signal<BranchResponse[]>([]);
  public readonly branchEmployees = signal<BranchUserResponse[]>([]);
  public readonly selectedBranch = signal<BranchResponse | null>(null);

  public readonly isPageLoading = signal(false);
  public readonly isModalLoading = signal(false);
  public readonly isSubmitting = signal(false);

  private readonly activeModal = signal<BranchModalType>(null);

  public readonly userColumns: TableColumn[] = [
    { key: 'name', title: 'Name' },
    { key: 'email', title: 'Email' },
    { key: 'departmentName', title: 'Department' },
    { key: 'positionName', title: 'Position' }
  ];

  public readonly isAddModalOpen = computed(() => this.activeModal() === 'add');

  public readonly isEditModalOpen = computed(() => this.activeModal() === 'edit');

  public readonly isDetailModalOpen = computed(() => this.activeModal() === 'detail');

  public readonly isEmployeesModalOpen = computed(() => this.activeModal() === 'employees');

  public readonly branchForm = new FormGroup({
    id: new FormControl<string>('', { nonNullable: true }),

    name: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(2), Validators.maxLength(100)]
    }),

    location: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(2), Validators.maxLength(100)]
    })
  });

  public ngOnInit(): void { this.loadBranches(); }

  public loadBranches(): void {
    this.isPageLoading.set(true);

    const cachedBranches = this.storageService.getItem<BranchResponse[]>('branches');

    if (cachedBranches?.length) {
      this.branches.set(cachedBranches);

      this.isPageLoading.set(false);

      return;
    }

    this.branchApiService
      .getAllBranches()
      .pipe(finalize(() => this.isPageLoading.set(false)))
      .subscribe({
        next: (response) => {
          const branches = response.data ?? [];
          this.branches.set(branches);
          this.storageService.setItem('branches', branches);
        },

        error: (error) => {
          this.branches.set([]);

          this.toastService.show(getStatusCodeMessage(error.statusCode));
        }
      });
  }

  public openAddModal(): void {
    this.branchForm.reset({ id: '', name: '', location: '' });

    this.selectedBranch.set(null);
    this.branchEmployees.set([]);

    this.activeModal.set('add');
  }

  public openEditModal(branch: BranchResponse): void {
    this.branchForm.reset({
      id: branch.id,
      name: branch.name ?? '',
      location: branch.location ?? ''
    });

    this.selectedBranch.set(null);
    this.branchEmployees.set([]);

    this.activeModal.set('edit');
  }

  public openDetailModal(branch: BranchResponse): void {
    this.branchEmployees.set([]);
    this.selectedBranch.set(branch);
    this.activeModal.set('detail');
  }

  public openEmployeesModal(branchId: string): void {
    this.selectedBranch.set(null);
    this.branchEmployees.set([]);

    this.activeModal.set('employees');

    this.isModalLoading.set(true);

    this.branchApiService
      .getBranchEmployees(branchId)
      .pipe(finalize(() => this.isModalLoading.set(false)))
      .subscribe({
        next: (response) => { this.branchEmployees.set(response.data ?? []); },

        error: (error) => {
          this.branchEmployees.set([]);

          this.toastService.show(getStatusCodeMessage(error.statusCode));
        }
      });
  }

  public createBranch(): void {
    if (this.branchForm.invalid) {
      this.branchForm.markAllAsTouched();
      return;
    }

    const request: CreateBranchRequest = {
      name: this.branchForm.controls.name.value.trim(),
      location: this.branchForm.controls.location.value.trim()
    };

    this.isSubmitting.set(true);

    this.branchApiService
      .createBranch(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.toastService.show(getStatusCodeMessage(response.statusCode));

          if (response.statusCode !== 702) {
            return;
          }

          this.storageService.removeItem('branches');

          this.closeModals();
          this.loadBranches();
        },

        error: (error) => {
          this.toastService.show(getStatusCodeMessage(error.statusCode));
        }
      });
  }

  public updateBranch(): void {
    if (this.branchForm.invalid) {
      this.branchForm.markAllAsTouched();
      return;
    }

    const branchId = this.branchForm.controls.id.value;

    if (!branchId) {
      this.toastService.show('Branch id is missing.');
      return;
    }

    const request: UpdateBranchRequest = {
      id: branchId,
      name: this.branchForm.controls.name.value.trim(),
      location: this.branchForm.controls.location.value.trim()
    };

    this.isSubmitting.set(true);

    this.branchApiService
      .updateBranch(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.toastService.show(getStatusCodeMessage(response.statusCode));

          if (response.statusCode !== 712) {
            return;
          }

          const newBranch: BranchResponse = {
            id: request.id,
            name: request.name,
            location: request.location,
            totalUsers: this.selectedBranch()?.totalUsers ?? 0
          };

          const updatedBranches = [
            ...this.branches().filter((b) => b.id !== request.id),
            newBranch
          ];

          this.branches.set(updatedBranches);

          this.storageService.setItem(
            'branches',
            updatedBranches
          );

          this.closeModals();
          this.loadBranches();
        },

        error: (error) => {
          this.toastService.show(getStatusCodeMessage(error.statusCode));
        }
      });
  }

  public closeModals(): void {
    this.activeModal.set(null);

    this.isModalLoading.set(false);
    this.isSubmitting.set(false);

    this.selectedBranch.set(null);
    this.branchEmployees.set([]);

    this.branchForm.reset({
      id: '',
      name: '',
      location: ''
    });
  }

  public controlInvalid(controlName: 'name' | 'location'): boolean {
    const control = this.branchForm.controls[controlName];

    return (control.invalid && (control.dirty || control.touched)
    );
  }
}
