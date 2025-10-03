import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';

type JwtPayload = { exp?: number; email?: string; role?: string[] | string };

@Injectable({ providedIn: 'root' })
export class AuthService {
  private key = 'jwt';
  private platformId = inject(PLATFORM_ID);
  private router = inject(Router);

  private get canUseStorage() {
    return isPlatformBrowser(this.platformId) && typeof localStorage !== 'undefined';
  }

  setToken(token: string) {
    if (this.canUseStorage) localStorage.setItem(this.key, token);
  }
  getToken(): string | null {
    return this.canUseStorage ? localStorage.getItem(this.key) : null;
  }
  clear() {
    if (this.canUseStorage) localStorage.removeItem(this.key);
  }

  isAuthenticated(): boolean {
    const t = this.getToken();
    if (!t) return false;
    const p = this.decode(t);
    if (p?.exp && Date.now() / 1000 > p.exp) {
      this.logout();
      return false;
    }
    return true;
  }

  logout() {
    this.clear();
    this.router.navigate(['/login']);
  }

  get email(): string | undefined { return this.decode(this.getToken() || '')?.email; }
  get roles(): string[] {
    const p = this.decode(this.getToken() || '');
    const r = p?.role;
    return Array.isArray(r) ? r : (r ? [r] : []);
  }
  isManager() { return this.roles.includes('Manager'); }
  isSales() { return this.roles.includes('Sales'); }

  private decode(token: string): JwtPayload | undefined {
    try {
      const base = token.split('.')[1];
      const json = atob(base.replace(/-/g, '+').replace(/_/g, '/'));
      console.log( JSON.parse(json) );
      return JSON.parse(json);
    } catch { return undefined; }
  }
}
