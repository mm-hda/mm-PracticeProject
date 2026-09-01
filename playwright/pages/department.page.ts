import { expect, Page } from '@playwright/test';

import locators from '../utils/locators.json';

export class DepartmentPage {
    constructor(public readonly page: Page) { }

    private readonly departmentUrl = '/departments';

    get table() {
        return this.page.locator(locators.department.table);
    }

    get tableHead() {
        return this.page.locator(locators.department.tableHead);
    }

    get tableBody() {
        return this.page.locator(locators.department.tableBody);
    }

    get dialog() {
        return this.page.locator(locators.department.modal);
    }

    get toast() {
        return this.page.locator(locators.toast.container);
    }

    get toastMessage() {
        return this.page.locator(locators.toast.message);
    }

    async goto(): Promise<void> {
        await this.page.goto(this.departmentUrl);

        await expect(this.page).toHaveURL(/\/departments\/?$/);
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
        await this.page.getByRole('button', { name: /Add Department/i }).click();

        await expect(this.dialog).toBeVisible();
        await expect(this.dialog.locator(locators.department.departmentName)).toBeVisible();
    }

    async fillDepartment(name: string): Promise<void> {
        await this.dialog.locator(locators.department.departmentName).fill(name);
    }

    async createDepartment(name: string): Promise<void> {
        await this.openAddModal();
        await this.fillDepartment(name);

        await this.dialog.getByRole('button', { name: /Create Department/i }).click();

        await this.expectToastVisible();
        await expect(this.dialog).not.toBeVisible();
    }

    async openDetail(departmentName: string): Promise<void> {
        const row = this.getDepartmentRow(departmentName);
        await expect(row).toBeVisible();
        await row.locator(locators.department.detailButton).click();
        await expect(this.dialog).toBeVisible();
        await expect(this.dialog.getByRole('heading', { name: /Detail/i })).toBeVisible();
    }

    async openEmployees(departmentName: string): Promise<void> {
        const row = this.getDepartmentRow(departmentName);

        await expect(row).toBeVisible();

        await row.locator(locators.department.employeesButton).click();

        await expect(this.dialog).toBeVisible();

        await expect(this.dialog.getByRole('heading', { name: /Department Employees/i })).toBeVisible();

        await expect(this.dialog.locator(locators.department.employeeTable)).toBeVisible();
    }

    async openEdit(departmentName: string): Promise<void> {
        const row = this.getDepartmentRow(departmentName);

        await expect(row).toBeVisible();

        await row.locator(locators.department.editButton).click();

        await expect(this.dialog).toBeVisible();

        await expect(this.dialog.getByRole('heading', { name: /Edit Department/i })).toBeVisible();
    }

    async updateDepartment(name: string): Promise<void> {
        await this.fillDepartment(name);

        await this.dialog.getByRole('button', { name: /Update Department/i }).click();

        await this.expectToastVisible();
    }

    async closeModal(): Promise<void> {
        await this.dialog.locator(locators.department.closeButton).click();

        await expect(this.dialog).not.toBeVisible();
    }

    async cancelModal(): Promise<void> {
        await this.dialog.getByRole('button', { name: /Cancel/i }).click();

        await expect(this.dialog).not.toBeVisible();
    }

    getDepartmentRow(departmentName: string) {
        return this.page.locator(locators.department.tableRow).filter({ hasText: departmentName });
    }

    async expectDepartmentVisible(departmentName: string): Promise<void> {
        await expect(this.getDepartmentRow(departmentName)).toBeVisible();
    }

    async expectDepartmentContains(departmentName: string): Promise<void> {
        const row = this.getDepartmentRow(departmentName);

        await expect(row).toBeVisible();
        await expect(row).toContainText(departmentName);
    }

    async expectDepartmentNotVisible(departmentName: string): Promise<void> {
        await expect(this.getDepartmentRow(departmentName)).not.toBeVisible();
    }

    async expectEditValue(name: string): Promise<void> {
        await expect(this.dialog.locator(locators.department.departmentName)).toHaveValue(name);
    }

    async expectValidationMessages(count: number): Promise<void> {
        await expect(this.dialog.locator(locators.department.validationMessage)).toHaveCount(count);
    }

    async expectPositionsTableVisible(): Promise<void> {
        await expect(this.dialog.locator(locators.department.positionsTable)).toBeVisible();
    }

    async expectEmployeesTableVisible(): Promise<void> {
        await expect(this.dialog.locator(locators.department.employeeTable)).toBeVisible();
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