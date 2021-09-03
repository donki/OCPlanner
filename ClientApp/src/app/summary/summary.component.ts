import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, ElementRef, OnInit, SimpleChanges, ViewChild, ViewEncapsulation } from '@angular/core';
import { MessageService } from 'primeng/api';
import { Globals } from '../globals.service';
import { OCProject } from '../models/OCProject';
import { ProjectSummaryMonth } from '../models/ProjecSummaryMonth';

@Component({
	encapsulation: ViewEncapsulation.None,
	selector: 'summary',
	styleUrls: ['./summary.component.css'],
	templateUrl: './summary.component.html'
})

export class SummaryComponent implements OnInit {

  constructor(private http:HttpClient,private messageService: MessageService, public globals:Globals) { 
    
  }

  
  ngOnInit() {
	  this.getProjectInfo();
  }

  getProjectInfo() {
    this.globals.getProjectSummary();
    this.globals.getProjectInfo();
  }

  updateProjectInfo(){
    this.globals.updateProjectInfo();
    this.globals.getProjectSummary();
    this.globals.getProjectInfo();
  }

}