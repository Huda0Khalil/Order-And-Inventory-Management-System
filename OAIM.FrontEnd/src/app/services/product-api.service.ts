import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { IProduct } from '../models/IProduct';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ProductApiService {
  constructor(private http: HttpClient) {}
  header = new HttpHeaders({ tenant: environment.tenant });
  getAllProducts(
    pageNumber: number,
    pageSize: number,
    categoryId: number,
  ): Observable<any> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize)
      .set('categoryId', categoryId);
    return this.http.get<any>(`${environment.apiUrl}/product`, {
      params,
      headers: this.header,
    });
  }
  getProductById(id: number) {
    return this.http.get<any>(`${environment.apiUrl}/product/${id}`, {
      headers: this.header,
    });
  }

  addProduct(product: IProduct): Observable<IProduct> {
    product.TenantId = environment.tenant;
    return this.http.post<IProduct>(`${environment.apiUrl}/product`, product, {
      headers: this.header,
    });
  }
  updateProduct(product: IProduct): Observable<any> {
    product.TenantId = environment.tenant;
    return this.http.put<IProduct>(
      `${environment.apiUrl}/product/${product.Id}`,
      product,
      { headers: this.header },
    );
  }
  deleteProduct(id: number): Observable<any> {
    return this.http.delete<any>(`${environment.apiUrl}/product/${id}`, {
      headers: this.header,
    });
  }
}
