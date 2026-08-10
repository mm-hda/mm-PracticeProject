import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { OfflineQueueService } from './offline-queue.service';
import { UserApiService } from './user-api.service';

import { CreateUserRequest } from '../models/userModels/user.model';
import { ToastService } from './toast.service';

@Injectable({
  providedIn: 'root'
})
export class SyncService {

  private readonly queue = inject(OfflineQueueService);
  private readonly userApi = inject(UserApiService);

  private readonly toastService = inject(ToastService);

  constructor() {
    window.addEventListener('online', () => {

      console.log('Internet restored');
      this.syncPendingRequests();
    }
    );
  }

  public async syncPendingRequests(): Promise<void> {
    const requests = await this.queue.getPendingUserRequests();

    for (const request of requests) {
      try {
        switch (request.operation) {
          case 'create-user':
            await firstValueFrom(this.userApi.createUser(request.payload as CreateUserRequest));
            this.toastService.show('User created successfully');
            window.location.href = '/users';
            await this.queue.markAsSynced(request.id!);
            break;
        }
      }

      catch (error) {
        break;
      }
    }
  }
}
