import { Component } from '@angular/core';
import {
  ComponentFixture,
  TestBed
} from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';

import { DepartmentComponent } from './department.component';

import { DepartmentApiService } from '@app/core/services/api-service/department-api.service';
import { PositionApiService } from '@app/core/services/api-service/position-api.service';
import { StorageService } from '@app/core/services/storage.service';

import { DepartmentResponse } from '@app/core/models/departmentModels/department.model';
import { PositionResponse } from '@app/core/models/positionModels/position.model';

import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';

@Component({
  selector: 'app-table',
  standalone: true,
  template: ''
})
class MockGenericTableComponent { }

import { Pipe, PipeTransform } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { GenericTableComponent } from '@app/shared/components/table/generic-table.component';

@Pipe({
  name: 'translate',
  standalone: true
})
class MockTranslatePipe implements PipeTransform {
  transform(value: string): string {
    return value;
  }
}

const mockDepartments: DepartmentResponse[] = [
  {
    id: 'department-1',
    name: 'Information Technology',
    totalPositions: 5,
    totalUsers: 20
  },
  {
    id: 'department-2',
    name: 'Human Resources',
    totalPositions: 3,
    totalUsers: 10
  }
];

const mockPositions: PositionResponse[] = [
  {
    id: 'position-1',
    name: 'Software Developer',
    departmentId: 'department-1',
    totalUsers: 10
  },
  {
    id: 'position-2',
    name: 'Senior Developer',
    departmentId: 'department-2',
    totalUsers: 5
  }
];

const mockEmployees = [
  {
    id: 'user-1',
    name: 'Harsh',
    email: 'harsh@example.com',
    branchName: 'Main Branch',
    positionName: 'Developer',
    roleName: 'Employee'
  }
];

describe('DepartmentComponent', () => {

  let fixture: ComponentFixture<DepartmentComponent>;
  let component: DepartmentComponent;

  let departmentApiService: {
    getAllDepartments: ReturnType<typeof vi.fn>;
    getDepartmentEmployees: ReturnType<typeof vi.fn>;
    createDepartment: ReturnType<typeof vi.fn>;
    updateDepartment: ReturnType<typeof vi.fn>;
  };

  let positionApiService: {
    getPositionByDepartment: ReturnType<typeof vi.fn>;
  };

  let storageService: {
    getItem: ReturnType<typeof vi.fn>;
    setItem: ReturnType<typeof vi.fn>;
    removeItem: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {

    departmentApiService = {
      getAllDepartments: vi.fn(),
      getDepartmentEmployees: vi.fn(),
      createDepartment: vi.fn(),
      updateDepartment: vi.fn()
    };

    positionApiService = {
      getPositionByDepartment: vi.fn()
    };

    storageService = {
      getItem: vi.fn(),
      setItem: vi.fn(),
      removeItem: vi.fn()
    };

    departmentApiService.getAllDepartments.mockReturnValue(
      of({
        data: mockDepartments
      })
    );

    departmentApiService.getDepartmentEmployees.mockReturnValue(
      of({
        data: mockEmployees
      })
    );

    departmentApiService.createDepartment.mockReturnValue(
      of({
        statusCode: 710
      })
    );

    departmentApiService.updateDepartment.mockReturnValue(
      of({
        statusCode: 711
      })
    );

    positionApiService.getPositionByDepartment.mockReturnValue(
      of({
        data: mockPositions
      })
    );

    storageService.getItem.mockReturnValue(null);

    await TestBed.configureTestingModule({
      imports: [
        DepartmentComponent,
        ReactiveFormsModule
      ],
      providers: [
        {
          provide: DepartmentApiService,
          useValue: departmentApiService
        },
        {
          provide: PositionApiService,
          useValue: positionApiService
        },
        {
          provide: StorageService,
          useValue: storageService
        }
      ]
    }).overrideComponent(DepartmentComponent, {
      remove: { imports: [GenericTableComponent, TranslatePipe] },
      add: { imports: [MockGenericTableComponent, MockTranslatePipe] }
    }).compileComponents();

    fixture = TestBed.createComponent(DepartmentComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('Component creation', () => {

    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should have empty departments initially', () => {
      expect(component.departments()).toEqual([]);
    });
  });

  describe('ngOnInit', () => {
    it('should load departments on initialization', () => {
      fixture.detectChanges();

      expect(departmentApiService.getAllDepartments).toHaveBeenCalledTimes(1);

      expect(component.departments()).toEqual(mockDepartments);
    });

    it('should use cached departments instead of calling API', () => {
      storageService.getItem.mockReturnValue(mockDepartments);

      fixture.detectChanges();

      expect(departmentApiService.getAllDepartments).not.toHaveBeenCalled();
      expect(component.departments()).toEqual(mockDepartments);
      expect(component.isPageLoading()).toBe(false);
    });

    it('should handle empty cache and call API', () => {
      storageService.getItem.mockReturnValue([]);

      fixture.detectChanges();

      expect(departmentApiService.getAllDepartments).toHaveBeenCalledTimes(1);
    });

    it('should handle API error', () => {
      departmentApiService.getAllDepartments.mockReturnValue(
        throwError(() => ({
          statusCode: 500
        }))
      );

      fixture.detectChanges();
      expect(component.departments()).toEqual([]);
      expect(component.isPageLoading()).toBe(false);
    });
  });

  describe('openAddModal', () => {

    it('should open add modal', () => {
      component.openAddModal();

      expect(component.isAddModalOpen()).toBe(true);
      expect(component.isEditModalOpen()).toBe(false);
    });
  });

  describe('openEditModal', () => {

    it('should open edit modal', () => {
      component.openEditModal(mockDepartments[0]);
      expect(component.isEditModalOpen()).toBe(true);
      expect(component.isAddModalOpen()).toBe(false);
    });

    it('should populate form with department data', () => {
      component.openEditModal(mockDepartments[0]);

      expect(component.departmentForm.controls.id.value).toBe(mockDepartments[0].id);
      expect(component.departmentForm.controls.name.value).toBe(mockDepartments[0].name);
    });

    it('should handle null department name', () => {
      const department = { ...mockDepartments[0], name: null } as unknown as DepartmentResponse;
      component.openEditModal(department);

      expect(component.departmentForm.controls.name.value).toBe('');
    });

    it('should clear selected department', () => {
      component.selectedDepartment.set(mockDepartments[0]);
      component.openEditModal(mockDepartments[1]);
      expect(component.selectedDepartment()).toBe(null);
    });

  });

  describe('openDetailModal', () => {
    it('should open detail modal', () => {
      component.openDetailModal(mockDepartments[0]);

      expect(component.isDetailModalOpen()).toBe(true);
      expect(component.isAddModalOpen()).toBe(false);
      expect(component.isEditModalOpen()).toBe(false);
    });

    it('should select department', () => {
      component.openDetailModal(mockDepartments[0]);

      expect(component.selectedDepartment()).toEqual(mockDepartments[0]);
    });

    it('should load positions for department', () => {
      component.openDetailModal(mockDepartments[0]);

      expect(positionApiService.getPositionByDepartment).toHaveBeenCalledWith(mockDepartments[0].id);
      expect(component.departmentPositions()).toEqual(mockPositions);
    });

    it('should stop modal loading after successful response', () => {
      component.openDetailModal(mockDepartments[0]);

      expect(component.isModalLoading()).toBe(false);
    });

    it('should handle null positions response', () => {
      positionApiService.getPositionByDepartment.mockReturnValue(
        of({
          data: null
        })
      );

      component.openDetailModal(mockDepartments[0]);
      expect(component.departmentPositions()).toEqual([]);
    });

    it('should handle position API error', () => {
      positionApiService.getPositionByDepartment.mockReturnValue(
        throwError(() => ({
          statusCode: 500
        }))
      );

      component.openDetailModal(mockDepartments[0]);

      expect(component.departmentPositions()).toEqual([]);
      expect(component.isModalLoading()).toBe(false);
    });
  });

  describe('openEmployeesModal', () => {
    it('should open employees modal', () => {
      component.openEmployeesModal(mockDepartments[0].id);

      expect(component.isEmployeesModalOpen()).toBe(true);
    });

    it('should call employee API with department id', () => {
      component.openEmployeesModal(mockDepartments[0].id);
      expect(departmentApiService.getDepartmentEmployees).toHaveBeenCalledWith(mockDepartments[0].id);
    });

    it('should set employees from API response', () => {
      component.openEmployeesModal(mockDepartments[0].id);
      expect(component.departmentEmployees()).toEqual(mockEmployees);
    });

    it('should handle null employee data', () => {
      departmentApiService.getDepartmentEmployees.mockReturnValue(of({ data: null }));
      component.openEmployeesModal(mockDepartments[0].id);

      expect(component.departmentEmployees()).toEqual([]);
    });

    it('should handle employee API error', () => {
      departmentApiService.getDepartmentEmployees.mockReturnValue(
        throwError(() => ({
          statusCode: 500
        }))
      );

      component.openEmployeesModal(mockDepartments[0].id);
      expect(component.departmentEmployees()).toEqual([]);
      expect(component.isModalLoading()).toBe(false);
    });
  });

  describe('createDepartment', () => {

    it('should not create department when form is invalid', () => {
      component.departmentForm.controls.name.setValue('');
      component.createDepartment();
      expect(departmentApiService.createDepartment).not.toHaveBeenCalled();
    });

    it('should mark form as touched when invalid', () => {
      component.departmentForm.controls.name.setValue('');
      component.createDepartment();
      expect(component.departmentForm.controls.name.touched).toBe(true);
    });

    it('should create department with trimmed name', () => {
      component.departmentForm.controls.name.setValue('  Information Technology  ');
      component.createDepartment();

      expect(departmentApiService.createDepartment).toHaveBeenCalledWith({ name: 'Information Technology' });
    });

    it('should set submitting state during create', () => {
      departmentApiService.createDepartment.mockReturnValue(
        of({
          statusCode: 710
        })
      );
      component.departmentForm.controls.name.setValue('Information Technology');

      component.createDepartment();
      expect(component.isSubmitting()).toBe(false);
    });

    it('should handle successful create response with status 710', () => {
      component.departmentForm.controls.name.setValue('Information Technology');

      component.createDepartment();
      expect(storageService.removeItem).toHaveBeenCalledWith('departments');
      expect(component.isAddModalOpen()).toBe(false);
    });

    it('should NOT close modal when create returns unexpected status', () => {
      departmentApiService.createDepartment.mockReturnValue(
        of({
          statusCode: 500
        })
      );

      component.openAddModal();
      component.departmentForm.controls.name.setValue('Information Technology');
      component.createDepartment();

      expect(component.isAddModalOpen()).toBe(true);
    });

    it('should handle create API error', () => {
      departmentApiService.createDepartment.mockReturnValue(
        throwError(() => ({
          statusCode: 500
        }))
      );
      component.departmentForm.controls.name.setValue('Information Technology');

      component.createDepartment();
      expect(component.isSubmitting()).toBe(false);
    });

  });

  describe('updateDepartment', () => {
    it('should not update when form is invalid', () => {
      component.departmentForm.controls.name.setValue('');
      component.updateDepartment();
      expect(departmentApiService.updateDepartment).not.toHaveBeenCalled();
    });

    it('should show error when department id is missing', () => {
      component.departmentForm.setValue({
        id: '',
        name: 'Information Technology'
      });
      component.updateDepartment();

      expect(departmentApiService.updateDepartment).not.toHaveBeenCalled();
    });

    it('should update department with trimmed name', () => {
      component.departmentForm.setValue({
        id: 'department-1',
        name: '  Updated Department  '
      });

      component.updateDepartment();

      expect(departmentApiService.updateDepartment).toHaveBeenCalledWith({
        id: 'department-1',
        name: 'Updated Department'
      });
    });

    it('should NOT update local data when status is not 711', () => {
      departmentApiService.updateDepartment.mockReturnValue(
        of({
          statusCode: 500
        })
      );

      component.departments.set(mockDepartments);
      component.departmentForm.setValue({
        id: 'department-1',
        name: 'Updated Department'
      });
      component.updateDepartment();
      expect(component.departments()).toEqual(mockDepartments);
    });

    it('should handle update API error', () => {
      departmentApiService.updateDepartment.mockReturnValue(
        throwError(() => ({
          statusCode: 500
        }))
      );
      component.departmentForm.setValue({
        id: 'department-1',
        name: 'Updated Department'
      });

      component.updateDepartment();
      expect(component.isSubmitting()).toBe(false);
    });
  });

  describe('closeModals', () => {

    it('should close all modals', () => {
      component.openAddModal();
      component.closeModals();

      expect(component.isAddModalOpen()).toBe(false);
      expect(component.isEditModalOpen()).toBe(false);
      expect(component.isDetailModalOpen()).toBe(false);
      expect(component.isEmployeesModalOpen()).toBe(false);
    });

    it('should clear selected department', () => {
      component.selectedDepartment.set(mockDepartments[0]);
      component.closeModals();

      expect(component.selectedDepartment()).toBe(null);
    });

    it('should reset form', () => {
      component.departmentForm.setValue({
        id: 'department-1',
        name: 'Information Technology'
      });

      component.closeModals();

      expect(component.departmentForm.value).toEqual({ id: '', name: '' });

    });

    it('should reset loading and submitting states', () => {
      component.isModalLoading.set(true);
      component.isSubmitting.set(true);
      component.closeModals();
      expect(component.isModalLoading()).toBe(false);
      expect(component.isSubmitting()).toBe(false);
    });

  });

  describe('controlInvalid', () => {

    it('should return false for untouched valid control', () => {
      component.departmentForm.controls.name.setValue('IT');
      expect(component.controlInvalid('name')).toBe(false);
    });

    it('should return true for touched invalid control', () => {
      const control = component.departmentForm.controls.name;

      control.setValue('');
      control.markAsTouched();

      expect(component.controlInvalid('name')).toBe(true);
    });

    it('should return true for dirty invalid control', () => {
      const control = component.departmentForm.controls.name;

      control.setValue('');
      control.markAsDirty();
      expect(component.controlInvalid('name')).toBe(true);
    });

    it('should return false when invalid but untouched and pristine', () => {
      const control = component.departmentForm.controls.name;
      control.setValue('');

      control.markAsPristine();
      control.markAsUntouched();

      expect(component.controlInvalid('name')).toBe(false);
    });
  });
});
