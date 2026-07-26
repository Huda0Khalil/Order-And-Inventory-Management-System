import { Component, OnChanges, OnInit } from '@angular/core';
import { TopBarComponent } from '../../top-bar/top-bar.component';
import { ProductApiService } from '../../../services/product-api.service';
import { CommonModule } from '@angular/common';
import { InfiniteScrollDirective } from 'ngx-infinite-scroll';
import {
  FormArray,
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { CustomerApiService } from '../../../services/customer-api.service';
import { OrderApiService } from '../../../services/order-api.service';
import { customerType } from '../../../models/customerType';
import { Router } from '@angular/router';
import { CategoryApiService } from '../../../services/category-api.service';

@Component({
  selector: 'app-add-order',
  imports: [
    TopBarComponent,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    InfiniteScrollDirective,
  ],
  templateUrl: './add-order.component.html',
  styleUrl: './add-order.component.css',
})
export class AddOrderComponent implements OnInit {
  pageNumber = 1;
  pageSize = 6;
  loading = false;
  hasMoreProducts = true;
  filteredProducts: any[] = [];
  products: any[] = [];
  categories: any[] = [];
  catId: number = 0;
  order: any;
  data: any;
  orderForm: FormGroup = new FormGroup({});
  minQuantity: number = 1;
  customers: any[] = [];
  CustomerType = customerType;
  customerTypes = [
    { label: 'Retail', value: this.CustomerType.Retail },
    { label: 'Wholesale', value: this.CustomerType.Wholesale },
  ];
  constructor(
    private productApi: ProductApiService,
    private categoryApi: CategoryApiService,
    private customerApi: CustomerApiService,
    private orderApi: OrderApiService,
    private _router: Router,
  ) {
    this.data = {
      items: [
        {
          id: 1,
          name: 'Test Product1',
          price: 10,
          barcode: '2000441',
          stockQuantity: 18,
          category: {
            id: 1,
            name: 'Test category',
            tenantId: 'Tenant3',
          },
          categoryId: 1,
          supplier: {
            id: 1,
            name: 'Test Supplier0',
            contactEmail: 'testSup0@gmal.com',
            phoneNumber: '0796541238',
            address: 'Zarqa',
            tenantId: 'Tenant3',
          },
          supplierId: 1,
          rowVersion: 'AAAAAAAARlw=',
          tenantId: 'Tenant3',
        },
        {
          id: 2,
          name: 'Test Product2',
          price: 10,
          barcode: '2000442',
          stockQuantity: 1,
          category: {
            id: 1,
            name: 'Test category',
            tenantId: 'Tenant3',
          },
          categoryId: 1,
          supplier: {
            id: 1,
            name: 'Test Supplier0',
            contactEmail: 'testSup0@gmal.com',
            phoneNumber: '0796541238',
            address: 'Zarqa',
            tenantId: 'Tenant3',
          },
          supplierId: 1,
          rowVersion: 'AAAAAAAApBg=',
          tenantId: 'Tenant3',
        },
        {
          id: 1002,
          name: 'Test Product3',
          price: 10,
          barcode: '2000443',
          stockQuantity: 39,
          category: {
            id: 2,
            name: 'Test Category 2',
            tenantId: 'Tenant3',
          },
          categoryId: 2,
          supplier: {
            id: 1,
            name: 'Test Supplier0',
            contactEmail: 'testSup0@gmal.com',
            phoneNumber: '0796541238',
            address: 'Zarqa',
            tenantId: 'Tenant3',
          },
          supplierId: 1,
          rowVersion: 'AAAAAAAApB0=',
          tenantId: 'Tenant3',
        },
        {
          id: 1013,
          name: 'Test Product4',
          price: 5,
          barcode: '2000444',
          stockQuantity: 0,
          category: {
            id: 3,
            name: 'Test category3',
            tenantId: 'Tenant3',
          },
          categoryId: 3,
          supplier: {
            id: 1,
            name: 'Test Supplier0',
            contactEmail: 'testSup0@gmal.com',
            phoneNumber: '0796541238',
            address: 'Zarqa',
            tenantId: 'Tenant3',
          },
          supplierId: 1,
          rowVersion: 'AAAAAAAApCw=',
          tenantId: 'Tenant3',
        },
      ],
      totalCount: 4,
      pageNumber: 1,
      pageSize: 10,
    };
    this.orderForm = new FormGroup({
      // quantity: new FormControl(1, [Validators.required, Validators.min(1)])
      customerType: new FormControl(customerType.Retail, [Validators.required]),
      customerId: new FormControl(''),
      items: new FormArray([], Validators.required),
    });
  }
  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts(true);
    this.loadCustomers();
    this.orderForm.get('customerType')?.valueChanges.subscribe((type) => {
      this.updateCustomerValidation(type);
    });

    this.updateCustomerValidation(this.orderForm.get('customerType')?.value);
  }

  loadProducts(reset = false) {
    if (this.loading || !this.hasMoreProducts) {
      return;
    }

    if (reset) {
      this.pageNumber = 1;
      this.products = [];
      this.filteredProducts = [];
      this.hasMoreProducts = true;
    }

    this.loading = true;

    this.productApi
      .getAllProducts(
        this.pageNumber,
        this.pageSize,
        this.catId > 0 ? this.catId : 0,
      )
      .subscribe({
        next: (data) => {
          this.products.push(...data.items);

          this.filteredProducts = [...this.products];

          this.loading = false;

          if (data.items.length < this.pageSize) {
            this.hasMoreProducts = false;
          } else {
            this.pageNumber++;
          }
        },

        error: (err) => {
          console.log(err);
          this.loading = false;
        },
      });
  }
  onCategoryChanged(categoryId: number) {
    this.catId = categoryId;
    this.hasMoreProducts = true;
    this.loadProducts(true);
  }
  loadCustomers() {
    this.customerApi.getCustomerList().subscribe({
      next: (data) => {
        console.log('customers data: ', data);
        this.customers = data;
      },
      error: (err) => {
        console.log('error during load customers: ', err);
      },
    });
  }
  loadCategories() {
    this.categoryApi.getListCategory().subscribe({
      next: (data) => {
        console.log('categories data: ', data);
        this.categories = [{ id: 0, name: 'Select Category' }, ...data];
      },
      error: (err) => {
        console.log('error during load categories: ', err);
      },
    });
  }
  get items(): FormArray {
    return this.orderForm.get('items') as FormArray;
  }
  getProduct(productId: number) {
    return this.products.find((p) => p.id === productId);
  }
  updateCustomerValidation(type: customerType) {
    const customerControl = this.orderForm.get('customerId');

    if (!customerControl) return;

    if (type === customerType.Wholesale) {
      customerControl.setValidators([Validators.required]);
    } else {
      customerControl.clearValidators();
      customerControl.setValue(null); // optional if you want to clear it
    }

    customerControl.updateValueAndValidity();
  }
  addToOrder(product: any) {
    if (product.stockQuantity <= 0) {
      return;
    }
    const orderItems = this.items;
    const existingIndex = orderItems.controls.findIndex(
      (control) => control.get('productId')?.value === product.id,
    );
    if (existingIndex > -1) {
      const quantity = orderItems
        .at(existingIndex)
        .get('quantity') as FormControl<number>;
      quantity.setValue(quantity.value + 1);
    } else {
      this.items.push(
        new FormGroup({
          productId: new FormControl(product.id, Validators.required),
          quantity: new FormControl(1, [
            Validators.required,
            Validators.min(1),
          ]),
        }),
      );
    }
    product.stockQuantity--;
  }
  increaseQuantity(index: number) {
    const item = this.items.at(index);
    const productId = item.get('productId')?.value;
    const product = this.products.find((p) => p.id === productId);
    if (!product || product.stockQuantity <= 0) {
      return;
    }
    const quantity = item.get('quantity') as FormControl<number>;
    quantity.setValue(quantity.value + 1);
    product.stockQuantity--;
  }

  decreaseQuantity(index: number) {
    const item = this.items.at(index);

    const productId = item.get('productId')?.value;

    const product = this.products.find((p) => p.id === productId);

    const quantity = item.get('quantity') as FormControl<number>;

    quantity.setValue(quantity.value - 1);

    if (product) {
      product.stockQuantity++;
    }

    if (quantity.value <= 0) {
      this.items.removeAt(index);
    }
  }
  getTotal(): number {
    return this.items.controls.reduce((total, control) => {
      const productId = control.get('productId')?.value;
      const quantity = control.get('quantity')?.value;
      const product = this.products.find((p) => p.id === productId);
      return total + (product ? product.price * quantity : 0);
    }, 0);
  }
  selectCustomerType(type: customerType) {
    this.orderForm.patchValue({
      customerType: type,
    });
  }
  completeOrder() {
    console.log('form', this.orderForm.value);
    if (this.orderForm.invalid || this.items.length === 0) {
      console.log('Order form is invalid or empty.');
      return;
    }
    this.orderApi.AddOrder(this.orderForm.value).subscribe({
      next: (data) => {
        console.log('Order completed:', data);
        this._router.navigate(['/Orders']);
      },
      error: (err) => {
        console.log('error during complete order: ', err);
      },
    });
  }
  clearOrder() {
    this.items.controls.forEach((control) => {
      const productId = control.get('productId')?.value;
      const quantity = control.get('quantity')?.value;
      this.products.find((p) => p.id === productId).stockQuantity += quantity;
    });
    this.orderForm.reset({});
    this.items.clear();
  }
  onScroll() {
    this.loadProducts();
  }
}
