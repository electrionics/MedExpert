import {Component, Inject, OnDestroy, OnInit} from "@angular/core";
import {HttpClient} from "@angular/common/http";
import {MatDialog} from "@angular/material/dialog";
import {Router} from '@angular/router';
import {PassGameParametersService} from "./passGameParametersService";
import {CookieService} from "../common/cookieService.component";

@Component({
  selector: 'app-start-game',
  templateUrl: './startGame.component.html'
})
export class StartGameComponent implements OnInit, OnDestroy {
  private cookies: CookieService;
  public tutorialPassed: boolean;

  public difficultyLevel: number;
  public difficultyLevels: { type: number; label: string; }[];

  public totalLevels: number;
  public totalTasks: number;

  constructor(http: HttpClient, @Inject('BASE_API_URL') baseUrl: string, public dialog: MatDialog, private router: Router, public dataService: PassGameParametersService, cookies: CookieService) {
    this.difficultyLevels = [];

    this.difficultyLevels.push({type: 1, label: "Лёгкий" });
    this.difficultyLevels.push({type: 2, label: "Средний" });
    this.difficultyLevels.push({type: 3, label: "Тяжелый" });

    this.cookies = cookies;
    let cookie = this.cookies.getCookie('tutorial_passed');
    this.tutorialPassed = cookie == '1';

    http.get<TutorialDetailsModel>(baseUrl + 'Game/TutorialDetails').subscribe(result => {
      this.totalLevels = result.TotalLevels;
      this.totalTasks = result.TotalTasks;
    }, error => console.error(error));
  }

  public navigateStart(){
    this.router.navigateByUrl('/game');
  }

  public navigateTutorial(){
    this.difficultyLevel = 0;
    this.router.navigateByUrl('/tutorial');
  }

  ngOnInit() {
    const defaultDifficultyLevel = 1;

    if (this.dataService.parameter){
      this.difficultyLevel = this.dataService.parameter.difficultyLevel;
      if (!this.difficultyLevel){
        this.difficultyLevel = defaultDifficultyLevel;
      }
    }
    else{
      this.difficultyLevel = defaultDifficultyLevel;
    }
  }

  ngOnDestroy() {
    this.dataService.parameter = {
      difficultyLevel: this.difficultyLevel,
      totalTutorialLevels: this.totalLevels
    };
  }
}

interface TutorialDetailsModel{
  TotalLevels: number;
  TotalTasks: number;
}
