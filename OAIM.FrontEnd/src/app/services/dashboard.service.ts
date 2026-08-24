import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  constructor(private http: HttpClient) {}
  header = new HttpHeaders({ tenant: environment.tenant });
  getData(): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/dashboard`, {
      headers: this.header,
    });
  }
}
