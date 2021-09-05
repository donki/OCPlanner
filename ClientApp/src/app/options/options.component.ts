import { ChangeDetectionStrategy, Component, OnChanges, OnInit, SimpleChanges, ViewEncapsulation } from '@angular/core';
import { Globals } from '../globals.service';


@Component({
	encapsulation: ViewEncapsulation.None,
	selector: 'options',
	styleUrls: ['./options.component.css'],
	templateUrl: './options.component.html'
})
export class OptionsComponent implements OnInit {
 

	constructor(public globals:Globals) { 
  }

  
  ngOnInit() {
    this.globals.getProjectSummary();
    this.globals.getProjectInfo();
  }


}