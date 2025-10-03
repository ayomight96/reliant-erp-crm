import {
  Component,
  OnInit,
  inject,
  Input,
  Output,
  EventEmitter,
} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { UI_IMPORTS } from '../../shared/ui-imports';
import { QuotesApi } from '../../services/quotes.api';
import {
  CreateQuoteRequest,
  QuoteItemCreateRequest,
} from '../../services/api.models';
import { Notify } from '../../core/notify.service';

type Product = {
  id: number;
  name: string;
  productType: string;
  basePrice: number;
};

@Component({
  standalone: true,
  providers: [MessageService],
  imports: [...UI_IMPORTS],
  templateUrl: './create-quote.component.html',
  selector: 'app-create-quote',
})
export class CreateQuoteComponent implements OnInit {
  private http = inject(HttpClient); // for /api/products
  private quotes = inject(QuotesApi); // your service
  private fb = inject(FormBuilder);
  private msg = inject(MessageService);
  private notify = inject(Notify);

  @Input() inline = false;
  @Input() customerId?: number | null;
  @Output() done = new EventEmitter<'created' | 'cancelled'>();

  products: Product[] = [];
  productsLoading = false;

  materials = ['uPVC', 'Aluminium', 'Composite'];
  glazings = ['double', 'triple'];

  submitting = false;

  form = this.fb.group({
    customerId: [1, Validators.required],
    productId: [null as number | null, Validators.required],
    widthMm: [1200, [Validators.required, Validators.min(300)]],
    heightMm: [900, [Validators.required, Validators.min(300)]],
    material: ['uPVC', Validators.required],
    glazing: ['double', Validators.required],
    qty: [1, [Validators.required, Validators.min(1)]],

    // pricing
    useAi: [true], // NEW: toggle
    unitPrice: [null as number | null], // disabled when useAi === true

    // Optional fields present in your API
    colorTier: [null as string | null],
    hardwareTier: [null as string | null],
    installComplexity: [null as string | null],
    notes: [null as string | null],
  });

  async ngOnInit() {
    // Load products
    this.productsLoading = true;
    try {
      this.products =
        (await this.http.get<Product[]>('/api/products').toPromise()) ?? [];
    } catch (e) {
      this.notify.httpError(e, 'Failed to load products');
    } finally {
      this.productsLoading = false;
    }

    // Pre-fill customer
    if (this.customerId != null) {
      this.form.patchValue({ customerId: this.customerId });
    }

    // Toggle unitPrice control based on useAi
    this.form.get('useAi')!.valueChanges.subscribe((on) => {
      const unit = this.form.get('unitPrice')!;
      if (on) {
        unit.disable({ emitEvent: false });
        unit.setValue(null, { emitEvent: false });
      } else {
        unit.enable({ emitEvent: false });
      }
    });
    // initialize disabled state
    if (this.form.value.useAi)
      this.form.get('unitPrice')!.disable({ emitEvent: false });
  }

  get aiPlaceholderText(): string {
    return this.form.get('useAi')?.value ? 'AI will suggest a price' : '';
  }

  // -------- computed helpers for summary --------
  get selectedProduct(): Product | undefined {
    const id = this.form.value.productId ?? undefined;
    return this.products.find((p) => p.id === id);
  }

  get computedUnit(): number {
    // If custom price entered, use it. Otherwise fallback to basePrice.
    const custom = this.form.value.unitPrice ?? null;
    if (custom != null) return Number(custom);
    return Number(this.selectedProduct?.basePrice ?? 0);
  }

  get lineTotal(): number {
    const qty = Number(this.form.value.qty ?? 0);
    return this.computedUnit * (isFinite(qty) ? qty : 0);
  }

  get productName(): string {
    return this.selectedProduct?.name ?? '—';
  }

  resetForm() {
    const cust = this.form.value.customerId ?? 1;
    this.form.reset({
      customerId: cust,
      productId: null,
      widthMm: 1200,
      heightMm: 900,
      material: 'uPVC',
      glazing: 'double',
      qty: 1,
      useAi: true,
      unitPrice: null,
      colorTier: null,
      hardwareTier: null,
      installComplexity: null,
      notes: null,
    });
    // keep disabled state in sync
    this.form.get('unitPrice')!.disable({ emitEvent: false });
  }

  // -------- submit --------
  async submit() {
    if (this.form.invalid || this.submitting) return;

    this.submitting = true;
    try {
      const v = this.form.value;

      const item: QuoteItemCreateRequest = {
        productId: v.productId!,
        widthMm: v.widthMm!,
        heightMm: v.heightMm!,
        material: v.material!,
        glazing: v.glazing!,
        colorTier: v.colorTier ?? null,
        hardwareTier: v.hardwareTier ?? null,
        installComplexity: v.installComplexity ?? null,
        qty: v.qty!,
        unitPrice: v.useAi ? null : v.unitPrice ?? null,
      };

      const payload: CreateQuoteRequest = {
        customerId: v.customerId!,
        notes: v.notes ?? null,
        items: [item],
      };

      await this.quotes.create(payload).toPromise();

      this.notify.success('Quote created');

      // Reset for the next quick entry
      this.resetForm();

      this.done.emit('created');
    } catch (e: any) {
      this.notify.httpError(e, 'Failed to create quote');
    } finally {
      this.submitting = false;
    }
  }

  cancel() {
    this.done.emit('cancelled');
  }
}
