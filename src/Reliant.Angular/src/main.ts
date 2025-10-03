import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { routes } from './app/app.routes';
import { AppComponent } from './app/app.component';
import {
  provideHttpClient,
  withFetch,
  withInterceptors,
} from '@angular/common/http';
import { authInterceptor } from './app/core/auth.interceptor';
import { provideAnimations } from '@angular/platform-browser/animations';
import { app } from '../server';
import { appConfig } from './app/app.config';
import { provideToastr } from 'ngx-toastr';

bootstrapApplication(AppComponent, {
  ...appConfig,
  providers: [
    ...appConfig.providers!,
    provideRouter(routes),
    provideHttpClient(withFetch(), withInterceptors([authInterceptor])),
    provideAnimations(),
    provideToastr({
      positionClass: 'toast-bottom-right',
      timeOut: 3500,
      closeButton: true,
      progressBar: true,
      preventDuplicates: true,
      tapToDismiss: true,
    }),
  ],
}).catch((err) => console.error(err));
