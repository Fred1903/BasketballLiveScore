export interface UserLogin {
  username: string;
  password: string;
}

export interface Player {
  firstName: string;
  lastName: string;
  number: number;
  height: number;
  position: string;
  team: string;
}

export interface Team {
  name: string;
  coach: string;
}

export interface Position {
  value: string;
  display: string;
}

export interface UserRegister {
  email: string,
  password: string,
  username: string,
  firstName: string,
  lastName: string
}

export interface PlayerMatch {
  id: number;
  number: number;
  name: string;
  fouls: number;
  points: number;
  onCourt: boolean;
}

export interface Team {
  name: string;
  score: number;
  players: PlayerMatch[];
  timeouts: number;
  coach: string;
}

export interface SubstitutionEvent {
  teamId: number;
  playerOutId: number;
  playerInId: number;
  timestamp: Date;
}

export interface ScoreEvent {
  teamId: number;
  playerId: number;
  points: number;
  timestamp: Date;
}

export interface FoulEvent {
  teamId: number;
  playerId: number;
  timestamp: Date;
}

export interface TimeoutEvent {
  teamId: number;
  timestamp: Date;
}

export interface MatchState {
  quarter: number;
  timeRemaining: number;
  isRunning: boolean;
  timeoutRemaining: number;
  isTimeout: boolean;
  team1: Team;
  team2: Team;
}

export interface MatchDetails {
  matchId: number;
  matchDate: string;
  homeTeam: {
    teamId: number;
    name: string;
    coach: string;
    score: number;
    startingFive: any[]; 
  };
  awayTeam: {
    teamId: number;
    name: string;
    coach: string;
    score: number;
    startingFive: any[];
  };
  quarterDuration: number;
  numberOfQuarters: number;
  timeouts: number;
  timeoutDuration: number;
  homeTeamRemainingTimeouts: number;
  awayTeamRemainingTimeouts: number;
  scoreHome: number;
  scoreAway: number;
  players: {
    playerId: number;
    playerName: string;
    playerNumber: number;
    isStarter: boolean;
    isHomeTeam: boolean;
    fouls: number;
    points: number;
    onField: boolean; 
  }[];
  currentQuarter: number;
  currentTime: number;
  isRunning: boolean;
  matchStatus: string;
}

export interface MatchEvent {
  matchId: number;
  playerId?: number; // Optionnel pour certains types d'événements
  quarter: number;
  time: string; // Temps en secondes ou un format spécifique
}

export interface BasketEvent extends MatchEvent {
  points: number; // Points marqués (1, 2 ou 3)
}

export interface FoulEvent extends MatchEvent {
  foulType: string; 
}

export interface SubstitutionEvent extends MatchEvent {
  playerInId: number; 
  playerOutId: number; 
}

export interface TimeoutEvent extends MatchEvent {
  team: string; //'équipe qui a demandée le timeout
}

export interface QuarterChangeEvent extends MatchEvent {
  // Aucun champ supplémentaire
}

export interface ChronoEvent extends MatchEvent {
  isRunning: boolean; 
}

export interface ViewerMatchState {
  matchStatus: 'not_started' | 'in_progress' | 'timeout' | 'quarter_break' | 'finished';
  quarter: number;
  time: number;
  isTimeout: boolean;
  timeoutTime: number;
}

export interface RecentEvent {
  time: string;
  description: string;
  type: 'score' | 'foul' | 'timeout' | 'substitution';
}


export interface MatchLiveScore {
  matchId: number;
  matchDate: string;
  homeTeam: {
    teamId: number;
    name: string;
    coach: string;
  };
  awayTeam: {
    teamId: number;
    name: string;
    coach: string;
  };
  scoreHome: number;
  scoreAway: number;
  status: string;
  encoderRealTimeId: string;
  remainingTime: number;
  currentQuarter: number;
}
