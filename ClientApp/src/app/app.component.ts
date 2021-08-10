import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, SecurityContext } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { MsalService } from '@azure/msal-angular';

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

  jwtHelper = new JwtHelperService();  
  
  constructor(private http: HttpClient, private routeService: Router, private authService: MsalService, private _sanitizer: DomSanitizer) {
    this.loggedIn = false;
    this.InitMSal();
  }

  ngOnInit() {
    this.checkAccount();
    this.GetRights();
  }   

  private getHeaders():HttpHeaders 
  {
    let jwt = localStorage.getItem('msal.idtoken');
    let headers = new HttpHeaders({ 'Content-Type': 'application/json', Authorization: 'Bearer ' + localStorage.getItem('msal.idtoken') });
    return headers;
  }

  private GetRights() {

    this.http.get<any>('./api/Rights/GetRights/'+this.email, { headers: this.getHeaders() }).subscribe(result => {
      this.HasRights = result;
      this.canShow = true; //this.HasRights && this.loggedIn;
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
  
}