import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { Table } from 'primeng/table';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { UI_IMPORTS } from '../../shared/ui-imports';
import { FormBuilder, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../core/auth.service';
import { Customer } from '../../services/api.models';
import { CustomersApi } from '../../services/customer.api';
import { Notify } from '../../core/notify.service';

@Component({
  standalone: true,
  imports: [...UI_IMPORTS],
  providers: [MessageService],
  templateUrl: './customers.component.html',
})
export class CustomersComponent implements OnInit {
  private api = inject(CustomersApi);
  private fb = inject(FormBuilder);
  private msg = inject(MessageService);
  private notify = inject(Notify);
  auth = inject(AuthService);

  @ViewChild('dt') dt!: Table;

  rows: Customer[] = [];
  q = '';
  loading = false;

  // modal
  modalOpen = false;
  formMode: 'create' | 'update' = 'create';
  editing: Customer | null = null;
  saving = false;

  private query$ = new Subject<string>();

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    email: [''],
    phone: [''],
    addressLine1: [''],
    city: [''],
    postcode: [''],
  });

  async ngOnInit() {
    this.query$
      .pipe(debounceTime(350), distinctUntilChanged())
      .subscribe(() => {
        this.search();
      });
    await this.search();
  }

  // ----- data -----
  async search() {
    this.loading = true;
    try {
      const list = await this.api.list(this.q || undefined).toPromise();
      this.rows = list ?? [];
    } catch (e) {
      this.notify.httpError(e, 'Failed to load customers');
    } finally {
      this.loading = false;
    }
  }
  async refresh() {
    await this.search();
  }
  async reset() {
    this.q = '';
    (this.dt as any)?.clear?.();
    await this.search();
  }
  onQueryChange(val: string) {
    this.q = val ?? '';
    this.query$.next(this.q);
  }

  // ----- modal openers -----
  openCreate() {
    this.formMode = 'create';
    this.editing = null;
    this.form.reset({
      name: '',
      email: '',
      phone: '',
      addressLine1: '',
      city: '',
      postcode: '',
    });
    this.modalOpen = true;
  }

  openEdit(c: Customer) {
    if (!this.auth.isManager()) return;
    this.formMode = 'update';
    this.editing = c;
    this.form.reset({
      name: c.name ?? '',
      email: c.email ?? '',
      phone: c.phone ?? '',
      addressLine1: c.addressLine1 ?? '',
      city: c.city ?? '',
      postcode: c.postcode ?? '',
    });
    this.modalOpen = true;
  }

  private buildPayload(): Partial<Customer> {
    const v = this.form.getRawValue(); // all strings (non-nullable)
    // Trim & normalize: empty string becomes undefined (except name which is required)
    return {
      name: v.name.trim(),
      email: v.email.trim() || undefined,
      phone: v.phone.trim() || undefined,
      addressLine1: v.addressLine1.trim() || undefined,
      city: v.city.trim() || undefined,
      postcode: v.postcode.trim() || undefined,
    };
  }
  cancel() {
    this.modalOpen = false;
    this.editing = null;
    this.saving = false;
  }

  // ----- save -----
  async save() {
    if (this.form.invalid) return;
    this.saving = true;
    const payload = this.buildPayload();

    try {
      if (this.formMode === 'create') {
        const created = await this.api.create(payload).toPromise();
        if (created) this.rows = [created, ...this.rows];
        this.notify.success('Customer added');
      } else {
        if (!this.editing) return;
        if (!this.auth.isManager()) {
          this.notify.warn('Only managers can update.');
          return;
        }
        const res = await this.api.update(this.editing.id, payload).toPromise();
        if (res?.status === 204) {
          this.rows = this.rows.map((r) =>
            r.id === this.editing!.id ? ({ ...r, ...payload } as Customer) : r
          );
          this.notify.success('Customer updated');
        }
      }
      this.modalOpen = false;
      this.editing = null;
    } catch (e: any) {
      this.notify.httpError(e, 'Save failed');
    } finally {
      this.saving = false;
    }
  }

  // optional: stable trackBy
  trackById = (_: number, c: Customer) => c.id;
}
