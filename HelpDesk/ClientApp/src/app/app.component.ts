import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ApiService } from './services/api.service';
import { SessionService } from './services/session.service';
import { NgIf } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './components/shared/header/header.component';
import { MenuComponent } from './components/shared/menu/menu.component';
import { ContentComponent } from './components/shared/content/content.component';
import { FooterComponent } from './components/shared/footer/footer.component';
import { LoginComponent } from './components/shared/login/login.component';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrls: ['./app.component.css'],
    imports: [NgIf, RouterOutlet, HeaderComponent, MenuComponent, ContentComponent, FooterComponent, LoginComponent]
})
export class AppComponent implements OnInit {
  constructor(public service: ApiService, private session: SessionService){ }

  // The tracking page of an order (/t/<token>) is open to the customer: no sign in, no menu.
  publicPage = location.pathname.startsWith('/t/');

  ngOnInit(){
    if (!this.publicPage) { this.session.start(); }
    // Any image that fails to load (corrupt data, missing file) shows the default photo instead of a broken icon.
    document.addEventListener('error', (event: any) => {
      const img = event.target;
      if (img && img.tagName === 'IMG' && !img.dataset.fallback) {
        img.dataset.fallback = '1';
        img.src = this.service.baseUrl + 'assets/dist/img/photo_default.png';
      }
    }, true);
  }
  
  title = this.service.aplicationName;
  

}
