import { Component, Inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { DOCUMENT, NgIf } from '@angular/common';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NgIf],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('content-web');
  theme: 'light' | 'dark' = 'light';

  constructor(
    public auth: AuthService,
    private router: Router,
    @Inject(DOCUMENT) private document: Document
  ) {
    const saved = localStorage.getItem('contenthub_theme');
    this.theme = saved === 'dark' ? 'dark' : 'light';
    this.applyTheme();
    this.auth.initialize();
  }
  isProfileMenuOpen = false;
  isWorkspaceMenuOpen = false;

  toggleProfileMenu() {
    this.isProfileMenuOpen = !this.isProfileMenuOpen;
    if (this.isProfileMenuOpen) this.isWorkspaceMenuOpen = false;
  }

  closeProfileMenu() {
    this.isProfileMenuOpen = false;
  }

  toggleWorkspaceMenu() {
    this.isWorkspaceMenuOpen = !this.isWorkspaceMenuOpen;
    if (this.isWorkspaceMenuOpen) this.isProfileMenuOpen = false;
  }

  closeWorkspaceMenu() {
    this.isWorkspaceMenuOpen = false;
  }

  logout() {
    this.closeProfileMenu();
    this.closeWorkspaceMenu();
    this.auth.logout();
    this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
      this.router.navigateByUrl('/');
    });
  }

  toggleTheme() {
    this.theme = this.theme === 'dark' ? 'light' : 'dark';
    localStorage.setItem('contenthub_theme', this.theme);
    this.applyTheme();
  }

  private applyTheme() {
    const root = this.document.documentElement;
    root.classList.toggle('theme-dark', this.theme === 'dark');
  }
}
