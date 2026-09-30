import { Component, ChangeDetectionStrategy, ChangeDetectorRef, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { of, switchMap } from 'rxjs';
import { ContentService } from '../../../../core/services/content.service';
import { UploadService } from '../../../../core/services/upload.service';
import { marked } from 'marked';
import Cropper from 'cropperjs';

@Component({
  selector: 'app-edit-content-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './edit-content.page.html',
  styleUrl: './edit-content.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditContentPage {
  @ViewChild('cropperImage') cropperImageRef?: ElementRef<HTMLImageElement>;

  title = '';
  body = '';
  rowVersion = '';
  isSubmitting = false;
  isUploading = false;
  selectedImage: File | null = null;
  croppedPreviewUrl: string | null = null;
  isCropping = false;
  private cropper?: Cropper;
  private cropSourceUrl: string | null = null;
  error: string | null = null;
  returnUrl = '/drafts';

  constructor(
    private route: ActivatedRoute,
    private contentService: ContentService,
    private uploadService: UploadService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {

    this.route.queryParamMap.subscribe(params => {
      const from = params.get('from');
      this.returnUrl = from === 'published' ? '/published' : '/drafts';
    });

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

  get markdownPreview(): string {
    return marked.parse(this.body || '', { breaks: true, gfm: true }) as string;
  }

  insertSyntax(before: string, after = '') {
    const selectionStart = this.body.length;
    const insertion = `${before}${after}`;
    this.body = `${this.body.slice(0, selectionStart)}${insertion}${this.body.slice(selectionStart)}`;
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      this.selectedImage = null;
      this.croppedPreviewUrl = null;
      return;
    }

    this.startCrop(file);
  }

  async uploadImage() {
    this.error = null;

    if (!this.selectedImage) {
      this.error = 'Select an image first.';
      return;
    }

    if (!this.rowVersion) {
      this.error = 'RowVersion is required to update image.';
      return;
    }

    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    try {
      this.isUploading = true;
      await this.uploadService.uploadContentImage(id, this.selectedImage, this.rowVersion);
      this.contentService.getById(id).subscribe({
        next: content => {
          this.rowVersion = content.rowVersion ?? this.rowVersion;
          this.selectedImage = null;
          this.cdr.markForCheck();
        },
        error: () => {
          this.error = 'Image uploaded, but failed to refresh content.';
        }
      });
    } catch {
      this.error = 'Failed to upload image.';
    } finally {
      this.isUploading = false;
      this.cdr.markForCheck();
    }
  }

  submit() {
    this.error = null;
    this.body = this.body.trim();

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
        this.router.navigateByUrl(this.returnUrl);
      },
      error: () => {
        this.isSubmitting = false;
        this.error = 'Failed to update content.';
      }
    });
  }

  private startCrop(file: File) {
    this.cleanupCropper();
    this.cropSourceUrl = URL.createObjectURL(file);
    this.isCropping = true;
    this.cdr.markForCheck();

    setTimeout(() => {
      const image = this.cropperImageRef?.nativeElement;
      if (!image || !this.cropSourceUrl) return;
      image.src = this.cropSourceUrl;
      this.cropper = new Cropper(image, {
        aspectRatio: 16 / 9,
        viewMode: 1,
        autoCropArea: 1,
        responsive: true
      });
    }, 0);
  }

  cancelCrop() {
    this.cleanupCropper();
    this.selectedImage = null;
    this.croppedPreviewUrl = null;
    this.isCropping = false;
    this.cdr.markForCheck();
  }

  applyCrop() {
    if (!this.cropper) return;
    const canvas = this.cropper.getCroppedCanvas();
    canvas.toBlob(blob => {
      if (!blob) return;
      const file = new File([blob], `cover-${Date.now()}.jpg`, { type: 'image/jpeg' });
      this.selectedImage = file;
      this.croppedPreviewUrl = URL.createObjectURL(blob);
      this.isCropping = false;
      this.cleanupCropper();
      this.cdr.markForCheck();
    }, 'image/jpeg', 0.92);
  }

  private cleanupCropper() {
    this.cropper?.destroy();
    this.cropper = undefined;
    if (this.cropSourceUrl) {
      URL.revokeObjectURL(this.cropSourceUrl);
      this.cropSourceUrl = null;
    }
  }
}
