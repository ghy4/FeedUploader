
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { Product, ProductService } from '../../services/product';
import { FeedService } from '../../services/feed.service';

@Component({
  selector: 'app-feed-management',
  templateUrl: './feed-management.component.html',
  styleUrl: './feed-management.component.css',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    HttpClientModule
  ]
})
export class FeedManagementComponent implements OnInit {
  products: (Product & { selected?: boolean })[] = [];
  filteredProducts: (Product & { selected?: boolean })[] = [];
  paginatedProducts: (Product & { selected?: boolean })[] = [];

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

  // Inline edit state
  editingProductId: number | null = null;
  editBuffer: (Product & { selected?: boolean }) | null = null;
  savingEdit = false;
  deletingIds = new Set<number>();

  constructor(
    private productService: ProductService,
    private feedService: FeedService
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.productService.getProducts().subscribe({
      next: (products: any) => {
        // Support possible $values wrapper if backend used Preserve previously
        const list: any[] = Array.isArray(products) ? products : (products?.$values ?? []);
        this.products = list.map(p => ({ ...p, selected: false }));
        this.extractCategories();
        this.filterProducts();
      },
      error: (err) => {
        console.error('Error loading products', err);
        this.products = [];
        this.extractCategories();
        this.filterProducts();
      }
    });
  }

  extractCategories(): void {
    this.categories = [...new Set(this.products.map(p => p.category))].filter(Boolean);
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
    this.feedService.uploadFile(this.selectedFile).subscribe({
      next: (result) => {
        this.uploadProgress = result.progress;
        if (result.products) {
          // Normalize potential $values wrapper
          const incomingRaw: any = result.products as any;
          const incoming: any[] = Array.isArray(incomingRaw) ? incomingRaw : (incomingRaw?.$values ?? []);
          if (!Array.isArray(incoming)) {
            console.warn('Upload response products not an array', incomingRaw);
            return;
          }
          const existingMap = new Map(this.products.map(p => [p.id, p]));
          for (const prod of incoming) {
            if (existingMap.has(prod.id)) {
              Object.assign(existingMap.get(prod.id)!, prod, { selected: false });
            } else {
              existingMap.set(prod.id, { ...prod, selected: false });
            }
          }
          this.products = Array.from(existingMap.values());
          this.extractCategories();
          this.filterProducts();
        }
        if (result.progress === 100) {
          this.isLoading = false;
        }
      },
      error: (err) => {
        this.isLoading = false;
        alert('Eroare la încărcarea fișierului!');
        console.error('Upload error:', err);
      }
    });
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
        (product.model?.toLowerCase().includes(this.searchTerm.toLowerCase()) ?? false) ||
        (product.manufacturer?.toLowerCase().includes(this.searchTerm.toLowerCase()) ?? false);

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

  // Handle null / undefined gracefully
  if (aValue == null && bValue == null) return 0;
  if (aValue == null) return this.sortDirection === 'asc' ? 1 : -1;
  if (bValue == null) return this.sortDirection === 'asc' ? -1 : 1;

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

  editProduct(product: Product & { selected?: boolean }): void {
    // Backward compatibility method name - delegate to startEdit
    this.startEdit(product);
  }

  startEdit(product: Product & { selected?: boolean }): void {
    if (this.savingEdit) return;
    this.editingProductId = product.id;
    // Shallow clone is enough (all primitives)
    this.editBuffer = { ...product };
  }

  isEditing(product: Product): boolean {
    return this.editingProductId === product.id;
  }

  cancelEdit(): void {
    this.editingProductId = null;
    this.editBuffer = null;
    this.savingEdit = false;
  }

  saveEdit(): void {
    if (!this.editBuffer || this.editingProductId == null) return;
    this.savingEdit = true;
    const payload: Product = { ...this.editBuffer } as Product; // ensure Product shape
    this.productService.updateProduct(payload.id, payload).subscribe({
      next: () => {
        // Update local list manually (API returns no content)
        const idx = this.products.findIndex(p => p.id === payload.id);
        if (idx !== -1) {
          const selected = this.products[idx].selected;
          this.products[idx] = { ...payload, selected };
        }
        this.filterProducts();
        this.cancelEdit();
      },
      error: (err) => {
        console.error('Update error', err);
        alert('Eroare la salvarea produsului.');
        this.savingEdit = false;
      }
    });
  }

  deleteProduct(productId: number): void {
    if (this.deletingIds.has(productId)) return;
    if (!confirm('Ești sigur că vrei să ștergi acest produs?')) return;
    this.deletingIds.add(productId);
    this.productService.deleteProduct(productId).subscribe({
      next: () => {
        this.products = this.products.filter(p => p.id !== productId);
        this.filterProducts();
        if (this.editingProductId === productId) this.cancelEdit();
        this.deletingIds.delete(productId);
      },
      error: (err) => {
        console.error('Delete error:', err);
        alert('Eroare la ștergerea produsului!');
        this.deletingIds.delete(productId);
      }
    });
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

  trackByProductId(index: number, product: Product): number {
    return product.id;
  }

  exportToExcel(): void {
    const selectedIds = this.products.filter(p => p.selected).map(p => p.id);
    if (selectedIds.length === 0) {
      alert('Selectează cel puțin un produs pentru export!');
      return;
    }
    this.feedService.exportToExcel(selectedIds).subscribe({
      next: (blob) => {
        // Download the file
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'export.xlsx';
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        alert('Eroare la export!');
        console.error('Export error:', err);
      }
    });
  }

  clearData(): void {
    if (confirm('Ești sigur că vrei să ștergi toate produsele?')) {
      this.feedService.clearDatabase().subscribe({
        next: (success) => {
          if (success) {
            this.products = [];
            this.filteredProducts = [];
            this.paginatedProducts = [];
            this.selectedFile = null;
            this.uploadProgress = 0;
            this.allSelected = false;
          } else {
            alert('Eroare la ștergerea bazei de date!');
          }
        },
        error: (err) => {
          alert('Eroare la ștergerea bazei de date!');
          console.error('Clear DB error:', err);
        }
      });
    }
  }

  openImportHistory(): void {
    console.log('Open import history');
    // In real app, show import history modal
  }

  // Math utility for template
  Math = Math;
} 