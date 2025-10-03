import { Injectable } from '@angular/core';
import { ToastrService, ActiveToast } from 'ngx-toastr';

@Injectable({ providedIn: 'root' })
export class Notify {
  constructor(private t: ToastrService) {}

  success(message: string, title?: string) {
    return this.t.success(message, title, {
      toastClass: 'ngx-toastr toast-success',
    });
  }

  error(message: string, title?: string) {
    return this.t.error(message, title, {
      toastClass: 'ngx-toastr toast-error',
    });
  }

  info(message: string, title?: string) {
    return this.t.info(message, title);
  }

  warn(message: string, title?: string) {
    return this.t.warning(message, title);
  }

  /** Handy helper for HTTP errors */
  httpError(e: any, fallback = 'Something went wrong') {
    const msg =
      e?.error?.title ||
      e?.error?.message ||
      e?.message ||
      e?.statusText ||
      fallback;
    this.error(msg);
  }
}
