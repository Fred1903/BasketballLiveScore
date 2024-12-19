import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { SharedModule } from './pages/shared/shared.module'
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavbarComponent } from './pages/shared/navbar/navbar.component';

@NgModule({
  declarations: [
    AppComponent,
    //NavbarComponent
  ],
  imports: [
    BrowserModule, HttpClientModule,
    AppRoutingModule,
    SharedModule //c'est mieux de déclarer le component là où il est et puis de l'importer que de le déclarer ici
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
