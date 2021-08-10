import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { Configuration } from 'msal';
import { MsalAngularConfiguration, MsalModule, MsalService, MSAL_CONFIG, MSAL_CONFIG_ANGULAR } from '@azure/msal-angular';
import { AppComponent } from './app.component';
import { GanttComponent } from './gantt/gantt.component';
import { AppRoutingModule } from './app-routing.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';


export const protectedResourceMap: [string, string[]][] = [
  ['https://graph.microsoft.com/v1.0/me', ['user.read']]
];

// This setup won't change in our environment, so we were fine with having it statically included
export const clientId = window.location.origin.includes('qa') || window.location.origin.includes('localhost') ? '<qa_id>' : '<prod_id>';

export function MSALConfigFactory(): Configuration {
  return {
    auth: {
      clientId: '61d20a95-775b-4fd5-9e97-2412aa3886ec',
      authority: "https://login.microsoftonline.com/cb2521c7-79a9-48f1-ab75-51511b37e755/",
      validateAuthority: true,
      redirectUri: window.location.origin,
      postLogoutRedirectUri: window.location.origin,
      navigateToLoginRequestUrl: true,
    },
    cache: {
      cacheLocation: "localStorage"
    },
  };
}

export function MSALAngularConfigFactory(): MsalAngularConfiguration {
  return {
    consentScopes: [
      "user.read",
    ],
    unprotectedResources: [],
    protectedResourceMap,
    extraQueryParameters: {}
  };
}

@NgModule({
  declarations: [
    AppComponent,
    GanttComponent
    
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule, 
    FormsModule,
    BrowserAnimationsModule,   
    MsalModule,
    ToastrModule.forRoot()    
  ],
  providers: [
    {
      provide: MSAL_CONFIG,
      useFactory: MSALConfigFactory
    },
    {
      provide: MSAL_CONFIG_ANGULAR,
      useFactory: MSALAngularConfigFactory
    },
    MsalService,
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
