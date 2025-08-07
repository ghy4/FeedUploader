import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  template: `
    <div style="padding: 20px;">
      <h2>Dashboard Component</h2>
      <p>Această componentă funcționează corect!</p>
      <div style="background-color: #e3f2fd; padding: 15px; border-radius: 4px;">
        <h3>Test Status</h3>
        <p>Componenta se încarcă cu succes.</p>
      </div>
    </div>
  `,
  standalone: true,
  imports: [CommonModule]
})
export class DashboardComponent {
} 