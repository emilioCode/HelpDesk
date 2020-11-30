import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
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
import { Ng2Webstorage } from 'ngx-webstorage';
import { EmpresaComponent } from './components/shared/content/empresa/empresa.component';
import { UserComponent } from './components/shared/content/user/user.component';
import { ProfileComponent } from './components/shared/content/profile/profile.component';
import { FilterPipe } from './pipes/filter.pipe';
import { CostumerComponent } from './components/shared/content/costumer/costumer.component';
import { TicketComponent } from './components/shared/content/ticket/ticket.component';
import { InboxComponent } from './components/shared/content/inbox/inbox.component';
import { ReportComponent } from './components/shared/content/report/report.component';


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
    ReportComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'counter', component: CounterComponent },
      { path: 'fetch-data', component: FetchDataComponent },
      { path: 'business', component: EmpresaComponent },
      { path: 'user', component: UserComponent },
      { path: 'profile', component: ProfileComponent },
      { path: 'costumer', component: CostumerComponent },
      { path: 'ticket', component: TicketComponent },
      { path: 'inbox', component: InboxComponent },
      { path: 'report', component: ReportComponent }
    ]),
    Ng2Webstorage
  ],
  providers: [ApiService],
  bootstrap: [AppComponent]
})
export class AppModule { }
