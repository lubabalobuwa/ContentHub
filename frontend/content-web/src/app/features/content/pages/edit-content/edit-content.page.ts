import { Component, ChangeDetectionStrategy, ChangeDetectorRef, AfterViewInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { of, switchMap } from 'rxjs';
import { ContentService } from '../../../../core/services/content.service';
import { UploadService } from '../../../../core/services/upload.service';
import Quill from 'quill';
import Cropper from 'cropperjs';

@Component({
  selector: 'app-edit-content-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './edit-content.page.html',
  styleUrl: './edit-content.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditContentPage implements AfterViewInit, OnDestroy {
  @ViewChild('editor', { static: true }) editorRef!: ElementRef<HTMLDivElement>;
  @ViewChild('cropperImage') cropperImageRef?: ElementRef<HTMLImageElement>;

  title = '';
  body = '';
  rowVersion = '';
  private quill?: Quill;
  private pendingHtml: string | null = null;
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
      this.queueEditorHtml(this.body);
      this.cdr.markForCheck();
    });
  }

  ngAfterViewInit() {
    this.quill = new Quill(this.editorRef.nativeElement, {
      theme: 'snow',
      modules: {
        syntax: false,
        toolbar: [
          [{ header: [1, 2, 3, false] }],
          ['bold', 'italic', 'underline', 'strike'],
          [{ list: 'ordered' }, { list: 'bullet' }],
          ['blockquote', 'code-block'],
          ['link', 'image'],
          ['clean']
        ]
      }
    });

    this.quill.on('text-change', () => {
      this.body = this.quill?.root.innerHTML ?? '';
    });

    const initialHtml = this.pendingHtml ?? this.body;
    if (initialHtml) {
      this.setEditorHtml(initialHtml);
      this.pendingHtml = null;
    }
    this.ensureEditorContent();
  }

  ngOnDestroy() {
    this.quill = undefined;
    this.cleanupCropper();
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
    if (this.quill) {
      this.body = this.quill.root.innerHTML ?? this.body;
    } else {
      this.body = this.getEditorHtml() ?? this.body;
    }

    if (!this.title.trim()) {
      this.error = 'Title is required.';
      return;
    }
    if (this.title.length > 200) {
      this.error = 'Title must be under 200 characters.';
      return;
    }
    if (!this.getEditorText().trim()) {
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

  private setEditorHtml(html: string) {
    if (!this.quill) {
      this.pendingHtml = html;
      return;
    }

    const safeHtml = html || '';
    const delta = this.quill.clipboard.convert({ html: safeHtml });
    this.quill.setContents(delta, 'silent');
  }

  private queueEditorHtml(html: string) {
    this.pendingHtml = html;
    this.ensureEditorContent();
  }

  private ensureEditorContent() {
    if (!this.quill || !this.pendingHtml) return;
    const currentLength = this.quill.getLength();
    if (currentLength <= 1) {
      const html = this.pendingHtml;
      this.pendingHtml = null;
      this.quill.enable(false);
      setTimeout(() => {
        this.setEditorHtml(html);
        this.quill?.enable(true);
      }, 0);
    }
  }

  private getEditorText(): string {
    const quillText = this.quill?.getText() ?? '';
    if (quillText.trim()) return quillText.trim();
    const fallbackText = this.getEditorTextFromDom();
    if (fallbackText.trim()) return fallbackText.trim();
    return this.body.replace(/<[^>]*>/g, '').trim();
  }

  private getEditorTextFromDom(): string {
    const editor = this.editorRef?.nativeElement.querySelector('.ql-editor');
    return editor?.textContent ?? '';
  }

  private getEditorHtml(): string | null {
    const editor = this.editorRef?.nativeElement.querySelector('.ql-editor');
    return editor?.innerHTML ?? null;
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
