import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';

export interface Category {
  id: string;
  name: string;
  path: string;
  children?: Category[];
  mappedTo?: string;
  attributes?: string[];
}

export interface MarketplaceCategory {
  id: string;
  name: string;
  path: string;
  children?: MarketplaceCategory[];
  attributes: string[];
}

export interface CategoryMapping {
  feedCategoryId: string;
  marketplaceCategoryId: string;
  confidence: number;
  attributes: { [key: string]: string };
}

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private feedCategories: Category[] = [];

  private marketplaceCategories: MarketplaceCategory[] = [];

  constructor() { }

  getFeedCategories(): Observable<Category[]> {
    return of(this.feedCategories).pipe(delay(500));
  }

  getMarketplaceCategories(): Observable<MarketplaceCategory[]> {
    return of(this.marketplaceCategories).pipe(delay(500));
  }

  getCategoryMapping(categoryId: string): Observable<CategoryMapping | null> {
    const category = this.findCategoryById(this.feedCategories, categoryId);
    if (category?.mappedTo) {
      const mapping: CategoryMapping = {
        feedCategoryId: categoryId,
        marketplaceCategoryId: category.mappedTo,
        confidence: 0.85,
        attributes: {}
      };
      return of(mapping).pipe(delay(300));
    }
    return of(null).pipe(delay(300));
  }

  suggestMapping(categoryId: string): Observable<CategoryMapping> {
    // Simulate AI suggestion
    const suggestions: CategoryMapping[] = [
      {
        feedCategoryId: categoryId,
        marketplaceCategoryId: 'suggested-category',
        confidence: 0.85,
        attributes: { 'Brand': 'auto', 'Model': 'auto' }
      }
    ];
    
    return of(suggestions[0]).pipe(delay(1000));
  }

  saveMapping(mapping: CategoryMapping): Observable<boolean> {
    const category = this.findCategoryById(this.feedCategories, mapping.feedCategoryId);
    if (category) {
      category.mappedTo = mapping.marketplaceCategoryId;
      return of(true).pipe(delay(500));
    }
    return of(false).pipe(delay(500));
  }

  getAttributeSuggestions(attribute: string): Observable<string[]> {
    const suggestions: { [key: string]: string[] } = {
      'Brand': ['Apple', 'Samsung', 'Huawei', 'Xiaomi', 'OnePlus'],
      'Culoare': ['Negru', 'Alb', 'Gri', 'Albastru', 'Roșu', 'Verde'],
      'Mărime': ['XS', 'S', 'M', 'L', 'XL', 'XXL'],
      'Capacitate': ['64GB', '128GB', '256GB', '512GB', '1TB'],
      'Procesor': ['A17 Pro', 'Snapdragon 8 Gen 3', 'Exynos 2400', 'M3']
    };
    
    return of(suggestions[attribute] || []).pipe(delay(300));
  }

  private findCategoryById(categories: Category[], id: string): Category | null {
    for (const category of categories) {
      if (category.id === id) {
        return category;
      }
      if (category.children) {
        const found = this.findCategoryById(category.children, id);
        if (found) return found;
      }
    }
    return null;
  }
} 