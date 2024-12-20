import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  //Players : 
  { path: 'players', loadChildren: () => import('./pages/players/players.module').then(m => m.PlayersModule) },
  //Teams :
  { path: 'teams', loadChildren: () => import('./pages/teams/teams.module').then(m => m.TeamsModule) },
  { path: '', redirectTo: '/players', pathMatch: 'full' }, //par défaut on est redirigé vers cette page
  { path: '**', redirectTo: '/players' }, //si chemin invalide, on est redirigé ici
 

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
