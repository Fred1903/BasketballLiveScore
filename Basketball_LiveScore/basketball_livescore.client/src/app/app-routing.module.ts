import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './pages/auth/login/login.component';
import { SignupComponent } from './pages/auth/signup/signup.component';

const routes: Routes = [
  //Auth :
  { path: 'login', component: LoginComponent },
  { path: 'signup', component: SignupComponent },
  //Players : 
  { path: 'players', loadChildren: () => import('./pages/players/players.module').then(m => m.PlayersModule) },
  //Teams :
  { path: 'teams', loadChildren: () => import('./pages/teams/teams.module').then(m => m.TeamsModule) },
  { path: '', redirectTo: '/login', pathMatch: 'full' }, //par défaut on est redirigé vers cette page
  { path: '**', redirectTo: '/login' }, //si chemin invalide, on est redirigé ici
 

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
