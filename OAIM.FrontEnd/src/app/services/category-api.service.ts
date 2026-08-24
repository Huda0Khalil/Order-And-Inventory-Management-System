import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ICategory } from '../models/ICategory';

@Injectable({
  providedIn: 'root',
})
export class CategoryApiService {
  constructor(private http: HttpClient) {}
  header = new HttpHeaders({ tenant: environment.tenant });

  getAllCategories(pageNumber: number, pageSize: number): Observable<any> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<any>(`${environment.apiUrl}/category`, {
      params,
      headers: this.header,
    });
  }
  getListCategory(): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/category/getList`, {
      headers: this.header,
    });
  }

  addCategory(category: ICategory): Observable<any> {
    category.TenantId = localStorage.getItem('tenant') ?? '';
    return this.http.post<any>(`${environment.apiUrl}/category`, category, {
      headers: this.header,
    });
  }
  updateCategory(category: ICategory): Observable<ICategory> {
    category.TenantId = localStorage.getItem('tenant') ?? '';
    return this.http.put<ICategory>(
      `${environment.apiUrl}/category/${category.Id}`,
      category,
      { headers: this.header },
    );
  }
  deleteCategory(id: number): Observable<any> {
    return this.http.delete<any>(`${environment.apiUrl}/category/${id}`, {
      headers: this.header,
    });
  }
}
