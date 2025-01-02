import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EncodersComponent } from './encoders.component';

const routes: Routes = [
  { path: '', component: EncodersComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EncodersRoutingModule { }
