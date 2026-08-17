import { Component, Input, Pipe, PipeTransform, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { TranslatePipe } from '@ngx-translate/core';
import { ProjectComponent } from './project.component';
import { GenericTableComponent } from '@app/shared/components/table/generic-table.component';
import { ProjectApiService } from '@app/core/services/api-service/project-api.service';
import { UserApiService } from '@app/core/services/api-service/user-api.service';
import { EmployeeProjectApiService } from '@app/core/services/api-service/employeeProject-api.service';
import { ToastService } from '@app/core/services/toast.service';
import { AuthService } from '@app/core/services/auth.service';

@Pipe({ name: 'translate', standalone: true })
class MockTranslatePipe implements PipeTransform {
  public transform(value: string): string { return value; }
}

@Component({
  selector: 'app-table',
  standalone: true,
  template: ''
})
class MockGenericTableComponent {
  @Input() columns: unknown[] = [];
  @Input() data: unknown[] = [];
}

describe('ProjectComponent', () => {
  let component: ProjectComponent;
  let fixture: ComponentFixture<ProjectComponent>;

  let projectApiServiceMock: {
    getAllProjects: ReturnType<typeof vi.fn>;
    getProjectsByManagerId: ReturnType<typeof vi.fn>;
    getEmployeeProjects: ReturnType<typeof vi.fn>;
    getProjectEmployees: ReturnType<typeof vi.fn>;
    createProject: ReturnType<typeof vi.fn>;
    updateProject: ReturnType<typeof vi.fn>;
  };

  let userApiServiceMock: {
    getManagers: ReturnType<typeof vi.fn>;
    getUserBySearch: ReturnType<typeof vi.fn>;
  };

  let employeeProjectApiServiceMock: {
    CreateEmployeeProject: ReturnType<typeof vi.fn>;
  };

  let toastServiceMock: {
    show: ReturnType<typeof vi.fn>;
  };

  let authServiceMock: {
    currentUser: ReturnType<typeof signal>;
  };

  const mockProjects = [
    {
      id: '1',
      name: 'HRMS',
      description: 'Human Resource Management System',
      startDate: '2026-01-01',
      endDate: '2026-12-31',
      projectManagerId: 'manager-1',
      projectManagerName: 'John Manager',
      totalUsers: 10
    },
    {
      id: '2',
      name: 'Payroll',
      description: 'Payroll Management',
      startDate: '2026-02-01',
      endDate: null,
      projectManagerId: 'manager-2',
      projectManagerName: 'Jane Manager',
      totalUsers: 5
    }
  ];

  const mockManagers = [
    { userId: 'manager-1', name: 'John Manager' },
    { userId: 'manager-2', name: 'Jane Manager' }
  ];

  const mockEmployees = [
    {
      userId: 'user-1',
      name: 'Harsh Donda',
      email: 'harsh@test.com',
      branchName: 'Main Branch',
      departmentName: 'IT',
      positionName: 'Developer',
      roleName: 'Employee'
    }
  ];

  const mockProjectEmployees = [
    {
      userId: 'user-1',
      name: 'Harsh Donda',
      email: 'harsh@test.com',
      branchName: 'Main Branch',
      departmentName: 'IT',
      positionName: 'Developer',
      roleName: 'Employee'
    }
  ];

  beforeEach(async () => {
    projectApiServiceMock = {
      getAllProjects: vi.fn().mockReturnValue(of({ data: [] })),
      getProjectsByManagerId: vi.fn().mockReturnValue(of({ data: [] })),
      getEmployeeProjects: vi.fn().mockReturnValue(of({ data: [] })),
      getProjectEmployees: vi.fn().mockReturnValue(of({ data: [] })),
      createProject: vi.fn().mockReturnValue(of({ statusCode: 704 })),
      updateProject: vi.fn().mockReturnValue(of({ statusCode: 706 }))
    };

    userApiServiceMock = {
      getManagers: vi.fn().mockReturnValue(of({ data: [] })),
      getUserBySearch: vi.fn().mockReturnValue(of({ data: [] }))
    };

    employeeProjectApiServiceMock = {
      CreateEmployeeProject: vi.fn().mockReturnValue(of({ statusCode: 704 }))
    };

    toastServiceMock = {
      show: vi.fn()
    };

    authServiceMock = {
      currentUser: signal({ userId: 'user-1', role: 'Admin' })
    };

    await TestBed.configureTestingModule({
      imports: [ProjectComponent],
      providers: [
        { provide: ProjectApiService, useValue: projectApiServiceMock },
        { provide: UserApiService, useValue: userApiServiceMock },
        { provide: EmployeeProjectApiService, useValue: employeeProjectApiServiceMock },
        { provide: ToastService, useValue: toastServiceMock },
        { provide: AuthService, useValue: authServiceMock }
      ]
    }).overrideComponent(ProjectComponent, {
      remove: { imports: [GenericTableComponent, TranslatePipe] },
      add: { imports: [MockGenericTableComponent, MockTranslatePipe] }
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectComponent);
    component = fixture.componentInstance;
  });

  describe('Unit tests', () => {

    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should load projects for admin', () => {
      authServiceMock.currentUser.set({ userId: 'admin-1', role: 'Admin' });
      userApiServiceMock.getManagers.mockReturnValue(of({ data: mockManagers }));
      projectApiServiceMock.getAllProjects.mockReturnValue(of({ data: mockProjects }));

      component.loadProjects();

      expect(userApiServiceMock.getManagers).toHaveBeenCalled();
      expect(component.managers()).toEqual(mockManagers);
      expect(projectApiServiceMock.getAllProjects).toHaveBeenCalled();
      expect(component.projects()).toEqual(mockProjects);
      expect(component.isPageLoading()).toBe(false);
    });

    it('should handle admin managers error', () => {
      authServiceMock.currentUser.set({ userId: 'admin-1', role: 'Admin' });

      userApiServiceMock.getManagers.mockReturnValue(throwError(() => ({ statusCode: 500 })));

      component.loadProjects();
      expect(component.managers()).toEqual([]);
      expect(toastServiceMock.show).toHaveBeenCalled();
    });

    it('should use empty user id when Manager user id is undefined', () => {
      authServiceMock.currentUser.set({ role: 'Manager', userId: undefined });

      projectApiServiceMock.getProjectsByManagerId.mockReturnValue(of({ data: [] }));
      component.loadProjects();

      expect(projectApiServiceMock.getProjectsByManagerId).toHaveBeenCalledWith('');
    });

    it('should handle manager projects error', () => {
      authServiceMock.currentUser.set({ userId: 'manager-1', role: 'Manager' });
      projectApiServiceMock.getProjectsByManagerId.mockReturnValue(throwError(() => ({ statusCode: 500 })));
      component.loadProjects();

      expect(component.projects()).toEqual([]);
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.isPageLoading()).toBe(false);
    });

    it('should load projects for employee', () => {
      authServiceMock.currentUser.set({ userId: 'employee-1', role: 'Employee' });
      projectApiServiceMock.getEmployeeProjects.mockReturnValue(of({ data: mockProjects }));

      component.loadProjects();

      expect(projectApiServiceMock.getEmployeeProjects).toHaveBeenCalledWith('employee-1');
      expect(component.projects()).toEqual(mockProjects);
      expect(component.isPageLoading()).toBe(false);
    });

    it('should use empty user id for employee when user id is missing', () => {
      authServiceMock.currentUser.set({ role: 'Employee' });
      component.loadProjects();
      expect(projectApiServiceMock.getEmployeeProjects).toHaveBeenCalledWith('');
    });

    it('should handle employee projects error', () => {
      authServiceMock.currentUser.set({ userId: 'employee-1', role: 'Employee' });
      projectApiServiceMock.getEmployeeProjects.mockReturnValue(throwError(() => ({ statusCode: 500 })));

      component.loadProjects();

      expect(component.projects()).toEqual([]);
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.isPageLoading()).toBe(false);
    });

    it('should load all projects for other roles', () => {
      authServiceMock.currentUser.set({ userId: 'user-1', role: 'Other' });
      projectApiServiceMock.getAllProjects.mockReturnValue(of({ data: mockProjects }));

      component.loadProjects();

      expect(projectApiServiceMock.getAllProjects).toHaveBeenCalled();
      expect(component.projects()).toEqual(mockProjects);
      expect(component.isPageLoading()).toBe(false);
    });

    it('should load all projects when current user is null', () => {
      authServiceMock.currentUser.set(null);
      projectApiServiceMock.getAllProjects.mockReturnValue(of({ data: mockProjects }));

      component.loadProjects();

      expect(projectApiServiceMock.getAllProjects).toHaveBeenCalled();
      expect(component.projects()).toEqual(mockProjects);
    });

    it('should handle all projects error', () => {
      authServiceMock.currentUser.set({ userId: 'user-1', role: 'Other' });
      projectApiServiceMock.getAllProjects.mockReturnValue(throwError(() => ({ statusCode: 500 })));

      component.loadProjects();

      expect(component.projects()).toEqual([]);
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.isPageLoading()).toBe(false);
    });

    it('should handle empty project response', () => {
      authServiceMock.currentUser.set({ userId: 'user-1', role: 'Other' });
      projectApiServiceMock.getAllProjects.mockReturnValue(of({ data: null }));
      component.loadProjects();
      expect(component.projects()).toEqual([]);
    });

    it('should clear employees when search term is empty', () => {
      component.Employees.set(mockEmployees as any);

      component.searchEmployees('');
      expect(component.Employees()).toEqual([]);
      expect(userApiServiceMock.getUserBySearch).not.toHaveBeenCalled();
    });

    it('should clear employees when search term contains only spaces', () => {
      component.Employees.set(mockEmployees as any);
      component.searchEmployees('   ');

      expect(component.Employees()).toEqual([]);
      expect(userApiServiceMock.getUserBySearch).not.toHaveBeenCalled();
    });

    it('should search employees with trimmed search term', () => {
      userApiServiceMock.getUserBySearch.mockReturnValue(of({ data: mockEmployees }));
      component.searchEmployees('  Harsh  ');

      expect(userApiServiceMock.getUserBySearch).toHaveBeenCalledWith({ searchTerm: 'Harsh' });
      expect(component.Employees()).toEqual(mockEmployees);
    });

    it('should handle employee search error', () => {
      userApiServiceMock.getUserBySearch.mockReturnValue(throwError(() => ({ statusCode: 500 })));
      component.searchEmployees('Harsh');

      expect(component.Employees()).toEqual([]);
      expect(toastServiceMock.show).toHaveBeenCalled();
    });

    it('should not add employee when employee project form is invalid', () => {
      component.EmployeeProjectForm.reset({ projectId: '', userId: '' });
      component.addEmployeeProject('project-1');

      expect(employeeProjectApiServiceMock.CreateEmployeeProject).not.toHaveBeenCalled();
      expect(component.EmployeeProjectForm.touched).toBe(true);
    });

    it('should add employee to project successfully', () => {
      authServiceMock.currentUser.set({ userId: 'user-1', role: 'Other' });

      vi.spyOn(component, 'loadProjects').mockImplementation(() => { });
      vi.spyOn(component, 'closeModals').mockImplementation(() => { });

      component.EmployeeProjectForm.setValue({ projectId: 'project-1', userId: 'user-2' });

      employeeProjectApiServiceMock.CreateEmployeeProject.mockReturnValue(of({ statusCode: 704 }));
      component.addEmployeeProject('project-1');

      expect(employeeProjectApiServiceMock.CreateEmployeeProject).toHaveBeenCalledWith({ projectId: 'project-1', userId: 'user-2' });
      expect(toastServiceMock.show).toHaveBeenCalledWith('Employee added to project successfully.');
      expect(component.projects()).toEqual([]);
      expect(component.isSubmitting()).toBe(false);
      expect(component.closeModals).toHaveBeenCalled();
    });

    it('should show error when adding employee project fails', () => {
      component.EmployeeProjectForm.setValue({
        projectId: 'project-1',
        userId: 'user-1'
      });
      employeeProjectApiServiceMock.CreateEmployeeProject.mockReturnValue(throwError(() => ({ statusCode: 500 })));
      component.addEmployeeProject('project-1');
      expect(toastServiceMock.show).toHaveBeenCalled();
    });

    it('should open add employee project modal with selected project', () => {
      component.projects.set(mockProjects as any);
      component.openAddEmployeeProjectModal('1');

      expect(component.isAddEmployeeProjectModalOpen()).toBe(true);
      expect(component.selectedProject()).toEqual(mockProjects[0]);
      expect(component.EmployeeProjectForm.value).toEqual({ projectId: '1', userId: '' });
      expect(component.Employees()).toEqual([]);
    });

    it('should set selected project to null when project does not exist', () => {
      component.projects.set(mockProjects as any);
      component.openAddEmployeeProjectModal('invalid');

      expect(component.selectedProject()).toBe(null);
      expect(component.isAddEmployeeProjectModalOpen()).toBe(true);
    });

    it('should open add modal', () => {
      component.openAddModal();

      expect(component.isAddModalOpen()).toBe(true);
      expect(component.selectedProject()).toBe(null);
      expect(component.projectEmployees()).toEqual([]);
      expect(component.projectForm.value).toEqual({ id: '', name: '', description: '', startDate: '', endDate: '', projectManagerId: '' });
    });

    it('should open edit modal', () => {
      component.openEditModal(mockProjects[0] as any);

      expect(component.isEditModalOpen()).toBe(true);
      expect(component.selectedProject()).toEqual(mockProjects[0]);
      expect(component.projectForm.value).toEqual({
        id: '1',
        name: 'HRMS',
        description: 'Human Resource Management System',
        startDate: '2026-01-01',
        endDate: '2026-12-31',
        projectManagerId: 'manager-1'
      });
    });

    it('should open detail modal', () => {
      component.openDetailModal(mockProjects[0] as any);

      expect(component.isDetailModalOpen()).toBe(true);
      expect(component.selectedProject()).toEqual(mockProjects[0]);
      expect(component.projectEmployees()).toEqual([]);
    });

    it('should load project employees', () => {
      projectApiServiceMock.getProjectEmployees.mockReturnValue(of({ data: mockProjectEmployees }));

      component.openEmployeesModal('1');

      expect(component.isEmployeesModalOpen()).toBe(true);
      expect(projectApiServiceMock.getProjectEmployees).toHaveBeenCalledWith('1');
      expect(component.projectEmployees()).toEqual(mockProjectEmployees);
      expect(component.isModalLoading()).toBe(false);
    });

    it('should handle empty project employees response', () => {
      projectApiServiceMock.getProjectEmployees.mockReturnValue(of({ data: null }));

      component.openEmployeesModal('1');

      expect(component.projectEmployees()).toEqual([]);
      expect(component.isModalLoading()).toBe(false);
    });

    it('should handle project employees error', () => {
      projectApiServiceMock.getProjectEmployees.mockReturnValue(throwError(() => ({ statusCode: 500 })));

      component.openEmployeesModal('1');

      expect(component.projectEmployees()).toEqual([]);
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.isModalLoading()).toBe(false);
    });

    it('should not create project when form is invalid', () => {
      component.openAddModal();
      component.createProject();

      expect(projectApiServiceMock.createProject).not.toHaveBeenCalled();
      expect(component.projectForm.touched).toBe(true);
    });

    it('should create project with trimmed values', () => {
      component.projectForm.setValue({
        id: '',
        name: '  HRMS  ',
        description: '  Human Resource System  ',
        startDate: '2026-01-01',
        endDate: '2026-12-31',
        projectManagerId: 'manager-1'
      });
      component.createProject();

      expect(projectApiServiceMock.createProject).toHaveBeenCalledWith({
        name: 'HRMS',
        description: 'Human Resource System',
        startDate: '2026-01-01',
        endDate: '2026-12-31',
        projectManagerId: 'manager-1'
      });
      expect(component.isSubmitting()).toBe(false);
    });

    it('should use empty description when description is undefined', () => {
      const project = { ...mockProjects[0], description: undefined } as any;
      component.openEditModal(project);
      expect(component.projectForm.controls.description.value).toBe('');
    });

    it('should handle create project success', () => {
      vi.spyOn(component, 'closeModals').mockImplementation(() => { });
      vi.spyOn(component, 'loadProjects').mockImplementation(() => { });

      projectApiServiceMock.createProject.mockReturnValue(of({ statusCode: 704 }));

      component.projectForm.setValue({
        id: '',
        name: 'HRMS',
        description: 'System',
        startDate: '2026-01-01',
        endDate: '',
        projectManagerId: 'manager-1'
      });
      component.createProject();

      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.closeModals).toHaveBeenCalled();
      expect(component.loadProjects).toHaveBeenCalled();
      expect(component.isSubmitting()).toBe(false);
    });

    it('should show error when project creation fails', () => {
      component.projectForm.setValue({
        id: '',
        name: 'Project A',
        description: '',
        startDate: '2026-08-17',
        endDate: '',
        projectManagerId: 'manager-1'
      });
      projectApiServiceMock.createProject.mockReturnValue(throwError(() => ({ statusCode: 500 })));
      component.createProject();
      expect(toastServiceMock.show).toHaveBeenCalled();
    });

    it('should not update project when form is invalid', () => {
      component.openEditModal(mockProjects[0] as any);
      component.projectForm.controls.name.setValue('');

      component.updateProject();

      expect(projectApiServiceMock.updateProject).not.toHaveBeenCalled();
    });

    it('should show error when project id is missing', () => {
      component.openEditModal(mockProjects[0] as any);

      component.projectForm.controls.id.setValue('');

      component.updateProject();

      expect(projectApiServiceMock.updateProject).not.toHaveBeenCalled();
      expect(toastServiceMock.show).toHaveBeenCalledWith('Project id is missing.');
    });



    it('should update project successfully', () => {
      vi.spyOn(component, 'closeModals').mockImplementation(() => { });
      vi.spyOn(component, 'loadProjects').mockImplementation(() => { });

      projectApiServiceMock.updateProject.mockReturnValue(of({ statusCode: 706 }));

      component.projectForm.setValue({
        id: '1',
        name: 'Updated HRMS',
        description: 'Updated System',
        startDate: '2026-01-01',
        endDate: '2026-12-31',
        projectManagerId: 'manager-1'
      });
      component.updateProject();

      expect(projectApiServiceMock.updateProject).toHaveBeenCalledWith({
        id: '1',
        name: 'Updated HRMS',
        description: 'Updated System',
        startDate: '2026-01-01',
        endDate: '2026-12-31',
        projectManagerId: 'manager-1'
      });
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.closeModals).toHaveBeenCalled();
      expect(component.loadProjects).toHaveBeenCalled();
      expect(component.isSubmitting()).toBe(false);
    });


    it('should show error when project update fails', () => {
      component.projectForm.setValue({
        id: 'project-1',
        name: 'Updated Project',
        description: '',
        startDate: '2026-08-17',
        endDate: '',
        projectManagerId: 'manager-1'
      });
      projectApiServiceMock.updateProject.mockReturnValue(throwError(() => ({ statusCode: 500 })));
      component.updateProject();
      expect(toastServiceMock.show).toHaveBeenCalled();
    });

    it('should close all modals and reset state', () => {
      component.openEditModal(mockProjects[0] as any);
      component.isModalLoading.set(true);
      component.isSubmitting.set(true);
      component.closeModals();

      expect(component.isAddModalOpen()).toBe(false);
      expect(component.isEditModalOpen()).toBe(false);
      expect(component.isDetailModalOpen()).toBe(false);
      expect(component.isEmployeesModalOpen()).toBe(false);
      expect(component.isAddEmployeeProjectModalOpen()).toBe(false);
      expect(component.selectedProject()).toBe(null);
      expect(component.projectEmployees()).toEqual([]);
      expect(component.isModalLoading()).toBe(false);
      expect(component.isSubmitting()).toBe(false);
      expect(component.projectForm.value).toEqual({
        id: '',
        name: '',
        description: '',
        startDate: '',
        endDate: '',
        projectManagerId: ''
      });
    });
  });

  describe('Integration tests', () => {
    it('should load projects when component initializes', () => {
      authServiceMock.currentUser.set({ userId: 'user-1', role: 'Admin' });

      projectApiServiceMock.getAllProjects.mockReturnValue(of({ data: mockProjects }));
      userApiServiceMock.getManagers.mockReturnValue(of({ data: mockManagers }));

      fixture.detectChanges();

      expect(projectApiServiceMock.getAllProjects).toHaveBeenCalled();
      expect(userApiServiceMock.getManagers).toHaveBeenCalled();
      expect(component.projects()).toEqual(mockProjects);
      expect(component.managers()).toEqual(mockManagers);
    });

    it('should open add modal from add button', () => {
      fixture.detectChanges();
      const button = fixture.nativeElement.querySelector('.btn-dark') as HTMLButtonElement;

      button.click();
      fixture.detectChanges();

      expect(component.isAddModalOpen()).toBe(true);
    });

    it('should display project detail modal', () => {
      component.openDetailModal(mockProjects[0] as any);

      fixture.detectChanges();

      expect(component.isDetailModalOpen()).toBe(true);
      expect(fixture.nativeElement.textContent).toContain('HRMS');
      expect(fixture.nativeElement.textContent).toContain('Human Resource Management System');
      expect(fixture.nativeElement.textContent).toContain('John Manager');
    });

    it('should display fallback values in project detail modal', () => {
      component.openDetailModal({
        ...mockProjects[0],
        description: null,
        projectManagerName: null
      } as any);

      fixture.detectChanges();

      expect(fixture.nativeElement.textContent).toContain('-');
    });

    it('should display employees modal', () => {
      projectApiServiceMock.getProjectEmployees.mockReturnValue(of({ data: mockProjectEmployees }));

      component.openEmployeesModal('1');
      fixture.detectChanges();

      expect(component.isEmployeesModalOpen()).toBe(true);
      expect(fixture.nativeElement.querySelector('app-table')).toBeTruthy();
    });

    it('should display searched employees', () => {
      component.projects.set(mockProjects as any);
      component.openAddEmployeeProjectModal('1');
      component.Employees.set(mockEmployees as any);

      fixture.detectChanges();

      expect(fixture.nativeElement.textContent).toContain('Harsh Donda');
      expect(fixture.nativeElement.textContent).toContain('harsh@test.com');
    });

    it('should not display employee selector when employees are empty', () => {
      component.projects.set(mockProjects as any);
      component.openAddEmployeeProjectModal('1');
      component.Employees.set([]);

      fixture.detectChanges();

      expect(fixture.nativeElement.querySelector('select[formControlName="userId"]')).toBeFalsy();
    });

    it('should close modal when cancel button is clicked', () => {
      component.openAddModal();
      fixture.detectChanges();

      const cancelButton = fixture.nativeElement.querySelector('.modal-footer button[type="button"]') as HTMLButtonElement;
      expect(cancelButton).toBeTruthy();

      cancelButton.click();
      fixture.detectChanges();

      expect(component.isAddModalOpen()).toBe(false);
    });
  });
});
