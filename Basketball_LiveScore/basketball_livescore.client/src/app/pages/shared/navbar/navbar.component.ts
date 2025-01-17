import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent implements OnInit {
  isAuthenticated = false;
  isEncoder = false;
  isAdmin = false;
  constructor(private authService: AuthService, private router: Router) { }

  ngOnInit(): void {
    //Subscribe au statut d authentification
    this.authService.isAuthenticated$.subscribe((status) => {
      this.isAuthenticated = status;

      if (status) {//si il est connecté on verif le role
        const role = this.authService.getRole();
        console.log("Role : " + role);
        this.isAdmin = role === 'Admin';
        this.isEncoder = role === 'Encoder';
        console.log("isAdmin ? " + this.isAdmin + " isEncoder : " + this.isEncoder)
      } else {
        this.isAdmin = false; // Réinitialise isAdmin si déconnecté
      }

    });
  }

  logout(): void {//Pas besoin de rediriger qql part qd pas login on a pas accès aux autres pages
    this.authService.logout();
  }

  navigateToMatches(): void {
    this.router.navigate(['/']);
  }
}
