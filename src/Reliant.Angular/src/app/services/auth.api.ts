import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { LoginResponse } from './api.models';

@Injectable({ providedIn: 'root' })
export class AuthApi {
  constructor(private http: HttpClient) {}
  login(email: string, password: string) {
    return this.http.post<LoginResponse>('/auth/login', { email, password });
  }
}
