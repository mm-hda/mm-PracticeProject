import {
  Component,
  Input
} from '@angular/core';

import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './generic-table.component.html'
})
export class GenericTableComponent {
  @Input({ required: true })
  public columns: TableColumn[] = [];

  @Input({ required: true })
  public data: unknown[] = [];
}

export interface TableColumn {
  key: string;
  title: string;
}
