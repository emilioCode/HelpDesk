import { enableProdMode } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';

import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';
import { environment } from './environments/environment';
import { installPageBehaviors } from './app/ui/page-behaviors';
import { ThemeService } from './app/ui/theme.service';

if (environment.production) {
  enableProdMode();
}

installPageBehaviors();

bootstrapApplication(AppComponent, appConfig)
  .then(app => app.injector.get(ThemeService).init())
  .catch(err => console.log(err));
