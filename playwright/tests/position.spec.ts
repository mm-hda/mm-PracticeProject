import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/auth.page';
import { PositionPage } from '../pages/position.page';
import { DepartmentPage } from '../pages/department.page';
import locators from '../utils/locators.json';

const VALID_EMAIL = 'admin@example.com';
const VALID_PASSWORD = '123456';

function uniquePositionName(): string {
    return `Playwright Position ${Date.now()}`;
}

function uniqueDepartmentName(): string {
    return `Playwright Department ${Date.now()}`;
}

test.describe('Position management', () => {
    let positionPage: PositionPage;

    test.beforeEach(async ({ page }) => {
        const loginPage = new LoginPage(page);
        await loginPage.login(VALID_EMAIL, VALID_PASSWORD);
        positionPage = new PositionPage(page);
        await positionPage.goto();
    });

    test('should display the positions table', async () => {
        await positionPage.expectTableVisible();
    });

    test('should display all position table headers', async () => {
        await positionPage.expectTableHeaders();
    });

    test('should open Add Position modal', async () => {
        await positionPage.openAddModal();
        await expect(positionPage.dialog.getByRole('heading', { name: /Add Position/i })).toBeVisible();
        await expect(positionPage.dialog.locator(locators.position.positionName)).toBeVisible();
        await expect(positionPage.dialog.locator(locators.position.departmentSelect)).toBeVisible();
        await expect(positionPage.dialog.getByRole('button', { name: /Create Position/i })).toBeVisible();
        await positionPage.cancelModal();
    });

    test('should close Add Position modal using Cancel', async () => {
        await positionPage.openAddModal();
        await positionPage.cancelModal();
    });

    test('should close Add Position modal using X button', async () => {
        await positionPage.openAddModal();
        await positionPage.closeModal();
    });

    test('should load departments in Add Position modal', async () => {
        await positionPage.openAddModal();
        const options = await positionPage.getDepartmentOptions();
        expect(options.length).toBeGreaterThan(1);
        await positionPage.closeModal();
    });

    test('should show required validation for position name and department', async () => {
        await positionPage.openAddModal();
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.expectValidationMessages(2);
    });

    test('should show minimum length validation for position name', async () => {
        await positionPage.openAddModal();
        await positionPage.fillPosition('A');
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.expectValidationMessages(2);
    });

    test('should show department required validation', async () => {
        await positionPage.openAddModal();
        await positionPage.fillPosition('Developer');
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await expect(positionPage.dialog.locator(locators.position.validationMessage)).toContainText(/Department/i);
    });

    test('should show maximum length validation for position name', async () => {
        await positionPage.openAddModal();
        const longName = 'A'.repeat(101);
        await positionPage.fillPosition(longName);
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.expectValidationMessages(2);
    });

    test('should create a new position', async () => {
        const positionName = uniquePositionName();
        await positionPage.openAddModal();
        const departments = await positionPage.getDepartmentOptions();
        expect(departments.length).toBeGreaterThan(1);
        const departmentName = departments[1].trim();
        await positionPage.fillPosition(positionName, departmentName);
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.expectToastVisible();
        await expect(positionPage.dialog).not.toBeVisible();
        await positionPage.expectPositionVisible(positionName);
    });

    test('should display toast after creating position', async () => {
        const positionName = uniquePositionName();
        await positionPage.openAddModal();
        const departments = await positionPage.getDepartmentOptions();
        const departmentName = departments[1].trim();
        await positionPage.fillPosition(positionName, departmentName);
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.expectToastVisible();
    });

    test('should close position creation toast', async () => {
        const positionName = uniquePositionName();
        await positionPage.openAddModal();
        const departments = await positionPage.getDepartmentOptions();
        await positionPage.fillPosition(positionName, departments[1].trim());
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.expectToastVisible();
        await positionPage.closeToast();
    });

    test('should open position detail modal', async () => {
        const positionName = uniquePositionName();
        await positionPage.openAddModal();
        const departments = await positionPage.getDepartmentOptions();
        await positionPage.fillPosition(positionName, departments[1].trim());
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.expectPositionVisible(positionName);
        await positionPage.openDetail(positionName);
        await expect(positionPage.dialog).toContainText(positionName);
        await positionPage.closeModal();
    });

    test('should display position department in detail modal', async () => {
        const positionName = uniquePositionName();
        await positionPage.openAddModal();
        const departments = await positionPage.getDepartmentOptions();
        const departmentName = departments[1].trim();
        await positionPage.fillPosition(positionName, departmentName);
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.openDetail(positionName);
        await expect(positionPage.dialog).toContainText(departmentName);
        await positionPage.closeModal();
    });

    test('should open position employees modal', async () => {
        const positionName = uniquePositionName();
        await positionPage.openAddModal();
        const departments = await positionPage.getDepartmentOptions();
        await positionPage.fillPosition(positionName, departments[1].trim());
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.expectPositionVisible(positionName);
        await positionPage.openEmployees(positionName);
        await positionPage.expectEmployeesTableVisible();
        await positionPage.closeModal();
    });

    test('should open Edit Position modal with existing values', async () => {
        const positionName = uniquePositionName();
        await positionPage.openAddModal();
        const departments = await positionPage.getDepartmentOptions();
        const departmentName = departments[1].trim();
        await positionPage.fillPosition(positionName, departmentName);
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.openEdit(positionName);
        await positionPage.expectEditValue(positionName);
        await positionPage.expectSelectedDepartment(departmentName);
        await positionPage.closeModal();
    });

    test('should display toast after updating position', async () => {
        const originalName = uniquePositionName();
        const updatedName = `${originalName} Updated`;
        await positionPage.openAddModal();
        const departments = await positionPage.getDepartmentOptions();
        const departmentName = departments[1].trim();
        await positionPage.fillPosition(originalName, departmentName);
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.openEdit(originalName);
        await positionPage.updatePosition(updatedName, departmentName);
        await positionPage.expectToastVisible();
    });

    test('should close update toast', async () => {
        const originalName = uniquePositionName();
        const updatedName = `${originalName} Updated`;
        await positionPage.openAddModal();
        const departments = await positionPage.getDepartmentOptions();
        const departmentName = departments[1].trim();
        await positionPage.fillPosition(originalName, departmentName);
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.openEdit(originalName);
        await positionPage.updatePosition(updatedName, departmentName);
        await positionPage.expectToastVisible();
        await positionPage.closeToast();
    });

    test('should close position detail modal using X button', async () => {
        const positionName = uniquePositionName();
        await positionPage.openAddModal();
        const departments = await positionPage.getDepartmentOptions();
        await positionPage.fillPosition(positionName, departments[1].trim());
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.openDetail(positionName);
        await positionPage.closeModal();
    });

    test('should close position employees modal using X button', async () => {
        const positionName = uniquePositionName();
        await positionPage.openAddModal();
        const departments = await positionPage.getDepartmentOptions();
        await positionPage.fillPosition(positionName, departments[1].trim());
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.openEmployees(positionName);
        await positionPage.closeModal();
    });

    test('should display the created position in the department detail', async ({ page }) => {
        const departmentName = uniqueDepartmentName();
        const positionName = uniquePositionName();
        const departmentPage = new DepartmentPage(page);
        await departmentPage.goto();
        await departmentPage.openAddModal();
        await departmentPage.dialog.locator('#departmentName').fill(departmentName);
        await departmentPage.dialog.getByRole('button', { name: /Create Department/i }).click();
        await departmentPage.expectToastVisible();
        await expect(departmentPage.dialog).not.toBeVisible();
        await departmentPage.expectDepartmentVisible(departmentName);
        const positionPage = new PositionPage(page);
        await positionPage.goto();
        await positionPage.openAddModal();
        await positionPage.fillPosition(positionName, departmentName);
        await positionPage.dialog.getByRole('button', { name: /Create Position/i }).click();
        await positionPage.expectToastVisible();
        await expect(positionPage.dialog).not.toBeVisible();
        await positionPage.expectPositionVisible(positionName);
        await departmentPage.goto();
        await departmentPage.expectDepartmentVisible(departmentName);
        await departmentPage.openDetail(departmentName);
        await departmentPage.expectPositionsTableVisible();
        await expect(departmentPage.dialog.locator('app-table')).toContainText(positionName);
        await departmentPage.closeModal();
    });
});