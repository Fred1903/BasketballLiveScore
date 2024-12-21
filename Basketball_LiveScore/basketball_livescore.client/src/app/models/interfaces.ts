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
