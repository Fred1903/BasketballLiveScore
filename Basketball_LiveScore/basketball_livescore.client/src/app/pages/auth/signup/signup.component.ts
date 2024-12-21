import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css']
})
export class SignupComponent implements OnInit {
  registerForm!: FormGroup;
  errorMessage: string | null = null;

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) { }

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      username: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  get firstNameControl(): FormControl | null {
    return this.registerForm.get('firstName') as FormControl | null;
  }

  get lastNameControl(): FormControl | null {
    return this.registerForm.get('lastName') as FormControl | null;
  }

  get usernameControl(): FormControl | null {
    return this.registerForm.get('username') as FormControl | null;
  }

  get emailControl(): FormControl | null {
    return this.registerForm.get('email') as FormControl | null;
  }

  get passwordControl(): FormControl | null {
    return this.registerForm.get('password') as FormControl | null;
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.authService.register(this.registerForm.value).subscribe({
        next: () => {
          this.router.navigate(['/login']); //aprÈs avoir créer un compte on redirige vers la fenetre de login
          this.registerForm.reset();
          this.errorMessage = null;
        },
        error: (error: any) => {
          console.error('Error during registrattttion:', error);
          this.errorMessage = error.message;
        },
      });
    } else {
      this.errorMessage = "Please fill out the form correctly.";
    }
  }
}
