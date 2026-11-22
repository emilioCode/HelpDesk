import { ChangeDetectionStrategy, Component, ElementRef, Input, OnChanges } from '@angular/core';
import { ICONS } from './icons';

// An icon of the Lucide set (ISC license), drawn as an inline SVG that takes the color of the text around it.
@Component({
  selector: 'hd-icon',
  template: '',
  changeDetection: ChangeDetectionStrategy.Eager,
  host: { 'aria-hidden': 'true', style: 'display: inline-flex; line-height: 0;' }
})
export class HdIconComponent implements OnChanges {
  @Input() name = '';
  @Input() size = 18;

  constructor(private host: ElementRef<HTMLElement>) { }

  ngOnChanges() {
    const ns = 'http://www.w3.org/2000/svg';
    const el = this.host.nativeElement;
    el.textContent = '';
    const node = ICONS[this.name];
    if (!node) { return; }
    const svg = document.createElementNS(ns, 'svg');
    const set = { width: this.size, height: this.size, viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': 1.8, 'stroke-linecap': 'round', 'stroke-linejoin': 'round' };
    Object.entries(set).forEach(([k, v]) => svg.setAttribute(k, String(v)));
    for (const [tag, attrs] of node) {
      const child = document.createElementNS(ns, tag);
      Object.entries(attrs).forEach(([k, v]) => child.setAttribute(k, String(v)));
      svg.appendChild(child);
    }
    el.appendChild(svg);
  }
}
