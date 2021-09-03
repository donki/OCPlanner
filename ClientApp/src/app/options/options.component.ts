import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { Globals } from '../globals.service';


@Component({
	encapsulation: ViewEncapsulation.None,
	selector: 'options',
	styleUrls: ['./options.component.css'],
	templateUrl: './options.component.html'
})
export class OptionsComponent implements OnInit {
 

	constructor(private http:HttpClient, public globals:Globals) { 
  }

  
  ngOnInit() {

  }

  updateProjectInfo(){
    this.globals.updateProjectInfo();
    this.globals.getProjectSummary();
    this.globals.getProjectInfo();
  }


}