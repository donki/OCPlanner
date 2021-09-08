export class OCActivity {
	id: string = '';
	title: string = '';
	description: string = '';
	duration: string = '0';
	progress: string = '0';	
	start_date: any;
	end_date: any;
	url: string = '';
	activitytype: any;
	tags: string[]  = [];
	period: any;
	idlinked: string[] = [];
	parentid: string  = '';
	planned: boolean = true;
}

export interface ActivityTypes {
	name: string,
	code: string
}

export interface Period {
	name: string,
	code: string
}