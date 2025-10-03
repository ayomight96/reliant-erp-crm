import { Component, inject, ViewChild } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { AvatarModule } from 'primeng/avatar';
import { MenuModule } from 'primeng/menu';
import { Menu } from 'primeng/menu';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { AuthService } from '../../core/auth.service';

@Component({
  standalone: true,
  selector: 'app-layout',
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    ButtonModule,
    TagModule,
    AvatarModule,
    MenuModule,
    ConfirmDialogModule,
  ],
  templateUrl: './layout.component.html',
})
export class LayoutComponent {
  auth = inject(AuthService);
  private confirm = inject(ConfirmationService);
  menuOpen = false;

  @ViewChild('userMenu') userMenu!: Menu; // ⬅️ to close popup before opening modal
  userMenuItems: MenuItem[] = []; // ⬅️ field, not getter

  ngOnInit() {
    // Build once
    if (this.auth.isManager()) {
      this.userMenuItems.push({
        label: 'Admin',
        icon: 'pi pi-shield',
        routerLink: '/admin',
      });
      this.userMenuItems.push({ separator: true });
    }
    this.userMenuItems.push({
      label: 'Sign out',
      icon: 'pi pi-power-off',
      command: () => {
        this.userMenu?.hide();
        this.confirmLogoutModal();
      },
    });
  }

  get crumb() {
    const url = location.pathname.replace(/^\/|\/$/g, '');
    if (!url) return 'Dashboard';
    return url
      .split('/')
      .map((s) => s[0].toUpperCase() + s.slice(1))
      .join(' / ');
  }

  get avatarInitials(): string {
    const e = this.auth.email ?? '';
    const name = e.split('@')[0] ?? '';
    const parts = name.split(/[._-]/).filter(Boolean);
    const a = (parts[0]?.[0] ?? name[0] ?? 'U').toUpperCase();
    const b = (parts[1]?.[0] ?? '').toUpperCase();
    return (a + b).trim();
  }

  confirmLogoutModal() {
    this.confirm.confirm({
      key: 'layoutConfirm',
      header: 'Sign out',
      message: 'Sign out of your session?',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sign out',
      rejectLabel: 'Cancel',
      // ✅ polished styles via custom classes
      acceptButtonStyleClass: 'btn-logout',
      rejectButtonStyleClass: 'btn-cancel',
      // a couple nice defaults
      closeOnEscape: true,
      blockScroll: true,
      defaultFocus: 'reject', // focus "Cancel" first
      accept: () => this.auth.logout(),
    });
  }
}
