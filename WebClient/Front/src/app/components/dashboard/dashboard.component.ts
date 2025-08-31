import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProductService, Product } from '../../services/product';
import { ActivityService, ActivityEntry } from '../../services/activity.service';

interface DashboardStats {
  totalProducts: number;
  successfulExports: number;
  failedExports: number;
  successRate: number;
  lastExport: Date | null;
}

interface Activity {
  type: 'upload' | 'export' | 'mapping';
  description: string;
  status: 'success' | 'error' | 'warning';
  date: Date;
}

interface ScheduledExport {
  id: string;
  name: string;
  marketplace: string;
  schedule: string;
  nextRun: Date;
  active: boolean;
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule
  ]
})
export class DashboardComponent implements OnInit {
  stats: DashboardStats = {
    totalProducts: 0,
    successfulExports: 0,
    failedExports: 0,
    successRate: 0,
    lastExport: null
  };

  recentActivities: Activity[] = [];
  scheduledExports: ScheduledExport[] = [];
  loading = false;

  constructor(private products: ProductService, private activity: ActivityService) {}

  ngOnInit(): void {
    this.refreshData();
    // Subscribe to live activity updates
    this.activity.activities$.subscribe(list => {
      this.recentActivities = list.map(a => ({
        type: a.type,
        description: a.description,
        status: a.status,
        date: a.date
      }));
      this.recomputeExportStatsFromActivities();
    });
  }

  getTimeAgo(date: Date | null): string {
    if (!date) return 'Niciodată';
    
    const now = new Date();
    const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));
    
    if (diffInMinutes < 1) return 'Acum';
    if (diffInMinutes < 60) return `${diffInMinutes} min`;
    if (diffInMinutes < 1440) return `${Math.floor(diffInMinutes / 60)} ore`;
    return `${Math.floor(diffInMinutes / 1440)} zile`;
  }

  getActivityIcon(type: string): string {
    switch (type) {
      case 'upload': return 'bi bi-upload';
      case 'export': return 'bi bi-download';
      case 'mapping': return 'bi bi-diagram-3';
      default: return 'bi bi-circle';
    }
  }

  getActivityTypeLabel(type: string): string {
    switch (type) {
      case 'upload': return 'Upload';
      case 'export': return 'Export';
      case 'mapping': return 'Mapare';
      default: return 'Necunoscut';
    }
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'success': return 'status-success';
      case 'error': return 'status-error';
      case 'warning': return 'status-warning';
      default: return '';
    }
  }

  openScheduleModal(): void {
    // Open modal for creating new scheduled export
    console.log('Opening schedule modal...');
  }

  refreshData(): void {
    if (this.loading) return;
    this.loading = true;
    this.products.getProducts().subscribe({
      next: (data: any) => {
        const list: Product[] = Array.isArray(data) ? data : (data?.$values ?? []);
        this.stats.totalProducts = list.length;
        this.loading = false;
      },
      error: _ => { this.loading = false; }
    });
    // Activities already subscribed; export stats update inside subscription
  }

  private recomputeExportStatsFromActivities(): void {
    const exportActs = this.recentActivities.filter(a => a.type === 'export');
    const success = exportActs.filter(a => a.status === 'success').length;
    const failed = exportActs.filter(a => a.status === 'error').length;
    const total = success + failed;
    this.stats.successfulExports = success;
    this.stats.failedExports = failed;
    this.stats.successRate = total ? Math.round((success / total) * 100) : 0;
    this.stats.lastExport = exportActs.length ? exportActs[0].date : null;
  }
} 