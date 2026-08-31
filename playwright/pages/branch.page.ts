import { expect, Page } from '@playwright/test';
import locators from '../utils/locators.json';

export class BranchPage {
    constructor(private readonly page: Page) { }

    private readonly branchUrl = '/branches';

    async goto(): Promise<void> {
        await this.page.goto(this.branchUrl);
        await expect(this.page).toHaveURL(/\/branches\/?$/);
        await expect(this.table).toBeVisible();
    }

    get table() {
        return this.page.locator(locators.branch.table);
    }

    get dialog() {
        return this.page.locator(locators.branch.modal);
    }

    get toast() {
        return this.page.locator(locators.toast.container);
    }

    get toastMessage() {
        return this.page.locator(locators.toast.message);
    }

    async expectTableVisible(): Promise<void> {
        await expect(this.table).toBeVisible();
        await expect(this.page.locator(locators.branch.tableHead)).toBeVisible();
        await expect(this.page.locator(locators.branch.tableBody)).toBeVisible();
    }

    async openAddModal(): Promise<void> {
        await this.page.getByRole('button', { name: /Add Branch/i }).click();
        await expect(this.dialog).toBeVisible();
        await expect(this.dialog.locator(locators.branch.branchName)).toBeVisible();
        await expect(this.dialog.locator(locators.branch.branchLocation)).toBeVisible();
    }

    async fillBranch(name: string, location: string): Promise<void> {
        await this.dialog.locator(locators.branch.branchName).fill(name);
        await this.dialog.locator(locators.branch.branchLocation).fill(location);
    }

    async createBranch(name: string, location: string): Promise<void> {
        await this.openAddModal();
        await this.fillBranch(name, location);
        await this.dialog.getByRole('button', { name: /Create Branch/i }).click();
        await this.expectToastVisible();
        await expect(this.dialog).not.toBeVisible();
    }

    async openDetail(branchName: string): Promise<void> {
        const row = this.getBranchRow(branchName);
        await expect(row).toBeVisible();
        await row.locator(locators.branch.detailButton).click();
        await expect(this.dialog).toBeVisible();
        await expect(this.dialog.getByRole('heading', { name: /Detail/i })).toBeVisible();
    }

    async openEmployees(branchName: string): Promise<void> {
        const row = this.getBranchRow(branchName);
        await expect(row).toBeVisible();
        await row.locator(locators.branch.employeesButton).click();
        await expect(this.dialog).toBeVisible();
        await expect(this.dialog.getByRole('heading', { name: /Branch Employees/i })).toBeVisible();
        await expect(this.dialog.locator(locators.branch.employeeTable)).toBeVisible();
    }

    async openEdit(branchName: string): Promise<void> {
        const row = this.getBranchRow(branchName);
        await expect(row).toBeVisible();
        await row.locator(locators.branch.editButton).click();
        await expect(this.dialog).toBeVisible();
        await expect(this.dialog.getByRole('heading', { name: /Edit Branch/i })).toBeVisible();
    }

    async updateBranch(name: string, location: string): Promise<void> {
        await this.fillBranch(name, location);
        await this.dialog.getByRole('button', { name: /Update Branch/i }).click();
        await this.expectToastVisible();
        await expect(this.dialog).not.toBeVisible();
    }

    async closeModal(): Promise<void> {
        await this.dialog.locator(locators.branch.closeButton).click();
        await expect(this.dialog).not.toBeVisible();
    }

    async cancelModal(): Promise<void> {
        await this.dialog.getByRole('button', { name: /Cancel/i }).click();
        await expect(this.dialog).not.toBeVisible();
    }

    async expectToastVisible(): Promise<void> {
        await expect(this.toast).toBeVisible();
        await expect(this.toastMessage).toBeVisible();
    }

    async expectToastContains(text: string): Promise<void> {
        await expect(this.toast).toBeVisible();
        await expect(this.toastMessage).toContainText(text);
    }

    async closeToast(): Promise<void> {
        await this.toast.locator(locators.toast.closeButton).click();
        await expect(this.toast).not.toBeVisible();
    }

    getBranchRow(branchName: string) {
        return this.page
            .locator(locators.branch.tableRow)
            .filter({ hasText: branchName });
    }

    async expectBranchVisible(branchName: string): Promise<void> {
        await expect(this.getBranchRow(branchName)).toBeVisible();
    }

    async expectBranchContains(branchName: string, location: string): Promise<void> {
        const row = this.getBranchRow(branchName);
        await expect(row).toBeVisible();
        await expect(row).toContainText(branchName);
        await expect(row).toContainText(location);
    }

    async expectBranchNotVisible(branchName: string): Promise<void> {
        await expect(this.getBranchRow(branchName)).not.toBeVisible();
    }

    async expectEditValues(name: string, location: string): Promise<void> {
        await expect(this.dialog.locator(locators.branch.branchName)).toHaveValue(name);
        await expect(this.dialog.locator(locators.branch.branchLocation)).toHaveValue(location);
    }

    async expectValidationMessages(count: number): Promise<void> {
        await expect(this.dialog.locator('.text-danger')).toHaveCount(count);
    }
}