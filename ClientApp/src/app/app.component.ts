import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, SecurityContext } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { MsalService } from '@azure/msal-angular';
import { NgxSpinnerService } from 'ngx-spinner';
import { MenuItem, MessageService } from 'primeng/api';
import { Globals } from './globals.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {

  private _opened: boolean = false;
 
  private _toggleSidebar() {
    this._opened = !this._opened;
  }

  title = 'Remote Management';
  canShow = false;
  loggedIn = false;
  HasRights = false;
  userName: string; 
  email: string;
  items: MenuItem[];
  home: MenuItem;

  jwtHelper = new JwtHelperService();  
  
  constructor(public globals:Globals,  private http: HttpClient, private routeService: Router, private messageService: MessageService,
              private authService: MsalService, private _sanitizer: DomSanitizer, private spinner: NgxSpinnerService) {
    this.loggedIn = false;
    this.InitMSal();
    this.globals.setToastFunction(this.ToastSaveDate,this.messageService);
  }

  ngOnInit() {
    this.spinner.show();
    this.checkAccount();
    this.GetRights();    
    this.items = [
            {label: 'Análisis', icon: 'pi pi-fw pi-book', routerLink: ['/summary']},
            {label: 'Calendario', icon: 'pi pi-fw pi-calendar', routerLink: ['/calendar']},
            {label: 'Tareas no planificadas', icon: 'pi pi-fw pi-folder', routerLink: ['/tasks']},            
            {label: 'Opciones', icon: 'pi pi-fw pi-cog', routerLink: ['/options']}
        ];            
  }   

  private GetRights() {

    this.http.get<any>('./api/Rights/GetRights/'+this.email, { headers: this.globals.getHeaders() }).subscribe(result => {
      this.HasRights = result;
      this.globals.PlanTabVisible = true;
      setTimeout(() => {
        this.spinner.hide();              
      }, 1000);
    }, error => console.log(error));

  } 

  
  private InitMSal() {
    if (!this.authService.getAccount()) {
      this.authService.loginRedirect();
    } else {
      let rawToken = localStorage.getItem('msal.idtoken');
      if (this.jwtHelper.isTokenExpired(rawToken)){
        localStorage.clear();
        this.authService.loginRedirect();        
      }
    }

  }

  public logOut() {
    this.authService.logout();
  }
  
  checkAccount() {
    let account = this.authService.getAccount();
    this.userName = account.name;
    this.email = account.userName;
    this.loggedIn = !!this.authService.getAccount();
    this.canShow = true;//this.HasRights && this.loggedIn;    
  }   

  navigateToTasks()
  {
    this.routeService.navigateByUrl("/Tasks");
  }

  navigateToGantt()
  {
    this.routeService.navigateByUrl("/Gantt");
  }  

  ToastSaveDate(messageService) {
    messageService.clear();
    messageService.add({severity:'success', summary:'Guardado', detail:'Los datos se han guardado correctamente.'});
  }  
  
}