import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Product } from './api.models';

@Injectable({ providedIn: 'root' })
export class ProductsApi {
  constructor(private http: HttpClient) {}
  list() { return this.http.get<Product[]>('/api/products'); }
}
