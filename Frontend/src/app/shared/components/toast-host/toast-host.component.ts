import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';

import { ToastService } from '@app/core/services/toast.service';

@Component({
  standalone: true,
  selector: 'app-toast-host',
  imports: [CommonModule],
  templateUrl: './toast-host.component.html'
})
export class ToastHostComponent {
  protected readonly toastService = inject(ToastService);
}
