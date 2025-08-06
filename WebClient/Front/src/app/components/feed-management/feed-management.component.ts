import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface Product {
  id: string;
  name: string;
  sku: string;
  category: string;
  price: number;
  oldPrice?: number;
  stock: number;
  status: 'active' | 'inactive' | 'pending';
  image?: string;
  selected?: boolean;
}

@Component({
  selector: 'app-feed-management',
  templateUrl: './feed-management.component.html',
  styleUrl: './feed-management.component.css',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ]
})
export class FeedManagementComponent implements OnInit {
  products: Product[] = [];
  filteredProducts: Product[] = [];
  paginatedProducts: Product[] = [];
  
  selectedFile: File | null = null;
  isDragOver = false;
  isLoading = false;
  uploadProgress = 0;
  
  // Search and filters
  searchTerm = '';
  selectedCategory = '';
  priceRange = '';
  categories: string[] = [];
  
  // Sorting
  sortField = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  
  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalPages = 1;
  
  // Selection
  allSelected = false;

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

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
    
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFileSelection(files[0]);
    }
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.handleFileSelection(file);
    }
  }

  handleFileSelection(file: File): void {
    if (!this.isValidFileType(file)) {
      alert('Tip de fișier invalid. Folosește CSV, Excel, XML sau JSON.');
      return;
    }

    if (file.size > 50 * 1024 * 1024) { // 50MB
      alert('Fișierul este prea mare. Dimensiunea maximă este 50MB.');
      return;
    }

    this.selectedFile = file;
    this.uploadProgress = 0;
    this.processFile();
  }

  isValidFileType(file: File): boolean {
    const validExtensions = ['.csv', '.xlsx', '.xls', '.xml', '.json'];
    return validExtensions.some(ext => file.name.toLowerCase().endsWith(ext));
  }

  processFile(): void {
    if (!this.selectedFile) return;

    this.isLoading = true;
    
    // Simulate file processing with progress
    const interval = setInterval(() => {
      this.uploadProgress += 10;
      
      if (this.uploadProgress >= 100) {
        clearInterval(interval);
        setTimeout(() => {
          this.isLoading = false;
          // In real app, parse the actual file and populate products
          // For now, keep products empty until real file is uploaded
        }, 1000);
      }
    }, 200);
  }

  removeFile(): void {
    this.selectedFile = null;
    this.uploadProgress = 0;
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  getFileType(filename: string): string {
    const extension = filename.split('.').pop()?.toUpperCase();
    switch (extension) {
      case 'CSV': return 'CSV';
      case 'XLSX': case 'XLS': return 'Excel';
      case 'XML': return 'XML';
      case 'JSON': return 'JSON';
      default: return 'Unknown';
    }
  }

  filterProducts(): void {
    this.filteredProducts = this.products.filter(product => {
      // Search filter
      const matchesSearch = !this.searchTerm || 
        product.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        product.sku.toLowerCase().includes(this.searchTerm.toLowerCase());

      // Category filter
      const matchesCategory = !this.selectedCategory || 
        product.category === this.selectedCategory;

      // Price range filter
      const matchesPrice = this.matchesPriceRange(product.price);

      return matchesSearch && matchesCategory && matchesPrice;
    });

    this.sortProducts();
    this.updatePagination();
  }

  matchesPriceRange(price: number): boolean {
    if (!this.priceRange) return true;
    
    switch (this.priceRange) {
      case '0-50': return price <= 50;
      case '50-100': return price > 50 && price <= 100;
      case '100-500': return price > 100 && price <= 500;
      case '500+': return price > 500;
      default: return true;
    }
  }

  sortBy(field: string): void {
    if (this.sortField === field) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = field;
      this.sortDirection = 'asc';
    }
    
    this.sortProducts();
    this.updatePagination();
  }

  sortProducts(): void {
    if (!this.sortField) return;

    this.filteredProducts.sort((a, b) => {
      let aValue = (a as any)[this.sortField];
      let bValue = (b as any)[this.sortField];

      if (typeof aValue === 'string') {
        aValue = aValue.toLowerCase();
        bValue = bValue.toLowerCase();
      }

      if (aValue < bValue) {
        return this.sortDirection === 'asc' ? -1 : 1;
      }
      if (aValue > bValue) {
        return this.sortDirection === 'asc' ? 1 : -1;
      }
      return 0;
    });
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
    this.selectedCategory = '';
    this.priceRange = '';
    this.filterProducts();
  }

  toggleSelectAll(): void {
    this.allSelected = !this.allSelected;
    this.paginatedProducts.forEach(product => {
      product.selected = this.allSelected;
    });
  }

  selectAllProducts(): void {
    this.allSelected = !this.allSelected;
    this.products.forEach(product => {
      product.selected = this.allSelected;
    });
  }

  editProduct(product: Product): void {
    console.log('Edit product:', product);
    // In real app, open edit modal
  }

  deleteProduct(productId: string): void {
    if (confirm('Ești sigur că vrei să ștergi acest produs?')) {
      this.products = this.products.filter(p => p.id !== productId);
      this.filterProducts();
    }
  }

  getStockClass(stock: number): string {
    if (stock === 0) return 'status-error';
    if (stock < 10) return 'status-warning';
    return 'status-success';
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'active': return 'status-success';
      case 'inactive': return 'status-error';
      case 'pending': return 'status-warning';
      default: return '';
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'active': return 'Activ';
      case 'inactive': return 'Inactiv';
      case 'pending': return 'În așteptare';
      default: return status;
    }
  }

  trackByProductId(index: number, product: Product): string {
    return product.id;
  }

  exportToExcel(): void {
    const selectedProducts = this.products.filter(p => p.selected);
    console.log('Export to Excel:', selectedProducts);
    // In real app, generate Excel file
  }

  clearData(): void {
    if (confirm('Ești sigur că vrei să ștergi toate produsele?')) {
      this.products = [];
      this.filteredProducts = [];
      this.paginatedProducts = [];
      this.selectedFile = null;
    }
  }

  openImportHistory(): void {
    console.log('Open import history');
    // In real app, show import history modal
  }

  // Math utility for template
  Math = Math;
} 