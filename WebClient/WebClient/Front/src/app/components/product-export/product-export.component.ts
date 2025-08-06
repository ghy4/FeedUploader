import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

interface Product {
  id: string;
  name: string;
  sku: string;
  category: string;
  mappedCategory?: string;
  price: number;
  stock: number;
  brand?: string;
  description?: string;
  image?: string;
  selected?: boolean;
  isMapped: boolean;
  exportStatus: 'ready' | 'validation_errors' | 'missing_mapping' | 'processing';
  validationErrors?: string[];
}

interface Marketplace {
  id: string;
  name: string;
}

interface ExportStats {
  total: number;
  success: number;
  warnings: number;
  errors: number;
}

interface ExportLogEntry {
  product: string;
  message: string;
  type: 'success' | 'warning' | 'error';
  timestamp: Date;
}

@Component({
  selector: 'app-product-export',
  templateUrl: './product-export.component.html',
  styleUrl: './product-export.component.css',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule
  ]
})
export class ProductExportComponent implements OnInit {
  marketplaces: Marketplace[] = [
    { id: 'emag', name: 'eMAG' },
    { id: 'altex', name: 'Altex' },
    { id: 'okazii', name: 'Okazii' },
    { id: 'olx', name: 'OLX' }
  ];

  products: Product[] = [];
  filteredProducts: Product[] = [];
  paginatedProducts: Product[] = [];
  selectedProducts: Product[] = [];
  categories: string[] = [];

  // Configuration
  selectedMarketplace = '';
  exportFormat = 'excel';
  exportType = 'full';
  includeImages = true;
  validateBeforeExport = true;

  // Filters
  searchTerm = '';
  categoryFilter = '';
  statusFilter = '';
  stockFilter = '';

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalPages = 1;
  allSelected = false;

  // Export process
  isExporting = false;
  isLoading = false;
  showExportModal = false;
  exportProgress = 0;
  exportStats: ExportStats = { total: 0, success: 0, warnings: 0, errors: 0 };
  exportCurrentItem = '';
  exportLog: ExportLogEntry[] = [];

  // Preview
  showPreviewModal = false;
  previewProduct: Product | null = null;

  ngOnInit(): void {
    this.loadMockData();
  }

  loadMockData(): void {
    // Initialize with empty products
    this.products = [];
    this.extractCategories();
    this.filterProducts();
  }

  extractCategories(): void {
    this.categories = [...new Set(this.products.map(p => p.category))];
  }

  onMarketplaceChange(): void {
    // In real app, update mapping status based on selected marketplace
    this.filterProducts();
  }

  filterProducts(): void {
    this.filteredProducts = this.products.filter(product => {
      // Search filter
      const matchesSearch = !this.searchTerm || 
        product.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        product.sku.toLowerCase().includes(this.searchTerm.toLowerCase());

      // Category filter
      const matchesCategory = !this.categoryFilter || 
        product.category === this.categoryFilter;

      // Status filter
      const matchesStatus = !this.statusFilter || 
        product.exportStatus === this.statusFilter;

      // Stock filter
      const matchesStock = this.matchesStockFilter(product.stock);

      return matchesSearch && matchesCategory && matchesStatus && matchesStock;
    });

    this.updatePagination();
  }

  matchesStockFilter(stock: number): boolean {
    if (!this.stockFilter) return true;
    
    switch (this.stockFilter) {
      case 'in_stock': return stock > 10;
      case 'low_stock': return stock > 0 && stock <= 10;
      case 'out_of_stock': return stock === 0;
      default: return true;
    }
  }

  updatePagination(): void {
    this.totalPages = Math.ceil(this.filteredProducts.length / this.pageSize);
    
    if (this.currentPage > this.totalPages) {
      this.currentPage = 1;
    }

    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.paginatedProducts = this.filteredProducts.slice(startIndex, endIndex);
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updatePagination();
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxPages = 5;
    
    let start = Math.max(1, this.currentPage - Math.floor(maxPages / 2));
    let end = Math.min(this.totalPages, start + maxPages - 1);
    
    if (end - start < maxPages - 1) {
      start = Math.max(1, end - maxPages + 1);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.categoryFilter = '';
    this.statusFilter = '';
    this.stockFilter = '';
    this.filterProducts();
  }

  onProductSelectionChange(product: Product): void {
    this.updateSelectedProducts();
  }

  toggleSelectAll(): void {
    this.allSelected = !this.allSelected;
    this.paginatedProducts.forEach(product => {
      product.selected = this.allSelected;
    });
    this.updateSelectedProducts();
  }

  updateSelectedProducts(): void {
    this.selectedProducts = this.products.filter(p => p.selected);
  }

  clearAllSelections(): void {
    this.products.forEach(product => {
      product.selected = false;
    });
    this.selectedProducts = [];
    this.allSelected = false;
  }

  refreshProducts(): void {
    this.isLoading = true;
    
    // Simulate refresh
    setTimeout(() => {
      this.loadMockData();
      this.isLoading = false;
    }, 1000);
  }

  // Export functionality
  startExport(): void {
    if (this.selectedProducts.length === 0) {
      alert('Selectează cel puțin un produs pentru export!');
      return;
    }

    if (!this.selectedMarketplace) {
      alert('Selectează marketplace-ul pentru export!');
      return;
    }

    if (this.validateBeforeExport) {
      const invalidProducts = this.selectedProducts.filter(p => 
        p.exportStatus === 'validation_errors' || p.exportStatus === 'missing_mapping'
      );
      
      if (invalidProducts.length > 0) {
        const proceed = confirm(`${invalidProducts.length} produse au erori de validare. Continui exportul doar cu produsele valide?`);
        if (!proceed) return;
      }
    }

    this.showExportModal = true;
    this.isExporting = true;
    this.exportProgress = 0;
    this.exportStats = { total: this.selectedProducts.length, success: 0, warnings: 0, errors: 0 };
    this.exportLog = [];

    this.processExport();
  }

  processExport(): void {
    const validProducts = this.selectedProducts.filter(p => p.exportStatus === 'ready');
    let processedCount = 0;

    const processNext = () => {
      if (processedCount < validProducts.length) {
        const product = validProducts[processedCount];
        this.exportCurrentItem = product.name;

        // Simulate processing time
        setTimeout(() => {
          const success = Math.random() > 0.1; // 90% success rate
          
          if (success) {
            this.exportStats.success++;
            this.exportLog.push({
              product: product.name,
              message: 'Export reușit',
              type: 'success',
              timestamp: new Date()
            });
          } else {
            this.exportStats.errors++;
            this.exportLog.push({
              product: product.name,
              message: 'Eroare la export: Conexiune marketplace întreruptă',
              type: 'error',
              timestamp: new Date()
            });
          }

          processedCount++;
          this.exportProgress = Math.round((processedCount / validProducts.length) * 100);
          
          processNext();
        }, 200 + Math.random() * 300);
      } else {
        this.isExporting = false;
        this.exportCurrentItem = '';
        
        // Add summary log entry
        this.exportLog.push({
          product: 'Export Summary',
          message: `Export finalizat: ${this.exportStats.success} succese, ${this.exportStats.errors} erori`,
          type: this.exportStats.errors > 0 ? 'warning' : 'success',
          timestamp: new Date()
        });
      }
    };

    processNext();
  }

  closeExportModal(): void {
    if (!this.isExporting) {
      this.showExportModal = false;
    }
  }

  downloadExportFile(): void {
    // Simulate file download
    const filename = `export_${this.selectedMarketplace}_${new Date().toISOString().split('T')[0]}.${this.exportFormat === 'excel' ? 'xlsx' : this.exportFormat}`;
    alert(`Fișierul ${filename} a fost descărcat cu succes!`);
  }

  // Product actions
  showPreview(product: Product): void {
    this.previewProduct = product;
    this.showPreviewModal = true;
  }

  closePreviewModal(): void {
    this.showPreviewModal = false;
    this.previewProduct = null;
  }

  validateProduct(product: Product): void {
    // Simulate validation
    product.validationErrors = [];
    
    if (!product.isMapped) {
      product.validationErrors.push('Categoria nu este mapată');
    }
    
    if (product.stock === 0) {
      product.validationErrors.push('Produsul nu este în stoc');
    }
    
    if (product.stock > 0 && product.stock < 5) {
      product.validationErrors.push('Stoc scăzut');
    }
    
    if (!product.description) {
      product.validationErrors.push('Lipsește descrierea');
    }

    if (product.validationErrors.length === 0) {
      product.exportStatus = 'ready';
      alert('Produsul a trecut validarea cu succes!');
    } else {
      product.exportStatus = 'validation_errors';
      alert(`Produsul are ${product.validationErrors.length} erori de validare.`);
    }
  }

  editProduct(product: Product): void {
    console.log('Edit product:', product);
    // In real app, open product edit modal
  }

  // Utility methods
  getStockClass(stock: number): string {
    if (stock === 0) return 'status-error';
    if (stock < 10) return 'status-warning';
    return 'status-success';
  }

  getExportStatusClass(status: string): string {
    switch (status) {
      case 'ready': return 'status-success';
      case 'validation_errors': return 'status-error';
      case 'missing_mapping': return 'status-warning';
      case 'processing': return 'status-info';
      default: return '';
    }
  }

  getExportStatusLabel(status: string): string {
    switch (status) {
      case 'ready': return 'Gata Export';
      case 'validation_errors': return 'Erori Validare';
      case 'missing_mapping': return 'Fără Mapare';
      case 'processing': return 'Se procesează';
      default: return status;
    }
  }

  getValidationErrors(): number {
    return this.products.filter(p => p.exportStatus === 'validation_errors').length;
  }

  getReadyForExport(): number {
    return this.products.filter(p => p.exportStatus === 'ready').length;
  }

  getMarketplaceName(marketplaceId: string): string {
    const marketplace = this.marketplaces.find(m => m.id === marketplaceId);
    return marketplace ? marketplace.name : '';
  }

  trackByProductId(index: number, product: Product): string {
    return product.id;
  }

  // Math utility for template
  Math = Math;
} 