import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  errorMessage: string | null = null;
  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) { }

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      username: ['', [Validators.required]],
      password: ['', [Validators.required]]
    });
  }

  get usernameControl(): FormControl | null {
    return this.loginForm.get('username') as FormControl | null;
  }

  get passwordControl(): FormControl | null {
    return this.loginForm.get('password') as FormControl | null;
  }


  onSubmit(): void {
    if (this.loginForm.valid) {
      this.authService.login(this.loginForm.value).subscribe({
        next: (response: any) => {
          this.router.navigate(['/matches']);
          this.loginForm.reset(); //apres avoir rempli le form on le remet a zero
          this.errorMessage = null;
        },
        error: (error: any) => {
          console.error('Error to log in:', error);
          this.errorMessage = error.message;
        },
      });
    } else {
      this.errorMessage = "Please fill out the form correctly."
    }
  }

}
