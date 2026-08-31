import { expect, Page } from '@playwright/test';
import locators from '../utils/locators.json';

export class PositionPage {
    constructor(public readonly page: Page) { }

    private readonly positionUrl = '/positions';

    get table() {
        return this.page.locator(locators.position.table);
    }

    get tableHead() {
        return this.page.locator(locators.position.tableHead);
    }

    get tableBody() {
        return this.page.locator(locators.position.tableBody);
    }

    get dialog() {
        return this.page.locator(locators.position.modal);
    }

    get toast() {
        return this.page.locator(locators.toast.container);
    }

    get toastMessage() {
        return this.page.locator(locators.toast.message);
    }

    async goto(): Promise<void> {
        await this.page.goto(this.positionUrl);
        await expect(this.page).toHaveURL(/\/positions\/?$/);
        await expect(this.table).toBeVisible();
    }

    async expectTableVisible(): Promise<void> {
        await expect(this.table).toBeVisible();
        await expect(this.tableHead).toBeVisible();
        await expect(this.tableBody).toBeVisible();
    }

    async expectTableHeaders(): Promise<void> {
        const headers = this.tableHead.getByRole('columnheader');
        await expect(headers).toHaveCount(4);
        await expect(headers.nth(0)).toBeVisible();
        await expect(headers.nth(1)).toBeVisible();
        await expect(headers.nth(2)).toBeVisible();
        await expect(headers.nth(3)).toBeVisible();
    }

    async openAddModal(): Promise<void> {
        await this.page.getByRole('button', { name: /Add Position/i }).click();
        await expect(this.dialog).toBeVisible();
        await expect(this.dialog.locator(locators.position.positionName)).toBeVisible();
        await expect(this.dialog.locator(locators.position.departmentSelect)).toBeVisible();
    }

    async fillPosition(name: string, departmentName?: string): Promise<void> {
        await this.dialog.locator(locators.position.positionName).fill(name);
        if (departmentName) await this.selectDepartment(departmentName);
    }

    async selectDepartment(departmentName: string): Promise<void> {
        const departmentSelect = this.dialog.locator(locators.position.departmentSelect);
        await departmentSelect.selectOption({ label: departmentName });
    }

    async getDepartmentOptions(): Promise<string[]> {
        return await this.dialog.locator(`${locators.position.departmentSelect} option`).allTextContents();
    }

    async createPosition(name: string, departmentName: string): Promise<void> {
        await this.openAddModal();
        await this.fillPosition(name, departmentName);
        await this.dialog.getByRole('button', { name: /Create Position/i }).click();
        await this.expectToastVisible();
        await expect(this.dialog).not.toBeVisible();
    }

    async openDetail(positionName: string): Promise<void> {
        const row = this.getPositionRow(positionName);
        await expect(row).toBeVisible();
        await row.locator(locators.position.detailButton).click();
        await expect(this.dialog).toBeVisible();
        await expect(this.dialog.getByRole('heading', { name: /Detail/i })).toBeVisible();
    }

    async openEmployees(positionName: string): Promise<void> {
        const row = this.getPositionRow(positionName);
        await expect(row).toBeVisible();
        await row.getByRole('button').nth(1).click();
        await expect(this.dialog).toBeVisible();
        await expect(this.dialog.getByRole('heading', { name: /Employees/i })).toBeVisible();
        await expect(this.dialog.locator(locators.position.tableComponent)).toBeVisible();
    }

    async openEdit(positionName: string): Promise<void> {
        const row = this.getPositionRow(positionName);
        await expect(row).toBeVisible();
        await row.getByRole('button').nth(2).click();
        await expect(this.dialog).toBeVisible();
        await expect(this.dialog.getByRole('heading', { name: /Edit Position/i })).toBeVisible();
    }

    async updatePosition(name: string, departmentName: string): Promise<void> {
        await this.fillPosition(name, departmentName);
        await this.dialog.getByRole('button', { name: /Update Position/i }).click();
        await this.expectToastVisible();
    }

    getPositionRow(positionName: string) {
        return this.page.locator(locators.position.tableRow).filter({ hasText: positionName });
    }

    async expectPositionVisible(positionName: string): Promise<void> {
        await expect(this.getPositionRow(positionName)).toBeVisible();
    }

    async expectPositionNotVisible(positionName: string): Promise<void> {
        await expect(this.getPositionRow(positionName)).not.toBeVisible();
    }

    async expectPositionContains(positionName: string, text: string): Promise<void> {
        const row = this.getPositionRow(positionName);
        await expect(row).toBeVisible();
        await expect(row).toContainText(text);
    }

    async expectEditValue(name: string): Promise<void> {
        await expect(this.dialog.locator(locators.position.positionName)).toHaveValue(name);
    }

    async expectSelectedDepartment(departmentName: string): Promise<void> {
        await expect(this.dialog.locator(locators.position.departmentSelect)).toHaveValue(await this.getDepartmentValue(departmentName));
    }

    async getDepartmentValue(departmentName: string): Promise<string> {
        return await this.dialog.locator(`${locators.position.departmentSelect} option`).filter({ hasText: departmentName }).first().getAttribute('value').then(value => value ?? '');
    }

    async expectValidationMessages(count: number): Promise<void> {
        await expect(this.dialog.locator(locators.position.validationMessage)).toHaveCount(count);
    }

    async expectDepartmentSelectVisible(): Promise<void> {
        await expect(this.dialog.locator(locators.position.departmentSelect)).toBeVisible();
    }

    async expectEmployeesTableVisible(): Promise<void> {
        await expect(this.dialog.locator(locators.position.tableComponent)).toBeVisible();
    }

    async closeModal(): Promise<void> {
        await this.dialog.locator(locators.position.closeButton).click();
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
}