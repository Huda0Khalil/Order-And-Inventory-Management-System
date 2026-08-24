import { Component, signal } from '@angular/core';
import { CustomerApiService } from '../../../services/customer-api.service';
import Swal from 'sweetalert2';
import { TopBarComponent } from '../../top-bar/top-bar.component';
import { FormsModule } from '@angular/forms';
import { AddEditCustomerComponent } from "../add-edit-customer/add-edit-customer.component";
import { CommonModule } from '@angular/common';
import { Actions } from '../../../models/breadcrumb';

@Component({
  selector: 'app-all-customers',
  imports: [TopBarComponent, FormsModule, AddEditCustomerComponent,CommonModule],
  templateUrl: './all-customers.component.html',
  styleUrl: './all-customers.component.css'
})
export class AllCustomersComponent {
 customers: any[] = [];
  searchText: string = '';
  pageNumber = 1;
  pageSize=10;
  totalCount=0;
  showModal:boolean=false;
  modalMode: 'add'|'edit' = 'add';
  selectedCustomer : any;
  filteredCustomers:any;
  deleteTarget = signal<any | null>(null);
  protected readonly Math = Math;
  action :Actions[] = [
    {
      label: 'New Customer',
      icon: 'bi bi-plus',
      style: { 'background-color': 'blue', color: 'white' },
      func: 'openAddModel'
    }
  ];
  constructor(private customerApi: CustomerApiService){}
  ngOnInit(): void {
    this.loadCustomers();
  }
  loadCustomers(){
    this.customerApi.getAllCustomers(this.pageNumber,this.pageSize).subscribe({
      next:(data)=>{
        this.customers =data.items;
        this.filteredCustomers = [...data.items];
        this.totalCount = data.totalCount; 
      },
      error:(err)=>{
        console.log("error during load customers: ",err)
      }
    });
    this.showModal = false;

  }
openAddModel() {
  this.showModal = true;
  this.modalMode = 'add';
}
closeModel() {
  this.showModal = false;
  this.modalMode = 'add'; 
}
nextPage() {
if (this.pageNumber * this.pageSize < this.totalCount) {
    this.pageNumber++;
    this.loadCustomers();
  }
}
previousPage() {
  if (this.pageNumber > 1) {
      this.pageNumber--;
      this.loadCustomers();
    }
}
confirmDelete(cus: any) {
 this.deleteTarget.set(cus); 
  this.doDelete();
}
doDelete(){
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
          this.customerApi.deleteCustomer(target.id).subscribe({
          next: () => resolve(true),
          error:()=> reject(new Error('Delete failed'))
          });
         }) }})   
      .then((result) => {
        if (result.isConfirmed) {
          this.deleteTarget.set(null);
           this.loadCustomers();
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
openEdit(cus: any) {
  this.showModal = true;
  this.modalMode = 'edit';
  this.selectedCustomer = cus;
}

applyFilters() {
  console.log(this.customers);
    this.filteredCustomers = this.customers.filter(cus => {
      const matchesSearch =
        this.searchText === '' ||
        cus.id.toString().includes(this.searchText)||
        cus.firstName.toLowerCase().includes(this.searchText.toLowerCase())||
        cus.lastName.toLowerCase().includes(this.searchText.toLowerCase()) ||
        cus.address.toLowerCase().includes(this.searchText.toLowerCase()) ||
        cus.email.toLowerCase().includes(this.searchText.toLowerCase()) ||
        cus.phoneNumber.toLowerCase().includes(this.searchText.toLowerCase());
      return matchesSearch;
  });
}


}
