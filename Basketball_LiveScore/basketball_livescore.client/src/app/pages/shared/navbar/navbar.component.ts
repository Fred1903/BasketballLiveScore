import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent implements OnInit {
  isAuthenticated = false;
  isAdmin = false;
  constructor(private authService: AuthService) { }

  ngOnInit(): void {
    //Subscribe au statut d authentification
    this.authService.isAuthenticated$.subscribe((status) => {
      this.isAuthenticated = status;

      if (status) {//si il est connecté on verif le role
        const role = this.authService.getRole();
        console.log("Role : " + role);
        this.isAdmin = role === 'Admin';
      } else {
        this.isAdmin = false; // Réinitialise isAdmin si déconnecté
      }

    });
  }

  logout(): void {//Pas besoin de rediriger qql part qd pas login on a pas accès aux autres pages
    this.authService.logout();
  }
}
