import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Customer } from './api.models';

@Injectable({ providedIn: 'root' })
export class CustomersApi {
  constructor(private http: HttpClient) {}

  list(q?: string) {
    const params: any = q ? { q } : {};
    return this.http.get<Customer[]>('/api/customers', { params });
  }

  create(c: Partial<Customer>) {
    return this.http.post<Customer>('/api/customers', c);
  }

  update(id: number, c: Partial<Customer>) {
    // Backend PUT /api/customers/{id} returns 204 No Content
    return this.http.put<void>(`/api/customers/${id}`, c, {
      observe: 'response',
    });
  }
}
