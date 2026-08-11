import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { ToastHostComponent } from '@app/shared/components/toast-host/toast-host.component';
import { SyncService } from '@app/core/services/sync.service';
import { LanguageService } from './core/services/language.service';

@Component({
  standalone: true,
  selector: 'app-root',
  imports: [RouterOutlet, ToastHostComponent],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './app.css',
})
export class App {
  private readonly syncService = inject(SyncService);
  private readonly languageService = inject(LanguageService);

  constructor() {
    this.syncService.syncPendingRequests();
    this.languageService.getCurrentLanguage();
  }
}
