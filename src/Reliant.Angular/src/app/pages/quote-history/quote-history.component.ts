import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { NgFor, DatePipe, CurrencyPipe } from '@angular/common';

type QuoteResponse = {
  id: number;
  customerId: number;
  customerName: string;
  status: string;
  total: number;
  createdAt: string;
};

@Component({
  standalone: true,
  selector: 'app-quote-history',
  imports: [NgFor, DatePipe, CurrencyPipe],
  template: `
    <h2>Quotes for Customer #{{ customerId }}</h2>
    <ul>
      <li *ngFor="let q of quotes">
        <strong>#{{ q.id }}</strong> — {{ q.status }} —
        {{ q.total | currency : 'GBP' }} — {{ q.createdAt | date : 'short' }}
      </li>
    </ul>
  `,
})
export class QuoteHistoryComponent implements OnInit {
  customerId!: number;
  quotes: QuoteResponse[] = [];
  constructor(private route: ActivatedRoute, private http: HttpClient) {}
  ngOnInit() {
    this.customerId = Number(this.route.snapshot.paramMap.get('id'));
    this.http
      .get<QuoteResponse[]>(`/api/customers/${this.customerId}/quotes`)
      .subscribe((r) => (this.quotes = r));
  }
}
