import { Component, Input, Pipe, PipeTransform } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { TranslatePipe } from '@ngx-translate/core';

import { BranchComponent } from './branch.component';
import { GenericTableComponent } from '@app/shared/components/table/generic-table.component';
import { BranchApiService } from '@app/core/services/api-service/branch-api.service';
import { StorageService } from '@app/core/services/storage.service';

@Pipe({ name: 'translate', standalone: true })
class MockTranslatePipe implements PipeTransform {
  public transform(value: string): string {
    return value;
  }
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

describe('BranchComponent', () => {
  let component: BranchComponent;
  let fixture: ComponentFixture<BranchComponent>;

  let branchApiServiceMock: {
    getAllBranches: ReturnType<typeof vi.fn>;
    getBranchEmployees: ReturnType<typeof vi.fn>;
    createBranch: ReturnType<typeof vi.fn>;
    updateBranch: ReturnType<typeof vi.fn>;
  };

  let storageServiceMock: {
    getItem: ReturnType<typeof vi.fn>;
    setItem: ReturnType<typeof vi.fn>;
    removeItem: ReturnType<typeof vi.fn>;
  };

  const mockBranches = [
    {
      id: '1',
      name: 'Main Branch',
      location: 'Germany',
      totalUsers: 10
    },
    {
      id: '2',
      name: 'Second Branch',
      location: 'Vadodara',
      totalUsers: 5
    }
  ];

  const mockEmployees = [
    {
      id: '1',
      name: 'Harsh Donda',
      email: 'harsh@test.com',
      departmentName: 'IT',
      positionName: 'Developer'
    }
  ];

  beforeEach(async () => {
    branchApiServiceMock = {
      getAllBranches: vi.fn().mockReturnValue(of({ data: [] })),
      getBranchEmployees: vi.fn().mockReturnValue(of({ data: [] })),
      createBranch: vi.fn(),
      updateBranch: vi.fn()
    };

    storageServiceMock = {
      getItem: vi.fn().mockReturnValue(null),
      setItem: vi.fn(),
      removeItem: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [BranchComponent],
      providers: [
        { provide: BranchApiService, useValue: branchApiServiceMock },
        { provide: StorageService, useValue: storageServiceMock }
      ]
    }).overrideComponent(BranchComponent, {
      remove: { imports: [GenericTableComponent, TranslatePipe] },
      add: { imports: [MockGenericTableComponent, MockTranslatePipe] }
    }).compileComponents();

    fixture = TestBed.createComponent(BranchComponent);
    component = fixture.componentInstance;
  });

  describe('Unit tests', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should load branches from storage when cached branches exist', () => {
      storageServiceMock.getItem.mockReturnValue(mockBranches);
      component.loadBranches();
      expect(component.branches()).toEqual(mockBranches);
      expect(branchApiServiceMock.getAllBranches).not.toHaveBeenCalled();
      expect(component.isPageLoading()).toBe(false);
    });

    it('should load branches from API when no cached branches exist', () => {
      branchApiServiceMock.getAllBranches.mockReturnValue(of({
        data: mockBranches
      }));

      component.loadBranches();

      expect(branchApiServiceMock.getAllBranches).toHaveBeenCalled();
      expect(component.branches()).toEqual(mockBranches);
      expect(storageServiceMock.setItem).toHaveBeenCalledWith('branches', mockBranches);
      expect(component.isPageLoading()).toBe(false);
    });

    it('should handle empty branches response', () => {
      branchApiServiceMock.getAllBranches.mockReturnValue(of({
        data: null
      }));

      component.loadBranches();

      expect(component.branches()).toEqual([]);
      expect(storageServiceMock.setItem).toHaveBeenCalledWith('branches', []);
    });

    it('should handle load branches error', () => {
      branchApiServiceMock.getAllBranches.mockReturnValue(
        throwError(() => ({ statusCode: 500 }))
      );
      component.loadBranches();
      expect(component.branches()).toEqual([]);
      expect(component.isPageLoading()).toBe(false);
    });

    it('should open add modal', () => {
      component.openAddModal();
      expect(component.isAddModalOpen()).toBe(true);
      expect(component.isEditModalOpen()).toBe(false);
      expect(component.selectedBranch()).toBe(null);
      expect(component.branchEmployees()).toEqual([]);
      expect(component.branchForm.value).toEqual({
        id: '',
        name: '',
        location: ''
      });
    });

    it('should open edit modal with branch data', () => {
      component.openEditModal(mockBranches[0]);
      expect(component.isEditModalOpen()).toBe(true);
      expect(component.selectedBranch()).toBe(null);
      expect(component.branchForm.value).toEqual({
        id: '1',
        name: 'Main Branch',
        location: 'Germany'
      });
    });

    it('should open detail modal', () => {
      component.openDetailModal(mockBranches[0]);

      expect(component.isDetailModalOpen()).toBe(true);
      expect(component.selectedBranch()).toEqual(mockBranches[0]);
      expect(component.branchEmployees()).toEqual([]);
    });

    it('should open employees modal and load employees', () => {
      branchApiServiceMock.getBranchEmployees.mockReturnValue(of({
        data: mockEmployees
      }));

      component.openEmployeesModal('1');

      expect(component.isEmployeesModalOpen()).toBe(true);
      expect(branchApiServiceMock.getBranchEmployees).toHaveBeenCalledWith('1');
      expect(component.branchEmployees()).toEqual(mockEmployees);
      expect(component.isModalLoading()).toBe(false);
    });

    it('should handle employees loading error', () => {
      branchApiServiceMock.getBranchEmployees.mockReturnValue(
        throwError(() => ({ statusCode: 500 }))
      );

      component.openEmployeesModal('1');

      expect(component.branchEmployees()).toEqual([]);
      expect(component.isModalLoading()).toBe(false);
    });

    it('should reject invalid create branch form', () => {
      component.openAddModal();

      component.createBranch();

      expect(branchApiServiceMock.createBranch).not.toHaveBeenCalled();
      expect(component.branchForm.touched).toBe(true);
    });

    it('should create a branch successfully', () => {
      branchApiServiceMock.createBranch.mockReturnValue(of({
        statusCode: 702,
        data: mockBranches[0]
      }));

      branchApiServiceMock.getAllBranches.mockReturnValue(of({
        data: mockBranches
      }));

      component.openAddModal();
      component.branchForm.setValue({
        id: '',
        name: 'New Branch',
        location: 'Chaina'
      });

      component.createBranch();

      expect(branchApiServiceMock.createBranch).toHaveBeenCalledWith({
        name: 'New Branch',
        location: 'Chaina'
      });

      expect(storageServiceMock.removeItem).toHaveBeenCalledWith('branches');
      expect(component.isSubmitting()).toBe(false);
    });

    it('should not close modal when create branch returns unsuccessful status', () => {
      branchApiServiceMock.createBranch.mockReturnValue(of({
        statusCode: 500,
        data: null
      }));

      component.openAddModal();
      component.branchForm.setValue({
        id: '',
        name: 'New Branch',
        location: 'Chaina'
      });

      component.createBranch();

      expect(component.isAddModalOpen()).toBe(true);
    });

    it('should handle create branch error', () => {
      branchApiServiceMock.createBranch.mockReturnValue(
        throwError(() => ({ statusCode: 500 }))
      );

      component.openAddModal();
      component.branchForm.setValue({
        id: '',
        name: 'New Branch',
        location: 'Chaina'
      });

      component.createBranch();

      expect(component.isSubmitting()).toBe(false);
    });

    it('should reject update when form is invalid', () => {
      component.openEditModal(mockBranches[0]);
      component.branchForm.controls.name.setValue('');

      component.updateBranch();

      expect(branchApiServiceMock.updateBranch).not.toHaveBeenCalled();
    });

    it('should reject update when branch id is missing', () => {
      component.openEditModal(mockBranches[0]);
      component.branchForm.controls.id.setValue('');

      component.updateBranch();

      expect(branchApiServiceMock.updateBranch).not.toHaveBeenCalled();
    });

    it('should update a branch successfully', () => {
      branchApiServiceMock.updateBranch.mockReturnValue(of({
        statusCode: 712,
        data: null
      }));

      branchApiServiceMock.getAllBranches.mockReturnValue(of({
        data: mockBranches
      }));

      component.openEditModal(mockBranches[0]);
      component.branchForm.setValue({
        id: '1',
        name: 'Updated Branch',
        location: 'Surat'
      });

      component.updateBranch();

      expect(branchApiServiceMock.updateBranch).toHaveBeenCalledWith({
        id: '1',
        name: 'Updated Branch',
        location: 'Surat'
      });

      expect(storageServiceMock.setItem).toHaveBeenCalled();
      expect(component.isSubmitting()).toBe(false);
    });

    it('should handle update branch error', () => {
      branchApiServiceMock.updateBranch.mockReturnValue(
        throwError(() => ({ statusCode: 500 }))
      );

      component.openEditModal(mockBranches[0]);
      component.branchForm.setValue({
        id: '1',
        name: 'Updated Branch',
        location: 'Surat'
      });

      component.updateBranch();

      expect(component.isSubmitting()).toBe(false);
    });

    it('should close all modals and reset state', () => {
      component.openEditModal(mockBranches[0]);
      component.closeModals();

      expect(component.isAddModalOpen()).toBe(false);
      expect(component.isEditModalOpen()).toBe(false);
      expect(component.isDetailModalOpen()).toBe(false);
      expect(component.isEmployeesModalOpen()).toBe(false);
      expect(component.selectedBranch()).toBe(null);
      expect(component.branchEmployees()).toEqual([]);
      expect(component.isModalLoading()).toBe(false);
      expect(component.isSubmitting()).toBe(false);
      expect(component.branchForm.value).toEqual({
        id: '',
        name: '',
        location: ''
      });
    });

    it('should return true when form control is invalid and touched', () => {
      component.branchForm.controls.name.markAsTouched();

      expect(component.controlInvalid('name')).toBe(true);
    });

    it('should return false when form control is valid', () => {
      component.branchForm.controls.name.setValue('Main Branch');

      expect(component.controlInvalid('name')).toBe(false);
    });
  });

  describe('Integration tests', () => {
    it('should load branches when component initializes', () => {
      branchApiServiceMock.getAllBranches.mockReturnValue(of({
        data: mockBranches
      }));

      fixture.detectChanges();

      expect(branchApiServiceMock.getAllBranches).toHaveBeenCalled();
      expect(component.branches()).toEqual(mockBranches);
    });

    it('should display no branches message when branches are empty', () => {
      component.branches.set([]);
      fixture.detectChanges();

      expect(fixture.nativeElement.textContent).toContain('BRANCHES.NO_BRANCHES');
    });

    it('should open add modal from template', () => {
      fixture.detectChanges();

      const button = fixture.nativeElement.querySelector(
        'button.btn-dark'
      ) as HTMLButtonElement;

      button.click();
      fixture.detectChanges();

      expect(component.isAddModalOpen()).toBe(true);
      expect(fixture.nativeElement.textContent).toContain('BRANCHES.ADD_BRANCH');
    });

    it('should display branch form validation message', () => {
      component.openAddModal();
      component.branchForm.controls.name.markAsTouched();
      fixture.detectChanges();

      expect(fixture.nativeElement.textContent).toContain('BRANCHES.NAME_REQUIRED');
    });
  });
});
