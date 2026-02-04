import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { AdminService } from '../../../../core/services/admin.service';
import { ContentService } from '../../../../core/services/content.service';
import { AdminUser } from '../../../../core/models/admin-user.model';
import { PagedResponse } from '../../../../core/models/paged-response.model';
import { Content } from '../../../../core/models/content.model';
import { catchError, finalize, of, timeout } from 'rxjs';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin.page.html',
  styleUrl: './admin.page.scss'
})
export class AdminPage implements OnInit {
  private readonly requestTimeoutMs = 15000;
  loading = true;
  error = '';
  isAdmin = false;
  activeTab: 'users' | 'moderation' = 'users';

  users?: PagedResponse<AdminUser>;
  userPage = 1;
  userPageSize = 12;
  search = '';

  drafts?: PagedResponse<Content>;
  draftPage = 1;

  published?: PagedResponse<Content>;
  publishedPage = 1;

  archived?: PagedResponse<Content>;
  archivedPage = 1;

  pending: Record<string, string> = {};

  constructor(
    private auth: AuthService,
    private admin: AdminService,
    private content: ContentService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (!this.auth.isAuthenticated()) {
      this.loading = false;
      this.error = 'Please sign in to access admin tools.';
      return;
    }

    this.isAdmin = true;
    this.loadUsers();
    this.loadModeration();
  }

  setTab(tab: 'users' | 'moderation') {
    this.activeTab = tab;
  }

  loadUsers() {
    this.admin.getUsers(this.userPage, this.userPageSize, this.search)
      .pipe(
        timeout(this.requestTimeoutMs),
        catchError(() => {
          this.error = 'Unable to load users.';
          this.isAdmin = false;
          this.finishLoading();
          return of(this.emptyUsers(this.userPage, this.userPageSize));
        })
      )
      .subscribe(users => {
        this.users = users;
        this.finishLoading();
      });
  }

  searchUsers() {
    this.userPage = 1;
    this.loadUsers();
  }

  nextUserPage() {
    if (!this.users || this.userPage >= this.users.totalPages) return;
    this.userPage += 1;
    this.loadUsers();
  }

  previousUserPage() {
    if (this.userPage <= 1) return;
    this.userPage -= 1;
    this.loadUsers();
  }

  toggleUser(user: AdminUser) {
    const nextDisabled = !user.isDisabled;
    this.pending[user.id] = 'user';
    this.admin.setUserDisabled(user.id, nextDisabled)
      .pipe(
        timeout(this.requestTimeoutMs),
        finalize(() => {
          this.pending[user.id] = '';
        })
      )
      .subscribe({
        next: () => this.loadUsers(),
        error: () => {
          this.error = 'Unable to update user status.';
        }
      });
  }

  loadModeration() {
    this.content.getDrafts(this.draftPage, 20)
      .pipe(
        timeout(this.requestTimeoutMs),
        catchError(() => {
          this.error = 'Unable to load drafts.';
          this.isAdmin = false;
          this.finishLoading();
          return of(this.emptyContent(this.draftPage));
        })
      )
      .subscribe(data => {
        this.drafts = data;
        this.finishLoading();
      });

    this.content.getPublished(this.publishedPage, 20)
      .pipe(
        timeout(this.requestTimeoutMs),
        catchError(() => {
          this.error = 'Unable to load published content.';
          this.isAdmin = false;
          this.finishLoading();
          return of(this.emptyContent(this.publishedPage));
        })
      )
      .subscribe(data => {
        this.published = data;
        this.finishLoading();
      });

    this.content.getArchived(this.archivedPage, 20)
      .pipe(
        timeout(this.requestTimeoutMs),
        catchError(() => {
          this.error = 'Unable to load archived content.';
          this.isAdmin = false;
          this.finishLoading();
          return of(this.emptyContent(this.archivedPage));
        })
      )
      .subscribe(data => {
        this.archived = data;
        this.finishLoading();
      });
  }

  publish(item: Content) {
    if (!item.rowVersion) {
      this.error = 'Unable to publish content without a version token.';
      return;
    }
    this.pending[item.id] = 'publish';
    this.content.publish(item.id, item.rowVersion)
      .pipe(
        timeout(this.requestTimeoutMs),
        finalize(() => {
          this.pending[item.id] = '';
        })
      )
      .subscribe({
        next: () => this.loadModeration(),
        error: () => {
          this.error = 'Unable to publish content.';
        }
      });
  }

  archive(item: Content) {
    if (!item.rowVersion) {
      this.error = 'Unable to archive content without a version token.';
      return;
    }
    this.pending[item.id] = 'archive';
    this.content.archive(item.id, item.rowVersion)
      .pipe(
        timeout(this.requestTimeoutMs),
        finalize(() => {
          this.pending[item.id] = '';
        })
      )
      .subscribe({
        next: () => this.loadModeration(),
        error: () => {
          this.error = 'Unable to archive content.';
        }
      });
  }

  restore(item: Content) {
    if (!item.rowVersion) {
      this.error = 'Unable to restore content without a version token.';
      return;
    }
    this.pending[item.id] = 'restore';
    this.content.restore(item.id, item.rowVersion)
      .pipe(
        timeout(this.requestTimeoutMs),
        finalize(() => {
          this.pending[item.id] = '';
        })
      )
      .subscribe({
        next: () => this.loadModeration(),
        error: () => {
          this.error = 'Unable to restore content.';
        }
      });
  }

  isBusy(item: Content) {
    return !!this.pending[item.id];
  }

  getInitials(name?: string, email?: string) {
    const source = (name && name.trim().length > 0) ? name : (email ?? '');
    if (!source) return '?';
    const parts = source.trim().split(/\s+/).filter(Boolean);
    if (parts.length === 0) return '?';
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase();
  }

  private emptyUsers(page: number, pageSize: number): PagedResponse<AdminUser> {
    return { items: [], page, pageSize, totalCount: 0, totalPages: 0 };
  }

  private emptyContent(page: number): PagedResponse<Content> {
    return { items: [], page, pageSize: 20, totalCount: 0, totalPages: 0 };
  }

  private finishLoading() {
    this.loading = false;
    this.cdr.markForCheck();
  }
}
