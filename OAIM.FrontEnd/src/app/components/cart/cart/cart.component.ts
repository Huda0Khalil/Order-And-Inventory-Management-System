import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';

import { OrderApiService } from '../../../services/order-api.service';
import { ProductApiService } from '../../../services/product-api.service';
import { customerType } from '../../../models/customerType';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    CommonModule
  ],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css',
})
export class CartComponent implements OnInit {

  customers: any[] = [];
  order: any;
  orderId!: number;

  CustomerType = customerType;

  customerTypes = [
    { 
      label: 'Retail', 
      value: customerType.Retail 
    },
    { 
      label: 'Wholesale', 
      value: customerType.Wholesale 
    }
  ];


  orderForm = new FormGroup({

    customerType: new FormControl(customerType.Retail),

    customerId: new FormControl<number | null>(null),

    items: new FormArray([])

  });


  productCache: Record<number, any> = {};


  constructor(
    private _orderApi: OrderApiService,
    private _activatedRoute: ActivatedRoute,
    private _productApi: ProductApiService,
    private cd: ChangeDetectorRef
  ) {}


  ngOnInit(): void {

    this._activatedRoute.paramMap.subscribe(param => {

      this.orderId = Number(param.get('orderId'));

      this.loadOrder();

    });

  }



  



  loadOrder(): void {

    this._orderApi.getOrderById(this.orderId)
      .subscribe({

        next: (data) => {

          this.order = data;
         
          console.log("OrderData: ",data)   
          this.order.items.forEach((i: any) => {
            this.loadProduct(i.productId);
          });

        }

      });

  }



  loadProduct(productId:number):void{


    // prevent duplicate API calls
    if(this.productCache[productId]){
      return;
    }


    this._productApi.getProductById(productId)
      .subscribe({

        next:(product)=>{

          this.productCache[productId] = product;


          // refresh UI
          this.cd.detectChanges();

        }

      });


  }



  getProduct(productId:number){

    return this.productCache[productId];

  }




  // get itemCount():number{

  //   return this.items.controls.length;

  // }




  get subtotal():number{

    return this.order?.items.reduce(
      (total:number,item:any)=>{
       return total + (item.unitPrice*item.quantity)
        const product = this.getProduct(item.productId);


        


      },0
    );


  }



  get total():number{

    return this.subtotal;

  }



}