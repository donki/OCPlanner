import { ChangeDetectionStrategy, Component, OnChanges, OnInit, SimpleChanges, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { Globals } from '../globals.service';


@Component({
	encapsulation: ViewEncapsulation.None,
	selector: 'tasks',
	styleUrls: ['./tasks.component.css'],
	templateUrl: './tasks.component.html'
})
export class TasksComponent implements OnInit {
 

	constructor(public globals:Globals, private routeService: Router) { 
  }

  
  ngOnInit() {
    this.globals.getProjectSummary();
    this.globals.getProjectInfo();
    this.globals.getActivities();    
  }

  NewActivity() {
    this.routeService.navigateByUrl('/calendardetail/nonplanned');
  }

  EditTask(id:string) {
    this.routeService.navigateByUrl('/calendardetail/nonplanned'+id);
  }


}