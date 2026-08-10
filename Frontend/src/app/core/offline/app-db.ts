import Dexie, { Table } from 'dexie';

export interface PendingRequest {
  id?: number;
  operation: string;
  payload: unknown;
  status: 'pending' | 'syncing' | 'synced' | 'failed';
  createdAt: string;
}

export class AppDb extends Dexie {
  pendingRequests!: Table<PendingRequest>;
  constructor() {

    super('HRMS_DB');

    this.version(1).stores({
      pendingRequests: '++id,operation,status'
    });
  }
}

export const db = new AppDb();
