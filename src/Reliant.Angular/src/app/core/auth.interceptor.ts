import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { MessageService } from 'primeng/api';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const msg = inject(MessageService);
  const router = inject(Router);

  const token = auth.getToken();
  if (token)
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });

  return next(req).pipe(
    catchError((err) => {
      if (err.status === 401) {
        msg.add({
          severity: 'warn',
          summary: 'Session expired',
          detail: 'Please sign in again.',
        });
        auth.clear();
        router.navigate(['/login']);
      }
      return throwError(() => err);
    })
  );
};
