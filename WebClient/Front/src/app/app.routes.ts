import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { FeedManagementComponent } from './components/feed-management/feed-management.component';
import { CategoryMappingComponent } from './components/category-mapping/category-mapping.component';
import { ProductExportComponent } from './components/product-export/product-export.component';
import { ExportLogComponent } from './components/export-log/export-log.component';
import { AdminPanelComponent } from './components/admin-panel/admin-panel.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { AuthGuard } from './auth.guard';
import { AdminGuard } from './admin.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'feed-management', component: FeedManagementComponent, canActivate: [AuthGuard] },
  { path: 'category-mapping', component: CategoryMappingComponent, canActivate: [AuthGuard] },
  { path: 'product-export', component: ProductExportComponent, canActivate: [AuthGuard] },
  { path: 'export-log', component: ExportLogComponent, canActivate: [AuthGuard] },
  { path: 'admin-panel', component: AdminPanelComponent, canActivate: [AuthGuard, AdminGuard] },
  { path: '', redirectTo: '/login', pathMatch: 'full' }, // Default to login for unauthenticated users
  { path: '**', redirectTo: '/login' } // Wildcard to redirect invalid paths to login
];

// Export the router provider configuration
import { provideRouter } from '@angular/router';

export const appRoutingProviders = [
  provideRouter(routes)
];