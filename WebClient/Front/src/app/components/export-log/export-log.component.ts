import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

interface ExportJob {
  id: string;
  name: string;
  marketplace: string;
  startTime: Date;
  endTime?: Date;
  status: 'completed' | 'failed' | 'processing';
  progress: number;
  totalProducts: number;
  successfulProducts: number;
  failedProducts: number;
  logEntries?: LogEntry[];
}

interface LogEntry {
  timestamp: Date;
  type: 'success' | 'error' | 'warning' | 'info';
  action?: string;
  productName?: string;
  productSku?: string;
  message: string;
  details?: string;
}

interface ErrorAnalysis {
  type: string;
  description: string;
  frequency: number;
  lastOccurrence: Date;
  recommendedActions: string[];
}

@Component({
  selector: 'app-export-log',
  templateUrl: './export-log.component.html',
  styleUrl: './export-log.component.css',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule
  ]
})
export class ExportLogComponent implements OnInit {
  exportJobs: ExportJob[] = [];
  filteredExportJobs: ExportJob[] = [];
  selectedJob: ExportJob | null = null;
  
  marketplaces: string[] = ['eMAG', 'Altex', 'Okazii', 'OLX'];
  
  // Filters
  selectedStatus = '';
  selectedMarketplace = '';
  
  // Log filtering
  logTypeFilter = '';
  logSearchTerm = '';
  filteredLogEntries: LogEntry[] = [];
  
  // Statistics
  totalExports = 0;
  successfulExports = 0;
  failedExports = 0;
  
  // Error analysis
  errorAnalysis: ErrorAnalysis[] = [];

  ngOnInit(): void {
    this.loadMockData();
    this.calculateStatistics();
    this.generateErrorAnalysis();
  }

  loadMockData(): void {
    this.exportJobs = [];
    this.filteredExportJobs = [...this.exportJobs];
  }

  calculateStatistics(): void {
    this.totalExports = this.exportJobs.length;
    this.successfulExports = this.exportJobs.filter(job => job.status === 'completed').length;
    this.failedExports = this.exportJobs.filter(job => job.status === 'failed').length;
  }

  generateErrorAnalysis(): void {
    this.errorAnalysis = [];
  }

  filterJobs(): void {
    this.filteredExportJobs = this.exportJobs.filter(job => {
      const matchesStatus = !this.selectedStatus || job.status === this.selectedStatus;
      const matchesMarketplace = !this.selectedMarketplace || job.marketplace === this.selectedMarketplace;
      
      return matchesStatus && matchesMarketplace;
    });
  }

  viewJobDetails(job: ExportJob): void {
    this.selectedJob = job;
    this.filteredLogEntries = job.logEntries || [];
  }

  closeJobDetails(): void {
    this.selectedJob = null;
    this.filteredLogEntries = [];
  }

  filterLogEntries(): void {
    if (!this.selectedJob?.logEntries) return;

    this.filteredLogEntries = this.selectedJob.logEntries.filter(entry => {
      const matchesType = !this.logTypeFilter || entry.type === this.logTypeFilter;
      const matchesSearch = !this.logSearchTerm || 
        entry.message.toLowerCase().includes(this.logSearchTerm.toLowerCase()) ||
        (entry.productName && entry.productName.toLowerCase().includes(this.logSearchTerm.toLowerCase())) ||
        (entry.productSku && entry.productSku.toLowerCase().includes(this.logSearchTerm.toLowerCase()));

      return matchesType && matchesSearch;
    });
  }

  downloadExport(job: ExportJob): void {
    if (job.status === 'completed') {
      // Simulate download
      const filename = `export_${job.marketplace}_${job.id}.xlsx`;
      alert(`Descarcă fișierul: ${filename}`);
    }
  }

  deleteJob(job: ExportJob): void {
    if (confirm(`Ești sigur că vrei să ștergi exportul ${job.name}?`)) {
      this.exportJobs = this.exportJobs.filter(j => j.id !== job.id);
      this.filterJobs();
      this.calculateStatistics();
      
      if (this.selectedJob?.id === job.id) {
        this.closeJobDetails();
      }
    }
  }

  clearLogs(): void {
    if (confirm('Ești sigur că vrei să ștergi toate log-urile de export?')) {
      this.exportJobs = [];
      this.filteredExportJobs = [];
      this.selectedJob = null;
      this.calculateStatistics();
    }
  }

  exportLogs(): void {
    // Simulate export of logs
    const filename = `export_logs_${new Date().toISOString().split('T')[0]}.csv`;
    alert(`Log-urile au fost exportate în fișierul: ${filename}`);
  }

  // Utility methods
  getSuccessRate(): number {
    if (this.totalExports === 0) return 0;
    return Math.round((this.successfulExports / this.totalExports) * 100);
  }

  getJobSuccessRate(job: ExportJob): number {
    if (job.totalProducts === 0) return 0;
    return Math.round((job.successfulProducts / job.totalProducts) * 100);
  }

  getJobStatusClass(status: string): string {
    switch (status) {
      case 'completed': return 'status-success';
      case 'failed': return 'status-error';
      case 'processing': return 'status-warning';
      default: return '';
    }
  }

  getJobStatusLabel(status: string): string {
    switch (status) {
      case 'completed': return 'Finalizat';
      case 'failed': return 'Eșuat';
      case 'processing': return 'În Progres';
      default: return status;
    }
  }

  getDuration(startTime: Date, endTime?: Date): string {
    if (!endTime) return 'În progres...';
    
    const diffMs = endTime.getTime() - startTime.getTime();
    const diffMinutes = Math.floor(diffMs / (1000 * 60));
    const diffSeconds = Math.floor((diffMs % (1000 * 60)) / 1000);
    
    if (diffMinutes > 0) {
      return `${diffMinutes}m ${diffSeconds}s`;
    }
    return `${diffSeconds}s`;
  }
} 