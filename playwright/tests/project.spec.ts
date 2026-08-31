import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/auth.page';
import { ProjectPage } from '../pages/project.page';

const VALID_EMAIL = 'admin@example.com';
const VALID_PASSWORD = '123456';
const PROJECT_NAME = 'Playwright Automation Project';
const PROJECT_DESCRIPTION = 'Project created for Playwright automation testing';
const START_DATE = '2026-09-01';
const END_DATE = '2026-12-31';
const PROJECT_MANAGER = 'Karatvya Shukala';
const EMPLOYEE_SEARCH = 'Harsh';

test.describe('Project', () => {
    let loginPage: LoginPage;
    let projectPage: ProjectPage;

    test.beforeEach(async ({ page }) => {
        loginPage = new LoginPage(page);
        projectPage = new ProjectPage(page);
        await loginPage.login(VALID_EMAIL, VALID_PASSWORD);
        await projectPage.goto();
    });

    test('should display the project page', async () => {
        await expect(projectPage.pageTitle).toBeVisible();
        await expect(projectPage.addProjectButton).toBeVisible();
        await expect(projectPage.projectTable).toBeVisible();
    });

    test('should display Add Project button', async () => {
        await expect(projectPage.addProjectButton).toBeVisible();
        await expect(projectPage.addProjectButton).toHaveAttribute('type', 'button');
    });

    test('should open Add Project modal', async () => {
        await projectPage.openAddProjectModal();
        await expect(projectPage.projectNameInput).toBeVisible();
        await expect(projectPage.projectDescriptionInput).toBeVisible();
        await expect(projectPage.startDateInput).toBeVisible();
        await expect(projectPage.endDateInput).toBeVisible();
        await expect(projectPage.projectManagerSelect).toBeVisible();
    });

    test('should display project name input', async () => {
        await projectPage.openAddProjectModal();
        await expect(projectPage.projectNameInput).toHaveAttribute('type', 'text');
    });

    test('should display start date input', async () => {
        await projectPage.openAddProjectModal();
        await expect(projectPage.startDateInput).toHaveAttribute('type', 'date');
    });

    test('should display end date input', async () => {
        await projectPage.openAddProjectModal();
        await expect(projectPage.endDateInput).toHaveAttribute('type', 'date');
    });

    test('should display project manager dropdown', async () => {
        await projectPage.openAddProjectModal();
        await expect(projectPage.projectManagerSelect).toBeVisible();
        await expect(projectPage.projectManagerSelect).toHaveAttribute('formControlName', 'projectManagerId');
    });

    test('should show validation when required project fields are empty', async () => {
        await projectPage.openAddProjectModal();
        await projectPage.submitProject();
        await projectPage.expectNameValidation();
        await projectPage.expectStartDateValidation();
        await projectPage.expectProjectManagerValidation();
    });

    test('should show validation when project name is empty', async () => {
        await projectPage.openAddProjectModal();
        await projectPage.fillProjectDescription(PROJECT_DESCRIPTION);
        await projectPage.fillStartDate(START_DATE);
        await projectPage.selectProjectManager(PROJECT_MANAGER);
        await projectPage.submitProject();
        await projectPage.expectNameValidation();
    });

    test('should show validation when start date is empty', async () => {
        await projectPage.openAddProjectModal();
        await projectPage.fillProjectName(PROJECT_NAME);
        await projectPage.selectProjectManager(PROJECT_MANAGER);
        await projectPage.submitProject();
        await projectPage.expectStartDateValidation();
    });

    test('should show validation when project manager is not selected', async () => {
        await projectPage.openAddProjectModal();
        await projectPage.fillProjectName(PROJECT_NAME);
        await projectPage.fillStartDate(START_DATE);
        await projectPage.submitProject();
        await projectPage.expectProjectManagerValidation();
    });

    test('should create project with valid project details', async () => {
        await projectPage.openAddProjectModal();
        await projectPage.fillProjectForm(PROJECT_NAME, PROJECT_DESCRIPTION, START_DATE, END_DATE, PROJECT_MANAGER);
        await projectPage.submitProject();
        await projectPage.expectToastVisible();
    });

    test('should create project without end date', async () => {
        await projectPage.openAddProjectModal();
        await projectPage.fillProjectName(`${PROJECT_NAME} Without End Date`);
        await projectPage.fillProjectDescription(PROJECT_DESCRIPTION);
        await projectPage.fillStartDate(START_DATE);
        await projectPage.selectProjectManager(PROJECT_MANAGER);
        await projectPage.submitProject();
        await projectPage.expectToastVisible();
    });

    test('should close Add Project modal using cancel button', async () => {
        await projectPage.openAddProjectModal();
        await projectPage.cancelProjectForm();
        await projectPage.expectModalClosed();
    });

    test('should open project details', async () => {
        await projectPage.openProjectDetails(PROJECT_NAME);
        await expect(projectPage.modal).toBeVisible();
    });

    test('should display project manager in project details', async () => {
        await projectPage.openProjectDetails(PROJECT_NAME);
        await projectPage.expectProjectDetails(PROJECT_NAME, PROJECT_MANAGER);
    });

    test('should close project details modal', async () => {
        await projectPage.openProjectDetails(PROJECT_NAME);
        await projectPage.closeModal();
        await projectPage.expectModalClosed();
    });

    test('should open Edit Project modal', async () => {
        await projectPage.openEditProject(PROJECT_NAME);
        await expect(projectPage.projectNameInput).toBeVisible();
        await expect(projectPage.startDateInput).toBeVisible();
        await expect(projectPage.projectManagerSelect).toBeVisible();
    });

    test('should open project employees modal', async () => {
        await projectPage.openProjectEmployees(PROJECT_NAME);
        await expect(projectPage.modal).toBeVisible();
    });

    test('should display project employees table', async () => {
        await projectPage.openProjectEmployees(PROJECT_NAME);
        await expect(projectPage.modal.locator('app-table')).toBeVisible();
    });

    test('should open Add Employee modal', async () => {
        await projectPage.openAddEmployee(PROJECT_NAME);
        await expect(projectPage.employeeSearchInput).toBeVisible();
        await expect(projectPage.employeeSearchButton).toBeVisible();
        await expect(projectPage.addEmployeeSubmitButton).toBeVisible();
    });

    test('should search employee by name', async () => {
        await projectPage.openAddEmployee(PROJECT_NAME);
        await projectPage.searchEmployee(EMPLOYEE_SEARCH);
        await expect(projectPage.employeeSelect).toBeVisible();
    });

    test('should display Harsh in employee search results', async () => {
        await projectPage.openAddEmployee(PROJECT_NAME);
        await projectPage.searchEmployee(EMPLOYEE_SEARCH);
        await expect(projectPage.employeeSelect).toBeVisible();
    });

    test('should select Harsh from employee search results', async () => {
        await projectPage.openAddEmployee(PROJECT_NAME);
        await projectPage.searchEmployee(EMPLOYEE_SEARCH);
        await projectPage.selectEmployee(EMPLOYEE_SEARCH);
        await expect(projectPage.employeeSelect).not.toHaveValue('');
    });

    test('should close Add Employee modal using cancel button', async () => {
        await projectPage.openAddEmployee(PROJECT_NAME);
        await projectPage.cancelButton.click();
        await projectPage.expectModalClosed();
    });
});
