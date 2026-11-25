import { Routes } from '@angular/router';

import { AuthGuard } from './services/auth.guard';
import { HomeComponent } from './home/home.component';
import { EmpresaComponent } from './components/shared/content/empresa/empresa.component';
import { UserComponent } from './components/shared/content/user/user.component';
import { ProfileComponent } from './components/shared/content/profile/profile.component';
import { CostumerComponent } from './components/shared/content/costumer/costumer.component';
import { WaitingComponent } from './components/shared/content/waiting/waiting.component';
import { ReportComponent } from './components/shared/content/report/report.component';
import { TrackingPageComponent } from './tracking/tracking-page.component';
import { CalendarComponent } from './components/shared/content/calendar/calendar.component';

export const routes: Routes = [
  // The customer's page: open to whoever has the link, so it carries no guard.
  { path: 't/:token', component: TrackingPageComponent },
  // minLevel is the lowest role that may open the screen: TECNICO 1, MODERADOR 2, ADMINISTRADOR 3, ROOT 4.
  { path: '', component: HomeComponent, pathMatch: 'full', canActivate: [AuthGuard], data: { minLevel: 1 } },
  { path: 'business', component: EmpresaComponent, canActivate: [AuthGuard], data: { minLevel: 3 } },
  { path: 'user', component: UserComponent, canActivate: [AuthGuard], data: { minLevel: 3 } },
  { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard], data: { minLevel: 1 } },
  { path: 'costumer', component: CostumerComponent, canActivate: [AuthGuard], data: { minLevel: 1 } },
  { path: 'ticket', component: WaitingComponent, canActivate: [AuthGuard], data: { minLevel: 2, mode: 'all', title: 'Ordenes de servicio' } },
  { path: 'inbox', component: WaitingComponent, canActivate: [AuthGuard], data: { minLevel: 1, mode: 'inbox', title: 'Buzón' } },
  { path: 'report', component: ReportComponent, canActivate: [AuthGuard], data: { minLevel: 3 } },
  // The lists of tickets (pending to assign, inbox and finished) are one screen; the route says which one it is, which type it shows and its title.
  { path: 'waiting', component: WaitingComponent, canActivate: [AuthGuard], data: { minLevel: 2, requestType: null, title: 'Ordenes pendiente por asignar' } },
  { path: 'waiting2', component: WaitingComponent, canActivate: [AuthGuard], data: { minLevel: 2, requestType: 'workshop', title: 'Ordenes de Servicio de Taller pendiente por asignar' } },
  { path: 'waiting3', component: WaitingComponent, canActivate: [AuthGuard], data: { minLevel: 2, requestType: 'onSite', title: 'Ordenes de Servicio a Domicilio pendiente por asignar' } },
  { path: 'finished', component: WaitingComponent, canActivate: [AuthGuard], data: { minLevel: 2, mode: 'finished', title: 'Ordenes Completadas' } },
  { path: 'calendar', component: CalendarComponent, canActivate: [AuthGuard], data: { minLevel: 1 } }
];
