import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, SecurityContext } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { InteractionStatus } from '@azure/msal-browser';
//import { NgxSpinnerService } from 'ngx-spinner';
import { MenuItem, MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
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
  
  isIframe = false;
  loginDisplay = false;
  private readonly _destroying$ = new Subject<void>();

  canShow = false;
  loggedIn = false;
  HasRights = false;
  userName: string | undefined; 
  email: string = '';
  items: MenuItem[] = [];

  jwtHelper = new JwtHelperService();  
  
  constructor(public globals:Globals,  private http: HttpClient, private routeService: Router, //private spinner: NgxSpinnerService
              private messageService: MessageService, private msalBroadcastService: MsalBroadcastService,
              private authService: MsalService, private _sanitizer: DomSanitizer) {
    this.loggedIn = false;
    //this.InitMSal();
    this.globals.setToastFunction(this.ToastSaveDate,this.messageService);
  }

  ngOnInit() {
    //this.spinner.show();
    this.setLoginDisplay();
    
    this.msalBroadcastService.inProgress$
      .pipe(
        filter((status: InteractionStatus) => status === InteractionStatus.None),
        takeUntil(this._destroying$)
      )
      .subscribe(() => {
        this.setLoginDisplay();
        this.getActiveAccount();
        //this.InitMSal();
        this.checkAccount();
        this.GetRights();            
      })
    this.items = [
            {label: 'Análisis', icon: 'pi pi-fw pi-book', routerLink: ['/summary']},
            {label: 'Calendario', icon: 'pi pi-fw pi-calendar', routerLink: ['/calendar']},
            {label: 'Tareas no planificadas', icon: 'pi pi-fw pi-folder', routerLink: ['/tasks']},            
            {label: 'Opciones', icon: 'pi pi-fw pi-cog', routerLink: ['/options']}
        ];            
  }   

  setLoginDisplay() {
    this.loginDisplay = this.authService.instance.getAllAccounts().length > 0;
  }

  private GetRights() {

    this.http.get<any>('./api/Rights/GetRights/'+this.email, { headers: this.globals.getHeaders() }).subscribe(result => {
      this.HasRights = result;
      this.globals.PlanTabVisible = true;
      setTimeout(() => {
        //this.spinner.hide();              
      }, 1000);
    }, error => console.log(error));

  } 

  
  private InitMSal() {
    if (!this.getActiveAccount()) {
      this.authService.loginRedirect();
    } else {
      let rawToken = localStorage.getItem('msal.idtoken')?.toString();
      if (this.jwtHelper.isTokenExpired(rawToken)){
        localStorage.clear();
        this.authService.loginRedirect();        
      }
    }

  }

  logout(popup?: boolean) {
    if (popup) {
      this.authService.logoutPopup({
        mainWindowRedirectUri: "/"
      });
    } else {
      this.authService.logoutRedirect();
    }
  }
  
  getActiveAccount(){
    let activeAccount = this.authService.instance.getActiveAccount();

    if (!activeAccount && this.authService.instance.getAllAccounts().length > 0) {
      let accounts = this.authService.instance.getAllAccounts();
      this.authService.instance.setActiveAccount(accounts[0]);
    }

    return activeAccount;
  }

  checkAccount() {
    let account = this.getActiveAccount();
    if (account != null) {
      this.userName = account.name?.toString();
      this.email = account.username.toString()  
    }
    this.loggedIn = !!this.getActiveAccount();
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

  ToastSaveDate(messageService: { clear: () => void; add: (arg0: { severity: string; summary: string; detail: string; }) => void; }) {
    messageService.clear();
    messageService.add({severity:'success', summary:'Guardado', detail:'Los datos se han guardado correctamente.'});
  }  
  
}