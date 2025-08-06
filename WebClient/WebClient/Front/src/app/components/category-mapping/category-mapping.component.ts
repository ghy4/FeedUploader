import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface Category {
  id: string;
  name: string;
  path?: string;
  productCount?: number;
  children?: Category[];
  expanded?: boolean;
  mapped?: boolean;
  needsMapping?: boolean;
  mappedFeedCategories?: string[];
}

interface Marketplace {
  id: string;
  name: string;
}

interface CategoryMapping {
  id: string;
  feedCategory: Category;
  marketplaceCategory: Category;
  status: 'active' | 'pending' | 'disabled';
  attributeMappings?: Record<string, string>;
}

interface ProductAttribute {
  name: string;
  type: 'text' | 'number' | 'select' | 'boolean';
  required?: boolean;
  options?: string[];
  sampleValue?: string;
  mappedValue?: string;
}

interface AISuggestion {
  feedCategory: string;
  marketplaceCategory: string;
  confidence: number;
  reason: string;
}

@Component({
  selector: 'app-category-mapping',
  templateUrl: './category-mapping.component.html',
  styleUrl: './category-mapping.component.css',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ]
})
export class CategoryMappingComponent implements OnInit {
  marketplaces: Marketplace[] = [
    { id: 'emag', name: 'eMAG' },
    { id: 'altex', name: 'Altex' },
    { id: 'okazii', name: 'Okazii' },
    { id: 'olx', name: 'OLX' }
  ];

  selectedMarketplace = '';
  
  // Categories
  feedCategories: Category[] = [];
  marketplaceCategories: Category[] = [];
  filteredFeedCategories: Category[] = [];
  filteredMarketplaceCategories: Category[] = [];
  
  // Search terms
  feedSearchTerm = '';
  marketplaceSearchTerm = '';
  
  // Selected categories
  selectedFeedCategory: Category | null = null;
  selectedMarketplaceCategory: Category | null = null;
  
  // Mappings
  mappings: CategoryMapping[] = [];
  selectedMapping: CategoryMapping | null = null;
  
  // Drag & Drop
  isDragOver = false;
  dragOverCategory: Category | null = null;
  draggedCategory: Category | null = null;
  
  // AI Suggestions
  showAISuggestions = false;
  aiSuggestions: AISuggestion[] = [];
  
  // Attributes
  feedAttributes: ProductAttribute[] = [];
  marketplaceAttributes: ProductAttribute[] = [];

  ngOnInit(): void {
    this.loadMockData();
  }

  loadMockData(): void {
    // Initialize with empty categories and mappings
    this.feedCategories = [];
    this.marketplaceCategories = [];
    
    this.filterFeedCategories();
    this.filterMarketplaceCategories();
    this.loadMockMappings();
  }

  loadMockMappings(): void {
    this.mappings = [];
  }

  onMarketplaceChange(): void {
    if (this.selectedMarketplace) {
      this.loadMarketplaceCategories(this.selectedMarketplace);
      this.loadMarketplaceAttributes(this.selectedMarketplace);
    }
  }

  loadMarketplaceCategories(marketplaceId: string): void {
    // In real app, load from API based on marketplace
    this.filterMarketplaceCategories();
  }

  loadMarketplaceAttributes(marketplaceId: string): void {
    // Mock attributes based on marketplace
    if (marketplaceId === 'emag') {
      this.marketplaceAttributes = [
        { name: 'Brand', type: 'text', required: true },
        { name: 'Model', type: 'text', required: true },
        { name: 'Culoare', type: 'select', options: ['Alb', 'Negru', 'Gri', 'Roșu'], required: false },
        { name: 'Garanție', type: 'number', required: true }
      ];
    } else if (marketplaceId === 'altex') {
      this.marketplaceAttributes = [
        { name: 'Producător', type: 'text', required: true },
        { name: 'Cod Produs', type: 'text', required: true },
        { name: 'Dimensiuni', type: 'text', required: false }
      ];
    } else {
      this.marketplaceAttributes = [];
    }

    this.feedAttributes = [];
  }

  filterFeedCategories(): void {
    if (!this.feedSearchTerm) {
      this.filteredFeedCategories = [...this.feedCategories];
    } else {
      this.filteredFeedCategories = this.feedCategories.filter(category =>
        category.name.toLowerCase().includes(this.feedSearchTerm.toLowerCase()) ||
        (category.children && category.children.some(child =>
          child.name.toLowerCase().includes(this.feedSearchTerm.toLowerCase())
        ))
      );
    }
  }

  filterMarketplaceCategories(): void {
    if (!this.marketplaceSearchTerm) {
      this.filteredMarketplaceCategories = [...this.marketplaceCategories];
    } else {
      this.filteredMarketplaceCategories = this.marketplaceCategories.filter(category =>
        category.name.toLowerCase().includes(this.marketplaceSearchTerm.toLowerCase()) ||
        (category.children && category.children.some(child =>
          child.name.toLowerCase().includes(this.marketplaceSearchTerm.toLowerCase())
        ))
      );
    }
  }

  selectFeedCategory(category: Category): void {
    this.selectedFeedCategory = category;
    if (category.children && category.children.length > 0) {
      category.expanded = !category.expanded;
    }
  }

  selectMarketplaceCategory(category: Category): void {
    this.selectedMarketplaceCategory = category;
  }

  toggleMarketplaceCategory(category: Category): void {
    category.expanded = !category.expanded;
  }

  // Drag & Drop functionality
  onDragStart(event: DragEvent, category: Category): void {
    this.draggedCategory = category;
    event.dataTransfer!.effectAllowed = 'move';
    event.dataTransfer!.setData('text/html', category.id);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.dataTransfer!.dropEffect = 'move';
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
    this.dragOverCategory = null;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
    
    if (this.draggedCategory && this.dragOverCategory) {
      this.createMappingFromDrop(this.draggedCategory, this.dragOverCategory);
    }
    
    this.draggedCategory = null;
    this.dragOverCategory = null;
  }

  onCategoryDragOver(event: DragEvent, category: Category): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOverCategory = category;
  }

  onCategoryDragLeave(event: DragEvent, category: Category): void {
    event.preventDefault();
    event.stopPropagation();
    if (this.dragOverCategory === category) {
      this.dragOverCategory = null;
    }
  }

  onCategoryDrop(event: DragEvent, category: Category): void {
    event.preventDefault();
    event.stopPropagation();
    
    if (this.draggedCategory) {
      this.createMappingFromDrop(this.draggedCategory, category);
    }
    
    this.draggedCategory = null;
    this.dragOverCategory = null;
    this.isDragOver = false;
  }

  createMappingFromDrop(feedCategory: Category, marketplaceCategory: Category): void {
    this.selectedFeedCategory = feedCategory;
    this.selectedMarketplaceCategory = marketplaceCategory;
    this.createMapping();
  }

  createMapping(): void {
    if (!this.selectedFeedCategory || !this.selectedMarketplaceCategory) {
      return;
    }

    // Check if mapping already exists
    const existingMapping = this.mappings.find(m => 
      m.feedCategory.id === this.selectedFeedCategory!.id
    );

    if (existingMapping) {
      if (confirm('Această categorie este deja mapată. Vrei să o remapezi?')) {
        this.removeMapping(existingMapping);
      } else {
        return;
      }
    }

    const newMapping: CategoryMapping = {
      id: 'map-' + Date.now(),
      feedCategory: this.selectedFeedCategory,
      marketplaceCategory: this.selectedMarketplaceCategory,
      status: 'active'
    };

    this.mappings.push(newMapping);
    
    // Update category statuses
    this.selectedFeedCategory.mapped = true;
    this.selectedFeedCategory.needsMapping = false;
    
    if (!this.selectedMarketplaceCategory.mappedFeedCategories) {
      this.selectedMarketplaceCategory.mappedFeedCategories = [];
    }
    this.selectedMarketplaceCategory.mappedFeedCategories.push(this.selectedFeedCategory.id);

    // Clear selections
    this.selectedFeedCategory = null;
    this.selectedMarketplaceCategory = null;
  }

  editMapping(mapping: CategoryMapping): void {
    this.selectedMapping = mapping;
    this.selectedFeedCategory = mapping.feedCategory;
    this.selectedMarketplaceCategory = mapping.marketplaceCategory;
  }

  removeMapping(mapping: CategoryMapping): void {
    if (confirm('Ești sigur că vrei să ștergi această mapare?')) {
      this.mappings = this.mappings.filter(m => m.id !== mapping.id);
      
      // Update category statuses
      mapping.feedCategory.mapped = false;
      mapping.feedCategory.needsMapping = true;
      
      if (mapping.marketplaceCategory.mappedFeedCategories) {
        mapping.marketplaceCategory.mappedFeedCategories = 
          mapping.marketplaceCategory.mappedFeedCategories.filter(id => id !== mapping.feedCategory.id);
      }
    }
  }

  clearAllMappings(): void {
    if (confirm('Ești sigur că vrei să ștergi toate mapările?')) {
      this.mappings.forEach(mapping => {
        mapping.feedCategory.mapped = false;
        mapping.feedCategory.needsMapping = true;
        
        if (mapping.marketplaceCategory.mappedFeedCategories) {
          mapping.marketplaceCategory.mappedFeedCategories = [];
        }
      });
      
      this.mappings = [];
    }
  }

  resetMappings(): void {
    this.clearAllMappings();
    this.selectedFeedCategory = null;
    this.selectedMarketplaceCategory = null;
    this.selectedMapping = null;
  }

  saveMappings(): void {
    // In real app, save to backend
    console.log('Saving mappings:', this.mappings);
    alert('Mapările au fost salvate cu succes!');
  }

  // AI Suggestions
  getAISuggestions(): void {
    this.showAISuggestions = true;
    this.aiSuggestions = [];
  }

  closeAISuggestions(): void {
    this.showAISuggestions = false;
  }

  applyAISuggestion(suggestion: AISuggestion): void {
    // Find categories and create mapping
    const feedCat = this.findCategoryByName(this.feedCategories, suggestion.feedCategory);
    const marketplaceCat = this.findCategoryByName(this.marketplaceCategories, suggestion.marketplaceCategory);
    
    if (feedCat && marketplaceCat) {
      this.selectedFeedCategory = feedCat;
      this.selectedMarketplaceCategory = marketplaceCat;
      this.createMapping();
    }
  }

  applyAllAISuggestions(): void {
    this.aiSuggestions.forEach(suggestion => {
      this.applyAISuggestion(suggestion);
    });
    this.closeAISuggestions();
  }

  findCategoryByName(categories: Category[], name: string): Category | null {
    for (const category of categories) {
      if (category.name === name) {
        return category;
      }
      if (category.children) {
        const found = this.findCategoryByName(category.children, name);
        if (found) return found;
      }
    }
    return null;
  }

  // Attribute Mapping
  updateAttributeMapping(attribute: ProductAttribute): void {
    if (this.selectedMapping) {
      if (!this.selectedMapping.attributeMappings) {
        this.selectedMapping.attributeMappings = {};
      }
      this.selectedMapping.attributeMappings[attribute.name] = attribute.mappedValue || '';
    }
  }

  // Utility methods
  getMarketplaceName(marketplaceId: string): string {
    const marketplace = this.marketplaces.find(m => m.id === marketplaceId);
    return marketplace ? marketplace.name : '';
  }

  getMappingStatusClass(status: string): string {
    switch (status) {
      case 'active': return 'status-success';
      case 'pending': return 'status-warning';
      case 'disabled': return 'status-error';
      default: return '';
    }
  }

  getMappingStatusLabel(status: string): string {
    switch (status) {
      case 'active': return 'Activ';
      case 'pending': return 'În așteptare';
      case 'disabled': return 'Dezactivat';
      default: return status;
    }
  }
} 