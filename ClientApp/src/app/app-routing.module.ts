import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';
import { CalendarComponent } from './calendar/calendar.component';
import { SummaryComponent } from './summary/summary.component';
import { OptionsComponent } from './options/options.component';
import { CalendarDetailComponent } from './calendar/calendardetail/calendardetail.component';

const routes: Routes = [
  { path: '',  component: SummaryComponent,  canActivate: [MsalGuard] },
  { path: 'summary',  component: SummaryComponent,  canActivate: [MsalGuard] },
  { path: 'calendar',  component: CalendarComponent,  canActivate: [MsalGuard] }, 
  { path: 'calendardetail',  component: CalendarDetailComponent,  canActivate: [MsalGuard] }, 
  { path: 'calendardetail/:id',  component: CalendarDetailComponent,  canActivate: [MsalGuard] },   
  { path: 'options',  component: OptionsComponent,  canActivate: [MsalGuard] }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: false })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
