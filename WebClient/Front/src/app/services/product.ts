import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product {
  id: number;
  name: string;
  description: string;
  model: string;
  manufacturer: string;
  category: string;
  price: number;
  salePrice: number;
  currency: string;
  quantity: number;
  warranty: number | null; // Backend now allows null
  mainImage: string;
  additionalImage1?: string | null;
  additionalImage2?: string | null;
  additionalImage3?: string | null;
  additionalImage4?: string | null;
  type: string;
  // Optional collections coming from ProductDTO
  attributes?: any[];
  extractedAttributes?: { [key: string]: string } | null;
  temps?: { [key: string]: string }; // Legacy / temporary field if still used somewhere
}

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private apiUrl = 'http://localhost:5122/api/Product';

  constructor(private http: HttpClient) { }

  // Get all products
  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/Products`);
  }

  // Get a product by ID
  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/Product/${id}`);
  }

  // Create a new product
  createProduct(product: Product): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, product);
  }

  // Update an existing product
  updateProduct(id: number, product: Product): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, product);
  }

  // Delete a product
  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
