import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: true,
  styleUrls: ['./app.css'],
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive
  ]
})
export class App implements OnInit {
  protected readonly title = 'Platformă Export Marketplace';
  constructor(private router: Router) {}

  ngOnInit(): void {
    // ...existing code...
  }

  isAuthPage(): boolean {
    const url = this.router.url;
    return url === '/login' || url === '/register';
  }
}
