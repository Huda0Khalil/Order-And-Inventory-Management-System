import { Component, OnInit, signal} from '@angular/core';
import { TopBarComponent } from "../top-bar/top-bar.component";
import { IProduct } from '../../models/IProduct';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ISupplier } from '../../models/ISupplier';
import { ProductApiService } from '../../services/product-api.service';
import { CategoryApiService } from '../../services/category-api.service';
import { SupplierApiService } from '../../services/supplier-api.service';
import { AddEditProductComponent } from "../add-edit-product/add-edit-product.component";
import Swal from 'sweetalert2'
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-all-products',
  standalone: true,
  imports: [CommonModule, TopBarComponent, FormsModule, AddEditProductComponent],
  templateUrl: './all-products.component.html',
  styleUrls: ['./all-products.component.css']
})
export class AllProductsComponent implements OnInit {
pageNumber = 1;
pageSize=10;
totalCount=0;

searchText: string = '';
selectedCategory: any;

products: any[] = [] ;
filteredProducts: any[] = [];
categories: any[] = [];
suppliers: ISupplier[] = [];
//modal
showModal = false;
modalMode: 'add' | 'edit' = 'add';
selectedProduct?: IProduct;
catId !: number;
//delete confirmation
  deleteTarget = signal<any | null>(null);
protected readonly Math = Math;
  constructor(private productApi: ProductApiService, private supplierApi: SupplierApiService, private categoryApi: CategoryApiService,
    private _activetedRoute: ActivatedRoute
  ) {}
  
  ngOnInit() {
    this.categoryApi.getListCategory().subscribe({
      next: (data) => {
        this.categories = [{ id: 0, name: 'Select category' } , ...data];
        this.totalCount = data.totalCount;
        this.selectedCategory = this.categories[0].id;
        console.log("this.categories",this.categories)
      },
      error: (error) => {
        console.error('Error fetching categories:', error);
      }
    });
    this.supplierApi.getListSupplier().subscribe({
      next: (data) => {
        this.suppliers = data;},
      error: (error) => {
        console.error('Error fetching suppliers:', error);
      }
    });
    this._activetedRoute.paramMap.subscribe((paramMap)=>{
    this.catId = Number(paramMap.get('catId'));
    });
    this.LoadProducts();  
  }
 
  stockBadge(qty: number): string {
    if (qty <= 0)  return 'badge bg-danger text-white';
    if (qty <= 10) return 'badge bg-warning text-white';
    return 'badge bg-success text-white';
  }
  
  stockLabel(qty: number): string {
    if (qty <= 0)  return 'Out of Stock';
    if (qty <= 10) return 'Low Stock';
    return 'In Stock';
  }
  applyFilters(){
    this.selectedCategory = 0;
    this.filteredProducts = this.products.filter(product => {

      const matchesSearch =
        this.searchText === '' ||
        product.name.toLowerCase().includes(this.searchText.toLowerCase()) ||
        product.category?.name.toLowerCase().includes(this.searchText.toLowerCase()) ||
        product.supplier?.name.toLowerCase().includes(this.searchText.toLowerCase()) ||
        product.barcode.toLowerCase().includes(this.searchText.toLowerCase());
      return matchesSearch;
  });
}
filteredByCategory() {
    if (this.selectedCategory == 0) {
    this.filteredProducts = [...this.products];
  } else {
    this.productApi.getAllProducts(this.pageNumber, this.pageSize, this.selectedCategory).subscribe({
      next: (data) => {
        this.filteredProducts = data.items;
        this.totalCount = data.totalCount;
      },
      error: (error) => {
        console.error('Error fetching products by category:', error);
      }
    });
  }
   this.searchText = '';
}
//modal
openAddModel() {
  this.showModal = true;
  this.modalMode = 'add';
}
closeModel() {
  this.showModal = false;
  this.modalMode = 'add'; 
}
openEdit(product: IProduct) {
    this.modalMode = 'edit';

    this.selectedProduct = product;

    this.showModal = true;
}
confirmDelete(product: IProduct) {
  this.deleteTarget.set(product);
  this.doDelete();

}
doDelete() { 
  const target = this.deleteTarget(); 
  if(!target) return;
  Swal.fire({
    title: 'Are you sure?',
    text: 'You will not be able to recover this product!',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Yes, delete it!',
    cancelButtonText: 'No, keep it',
    preConfirm: () => {
      return new Promise((resolve, reject) =>{
        this.productApi.deleteProduct(target.id).subscribe({
        next: () => resolve(true),
        error:()=> reject(new Error('Delete failed'))
        });
       }) }})   
    .then((result) => {
      if (result.isConfirmed) {
        this.deleteTarget.set(null);
         this.LoadProducts();
         Swal.fire({
        title: 'Deleted!',
        text: 'Your product has been deleted.',
        icon: 'success'
      });
        }
      }).catch(() => {
    Swal.fire({
      title: 'Error',
      text: 'Failed to delete the product.',
      icon: 'error'
    });
  });
}
 
LoadProducts() {
  this.productApi.getAllProducts(this.pageNumber,this.pageSize,this.catId).subscribe({
      next: (data) => {
        this.products = data.items;
        this.totalCount = data.totalCount;
        if(this.catId == 0){
        this.filteredProducts = [...this.products];
        }
        else{
          this.filteredProducts = this.products.filter((p)=>p.categoryId == this.catId);
          this.selectedCategory = this.catId
        }
        console.log('Fetched products:', this.products);
      },
      error: (error) => {
        console.error('Error fetching products:', error);
      }
    });
    this.showModal = false;
}
nextPage() {
if (this.pageNumber * this.pageSize < this.totalCount) {
    this.pageNumber++;
    this.LoadProducts();
  }
}
previousPage() {
  if (this.pageNumber > 1) {
      this.pageNumber--;
      this.LoadProducts();
    }
}
}