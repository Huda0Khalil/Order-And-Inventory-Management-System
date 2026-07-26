import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

  form: FormGroup;
  isPasswordHidden = true;

  passwordPattern =
    '^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,20}$';

  patternEmail =
    '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,4}$';

  messageError!: string;

  constructor(
    private _authService: AuthService,
    private _router: Router
  ) {
    this.form = new FormGroup({
      email: new FormControl('', [
        Validators.required,
        Validators.pattern(this.patternEmail)
      ]),
      password: new FormControl('', [
        Validators.required,
        Validators.pattern(this.passwordPattern)
      ])
    });
  }

  onSubmit(): void {
    this.messageError = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.messageError = 'Please enter a valid email and password.';
      return;
    }

    this._authService.login(this.form.value).subscribe({
      next: () => {
        this._router.navigate(['/Dashboard']);
      },
      error: (err) => {
        this.messageError =
          err?.error?.message ||
          err?.error ||
          'Invalid email or password.';
      }
    });
  }
}