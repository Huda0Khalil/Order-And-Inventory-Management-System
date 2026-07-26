import { Component, OnInit } from '@angular/core';
import { TopBarComponent } from '../top-bar/top-bar.component';
import { OrderApiService } from '../../services/order-api.service';
import { CommonModule } from '@angular/common';
import { customerType } from '../../models/customerType';
import { CustomerApiService } from '../../services/customer-api.service';
import { ProductApiService } from '../../services/product-api.service';
import { DashboardService } from '../../services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  imports: [TopBarComponent, CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
 constructor(private _dashboardApi: DashboardService ){}
 recentOrders:any;
 totalOrdersToday!:number;
 revenueToday!: number;
 activeCustomers!: number;
 totalProducts!: number;
 lowStockProducts:any;
 protected readonly customerType = customerType;
 ngOnInit(){
    this._dashboardApi.getData().subscribe({
      next: (data)=>{
        console.log(data);
        this.recentOrders = data.recentOrders;
        this.totalProducts = data.totalProducts;
        this.totalOrdersToday = data.totalOrdersToday;
        this.revenueToday = data.revenueToday;
        this.activeCustomers = data.activeCustomers;
        this.lowStockProducts = data.lowStockProducts;
        // this.orders = data.items;
        // this.totalOrders = data.totalCount

      }
    });
    
    
 }
}
/*
{
    "totalOrdersToday": 0,
    "revenueToday": 0,
    "totalProducts": 10,
    "activeCustomers": 1,
    "recentOrders": [
        {
            "id": 6002,
            "orderNumber": "6002",
            "customerName": null,
            "type": "Retail",
            "amount": 28,
            "status": "Pending",
            "orderDate": "2026-07-12T11:42:21.7569972"
        },
        {
            "id": 5005,
            "orderNumber": "5005",
            "customerName": null,
            "type": "Wholesale",
            "amount": 100,
            "status": "Pending",
            "orderDate": "2026-07-09T12:46:06.7672919"
        },
        {
            "id": 5004,
            "orderNumber": "5004",
            "customerName": null,
            "type": "Wholesale",
            "amount": 10,
            "status": "Pending",
            "orderDate": "2026-07-09T12:43:30.957373"
        },
        {
            "id": 5003,
            "orderNumber": "5003",
            "customerName": null,
            "type": "Retail",
            "amount": 18,
            "status": "Pending",
            "orderDate": "2026-07-09T12:42:55.3979986"
        },
        {
            "id": 5002,
            "orderNumber": "5002",
            "customerName": null,
            "type": "Wholesale",
            "amount": 10,
            "status": "Pending",
            "orderDate": "2026-07-09T12:41:33.9448061"
        },
        {
            "id": 4002,
            "orderNumber": "4002",
            "customerName": null,
            "type": "Wholesale",
            "amount": 18,
            "status": "Pending",
            "orderDate": "2026-07-08T12:57:54.9087334"
        },
        {
            "id": 3005,
            "orderNumber": "3005",
            "customerName": null,
            "type": "Retail",
            "amount": 10,
            "status": "Pending",
            "orderDate": "2026-07-05T12:36:39.1071837"
        },
        {
            "id": 3004,
            "orderNumber": "3004",
            "customerName": null,
            "type": "Retail",
            "amount": 10,
            "status": "Pending",
            "orderDate": "2026-07-05T12:32:51.8084816"
        },
        {
            "id": 3003,
            "orderNumber": "3003",
            "customerName": null,
            "type": "Retail",
            "amount": 10,
            "status": "Pending",
            "orderDate": "2026-07-05T12:06:45.3318503"
        },
        {
            "id": 3002,
            "orderNumber": "3002",
            "customerName": null,
            "type": "Retail",
            "amount": 10,
            "status": "Pending",
            "orderDate": "2026-07-05T12:04:50.4969226"
        }
    ],
    "lowStockProducts": [
        {
            "id": 2,
            "name": "Test Product2",
            "sku": "2000442",
            "stock": 0,
            "minimumStock": 10
        },
        {
            "id": 1013,
            "name": "Test Product4",
            "sku": "2000444",
            "stock": 0,
            "minimumStock": 10
        },
        {
            "id": 3004,
            "name": "Test Product10",
            "sku": "2000450",
            "stock": 1,
            "minimumStock": 10
        },
        {
            "id": 3002,
            "name": "Test Product8",
            "sku": "2000448",
            "stock": 3,
            "minimumStock": 10
        },
        {
            "id": 2004,
            "name": "Test Product 7",
            "sku": "2000447",
            "stock": 5,
            "minimumStock": 10
        }
    ]
} */