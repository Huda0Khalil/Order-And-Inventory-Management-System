import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class OrderApiService {

  constructor(private http: HttpClient) { }
  header = new HttpHeaders({tenant:'Tenant3'});
  getAllOrders(pageNumber:number, pageSize: number):Observable<any> {
    const params = new HttpParams()
    .set('pageNumber', pageNumber)
    .set('pageSize', pageSize);
    return this.http.get<any>(`${environment.baseApiUrl}/order`,{params,headers: this.header});
  }
  AddOrder(order: any): Observable<any> {
    order.TenantId = localStorage.getItem("tenant") ?? '';
    return this.http.post<any>(`${environment.baseApiUrl}/order`, order,{headers:this.header});
  }
  getOrderById(id: number): Observable<any>{

    return this.http.get<any>(`${environment.baseApiUrl}/order/${id}`,{headers: this.header})
  }
}
