import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { FormControl } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { User } from '../../models/interfaces';

@Component({
  selector: 'app-encoders',
  templateUrl: './encoders.component.html',
  styleUrl: './encoders.component.css'
})
export class EncodersComponent implements OnInit {
  users: User[] = [];
  encoders: User[] = [];
  searchControl = new FormControl('');
  filteredUsers: User[] = [];
  filteredEncoders: User[] = [];

  constructor(private userService: UserService) { }

  ngOnInit(): void {
    this.loadUsers();
    this.loadEncoders();

    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(searchTerm => {
      this.filterUsers(searchTerm || '');
    });
  }

  loadUsers(): void {
    this.userService.getUsersByRole('User').subscribe(
      users => {
        console.log('Loaded users:', users);
        this.users = users;
        this.filteredUsers = users;
      },
      error => console.error('Error loading users:', error)
    );
  }

  loadEncoders(): void {
    this.userService.getUsersByRole('Encoder').subscribe(
      encoders => {
        console.log('Loaded encoder:', encoders);
        this.encoders = encoders;
        this.filteredEncoders = encoders;
      },
      error => console.error('Error loading encoders:', error)
    );
  }

  filterUsers(searchTerm: string): void {
    const term = searchTerm.toLowerCase();
    this.filteredUsers = this.users.filter(user =>
      user.username.toLowerCase().includes(term) ||
      user.firstName.toLowerCase().includes(term) ||
      user.lastName.toLowerCase().includes(term) ||
      user.email.toLowerCase().includes(term)
    );
    this.filteredEncoders = this.encoders.filter(encoder =>
      encoder.username.toLowerCase().includes(term) ||
      encoder.firstName.toLowerCase().includes(term) ||
      encoder.lastName.toLowerCase().includes(term) ||
      encoder.email.toLowerCase().includes(term)
    );
  }

  addEncoder(userId: string): void {//ON utilise des subscribe car on veut que le back-end ait fini de changer le role du user avant de reload la page
    this.userService.addUserAsEncoder(userId).subscribe({
      next: () => {
        // Appeler les méthodes de rechargement une fois que l'ajout est terminé
        this.loadUsers();
        this.loadEncoders();
      },
      error: (err) => {
        console.error("Error while adding encoder:", err);
      }
    });
  }

  removeEncoder(userId: string): void {
    this.userService.removeUserFromEncoders(userId).subscribe({
      next: () => {
        // Appeler les méthodes de rechargement une fois que la suppression est terminée
        this.loadUsers();
        this.loadEncoders();
      },
      error: (err) => {
        console.error("Error while removing encoder:", err);
      }
    });
  }

}
