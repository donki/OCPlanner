import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ProgressBarModule } from 'primeng/progressbar';
import { SpinnerModule } from 'primeng/spinner';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { TabMenuModule } from 'primeng/tabmenu';
import { CalendarComponent } from './calendar/calendar.component';
import { MenuModule } from 'primeng/menu'
import { SummaryComponent } from './summary/summary.component';
import { OptionsComponent } from './options/options.component';
import { Globals } from './globals.service';
import { CalendarCommonModule, CalendarModule, DateAdapter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';
import { DatePipe, registerLocaleData } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { CalendarModule as CalendarModulePrimeNG } from 'primeng/calendar';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import localeEs from '@angular/common/locales/es';
import { CalendarDetailComponent } from './calendar/calendardetail/calendardetail.component';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ChipsModule } from 'primeng/chips';
import { CheckboxModule } from 'primeng/checkbox';
import { TableModule } from 'primeng/table';
import { TasksComponent } from './tasks/tasks.component';
import { DataViewModule } from 'primeng/dataview';
import { MsalBroadcastService, MsalGuard, MsalGuardConfiguration, MsalInterceptor, MsalInterceptorConfiguration, MsalModule, MsalService, MSAL_GUARD_CONFIG, MSAL_INSTANCE, MSAL_INTERCEPTOR_CONFIG } from '@azure/msal-angular';
import { BrowserCacheLocation, Configuration, IPublicClientApplication, LogLevel, PublicClientApplication } from '@azure/msal-browser';
import { InteractionType } from '@azure/msal-browser';
import { InputNumberModule } from 'primeng/inputnumber';
import { DividerModule } from 'primeng/divider';


// This setup won't change in our environment, so we were fine with having it statically included
export const clientId = window.location.origin.includes('qa') || window.location.origin.includes('localhost') ? '<qa_id>' : '<prod_id>';

export function MSALInstanceFactory(): IPublicClientApplication {
  return new PublicClientApplication(msalConfig);
}

export const msalConfig: Configuration = {
  auth: {
    clientId: '61d20a95-775b-4fd5-9e97-2412aa3886ec',
    authority: "https://login.microsoftonline.com/cb2521c7-79a9-48f1-ab75-51511b37e755/",
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
    navigateToLoginRequestUrl: true
  },
  cache: {
    cacheLocation: BrowserCacheLocation.LocalStorage
  },
  system: {
    loggerOptions: {
      loggerCallback(logLevel: LogLevel, message: string) {
        console.log(message);
      },
      logLevel: LogLevel.Error,
      piiLoggingEnabled: false
    }
  }
}

export function MSALInterceptorConfigFactory(): MsalInterceptorConfiguration {
  const protectedResourceMap = new Map<string, Array<string>>();
    protectedResourceMap.set('https://graph.microsoft-ppe.com/v1.0/me', ['user.read']);

  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap
  };
}

registerLocaleData(localeEs);

export function MSALGuardConfigFactory(): MsalGuardConfiguration {
  return { 
    interactionType: InteractionType.Redirect,
    authRequest: {
      scopes: ['user.read']
    },
    loginFailedRoute: '/login-failed'
  };
}

@NgModule({
  declarations: [
    AppComponent,
    CalendarComponent,
    CalendarDetailComponent,
    SummaryComponent,
    OptionsComponent,
    TasksComponent
    
  ],
  imports: [
    DividerModule,
    InputNumberModule,
    DataViewModule,
    TableModule,
    TabMenuModule,
    CheckboxModule,
    ChipsModule,
    ConfirmDialogModule,
    DropdownModule,
    InputTextareaModule,
    CalendarModulePrimeNG,
    DialogModule,
    BrowserModule,
    HttpClientModule,
    AppRoutingModule, 
    FormsModule,
    BrowserAnimationsModule,   
    MsalModule,
    ProgressBarModule,
    SpinnerModule,
    InputTextModule,
    ToastModule,
    TabMenuModule,
    MenuModule,
    BrowserAnimationsModule,
    CalendarCommonModule,
    CalendarModule.forRoot({
      provide: DateAdapter,
      useFactory: adapterFactory,
    })    
  ],
  providers: [ Globals, MessageService, ConfirmationService, DatePipe,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true
    },
    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory
    },
    {
      provide: MSAL_GUARD_CONFIG,
      useFactory: MSALGuardConfigFactory
    },
    {
      provide: MSAL_INTERCEPTOR_CONFIG,
      useFactory: MSALInterceptorConfigFactory
    },
    MsalService,
    MsalGuard,
    MsalBroadcastService,
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
