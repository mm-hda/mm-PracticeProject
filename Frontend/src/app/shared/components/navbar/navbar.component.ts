import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthApiService } from '@app/core/services/auth-api.service';
import { StorageService } from '@app/core/services/storage.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink],
  templateUrl: './navbar.component.html',
})
export class NavbarComponent {

  private readonly authApi = inject(AuthApiService);
  private readonly storageService = inject(StorageService);
  public userName: string;
  public email: string;
  public role: string;

  public constructor(
    private readonly router: Router
  ) {

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

  public logout(): void {
    this.authApi.logout();
    this.storageService.clear();
    this.router.navigate(['/login']);
  }
}
