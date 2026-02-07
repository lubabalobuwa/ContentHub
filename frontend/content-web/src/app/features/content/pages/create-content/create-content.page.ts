import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ContentService } from '../../../../core/services/content.service';
import { UploadService } from '../../../../core/services/upload.service';
import { Content } from '../../../../core/models/content.model';

@Component({
  selector: 'app-create-content-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './create-content.page.html',
  styleUrl: './create-content.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateContentPage {
  title = '';
  body = '';
  selectedImage: File | null = null;

  isSubmitting = false;
  isUploading = false;
  error: string | null = null;

  constructor(
    private contentService: ContentService,
    private uploadService: UploadService,
    private router: Router
  ) {}

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    this.selectedImage = file ?? null;
  }

  submit() {
    this.error = null;

    if (!this.title.trim()) {
      this.error = 'Title is required.';
      return;
    }
    if (this.title.length > 200) {
      this.error = 'Title must be under 200 characters.';
      return;
    }
    if (!this.body.trim()) {
      this.error = 'Body is required.';
      return;
    }
    this.isSubmitting = true;

    this.contentService.create({
      title: this.title.trim(),
      body: this.body.trim()
    }).subscribe({
      next: async (response) => {
        if (this.selectedImage) {
          try {
            this.isUploading = true;
            const content = await this.fetchContent(response.id);
            if (!content?.rowVersion) {
              throw new Error('Missing row version for image upload.');
            }
            await this.uploadService.uploadContentImage(response.id, this.selectedImage, content.rowVersion);
          } catch {
            this.error = 'Content created, but image upload failed.';
          } finally {
            this.isUploading = false;
          }
        }
        this.isSubmitting = false;
        this.router.navigateByUrl('/drafts');
      },
      error: () => {
        this.isSubmitting = false;
        this.error = 'Failed to create content.';
      }
    });
  }

  private fetchContent(id: string): Promise<Content | null> {
    return new Promise(resolve => {
      this.contentService.getById(id).subscribe({
        next: content => resolve(content),
        error: () => resolve(null)
      });
    });
  }
}
