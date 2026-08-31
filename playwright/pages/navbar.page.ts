import { expect, Page } from '@playwright/test';
import locators from '../utils/locators.json';

export class NavbarPage {
    constructor(readonly page: Page) { }

    get navbar() { return this.page.locator(locators.navbar.container); }
    get userName() { return this.page.locator(locators.navbar.userName); }
    get email() { return this.page.locator(locators.navbar.email); }
    get role() { return this.page.locator(locators.navbar.role); }
    get languageSelect() { return this.page.locator(locators.navbar.languageSelect); }
    get logoutButton() { return this.page.locator(locators.navbar.logoutButton); }

    async expectNavbarVisible(): Promise<void> {
        await expect(this.navbar).toBeVisible();
    }

    async expectUserName(name: string): Promise<void> {
        await expect(this.userName).toContainText(name);
    }

    async expectEmail(email: string): Promise<void> {
        await expect(this.email).toContainText(email);
    }

    async expectRole(role: string): Promise<void> {
        await expect(this.role).toContainText(role);
    }

    async expectUserDetails(name: string, email: string, role: string): Promise<void> {
        await this.expectUserName(name);
        await this.expectEmail(email);
        await this.expectRole(role);
    }

    async selectLanguage(language: string): Promise<void> {
        await this.languageSelect.selectOption(language);
    }

    async changeLanguage(language: string): Promise<void> {
        await this.selectLanguage(language);
    }

    async logout(): Promise<void> {
        await this.logoutButton.click();
    }

    async logoutAndExpectLogin(): Promise<void> {
        await this.logout();
        await expect(this.page).toHaveURL(/\/login\/?$/);
    }
}
