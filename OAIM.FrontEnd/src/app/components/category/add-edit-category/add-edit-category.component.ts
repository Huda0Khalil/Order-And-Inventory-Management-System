import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoryApiService } from '../../../services/category-api.service';
import { ICategory } from '../../../models/ICategory';

@Component({
  selector: 'app-add-edit-category',
  imports: [ReactiveFormsModule],
  templateUrl: './add-edit-category.component.html',
  styleUrl: './add-edit-category.component.css'
})
export class AddEditCategoryComponent implements OnChanges {
@Input() modalMode : 'add'|'edit' = 'add';
@Input() category : ICategory| undefined;
@Output() closed= new EventEmitter<void>();  
@Output() saved = new EventEmitter<void>();

form: FormGroup = new FormGroup({});
constructor(private categoryApi: CategoryApiService){
  this.form = new FormGroup({
    Id: new FormControl(0),
    Name: new FormControl('',[Validators.required, Validators.minLength(3)])
  })
}
ngOnChanges(changes: SimpleChanges): void {
  if(this.modalMode == 'edit'){
    this.form.patchValue({
      Id: this.category?.Id,
      Name: this.category?.Name
    })
  }
}

save(){
  if(this.form.invalid){
    return;
  }
  if(this.modalMode == 'add'){
    this.categoryApi.addCategory(this.form.value).subscribe({
        next:(data)=>{
          this.saved.emit();
        },
        error:(error)=>{
          console.log("Error adding Category ", error);
        }
      });
  }else{
    this.categoryApi.updateCategory(this.form.value).subscribe({
      next:()=>{
        this.saved.emit();
      },
      error:(error)=>{
        console.log("Error updating category ", error);
      }
    })
  }
  
 }
}
