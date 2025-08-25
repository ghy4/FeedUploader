import { bootstrapApplication } from '@angular/platform-browser';
import { importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient } from '@angular/common/http';
import { App } from './app/app';

// Import routes
import { routes } from './app/app.routes';

// Import services
import { FeedService } from './app/services/feed.service';
import { CategoryService } from './app/services/category.service';
import { ExportService } from './app/services/export.service';
import { AdminService } from './app/services/admin.service';

bootstrapApplication(App, {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    importProvidersFrom(BrowserAnimationsModule)
  ]
}).catch(err => console.error(err));
