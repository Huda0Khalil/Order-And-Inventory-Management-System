import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {

  constructor(private http: HttpClient) {
    localStorage.setItem('tenant', 'Tenant3');
   }
  header = new HttpHeaders({tenant:'Tenant3'});
  getData():Observable<any>{
    return this.http.get<any>(`${environment.baseApiUrl}/dashboard`,{headers: this.header});
  }
}
