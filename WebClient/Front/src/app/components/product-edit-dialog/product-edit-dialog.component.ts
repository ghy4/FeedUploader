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
    this.product = { ...data }; // Create a copy of the input product
  }

  ngOnInit(): void {
    this.initForm();
  }

  initForm(): void {
    this.productForm = this.fb.group({
      id: [this.product.id || 0], // Optional, read-only if needed
      name: [this.product.name || '', [Validators.required, Validators.minLength(2)]],
      description: [this.product.description || '', [Validators.required, Validators.minLength(10)]],
      model: [this.product.model || '', [Validators.required]],
      manufacturer: [this.product.manufacturer || '', [Validators.required]],
      category: [this.product.category || '', [Validators.required]],
      price: [this.product.price || 0, [Validators.required, Validators.min(0)]],
      salePrice: [this.product.salePrice || 0, [Validators.min(0)]],
      currency: [this.product.currency || '', [Validators.required]],
      quantity: [this.product.quantity || 0, [Validators.required, Validators.min(0)]],
      warranty: [this.product.warranty || 0, [Validators.required, Validators.min(0)]],
      mainImage: [this.product.mainImage || ''],
      additionalImage1: [this.product.additionalImage1 || ''],
      additionalImage2: [this.product.additionalImage2 || ''],
      additionalImage3: [this.product.additionalImage3 || ''],
      additionalImage4: [this.product.additionalImage4 || ''],
      type: [this.product.type || '', [Validators.required]]
      // temps is optional, no need for default unless required
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
}