import { Routes } from '@angular/router';

import { DashboardComponent } from './components/dashboard/dashboard.component';
import { FeedManagementComponent } from './components/feed-management/feed-management.component';
import { CategoryMappingComponent } from './components/category-mapping/category-mapping.component';
import { ProductExportComponent } from './components/product-export/product-export.component';
import { ExportLogComponent } from './components/export-log/export-log.component';
import { AdminPanelComponent } from './components/admin-panel/admin-panel.component';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'feed-management', component: FeedManagementComponent },
  { path: 'category-mapping', component: CategoryMappingComponent },
  { path: 'product-export', component: ProductExportComponent },
  { path: 'export-log', component: ExportLogComponent },
  { path: 'admin-panel', component: AdminPanelComponent },
  { path: '**', redirectTo: '/dashboard' }
];
