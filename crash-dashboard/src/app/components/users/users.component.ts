import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';

interface AppUser { id: number; username: string; }

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent implements OnInit {
  users: AppUser[] = [];
  newUsername = '';
  newPassword = '';
  formError = '';
  formSuccess = '';
  submitting = false;
  deletingId: number | null = null;

  constructor(
    private http: HttpClient,
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() { this.load(); }

  load() {
    this.http.get<AppUser[]>('/api/users').subscribe({
      next: users => { this.users = users; this.cdr.detectChanges(); },
      error: () => { this.cdr.detectChanges(); }
    });
  }

  addUser() {
    if (!this.newUsername.trim() || !this.newPassword.trim()) return;
    this.submitting = true;
    this.formError = '';
    this.formSuccess = '';

    this.http.post('/api/users', {
      username: this.newUsername.trim(),
      password: this.newPassword
    }).subscribe({
      next: () => {
        this.formSuccess = `User "${this.newUsername}" created.`;
        this.newUsername = '';
        this.newPassword = '';
        this.submitting = false;
        this.cdr.detectChanges();
        this.load();
      },
      error: (err) => {
        this.formError = err.error?.message ?? 'Failed to create user.';
        this.submitting = false;
        this.cdr.detectChanges();
      }
    });
  }

  deleteUser(user: AppUser) {
    if (!confirm(`Delete user "${user.username}"?`)) return;
    this.deletingId = user.id;
    this.http.delete(`/api/users/${user.id}`).subscribe({
      next: () => { this.deletingId = null; this.load(); },
      error: (err) => {
        alert(err.error?.message ?? 'Failed to delete user.');
        this.deletingId = null;
        this.cdr.detectChanges();
      }
    });
  }

  logout() { this.auth.logout(); }
}
