import { test, expect } from '@playwright/test';

import { LoginPage } from '../pages/auth.page';

const VALID_EMAIL = 'admin@example.com';
const VALID_PASSWORD = '123456';

test.describe('Login', () => {
    let loginPage: LoginPage;

    test.beforeEach(async ({ page }) => {
        loginPage = new LoginPage(page);
        await loginPage.goto();
    });

    test('should display the login page', async () => {
        await expect(loginPage.emailInput).toBeVisible();
        await expect(loginPage.passwordInput).toBeVisible();
        await expect(loginPage.loginButton).toBeVisible();
    });

    test('should display email input with email type', async () => {
        await expect(loginPage.emailInput).toHaveAttribute('type', 'email');
    });

    test('should display password input with password type', async () => {
        await expect(loginPage.passwordInput).toHaveAttribute('type', 'password');
    });

    test('should display login button', async () => {
        await expect(loginPage.loginButton).toBeVisible();
        await expect(loginPage.loginButton).toHaveAttribute('type', 'submit');
    });

    test('should show validation when both fields are empty', async () => {
        await loginPage.submit();

        await loginPage.expectValidationMessages(2);
        await loginPage.expectEmailValidation();
        await loginPage.expectPasswordValidation();
    });

    test('should show email validation when email is empty', async () => {
        await loginPage.fillPassword(VALID_PASSWORD);

        await loginPage.submit();

        await loginPage.expectEmailValidation();
    });

    test('should show password validation when password is empty', async () => {
        await loginPage.fillEmail(VALID_EMAIL);

        await loginPage.submit();

        await loginPage.expectPasswordValidation();
    });

    test('should show validation for invalid email format', async () => {
        await loginPage.fillLoginForm(
            'invalid-email',
            VALID_PASSWORD
        );

        await loginPage.submit();

        await loginPage.expectEmailValidation();
    });

    test('should show validation for email without @ symbol', async () => {
        await loginPage.fillLoginForm(
            'adminexample.com',
            VALID_PASSWORD
        );

        await loginPage.submit();

        await loginPage.expectEmailValidation();
    });

    test('should show validation for incomplete email', async () => {
        await loginPage.fillLoginForm(
            'admin@',
            VALID_PASSWORD
        );

        await loginPage.submit();

        await loginPage.expectEmailValidation();
    });

    test('should not submit when email is invalid', async ({ page }) => {
        await loginPage.fillLoginForm(
            'invalid-email',
            VALID_PASSWORD
        );

        await loginPage.submit();

        await expect(page).toHaveURL(/\/login\/?$/);
        await expect(loginPage.toast).not.toBeVisible();
    });

    test('should show toast for wrong password', async () => {
        await loginPage.fillLoginForm(
            VALID_EMAIL,
            'wrong-password'
        );

        await loginPage.submit();

        await expect(loginPage.toast).toBeVisible();
        await expect(loginPage.toastMessage).toBeVisible();
        await expect(loginPage.page).toHaveURL(/\/login\/?$/);
    });

    test('should show toast for wrong email', async () => {
        await loginPage.fillLoginForm(
            'wrong@example.com',
            VALID_PASSWORD
        );

        await loginPage.submit();

        await loginPage.expectToastVisible();
        await expect(loginPage.page).toHaveURL(/\/login\/?$/);
    });

    test('should show toast when both credentials are wrong', async () => {
        await loginPage.fillLoginForm(
            'wrong@example.com',
            'wrong-password'
        );

        await loginPage.submit();

        await loginPage.expectToastVisible();
        await expect(loginPage.page).toHaveURL(/\/login\/?$/);
    });

    test('should keep password hidden by default', async () => {
        await loginPage.expectPasswordType('password');
    });

    test('should show password when visibility toggle is clicked', async () => {
        await loginPage.expectPasswordType('password');

        await loginPage.togglePasswordVisibility();

        await loginPage.expectPasswordType('text');
    });

    test('should hide password again after clicking visibility toggle twice', async () => {
        await loginPage.expectPasswordType('password');

        await loginPage.togglePasswordVisibility();
        await loginPage.expectPasswordType('text');

        await loginPage.togglePasswordVisibility();
        await loginPage.expectPasswordType('password');
    });

    test('should successfully login with valid credentials', async () => {
        await loginPage.fillLoginForm(
            VALID_EMAIL,
            VALID_PASSWORD
        );

        await loginPage.submit();

        await expect(loginPage.page).toHaveURL(/\/dashboard/);
    });

    test('should display success toast after successful login', async () => {
        await loginPage.fillLoginForm(
            VALID_EMAIL,
            VALID_PASSWORD
        );

        await loginPage.submit();

        await expect(loginPage.page).toHaveURL(/\/dashboard/);
    });

    test('should allow closing the toast', async () => {
        await loginPage.fillLoginForm(
            VALID_EMAIL,
            'wrong-password'
        );

        await loginPage.submit();

        await loginPage.expectToastVisible();

        await loginPage.closeToast();
    });
});