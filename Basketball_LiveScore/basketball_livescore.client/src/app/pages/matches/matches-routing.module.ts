import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CreateMatchComponent } from './create-match/create-match.component';
import { UpdateMatchComponent } from './update-match/update-match.component';
import { MatchViewerComponent } from './match-viewer/match-viewer.component';

const routes: Routes = [
  { path: 'create', component: CreateMatchComponent },
  { path: 'update', component: UpdateMatchComponent },
  { path: 'view/:id', component: MatchViewerComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MatchesRoutingModule { }
