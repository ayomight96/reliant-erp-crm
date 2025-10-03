import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { managerGuard } from './core/manager.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/layout/layout.component').then((m) => m.LayoutComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'customers' },
      {
        path: 'customers',
        loadComponent: () =>
          import('./pages/customers/customers.component').then(
            (m) => m.CustomersComponent
          ),
      },

      {
        path: 'customers/:id/quotes',
        loadComponent: () =>
          import('./pages/quotes/quotes.component').then(
            (m) => m.QuotesComponent
          ),
      },
      {
        path: 'products',
        loadComponent: () =>
          import('./pages/products/products.component').then(
            (m) => m.ProductsComponent
          ),
      },
      {
        path: 'quotes',
        loadComponent: () =>
          import('./pages/quotes/quotes.component').then(
            (m) => m.QuotesComponent
          ),
      },

      {
        path: 'quotes/new',
        loadComponent: () =>
          import('./pages/create-quote/create-quote.component').then(
            (m) => m.CreateQuoteComponent
          ),
      },
      {
        path: 'admin',
        canActivate: [managerGuard],
        loadComponent: () =>
          import('./pages/admin/admin.component').then((m) => m.AdminComponent),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
