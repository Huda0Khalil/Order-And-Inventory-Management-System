import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { IProduct } from '../models/IProduct';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProductApiService {

  constructor(private http: HttpClient) {
    localStorage.setItem('tenant', 'Tenant3');
   }
  header = new HttpHeaders({tenant:'Tenant3'});
  getAllProducts(pageNumber:number, pageSize: number, categoryId: number):Observable<any> {
    const params = new HttpParams()
    .set('pageNumber', pageNumber)
    .set('pageSize', pageSize)
    .set('categoryId', categoryId);
    return this.http.get<any>(`${environment.baseApiUrl}/product`,{params,headers: this.header});
  }
  getProductById(id:number){
    return this.http.get<any>(`${environment.baseApiUrl}/product/${id}`,{headers:this.header});
  }
  
  addProduct(product: IProduct): Observable<IProduct> {
    product.TenantId = localStorage.getItem("tenant") ?? '';
    return this.http.post<IProduct>(`${environment.baseApiUrl}/product`, product,{headers:this.header});
  }
  updateProduct(product: IProduct): Observable<any> {
     product.TenantId = localStorage.getItem("tenant") ?? '';
     return this.http.put<IProduct>(`${environment.baseApiUrl}/product/${product.Id}`, product,{headers:this.header});
  }
  deleteProduct(id: number): Observable<any> {
    return this.http.delete<any>(`${environment.baseApiUrl}/product/${id}`, {headers:this.header});
  }

}