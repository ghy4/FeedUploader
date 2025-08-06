import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { Product } from './feed.service';

export interface ExportJob {
  id: string;
  name: string;
  status: 'pending' | 'processing' | 'completed' | 'failed';
  progress: number;
  totalProducts: number;
  exportedProducts: number;
  failedProducts: number;
  createdAt: Date;
  completedAt?: Date;
  marketplace: string;
  errors?: ExportError[];
}

export interface ExportError {
  productId: string;
  productName: string;
  error: string;
  field?: string;
}

export interface ExportSettings {
  marketplace: string;
  selectedProducts: string[];
  categoryMappings: { [key: string]: string };
  attributeMappings: { [key: string]: string };
  scheduledExport?: {
    enabled: boolean;
    time: string;
    frequency: 'daily' | 'weekly' | 'monthly';
  };
}

@Injectable({
  providedIn: 'root'
})
export class ExportService {
  private exportJobs: ExportJob[] = [];

  constructor() { }

  getExportJobs(): Observable<ExportJob[]> {
    return of(this.exportJobs).pipe(delay(500));
  }

  getExportJob(id: string): Observable<ExportJob | null> {
    const job = this.exportJobs.find(j => j.id === id);
    return of(job || null).pipe(delay(300));
  }

  createExportJob(settings: ExportSettings): Observable<ExportJob> {
    const newJob: ExportJob = {
      id: Date.now().toString(),
      name: `Export ${settings.marketplace} - ${new Date().toISOString().split('T')[0]}`,
      status: 'pending',
      progress: 0,
      totalProducts: settings.selectedProducts.length,
      exportedProducts: 0,
      failedProducts: 0,
      createdAt: new Date(),
      marketplace: settings.marketplace
    };

    this.exportJobs.unshift(newJob);
    return of(newJob).pipe(delay(500));
  }

  startExport(jobId: string): Observable<ExportJob> {
    const job = this.exportJobs.find(j => j.id === jobId);
    if (job) {
      job.status = 'processing';
      job.progress = 0;
      
      // Simulate export progress
      const interval = setInterval(() => {
        job.progress += Math.random() * 20;
        if (job.progress >= 100) {
          job.progress = 100;
          job.status = 'completed';
          job.completedAt = new Date();
          job.exportedProducts = job.totalProducts - job.failedProducts;
          clearInterval(interval);
        }
      }, 1000);
    }
    
    return of(job!).pipe(delay(100));
  }

  getExportStatistics(): Observable<{
    totalExports: number;
    successfulExports: number;
    failedExports: number;
    totalProductsExported: number;
    lastExportDate?: Date;
  }> {
    const stats = {
      totalExports: this.exportJobs.length,
      successfulExports: this.exportJobs.filter(j => j.status === 'completed').length,
      failedExports: this.exportJobs.filter(j => j.status === 'failed').length,
      totalProductsExported: this.exportJobs.reduce((sum, j) => sum + j.exportedProducts, 0),
      lastExportDate: this.exportJobs[0]?.createdAt
    };
    
    return of(stats).pipe(delay(300));
  }

  validateProducts(products: Product[], marketplace: string): Observable<ExportError[]> {
    const errors: ExportError[] = [];
    
    products.forEach(product => {
      if (!product.category) {
        errors.push({
          productId: product.id,
          productName: product.name,
          error: 'Categoria lipsește',
          field: 'category'
        });
      }
      
      if (product.price <= 0) {
        errors.push({
          productId: product.id,
          productName: product.name,
          error: 'Prețul trebuie să fie mai mare decât 0',
          field: 'price'
        });
      }
      
      if (product.stock < 0) {
        errors.push({
          productId: product.id,
          productName: product.name,
          error: 'Stocul nu poate fi negativ',
          field: 'stock'
        });
      }
    });
    
    return of(errors).pipe(delay(1000));
  }

  downloadExport(jobId: string): Observable<Blob> {
    // Simulate file download
    const csvContent = 'Product ID,Name,SKU,Category,Brand,Price,Stock\n';
    const blob = new Blob([csvContent], { type: 'text/csv' });
    return of(blob).pipe(delay(500));
  }
} 