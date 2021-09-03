import { OCActivity } from "./OCActivity";

export class OCProject {
  ProjectName:string ='SIN TITULO';
  DailyPlannedWorkHours: number = 0;
  DailyFullWorkHours: number = 0;
  PercePlannedHoursvsFullWorkHours: number = 0;
  Tasks: OCActivity[];
  HolyDays: string [];
  StartDate: any;
  EndDate: any;
}