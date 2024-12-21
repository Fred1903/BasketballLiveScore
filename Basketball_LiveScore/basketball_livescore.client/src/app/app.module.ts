import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { SharedModule } from './pages/shared/shared.module'
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavbarComponent } from './pages/shared/navbar/navbar.component';
import { LoginComponent } from './pages/auth/login/login.component';
import { SignupComponent } from './pages/auth/signup/signup.component';


@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    SignupComponent,
    //NavbarComponent
  ],
  imports: [
    BrowserModule, HttpClientModule,
    AppRoutingModule,
    SharedModule, //c'est mieux de déclarer le component là où il est et puis de l'importer que de le déclarer ici
    ReactiveFormsModule, //si on déclare pas ca il va faire que des erreurs
    FormsModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
