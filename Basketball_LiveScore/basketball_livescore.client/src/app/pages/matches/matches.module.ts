import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatchesRoutingModule } from './matches-routing.module';
import { CreateMatchComponent } from './create-match/create-match.component';
import { SharedModule } from '../shared/shared.module';


@NgModule({
  declarations: [
    CreateMatchComponent
  ],
  imports: [
    CommonModule,
    MatchesRoutingModule,
    SharedModule, 
    ReactiveFormsModule,
  ]
})
export class MatchesModule { }
