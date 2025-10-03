import { Component, inject } from '@angular/core';
import {
  FormBuilder,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../core/auth.service';
import { CommonModule } from '@angular/common';
import { UI_IMPORTS } from '../../shared/ui-imports';
import { Notify } from '../../core/notify.service';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, CardModule, ...UI_IMPORTS],
  templateUrl: './login.component.html',
})
export class LoginComponent {
  fb = inject(FormBuilder);
  http = inject(HttpClient);
  auth = inject(AuthService);
  private notify = inject(Notify);
  router = inject(Router);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
    remember: [true],
  });

  get email() {
    return this.form.get('email');
  }
  get password() {
    return this.form.get('password');
  }

  loading = false;
  error?: string;

  fillDemo(kind: 'sales' | 'manager') {
    const email = kind === 'sales' ? 'sales@demo.local' : 'manager@demo.local';
    this.form.patchValue({ email, password: 'Passw0rd!' });
  }

  async submit() {
    if (this.form.invalid || this.loading) return;
    this.loading = true;
    this.error = undefined;
    try {
      const r = await this.http
        .post<any>('/auth/login', this.form.value)
        .toPromise();
      this.auth.setToken(r.accessToken);
      this.notify.success('Welcome back!');
      this.router.navigate(['/']);
    } catch (e: any) {
      this.error = 'Invalid email or password';
      this.notify.error('Invalid email or password');
    } finally {
      this.loading = false;
    }
  }
}
