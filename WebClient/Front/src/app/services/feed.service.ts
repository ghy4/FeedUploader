import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType, HttpRequest } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ProductService, Product } from './product'; // Adjust the import path as needed

export interface FeedFile {
  name: string;
  size: number;
  type: string;
  lastModified: Date;
  products: Product[];
}

export type { Product }

@Injectable({
  providedIn: 'root'
})
export class FeedService {
  private apiUrl = 'http://localhost:5122/api/feed'; // Match your backend FeedController route

  constructor(private http: HttpClient, private productService: ProductService) { }

  uploadFile(file: File, userId: number = 1): Observable<{ progress: number; products?: Product[] }> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('userId', userId.toString());

    const req = new HttpRequest('POST', `${this.apiUrl}/upload`, formData, {
      reportProgress: true,
      responseType: 'json'
    });

    return this.http.request(req).pipe(
      map((event: HttpEvent<any>) => {
        switch (event.type) {
          case HttpEventType.UploadProgress:
            // Safely handle undefined total
            const total = event.total || file.size; // Fallback to file size if total is undefined
            const progress = Math.round(100 * event.loaded / total);
            return { progress: progress >= 0 ? progress : 0 }; // Ensure progress is non-negative
          case HttpEventType.Response:
            return { progress: 100, products: event.body as Product[] };
          default:
            return { progress: 0 };
        }
      }),
      catchError(error => {
        console.error('Upload error:', error);
        return of({ progress: -1, error: error.message });
      })
    );
  }

  getProducts(): Observable<Product[]> {
    return this.productService.getProducts(); // Delegate to ProductService
  }

  getProductById(id: number): Observable<Product> {
    return this.productService.getProductById(id); // Delegate to ProductService
  }

  createProduct(product: Product): Observable<Product> {
    return this.productService.createProduct(product); // Delegate to ProductService
  }

  updateProduct(id: number, product: Product): Observable<Product> {
    return this.productService.updateProduct(id, product); // Delegate to ProductService
  }

  deleteProduct(id: number): Observable<void> {
    return this.productService.deleteProduct(id); // Delegate to ProductService
  }

  searchProducts(query: string): Observable<Product[]> {
    return this.productService.getProducts().pipe(
      map(products => products.filter(product =>
        product.name.toLowerCase().includes(query.toLowerCase()) ||
        (product.model?.toLowerCase() || '').includes(query.toLowerCase()) ||
        (product.manufacturer?.toLowerCase() || '').includes(query.toLowerCase())
      )),
      catchError(error => {
        console.error('Search products error:', error);
        return of([]);
      })
    );
  }

  exportToExcel(productIds: number[]): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/export`, productIds, { responseType: 'blob' }).pipe(
      catchError(error => {
        console.error('Export error:', error);
        return of(new Blob());
      })
    );
  }

  clearDatabase(): Observable<boolean> {
    return this.http.delete(`${this.apiUrl}/clear`, { observe: 'response' }).pipe(
      map(res => res.status === 204 || res.status === 200),
      catchError(error => {
        console.error('Clear database error:', error);
        return of(false);
      })
    );
  }
}