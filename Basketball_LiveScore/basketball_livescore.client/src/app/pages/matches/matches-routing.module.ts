import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CreateMatchComponent } from './create-match/create-match.component';
import { UpdateMatchComponent } from './update-match/update-match.component';

const routes: Routes = [
  { path: 'create', component: CreateMatchComponent },
  { path: 'update', component: UpdateMatchComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MatchesRoutingModule { }
