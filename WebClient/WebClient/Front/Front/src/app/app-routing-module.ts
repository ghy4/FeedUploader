import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { FeedManagementComponent } from './components/feed-management/feed-management.component';
import { CategoryMappingComponent } from './components/category-mapping/category-mapping.component';
import { ProductExportComponent } from './components/product-export/product-export.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { AdminPanelComponent } from './components/admin-panel/admin-panel.component';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'feed', component: FeedManagementComponent },
  { path: 'mapping', component: CategoryMappingComponent },
  { path: 'export', component: ProductExportComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'admin', component: AdminPanelComponent },
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { } 