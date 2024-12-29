import { NgModule } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatchesRoutingModule } from './matches-routing.module';
import { CreateMatchComponent } from './create-match/create-match.component';
import { SharedModule } from '../shared/shared.module';
import { UpdateMatchComponent } from './update-match/update-match.component';
import { MatchViewerComponent } from './match-viewer/match-viewer.component';
import { LivescoreComponent } from './livescore/livescore.component';

@NgModule({
  declarations: [
    CreateMatchComponent,
    UpdateMatchComponent,
    MatchViewerComponent,
    LivescoreComponent,
  ],
  imports: [
    CommonModule,
    MatchesRoutingModule,
    SharedModule, 
    ReactiveFormsModule,
    FormsModule
  ],
  providers: [
    DatePipe
  ]
})
export class MatchesModule { }
