import { Injectable, computed, signal, inject } from '@angular/core';
import { StorageService } from './storage.service';
import { TokenDto } from '../models/authModels/token.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly storageService = inject(StorageService);

  private readonly currentUserSignal = signal<TokenDto | null>(null);

  public readonly currentUser = this.currentUserSignal.asReadonly();

  public readonly isAuthenticated = computed(() => !!this.currentUserSignal());

  constructor() {
    const user = this.storageService.getItem<TokenDto>('auth_user');

    if (user) { this.currentUserSignal.set(user); }
  }

  public setCurrentUser(user: TokenDto): void {

    this.storageService.setItem('auth_user', user);

    this.currentUserSignal.set(user);
  }

  public clearCurrentUser(): void {

    this.storageService.removeItem('auth_user');

    this.currentUserSignal.set(null);
  }
}
