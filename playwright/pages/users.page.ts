import { expect, Page } from '@playwright/test';
import locators from '../utils/locators.json';

export class UsersPage {
    constructor(public readonly page: Page) { }
    private readonly usersUrl = '/users';

    get table() {
        return this.page.locator(locators.user.table);
    }

    get tableHead() {
        return this.page.locator(locators.user.tableHead);
    }

    get tableBody() {
        return this.page.locator(locators.user.tableBody);
    }

    get dialog() {
        return this.page.locator(locators.user.modal);
    }

    get toast() {
        return this.page.locator(locators.toast.container);
    }

    get toastMessage() {
        return this.page.locator(locators.toast.message);
    }

    async goto(): Promise<void> {
        await this.page.goto(this.usersUrl);
        await expect(this.page).toHaveURL(/\/users\/?$/);
        await expect(this.table).toBeVisible();
    }

    async expectTableVisible(): Promise<void> {
        await expect(this.table).toBeVisible();
        await expect(this.tableHead).toBeVisible();
        await expect(this.tableBody).toBeVisible();
    }

    async expectTableHeaders(): Promise<void> {
        const headers = this.tableHead.getByRole('columnheader');
        await expect(headers).toHaveCount(7);
        for (let index = 0; index < 7; index++) {
            await expect(headers.nth(index)).toBeVisible();
        }
    }

    async openAddModal(): Promise<void> {
        await this.page.getByRole('button', { name: /Add/i }).click();
        await expect(this.dialog).toBeVisible();
        await expect(this.dialog.locator(locators.user.firstName)).toBeVisible();
        await expect(this.dialog.locator(locators.user.lastName)).toBeVisible();
        await expect(this.dialog.locator(locators.user.email)).toBeVisible();
        await expect(this.dialog.locator(locators.user.password)).toBeVisible();
        await expect(this.dialog.locator(locators.user.roleSelect)).toBeVisible();
        await expect(this.dialog.locator(locators.user.branchSelect)).toBeVisible();
        await expect(this.dialog.locator(locators.user.departmentSelect)).toBeVisible();
        await expect(this.dialog.locator(locators.user.positionSelect)).toBeVisible();
    }

    async fillUser(firstName: string, lastName: string, email: string, password: string, dob = '1995-01-01'): Promise<void> {
        await this.dialog.locator(locators.user.firstName).fill(firstName);
        await this.dialog.locator(locators.user.lastName).fill(lastName);
        await this.dialog.locator(locators.user.email).fill(email);
        await this.dialog.locator(locators.user.password).fill(password);
        if (dob) {
            await this.dialog.locator(locators.user.dob).fill(dob);
        }
    }

    async selectRole(roleName: string): Promise<void> {
        await this.dialog.locator(locators.user.roleSelect).selectOption({ label: roleName });
    }

    async selectBranch(branchName: string): Promise<void> {
        await this.dialog.locator(locators.user.branchSelect).selectOption({ label: branchName });
    }

    async selectDepartment(departmentName: string): Promise<void> {
        await this.dialog.locator(locators.user.departmentSelect).selectOption({ label: departmentName });
    }

    async selectPosition(positionName: string): Promise<void> {
        await this.dialog.locator(locators.user.positionSelect).selectOption({ label: positionName });
    }

    async getOptions(locator: string): Promise<string[]> {
        return await this.dialog.locator(`${locator} option`).allTextContents();
    }

    async getRoleOptions(): Promise<string[]> {
        return await this.getOptions(locators.user.roleSelect);
    }

    async getBranchOptions(): Promise<string[]> {
        return await this.getOptions(locators.user.branchSelect);
    }

    async getDepartmentOptions(): Promise<string[]> {
        return await this.getOptions(locators.user.departmentSelect);
    }

    async getPositionOptions(): Promise<string[]> {
        return await this.getOptions(locators.user.positionSelect);
    }

    async getFirstRealOption(locator: string): Promise<string> {
        const option = await this.dialog.locator(`${locator} option`).evaluateAll(options => {
            const realOption = options.find(option => (option as HTMLOptionElement).value !== '');
            return realOption?.textContent?.trim() ?? '';
        });
        if (!option) {
            throw new Error(`No real option found for locator: ${locator} `);
        }
        return option;
    }

    async clickCreate(): Promise<void> {
        await this.dialog.getByRole('button', { name: /Create/i }).click();
    }

    async createUser(firstName: string, lastName: string, email: string, password: string, roleName: string, branchName: string, departmentName: string, positionName: string, dob = '1995-01-01'): Promise<void> {
        await this.openAddModal();
        await this.fillUser(firstName, lastName, email, password, dob);
        await this.selectRole(roleName);
        await this.selectBranch(branchName);
        await this.selectDepartment(departmentName);
        await this.selectPosition(positionName);
        await this.clickCreate();
        await this.expectToastVisible();
        await expect(this.dialog).not.toBeVisible();
    }

    async createUserUsingFirstOptions(firstName: string, lastName: string, email: string, password: string): Promise<{ role: string; branch: string; department: string; position: string }> {
        await this.openAddModal();
        const role = await this.getFirstRealOption(locators.user.roleSelect);
        const branch = await this.getFirstRealOption(locators.user.branchSelect);
        const department = await this.getFirstRealOption(locators.user.departmentSelect);
        await this.fillUser(firstName, lastName, email, password);
        await this.selectRole(role);
        await this.selectBranch(branch);
        await this.selectDepartment(department);
        const position = await this.getFirstRealOption(locators.user.positionSelect);
        await this.selectPosition(position);
        await this.clickCreate();
        await this.expectToastVisible();
        await expect(this.dialog).not.toBeVisible();
        return { role, branch, department, position };
    }

    getUserRow(userName: string) {
        return this.page.locator(locators.user.tableRow).filter({ hasText: userName });
    }

    async expectUserVisible(userName: string): Promise<void> {
        await expect(this.getUserRow(userName)).toBeVisible();
    }

    async expectUserNotVisible(userName: string): Promise<void> {
        await expect(this.getUserRow(userName)).not.toBeVisible();
    }

    async expectUserContains(userName: string, text: string): Promise<void> {
        const row = this.getUserRow(userName);
        await expect(row).toBeVisible();
        await expect(row).toContainText(text);
    }

    async openUserDetail(userName: string): Promise<void> {
        const row = this.getUserRow(userName);
        await expect(row).toBeVisible();
        await row.locator(locators.user.detailButton).click();
        await expect(this.dialog).toBeVisible();
        await expect(this.dialog.getByRole('heading', { name: /User Details/i })).toBeVisible();
    }

    async expectUserDetail(firstName: string, lastName: string, email: string, branchName: string, departmentName: string, positionName: string, roleName: string): Promise<void> {
        await expect(this.dialog).toContainText(`${firstName} ${lastName} `);
        await expect(this.dialog).toContainText(email);
        await expect(this.dialog).toContainText(branchName);
        await expect(this.dialog).toContainText(departmentName);
        await expect(this.dialog).toContainText(positionName);
        await expect(this.dialog).toContainText(roleName);
    }

    async searchUser(email: string): Promise<void> {
        await this.page.locator(locators.user.searchInput).fill(email);
        await this.page.getByRole('button', { name: /^Search$/i }).click();
        await expect(this.getUserRow(email)).toBeVisible();
    }

    async applyBranchFilter(branchName: string): Promise<void> {
        const selects = this.page.locator('select');
        await selects.nth(1).selectOption({ label: branchName });
        await this.page.getByRole('button').filter({ has: this.page.locator('i.bi-funnel') }).click();
        await expect(this.tableBody).toBeVisible();
    }

    async resetFilters(): Promise<void> {
        await this.page.getByRole('button', { name: /Reset/i }).click();
        await this.expectTableVisible();
    }

    async closeModal(): Promise<void> {
        await this.dialog.locator(locators.user.cancelButton).click();
        await expect(this.dialog).not.toBeVisible();
    }

    async closeWithX(): Promise<void> {
        await this.dialog.locator(locators.user.closeButton).click();
        await expect(this.dialog).not.toBeVisible();
    }

    async expectToastVisible(): Promise<void> {
        await expect(this.toast).toBeVisible();
        await expect(this.toastMessage).toBeVisible();
        await expect(this.toastMessage).not.toHaveText('');
    }

    async expectToastContains(text: string): Promise<void> {
        await expect(this.toast).toBeVisible();
        await expect(this.toastMessage).toContainText(text);
    }

    async closeToast(): Promise<void> {
        if (await this.toast.isVisible()) {
            await this.toast.locator(locators.toast.closeButton).click();
            await expect(this.toast).not.toBeVisible();
        }
    }

    async expectValidationMessages(count: number): Promise<void> {
        await expect(this.dialog.locator(locators.user.validationMessage)).toHaveCount(count);
    }

    async expectValidationMessage(text: string): Promise<void> {
        await expect(this.dialog.locator(locators.user.validationMessage).filter({ hasText: text })).toBeVisible();
    }

    private paginationLocator() {
        return this.page.locator('div.fw-semibold').filter({ hasText: /Page\s+\d+\s+of\s+\d+/i });
    }

    async getCurrentPage(): Promise<number> {
        const text = await this.paginationLocator().innerText();
        const match = text.match(/Page\s+(\d+)\s+of\s+(\d+)/i);
        if (!match) {
            throw new Error(`Unable to read current page from: "${text}"`);
        }
        return Number(match[1]);
    }

    async getTotalPages(): Promise<number> {
        const text = await this.paginationLocator().innerText();
        const match = text.match(/Page\s+(\d+)\s+of\s+(\d+)/i);
        if (!match) {
            throw new Error(`Unable to read total pages from: "${text}"`);
        }
        return Number(match[2]);
    }

    async goToLastPage(): Promise<void> {
        const totalPages = await this.getTotalPages();
        let currentPage = await this.getCurrentPage();
        if (totalPages < 1) {
            throw new Error('No user pages are available.');
        }
        while (currentPage < totalPages) {
            const nextButton = this.page.getByRole('button', { name: /Next/i });
            await expect(nextButton).toBeEnabled();
            const nextPageNumber = currentPage + 1;
            await nextButton.click();
            await expect.poll(() => this.getCurrentPage()).toBe(nextPageNumber);
            currentPage = nextPageNumber;
        }
        await expect(this.page.getByRole('button', { name: /Next/i })).toBeDisabled();
    }

    async goToFirstPage(): Promise<void> {
        let currentPage = await this.getCurrentPage();
        while (currentPage > 1) {
            const previousButton = this.page.getByRole('button', { name: /Previous/i });
            await expect(previousButton).toBeEnabled();
            const previousPageNumber = currentPage - 1;
            await previousButton.click();
            await expect.poll(() => this.getCurrentPage()).toBe(previousPageNumber);
            currentPage = previousPageNumber;
        }
        await expect(this.page.getByRole('button', { name: /Previous/i })).toBeDisabled();
    }

    async expectUserOnLastPage(userName: string): Promise<void> {
        await this.goToLastPage();
        await this.expectUserVisible(userName);
    }

    async nextPage(): Promise<void> {
        await this.page.getByRole('button', { name: /Next/i }).click();
    }

    async previousPage(): Promise<void> {
        await this.page.getByRole('button', { name: /Previous/i }).click();
    }

    async expectEmployeeInModal(userName: string): Promise<void> {
        await expect(this.dialog).toBeVisible();
        await expect(this.dialog).toContainText(userName);
    }
}
