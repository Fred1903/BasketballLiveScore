import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../shared/shared.module';
import { PlayersRoutingModule } from './players-routing.module';
import { CreatePlayerComponent } from './create-player/create-player.component';


@NgModule({
  declarations: [
    CreatePlayerComponent
  ],
  imports: [
    CommonModule,
    PlayersRoutingModule,
    ReactiveFormsModule,
    SharedModule, 
  ]
})
export class PlayersModule { }
