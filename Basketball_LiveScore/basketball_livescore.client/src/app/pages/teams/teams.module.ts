import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { TeamsRoutingModule } from './teams-routing.module';
import { CreateTeamComponent } from './create-team/create-team.component';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [
    CreateTeamComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TeamsRoutingModule,
    SharedModule,
  ]
})
export class TeamsModule { }
