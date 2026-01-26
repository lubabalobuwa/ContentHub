import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { of, switchMap } from 'rxjs';
import { ContentService } from '../../../../core/services/content.service';
import { Content } from '../../../../core/models/content.model';

@Component({
  selector: 'app-edit-content-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './edit-content.page.html',
  styleUrl: './edit-content.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditContentPage {
  title = '';
  body = '';
  rowVersion = '';
  isSubmitting = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private contentService: ContentService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id');
        if (!id) return of(null);
        return this.contentService.getById(id);
      })
    ).subscribe(content => {
      if (!content) return;
      this.title = content.title;
      this.body = content.body;
      this.rowVersion = content.rowVersion ?? '';
      this.cdr.markForCheck();
    });
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
    if (!this.rowVersion) {
      this.error = 'RowVersion is required to update.';
      return;
    }

    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.isSubmitting = true;
    this.contentService.update(id, {
      title: this.title.trim(),
      body: this.body.trim(),
      rowVersion: this.rowVersion
    }).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.router.navigateByUrl(`/${id}`);
      },
      error: () => {
        this.isSubmitting = false;
        this.error = 'Failed to update content.';
      }
    });
  }
}
