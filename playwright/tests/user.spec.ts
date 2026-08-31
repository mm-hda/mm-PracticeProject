import { expect, test } from '@playwright/test';
import { UsersPage } from '../pages/users.page';
import { LoginPage } from '../pages/auth.page';

const VALID_EMAIL = 'admin@example.com';
const VALID_PASSWORD = '123456';
const BRANCH_NAME = 'M&M Software Development Center India';
const DEPARTMENT_NAME = 'Software Development';
const POSITION_NAME = 'Junior Developer';
const ROLE_NAME = 'Employee';
const DOB = '2005-11-21';

test.describe('Users management', () => {
    const runId = `${Date.now()}-${Math.floor(Math.random() * 10000)}`;
    const testData = {
        firstName: 'Playwright',
        lastName: `User ${runId}`,
        email: `playwright.user.${runId}@example.com`,
        password: 'Test@12345'
    };

    test.beforeEach(async ({ page }) => {
        const loginPage = new LoginPage(page);
        await loginPage.login(VALID_EMAIL, VALID_PASSWORD);
    });

    test('shows the users table with seven headers', async ({ page }) => {
        const usersPage = new UsersPage(page);
        await usersPage.goto();
        await usersPage.expectTableVisible();
        await usersPage.expectTableHeaders();
    });

    test('opens and closes the Add User modal using X', async ({ page }) => {
        const usersPage = new UsersPage(page);
        await usersPage.goto();
        await usersPage.openAddModal();
        await usersPage.closeWithX();
    });

    test('opens and closes the Add User modal using Cancel', async ({ page }) => {
        const usersPage = new UsersPage(page);
        await usersPage.goto();
        await usersPage.openAddModal();
        await usersPage.closeModal();
    });

    test('shows required validation messages for an empty form', async ({ page }) => {
        const usersPage = new UsersPage(page);
        await usersPage.goto();
        await usersPage.openAddModal();
        await usersPage.clickCreate();
        await usersPage.expectValidationMessages(8);
    });

    test('contains the fixed branch and department in the user form', async ({ page }) => {
        const usersPage = new UsersPage(page);
        await usersPage.goto();
        await usersPage.openAddModal();
        const branches = (await usersPage.getBranchOptions()).map(value => value.trim());
        const departments = (await usersPage.getDepartmentOptions()).map(value => value.trim());
        expect(branches).toContain(BRANCH_NAME);
        expect(departments).toContain(DEPARTMENT_NAME);
        await usersPage.closeWithX();
    });

    test('contains the fixed Employee role in the user form', async ({ page }) => {
        const usersPage = new UsersPage(page);
        await usersPage.goto();
        await usersPage.openAddModal();
        const roles = (await usersPage.getRoleOptions()).map(value => value.trim());
        expect(roles).toContain(ROLE_NAME);
        await usersPage.closeWithX();
    });

    test('creates a user using the fixed master data', async ({ page }) => {
        const usersPage = new UsersPage(page);
        await usersPage.goto();
        await usersPage.openAddModal();
        await usersPage.fillUser(
            testData.firstName,
            testData.lastName,
            testData.email,
            testData.password,
            DOB
        );

        await usersPage.selectRole(ROLE_NAME);
        await usersPage.selectBranch(BRANCH_NAME);
        await usersPage.selectDepartment(DEPARTMENT_NAME);

        const departmentPositions = await usersPage.getPositionOptions();
        const validPosition = departmentPositions.map(position => position.trim()).find(position => position !== '' && position !== 'Select Position');

        expect(validPosition).toBeTruthy();
        await usersPage.selectPosition(validPosition!);
        await usersPage.clickCreate();
        await usersPage.expectToastVisible();
        await usersPage.goto();
        await usersPage.searchUser(testData.email);
        await usersPage.expectUserContains(testData.email, testData.email);
    });

    test('navigates to the next page and returns to the previous page when available', async ({ page }) => {
        const usersPage = new UsersPage(page);
        await usersPage.goto();
        const totalPages = await usersPage.getTotalPages();
        test.skip(totalPages <= 1, 'Pagination requires at least two pages.');
        const initialPage = await usersPage.getCurrentPage();
        await usersPage.nextPage();
        await expect.poll(() => usersPage.getCurrentPage()).toBe(initialPage + 1);
        await usersPage.previousPage();
        await expect.poll(() => usersPage.getCurrentPage()).toBe(initialPage);
    });

    test('navigates to the last page and verifies pagination state', async ({ page }) => {
        const usersPage = new UsersPage(page);
        await usersPage.goto();
        const totalPages = await usersPage.getTotalPages();
        test.skip(totalPages <= 1, 'Pagination requires at least two pages.');
        await usersPage.goToLastPage();
        expect(await usersPage.getCurrentPage()).toBe(totalPages);
        await expect(page.getByRole('button', { name: /Next/i })).toBeDisabled();
    });

    test('navigates back to the first page from the last page', async ({ page }) => {
        const usersPage = new UsersPage(page);
        await usersPage.goto();
        const totalPages = await usersPage.getTotalPages();
        test.skip(totalPages <= 1, 'Pagination requires at least two pages.');
        await usersPage.goToLastPage();
        await usersPage.goToFirstPage();
        expect(await usersPage.getCurrentPage()).toBe(1);
        await expect(page.getByRole('button', { name: /Previous/i })).toBeDisabled();
    });

    test('queues a user while offline and displays Pending Sync', async ({ page, context }) => {
        const usersPage = new UsersPage(page);
        const offlineEmail = `offline.user.${runId}@example.com`;
        await usersPage.goto();
        await usersPage.openAddModal();
        await usersPage.fillUser(
            'Offline',
            `User ${runId}`,
            offlineEmail,
            testData.password,
            DOB
        );
        await usersPage.selectRole(ROLE_NAME);
        await usersPage.selectBranch(BRANCH_NAME);
        await usersPage.selectDepartment(DEPARTMENT_NAME);
        await usersPage.selectPosition(POSITION_NAME);
        await context.setOffline(true);

        await usersPage.clickCreate();
        await expect(usersPage.dialog).not.toBeVisible();
        await usersPage.expectToastVisible();
        await expect(usersPage.getUserRow(offlineEmail)).toBeVisible();
    });
});
