import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { App } from './app';

@NgModule({
  imports: [
    BrowserModule,
    HttpClientModule,
    App,
  ],
  bootstrap: [App],
})
export class AppModule { }
