import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Table } from 'primeng/table';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

import { UI_IMPORTS, UI_PROVIDERS } from '../../shared/ui-imports';
import { MessageService } from 'primeng/api';
import { UserListItem } from '../../services/api.models';
import { AdminApi } from '../../services/admin.api';

@Component({
  standalone: true,
  providers: [MessageService, ...UI_PROVIDERS],
  imports: [...UI_IMPORTS],
  templateUrl: './admin.component.html',
})
export class AdminComponent implements OnInit {
  private fb = inject(FormBuilder);
  private api = inject(AdminApi);
  private msg = inject(MessageService);

  @ViewChild('dt') dt!: Table;

  // Table data
  users: (UserListItem & { rolesStr: string })[] = [];
  loading = false;

  // Search
  q = '';
  private q$ = new Subject<string>();

  // Columns (ID, FullName, Email, Roles, Action)
  cols = Array(5).fill(0);

  // Roles available
  roles = ['Manager', 'Sales'];

  // Modal state
  assignOpen = false;
  selected: UserListItem | null = null;

  // Assign form (used inside the modal)
  form = this.fb.group({
    userId: [null as number | null, [Validators.required, Validators.min(1)]],
    role: ['Sales', Validators.required],
  });

  ngOnInit() {
    this.q$
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(() => this.applyFilter());
    this.loadUsers();
  }

  // ---- Data ----
  async loadUsers() {
    this.loading = true;
    try {
      const data = await this.api.listUsers().toPromise();
      this.users = (data ?? []).map((u) => ({
        ...u,
        rolesStr: (u.roles ?? []).join(' '),
      }));
      if (this.q && this.dt) this.applyFilter();
    } finally {
      this.loading = false;
    }
  }

  // ---- Search ----
  onQueryChange(val: string) {
    this.q = val ?? '';
    this.q$.next(this.q);
  }
  applyFilter() {
    if (!this.dt) return;
    const value = this.q.trim();
    if (!value) return this.dt.clear();
    this.dt.filterGlobal(value, 'contains');
  }
  resetFilter() {
    this.q = '';
    this.dt?.clear();
  }

  // ---- Open modal for a row ----
  openAssign(u: UserListItem) {
    this.selected = u;
    this.form.reset({
      userId: u.id,
      role: u.roles?.[0] ?? 'Sales',
    });
    this.assignOpen = true;
  }

  // ---- Assign role ----
  async submitAssign() {
    if (this.form.invalid || this.loading) return;
    this.loading = true;
    try {
      const { userId, role } = this.form.value;
      const res = await this.api.assignRole(userId!, role!).toPromise();
      if (res?.status === 204) {
        this.msg.add({
          severity: 'success',
          summary: 'Role assigned',
          detail: `User #${userId} → ${role}`,
        });
        this.assignOpen = false;
        await this.loadUsers(); // refresh roles column
      } else {
        this.msg.add({
          severity: 'info',
          summary: 'No change',
          detail: 'User already has that role',
        });
      }
    } catch (e: any) {
      this.msg.add({
        severity: 'error',
        summary: 'Failed',
        detail: e?.error?.message ?? 'Could not assign role',
      });
    } finally {
      this.loading = false;
    }
  }

  // Optional stable ngFor
  trackById = (_: number, u: UserListItem) => u.id;
}
