import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

export const credentialsInterceptor: HttpInterceptorFn = (req, next) => {
  const _authService = inject(AuthService);
  const _router = inject(Router);
  const request = req.clone({
    withCredentials: true,
  });

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        _authService.logoutLocal();

        _router.navigate(['/Login']);
      }

      return throwError(() => error);
    }),
  );
};
