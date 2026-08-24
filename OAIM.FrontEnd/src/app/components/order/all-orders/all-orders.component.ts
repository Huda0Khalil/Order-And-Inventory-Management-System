import { Component, OnInit } from '@angular/core';
import { TopBarComponent } from "../../top-bar/top-bar.component";
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrderApiService } from '../../../services/order-api.service';
import { customerType } from '../../../models/customerType';
import { Route, Router, RouterLink } from '@angular/router';
import { Actions } from '../../../models/breadcrumb';

@Component({
  selector: 'app-all-orders',
  imports: [TopBarComponent, CommonModule, FormsModule],
  templateUrl: './all-orders.component.html',
  styleUrl: './all-orders.component.css'
})
export class AllOrdersComponent implements OnInit {
  orders: any[] = [];
  searchText: string = '';
  pageNumber = 1;
  pageSize=10;
  totalCount=0;
  filteredOrders:any;
  showModal:boolean=false;
  modalMode: 'add'|'edit' = 'add';
  protected readonly Math = Math;
 protected readonly customerType = customerType;
 action :Actions[] = [
    {
      label: 'New Order',
      icon: 'bi bi-plus',
      style: { 'background-color': 'blue', color: 'white' },
      func: 'openAddPage'
    }
  ];
  constructor(private orderApi: OrderApiService, private _router:Router){}
  ngOnInit(): void {
    this.loadOrders();  
  }
  loadOrders(){
    this.orderApi.getAllOrders(this.pageNumber,this.pageSize).subscribe({
      next:(data)=>{
        console.log("orders data: ",data);
        this.orders =data.items;
        this.filteredOrders = [...data.items];
        this.totalCount = data.totalCount; 
      },
      error:(err)=>{
        console.log("error during load orders: ",err)
      }
    });
  }
seeDetails(order: any) {
  this._router.navigate(['Cart', order.id]);
}

nextPage() {
if (this.pageNumber * this.pageSize < this.totalCount) {
    this.pageNumber++;
    this.loadOrders();
  }
}
previousPage() {
  if (this.pageNumber > 1) {
      this.pageNumber--;
      this.loadOrders();
    }
}
openAddPage() {
  this._router.navigate(['/Orders/AddOrder']);
}
applyFilters() {
  this.filteredOrders =  this.orders.filter(order => {

      const matchesSearch =
        this.searchText === '' ||
        order.id.toString().includes(this.searchText) ||
        order.createdBy.userName.toLowerCase().includes(this.searchText.toLowerCase()) ||
        customerType[order.customerType].toLowerCase().includes(this.searchText.toLowerCase()) ||
        order.customer?.firstName?.toLowerCase().includes(this.searchText.toLowerCase());
        console.log(matchesSearch)
      return matchesSearch;
})}
  
}
