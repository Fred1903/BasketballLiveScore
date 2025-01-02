import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { EncodersRoutingModule } from './encoders-routing.module';
import { EncodersComponent } from './encoders.component';


@NgModule({
  declarations: [
    EncodersComponent
  ],
  imports: [
    CommonModule,
    EncodersRoutingModule,
    ReactiveFormsModule  
  ]
})
export class EncodersModule { }
