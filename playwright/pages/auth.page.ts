import { expect, Page } from '@playwright/test';

import locators from '../utils/locators.json';

export class LoginPage {
    constructor(readonly page: Page) { }

    get emailInput() {
        return this.page.locator(locators.login.username);
    }

    get passwordInput() {
        return this.page.locator(locators.login.password);
    }

    get loginButton() {
        return this.page.locator(locators.login.loginButton);
    }

    get passwordToggle() {
        return this.page.locator(locators.login.passwordToggle);
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
        await this.page.goto('/login');

        await expect(this.page).toHaveURL(/\/login\/?$/);
        await expect(this.emailInput).toBeVisible();
        await expect(this.passwordInput).toBeVisible();
        await expect(this.loginButton).toBeVisible();
    }

    async fillEmail(email: string): Promise<void> {
        await this.emailInput.fill(email);
    }

    async fillPassword(password: string): Promise<void> {
        await this.passwordInput.fill(password);
    }

    async fillLoginForm(email: string, password: string): Promise<void> {
        await this.fillEmail(email);
        await this.fillPassword(password);
    }

    async submit(): Promise<void> {
        await this.loginButton.click();
    }

    async login(email: string, password: string): Promise<void> {
        await this.goto();
        await this.fillLoginForm(email, password);
        await this.submit();

        await expect(this.page).toHaveURL(/dashboard/);
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

    async expectEmailValidation(): Promise<void> {
        await expect(
            this.page.locator('.text-danger').filter({
                hasText: /email/i
            })
        ).toBeVisible();
    }

    async expectPasswordValidation(): Promise<void> {
        await expect(
            this.page.locator('.text-danger').filter({
                hasText: /password/i
            })
        ).toBeVisible();
    }

    async expectValidationMessages(count: number): Promise<void> {
        await expect(this.page.locator('.text-danger')).toHaveCount(count);
    }

    async togglePasswordVisibility(): Promise<void> {
        await this.passwordToggle.click();
    }

    async expectPasswordType(type: 'password' | 'text'): Promise<void> {
        await expect(this.passwordInput).toHaveAttribute('type', type);
    }
}