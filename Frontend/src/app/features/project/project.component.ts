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
  ManagerResponse,
  projectCreateRequest,
  projectResponse,
  projectUpdateRequest,
  ProjectUserResponse
} from '@app/core/models/projectModels/project.model';

import { ProjectApiService } from '@app/core/services/api-service/project-api.service';
import { UserApiService } from '@app/core/services/api-service/user-api.service';
import { ToastService } from '@app/core/services/toast.service';

import {
  GenericTableComponent,
  TableColumn
} from '@app/shared/components/table/generic-table.component';
import { EmployeeProjectApiService } from '@app/core/services/api-service/employeeProject-api.service';
import { UserResponse } from '@app/core/models/userModels/user.model';
import { AuthService } from '@app/core/services/auth.service';
import { TranslatePipe } from '@ngx-translate/core';

type ProjectModalType = | 'add' | 'edit' | 'detail' | 'employees' | 'addEmployeeProject' | null;

@Component({
  standalone: true,
  selector: 'app-project',
  imports: [CommonModule, ReactiveFormsModule, GenericTableComponent, TranslatePipe],
  templateUrl: './project.component.html',
})
export class ProjectComponent implements OnInit {
  private readonly projectApiService = inject(ProjectApiService);
  private readonly userApiService = inject(UserApiService);
  private readonly toastService = inject(ToastService);
  private readonly employeeProjectApiService = inject(EmployeeProjectApiService);
  private readonly authService = inject(AuthService);

  public readonly projects = signal<projectResponse[]>([]);
  public readonly Employees = signal<UserResponse[]>([]);

  public readonly managers = signal<ManagerResponse[]>([]);
  public readonly projectEmployees = signal<ProjectUserResponse[]>([]);

  public readonly selectedProject = signal<projectResponse | null>(null);

  public readonly currentUser = this.authService.currentUser;

  public readonly isPageLoading = signal(false);
  public readonly isModalLoading = signal(false);
  public readonly isSubmitting = signal(false);

  private readonly activeModal = signal<ProjectModalType>(null);

  public readonly isAddModalOpen = computed(() => this.activeModal() === 'add');

  public readonly isEditModalOpen = computed(() => this.activeModal() === 'edit');

  public readonly isDetailModalOpen = computed(() => this.activeModal() === 'detail');

  public readonly isEmployeesModalOpen = computed(() => this.activeModal() === 'employees');

  public readonly isAddEmployeeProjectModalOpen = computed(() => this.activeModal() === 'addEmployeeProject');

  public readonly employeeColumns: TableColumn[] = [
    { key: 'name', title: 'Name' },
    { key: 'email', title: 'Email' },
    { key: 'branchName', title: 'Branch' },
    { key: 'departmentName', title: 'Department' },
    { key: 'positionName', title: 'Position' },
    { key: 'roleName', title: 'Role' }
  ];

  public readonly projectEmployeesTableData = computed(() => this.projectEmployees() as unknown as Record<string, unknown>[]);

  public readonly projectForm = new FormGroup({
    id: new FormControl<string>('', { nonNullable: true }),

    name: new FormControl<string>('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(100)
      ]
    }),

    description: new FormControl<string>('', { nonNullable: true, }),

    startDate: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),

    endDate: new FormControl<string>('', { nonNullable: true }),

    projectManagerId: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] })
  });

  public readonly EmployeeProjectForm = new FormGroup({
    projectId: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    userId: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] })
  });

  public ngOnInit(): void {
    this.loadProjects();
  }

  public loadProjects(): void {
    this.isPageLoading.set(true);

    if (this.currentUser()?.role === 'Admin') {
      this.userApiService.getManagers().subscribe({
        next: response => { this.managers.set(response.data ?? []); },
        error: error => {
          this.managers.set([]);
          this.toastService.show(getStatusCodeMessage(error.error.statusCode));
        }
      });
    }

    if (this.currentUser()?.role === 'Manager') {
      const userId = this.currentUser()?.userId;

      this.projectApiService
        .getProjectsByManagerId(userId ?? '')
        .pipe(finalize(() => this.isPageLoading.set(false)))
        .subscribe({
          next: response => {
            this.projects.set(response.data ?? []);
          },
          error: error => {
            this.projects.set([]);
            this.toastService.show(getStatusCodeMessage(error.error.statusCode));
          }
        });
    } else if (this.currentUser()?.role === 'Employee') {
      const userId = this.currentUser()?.userId;

      this.projectApiService
        .getEmployeeProjects(userId ?? '')
        .pipe(finalize(() => this.isPageLoading.set(false)))
        .subscribe({
          next: response => {
            this.projects.set(response.data ?? []);
          },
          error: error => {
            this.projects.set([]);
            this.toastService.show(getStatusCodeMessage(error.error.statusCode));
          }
        });
    }
    else {
      this.projectApiService
        .getAllProjects()
        .pipe(finalize(() => this.isPageLoading.set(false)))
        .subscribe({
          next: response => {
            this.projects.set(response.data ?? []);
          },

          error: error => {
            this.projects.set([]);

            this.toastService.show(getStatusCodeMessage(error.error.statusCode)
            );
          }
        });
    }
  }

  public searchEmployees(searchTerm: string): void {

    if (!searchTerm || searchTerm.trim() === '') {
      this.Employees.set([]);
      return;
    }
    this.userApiService.getUserBySearch({ searchTerm: searchTerm.trim() }).subscribe({
      next: response => { this.Employees.set(response.data ?? []); },
      error: error => {
        this.Employees.set([]);
        this.toastService.show(getStatusCodeMessage(error.error.statusCode));
      }
    });
  }

  public addEmployeeProject(projectId: string): void {
    if (this.EmployeeProjectForm.invalid) {
      this.EmployeeProjectForm.markAllAsTouched();
      return;
    }

    const request = {
      projectId: projectId,
      userId: this.EmployeeProjectForm.controls.userId.value
    };

    this.isSubmitting.set(true);

    this.employeeProjectApiService
      .CreateEmployeeProject(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.toastService.show('Employee added to project successfully.');

          this.projects.set([]);
          this.loadProjects();
          this.closeModals();
        },
        error: error => {
          this.toastService.show(getStatusCodeMessage(error.error.statusCode));
        }
      });
  }

  public openAddEmployeeProjectModal(projectId: string): void {
    this.EmployeeProjectForm.reset({
      projectId: projectId,
      userId: ''
    });
    this.Employees.set([]);

    this.selectedProject.set(this.projects().find(project => project.id === projectId) ?? null);

    this.activeModal.set('addEmployeeProject');
  }

  public openAddModal(): void {
    this.projectForm.reset({
      id: '',
      name: '',
      description: '',
      startDate: '',
      endDate: '',
      projectManagerId: ''
    });

    this.selectedProject.set(null);
    this.projectEmployees.set([]);

    this.activeModal.set('add');
  }

  public openEditModal(project: projectResponse): void {

    this.projectForm.reset({
      id: project.id,
      name: project.name,
      description: project.description ?? '',
      startDate: project.startDate,
      endDate: project.endDate,
      projectManagerId: project.projectManagerId
    });

    this.selectedProject.set(project);
    this.projectEmployees.set([]);

    this.activeModal.set('edit');
  }

  public openDetailModal(project: projectResponse): void {

    this.selectedProject.set(project);
    this.projectEmployees.set([]);

    this.activeModal.set('detail');
  }

  public openEmployeesModal(projectId: string): void {
    this.selectedProject.set(null);
    this.projectEmployees.set([]);

    this.activeModal.set('employees');

    this.isModalLoading.set(true);

    this.projectApiService
      .getProjectEmployees(projectId)
      .pipe(finalize(() => this.isModalLoading.set(false)))
      .subscribe({
        next: response => { this.projectEmployees.set(response.data ?? []); },

        error: error => {
          this.projectEmployees.set([]);

          this.toastService.show(getStatusCodeMessage(error.error.statusCode));
        }
      });
  }

  public createProject(): void {
    if (this.projectForm.invalid) {
      this.projectForm.markAllAsTouched();
      return;
    }

    const endDate = this.projectForm.controls.endDate.value;

    const request: projectCreateRequest = {
      name: this.projectForm.controls.name.value.trim(),
      description: this.projectForm.controls.description.value.trim() || undefined,
      startDate: this.projectForm.controls.startDate.value,
      endDate: endDate ? endDate : null,
      projectManagerId: this.projectForm.controls.projectManagerId.value
    };

    this.isSubmitting.set(true);

    this.projectApiService
      .createProject(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: response => {
          this.toastService.show(getStatusCodeMessage(response.statusCode));
          this.closeModals();
          this.loadProjects();
        },
        error: error => {
          this.toastService.show(getStatusCodeMessage(error.error.statusCode));
        }
      });
  }
  public updateProject(): void {

    if (this.projectForm.invalid) {
      this.projectForm.markAllAsTouched();
      return;
    }

    const projectId = this.projectForm.controls.id.value;

    if (!projectId) {
      this.toastService.show('Project id is missing.');
      return;
    }

    var endDate = this.projectForm.controls.endDate.value;
    const request: projectUpdateRequest = {
      id: projectId,
      name: this.projectForm.controls.name.value.trim(),
      description: this.projectForm.controls.description.value.trim() || undefined,
      startDate: this.projectForm.controls.startDate.value,
      endDate: this.projectForm.controls.endDate.value,
      projectManagerId: this.projectForm.controls.projectManagerId.value
    };

    this.isSubmitting.set(true);

    this.projectApiService
      .updateProject(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: response => {
          this.toastService.show(getStatusCodeMessage(response.statusCode));

          this.closeModals();
          this.loadProjects();
        },

        error: error => { this.toastService.show(getStatusCodeMessage(error.error.statusCode)); }
      });
  }

  public closeModals(): void {

    this.activeModal.set(null);

    this.isModalLoading.set(false);
    this.isSubmitting.set(false);

    this.selectedProject.set(null);
    this.projectEmployees.set([]);

    this.projectForm.reset({
      id: '',
      name: '',
      description: '',
      startDate: '',
      endDate: '',
      projectManagerId: ''
    });
  }

  public controlInvalid(controlName: | 'name' | 'startDate' | 'endDate' | 'projectManagerId'): boolean {
    const control = this.projectForm.controls[controlName];
    return (control.invalid && (control.dirty || control.touched));
  }
}
