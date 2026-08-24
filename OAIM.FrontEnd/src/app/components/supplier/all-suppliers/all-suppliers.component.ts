import { Component, OnInit, signal } from '@angular/core';
import { TopBarComponent } from "../../top-bar/top-bar.component";
import { FormsModule } from '@angular/forms';
import { SupplierApiService } from '../../../services/supplier-api.service';
import { AddEditSupplierComponent } from '../add-edit-supplier/add-edit-supplier.component';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';
import { Actions } from '../../../models/breadcrumb';

@Component({
  selector: 'app-all-suppliers',
  imports: [TopBarComponent, AddEditSupplierComponent, FormsModule,CommonModule],
  templateUrl: './all-suppliers.component.html',
  styleUrl: './all-suppliers.component.css'
})
export class AllSuppliersComponent implements OnInit {
  suppliers: any[] = [];
  searchText: string = '';
  pageNumber = 1;
  pageSize=10;
  totalCount=0;
  showModal:boolean=false;
  modalMode: 'add'|'edit' = 'add';
  selectedSupplier : any;
  filteredSupplier:any;
  deleteTarget = signal<any | null>(null);
  protected readonly Math = Math;
  action :Actions[] = [
    {
      label: 'New Supplier',
      icon: 'bi bi-plus',
      style: { 'background-color': 'blue', color: 'white' },
      func: 'openAddModel'
    }
  ];
  constructor(private supplierApi: SupplierApiService){}
  ngOnInit(): void {
    this.loadSuppliers();
  }
  loadSuppliers(){
    this.supplierApi.getAllSuppliers(this.pageNumber,this.pageSize).subscribe({
      next:(data)=>{
        this.suppliers =data.items;
        this.filteredSupplier = [...data.items];
        this.totalCount = data.totalCount; 
      },
      error:(err)=>{
        console.log("error during load suppliers: ",err)
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
    this.loadSuppliers();
  }
}
previousPage() {
  if (this.pageNumber > 1) {
      this.pageNumber--;
      this.loadSuppliers();
    }
}
confirmDelete(sup: any) {
 this.deleteTarget.set(sup); 
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
          this.supplierApi.deleteSupplier(target.id).subscribe({
          next: () => resolve(true),
          error:()=> reject(new Error('Delete failed'))
          });
         }) }})   
      .then((result) => {
        if (result.isConfirmed) {
          this.deleteTarget.set(null);
           this.loadSuppliers();
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
openEdit(sup: any) {
  this.showModal = true;
  this.modalMode = 'edit';
  this.selectedSupplier = sup;
}

applyFilters() {
    this.filteredSupplier = this.suppliers.filter(sup => {
      const matchesSearch =
        this.searchText === '' ||
        sup.id.toString().includes(this.searchText) ||
        sup.name.toLowerCase().includes(this.searchText.toLowerCase()) ||
        sup.contactEmail.toLowerCase().includes(this.searchText.toLowerCase()) ||
        sup.address.toLowerCase().includes(this.searchText.toLowerCase()) ||
        sup.phoneNumber.toLowerCase().includes(this.searchText.toLowerCase());
      return matchesSearch;
  });
}
  
}
