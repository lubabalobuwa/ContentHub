import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { catchError, of } from 'rxjs';
import { AuthService } from '../../../../core/services/auth.service';
import { UserProfile } from '../../../../core/models/user-profile.model';
import { UploadService } from '../../../../core/services/upload.service';

@Component({
  selector: 'app-account-settings-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './account-settings.page.html',
  styleUrl: './account-settings.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccountSettingsPage {
  profile$;
  selectedImage: File | null = null;
  isUploading = false;
  error: string | null = null;

  constructor(
    private auth: AuthService,
    private uploadService: UploadService,
    private cdr: ChangeDetectorRef
  ) {
    this.profile$ = this.loadProfile();
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    this.selectedImage = file ?? null;
  }

  async uploadProfileImage() {
    this.error = null;
    if (!this.selectedImage) {
      this.error = 'Select an image first.';
      this.cdr.markForCheck();
      return;
    }

    try {
      this.isUploading = true;
      await this.uploadService.uploadProfileImage(this.selectedImage);
      this.selectedImage = null;
      this.profile$ = this.loadProfile();
    } catch {
      this.error = 'Failed to upload profile image.';
    } finally {
      this.isUploading = false;
      this.cdr.markForCheck();
    }
  }

  private loadProfile() {
    return this.auth.me().pipe(
      catchError(() => of(null as UserProfile | null))
    );
  }
}
