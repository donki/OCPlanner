import { StringMapWithRename } from "@angular/compiler/src/compiler_facade_interface";
import { OCActivity } from "./OCActivity";

export class OCProject {
  ProjectName:string ='SIN TITULO';
  DailyPlannedWorkHours: number = 0;
  DailyFullWorkHours: number = 0;
  PercePlannedHoursvsFullWorkHours: number = 0;
  Tasks: OCActivity[] = [];
  NonPlannedActivities: OCActivity[] = [];
  HolyDays: string [] = [];
  StartDate: any;
  EndDate: any;
  WebHook: string = '';
  APIKey: string = '';
}