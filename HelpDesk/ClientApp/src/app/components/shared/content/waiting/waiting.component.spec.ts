import { describe, expect, it } from 'vitest';
import { WaitingComponent } from './waiting.component';

// The state of an order as the list and the order window show it. Only the two pure functions are tried here, so the component
// is not built: they read the names of the states from the service and nothing else.
const states = { open: 'Abierto', inProgress: 'en Proceso', completed: 'Completado' };
const host: any = { service: { ticketStatuses: states } };
const badge = (item: any) => WaitingComponent.prototype.badge.call(host, item);
const progress = (item: any) => WaitingComponent.prototype.progress.call(host, item);

describe('WaitingComponent state of an order', () => {
  const open = { estado: 'Abierto', aprobadoPor: null };
  const inProgress = { estado: 'en Proceso', aprobadoPor: null };
  const completed = { estado: 'Completado', aprobadoPor: null };
  const approved = { estado: 'Completado', aprobadoPor: 9 };

  it('gives each state its word and tone', () => {
    expect(badge(open)).toEqual({ tone: 'open', label: 'Abierto' });
    expect(badge(inProgress)).toEqual({ tone: 'prog', label: 'En proceso' });
    expect(badge(completed)).toEqual({ tone: 'done', label: 'Completado' });
    expect(badge(approved)).toEqual({ tone: 'appr', label: 'Aprobado' });
  });

  it('counts four steps: 25, 50, 75 and 100', () => {
    expect([open, inProgress, completed, approved].map(progress)).toEqual([25, 50, 75, 100]);
  });

  it('treats an order that has no state yet as open', () => {
    expect(badge({ estado: '', aprobadoPor: null }).tone).toBe('open');
    expect(progress({ estado: undefined })).toBe(25);
  });

  it('an order is approved only when it is completed and somebody approved it', () => {
    expect(badge({ estado: 'en Proceso', aprobadoPor: 9 }).tone).toBe('prog');
  });
});
