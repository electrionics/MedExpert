import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {HTTP_INTERCEPTORS, HttpClientModule} from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { NgxsModule } from '@ngxs/store';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { FooterComponent } from './components/footer/footer.component';
import { HomeComponent } from './components/home/home.component';
import { AboutComponent } from './components/about/about.component';

import { ImportReportComponent } from './components/import/importReport.component';


import { ImportIndicatorsComponent } from './components/import/importIndicators.component';
import { ImportReferenceIntervalsComponent } from './components/import/importReferenceIntervals.component';
import { ImportSymptomsComponent } from "./components/import/importSymptoms.component";
import { ImportAnalysisComponent } from "./components/import/importAnalysis.component";

import { LoginComponent } from "./components/account/login.component";

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatTooltipModule } from "@angular/material/tooltip";
import { MAT_DIALOG_DEFAULT_OPTIONS, MatDialogModule } from "@angular/material/dialog";
import { MatButtonModule } from "@angular/material/button";
import { ConfirmDialogComponent } from "./components/common/confirm.component";
import { AlertDialogComponent } from "./components/common/alert.component";
import { CookieService } from "./services/cookieService.component";

import {AnalysesCheckModule} from './pages/analyses-check/analyses-check.module';
import {environment} from '../environments/environment';
import {AnalysesState} from './store/state/analyses.state';
import {IfLoggedDirective} from "./directives/if-logged.directive";
import {WithCredentialsInterceptor} from "./interceptors/withCredentials.interceptor";
import {AuthInteceptor} from "./interceptors/auth.interceptor";
import {AuthGuard} from "./services/auth.guard";

@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        FooterComponent,
        HomeComponent,
        AboutComponent,
        ImportReportComponent,
        ImportReferenceIntervalsComponent,
        ImportIndicatorsComponent,
        ImportSymptomsComponent,
        ImportAnalysisComponent,
        LoginComponent,
        ConfirmDialogComponent,
        AlertDialogComponent,
        IfLoggedDirective
    ],
    imports: [
        BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
        NgxsModule.forRoot([AnalysesState], {
          developmentMode: !environment.production
        }),
        HttpClientModule,
        FormsModule,
        RouterModule.forRoot([
            //{ path: '', component: HomeComponent, pathMatch: 'full' },
            //{ path: 'about', component: AboutComponent },
            { path: 'import/reference-intervals', component: ImportReferenceIntervalsComponent, canActivate: [AuthGuard] },
            { path: 'import/indicators', component: ImportIndicatorsComponent, canActivate: [AuthGuard] },
            { path: 'import/symptoms', component: ImportSymptomsComponent, canActivate: [AuthGuard] },
            { path: 'import/analysis', component: ImportAnalysisComponent, canActivate: [AuthGuard] },
            { path: 'account/login', component: LoginComponent },
        ]),
        AnalysesCheckModule,
        BrowserAnimationsModule,
        MatTooltipModule,
        MatDialogModule,
        MatButtonModule
    ],
    providers: [
        { provide: MAT_DIALOG_DEFAULT_OPTIONS, useValue: { hasBackdrop: true, disableClose: true } },
        CookieService,
        { provide: HTTP_INTERCEPTORS, useClass: WithCredentialsInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: AuthInteceptor, multi: true }
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
