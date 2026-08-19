import { Injectable } from '@angular/core';
import { db, PendingRequest } from '../offline/app-db';

@Injectable({
  providedIn: 'root'
})
export class OfflineQueueService {

  public async addRequest(operation: string, payload: unknown): Promise<void> {
    await db.pendingRequests.add({
      operation,
      payload,
      status: 'pending',
      createdAt: new Date().toISOString()
    });
  }

  public async markAsSynced(id: number): Promise<void> {
    await db.pendingRequests.delete(id);
  }

  public async getPendingUserRequests() {
    return await db.pendingRequests
      .where('operation')
      .equals('create-user')
      .toArray();
  }
}
