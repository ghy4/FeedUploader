import { Injectable } from '@angular/core';
import { ProductService, Product } from './product';
import { map, Observable, of } from 'rxjs';

export interface CMCategory {
  id: string;
  name: string;
  path?: string;
  productCount?: number;
  children?: CMCategory[];
  expanded?: boolean;
  mapped?: boolean;
  needsMapping?: boolean;
  mappedFeedCategories?: string[];
}

export interface CategoryMappingRecord {
  id: string;
  feedCategoryId: string;
  marketplaceCategoryId: string;
  marketplaceId: string;
  status: 'active' | 'pending' | 'disabled';
  attributeMappings?: Record<string, string>;
}

@Injectable({ providedIn: 'root' })
export class CategoryMappingService {
  private storageKey = 'categoryMappings';

  constructor(private productService: ProductService) {}

  getFeedCategories(): Observable<CMCategory[]> {
    return this.productService.getProducts().pipe(
      map((products: Product[]) => {
        const groups: Record<string, number> = {};
        products.forEach(p => {
          const cat = p.category || 'Necunoscut';
            groups[cat] = (groups[cat] || 0) + 1;
        });
        return Object.entries(groups).map(([name, count]) => ({
          id: 'feed-' + name.toLowerCase().replace(/[^a-z0-9]+/g, '-'),
          name,
          productCount: count,
          mapped: false,
          needsMapping: true
        }));
      })
    );
  }

  // Static marketplace categories for now (could later be fetched from backend)
  getMarketplaceCategories(marketplaceId: string): Observable<CMCategory[]> {
    const base: CMCategory[] = [
      {
        id: marketplaceId + '-electronics',
        name: 'Electronice',
        children: [
          { id: marketplaceId + '-phones', name: 'Telefoane', productCount: 0 },
          { id: marketplaceId + '-laptops', name: 'Laptopuri', productCount: 0 },
          { id: marketplaceId + '-tvs', name: 'Televizoare', productCount: 0 }
        ],
        expanded: true
      },
      {
        id: marketplaceId + '-appliances',
        name: 'Electrocasnice',
        children: [
          { id: marketplaceId + '-kitchen', name: 'Bucătărie' },
          { id: marketplaceId + '-cleaning', name: 'Curățenie' }
        ],
        expanded: true
      }
    ];
    return of(base);
  }

  loadMappings(): CategoryMappingRecord[] {
    try {
      const raw = localStorage.getItem(this.storageKey);
      if (!raw) return [];
      return JSON.parse(raw) as CategoryMappingRecord[];
    } catch {
      return [];
    }
  }

  saveMappings(mappings: CategoryMappingRecord[]): void {
    localStorage.setItem(this.storageKey, JSON.stringify(mappings));
  }
}
