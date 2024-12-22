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
