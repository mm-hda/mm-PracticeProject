import { Injectable, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {

  private readonly translate = inject(TranslateService);

  private readonly STORAGE_KEY = 'app-language';

  constructor() {
    const savedLanguage = localStorage.getItem(this.STORAGE_KEY);

    if (savedLanguage) {
      this.translate.use(savedLanguage);
      return;
    }

    const browserLanguage = navigator.language;

    if (browserLanguage.startsWith('gu')) {
      this.translate.use('gu-IN');
    }
    else if (browserLanguage.startsWith('hi')) {
      this.translate.use('hi-IN');
    }
    else {
      this.translate.use('en-US');
    }
  }

  public changeLanguage(language: string): void {
    localStorage.setItem(this.STORAGE_KEY, language);
    this.translate.use(language);
  }

  public getCurrentLanguage(): string {
    return localStorage.getItem(this.STORAGE_KEY) || (navigator.language.startsWith('gu') ? 'gu-IN' :
      navigator.language.startsWith('hi') ? 'hi-IN' : 'en-US');
  }
}
