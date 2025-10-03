import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Table } from 'primeng/table';
import { UI_IMPORTS } from '../../shared/ui-imports';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

type Product = {
  id: number;
  name: string;
  productType: 'window' | 'door' | 'conservatory' | string;
  basePrice: number;
};

@Component({
  standalone: true,
  imports: [...UI_IMPORTS],
  templateUrl: './products.component.html',
})
export class ProductsComponent implements OnInit {
  private http = inject(HttpClient);
  @ViewChild('dt') dt!: Table;

  rows: Product[] = []; // full dataset (client filtering uses this)
  loading = false;
  q = '';
  cols = Array(3).fill(0);

  private query$ = new Subject<string>();

  ngOnInit() {
    // Debounced client-side filtering
    this.query$
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(() => this.search());

    this.refresh(); // load data once
  }

  // Keystroke handler
  onQueryChange(val: string) {
    this.q = val ?? '';
    this.query$.next(this.q);
  }

  // Apply global filter to the table (client-side)
  search() {
    const q = this.q?.trim() ?? '';
    if (!this.dt) return;
    if (!q) {
      this.dt.clear(); // removes all filters & shows full list
      return;
    }
    this.dt.filterGlobal(q, 'contains');
  }

  // Clear input + filters
  reset() {
    this.q = '';
    this.dt?.clear();
  }

  // (Optional) Re-fetch from server
  async refresh() {
    this.loading = true;
    try {
      this.rows =
        (await this.http.get<Product[]>('/api/products').toPromise()) ?? [];
      // Re-apply current query if user typed before refresh finished
      if (this.q) this.search();
    } finally {
      this.loading = false;
    }
  }

  export(table: Table) {
    table.exportCSV();
  }

  tagSeverity(p: Product) {
    switch (p.productType) {
      case 'window':
        return 'info';
      case 'door':
        return 'warning';
      case 'conservatory':
        return 'success';
      default:
        return 'secondary';
    }
  }
}
