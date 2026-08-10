import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { ToastHostComponent } from '@app/shared/components/toast-host/toast-host.component';
import { SyncService } from '@app/core/services/sync.service';

@Component({
  standalone: true,
  selector: 'app-root',
  imports: [RouterOutlet, ToastHostComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})

export class App {
  private readonly syncService = inject(SyncService);

  constructor() {
    this.syncService.syncPendingRequests();
  }
}
