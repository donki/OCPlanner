export class OCActivity {
	id: string;
	title: string;
	description: string;
	duration: string;
	progress: string;	
	start_date: any;
	end_date: any;
	url: string;
	activitytype: any;
	tags: string[];
	period: any;
	idlinked: string[];
}

export interface ActivityTypes {
	name: string,
	code: string
}

export interface Period {
	name: string,
	code: string
}