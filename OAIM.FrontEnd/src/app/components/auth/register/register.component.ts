import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { RouterLink } from "@angular/router";
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink, CommonModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit{

  form : FormGroup = new FormGroup({});
  isPasswordHidden: boolean = true;
  isConfirmPasswordHidden: boolean = true;
  passwordPattern = '^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,20}$';
  patternEmail = '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$';

  constructor(private _authService: AuthService){
    this.form = new FormGroup({
      userName:new FormControl('', [Validators.required, Validators.minLength(3)]),
      phoneNumber: new FormControl('',[Validators.required, Validators.pattern('^[0-9]{10}$')]),
      email: new FormControl('',[Validators.required, Validators.pattern(this.patternEmail)]),
      password: new FormControl('',[Validators.required, Validators.pattern(this.passwordPattern)]),
      confirmPassword: new FormControl('',[Validators.required]),
      role: new FormControl('Employee')
    },{
      validators:this.passwordMatchValidator()
    })
  }
  ngOnInit(): void {
    console.log(this.form.value);
  }
  onSubmit() {
    if(this.form.invalid) return;
    this._authService.register(this.form.value).subscribe({
      next:(data)=>{
        console.log(data);
      },
      error:(err)=>{
        console.log(err);
      }
    })
  console.log(this.form.value);
}
passwordMatchValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;

    if (!password || !confirmPassword) {
      return null;
    }

    return password === confirmPassword
      ? null
      : { passwordMismatch: true };
  };
}
}
