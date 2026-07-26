import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, Observable, of, tap } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  constructor(private http: HttpClient) {
    localStorage.setItem('tenant', 'Tenant3');
  }

  private currentUserSubject = new BehaviorSubject<any>(null);
  currentUser$ = this.currentUserSubject.asObservable();

  private isLoggedInSubject = new BehaviorSubject<boolean>(false);
  isLoggedIn$ = this.isLoggedInSubject.asObservable();

  header = new HttpHeaders({ tenant: 'Tenant3' });
  initialize() {
    return this.http
      .get<User>(`${environment.baseApiUrl}/auth/me`, { headers: this.header })
      .pipe(
        tap((user) => {
          this.currentUserSubject.next(user);
          this.isLoggedInSubject.next(true);
          localStorage.setItem('user', JSON.stringify(user));
        }),
        catchError(() => {
          this.logoutLocal();

          return of(null);
        }),
      );
  }
  logoutLocal() {
    localStorage.removeItem('user');

    this.currentUserSubject.next(null);
    this.isLoggedInSubject.next(false);
  }
  register(user: User): Observable<any> {
    user.tenantId = localStorage.getItem('tenant') ?? '';

    return this.http.post<User>(
      `${environment.baseApiUrl}/auth/register`,
      user,
      { headers: this.header },
    );
  }

  login(user: any): Observable<any> {
    user.tenantId = localStorage.getItem('tenant') ?? '';

    return this.http
      .post<any>(`${environment.baseApiUrl}/auth/login`, user, {
        headers: this.header,
        withCredentials: true,
      })
      .pipe(

    tap(response => {
      this.currentUserSubject.next(response.user);
      this.isLoggedInSubject.next(true);
      localStorage.setItem(
        'user',
        JSON.stringify(response.user)
      );

    })

  );
  }

  logout(): Observable<any> {
    return this.http
      .post<any>(
        `${environment.baseApiUrl}/auth/logout`,
        {},
        { withCredentials: true, headers: this.header },
      )
      .pipe(
        tap(() => {
          this.logoutLocal();
        }),
      );
  }
isLoggedIn(): boolean {
  return this.currentUserSubject.value !== null;
}
  getRole(): string | null {
    return localStorage.getItem('userRole');
  }
}
