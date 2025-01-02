import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './pages/auth/login/login.component';
import { SignupComponent } from './pages/auth/signup/signup.component';

const routes: Routes = [
  //Auth :
  { path: 'login', component: LoginComponent },
  { path: 'signup', component: SignupComponent },
  //Lazy loading, permet de charger que quand on va sur leur pages
  //Contrairement aux imports dans app module qui seront tjrs importés
  //Players : 
  { path: 'players', loadChildren: () => import('./pages/players/players.module').then(m => m.PlayersModule) },
  //Teams :
  { path: 'teams', loadChildren: () => import('./pages/teams/teams.module').then(m => m.TeamsModule) },

  //Matchs
  { path: 'matches', loadChildren: () => import('./pages/matches/matches.module').then(m => m.MatchesModule) },
  //Encoders
  { path: 'encoders', loadChildren: () => import('./pages/encoders/encoders.module').then(m => m.EncodersModule) },
  //Par défaut
  { path: '', redirectTo: '/matches', pathMatch: 'full' }, //par défaut on est redirigé vers cette page
  { path: '**', redirectTo: '/matches' }, //si chemin invalide, on est redirigé ici
  
 

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
