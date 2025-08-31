import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ProductService, Product } from '../../services/product';
import { FeedService } from '../../services/feed.service';
import { AuthService } from '../../services/auth.service';
import { ActivityService } from '../../services/activity.service';

type ExportStatus = 'ready' | 'validation_errors' | 'missing_mapping' | 'processing';

interface ExportProduct extends Product { mappedCategory?: string; selected?: boolean; isMapped: boolean; exportStatus: ExportStatus; validationErrors?: string[]; }
interface Marketplace { id: string; name: string; }
interface ExportStats { total: number; success: number; warnings: number; errors: number; }
interface ExportLogEntry { product: string; message: string; type: 'success' | 'warning' | 'error'; timestamp: Date; }

@Component({
  selector: 'app-product-export',
  templateUrl: './product-export.component.html',
  styleUrl: './product-export.component.css',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule]
})
export class ProductExportComponent implements OnInit {
  marketplaces: Marketplace[] = [
    { id: 'emag', name: 'eMAG' },
    { id: 'altex', name: 'Altex' },
    { id: 'okazii', name: 'Okazii' },
    { id: 'olx', name: 'OLX' }
  ];
  products: ExportProduct[] = [];
  filteredProducts: ExportProduct[] = [];
  paginatedProducts: ExportProduct[] = [];
  selectedProducts: ExportProduct[] = [];
  categories: string[] = [];
  selectedMarketplace = '';
  exportFormat = 'excel';
  exportType = 'full';
  includeImages = true;
  validateBeforeExport = true;
  searchTerm = '';
  categoryFilter = '';
  statusFilter: '' | ExportStatus = '';
  stockFilter = '';
  currentPage = 1;
  pageSize = 20;
  totalPages = 1;
  allSelected = false;
  isExporting = false;
  isLoading = false;
  showExportModal = false;
  exportProgress = 0;
  exportStats: ExportStats = { total: 0, success: 0, warnings: 0, errors: 0 };
  exportCurrentItem = '';
  exportLog: ExportLogEntry[] = [];
  showPreviewModal = false;
  previewProduct: ExportProduct | null = null;
  exportedFileBlob: Blob | null = null; // holds generated export file
  private exportProgressTimer: any;
  autoDownloadOnSuccess = true;
  constructor(
    private productService: ProductService,
    private authService: AuthService,
    private feedService: FeedService,
    private activity: ActivityService
  ) {}
  ngOnInit(): void { this.loadProducts(); }
  loadProducts(): void {
    this.isLoading = true;
    this.productService.getProducts().subscribe({
      next: (data: any) => {
        const list: Product[] = Array.isArray(data) ? data : (data?.$values ?? []);
        this.products = list.map(p => ({ ...p, selected: false, isMapped: !!p.category, exportStatus: p.category ? 'ready' : 'missing_mapping', validationErrors: [] }));
        this.extractCategories();
        this.filterProducts();
        this.isLoading = false;
      },
      error: err => { console.error('Load products error', err); this.products = []; this.extractCategories(); this.filterProducts(); this.isLoading = false; }
    });
  }
  extractCategories(): void {
    this.categories = [...new Set(this.products.map(p => p.category).filter(Boolean))];
  }

  onMarketplaceChange(): void {
    this.filterProducts();
  }

  filterProducts(): void {
    const term = this.searchTerm.toLowerCase();
    this.filteredProducts = this.products.filter(p => {
      const matchesSearch = !term ||
        p.name.toLowerCase().includes(term) ||
        (p.model?.toLowerCase().includes(term) ?? false) ||
        (p.manufacturer?.toLowerCase().includes(term) ?? false);
      const matchesCategory = !this.categoryFilter || p.category === this.categoryFilter;
      const matchesStatus = !this.statusFilter || p.exportStatus === this.statusFilter;
      const matchesStock = this.matchesStockFilter(p.quantity ?? 0);
      return matchesSearch && matchesCategory && matchesStatus && matchesStock;
    });
    this.updatePagination();
    this.updateSelectedProducts();
  }

  matchesStockFilter(q: number): boolean {
    if (!this.stockFilter) return true;
    switch (this.stockFilter) {
      case 'in_stock': return q > 10;
      case 'low_stock': return q > 0 && q <= 10;
      case 'out_of_stock': return q === 0;
      default: return true;
    }
  }

  updatePagination(): void {
    this.totalPages = Math.max(1, Math.ceil(this.filteredProducts.length / this.pageSize));
    if (this.currentPage > this.totalPages) this.currentPage = 1;
    const start = (this.currentPage - 1) * this.pageSize;
    this.paginatedProducts = this.filteredProducts.slice(start, start + this.pageSize);
    this.syncSelectAllState();
  }

  changePage(p: number): void {
    if (p >= 1 && p <= this.totalPages) {
      this.currentPage = p;
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
    this.statusFilter = '' as any;
    this.stockFilter = '';
    this.filterProducts();
  }

  onProductSelectionChange(_p?: ExportProduct): void {
    this.updateSelectedProducts();
  }

  toggleSelectAll(): void {
    this.allSelected = !this.allSelected;
    this.paginatedProducts.forEach(p => p.selected = this.allSelected);
    this.updateSelectedProducts();
  }

  updateSelectedProducts(): void {
    this.selectedProducts = this.products.filter(p => p.selected);
    this.syncSelectAllState();
  }

  syncSelectAllState(): void {
    const pageItems = this.paginatedProducts;
    this.allSelected = pageItems.length > 0 && pageItems.every(p => p.selected);
  }

  clearAllSelections(): void {
    this.products.forEach(p => p.selected = false);
    this.selectedProducts = [];
    this.allSelected = false;
  }

  refreshProducts(): void {
    this.loadProducts();
  }

  startExport(): void {
  console.log('[Export] Start export clicked');
    if (this.selectedProducts.length === 0) {
      alert('Selectează cel puțin un produs pentru export!');
      return;
    }
    if (!this.selectedMarketplace) {
      alert('Selectează marketplace-ul pentru export!');
      return;
    }
    if (this.validateBeforeExport) {
      const invalid = this.selectedProducts.filter(p => p.exportStatus === 'validation_errors' || p.exportStatus === 'missing_mapping');
      if (invalid.length && !confirm(`${invalid.length} produse invalide. Continui cu cele valide?`)) return;
    }
    this.showExportModal = true;
    this.isExporting = true;
    const valid = this.selectedProducts.filter(p => p.exportStatus === 'ready');
    if (valid.length === 0) {
      alert('Nu există produse valide pentru export.');
      this.isExporting = false;
      return;
    }
    this.exportStats = { total: valid.length, success: 0, warnings: 0, errors: 0 };
    this.exportProgress = 5; // initial progress
    this.exportLog = [];
    this.exportedFileBlob = null;
    const ids = valid.map(p => p.id);
  // Start a pulse timer to show progress while awaiting backend
  this.startProgressPulse();
    this.processExport(ids, valid);
  }

  // Performs real export via FeedController using FeedService
  processExport(productIds: number[], products: ExportProduct[]): void {
    this.exportCurrentItem = `Se exportă ${productIds.length} produse...`;
    // Only Excel supported currently by backend endpoint; ignore exportFormat mapping for now
    this.feedService.exportToExcel(productIds).subscribe({
      next: (blob: Blob) => {
  console.log('[Export] Response received. Blob size:', blob?.size);
        if (blob && blob.size > 0) {
          this.exportedFileBlob = blob;
          products.forEach(p => {
            this.exportStats.success++;
            this.exportLog.push({ product: p.name, message: 'Exportat cu succes', type: 'success', timestamp: new Date() });
          });
          this.exportProgress = 100;
          this.exportLog.push({ product: 'Sumar', message: `Export finalizat: ${this.exportStats.success} produse.`, type: 'success', timestamp: new Date() });
          this.activity.add({ type: 'export', description: `Export ${this.selectedMarketplace} (${this.exportStats.success} produse)`, status: 'success', date: new Date() });
          if (this.autoDownloadOnSuccess) {
            // defer to ensure modal renders progress 100 first
            setTimeout(() => this.downloadExportFile(), 300);
          }
        } else {
          this.exportStats.errors = productIds.length;
          this.exportProgress = 100;
          this.exportLog.push({ product: 'Eroare', message: 'Fișier gol sau invalid primit.', type: 'error', timestamp: new Date() });
          this.activity.add({ type: 'export', description: `Export eșuat (${productIds.length} produse)`, status: 'error', date: new Date() });
        }
        this.isExporting = false;
        this.exportCurrentItem = '';
  this.stopProgressPulse();
      },
      error: (err) => {
        console.error('Export error', err);
        this.exportStats.errors = productIds.length;
        this.exportProgress = 100;
        this.exportLog.push({ product: 'Eroare', message: 'Export eșuat: ' + (err?.message || 'necunoscut'), type: 'error', timestamp: new Date() });
        this.activity.add({ type: 'export', description: `Export eșuat (${productIds.length} produse)`, status: 'error', date: new Date() });
        this.isExporting = false;
        this.exportCurrentItem = '';
  this.stopProgressPulse();
      }
    });
  }

  closeExportModal(): void {
    if (!this.isExporting) {
      this.showExportModal = false;
    }
  }

  private startProgressPulse(): void {
    this.stopProgressPulse();
    this.exportProgressTimer = setInterval(() => {
      if (this.exportProgress < 90) {
        this.exportProgress += Math.max(1, Math.round((90 - this.exportProgress) * 0.1));
      }
    }, 400);
  }

  private stopProgressPulse(): void {
    if (this.exportProgressTimer) {
      clearInterval(this.exportProgressTimer);
      this.exportProgressTimer = null;
    }
  }

  downloadExportFile(): void {
    if (!this.exportedFileBlob) {
      alert('Niciun fișier disponibil.');
      return;
    }
    const extension = this.exportFormat === 'excel' ? 'xlsx' : this.exportFormat;
    const filename = `export_${this.selectedMarketplace}_${new Date().toISOString().split('T')[0]}.${extension}`;
    const url = URL.createObjectURL(this.exportedFileBlob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }

  showPreview(p: ExportProduct): void {
    this.previewProduct = p;
    this.showPreviewModal = true;
  }

  closePreviewModal(): void {
    this.showPreviewModal = false;
    this.previewProduct = null;
  }

  validateProduct(p: ExportProduct): void {
    p.validationErrors = [];
    if (!p.category) p.validationErrors.push('Categoria lipsește');
    const qty = p.quantity ?? 0;
    if (qty === 0) p.validationErrors.push('Stoc zero');
    if (qty > 0 && qty < 5) p.validationErrors.push('Stoc scăzut');
    if (!p.description) p.validationErrors.push('Descriere lipsă');
    p.exportStatus = p.validationErrors.length ? 'validation_errors' : 'ready';
  }

  editProduct(p: ExportProduct): void {
    console.log('Edit product', p);
  }

  getStockClass(q: number): string {
    if (q === 0) return 'status-error';
    if (q < 10) return 'status-warning';
    return 'status-success';
  }

  getExportStatusClass(s: ExportStatus): string {
    switch (s) {
      case 'ready': return 'status-success';
      case 'validation_errors': return 'status-error';
      case 'missing_mapping': return 'status-warning';
      case 'processing': return 'status-info';
      default: return '';
    }
  }

  getExportStatusLabel(s: ExportStatus): string {
    switch (s) {
      case 'ready': return 'Gata Export';
      case 'validation_errors': return 'Erori Validare';
      case 'missing_mapping': return 'Fără Mapare';
      case 'processing': return 'Se procesează';
      default: return s;
    }
  }

  getValidationErrors(): number {
    return this.products.filter(p => p.exportStatus === 'validation_errors').length;
  }

  getReadyForExport(): number {
    return this.products.filter(p => p.exportStatus === 'ready').length;
  }

  getMarketplaceName(id: string): string {
    return this.marketplaces.find(m => m.id === id)?.name || '';
  }

  trackByProductId(i: number, p: ExportProduct): number {
    return p.id;
  }
  Math = Math;
}