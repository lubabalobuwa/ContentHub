import { Pipe, PipeTransform } from '@angular/core';

const WORDS_PER_MINUTE = 225;

@Pipe({
  name: 'readingTime',
  standalone: true
})
export class ReadingTimePipe implements PipeTransform {
  transform(value: string | null | undefined): number {
    if (!value?.trim()) return 1;

    const text = value
      .replace(/<[^>]*>/g, ' ')
      .replace(/[`*_>#\[\]()~-]/g, ' ')
      .replace(/\s+/g, ' ')
      .trim();

    if (!text) return 1;

    return Math.max(1, Math.ceil(text.split(' ').length / WORDS_PER_MINUTE));
  }
}
