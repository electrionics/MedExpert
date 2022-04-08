import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
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
        ConfirmDialogComponent,
        AlertDialogComponent
    ],
    imports: [
        BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
        NgxsModule.forRoot([AnalysesState], {
          developmentMode: !environment.production
        }),
        HttpClientModule,
        FormsModule,
        RouterModule.forRoot([
            { path: '', component: HomeComponent, pathMatch: 'full' },
            { path: 'about', component: AboutComponent },
            { path: 'import/reference-intervals', component: ImportReferenceIntervalsComponent },
            { path: 'import/indicators', component: ImportIndicatorsComponent },
            { path: 'import/symptoms', component: ImportSymptomsComponent },
            { path: 'import/analysis', component: ImportAnalysisComponent }
        ]),
        AnalysesCheckModule,
        BrowserAnimationsModule,
        MatTooltipModule,
        MatDialogModule,
        MatButtonModule
    ],
    providers: [
        { provide: MAT_DIALOG_DEFAULT_OPTIONS, useValue: { hasBackdrop: true, disableClose: true } },
        CookieService
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
