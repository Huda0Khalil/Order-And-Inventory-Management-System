import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class CustomerApiService {

  constructor(private http: HttpClient) {
    localStorage.setItem('tenant', 'Tenant3');
   }
  header = new HttpHeaders({tenant:'Tenant3'});
  getAllCustomers(pageNumber:number, pageSize: number):Observable<any> {
    const params = new HttpParams()
    .set('pageNumber', pageNumber)
    .set('pageSize', pageSize);
    return this.http.get<any>(`${environment.baseApiUrl}/Customer`,{params,headers: this.header});
  }
  getCustomerList():Observable<any> {
    return this.http.get<any>(`${environment.baseApiUrl}/Customer/getList`,{headers: this.header});
  }
  addCustomer(cus: any): Observable<any> {
    cus.TenantId = localStorage.getItem("tenant") ?? '';
    return this.http.post<any>(`${environment.baseApiUrl}/Customer`, cus,{headers:this.header});
  }
  updateCustomer(cus: any): Observable<any> {
     cus.TenantId = localStorage.getItem("tenant") ?? '';
     return this.http.put<any>(`${environment.baseApiUrl}/Customer/${cus.Id}`, cus,{headers:this.header});
  }
  deleteCustomer(id: number): Observable<string> {
    return this.http.delete(`${environment.baseApiUrl}/Customer/${id}`, {
      headers: this.header,
      responseType: 'text' as 'text'
    });
  }
}
