import { Component, Input, Pipe, PipeTransform } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { TranslatePipe } from '@ngx-translate/core';

import { PositionComponent } from './position.component';
import { GenericTableComponent } from '@app/shared/components/table/generic-table.component';
import { PositionApiService } from '@app/core/services/api-service/position-api.service';
import { DepartmentApiService } from '@app/core/services/api-service/department-api.service';
import { ToastService } from '@app/core/services/toast.service';
import { StorageService } from '@app/core/services/storage.service';

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

describe('PositionComponent', () => {
  let component: PositionComponent;
  let fixture: ComponentFixture<PositionComponent>;

  let positionApiServiceMock: {
    getAllPositions: ReturnType<typeof vi.fn>;
    getPositionEmployees: ReturnType<typeof vi.fn>;
    createPosition: ReturnType<typeof vi.fn>;
    updatePosition: ReturnType<typeof vi.fn>;
  };

  let departmentApiServiceMock: {
    getAllDepartments: ReturnType<typeof vi.fn>;
  };

  let toastServiceMock: {
    show: ReturnType<typeof vi.fn>;
  };

  let storageServiceMock: {
    getItem: ReturnType<typeof vi.fn>;
    setItem: ReturnType<typeof vi.fn>;
    removeItem: ReturnType<typeof vi.fn>;
  };

  const mockPositions = [
    { id: '1', name: 'Software Developer', departmentId: 'd1', departmentName: 'Information Technology', totalUsers: 10 },
    { id: '2', name: 'HR Manager', departmentId: 'd2', departmentName: 'Human Resources', totalUsers: 5 }
  ];

  const mockDepartments = [
    { id: 'd1', name: 'Information Technology' },
    { id: 'd2', name: 'Human Resources' }
  ];

  const mockEmployees = [
    { userId: 'user-1', name: 'Harsh Donda', email: 'harsh@test.com', branchName: 'Main Branch', roleName: 'Employee' }
  ];

  beforeEach(async () => {
    positionApiServiceMock = {
      getAllPositions: vi.fn().mockReturnValue(of({ data: [] })),
      getPositionEmployees: vi.fn().mockReturnValue(of({ data: [] })),
      createPosition: vi.fn().mockReturnValue(of({ data: [] })),
      updatePosition: vi.fn().mockReturnValue(of({ statusCode: 706 }))
    };

    departmentApiServiceMock = {
      getAllDepartments: vi.fn().mockReturnValue(of({ data: [] }))
    };

    toastServiceMock = {
      show: vi.fn()
    };

    storageServiceMock = {
      getItem: vi.fn().mockReturnValue(null),
      setItem: vi.fn(),
      removeItem: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [PositionComponent],
      providers: [
        { provide: PositionApiService, useValue: positionApiServiceMock },
        { provide: DepartmentApiService, useValue: departmentApiServiceMock },
        { provide: ToastService, useValue: toastServiceMock },
        { provide: StorageService, useValue: storageServiceMock }
      ]
    }).overrideComponent(PositionComponent, {
      remove: { imports: [GenericTableComponent, TranslatePipe] },
      add: { imports: [MockGenericTableComponent, MockTranslatePipe] }
    }).compileComponents();

    fixture = TestBed.createComponent(PositionComponent);
    component = fixture.componentInstance;
  });

  describe('Unit tests', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should load positions from storage when cached positions exist', () => {
      storageServiceMock.getItem.mockReturnValue(mockPositions);

      component.loadPositions();

      expect(component.positions()).toEqual(mockPositions);
      expect(positionApiServiceMock.getAllPositions).not.toHaveBeenCalled();
      expect(component.isPageLoading()).toBe(false);
    });

    it('should load positions from API when cached positions are undefined', () => {
      storageServiceMock.getItem.mockReturnValue(undefined);

      positionApiServiceMock.getAllPositions.mockReturnValue(
        of({ data: mockPositions })
      );

      component.loadPositions();

      expect(positionApiServiceMock.getAllPositions).toHaveBeenCalled();
      expect(component.positions()).toEqual(mockPositions);
      expect(component.isPageLoading()).toBe(false);
    });

    it('should load positions from API when no cached positions exist', () => {
      positionApiServiceMock.getAllPositions.mockReturnValue(of({ data: mockPositions }));

      component.loadPositions();

      expect(positionApiServiceMock.getAllPositions).toHaveBeenCalled();
      expect(component.positions()).toEqual(mockPositions);
      expect(storageServiceMock.setItem).toHaveBeenCalledWith('positions', mockPositions);
      expect(component.isPageLoading()).toBe(false);
    });

    it('should handle empty positions response', () => {
      positionApiServiceMock.getAllPositions.mockReturnValue(of({ data: null }));

      component.loadPositions();

      expect(component.positions()).toEqual([]);
      expect(storageServiceMock.setItem).toHaveBeenCalledWith('positions', []);
    });

    it('should use zero totalUsers when no position is selected', () => {
      component.positions.set(mockPositions);
      component.selectedPosition.set(null);

      component.positionForm.setValue({
        id: '1',
        name: 'Senior Developer',
        departmentId: 'd1'
      });

      positionApiServiceMock.updatePosition.mockReturnValue(
        of({ statusCode: 706 })
      );

      vi.spyOn(component, 'loadPositions').mockImplementation(() => { });

      component.updatePosition();

      const updatedPosition = component.positions().find(
        p => p.id === '1'
      );

      expect(updatedPosition?.totalUsers).toBe(0);
    });

    it('should handle load positions error', () => {
      positionApiServiceMock.getAllPositions.mockReturnValue(throwError(() => ({ statusCode: 500 })));

      component.loadPositions();

      expect(component.positions()).toEqual([]);
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.isPageLoading()).toBe(false);
    });

    it('should load departments from storage when cached departments exist', () => {
      storageServiceMock.getItem.mockImplementation((key: string) => key === 'departments' ? mockDepartments : null);

      component.loadDepartments();

      expect(component.departments()).toEqual(mockDepartments);
      expect(departmentApiServiceMock.getAllDepartments).not.toHaveBeenCalled();
      expect(component.isModalLoading()).toBe(false);
    });

    it('should load departments from API when no cached departments exist', () => {
      departmentApiServiceMock.getAllDepartments.mockReturnValue(of({ data: mockDepartments }));

      component.loadDepartments();

      expect(departmentApiServiceMock.getAllDepartments).toHaveBeenCalled();
      expect(component.departments()).toEqual(mockDepartments);
      expect(component.isModalLoading()).toBe(false);
    });

    it('should handle load departments error', () => {
      departmentApiServiceMock.getAllDepartments.mockReturnValue(throwError(() => ({ statusCode: 500 })));

      component.loadDepartments();

      expect(component.departments()).toEqual([]);
      expect(storageServiceMock.removeItem).toHaveBeenCalledWith('departments');
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.isModalLoading()).toBe(false);
    });

    it('should open add modal', () => {
      component.openAddModal();

      expect(component.isAddModalOpen()).toBe(true);
      expect(component.isEditModalOpen()).toBe(false);
      expect(component.selectedPosition()).toBe(null);
      expect(component.positionEmployees()).toEqual([]);
      expect(component.positionForm.value).toEqual({ id: '', name: '', departmentId: '' });
      expect(departmentApiServiceMock.getAllDepartments).toHaveBeenCalled();
    });

    it('should open edit modal with position data', () => {
      component.openEditModal(mockPositions[0]);

      expect(component.isEditModalOpen()).toBe(true);
      expect(component.selectedPosition()).toEqual(mockPositions[0]);
      expect(component.positionForm.value).toEqual({ id: '1', name: 'Software Developer', departmentId: 'd1' });
      expect(departmentApiServiceMock.getAllDepartments).toHaveBeenCalled();
    });

    it('should open detail modal', () => {
      component.openDetailModal(mockPositions[0]);

      expect(component.isDetailModalOpen()).toBe(true);
      expect(component.selectedPosition()).toEqual(mockPositions[0]);
      expect(component.positionEmployees()).toEqual([]);
    });

    it('should open employees modal and load employees', () => {
      positionApiServiceMock.getPositionEmployees.mockReturnValue(of({ data: mockEmployees }));

      component.openEmployeesModal('1');

      expect(component.isEmployeesModalOpen()).toBe(true);
      expect(positionApiServiceMock.getPositionEmployees).toHaveBeenCalledWith('1');
      expect(component.positionEmployees()).toEqual(mockEmployees);
      expect(component.isModalLoading()).toBe(false);
    });

    it('should use empty name when position name is undefined', () => {
      const position = {
        ...mockPositions[0],
        name: undefined
      };

      vi.spyOn(component, 'loadDepartments').mockImplementation(() => { });

      component.openEditModal(position);

      expect(component.positionForm.controls.name.value).toBe('');
    });

    it('should handle employees loading error', () => {
      positionApiServiceMock.getPositionEmployees.mockReturnValue(throwError(() => ({ statusCode: 500 })));

      component.openEmployeesModal('1');

      expect(component.positionEmployees()).toEqual([]);
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.isModalLoading()).toBe(false);
    });

    it('should not create position when form is invalid', () => {
      component.openAddModal();
      component.createPosition();

      expect(positionApiServiceMock.createPosition).not.toHaveBeenCalled();
      expect(component.positionForm.touched).toBe(true);
    });

    it('should create position with trimmed name', () => {
      component.positionForm.setValue({ id: '', name: '  Software Developer  ', departmentId: 'd1' });

      component.createPosition();

      expect(positionApiServiceMock.createPosition).toHaveBeenCalledWith({ name: 'Software Developer', departmentId: 'd1' });
    });

    it('should handle successful position creation', () => {
      positionApiServiceMock.createPosition.mockReturnValue(of({ statusCode: 704 }));
      positionApiServiceMock.getAllPositions.mockReturnValue(of({ data: mockPositions }));
      component.openAddModal();
      component.positionForm.setValue({ id: '', name: 'Developer', departmentId: 'd1' });

      component.createPosition();

      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(storageServiceMock.removeItem).toHaveBeenCalledWith('positions');
      expect(component.isAddModalOpen()).toBe(false);
      expect(component.isSubmitting()).toBe(false);
    });

    it('should not close modal when create response status is not 704', () => {
      positionApiServiceMock.createPosition.mockReturnValue(of({ statusCode: 500, data: null }));
      component.openAddModal();
      component.positionForm.setValue({ id: '', name: 'Developer', departmentId: 'd1' });

      component.createPosition();

      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.isAddModalOpen()).toBe(true);
    });

    it('should handle create position error', () => {
      positionApiServiceMock.createPosition.mockReturnValue(throwError(() => ({ statusCode: 500 })));
      component.openAddModal();
      component.positionForm.setValue({ id: '', name: 'Developer', departmentId: 'd1' });

      component.createPosition();

      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.isSubmitting()).toBe(false);
    });

    it('should not update position when form is invalid', () => {
      component.openEditModal(mockPositions[0]);
      component.positionForm.controls.name.setValue('');

      component.updatePosition();

      expect(positionApiServiceMock.updatePosition).not.toHaveBeenCalled();
    });

    it('should show error when position id is missing', () => {
      component.openEditModal(mockPositions[0]);
      component.positionForm.controls.id.setValue('');

      component.updatePosition();

      expect(positionApiServiceMock.updatePosition).not.toHaveBeenCalled();
      expect(toastServiceMock.show).toHaveBeenCalledWith('Position id is missing.');
    });

    it('should update position with valid form', () => {
      positionApiServiceMock.updatePosition.mockReturnValue(of({ statusCode: 706 }));
      positionApiServiceMock.getAllPositions.mockReturnValue(of({ data: mockPositions }));
      component.openEditModal(mockPositions[0]);
      component.positionForm.setValue({ id: '1', name: 'Senior Developer', departmentId: 'd1' });

      component.updatePosition();

      expect(positionApiServiceMock.updatePosition).toHaveBeenCalledWith({ id: '1', name: 'Senior Developer', departmentId: 'd1' });
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(storageServiceMock.setItem).toHaveBeenCalled();
      expect(component.isSubmitting()).toBe(false);
    });

    it('should replace the existing position with the updated position', () => {
      const positions = mockPositions;
      component.positions.set(positions);
      component.selectedPosition.set(positions[0]);

      component.positionForm.setValue({
        id: '1',
        name: 'Senior Developer',
        departmentId: 'department-1',
      });

      positionApiServiceMock.updatePosition.mockReturnValue(of({ statusCode: 706 }));

      vi.spyOn(component, 'loadPositions').mockImplementation(() => { });

      component.updatePosition();

      expect(component.positions()).toEqual([
        {
          id: '2',
          name: 'HR Manager',
          departmentId: 'd2',
          departmentName: 'Human Resources',
          totalUsers: 5
        },
        {
          id: '1',
          name: 'Senior Developer',
          departmentId: 'department-1',
          totalUsers: 10
        }
      ]);
    });

    it('should handle update position error', () => {
      positionApiServiceMock.updatePosition.mockReturnValue(throwError(() => ({ statusCode: 500 })));
      component.openEditModal(mockPositions[0]);
      component.positionForm.setValue({ id: '1', name: 'Senior Developer', departmentId: 'd1' });

      component.updatePosition();

      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.isSubmitting()).toBe(false);
    });

    it('should close all modals and reset state', () => {
      component.openEditModal(mockPositions[0]);
      component.closeModals();

      expect(component.isAddModalOpen()).toBe(false);
      expect(component.isEditModalOpen()).toBe(false);
      expect(component.isDetailModalOpen()).toBe(false);
      expect(component.isEmployeesModalOpen()).toBe(false);
      expect(component.selectedPosition()).toBe(null);
      expect(component.positionEmployees()).toEqual([]);
      expect(component.isModalLoading()).toBe(false);
      expect(component.isSubmitting()).toBe(false);
      expect(component.positionForm.value).toEqual({ id: '', name: '', departmentId: '' });
    });

    it('should return true for invalid touched control', () => {
      component.positionForm.controls.name.markAsTouched();

      expect(component.controlInvalid('name')).toBe(true);
    });

    it('should return false for valid control', () => {
      component.positionForm.controls.name.setValue('Developer');

      expect(component.controlInvalid('name')).toBe(false);
    });

    it('should return position employees table data', () => {
      component.positionEmployees.set(mockEmployees as any);

      expect(component.positionEmployeesTableData()).toEqual(mockEmployees);
    });
  });

  describe('Integration tests', () => {
    it('should load positions when component initializes', () => {
      positionApiServiceMock.getAllPositions.mockReturnValue(of({ data: mockPositions }));

      fixture.detectChanges();

      expect(positionApiServiceMock.getAllPositions).toHaveBeenCalled();
      expect(component.positions()).toEqual(mockPositions);
    });

    it('should close modal from cancel button', () => {
      component.openAddModal();
      fixture.detectChanges();

      const cancelButton = fixture.nativeElement.querySelector('.modal-footer button[type="button"]') as HTMLButtonElement;
      expect(cancelButton).toBeTruthy();

      cancelButton.click();
      fixture.detectChanges();

      expect(component.isAddModalOpen()).toBe(false);
    });

    it('should display department options in edit modal', () => {
      departmentApiServiceMock.getAllDepartments.mockReturnValue(of({ data: mockDepartments }));
      component.openEditModal(mockPositions[0]);
      fixture.detectChanges();

      const options = fixture.nativeElement.querySelectorAll('#departmentId option');

      expect(options.length).toBe(3);
      expect(fixture.nativeElement.textContent).toContain('Information Technology');
      expect(fixture.nativeElement.textContent).toContain('Human Resources');
    });

    it('should display position name in detail modal', () => {
      component.openDetailModal(mockPositions[0]);
      fixture.detectChanges();

      expect(fixture.nativeElement.textContent).toContain('Software Developer');
      expect(fixture.nativeElement.textContent).toContain('Information Technology');
      expect(fixture.nativeElement.textContent).toContain('10');
    });

    it('should display employees table in employees modal', () => {
      positionApiServiceMock.getPositionEmployees.mockReturnValue(of({ data: mockEmployees }));
      component.openEmployeesModal('1');
      fixture.detectChanges();

      expect(component.isEmployeesModalOpen()).toBe(true);
      expect(component.positionEmployees()).toEqual(mockEmployees as any);
      expect(fixture.nativeElement.querySelector('app-table')).toBeTruthy();
    });

    it('should display empty state when there are no positions', () => {
      component.positions.set([]);
      fixture.detectChanges();

      expect(fixture.nativeElement.textContent).toContain('POSITION.NO_POSITIONS_FOUND');
    });

    it('should display modal loading state', () => {
      component.openAddModal();
      component.isModalLoading.set(true);
      fixture.detectChanges();

      expect(fixture.nativeElement.querySelector('.modal-body .spinner-border')).toBeTruthy();
      expect(fixture.nativeElement.textContent).toContain('POSITION.LOADING_DEPARTMENTS');
    });

    it('should disable submit button while submitting', () => {
      component.openAddModal();
      component.positionForm.setValue({ id: '', name: 'Developer', departmentId: 'd1' });
      component.isSubmitting.set(true);
      fixture.detectChanges();

      const submitButton = fixture.nativeElement.querySelector('.modal-footer button[type="submit"]') as HTMLButtonElement;

      expect(submitButton).toBeTruthy();
      expect(submitButton.disabled).toBe(true);
    });

    it('should submit create form from template', () => {
      const createPositionSpy = vi.spyOn(component, 'createPosition');
      component.openAddModal();
      component.positionForm.setValue({ id: '', name: 'Developer', departmentId: 'd1' });
      fixture.detectChanges();

      const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
      form.dispatchEvent(new Event('submit'));
      fixture.detectChanges();

      expect(createPositionSpy).toHaveBeenCalled();
    });

    it('should submit update form from template', () => {
      const updatePositionSpy = vi.spyOn(component, 'updatePosition');
      component.openEditModal(mockPositions[0]);
      component.positionForm.setValue({ id: '1', name: 'Senior Developer', departmentId: 'd1' });
      fixture.detectChanges();

      const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
      form.dispatchEvent(new Event('submit'));
      fixture.detectChanges();

      expect(updatePositionSpy).toHaveBeenCalled();
    });

    it('should close modal when backdrop is clicked', () => {
      component.openAddModal();
      fixture.detectChanges();

      const modal = fixture.nativeElement.querySelector('.modal') as HTMLElement;
      modal.click();
      fixture.detectChanges();

      expect(component.isAddModalOpen()).toBe(false);
    });

    it('should not close modal when modal dialog is clicked', () => {
      component.openAddModal();
      fixture.detectChanges();

      const dialog = fixture.nativeElement.querySelector('.modal-dialog') as HTMLElement;
      dialog.click();
      fixture.detectChanges();

      expect(component.isAddModalOpen()).toBe(true);
    });
  });
});
