import { DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit,  ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { CalendarDayViewBeforeRenderEvent, CalendarEvent, CalendarEventTimesChangedEvent, CalendarMonthViewBeforeRenderEvent, CalendarView, CalendarViewPeriod, CalendarWeekViewBeforeRenderEvent, DAYS_OF_WEEK } from 'angular-calendar';
import { Globals } from '../globals.service';
import { OCActivity } from '../models/OCActivity';


@Component({
	encapsulation: ViewEncapsulation.None,
	selector: 'calendar',
	styleUrls: ['./calendar.component.css'],
	templateUrl: './calendar.component.html'
})

export class CalendarComponent implements OnInit {

  view = CalendarView.Week;

  viewDate: Date = new Date();

  locale: string = 'es';

  weekStartsOn: number = DAYS_OF_WEEK.MONDAY;

  weekendDays: number[] = [DAYS_OF_WEEK.SATURDAY, DAYS_OF_WEEK.SUNDAY];  
  excludeDays: number[] = [0, 6];

  period: CalendarViewPeriod;

  constructor(private cdr: ChangeDetectorRef, private http:HttpClient, private globals:Globals, private routeService: Router, public datepipe: DatePipe) {}

  beforeViewRender(
    event:
      | CalendarMonthViewBeforeRenderEvent
      | CalendarWeekViewBeforeRenderEvent
      | CalendarDayViewBeforeRenderEvent
  ) {
    this.period = event.period;
    this.cdr.detectChanges();
  }
   
  ngOnInit() {       
    this.globals.getActivities();
    this.globals.getProjectSummary();
    this.globals.getProjectInfo();    
  }

  changeViewToWeek() {
    this.view = CalendarView.Week;   
  } 

  changeViewToMonth() {
    this.view = CalendarView.Month;   
  } 

  changeViewToDay() {
    this.view = CalendarView.Day;   
  } 

  eventClicked({ event }: { event: CalendarEvent }): void {
    this.routeService.navigateByUrl('/calendardetail/'+event.id);
  }

  eventTimesChanged({
    event,
    newStart,
    newEnd,
  }: CalendarEventTimesChangedEvent): void {

    this.http.get('api/Planner/Activities/'+event.id,{ headers: this.globals.getHeaders() }).subscribe(
       (d:any) => {
         let data = JSON.parse(d);
         data.start_date = this.datepipe.transform(newStart, 'yyyy-MM-dd HH:mm');
         data.end_date = this.datepipe.transform(newEnd, 'yyyy-MM-dd HH:mm');
         this.globals.updateTask(data);    
         this.globals.getActivities();      
       }
    );
  }

  NewActivity() {
    this.routeService.navigateByUrl('/calendardetail');
  }


}