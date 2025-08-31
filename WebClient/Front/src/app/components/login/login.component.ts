import { Component } from '@angular/core';
import { AuthService, User, LoginRequest } from '../../services/auth.service'; 
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule] 
})
export class LoginComponent {
  credentials: LoginRequest = { email: '', password: '' };
  errorMessage: string = '';

  constructor(private authService: AuthService, private router: Router) { }

  onLogin(): void {
    this.authService.login(this.credentials).subscribe({
      next: (response) => {
        console.log('Login successful', response.user);
        this.router.navigate(['/dashboard']); 
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