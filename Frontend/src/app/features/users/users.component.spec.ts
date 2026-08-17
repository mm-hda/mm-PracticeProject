import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, Pipe, PipeTransform, signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { UsersComponent } from './users.component';
import { UserApiService } from '@app/core/services/api-service/user-api.service';
import { RoleApiService } from '@app/core/services/api-service/role-api.service';
import { BranchApiService } from '@app/core/services/api-service/branch-api.service';
import { DepartmentApiService } from '@app/core/services/api-service/department-api.service';
import { PositionApiService } from '@app/core/services/api-service/position-api.service';
import { StorageService } from '@app/core/services/storage.service';
import { ToastService } from '@app/core/services/toast.service';
import { AuthService } from '@app/core/services/auth.service';
import { OfflineQueueService } from '@app/core/services/offline-queue.service';
import { TranslatePipe } from '@ngx-translate/core';
import { UserResponse } from '@app/core/models/userModels/user.model';

@Pipe({ name: 'translate', standalone: true })
class MockTranslatePipe implements PipeTransform {
  public transform(value: string): string { return value; }
}

describe('UsersComponent', () => {
  let component: UsersComponent;
  let fixture: ComponentFixture<UsersComponent>;
  let userApiServiceMock: {
    getAllUsers: ReturnType<typeof vi.fn>;
    getUserBySearch: ReturnType<typeof vi.fn>;
    getUsersByFilter: ReturnType<typeof vi.fn>;
    createUser: ReturnType<typeof vi.fn>;
  };
  let roleApiServiceMock: { getAllRoles: ReturnType<typeof vi.fn>; };
  let branchApiServiceMock: { getAllBranches: ReturnType<typeof vi.fn>; };
  let departmentApiServiceMock: { getAllDepartments: ReturnType<typeof vi.fn>; };
  let positionApiServiceMock: { getAllPositions: ReturnType<typeof vi.fn>; };
  let storageServiceMock: {
    getItem: ReturnType<typeof vi.fn>;
    setItem: ReturnType<typeof vi.fn>;
    removeItem: ReturnType<typeof vi.fn>;
  };
  let toastServiceMock: { show: ReturnType<typeof vi.fn>; };
  let authServiceMock: { currentUser: ReturnType<typeof signal> };
  let offlineQueueServiceMock: {
    getPendingUserRequests: ReturnType<typeof vi.fn>;
    addRequest: ReturnType<typeof vi.fn>;
  };

  const mockUsers = [
    {
      userId: 'user-1',
      name: 'Harsh Donda',
      email: 'harsh@test.com',
      branchName: 'Main Branch',
      departmentName: 'IT',
      positionName: 'Developer',
      roleName: 'Employee'
    },
    {
      userId: 'user-2',
      name: 'John Doe',
      email: 'john@test.com',
      branchName: 'Branch 2',
      departmentName: 'HR',
      positionName: 'Manager',
      roleName: 'HR'
    }
  ] as any;

  const mockRoles = [
    { id: 'role-1', name: 'Employee' },
    { id: 'role-2', name: 'HR' }
  ] as any;

  const mockBranches = [
    { id: 'branch-1', name: 'Main Branch' },
    { id: 'branch-2', name: 'Branch 2' }
  ] as any;

  const mockDepartments = [
    { id: 'department-1', name: 'IT' },
    { id: 'department-2', name: 'HR' }
  ] as any;

  const mockPositions = [
    { id: 'position-1', name: 'Developer', departmentId: 'department-1' },
    { id: 'position-2', name: 'Manager', departmentId: 'department-2' }
  ] as any;

  beforeEach(async () => {
    userApiServiceMock = {
      getAllUsers: vi.fn().mockReturnValue(of({ data: [], meta: { totalPages: 1, totalRecords: 0 } })),
      getUserBySearch: vi.fn().mockReturnValue(of({ data: [] })),
      getUsersByFilter: vi.fn().mockReturnValue(of({ data: [] })),
      createUser: vi.fn().mockReturnValue(of({ statusCode: 704 }))
    };

    roleApiServiceMock = {
      getAllRoles: vi.fn().mockReturnValue(of({ data: [] }))
    };

    branchApiServiceMock = {
      getAllBranches: vi.fn().mockReturnValue(of({ data: [] }))
    };

    departmentApiServiceMock = {
      getAllDepartments: vi.fn().mockReturnValue(of({ data: [] }))
    };

    positionApiServiceMock = {
      getAllPositions: vi.fn().mockReturnValue(of({ data: [] }))
    };

    storageServiceMock = {
      getItem: vi.fn().mockReturnValue(null),
      setItem: vi.fn(),
      removeItem: vi.fn()
    };

    toastServiceMock = {
      show: vi.fn()
    };

    authServiceMock = {
      currentUser: signal({ userId: 'admin-1', role: 'Admin', branch: 'Main Branch' })
    };

    offlineQueueServiceMock = {
      getPendingUserRequests: vi.fn().mockResolvedValue([]),
      addRequest: vi.fn().mockResolvedValue(undefined)
    };

    await TestBed.configureTestingModule({
      imports: [UsersComponent],
      providers: [
        { provide: UserApiService, useValue: userApiServiceMock },
        { provide: RoleApiService, useValue: roleApiServiceMock },
        { provide: BranchApiService, useValue: branchApiServiceMock },
        { provide: DepartmentApiService, useValue: departmentApiServiceMock },
        { provide: PositionApiService, useValue: positionApiServiceMock },
        { provide: StorageService, useValue: storageServiceMock },
        { provide: ToastService, useValue: toastServiceMock },
        { provide: AuthService, useValue: authServiceMock },
        { provide: OfflineQueueService, useValue: offlineQueueServiceMock }
      ]
    }).overrideComponent(UsersComponent, {
      add: { imports: [MockTranslatePipe] },
      remove: { imports: [TranslatePipe] }
    }).compileComponents();

    fixture = TestBed.createComponent(UsersComponent);
    component = fixture.componentInstance;
  });

  describe('Unit tests', () => {
    it('should create', () => { expect(component).toBeTruthy(); });

    it('should load users successfully', async () => {
      userApiServiceMock.getAllUsers.mockReturnValue(of({ data: mockUsers, meta: { totalPages: 2, totalRecords: 2 } }));
      offlineQueueServiceMock.getPendingUserRequests.mockResolvedValue([]);
      component.loadUsers();
      await Promise.resolve();
      expect(component.users()).toEqual(mockUsers);
      expect(component.totalPages()).toBe(2);
      expect(component.totalRecords()).toBe(2);
      expect(component.isPageLoading()).toBe(false);
    });

    it('should use cached page when page is already cached', () => {
      component.users.set(mockUsers);
      component['pageCache'].set(new Map([[1, mockUsers]]));
      component.loadUsers();
      expect(userApiServiceMock.getAllUsers).not.toHaveBeenCalled();
      expect(component.users()).toEqual(mockUsers);
    });

    it('should handle load users error', () => {
      userApiServiceMock.getAllUsers.mockReturnValue(throwError(() => ({ statusCode: 500 })));
      component.loadUsers();
      expect(component.users()).toEqual([]);
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.isPageLoading()).toBe(false);
    });

    it('should merge pending users with API users', async () => {
      userApiServiceMock.getAllUsers.mockReturnValue(of({ data: mockUsers, meta: { totalPages: 1, totalRecords: 2 } }));
      offlineQueueServiceMock.getPendingUserRequests.mockResolvedValue([{ id: 'request-1', payload: { firstName: 'Pending', lastName: 'User', email: 'pending@test.com' } }]);
      component.loadUsers();
      await Promise.resolve();
      expect(component.users()).toHaveLength(3);
      expect(component.users()[0]).toMatchObject({ userId: 'pending-request-1', name: 'Pending User', syncStatus: 'pending' });
    });

    it('should load cached lookup data', () => {
      storageServiceMock.getItem.mockImplementation((key: string) => ({ roles: mockRoles, branches: mockBranches, departments: mockDepartments, positions: mockPositions }[key] ?? null));
      component.loadLookupData();
      expect(component.roles()).toEqual(mockRoles);
      expect(component.branches()).toEqual(mockBranches);
      expect(component.departments()).toEqual(mockDepartments);
      expect(component.positions()).toEqual(mockPositions);
      expect(roleApiServiceMock.getAllRoles).not.toHaveBeenCalled();
      expect(branchApiServiceMock.getAllBranches).not.toHaveBeenCalled();
      expect(departmentApiServiceMock.getAllDepartments).not.toHaveBeenCalled();
      expect(positionApiServiceMock.getAllPositions).not.toHaveBeenCalled();
    });

    it('should load lookup data from APIs when cache is empty', () => {
      roleApiServiceMock.getAllRoles.mockReturnValue(of({ data: mockRoles }));
      branchApiServiceMock.getAllBranches.mockReturnValue(of({ data: mockBranches }));
      departmentApiServiceMock.getAllDepartments.mockReturnValue(of({ data: mockDepartments }));
      positionApiServiceMock.getAllPositions.mockReturnValue(of({ data: mockPositions }));
      component.loadLookupData();
      expect(component.roles()).toEqual(mockRoles);
      expect(component.branches()).toEqual(mockBranches);
      expect(component.departments()).toEqual(mockDepartments);
      expect(component.positions()).toEqual(mockPositions);
      expect(storageServiceMock.setItem).toHaveBeenCalled();
    });

    it('should use empty arrays when lookup API responses contain no data', () => {
      component.loadLookupData();
      expect(component.roles()).toEqual([]);
      expect(component.branches()).toEqual([]);
      expect(component.departments()).toEqual([]);
      expect(component.positions()).toEqual([]);
    });

    it('should filter positions by department', () => {
      component.positions.set(mockPositions);
      component.userForm.controls.departmentId.setValue('department-1');
      component.filterPositions();
      expect(component.DepartmentPositions()).toEqual([mockPositions[0]]);
    });

    it('should return no positions when department does not match', () => {
      component.positions.set(mockPositions);
      component.userForm.controls.departmentId.setValue('unknown');
      component.filterPositions();
      expect(component.DepartmentPositions()).toEqual([]);
    });

    it('should load users when search term is empty', () => {
      const loadUsersSpy = vi.spyOn(component, 'loadUsers');
      component.filterForm.controls.searchTerm.setValue('   ');
      component.searchUsers();
      expect(loadUsersSpy).toHaveBeenCalled();
      expect(userApiServiceMock.getUserBySearch).not.toHaveBeenCalled();
    });

    it('should search users successfully', () => {
      userApiServiceMock.getUserBySearch.mockReturnValue(of({ data: mockUsers }));
      component.filterForm.controls.searchTerm.setValue('Harsh');
      component.searchUsers();
      expect(userApiServiceMock.getUserBySearch).toHaveBeenCalledWith({ searchTerm: 'Harsh' });
      expect(component.users()).toEqual(mockUsers);
      expect(component.isPageLoading()).toBe(false);
    });

    it('should handle search error', () => {
      userApiServiceMock.getUserBySearch.mockReturnValue(throwError(() => ({ statusCode: 500 })));
      component.filterForm.controls.searchTerm.setValue('Harsh');
      component.searchUsers();
      expect(component.users()).toEqual([]);
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.isPageLoading()).toBe(false);
    });

    it('should apply filters with selected values', () => {
      component.filterForm.setValue({ searchTerm: '', roleId: 'role-1', branchId: 'branch-1', departmentId: 'department-1', positionId: 'position-1' });
      component.applyFilters();
      expect(userApiServiceMock.getUsersByFilter).toHaveBeenCalledWith({ roleId: 'role-1', branchId: 'branch-1', departmentId: 'department-1', positionId: 'position-1' });
    });

    it('should apply filters with undefined values when filters are empty', () => {
      component.applyFilters();
      expect(userApiServiceMock.getUsersByFilter).toHaveBeenCalledWith({ roleId: undefined, branchId: undefined, departmentId: undefined, positionId: undefined });
    });

    it('should handle filter error', () => {
      userApiServiceMock.getUsersByFilter.mockReturnValue(throwError(() => ({ statusCode: 500 })));
      component.applyFilters();
      expect(component.users()).toEqual([]);
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.isPageLoading()).toBe(false);
    });

    it('should reset filters and load first page', () => {
      component.currentPage.set(3);
      const loadUsersSpy = vi.spyOn(component, 'loadUsers');
      component.resetFilters();
      expect(component.currentPage()).toBe(1);
      expect(loadUsersSpy).toHaveBeenCalled();
      expect(component.filterForm.getRawValue()).toEqual({ searchTerm: '', roleId: '', branchId: '', departmentId: '', positionId: '' });
    });

    it('should not move to next page when current page is last page', () => {
      component.currentPage.set(2);
      component.totalPages.set(2);
      const loadUsersSpy = vi.spyOn(component, 'loadUsers');
      component.nextPage();
      expect(component.currentPage()).toBe(2);
      expect(loadUsersSpy).not.toHaveBeenCalled();
    });

    it('should move to next page', () => {
      component.currentPage.set(1);
      component.totalPages.set(2);
      const loadUsersSpy = vi.spyOn(component, 'loadUsers');
      component.nextPage();
      expect(component.currentPage()).toBe(2);
      expect(loadUsersSpy).toHaveBeenCalled();
    });

    it('should not move to previous page when on first page', () => {
      component.currentPage.set(1);
      const loadUsersSpy = vi.spyOn(component, 'loadUsers');
      component.previousPage();
      expect(component.currentPage()).toBe(1);
      expect(loadUsersSpy).not.toHaveBeenCalled();
    });

    it('should move to previous page', () => {
      component.currentPage.set(2);
      const loadUsersSpy = vi.spyOn(component, 'loadUsers');
      component.previousPage();
      expect(component.currentPage()).toBe(1);
      expect(loadUsersSpy).toHaveBeenCalled();
    });

    it('should open add modal with empty form when no draft exists', () => {
      component.draftUser.set(null);
      component.openAddModal();
      expect(component.isAddModalOpen()).toBe(true);
      expect(component.userForm.getRawValue()).toEqual({ firstName: '', lastName: '', email: '', password: '', dob: null, branchId: '', departmentId: '', positionId: '', roleId: '' });
    });

    it('should open add modal with draft data', () => {
      const draft = { firstName: 'Harsh', lastName: 'Donda', email: 'harsh@test.com', password: '123456', dob: null, branchId: 'b1', departmentId: 'd1', positionId: 'p1', roleId: 'r1' };
      component.draftUser.set(draft);
      component.openAddModal();
      expect(component.isAddModalOpen()).toBe(true);
      expect(component.userForm.getRawValue()).toEqual(draft);
    });

    it('should open detail modal', () => {
      component.openDetailModal(mockUsers[0]);
      expect(component.selectedUser()).toEqual(mockUsers[0]);
      expect(component.isDetailModalOpen()).toBe(true);
    });

    it('should save current form as draft and close modals', () => {
      component.userForm.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@test.com'
      });
      component.selectedUser.set({ userId: 'u1' } as UserResponse);

      component.closeModals();

      expect(component.draftUser()?.firstName).toBe('John');
      expect(component.draftUser()?.lastName).toBe('Doe');
      expect(component.selectedUser()).toBeNull();
      expect(component.isAddModalOpen()).toBe(false);
      expect(component.isDetailModalOpen()).toBe(false);
    });

    it('should mark form as touched when create form is invalid', async () => {
      const markAllAsTouchedSpy = vi.spyOn(component.userForm, 'markAllAsTouched');
      await component.createUser();
      expect(markAllAsTouchedSpy).toHaveBeenCalled();
      expect(userApiServiceMock.createUser).not.toHaveBeenCalled();
    });

    it('should create user successfully when online', async () => {
      Object.defineProperty(navigator, 'onLine', { configurable: true, value: true });
      component.userForm.setValue({ firstName: 'Harsh', lastName: 'Donda', email: 'harsh@test.com', password: '123456', dob: null, branchId: 'b1', departmentId: 'd1', positionId: 'p1', roleId: 'r1' });
      const loadUsersSpy = vi.spyOn(component, 'loadUsers').mockImplementation(() => { });
      const loadLookupDataSpy = vi.spyOn(component, 'loadLookupData').mockImplementation(() => { });
      await component.createUser();
      expect(userApiServiceMock.createUser).toHaveBeenCalledWith(expect.objectContaining({ firstName: 'Harsh', lastName: 'Donda', email: 'harsh@test.com' }));
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(loadUsersSpy).toHaveBeenCalled();
      expect(loadLookupDataSpy).toHaveBeenCalled();
      expect(component.isSubmitting()).toBe(false);
    });

    it('should create user successfully when online', async () => {
      vi.spyOn(navigator, 'onLine', 'get').mockReturnValue(true);

      const request = {
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@test.com',
        password: 'password123',
        dob: null,
        branchId: 'b1',
        departmentId: 'd1',
        positionId: 'p1',
        roleId: 'r1'
      };

      component.userForm.setValue(request);
      userApiServiceMock.createUser.mockReturnValue(of({ statusCode: 704 }));

      const loadUsersSpy = vi.spyOn(component, 'loadUsers').mockImplementation(() => { });
      const loadLookupDataSpy = vi.spyOn(component, 'loadLookupData').mockImplementation(() => { });

      await component.createUser();

      expect(userApiServiceMock.createUser).toHaveBeenCalledWith(request);
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(loadUsersSpy).toHaveBeenCalled();
      expect(loadLookupDataSpy).toHaveBeenCalled();
    });

    it('should handle create user error', async () => {
      Object.defineProperty(navigator, 'onLine', { configurable: true, value: true });
      userApiServiceMock.createUser.mockReturnValue(throwError(() => ({ statusCode: 500 })));
      component.userForm.setValue({ firstName: 'Harsh', lastName: 'Donda', email: 'harsh@test.com', password: '123456', dob: null, branchId: 'b1', departmentId: 'd1', positionId: 'p1', roleId: 'r1' });
      await component.createUser();
      expect(toastServiceMock.show).toHaveBeenCalled();
      expect(component.isSubmitting()).toBe(false);
    });

    it('should queue user when offline', async () => {
      Object.defineProperty(navigator, 'onLine', { configurable: true, value: false });
      component.userForm.setValue({ firstName: 'Harsh', lastName: 'Donda', email: 'harsh@test.com', password: '123456', dob: null, branchId: 'b1', departmentId: 'd1', positionId: 'p1', roleId: 'r1' });
      await component.createUser();
      expect(offlineQueueServiceMock.addRequest).toHaveBeenCalledWith('create-user', expect.objectContaining({ firstName: 'Harsh', lastName: 'Donda' }));
      expect(component.users()[0]).toMatchObject({ name: 'Harsh Donda', email: 'harsh@test.com', syncStatus: 'pending' });
      expect(toastServiceMock.show).toHaveBeenCalledWith('Internet unavailable. User queued for sync.');
      expect(userApiServiceMock.createUser).not.toHaveBeenCalled();
    });
  });

  describe('Integration tests', () => {
    it('should render component', () => {
      fixture.detectChanges();
      expect(fixture.nativeElement).toBeTruthy();
    });

    it('should display no users message when users list is empty', () => {
      component.isPageLoading.set(false);
      component.users.set([]);
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('USER.NO_USERS_FOUND');
    });

    it('should display pending sync badge', () => {
      component.isPageLoading.set(false);
      component.users.set([{ ...mockUsers[0], syncStatus: 'pending' }]);
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('USER.PENDING_SYNC');
    });

    it('should display synced badge', () => {
      component.isPageLoading.set(false);
      component.users.set([{ ...mockUsers[0], syncStatus: 'synced' }]);
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('USER.SYNCED');
    });

    it('should open add modal from add button', () => {
      fixture.detectChanges();
      const button = fixture.nativeElement.querySelector('button.btn-dark');
      button.click();
      fixture.detectChanges();
      expect(component.isAddModalOpen()).toBe(true);
    });

    it('should close add modal when backdrop is clicked', () => {
      component.openAddModal();
      fixture.detectChanges();
      const modal = fixture.nativeElement.querySelector('.modal.fade.show.d-block');
      modal.click();
      expect(component.isAddModalOpen()).toBe(false);
    });

    it('should not close add modal when modal dialog is clicked', () => {
      component.openAddModal();
      fixture.detectChanges();
      const dialog = fixture.nativeElement.querySelector('.modal-dialog');
      dialog.click();
      expect(component.isAddModalOpen()).toBe(true);
    });

    it('should open detail modal from detail button', () => {
      component.isPageLoading.set(false);
      component.users.set(mockUsers);
      fixture.detectChanges();
      const button = fixture.nativeElement.querySelector('button[title="User Detail"]');
      button.click();
      fixture.detectChanges();
      expect(component.isDetailModalOpen()).toBe(true);
      expect(component.selectedUser()).toEqual(mockUsers[0]);
    });

    it('should display selected user details', () => {
      component.openDetailModal(mockUsers[0]);
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('Harsh Donda');
      expect(fixture.nativeElement.textContent).toContain('harsh@test.com');
      expect(fixture.nativeElement.textContent).toContain('Main Branch');
      expect(fixture.nativeElement.textContent).toContain('Developer');
      expect(fixture.nativeElement.textContent).toContain('Employee');
    });

    it('should display user not found when detail modal has no selected user', () => {
      component['activeModal'].set('detail');
      component.selectedUser.set(null);
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('USER.NOT_FOUND');
    });

    it('should close detail modal using close button', () => {
      component.openDetailModal(mockUsers[0]);
      fixture.detectChanges();
      const button = fixture.nativeElement.querySelector('.btn-close');
      button.click();
      fixture.detectChanges();
      expect(component.isDetailModalOpen()).toBe(false);
    });

    it('should render departments and positions', () => {
      component.departments.set(mockDepartments);
      component.DepartmentPositions.set([mockPositions[0]]);
      component.openAddModal();
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('IT');
      expect(fixture.nativeElement.textContent).toContain('Developer');
    });

    it('should call searchUsers when search button is clicked', () => {
      const searchUsersSpy = vi.spyOn(component, 'searchUsers');
      fixture.detectChanges();
      const buttons = fixture.nativeElement.querySelectorAll('button');
      const searchButton = Array.from(buttons).find((button: unknown) => (button as HTMLButtonElement).textContent?.includes('USER.SEARCH')) as HTMLButtonElement;
      searchButton.click();
      expect(searchUsersSpy).toHaveBeenCalled();
    });

    it('should call resetFilters when reset button is clicked', () => {
      const resetFiltersSpy = vi.spyOn(component, 'resetFilters');
      fixture.detectChanges();
      const button = Array.from(fixture.nativeElement.querySelectorAll('button')).find((item: unknown) => (item as HTMLButtonElement).textContent?.includes('USER.RESET')) as HTMLButtonElement;
      button.click();
      expect(resetFiltersSpy).toHaveBeenCalled();
    });

    it('should call applyFilters when filter button is clicked', () => {
      const applyFiltersSpy = vi.spyOn(component, 'applyFilters');
      fixture.detectChanges();
      const button = fixture.nativeElement.querySelector('button.btn-dark i.bi-funnel')?.parentElement as HTMLButtonElement;
      button.click();
      expect(applyFiltersSpy).toHaveBeenCalled();
    });
  });
});
