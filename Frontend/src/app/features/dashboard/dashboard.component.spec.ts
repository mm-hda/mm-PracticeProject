import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Pipe, PipeTransform, signal } from '@angular/core';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { of, Subject } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { DashboardComponent } from './dashboard.component';
import { BranchApiService } from '@app/core/services/api-service/branch-api.service';
import { DepartmentApiService } from '@app/core/services/api-service/department-api.service';
import { PositionApiService } from '@app/core/services/api-service/position-api.service';
import { RoleApiService } from '@app/core/services/api-service/role-api.service';
import { StorageService } from '@app/core/services/storage.service';
import { AuthService } from '@app/core/services/auth.service';

@Pipe({ name: 'translate', standalone: true })
class MockTranslatePipe implements PipeTransform {
  public transform(value: string): string {
    return value;
  }
}

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;

  let branchServiceMock: {
    getAllBranches: ReturnType<typeof vi.fn>;
  };

  let departmentServiceMock: {
    getAllDepartments: ReturnType<typeof vi.fn>;
  };

  let positionServiceMock: {
    getAllPositions: ReturnType<typeof vi.fn>;
  };

  let rolesServiceMock: {
    getAllRoles: ReturnType<typeof vi.fn>;
  };

  let storageServiceMock: {
    removeItem: ReturnType<typeof vi.fn>;
    setItem: ReturnType<typeof vi.fn>;
  };

  let authServiceMock: {
    currentUser: ReturnType<typeof signal>;
  };

  beforeEach(async () => {
    branchServiceMock = {
      getAllBranches: vi.fn()
    };

    departmentServiceMock = {
      getAllDepartments: vi.fn()
    };

    positionServiceMock = {
      getAllPositions: vi.fn()
    };

    rolesServiceMock = {
      getAllRoles: vi.fn()
    };

    storageServiceMock = {
      removeItem: vi.fn(),
      setItem: vi.fn()
    };

    authServiceMock = {
      currentUser: signal({
        userId: '1',
        name: 'Harsh Donda',
        email: 'harsh@test.com',
        role: 'Admin',
        branch: 'Branch 1'
      })
    };

    const translateServiceMock = {
      currentLang: 'en-US',
      defaultLang: 'en-US',
      onLangChange: new Subject().asObservable(),
      onTranslationChange: new Subject().asObservable(),
      onDefaultLangChange: new Subject().asObservable(),
      get: vi.fn((key: string | string[]) => of(key)),
      instant: vi.fn((key: string | string[]) => key),
      stream: vi.fn((key: string | string[]) => of(key)),
      getStreamOnTranslationChange: vi.fn((key: string | string[]) => of(key)),
      use: vi.fn(() => of({})),
      setDefaultLang: vi.fn()
    } as unknown as TranslateService;

    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        { provide: BranchApiService, useValue: branchServiceMock },
        { provide: DepartmentApiService, useValue: departmentServiceMock },
        { provide: PositionApiService, useValue: positionServiceMock },
        { provide: RoleApiService, useValue: rolesServiceMock },
        { provide: StorageService, useValue: storageServiceMock },
        { provide: AuthService, useValue: authServiceMock },
        { provide: TranslateService, useValue: translateServiceMock }
      ]
    }).overrideComponent(DashboardComponent, {
      remove: { imports: [TranslatePipe] },
      add: { imports: [MockTranslatePipe] }
    }).compileComponents();

    branchServiceMock.getAllBranches.mockReturnValue(of({
      data: ['Branch 1', 'Branch 2']
    }));

    departmentServiceMock.getAllDepartments.mockReturnValue(of({
      data: ['Department 1', 'Department 2']
    }));

    positionServiceMock.getAllPositions.mockReturnValue(of({
      data: ['Position 1', 'Position 2']
    }));

    rolesServiceMock.getAllRoles.mockReturnValue(of({
      data: ['Admin', 'HR', 'Manager']
    }));

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('Unit tests', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should load local data for Admin', () => {
      component.loadLocalData();

      expect(branchServiceMock.getAllBranches).toHaveBeenCalled();
      expect(departmentServiceMock.getAllDepartments).toHaveBeenCalled();
      expect(positionServiceMock.getAllPositions).toHaveBeenCalled();
      expect(rolesServiceMock.getAllRoles).toHaveBeenCalled();
    });

    it('should not load local data for Employee', () => {
      vi.clearAllMocks();
      authServiceMock.currentUser.set({
        userId: '1',
        name: 'Harsh Donda',
        email: 'harsh@test.com',
        role: 'Employee',
        branch: 'Branch 1',
      });

      component.loadLocalData();

      expect(branchServiceMock.getAllBranches).not.toHaveBeenCalled();
      expect(departmentServiceMock.getAllDepartments).not.toHaveBeenCalled();
      expect(positionServiceMock.getAllPositions).not.toHaveBeenCalled();
      expect(rolesServiceMock.getAllRoles).not.toHaveBeenCalled();
    });

    it('should not load local data for Manager', () => {
      vi.clearAllMocks();
      authServiceMock.currentUser.set({
        userId: '1',
        name: 'Harsh Donda',
        email: 'harsh@test.com',
        role: 'Manager',
        branch: 'Branch 1'
      });

      component.loadLocalData();

      expect(branchServiceMock.getAllBranches).not.toHaveBeenCalled();
      expect(departmentServiceMock.getAllDepartments).not.toHaveBeenCalled();
      expect(positionServiceMock.getAllPositions).not.toHaveBeenCalled();
      expect(rolesServiceMock.getAllRoles).not.toHaveBeenCalled();
    });

    it('should clear existing local data before loading new data', () => {
      component.loadLocalData();

      expect(storageServiceMock.removeItem).toHaveBeenCalledWith('branches');
      expect(storageServiceMock.removeItem).toHaveBeenCalledWith('departments');
      expect(storageServiceMock.removeItem).toHaveBeenCalledWith('positions');
      expect(storageServiceMock.removeItem).toHaveBeenCalledWith('roles');
    });

    it('should store branches response', () => {
      component.loadLocalData();

      expect(storageServiceMock.setItem).toHaveBeenCalledWith('branches', ['Branch 1', 'Branch 2']);
    });

    it('should store departments response', () => {
      component.loadLocalData();

      expect(storageServiceMock.setItem).toHaveBeenCalledWith('departments', ['Department 1', 'Department 2']);
    });

    it('should store positions response', () => {
      component.loadLocalData();

      expect(storageServiceMock.setItem).toHaveBeenCalledWith('positions', ['Position 1', 'Position 2']);
    });

    it('should store roles response', () => {
      component.loadLocalData();

      expect(storageServiceMock.setItem).toHaveBeenCalledWith('roles', ['Admin', 'HR', 'Manager']);
    });
  });

  describe('Integration tests', () => {
    it('should load data when the dashboard initializes', () => {
      expect(branchServiceMock.getAllBranches).toHaveBeenCalled();
      expect(departmentServiceMock.getAllDepartments).toHaveBeenCalled();
      expect(positionServiceMock.getAllPositions).toHaveBeenCalled();
      expect(rolesServiceMock.getAllRoles).toHaveBeenCalled();
    });

    it('should render dashboard content', () => {
      const compiled = fixture.nativeElement as HTMLElement;

      expect(compiled.textContent).toContain('DASHBOARD.TITLE');
      expect(compiled.textContent).toContain('DASHBOARD.DESCRIPTION');
    });
  });
});
