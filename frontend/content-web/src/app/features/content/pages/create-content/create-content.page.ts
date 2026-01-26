import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ContentService } from '../../../../core/services/content.service';

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

  isSubmitting = false;
  error: string | null = null;

  constructor(private contentService: ContentService, private router: Router) {}

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
      next: () => {
        this.isSubmitting = false;
        this.router.navigateByUrl('/');
      },
      error: () => {
        this.isSubmitting = false;
        this.error = 'Failed to create content.';
      }
    });
  }
}
