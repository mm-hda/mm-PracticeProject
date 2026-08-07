import { Injectable } from '@angular/core';
import * as CryptoJS from 'crypto-js';

@Injectable({ providedIn: 'root' })
export class StorageService {

  private readonly secretKey = 'HrmsLocalStorageKey';

  setItem(key: string, value: unknown): void {

    const encrypted = CryptoJS.AES.encrypt(JSON.stringify(value), this.secretKey).toString();

    localStorage.setItem(key, encrypted);
  }

  getItem<T>(key: string): T | null {

    const encrypted = localStorage.getItem(key);

    if (!encrypted) { return null; }

    try {
      const bytes = CryptoJS.AES.decrypt(encrypted, this.secretKey);

      return JSON.parse(bytes.toString(CryptoJS.enc.Utf8)) as T;
    }
    catch {
      return null;
    }
  }

  removeItem(key: string): void {
    localStorage.removeItem(key);
  }

  clear(): void {
    localStorage.clear();
  }
}
