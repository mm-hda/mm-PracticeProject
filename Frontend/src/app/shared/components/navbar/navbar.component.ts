import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthApiService } from '@app/core/services/api-service/auth-api.service';
import { AuthService } from '@app/core/services/auth.service';
import { StorageService } from '@app/core/services/storage.service';
import { TranslatePipe } from '@ngx-translate/core';
import { LanguageService } from '@app/core/services/language.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, TranslatePipe],
  templateUrl: './navbar.component.html'
})
export class NavbarComponent {

  private readonly authApi = inject(AuthApiService);
  private readonly storageService = inject(StorageService);
  private readonly authService = inject(AuthService);

  private readonly languageService = inject(LanguageService);

  public userName: string;
  public email: string;
  public role: string;

  public readonly currentUser = this.authService.currentUser;

  public selectedLanguage = this.languageService.getCurrentLanguage();

  public constructor(private readonly router: Router) {

    const user = this.storageService.getItem<{
      userId: string;
      name: string;
      email: string;
      role: string;
    }>('auth_user');

    this.userName = user?.name ?? '';
    this.email = user?.email ?? '';
    this.role = user?.role ?? '';
  }

  public changeLanguage(event: Event): void {

    const language = (event.target as HTMLSelectElement).value;

    this.selectedLanguage = language;
    this.languageService.changeLanguage(language);
  }

  public logout(): void {
    this.authApi.logout();
    this.authService.clearCurrentUser();
    this.storageService.removeItem('auth_user');
    this.storageService.removeItem('branches');
    this.storageService.removeItem('departments');
    this.storageService.removeItem('positions');
    this.storageService.removeItem('roles');
    this.router.navigate(['/login']);
  }
}
