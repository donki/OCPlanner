import { DatePipe } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ThrowStmt } from '@angular/compiler';
import { ChangeDetectionStrategy, Component, DoCheck, Injectable, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CalendarEvent } from 'angular-calendar/modules/common/calendar-common.module';
import { MessageService } from 'primeng/api';
import { OCActivity } from './models/OCActivity';
import { OCProject } from './models/OCProject';
import { ProjectSummaryMonth } from './models/ProjecSummaryMonth';
import { ProjectSummary } from './models/ProjectSummary';

@Injectable()
export class Globals  {
  projectSummary: ProjectSummary = new ProjectSummary();
  project: OCProject = new OCProject();  
  PlanTabVisible = false;
  events: CalendarEvent[] = [];
  AllTasks: OCActivity[] = [];  
  NonPlannedTask: OCActivity[] = [];  
  MonthPeriod: ProjectSummaryMonth;
  
	constructor(private http:HttpClient, private messageService: MessageService, public datepipe: DatePipe) { 
  }  

  public getHeaders():HttpHeaders 
  {
    let jwt = localStorage.getItem('msal.idtoken');
    let headers = new HttpHeaders({ 'Content-Type': 'application/json', Authorization: 'Bearer ' + localStorage.getItem('msal.idtoken') });
    return headers;
  }


  getProjectSummary() {
    return this.http.get('api/Planner/ProjectSummary',{ headers: this.getHeaders() }).subscribe((data:any) => {
      this.projectSummary = JSON.parse(data);
      this.projectSummary.ProjectSummaryMonths.forEach((month) => {
                 if (month.isMonthPeriod) {
                     this.MonthPeriod = month
                 }
                });
    });
  }

 
  updateProjectInfo() {
    let tmp = new OCProject();
    tmp.DailyPlannedWorkHours = this.project.DailyPlannedWorkHours;
    tmp.DailyFullWorkHours = this.project.DailyFullWorkHours;    
    tmp.ProjectName = this.project.ProjectName;
    tmp.HolyDays = this.project.HolyDays;
    tmp.WebHook = this.project.WebHook;
    tmp.APIKey = this.project.APIKey;
    tmp.StartDate = this.datepipe.transform(this.project.StartDate, 'yyyy-MM-dd HH:mm');
    tmp.EndDate = this.datepipe.transform(this.project.EndDate, 'yyyy-MM-dd HH:mm');    
    return this.http.post('api/Planner/Project',tmp,{ headers: this.getHeaders() }).subscribe(() => {this.getProjectSummary(); this.getProjectInfo();});  
  }

  getProjectInfo() {
    return this.http.get('api/Planner/Project',{ headers: this.getHeaders() }).subscribe((data:string) => { 
                this.project = JSON.parse(data);
                this.project.StartDate = new Date(this.project.StartDate);  
                this.project.EndDate = new Date(this.project.EndDate);                  
              });  
  }    

  getActivities() {    
    this.events = [];
    this.NonPlannedTask = [];
    return this.http.get('api/Planner/Activities',{ headers: this.getHeaders() }).subscribe((data:any) => {
      let dataArr = JSON.parse(data);
      this.AllTasks = dataArr;
      this.AllTasks.forEach((item:OCActivity)=> 
        { 
          if(item.planned) {
            const event:CalendarEvent = {
              id : item.id,
              title : item.title,
              start : new Date(item.start_date),
              end : new Date(item.end_date),
              draggable : true,
              resizable : { beforeStart: true, afterEnd: true }
            }
            this.events = [...this.events, event];  
          } else {
            this.NonPlannedTask.push(item);
          }
        });
    });
  }

  ToastSaveDate() {
    this.messageService.clear();
    this.messageService.add({severity:'success', summary:'Guardado', detail:'Los datos se han guardado correctamente'});
  }  

  insertTask(activity:OCActivity){
    return this.http.post('api/Planner/Activities',activity,{ headers: this.getHeaders() }).subscribe(() => {this.getProjectSummary(); this.getProjectInfo();});    
  }

  updateTask(activity:OCActivity){
    return this.http.put('api/Planner/Activities/'+activity.id,activity,{ headers: this.getHeaders() }).subscribe(() => {this.getProjectSummary(); this.getProjectInfo();});    
  }

  deleteTask(activity){
    return this.http.delete('api/Planner/Activities/'+activity.id,{ headers: this.getHeaders() }).subscribe(() => {this.getProjectSummary(); this.getProjectInfo();});    
  }  
}

