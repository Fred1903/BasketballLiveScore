import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CreateMatchComponent } from './create-match/create-match.component';
import { UpdateMatchComponent } from './update-match/update-match.component';
import { MatchViewerComponent } from './match-viewer/match-viewer.component';
import { LivescoreComponent } from './livescore/livescore.component';

const routes: Routes = [
  { path: 'create', component: CreateMatchComponent },
  { path: 'update/:id', component: UpdateMatchComponent },
  { path: 'all', component: LivescoreComponent },
  { path: ':id', component: MatchViewerComponent },
  //route avec param dynamique comme id doit etre derriere le all et route specifique
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MatchesRoutingModule { }
