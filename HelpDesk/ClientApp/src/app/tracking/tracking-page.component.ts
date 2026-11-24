import { ChangeDetectionStrategy, Component, Inject, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Meta, Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { HdIconComponent } from '../ui/hd-icon.component';

// What the customer sees from the link of an order: where it stands and what has been done. No sign in; the link is the key.
@Component({
  selector: 'app-tracking-page',
  templateUrl: './tracking-page.component.html',
  styleUrls: ['./tracking-page.component.css'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [DatePipe, HdIconComponent]
})
export class TrackingPageComponent implements OnInit {
  state: 'loading' | 'ready' | 'invalid' = 'loading';
  view: any = null;
  readonly steps = ['Abierto', 'En proceso', 'Completado', 'Aprobado'];
  private readonly labels: any = { open: 'Abierto', inProgress: 'En proceso', completed: 'Completado', approved: 'Aprobado' };
  private readonly tones: any = { open: 'open', inProgress: 'prog', completed: 'done', approved: 'appr' };

  constructor(private route: ActivatedRoute, private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private title: Title, private meta: Meta) { }

  ngOnInit() {
    // The page is private to whoever has the link.
    this.meta.addTag({ name: 'robots', content: 'noindex, nofollow' });
    this.meta.addTag({ name: 'referrer', content: 'no-referrer' });
    const token = this.route.snapshot.paramMap.get('token') || '';
    this.http.get(this.baseUrl + 'api/Tracking/' + encodeURIComponent(token)).subscribe({
      next: (view: any) => {
        this.view = view;
        this.state = 'ready';
        this.title.setTitle('Orden No. ' + view.orderNumber + ' · ' + view.company);
      },
      error: () => {
        this.state = 'invalid';
        this.title.setTitle('Enlace no válido');
      }
    });
  }

  get statusLabel(): string { return this.labels[this.view.status]; }
  get tone(): string { return this.tones[this.view.status]; }
  get fillTone(): string { return this.view.status === 'approved' ? 'done' : 'accent'; }

  // done: the step is behind; current: where the order is now; pending: still to come.
  stepState(index: number): 'done' | 'current' | 'pending' {
    const step = index + 1;
    if (step < this.view.step || (step === this.view.step && this.view.step === 4)) { return 'done'; }
    return step === this.view.step ? 'current' : 'pending';
  }
}
