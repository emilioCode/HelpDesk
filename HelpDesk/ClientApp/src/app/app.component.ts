import { Component, OnInit } from '@angular/core';
import { ApiService } from './services/api.service';
import { SessionService } from './services/session.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  constructor(private service: ApiService, private session: SessionService){ }

  ngOnInit(){
    this.session.start();
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
