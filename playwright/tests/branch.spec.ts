import { test, expect } from '@playwright/test';

import { BranchPage } from '../pages/branch.page';
import { LoginPage } from '../pages/auth.page';

function uniqueBranchName(): string {
    return `Playwright Test Branch ${Date.now()}`;
}

test.describe('Branch management', () => {
    let branchPage: BranchPage;

    test.beforeEach(async ({ page }) => {
        var loginPage = new LoginPage(page);
        await loginPage.login("admin@example.com", "123456");
        branchPage = new BranchPage(page);
        await branchPage.goto();
    });

    test('should display the branches table', async () => {
        await branchPage.expectTableVisible();
    });

    test('should open and close the Add Branch modal', async () => {
        await branchPage.openAddModal();

        await expect(branchPage.dialog.getByRole('heading', { name: /Add Branch/i })).toBeVisible();
        await expect(branchPage.dialog.locator('#branchName')).toBeVisible();
        await expect(branchPage.dialog.locator('#branchLocation')).toBeVisible();
        await expect(branchPage.dialog.getByRole('button', { name: /Cancel/i })).toBeVisible();
        await expect(branchPage.dialog.getByRole('button', { name: /Create Branch/i })).toBeVisible();

        await branchPage.cancelModal();
    });

    test('should validate required branch fields', async () => {
        await branchPage.openAddModal();

        await branchPage.dialog.getByRole('button', { name: /Create Branch/i }).click();

        await branchPage.expectValidationMessages(2);
    });

    test('should validate branch name minimum length', async () => {
        await branchPage.openAddModal();

        await branchPage.fillBranch('A', 'Mumbai');

        await branchPage.dialog.getByRole('button', { name: /Create Branch/i }).click();

        await branchPage.expectValidationMessages(1);
    });

    test('should create a new branch and display it in the table', async () => {
        const branchName = uniqueBranchName();
        const location = 'Playwright Test Location';

        await branchPage.createBranch(branchName, location);

        await branchPage.expectBranchContains(branchName, location);
        await branchPage.expectToastVisible();
    });

    test('should open branch details', async () => {
        const branchName = uniqueBranchName();
        const location = 'Detail Test Location';

        await branchPage.createBranch(branchName, location);

        await branchPage.openDetail(branchName);

        await expect(branchPage.dialog).toContainText(branchName);
        await expect(branchPage.dialog).toContainText(location);

        await branchPage.closeModal();
    });

    test('should open branch employees modal', async () => {
        const branchName = uniqueBranchName();
        const location = 'Employees Test Location';

        await branchPage.createBranch(branchName, location);

        await branchPage.openEmployees(branchName);

        await expect(branchPage.dialog.locator('app-table')).toBeVisible();

        await branchPage.closeModal();
    });

    test('should open Edit Branch modal with existing values', async () => {
        const branchName = uniqueBranchName();
        const location = 'Edit Test Location';

        await branchPage.createBranch(branchName, location);

        await branchPage.openEdit(branchName);

        await branchPage.expectEditValues(branchName, location);

        await branchPage.closeModal();
    });

    test('should update an existing branch', async () => {
        const originalName = uniqueBranchName();
        const originalLocation = 'Original Location';

        const updatedName = `${originalName} changed`;
        const updatedLocation = 'Changed Location';

        await branchPage.createBranch(originalName, originalLocation);

        await branchPage.openEdit(originalName);

        await branchPage.updateBranch(updatedName, updatedLocation);

        await branchPage.expectBranchContains(updatedName, updatedLocation);
        await branchPage.expectToastVisible();
    });

    test('should close the modal using the X button', async () => {
        await branchPage.openAddModal();

        await branchPage.closeModal();
    });
});