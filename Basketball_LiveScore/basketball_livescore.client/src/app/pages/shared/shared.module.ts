import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { NavbarComponent } from './navbar/navbar.component';
import { FormInputComponent } from './form-input/form-input.component';
import { FormDropdownComponent } from './form-dropdown/form-dropdown.component';

@NgModule({
  declarations: [
    NavbarComponent,
    FormInputComponent,
    FormDropdownComponent
  ],
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatMenuModule,
    MatIconModule,
    RouterModule,
    ReactiveFormsModule,
  ],
  exports: [
    NavbarComponent, // Export so it can be used in other modules
    FormInputComponent,
    FormDropdownComponent,
  ]
})
export class SharedModule { }
