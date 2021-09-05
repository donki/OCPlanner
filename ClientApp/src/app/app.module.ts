import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { Configuration } from 'msal';
import { MsalAngularConfiguration, MsalModule, MsalService, MSAL_CONFIG, MSAL_CONFIG_ANGULAR } from '@azure/msal-angular';
import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ProgressBarModule } from 'primeng/progressbar';
import { SpinnerModule } from 'primeng/spinner';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/components/common/messageservice';
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
import { DynamicDialogModule } from 'primeng/components/dynamicdialog/dynamicdialog';
import { CalendarDetailComponent } from './calendar/calendardetail/calendardetail.component';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { ChipsModule } from 'primeng/chips';
import { NgxSpinnerModule } from 'ngx-spinner';
import { CheckboxModule } from 'primeng/checkbox';
import { TableModule } from 'primeng/table';
import { faBook, faCalendar } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule, FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { TasksComponent } from './tasks/tasks.component';
import { DataViewModule } from 'primeng/dataview';

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

registerLocaleData(localeEs);

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
    CalendarComponent,
    CalendarDetailComponent,
    SummaryComponent,
    OptionsComponent,
    TasksComponent
    
  ],
  imports: [
    DataViewModule,
    FontAwesomeModule,
    TableModule,
    TabMenuModule,
    CheckboxModule,
    NgxSpinnerModule,
    ChipsModule,
    ConfirmDialogModule,
    DynamicDialogModule,
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
export class AppModule {
  constructor(private library: FaIconLibrary) {
    library.addIcons(faBook, faCalendar);
  }
}
