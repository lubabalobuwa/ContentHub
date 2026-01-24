import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { ContentService } from '../../../../../app/core/services/content.service';
import { Content } from '../../../../core/models/content.model';

@Component({
  selector: 'app-content-list-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './content-list.page.html',
  styleUrl: './content-list.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentListPage {
  contents$!: Observable<Content[]>;

  constructor(private contentService: ContentService) {
    this.contents$ = this.contentService.getPublished();
  }
}
