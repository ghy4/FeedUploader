import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';

export interface Product {
  id: string;
  name: string;
  sku: string;
  category: string;
  brand: string;
  price: number;
  stock: number;
  description: string;
  images: string[];
  attributes: { [key: string]: string };
}

export interface FeedFile {
  name: string;
  size: number;
  type: string;
  lastModified: Date;
  products: Product[];
}

@Injectable({
  providedIn: 'root'
})
export class FeedService {
  private mockProducts: Product[] = [
    {
      id: '1',
      name: 'iPhone 15 Pro',
      sku: 'IPH15PRO-128',
      category: 'Electronice > Telefoane',
      brand: 'Apple',
      price: 4999,
      stock: 25,
      description: 'Cel mai nou iPhone cu cameră profesională',
      images: ['https://via.placeholder.com/300x300?text=iPhone+15+Pro'],
      attributes: {
        'Culoare': 'Titanium',
        'Capacitate': '128GB',
        'Procesor': 'A17 Pro'
      }
    },
    {
      id: '2',
      name: 'Samsung Galaxy S24',
      sku: 'SAMS24-256',
      category: 'Electronice > Telefoane',
      brand: 'Samsung',
      price: 3999,
      stock: 15,
      description: 'Flagship Samsung cu AI integrat',
      images: ['https://via.placeholder.com/300x300?text=Samsung+S24'],
      attributes: {
        'Culoare': 'Negru',
        'Capacitate': '256GB',
        'Procesor': 'Snapdragon 8 Gen 3'
      }
    },
    {
      id: '3',
      name: 'MacBook Air M3',
      sku: 'MBA-M3-512',
      category: 'Electronice > Laptopuri',
      brand: 'Apple',
      price: 8999,
      stock: 8,
      description: 'Laptop ultra-portabil cu performanță excepțională',
      images: ['https://via.placeholder.com/300x300?text=MacBook+Air+M3'],
      attributes: {
        'Culoare': 'Space Gray',
        'Capacitate': '512GB',
        'Procesor': 'M3',
        'RAM': '8GB'
      }
    }
  ];

  constructor() { }

  uploadFile(file: File): Observable<FeedFile> {
    // Simulate file upload and parsing
    return of({
      name: file.name,
      size: file.size,
      type: file.type,
      lastModified: new Date(file.lastModified),
      products: this.mockProducts
    }).pipe(delay(2000));
  }

  getProducts(): Observable<Product[]> {
    return of(this.mockProducts).pipe(delay(500));
  }

  updateProduct(product: Product): Observable<Product> {
    const index = this.mockProducts.findIndex(p => p.id === product.id);
    if (index !== -1) {
      this.mockProducts[index] = product;
    }
    return of(product).pipe(delay(500));
  }

  deleteProduct(productId: string): Observable<boolean> {
    const index = this.mockProducts.findIndex(p => p.id === productId);
    if (index !== -1) {
      this.mockProducts.splice(index, 1);
      return of(true).pipe(delay(500));
    }
    return of(false).pipe(delay(500));
  }

  searchProducts(query: string): Observable<Product[]> {
    const filtered = this.mockProducts.filter(product =>
      product.name.toLowerCase().includes(query.toLowerCase()) ||
      product.sku.toLowerCase().includes(query.toLowerCase()) ||
      product.brand.toLowerCase().includes(query.toLowerCase())
    );
    return of(filtered).pipe(delay(300));
  }
} 