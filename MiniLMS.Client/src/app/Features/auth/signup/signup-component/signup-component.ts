import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../../Core/Services/auth-service';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: 'signup-component.html',
})
export class SignupComponent implements OnInit {
  signupForm!: FormGroup;
  isLoading = false;
  selectedRole: 'Student' | 'Instructor' = 'Student';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.signupForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      username: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['Student', [Validators.required]],
      headline: [''],
      bio: ['']
    });
  }

  setRole(role: 'Student' | 'Instructor'): void {
    this.selectedRole = role;
    this.signupForm.patchValue({ role });
  }

  onSubmit(): void {
    if (this.signupForm.invalid) {
      this.signupForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.authService.signup(this.signupForm.value).subscribe({
      next: (res) => {
        this.isLoading = false;
        alert('Account created successfully! Redirecting to your portal...');
        if (res.role === 'Instructor') {
          this.router.navigate(['/instructor/dashboard']);
        } else {
          this.router.navigate(['/student/dashboard']);
        }
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }
}