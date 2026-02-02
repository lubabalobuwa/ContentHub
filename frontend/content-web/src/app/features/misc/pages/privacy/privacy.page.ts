import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-privacy-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './privacy.page.html',
  styleUrl: './privacy.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PrivacyPage {}
