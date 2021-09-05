import { DatePipe } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { ConfirmationService } from "primeng/api";
import { Globals } from "src/app/globals.service";
import { ActivityTypes, OCActivity, Period } from "src/app/models/OCActivity";

@Component({

	selector: 'calendardetail',
	styleUrls: ['./calendardetail.component.css'],
	templateUrl: './calendardetail.component.html'
})

export class CalendarDetailComponent implements OnInit {

    activitytypes: ActivityTypes[];
    periods: Period[];
    
    activity:OCActivity = new OCActivity();
    editing: boolean = false;
    returnroute:string = 'calendar';

    constructor( private route: ActivatedRoute, private routeService: Router, 
                 private confirmationService: ConfirmationService, private globals:Globals, 
                 public datepipe: DatePipe, private http:HttpClient ) {}

    ngOnInit(){

        this.activitytypes = [
            {name: 'Tarea', code: 'Task'},
            {name: 'Reunión', code: 'Meeting'}
        ];

        this.periods = [
            {name: 'Sin periodo', code: 'NoPeriod'},
            {name: 'Diaria', code: 'Dayly'},            
            {name: 'Semanal', code: 'Weekly'},
            {name: 'BiSemanal', code: 'BiWeekly'},                        
            {name: 'TriSemanal', code: 'ThreeWeekly'},            
            {name: 'Mensual', code: 'Monthly'}
        ];
      
        let taskid = this.route.snapshot.paramMap.get('id');
        this.activity.planned = true;
        if (taskid != undefined) {
            if(taskid.includes('nonplanned'))
            {
                this.activity.planned = false;
                this.returnroute = 'tasks';
                taskid = taskid.replace('nonplanned','');
            }
            if(taskid.length > 0) 
            {
                this.editing = true;
                this.http.get('api/Planner/Activities/'+taskid,{ headers: this.globals.getHeaders() }).subscribe(
                    (data:any)=> { 
                        this.activity = JSON.parse(data);
                        this.activity.start_date = new Date(this.activity.start_date);
                        this.activity.end_date = new Date(this.activity.end_date);                    
                        this.activitytypes.forEach((type) => {
                            if (this.activity.activitytype == type.code ) {
                                this.activity.activitytype = type;
                            }
                          });  
                          this.periods.forEach((type) => {
                            if (this.activity.period == type.code ) {
                                this.activity.period = type;
                            }
                          });                                             
                        });
    
            } 
        }    
    }

    CloseActivityDetail(){
        this.routeService.navigateByUrl('/'+this.returnroute);
    }

    SaveActivity(){
     if (
          (this.activity.title == undefined) ||
          (this.activity.activitytype == undefined) ||
          (this.activity.start_date == undefined) ||
          (this.activity.period == undefined) ||          
          (this.activity.duration == undefined)) 
        {
            this.confirmationService.confirm({
                header:'Aviso',
                message: 'Faltan campos por rellenar',
                rejectVisible:false
            });        
        }
        else 
        {
            this.activity.start_date = this.datepipe.transform(this.activity.start_date, 'yyyy-MM-dd HH:mm');
            this.activity.end_date = this.datepipe.transform(this.activity.end_date, 'yyyy-MM-dd HH:mm');            
            this.activity.activitytype = (this.activity.activitytype as unknown as ActivityTypes).code;
            this.activity.period = (this.activity.period as unknown as Period).code;
            this.activity.duration = this.activity.duration.toString();
            if (this.activity.progress == undefined) 
            {
                this.activity.progress = "0";     
            }
            this.activity.progress = this.activity.progress.toString();    
            if (!this.editing) {
                this.globals.insertTask(this.activity).add(()=>this.routeService.navigateByUrl('/'+this.returnroute));
            } else {
                this.globals.updateTask(this.activity).add(()=>this.routeService.navigateByUrl('/'+this.returnroute));
            }
            
        }


    }

    RemoveActivity() {
        this.globals.deleteTask(this.activity).add(()=>this.routeService.navigateByUrl('/'+this.returnroute)); 
    }
}