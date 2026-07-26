import { Component } from '@angular/core';
import {
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { customerType } from '../../../models/customerType';
import { ProductApiService } from '../../../services/product-api.service';
import { CategoryApiService } from '../../../services/category-api.service';
import { CustomerApiService } from '../../../services/customer-api.service';
import { OrderApiService } from '../../../services/order-api.service';
import { ActivatedRoute, Router } from '@angular/router';
import { TopBarComponent } from '../../top-bar/top-bar.component';
import { InfiniteScrollDirective } from 'ngx-infinite-scroll';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-edit-order',
  imports: [
    TopBarComponent,
    InfiniteScrollDirective,
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './edit-order.component.html',
  styleUrl: './edit-order.component.css',
})
export class EditOrderComponent {
  pageNumber = 1;
  pageSize = 6;
  loading = false;
  hasMoreProducts = true;
  filteredProducts: any[] = [];
  products: any[] = [];
  categories: any[] = [];
  orderId: number = 0;
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
  productCache: { [id: number]: any } = {};
  constructor(
    private productApi: ProductApiService,
    private categoryApi: CategoryApiService,
    private customerApi: CustomerApiService,
    private orderApi: OrderApiService,
    private _router: Router,
    private _activetedRoute: ActivatedRoute,
  ) {
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
    this._activetedRoute.paramMap.subscribe((paramMap) => {
      this.orderId = Number(paramMap.get('orderId'));
    });
    this.loadOrder();

    this.orderForm.get('customerType')?.valueChanges.subscribe((type) => {
      this.updateCustomerValidation(type);
    });

    this.updateCustomerValidation(this.orderForm.get('customerType')?.value);
  }
  loadOrder() {
    console.log(this.orderId);
    this.orderApi.getOrderById(this.orderId).subscribe({
      next: (data) => {
        this.order = data;
        console.log('data: ', data);
        console.log('items: ', data.items);
        this.orderForm.patchValue({
          customerType: data.customerType,
          customerId: data.customerId,
        });
        this.items.clear();

        // Add items to FormArray
        data.items.forEach((item: { productId: any; quantity: any }) => {
          if (!this.productCache[item.productId]) {
            this.productApi
              .getProductById(item.productId)
              .subscribe((product) => {
                this.productCache[product.id] = product;
              });
          }
          this.items.push(
            new FormGroup({
              productId: new FormControl(item.productId),
              quantity: new FormControl(item.quantity, [
                Validators.required,
                Validators.min(1),
              ]),
            }),
          );
        });
        console.log(data);
      },
    });
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
          data.items.forEach((prod: { id: any }) => {
            this.productCache[prod.id] = prod;
          });
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
      return this.productCache[productId];
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
