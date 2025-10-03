import { CanActivateFn, ActivatedRouteSnapshot, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const required: string[] = route.data?.['roles'] ?? [];
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    router.navigate(['/login']);
    return false;
  }
  if (required.length === 0) return true;

  const ok = required.some((r) => auth.roles.includes(r));
  if (!ok) {
    router.navigate(['/customers']);
  }
  return ok;
};
