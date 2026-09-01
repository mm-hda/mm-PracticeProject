import { test, expect } from '@playwright/test';

import { LoginPage } from '../pages/auth.page';
import { DepartmentPage } from '../pages/department.page';

const VALID_EMAIL = 'admin@example.com';
const VALID_PASSWORD = '123456';

function uniqueDepartmentName(): string {
    return `Playwright Department ${Date.now()}`;
}

test.describe('Department management', () => {
    let departmentPage: DepartmentPage;

    test.beforeEach(async ({ page }) => {
        const loginPage = new LoginPage(page);

        await loginPage.login(
            VALID_EMAIL,
            VALID_PASSWORD
        );

        departmentPage = new DepartmentPage(page);

        await departmentPage.goto();
    });

    test('should display the departments table', async () => {
        await departmentPage.expectTableVisible();
    });

    test('should display all department table headers', async () => {
        await departmentPage.expectTableHeaders();
    });

    test('should open and close the Add Department modal', async () => {
        await departmentPage.openAddModal();

        await expect(
            departmentPage.dialog.getByRole('heading', {
                name: /Add Department/i
            })
        ).toBeVisible();

        await expect(
            departmentPage.dialog.locator('#departmentName')
        ).toBeVisible();

        await expect(
            departmentPage.dialog.getByRole('button', {
                name: /Cancel/i
            })
        ).toBeVisible();

        await expect(
            departmentPage.dialog.getByRole('button', {
                name: /Create Department/i
            })
        ).toBeVisible();

        await departmentPage.cancelModal();
    });

    test('should close Add Department modal using X button', async () => {
        await departmentPage.openAddModal();

        await departmentPage.closeModal();
    });

    test('should show required validation when department name is empty', async () => {
        await departmentPage.openAddModal();

        await departmentPage.dialog
            .getByRole('button', {
                name: /Create Department/i
            })
            .click();

        await departmentPage.expectValidationMessages(1);
    });

    test('should show minimum length validation for department name', async () => {
        await departmentPage.openAddModal();

        await departmentPage.fillDepartment('A');

        await departmentPage.dialog
            .getByRole('button', {
                name: /Create Department/i
            })
            .click();

        await departmentPage.expectValidationMessages(1);
    });

    test('should show maximum length validation for department name', async () => {
        await departmentPage.openAddModal();

        const longDepartmentName = 'A'.repeat(101);

        await departmentPage.fillDepartment(
            longDepartmentName
        );

        await departmentPage.dialog
            .getByRole('button', {
                name: /Create Department/i
            })
            .click();

        await departmentPage.expectValidationMessages(1);
    });

    test('should not create department when name contains only one character', async () => {
        await departmentPage.openAddModal();

        await departmentPage.fillDepartment('A');

        await departmentPage.dialog
            .getByRole('button', {
                name: /Create Department/i
            })
            .click();

        await expect(
            departmentPage.dialog
        ).toBeVisible();
    });

    test('should create a new department', async () => {
        const departmentName = uniqueDepartmentName();

        await departmentPage.createDepartment(
            departmentName
        );

        await departmentPage.expectDepartmentVisible(
            departmentName
        );
    });

    test('should display toast after creating department', async () => {
        const departmentName = uniqueDepartmentName();

        await departmentPage.createDepartment(
            departmentName
        );

        await departmentPage.expectToastVisible();
    });

    test('should close create success toast', async () => {
        const departmentName = uniqueDepartmentName();

        await departmentPage.createDepartment(
            departmentName
        );

        await departmentPage.closeToast();
    });

    test('should open department detail modal', async () => {
        const departmentName = uniqueDepartmentName();

        await departmentPage.createDepartment(
            departmentName
        );

        await departmentPage.openDetail(
            departmentName
        );

        await expect(
            departmentPage.dialog
        ).toContainText(departmentName);

        await departmentPage.closeModal();
    });

    test('should display department positions table in detail modal', async () => {
        const departmentName = uniqueDepartmentName();

        await departmentPage.createDepartment(
            departmentName
        );

        await departmentPage.openDetail(
            departmentName
        );

        await departmentPage.expectPositionsTableVisible();

        await departmentPage.closeModal();
    });

    test('should open department employees modal', async () => {
        const departmentName = uniqueDepartmentName();

        await departmentPage.createDepartment(
            departmentName
        );

        await departmentPage.openEmployees(
            departmentName
        );

        await departmentPage.expectEmployeesTableVisible();

        await departmentPage.closeModal();
    });

    test('should open Edit Department modal', async () => {
        const departmentName = uniqueDepartmentName();

        await departmentPage.createDepartment(
            departmentName
        );

        await departmentPage.openEdit(
            departmentName
        );

        await departmentPage.expectEditValue(
            departmentName
        );

        await departmentPage.closeModal();
    });

    test('should populate existing department name in edit modal', async () => {
        const departmentName = uniqueDepartmentName();

        await departmentPage.createDepartment(
            departmentName
        );

        await departmentPage.openEdit(
            departmentName
        );

        await expect(
            departmentPage.dialog.locator('#departmentName')
        ).toHaveValue(departmentName);

        await departmentPage.closeModal();
    });

    test('should update an existing department', async () => {
        const originalName = uniqueDepartmentName();
        const updatedName = `${originalName} changed`;

        await departmentPage.createDepartment(originalName);
        await departmentPage.openEdit(originalName);
        await departmentPage.updateDepartment(updatedName);
        await departmentPage.expectDepartmentVisible(updatedName);
    });

    test('should display toast after updating department', async () => {
        const originalName = uniqueDepartmentName();
        const updatedName = `${originalName} changed`;

        await departmentPage.createDepartment(
            originalName
        );

        await departmentPage.openEdit(
            originalName
        );

        await departmentPage.updateDepartment(
            updatedName
        );

        await departmentPage.expectToastVisible();
    });

    test('should close update toast', async () => {
        const originalName = uniqueDepartmentName();
        const updatedName = `${originalName} changed`;

        await departmentPage.createDepartment(
            originalName
        );

        await departmentPage.openEdit(
            originalName
        );

        await departmentPage.updateDepartment(
            updatedName
        );

        await departmentPage.expectToastVisible();

        await departmentPage.closeToast();
    });

    test('should close department detail modal using X button', async () => {
        const departmentName = uniqueDepartmentName();

        await departmentPage.createDepartment(
            departmentName
        );

        await departmentPage.openDetail(
            departmentName
        );

        await departmentPage.closeModal();
    });

    test('should close department employees modal using X button', async () => {
        const departmentName = uniqueDepartmentName();

        await departmentPage.createDepartment(
            departmentName
        );

        await departmentPage.openEmployees(
            departmentName
        );

        await departmentPage.closeModal();
    });
});