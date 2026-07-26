import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CustomerApiService } from '../../../services/customer-api.service';

@Component({
  selector: 'app-add-edit-customer',
  imports: [ReactiveFormsModule],
  templateUrl: './add-edit-customer.component.html',
  styleUrl: './add-edit-customer.component.css'
})
export class AddEditCustomerComponent implements OnChanges{
  @Input() mode:'add' |'edit' = 'add';
  @Input() customer:any;
  form: FormGroup = new FormGroup({});
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  constructor(private customerApi: CustomerApiService){
    const patternEmail = '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$';
    this.form = new FormGroup({
    Id: new FormControl(0),
    firstName: new FormControl('',[Validators.required, Validators.minLength(3)]),
    lastName: new FormControl('',[Validators.required, Validators.minLength(3)]),
    email: new FormControl('',[Validators.required, Validators.min(0), Validators.pattern(patternEmail)]),
    phoneNumber: new FormControl('',[Validators.required,Validators.pattern('^[0-9]{10}$')]),
    address: new FormControl('',[Validators.required, Validators.min(0)])  
  }); 
  }
  ngOnChanges(): void {
 if (this.mode === 'edit' && this.customer) {
      this.form.patchValue({
        Id: this.customer.id,
        firstName: this.customer.firstName,
        lastName: this.customer.lastName,
        email: this.customer.email,
        phoneNumber: this.customer.phoneNumber,
        address: this.customer.address,
      });
      console.log('Form values after patching:', this.form.value);
    }else {
      this.form.reset();
  }  }
save() {
  if(this.mode == 'add'){
    this.customerApi.addCustomer(this.form.value).subscribe({
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
    this.customerApi.updateCustomer(this.form.value).subscribe({
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
