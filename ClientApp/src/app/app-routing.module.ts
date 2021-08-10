import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { GanttComponent } from './gantt/gantt.component';
import { MsalGuard } from '@azure/msal-angular';

const routes: Routes = [
  { path: '',  component: GanttComponent,  canActivate: [MsalGuard] },
  { path: 'Gantt',  component: GanttComponent,  canActivate: [MsalGuard] },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: false })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
