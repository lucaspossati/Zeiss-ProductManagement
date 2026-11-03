import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { Product } from '../models/product.model';
import { ProductFormComponent } from './product-form.component';

const product: Product = {
  productIdentifier: 123456,
  name: 'ZEISS Lens',
  description: 'Precision lens',
  price: 99.95,
  availableStock: 12,
  createdAtUtc: '2026-01-01T00:00:00Z',
  updatedAtUtc: '2026-01-01T00:00:00Z'
};

describe('ProductFormComponent', () => {
  it('clears all fields when the create dialog is opened again', async () => {
    await TestBed.configureTestingModule({
      imports: [ProductFormComponent]
    }).compileComponents();

    const fixture = TestBed.createComponent(ProductFormComponent);
    const component = fixture.componentInstance;
    fixture.componentRef.setInput('visible', true);
    fixture.detectChanges();

    component.form.setValue({
      name: 'New product',
      description: 'A new product',
      price: 25,
      availableStock: 4
    });

    fixture.componentRef.setInput('visible', false);
    fixture.detectChanges();
    fixture.componentRef.setInput('visible', true);
    fixture.detectChanges();

    expect(component.form.getRawValue()).toEqual({
      name: '',
      description: '',
      price: null,
      availableStock: null
    });
    expect(component.form.pristine).toBe(true);
    expect(component.form.untouched).toBe(true);
  });

  it('populates the fields with the selected product when editing', async () => {
    await TestBed.configureTestingModule({
      imports: [ProductFormComponent]
    }).compileComponents();

    const fixture = TestBed.createComponent(ProductFormComponent);
    const component = fixture.componentInstance;
    fixture.componentRef.setInput('product', product);
    fixture.componentRef.setInput('visible', true);
    fixture.detectChanges();

    expect(component.form.getRawValue()).toEqual({
      name: product.name,
      description: product.description,
      price: product.price,
      availableStock: product.availableStock
    });
  });
});
