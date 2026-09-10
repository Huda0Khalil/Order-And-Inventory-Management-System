import { ApplicationConfig, inject, provideAppInitializer, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { credentialsInterceptor } from './interceptors/credentials-interceptor.interceptor';
import { AuthService } from './services/auth.service';
import { firstValueFrom } from 'rxjs';

export const appConfig: ApplicationConfig = {
   providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideClientHydration(withEventReplay()),
    provideHttpClient(withFetch(), withInterceptors([credentialsInterceptor])),
    provideAppInitializer(() => {
      const authService = inject(AuthService);
      return firstValueFrom(authService.initialize());
    }),
  ],
    
};
