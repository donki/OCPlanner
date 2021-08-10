import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, ElementRef, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { GanttTask } from '../models/GanttTask';
import { GanttLink } from '../models/GanttLink';

import 'dhtmlx-gantt';
import { gantt } from 'dhtmlx-gantt';

@Component({
	encapsulation: ViewEncapsulation.None,
	selector: 'gantt',
	styleUrls: ['./gantt.component.css'],
	templateUrl: './gantt.component.html'
})
export class GanttComponent implements OnInit {
	@ViewChild('gantt_here',null) ganttContainer: ElementRef;

	constructor(private http:HttpClient) { 
  }

  customizations() {
  }

	ngOnInit() {

    this.customizations();
    gantt.i18n.setLocale("es");
		//gantt.config.xml_date = '%Y-%m-%d %H:%i';
    gantt.config.date_format = '%Y-%m-%d %H:%i';
    gantt.config.work_time = true;  
    gantt.config.skip_off_time = true; 
    gantt.config.min_duration = 60*60*1000;
    gantt.config.duration_unit = "hour";    
    gantt.setWorkTime({hours : ["9:00-14:00", "16:00-17:30"]})
 
    gantt.setWorkTime({ day:6, hours:false });
    gantt.setWorkTime({ day:7, hours:false });   
    
    gantt.plugins({ tooltip: true });     
  

		gantt.init(this.ganttContainer.nativeElement);
    this.getTasks();
    this.getLinks();

    this.AttachToEvents();       

	}

  private AttachToEvents() {
    gantt.attachEvent('onAfterTaskAdd', this.insertTask, { thisObject: this });
    gantt.attachEvent('onAfterTaskDelete', this.deleteTask, { thisObject: this });
    gantt.attachEvent('onAfterTaskUpdate', this.updateTask, { thisObject: this });

    gantt.attachEvent('onAfterLinkAdd', this.insertLink, { thisObject: this });
    gantt.attachEvent('onAfterLinkDelete', this.deleteLink, { thisObject: this });
    gantt.attachEvent('onAfterLinkUpdate', this.updateLink, { thisObject: this });
  }


    private getHeaders():HttpHeaders 
  {
    let jwt = localStorage.getItem('msal.idtoken');
    let headers = new HttpHeaders({ 'Content-Type': 'application/json', Authorization: 'Bearer ' + localStorage.getItem('msal.idtoken') });
    return headers;
  }

  //#region Task
  getTasks() {    
    return this.http.get('api/Gantt/Tasks',{ headers: this.getHeaders() }).subscribe((data:any) => {
      gantt.clearAll();
      let dataArr = JSON.parse(data);
      gantt.detachAllEvents();                                                  
      Array.from(dataArr).forEach((item)=> {

                                     let CastedItem = this.getCastedTask(item);
                                     gantt.addTask(CastedItem);
                                  });
      this.AttachToEvents();                                        
    });
  }

  insertTask(id,item){
    let tmp = this.getGanttTask(item);    
    return this.http.post('api/Gantt/Tasks',tmp,{ headers: this.getHeaders() }).subscribe(() => {});    
  }

  updateTask(id,item){
    let tmp = this.getGanttTask(item);    
    return this.http.put('api/Gantt/Tasks/'+id,tmp,{ headers: this.getHeaders() }).subscribe(() => {});    
  }

  deleteTask(id,item){
    return this.http.delete('api/Gantt/Tasks/'+id,{ headers: this.getHeaders() }).subscribe(() => {});    
  }

  

  private getGanttTask(item: any) {
    let formatFunc = gantt.date.date_to_str("%d/%m/%Y %H %i");
    let tmp = new GanttTask();
    tmp.duration = item.duration.toString();
    tmp.id = item.id.toString();
    tmp.parent = item.parent.toString();
    tmp.progress = item.progress.toString();
    tmp.text = item.text.toString();
    tmp.start_date = formatFunc(item.start_date);
    tmp.end_date = formatFunc(item.end_date);
    return tmp;
  }

  private getCastedTask(item: any) {
    let formatFunc = gantt.date.str_to_date("%d/%m/%Y %H %i");    
    let tmp = new GanttTask();
    tmp.duration = parseInt(item.duration);
    tmp.id = parseInt(item.id);
    tmp.parent = item.parent;
    tmp.progress = item.progress;
    tmp.text = item.text;
    tmp.start_date = formatFunc(item.start_date);
    tmp.end_date = formatFunc(item.end_date);
    return tmp;
  }

  //#endregion

 //#region Links
 getLinks() {
  return this.http.get('api/Gantt/Links',{ headers: this.getHeaders() }).subscribe((data:any) => {
    let dataArr = JSON.parse(data);
    gantt.detachAllEvents();
    Array.from(dataArr).forEach((item)=> {
                                   let CastedItem = this.getCastedLink(item);
                                   gantt.addLink(CastedItem);
                                });
    this.AttachToEvents();
  });
}

insertLink(id,item){
  let tmp = this.getGanttLink(item);    
  return this.http.post('api/Gantt/Links',tmp,{ headers: this.getHeaders() }).subscribe(() => {});    
}

updateLink(id,item){
  let tmp = this.getGanttLink(item);    
  return this.http.put('api/Gantt/Links/'+id,tmp,{ headers: this.getHeaders() }).subscribe(() => {});    
}

deleteLink(id,item){
  return this.http.delete('api/Gantt/Links/'+id,{ headers: this.getHeaders() }).subscribe(() => {});    
}



private getGanttLink(item: any) {
  let tmp = new GanttLink();
  tmp.source = item.source.toString();
  tmp.target = item.target.toString();  
  tmp.id = item.id.toString();
  tmp.type = item.type.toString();  
  return tmp;
}

private getCastedLink(item: any) {
  let tmp = new GanttLink();
  tmp.id = parseInt(item.id);
  tmp.source = item.source.toString();
  tmp.target = item.target.toString();  
  tmp.type = item.type.toString();
  return tmp;
}

donwloadIcalc(){
  gantt.exportToICal({});
}

//#endregion  
}