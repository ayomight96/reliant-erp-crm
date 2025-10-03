import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
  CreateQuoteRequest,
  QuoteResponse,
  QuoteSummaryRequest,
} from './api.models';

@Injectable({ providedIn: 'root' })
export class QuotesApi {
  constructor(private http: HttpClient) {}

  /** GET /api/customers/{customerId}/quotes */
  listByCustomer(customerId: number) {
    return this.http.get<QuoteResponse[]>(
      `/api/customers/${customerId}/quotes`
    );
  }

  /** POST /api/quotes */
  create(req: CreateQuoteRequest) {
    return this.http.post<QuoteResponse>('/api/quotes', req);
  }

  /** POST /api/quotes/summary */
  summarize(req: QuoteSummaryRequest) {
    return this.http.post<{ text: string }>('/api/quotes/summary', req);
  }
}
