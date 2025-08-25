import { Component } from '@angular/core';
import { AuthService, User, LoginRequest } from '../../services/auth.service'; // Adjust path
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule] // Import required modules
})
export class LoginComponent {
  credentials: LoginRequest = { email: '', password: '' };
  errorMessage: string = '';

  constructor(private authService: AuthService, private router: Router) { }

  onLogin(): void {
    this.authService.login(this.credentials).subscribe({
      next: (response) => {
        console.log('Login successful', response.user);
        this.router.navigate(['/dashboard']); // Redirect to dashboard
      },
      error: (err) => {
        this.errorMessage = err.message || 'Login failed';
      }
    });
  }

  goToRegister(): void {
    this.router.navigate(['/register']);
  }
}