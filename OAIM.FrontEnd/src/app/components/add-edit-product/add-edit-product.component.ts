import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { SupplierApiService } from '../../services/supplier-api.service';
import { CategoryApiService } from '../../services/category-api.service';
import { ICategory } from '../../models/ICategory';
import { ISupplier } from '../../models/ISupplier';
import { ProductApiService } from '../../services/product-api.service';
import { IProduct } from '../../models/IProduct';

@Component({
  selector: 'app-add-edit-product',
  imports: [ReactiveFormsModule],
  templateUrl: './add-edit-product.component.html',
  styleUrl: './add-edit-product.component.css'
})
export class AddEditProductComponent implements OnChanges {
  categories: any[] = [];
  suppliers: any[] = [];
  @Output() closed = new EventEmitter<void>();
 @Output() saved = new EventEmitter<void>();
  @Input() mode: 'add' | 'edit' = 'add';
  @Input() product: any | undefined;

form : FormGroup = new FormGroup({});
constructor(private supplierApi: SupplierApiService, private categoryApi: CategoryApiService, private productApi: ProductApiService) {
  this.form = new FormGroup({
    Id: new FormControl(0),
    Name: new FormControl('',[Validators.required, Validators.minLength(3)]),
    Price: new FormControl('',[Validators.required, Validators.min(0)]),
    Barcode: new FormControl('',[Validators.required]),
    StockQuantity: new FormControl('',[Validators.required, Validators.min(0)]),
    CategoryId: new FormControl('',[Validators.required, Validators.min(1)]),
    SupplierId: new FormControl('',[Validators.required, Validators.min(1)])  
  });
 }

  ngOnInit() {
    this.loadCategories();
    this.loadSuppliers();
  }
  ngOnChanges() {
       if (this.mode === 'edit' && this.product) {
      this.form.patchValue({
        Id: this.product.id,
        Name: this.product.name,
        Price: this.product.price,
        Barcode: this.product.barcode,
        StockQuantity: this.product.stockQuantity,
        CategoryId: Number(this.product.categoryId),
        SupplierId: Number(this.product.supplierId)
      });
      console.log('Form values after patching:', this.form.value);
    }else {
      this.form.reset();
  }
}

  save() {
     if (this.form.invalid)
        return;

    if (this.mode === 'add') {
      this.productApi.addProduct(this.form.value).subscribe({
          next: (data) => {
            console.log('Product added successfully:', data);
            this.saved.emit();
            
          },
          error: (error) => {
            console.error('Error adding product:', error);
          }
        });
    }
    else if (this.mode === 'edit') {
      this.productApi.updateProduct(this.form.value).subscribe({
        next: (data) => {
          console.log('Product updated successfully:', data);
          this.saved.emit();
        },
        error: (error) => {
          console.error('Error updating product:', error);
        }
      });}
  

}
loadCategories() {
  this.categoryApi.getListCategory().subscribe({
    next: (data) => {
      this.categories = [{ id: 0, name: 'Select category' }, ...data];
    },
    error: (error) => {
      console.error('Error fetching categories:', error);
    }
  });
}
loadSuppliers() {
  this.supplierApi.getListSupplier().subscribe({
    next: (data) => {
      this.suppliers = [{ id: 0, name: 'Select supplier' }, ...data];
    },
    error: (error) => {
      console.error('Error fetching suppliers:', error);
    }
  });
}
}
