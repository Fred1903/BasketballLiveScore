import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
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
  @ViewChild('loadingTemplate') loadingTemplate!: TemplateRef<any>;
  matches: MatchLiveScore[] = [];
  loading = true;
  selectedDate: Date = new Date();
  isAdmin: boolean = false;
  dateOptions: Date[] = [];
  liveMatches: MatchLiveScore[] = [];
  upcomingMatches: MatchLiveScore[] = [];
  finishedMatches: MatchLiveScore[] = [];

  constructor(
    private matchService: MatchService,
    private router: Router,
    private authService: AuthService
  ) { this.initializeDateOptions(); }

  ngOnInit() {
    console.log('Initial selected date:', this.selectedDate);
    this.loadMatches();
    this.isAdmin = this.authService.getRole() == 'Admin';

    this.matchService.subscribeToMatchStatusEvents((statusUpdate) => {
      this.updateMatchStatus(statusUpdate);
    });
    this.matchService.subscribeToQuarterUpdates((update) => {
      this.updateQuarter(update);
    });
  }

  initializeDateOptions() {
    for (let i = -3; i <= 3; i++) {
      const date = new Date();
      date.setDate(date.getDate() + i);
      date.setHours(0, 0, 0, 0);
      this.dateOptions.push(date);
    }
  }

  updateQuarter(update: { matchId: number; currentQuarter: number }) {
    const match = this.matches.find(m => m.matchId === update.matchId);
    if (match) {
      match.currentQuarter = update.currentQuarter;
    }
  }

  updateMatchStatus(statusUpdate: { matchId: number; matchStatus: string }) {
    const match = this.matches.find(m => m.matchId === statusUpdate.matchId);
    if (match) {
      match.status = statusUpdate.matchStatus;
    }
  }

  onDateChange(event: Event) {
    const dateStr = (event.target as HTMLSelectElement).value;
    this.selectedDate = new Date(dateStr);
  }
  onDateSelect(date: Date) {
    console.log('Selected date:', date);
    this.selectedDate = date;
    this.filterMatches();
  }

  previousDay() {
    const firstDate = this.dateOptions[0];
    const newDate = new Date(firstDate);
    newDate.setDate(newDate.getDate() - 1);
    this.dateOptions.unshift(newDate);
    this.dateOptions.pop();
  }

  nextDay() {
    const lastDate = this.dateOptions[this.dateOptions.length - 1];
    const newDate = new Date(lastDate);
    newDate.setDate(newDate.getDate() + 1);
    this.dateOptions.push(newDate);
    this.dateOptions.shift();
  }
  isSameDay(date1: Date, date2: Date): boolean {
    return date1.getFullYear() === date2.getFullYear() &&
      date1.getMonth() === date2.getMonth() &&
      date1.getDate() === date2.getDate();
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

  getMatchesByStatus(status: string): MatchLiveScore[] {
    const dateFiltered = this.matches.filter(match => {
      const matchDate = new Date(match.matchDate);
      matchDate.setHours(0, 0, 0, 0);
      const compareDate = new Date(this.selectedDate);
      compareDate.setHours(0, 0, 0, 0);
      return this.isSameDay(matchDate, compareDate);
    });

    const statusFiltered = dateFiltered.filter(match => match.status === status);

    return statusFiltered.sort((a, b) =>
      new Date(a.matchDate).getTime() - new Date(b.matchDate).getTime()
    );
  }

  loadMatches() {
    this.matchService.getAllMatches().subscribe({
      next: (data) => {
        this.matches = data;
        this.filterMatches();
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading matches:', err);
        this.loading = false;
      }
    });
  }

  filterMatches() {

    this.liveMatches = this.getMatchesByStatus('Live');
    this.upcomingMatches = this.getMatchesByStatus('NotStarted');
    this.finishedMatches = this.getMatchesByStatus('Finished');
  }

  getSortedMatches(): MatchLiveScore[] {
    return [...this.matches].sort((a, b) => {
      // Define status priority
      const statusPriority: { [key: string]: number } = {
        'Live': 0,
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
      case 'Live': return 'Live';
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
