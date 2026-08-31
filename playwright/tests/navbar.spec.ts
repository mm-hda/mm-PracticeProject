import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/auth.page';
import { NavbarPage } from '../pages/navbar.page';

const VALID_EMAIL = 'admin@example.com';
const VALID_PASSWORD = '123456';
const USER_NAME = 'Admin';
const USER_EMAIL = VALID_EMAIL;
const USER_ROLE = 'Admin';

test.describe('Navbar', () => {
    let loginPage: LoginPage;
    let navbarPage: NavbarPage;

    test.beforeEach(async ({ page }) => {
        loginPage = new LoginPage(page);
        navbarPage = new NavbarPage(page);
        await loginPage.login(VALID_EMAIL, VALID_PASSWORD);
    });

    test('should display navbar', async () => {
        await navbarPage.expectNavbarVisible();
    });

    test('should display logged in user name', async () => {
        await navbarPage.expectUserName(USER_NAME);
    });

    test('should display logged in user email', async () => {
        await navbarPage.expectEmail(USER_EMAIL);
    });

    test('should display logged in user role', async () => {
        await navbarPage.expectRole(USER_ROLE);
    });

    test('should display complete user details', async () => {
        await navbarPage.expectUserDetails(USER_NAME, USER_EMAIL, USER_ROLE);
    });

    test('should display language selector', async () => {
        await expect(navbarPage.languageSelect).toBeVisible();
    });

    test('should change language to Hindi', async () => {
        await navbarPage.changeLanguage('हिन्दी');
    });

    test('should change language to Gujarati', async () => {
        await navbarPage.changeLanguage('ગુજરાતી');
    });

    test('should change language to English', async () => {
        await navbarPage.changeLanguage('English');
    });

    test('should display logout button', async () => {
        await expect(navbarPage.logoutButton).toBeVisible();
    });

    test('should logout successfully', async () => {
        await navbarPage.logoutAndExpectLogin();
    });
});
