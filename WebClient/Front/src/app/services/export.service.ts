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
          productId: product.id.toString(), // Convert number to string
          productName: product.name,
          error: 'Categoria lipsește',
          field: 'category'
        });
      }
      
      if (product.price <= 0) {
        errors.push({
          productId: product.id.toString(), // Convert number to string
          productName: product.name,
          error: 'Prețul trebuie să fie mai mare decât 0',
          field: 'price'
        });
      }
      // Removed stock check since it's not in the Product model
    });
    
    return of(errors).pipe(delay(1000));
  }

  downloadExport(jobId: string): Observable<Blob> {
    // Simulate file download with existing Product properties
    const csvContent = 'Product ID,Name,Description,Model,Manufacturer,Category,Price,Sale Price,Currency,Quantity,Warranty,Main Image,Additional Image 1,Additional Image 2,Additional Image 3,Additional Image 4,Type\n';
    const job = this.exportJobs.find(j => j.id === jobId);
    if (job) {
      // Simulate adding product data (replace with actual data if needed)
      const sampleProduct = '1,Sample Product,Sample Description,Model X,Manufacturer X,Category X,100,90,RON,10,12,http://example.com/main.jpg,http://example.com/img1.jpg,http://example.com/img2.jpg,http://example.com/img3.jpg,http://example.com/img4.jpg,Type X';
      const blob = new Blob([csvContent + sampleProduct], { type: 'text/csv' });
      return of(blob).pipe(delay(500));
    }
    return of(new Blob()).pipe(delay(500));
  }
}