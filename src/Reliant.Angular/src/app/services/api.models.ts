export interface LoginResponse {
  accessToken: string;
  expires: string;
  roles: string[];
}

export interface Customer {
  id: number;
  name: string;
  email?: string;
  phone?: string;
  addressLine1?: string;
  city?: string;
  postcode?: string;
  segment?: string;
}

export interface Product {
  id: number;
  name: string;
  productType: string;
  basePrice: number;
}

export interface QuoteItemResponse {
  id: number;
  productId: number;
  productName: string;
  widthMm: number;
  heightMm: number;
  material: string;
  glazing: string;
  colorTier?: string | null;
  hardwareTier?: string | null;
  installComplexity?: string | null;
  qty: number;
  unitPrice: number;
  lineTotal: number;
}

export interface UserListItem {
  id: number;
  email: string;
  fullName: string;
  roles: string[];
}

export interface QuoteResponse {
  id: number;
  customerId: number;
  customerName: string;
  status: string; // 'Draft' | 'Sent' | 'Accepted' | 'Rejected' | etc
  subtotal: number;
  vat: number;
  total: number;
  createdAt: string; // ISO
  createdByUserId: number;
  notes?: string | null;
  items: QuoteItemResponse[];
}

/* ===== Create payloads ===== */

export interface QuoteItemCreateRequest {
  productId: number;
  widthMm: number;
  heightMm: number;
  material: string;
  glazing: string;
  colorTier?: string | null;
  hardwareTier?: string | null;
  installComplexity?: string | null;
  qty: number;
  unitPrice?: number | null; // omit to let AI/back-end fill
}

export interface CreateQuoteRequest {
  customerId: number;
  notes?: string | null;
  items: QuoteItemCreateRequest[];
}

/* Summary uses same shape as create */
export type QuoteSummaryRequest = CreateQuoteRequest;
