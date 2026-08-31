import { expect, Page } from '@playwright/test';
import locators from '../utils/locators.json';

export class ProjectPage {
    constructor(readonly page: Page) { }

    get pageTitle() {
        return this.page.locator(locators.project.pageTitle);
    }
    get addProjectButton() {
        return this.page.locator(locators.project.addProjectButton);

    }
    get projectTable() {
        return this.page.locator(locators.project.projectTable);
    }
    get projectRows() {
        return this.page.locator(locators.project.projectRows);
    }
    get projectNameInput() {
        return this.page.locator(locators.project.projectNameInput);
    }
    get projectDescriptionInput() {
        return this.page.locator(locators.project.projectDescriptionInput);
    }
    get startDateInput() {
        return this.page.locator(locators.project.startDateInput);
    }
    get endDateInput() {
        return this.page.locator(locators.project.endDateInput);
    }
    get projectManagerSelect() {
        return this.page.locator(locators.project.projectManagerSelect);
    }
    get cancelButton() {
        return this.page.locator(locators.project.cancelButton);
    }
    get submitButton() {
        return this.page.locator(locators.project.submitButton);
    }
    get modal() {
        return this.page.locator(locators.project.modal);
    }
    get modalTitle() {
        return this.page.locator(locators.project.modalTitle);
    }
    get modalCloseButton() {
        return this.page.locator(locators.project.modalCloseButton);
    }
    get projectDetailButton() {
        return this.page.locator(locators.project.projectDetailButton);
    }
    get projectEmployeesButton() {
        return this.page.locator(locators.project.projectEmployeesButton);
    }
    get editProjectButton() {
        return this.page.locator(locators.project.editProjectButton);
    }
    get addEmployeeButton() {
        return this.page.locator(locators.project.addEmployeeButton);
    }
    get employeeModal() {
        return this.page.locator(locators.project.employeeModal);
    }
    get employeeSearchInput() {
        return this.page.locator(locators.project.employeeSearchInput);
    }
    get employeeSearchButton() {
        return this.page.locator(locators.project.employeeSearchButton);
    }
    get employeeSelect() {
        return this.page.locator(locators.project.employeeSelect);
    }
    get addEmployeeSubmitButton() {
        return this.page.locator(locators.project.addEmployeeSubmitButton);
    }
    get toast() {
        return this.page.locator(locators.toast.container);
    }
    get toastMessage() {
        return this.page.locator(locators.toast.message);
    }
    get toastCloseButton() {
        return this.page.locator(locators.toast.closeButton);
    }

    async goto(): Promise<void> {
        await this.page.goto('/projects');
        await expect(this.page).toHaveURL(/\/projects\/?$/);
        await expect(this.pageTitle).toBeVisible();
        await expect(this.addProjectButton).toBeVisible();
    }

    async openAddProjectModal(): Promise<void> {
        await this.addProjectButton.click();
        await expect(this.modal).toBeVisible();
        await expect(this.modalTitle).toContainText(/Add Project/i);
    }

    async fillProjectName(name: string): Promise<void> {
        await this.projectNameInput.fill(name);
    }
    async fillProjectDescription(description: string): Promise<void> {
        await this.projectDescriptionInput.fill(description);
    }
    async fillStartDate(date: string): Promise<void> {
        await this.startDateInput.fill(date);
    }
    async fillEndDate(date: string): Promise<void> {
        await this.endDateInput.fill(date);
    }
    async selectProjectManager(managerName: string): Promise<void> {
        await this.projectManagerSelect.selectOption({ label: managerName });
    }

    async fillProjectForm(name: string, description: string, startDate: string, endDate: string, managerName: string): Promise<void> {
        await this.fillProjectName(name);
        await this.fillProjectDescription(description);
        await this.fillStartDate(startDate);
        await this.fillEndDate(endDate);
        await this.selectProjectManager(managerName);
    }

    async submitProject(): Promise<void> { await this.submitButton.click(); }

    async cancelProjectForm(): Promise<void> {
        await this.cancelButton.click();
        await expect(this.modal).not.toBeVisible();
    }

    projectRow(projectName: string) { return this.projectRows.filter({ hasText: projectName }); }

    async expectProjectVisible(projectName: string): Promise<void> { await expect(this.projectRow(projectName)).toBeVisible(); }

    async openProjectDetails(projectName: string): Promise<void> {
        const row = this.projectRow(projectName);
        await row.getByRole('button').nth(0).click();
        await expect(this.modal).toBeVisible();
        await expect(this.modalTitle).toContainText(/Project Detail/i);
    }

    async openEditProject(projectName: string): Promise<void> {
        const row = this.projectRow(projectName);
        await row.getByRole('button').nth(2).click();
        await expect(this.modal).toBeVisible();
        await expect(this.modalTitle).toContainText(/Edit Project/i);
    }

    async openProjectEmployees(projectName: string): Promise<void> {
        const row = this.projectRow(projectName);
        await row.getByRole('button').nth(1).click();
        await expect(this.modal).toBeVisible();
        await expect(this.modalTitle).toContainText(/Employees/i);
    }

    async openAddEmployee(projectName: string): Promise<void> {
        const row = this.projectRow(projectName);
        await row.getByRole('button').nth(3).click();
        await expect(this.employeeModal).toBeVisible();
    }

    async searchEmployee(employeeName: string): Promise<void> {
        await this.employeeSearchInput.fill(employeeName);
        await this.employeeSearchButton.click();
    }

    async selectEmployee(employeeName: string): Promise<void> {
        const option = this.employeeSelect.locator('option').filter({ hasText: employeeName }).first();
        const value = await option.getAttribute('value');
        expect(value).toBeTruthy();
        await this.employeeSelect.selectOption(value!);
    }

    async addEmployeeToProject(employeeName: string): Promise<void> {
        await this.searchEmployee(employeeName);
        await expect(this.employeeSelect).toBeVisible();
        await this.selectEmployee(employeeName);
        await this.addEmployeeSubmitButton.click();
    }

    async expectProjectDetails(projectName: string, managerName: string): Promise<void> {
        await expect(this.modal.locator(locators.project.detailProjectName)).toContainText(projectName);
        await expect(this.modal.locator(locators.project.detailProjectManager)).toContainText(managerName);
    }

    async expectNameValidation(): Promise<void> {
        await expect(this.page.locator('.text-danger').filter({ hasText: /name/i })).toBeVisible();
    }
    async expectStartDateValidation(): Promise<void> {
        await expect(this.page.locator('.text-danger').filter({ hasText: /start date/i })).toBeVisible();
    }
    async expectProjectManagerValidation(): Promise<void> {
        await expect(this.page.locator('.text-danger').filter({ hasText: /manager/i })).toBeVisible();
    }
    async expectValidationMessages(count: number): Promise<void> {
        await expect(this.page.locator('.text-danger')).toHaveCount(count);
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
        await this.toastCloseButton.click();
        await expect(this.toast).not.toBeVisible();
    }

    async closeModal(): Promise<void> {
        await this.modalCloseButton.click();
        await expect(this.modal).not.toBeVisible();
    }

    async expectModalClosed(): Promise<void> { await expect(this.modal).not.toBeVisible(); }
}
