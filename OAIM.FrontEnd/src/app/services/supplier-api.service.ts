import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { ISupplier } from '../models/ISupplier';

@Injectable({
  providedIn: 'root'
})
export class SupplierApiService {

  constructor(private http: HttpClient) { }
    header = new HttpHeaders({tenant:'Tenant3'})

  getAllSuppliers(pageNumber:number, pageSize: number) :Observable<any>{
    const params = new HttpParams()
    .set('pageNumber', pageNumber)
    .set('pageSize', pageSize);
    return this.http.get<any>(`${environment.baseApiUrl}/Supplier`,{params,headers:this.header});
  }
  getListSupplier():Observable<any>{

    return this.http.get<any>(`${environment.baseApiUrl}/supplier/getList`,{headers:this.header});
  }
  addSupplier(sup: any): Observable<any> {
      sup.TenantId = localStorage.getItem("tenant") ?? '';
      return this.http.post<any>(`${environment.baseApiUrl}/supplier`, sup,{headers:this.header});
    }
  updateSupplier(sup: any): Observable<any> {
       sup.TenantId = localStorage.getItem("tenant") ?? '';
       return this.http.put<ISupplier>(`${environment.baseApiUrl}/supplier/${sup.Id}`, sup,{headers:this.header});
    }
    deleteSupplier(id: number): Observable<any> {
      return this.http.delete<any>(`${environment.baseApiUrl}/supplier/${id}`, {headers:this.header});
    }
  
}
