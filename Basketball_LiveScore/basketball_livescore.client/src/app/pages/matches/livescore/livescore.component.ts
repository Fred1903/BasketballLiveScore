import { Component, OnInit } from '@angular/core';
import { MatchLiveScore } from '../../../models/interfaces';
import { MatchService } from '../../../services/match.service';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-livescore',
  templateUrl: './livescore.component.html',
  styleUrls: ['./livescore.component.css']
})
export class LivescoreComponent implements OnInit {
  matches: MatchLiveScore[] = [];
  loading = true;
  today: Date = new Date();
  isAdmin: boolean = false;

  constructor(
    private matchService: MatchService,
    private router: Router,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.loadMatches();
    this.isAdmin = this.authService.getRole() == 'Admin';

    this.matchService.subscribeToMatchStatusEvents((statusUpdate) => {
      this.updateMatchStatus(statusUpdate);
    });
    this.matchService.subscribeToQuarterUpdates((update) => {
      const match = this.matches.find(m => m.matchId === update.matchId);
      if (match) {
        match.currentQuarter = update.currentQuarter;
        console.log(`Match ${update.matchId} updated to Quarter ${update.currentQuarter}`);
      }
    });
  }

  updateMatchStatus(statusUpdate: { matchId: number; matchStatus: string }) {
    const match = this.matches.find(m => m.matchId === statusUpdate.matchId);
    if (match) {
      match.status = statusUpdate.matchStatus;
      console.log(`Match ${statusUpdate.matchId} status updated to ${statusUpdate.matchStatus}`);
    }
  }
  updateTimer(timerUpdate: { matchId: number; currentQuarter: number; remainingTime: number }) {
    const match = this.matches.find(m => m.matchId === timerUpdate.matchId);
    if (match) {
      match.currentQuarter = timerUpdate.currentQuarter;
      match.remainingTime = timerUpdate.remainingTime;
    }
  }

  canUpdateMatch(match: MatchLiveScore): boolean {
    if (!this.isAdmin) return false;
    const currentUserId = this.authService.getUserInfo()?.id;
    console.log("current user id : " + currentUserId + " and match encoder id ; " + match.encoderRealTimeId);
    return match.encoderRealTimeId === currentUserId;
  }

  viewMatch(matchId: number, event: Event) {
    event.stopPropagation(); // Prevent card click event
    this.router.navigate(['/matches', matchId]);
  }

  updateMatch(matchId: number, event: Event) {
    event.stopPropagation(); // Prevent card click event
    this.router.navigate(['/matches/update', matchId]);
  }


  loadMatches() {
    this.matchService.getAllMatches().subscribe({
      next: (data) => {
        this.matches = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading matches:', err);
        this.loading = false;
      }
    });
  }

  getSortedMatches(): MatchLiveScore[] {
    return [...this.matches].sort((a, b) => {
      // Define status priority
      const statusPriority: { [key: string]: number } = {
        'InProgress': 0,
        'NotStarted': 1,
        'Finished': 2
      };

      // First sort by status
      const statusDiff = statusPriority[a.status] - statusPriority[b.status];
      if (statusDiff !== 0) return statusDiff;

      // For same status, sort by date (newer first)
      return new Date(b.matchDate).getTime() - new Date(a.matchDate).getTime();
    });
  }

  getStatusDisplay(status: string): string {
    switch (status) {
      case 'NotStarted': return 'Starting Soon';
      case 'InProgress': return 'Live';
      case 'Finished': return 'Final';
      default: return status;
    }
  }

  navigateToMatch(matchId: number) {
    this.router.navigate(['/matches', matchId]);
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleString();
  }

  formatMatchTime(seconds: number): string {
    if (!seconds && seconds !== 0) return '';
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }
}
