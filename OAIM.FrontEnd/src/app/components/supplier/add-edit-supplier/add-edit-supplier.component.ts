import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { SupplierApiService } from '../../../services/supplier-api.service';

@Component({
  selector: 'app-add-edit-supplier',
  imports: [ReactiveFormsModule],
  templateUrl: './add-edit-supplier.component.html',
  styleUrl: './add-edit-supplier.component.css'
})
export class AddEditSupplierComponent implements OnChanges{
  form: FormGroup = new FormGroup({});
  @Input() mode:'add'|'edit' = 'add';
  @Input() supplier :any| undefined;
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  constructor(private supplierApi: SupplierApiService){
    const patternEmail = '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$';
    this.form = new FormGroup({
    Id: new FormControl(0),
    name: new FormControl('',[Validators.required, Validators.minLength(3)]),
    contactEmail: new FormControl('',[Validators.required, Validators.min(0), Validators.pattern(patternEmail)]),
    phoneNumber: new FormControl('',[Validators.required,Validators.pattern('^[0-9]{10}$')]),
    address: new FormControl('',[Validators.required, Validators.min(0)])  
  }); 
  }
  ngOnChanges(changes: SimpleChanges): void {
 if (this.mode === 'edit' && this.supplier) {
      this.form.patchValue({
        Id: this.supplier.id,
        name: this.supplier.name,
        contactEmail: this.supplier.contactEmail,
        phoneNumber: this.supplier.phoneNumber,
        address: this.supplier.address,
      });
      console.log('Form values after patching:', this.form.value);
    }else {
      this.form.reset();
  }  }
save() {
  if(this.mode == 'add'){
    this.supplierApi.addSupplier(this.form.value).subscribe({
      next:(data)=>{
        console.log(data);
        this.saved.emit();
      },
      error:(err)=>{
        console.log(err);
      }
    })
  }
  else if(this.mode == 'edit'){
    this.supplierApi.updateSupplier(this.form.value).subscribe({
      next:(data)=>{
        console.log(data);
        this.saved.emit();
      },
      error:(err)=>{
        console.log(err);
      }
    })
  }
  
  console.log(this.form.value);
}
}
