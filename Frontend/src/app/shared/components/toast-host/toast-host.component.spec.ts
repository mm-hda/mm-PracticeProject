import { TestBed } from '@angular/core/testing';
import { describe, it, expect, vi, beforeEach } from 'vitest';

import { ToastHostComponent } from './toast-host.component';
import { ToastService } from '@app/core/services/toast.service';

describe('ToastHostComponent', () => {
  let toastServiceMock: {
    message: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    toastServiceMock = {
      message: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [ToastHostComponent],
      providers: [
        {
          provide: ToastService,
          useValue: toastServiceMock
        }
      ]
    }).compileComponents();
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(ToastHostComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should display the toast message', () => {
    toastServiceMock.message.mockReturnValue('User created successfully');
    const fixture = TestBed.createComponent(ToastHostComponent);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('User created successfully');
  });

  it('should not display a message when toast message is empty', () => {
    toastServiceMock.message.mockReturnValue('');
    const fixture = TestBed.createComponent(ToastHostComponent);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent.trim()).toBe('');
  });
});
