import { Component } from '@angular/core';
import { AuthService, RegisterRequest } from '../../services/auth.service'; // Adjust path
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule] // Import required modules
})
export class RegisterComponent {
  userData: RegisterRequest = {
    name: '',
    surname: '',
    email: '',
    password: '',
    contactNumber: '',
    role: 'User' // Default role
  };
  errorMessage: string = '';

  constructor(private authService: AuthService, private router: Router) { }

  onRegister(): void {
    this.authService.register(this.userData).subscribe({
      next: (response) => {
        console.log('Registration successful', response.user);
        this.router.navigate(['/login']); // Redirect to login
      },
      error: (err) => {
        this.errorMessage = err.message || 'Registration failed';
      }
    });
  }
}