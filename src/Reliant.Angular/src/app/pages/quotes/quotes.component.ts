import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { Table } from 'primeng/table';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { UI_IMPORTS } from '../../shared/ui-imports';
import { CreateQuoteComponent } from '../create-quote/create-quote.component';
import { QuotesApi } from '../../services/quotes.api';
import { QuoteResponse, QuoteSummaryRequest } from '../../services/api.models';
import { ConfirmationService } from 'primeng/api';
import { Notify } from '../../core/notify.service';

type Quote = QuoteResponse & { itemsCount?: number };

@Component({
  standalone: true,
  imports: [...UI_IMPORTS, CreateQuoteComponent],
  templateUrl: './quotes.component.html',
  providers: [ConfirmationService],
})
export class QuotesComponent implements OnInit {
  private quotesApi = inject(QuotesApi);
  private route = inject(ActivatedRoute);
  private location = inject(Location);
  private notify = inject(Notify);
  private confirm = inject(ConfirmationService);
  summaryLoading = false;
  summaryText: string | null = null;

  @ViewChild('dt') dt!: Table;

  customerId!: number;
  rows: Quote[] = [];
  loading = false;

  // table + search
  cols = Array(6).fill(0);
  q = '';
  private q$ = new Subject<string>();

  // create dialog
  createOpen = false;

  // view dialog
  viewOpen = false;
  selectedQuote?: Quote | null;
  ngOnInit() {
    this.customerId = Number(this.route.snapshot.paramMap.get('id') ?? 0);
    this.q$.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => {
      this.applyFilter();
    });
    this.fetchQuotes();
  }

  // --- data ---
  async fetchQuotes() {
    this.loading = true;
    try {
      const data =
        (await this.quotesApi.listByCustomer(this.customerId).toPromise()) ??
        [];
      this.rows = data.map((q) => ({ ...q, itemsCount: q.items?.length ?? 0 }));
      if (this.q && this.dt) this.applyFilter();
    } catch (e) {
      this.notify.httpError(e, 'Failed to load quotes');
    } finally {
      this.loading = false;
    }
  }

  // --- back nav ---
  goBack() {
    if (history.length > 1) this.location.back();
    else this.location.go('/customers');
  }

  // --- search ---
  onQueryChange(val: string) {
    this.q = val ?? '';
    this.q$.next(this.q);
  }
  applyFilter() {
    const value = this.q.trim();
    if (!this.dt) return;
    if (!value) return this.dt.clear();
    this.dt.filterGlobal(value, 'contains');
  }
  resetFilter() {
    this.q = '';
    this.dt?.clear();
  }

  // --- create dialog ---
  openCreate() {
    this.createOpen = true;
  }
  onCreateDone(evt: 'created' | 'cancelled') {
    this.createOpen = false;
    if (evt === 'created') this.fetchQuotes();
  }

  lineTotal(it: any): number {
    const qty = Number(it?.qty ?? 0);
    const price = Number(it?.unitPrice ?? 0);
    return qty * price || 0;
  }

  confirmDelete(q: Quote) {
    this.confirm.confirm({
      header: 'Delete quote',
      icon: 'pi pi-exclamation-triangle',
      message: `Are you sure you want to delete Quote #${q.id}? This cannot be undone.`,
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-danger',
      accept: async () => {
        await this.deleteQuote(q.id);
        await this.fetchQuotes();
      },
    });
  }

  statusSeverity(s: string) {
    switch ((s || '').toLowerCase()) {
      case 'accepted':
        return 'success';
      case 'rejected':
        return 'danger';
      case 'sent':
        return 'info';
      default:
        return 'warning';
    }
  }

  openView(q: Quote) {
    this.selectedQuote = q;
    this.viewOpen = true;

    // kick off AI summary
    this.loadSummaryFor(q);
  }

  async loadSummaryFor(q: Quote) {
    if (!q?.items?.length) {
      this.summaryText = 'No items to summarize.';
      this.summaryLoading = false;
      return;
    }
    this.summaryLoading = true;
    this.summaryText = null;

    const req: QuoteSummaryRequest = {
      customerId: q.customerId,
      items: q.items.map((i) => ({
        productId: i.productId,
        widthMm: i.widthMm,
        heightMm: i.heightMm,
        material: i.material,
        glazing: i.glazing,
        colorTier: i.colorTier ?? null,
        hardwareTier: i.hardwareTier ?? null,
        installComplexity: i.installComplexity ?? null,
        qty: i.qty,
      })),
    };

    try {
      const resp = await this.quotesApi.summarize(req).toPromise();
      this.summaryText = (resp?.text || '').trim() || 'No summary available.';
    } catch (e) {
      this.summaryText = 'Unable to generate summary right now.';
      this.notify.info('AI summary not available at the moment');
    } finally {
      this.summaryLoading = false;
    }
  }

  private async deleteQuote(id: number) {
    try {
      if ((this.quotesApi as any).delete) {
        await (this.quotesApi as any).delete(id).toPromise();
      } else if ((this.quotesApi as any).remove) {
        await (this.quotesApi as any).remove(id).toPromise();
      } else if ((this.quotesApi as any).deleteQuote) {
        await (this.quotesApi as any).deleteQuote(id).toPromise();
      } else {
        console.warn('No delete method found on QuotesApi; implement one.');
        return;
      }
      this.notify.success(`Quote #${id} deleted`);
    } catch (e) {
      this.notify.httpError(e, 'Failed to delete quote');
    }
  }

  regenerateSummary() {
    if (this.selectedQuote) this.loadSummaryFor(this.selectedQuote);
  }
}
