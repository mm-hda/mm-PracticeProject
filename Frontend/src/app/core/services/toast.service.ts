import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private readonly messageSignal = signal<string | null>(null);

  private timeoutId: any;
  message = this.messageSignal.asReadonly();

  show(message: string): void {
    this.messageSignal.set(message);

    this.timeoutId = setTimeout(() => {
      this.clear();
    }, 3000);
  }

  clear(): void {
    this.messageSignal.set(null);
    clearTimeout(this.timeoutId);
  }
}
