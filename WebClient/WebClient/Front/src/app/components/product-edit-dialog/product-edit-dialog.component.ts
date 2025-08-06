import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { Product } from '../../services/feed.service';

@Component({
  selector: 'app-product-edit-dialog',
  templateUrl: './product-edit-dialog.component.html',
  styleUrls: ['./product-edit-dialog.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule
  ]
})
export class ProductEditDialogComponent implements OnInit {
  productForm!: FormGroup;
  product: Product;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ProductEditDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Product
  ) {
    this.product = { ...data };
  }

  ngOnInit(): void {
    this.initForm();
  }

  initForm(): void {
    this.productForm = this.fb.group({
      name: [this.product.name, [Validators.required, Validators.minLength(2)]],
      sku: [this.product.sku, [Validators.required]],
      category: [this.product.category, [Validators.required]],
      brand: [this.product.brand, [Validators.required]],
      price: [this.product.price, [Validators.required, Validators.min(0)]],
      stock: [this.product.stock, [Validators.required, Validators.min(0)]],
      description: [this.product.description, [Validators.required, Validators.minLength(10)]]
    });
  }

  onSubmit(): void {
    if (this.productForm.valid) {
      const updatedProduct: Product = {
        ...this.product,
        ...this.productForm.value
      };
      this.dialogRef.close(updatedProduct);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  getErrorMessage(controlName: string): string {
    const control = this.productForm.get(controlName);
    if (control?.hasError('required')) {
      return 'Acest câmp este obligatoriu';
    }
    if (control?.hasError('minlength')) {
      return `Minim ${control.errors?.['minlength'].requiredLength} caractere`;
    }
    if (control?.hasError('min')) {
      return 'Valoarea trebuie să fie mai mare decât 0';
    }
    return '';
  }

  getAttributeEntries(): { key: string; value: string }[] {
    return Object.entries(this.product.attributes).map(([key, value]) => ({ key, value }));
  }

  hasAttributes(): boolean {
    return this.product.attributes && Object.keys(this.product.attributes).length > 0;
  }
} 