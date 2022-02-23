import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { FooterComponent } from './footer/footer.component';
import { HomeComponent } from './home/home.component';
import { AboutComponent } from './about/about.component';
import { ImportReferenceIntervalsComponent } from './import/importReferenceIntervals.component';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatTooltipModule } from "@angular/material/tooltip";
import { MAT_DIALOG_DEFAULT_OPTIONS, MatDialogModule } from "@angular/material/dialog";
import { MatButtonModule } from "@angular/material/button";
import { ConfirmDialogComponent } from "./common/confirm.component";
import { AlertDialogComponent } from "./common/alert.component";
import { CookieService } from "./common/cookieService.component";

//TODO: delete
import { StartGameComponent } from "./game/startGame.component";
import { GameComponent } from "./game/game.component";
import { StartGameDialogComponent } from "./game/startGameDialog.component";
import { StartGameRedirectComponent } from "./game/startGameRedirect.component";
import { PassGameParametersService } from "./game/passGameParametersService";

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    FooterComponent,
    HomeComponent,
    AboutComponent,
    ImportReferenceIntervalsComponent,

    //TODO: delete
    StartGameComponent,
    GameComponent,
    StartGameDialogComponent,
    StartGameRedirectComponent,

    ConfirmDialogComponent,
    AlertDialogComponent
  ],
  imports: [
    BrowserModule.withServerTransition({appId: 'ng-cli-universal'}),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      {path: '', component: HomeComponent, pathMatch: 'full'},
      {path: 'game', component: GameComponent}, //TODO: delete
      {path: 'gameRedirect', component: StartGameRedirectComponent}, //TODO: delete
      {path: 'about', component: AboutComponent},
      {path: 'import/reference-intervals', component: ImportReferenceIntervalsComponent}
    ]),
    BrowserAnimationsModule,
    MatTooltipModule,
    MatDialogModule,
    MatButtonModule
  ],
  entryComponents: [
    StartGameDialogComponent, //TODO: delete

    ConfirmDialogComponent,
    AlertDialogComponent
  ],
  providers: [
    {provide: MAT_DIALOG_DEFAULT_OPTIONS, useValue: {hasBackdrop: true, disableClose: true}},
    CookieService,

    PassGameParametersService //TODO: delete
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
