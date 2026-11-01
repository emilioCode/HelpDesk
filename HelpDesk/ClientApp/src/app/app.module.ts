import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { HeaderComponent } from './components/shared/header/header.component';
import { ContentComponent } from './components/shared/content/content.component';
import { FooterComponent } from './components/shared/footer/footer.component';
import { SettingComponent } from './components/shared/setting/setting.component';
import { MenuComponent } from './components/shared/menu/menu.component';
import { LoginComponent } from './components/shared/login/login.component';

import { ApiService } from './services/api.service';
import { AuthGuard } from './services/auth.guard';
import { AuthInterceptor } from './services/auth.interceptor';
import { SessionService } from './services/session.service';
import { Ng2Webstorage } from 'ngx-webstorage';
import { EmpresaComponent } from './components/shared/content/empresa/empresa.component';
import { UserComponent } from './components/shared/content/user/user.component';
import { ProfileComponent } from './components/shared/content/profile/profile.component';
import { FilterPipe } from './pipes/filter.pipe';
import { CostumerComponent } from './components/shared/content/costumer/costumer.component';
import { TicketComponent } from './components/shared/content/ticket/ticket.component';
import { InboxComponent } from './components/shared/content/inbox/inbox.component';
import { ReportComponent } from './components/shared/content/report/report.component';
import { WaitingComponent } from './components/shared/content/waiting/waiting.component';
import { Waiting2Component } from './components/shared/content/waiting2/waiting2.component';
import { Waiting3Component } from './components/shared/content/waiting3/waiting3.component';
import { Waiting4Component } from './components/shared/content/waiting4/waiting4.component';
import { CalendarComponent } from './components/shared/content/calendar/calendar.component';


@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    CounterComponent,
    FetchDataComponent,
    HeaderComponent,
    ContentComponent,
    FooterComponent,
    SettingComponent,
    MenuComponent,
    LoginComponent,
    EmpresaComponent,
    UserComponent,
    ProfileComponent,
    FilterPipe,
    CostumerComponent,
    TicketComponent,
    InboxComponent,
    ReportComponent,
    WaitingComponent,
    Waiting2Component,
    Waiting3Component,
    Waiting4Component,
    CalendarComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      // minLevel is the lowest role that may open the screen: TECNICO 1, MODERADOR 2, ADMINISTRADOR 3, ROOT 4.
      { path: '', component: HomeComponent, pathMatch: 'full', canActivate: [AuthGuard], data: { minLevel: 1 } },
      { path: 'counter', component: CounterComponent, canActivate: [AuthGuard], data: { minLevel: 1 } },
      { path: 'fetch-data', component: FetchDataComponent, canActivate: [AuthGuard], data: { minLevel: 1 } },
      { path: 'business', component: EmpresaComponent, canActivate: [AuthGuard], data: { minLevel: 3 } },
      { path: 'user', component: UserComponent, canActivate: [AuthGuard], data: { minLevel: 3 } },
      { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard], data: { minLevel: 1 } },
      { path: 'costumer', component: CostumerComponent, canActivate: [AuthGuard], data: { minLevel: 1 } },
      { path: 'ticket', component: TicketComponent, canActivate: [AuthGuard], data: { minLevel: 2 } },
      { path: 'inbox', component: InboxComponent, canActivate: [AuthGuard], data: { minLevel: 1 } },
      { path: 'report', component: ReportComponent, canActivate: [AuthGuard], data: { minLevel: 3 } },
      { path: 'waiting', component: WaitingComponent, canActivate: [AuthGuard], data: { minLevel: 2 } },
      { path: 'waiting2', component: Waiting2Component, canActivate: [AuthGuard], data: { minLevel: 2 } },
      { path: 'waiting3', component: Waiting3Component, canActivate: [AuthGuard], data: { minLevel: 2 } },
      { path: 'finished', component: Waiting4Component, canActivate: [AuthGuard], data: { minLevel: 2 } },
      { path: 'calendar', component: CalendarComponent, canActivate: [AuthGuard], data: { minLevel: 1 } }
    ]),
    ReactiveFormsModule,
    Ng2Webstorage
  ],
  providers: [
    ApiService,
    AuthGuard,
    SessionService,
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
