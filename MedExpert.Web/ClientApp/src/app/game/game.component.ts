import {Component, Inject, OnDestroy, OnInit} from '@angular/core';
import {Router} from '@angular/router';
import {HttpClient} from '@angular/common/http';
import {MatDialog} from '@angular/material/dialog';
import {ConfirmDialogComponent} from "../common/confirm.component";
import {AlertDialogComponent} from "../common/alert.component";
import {PassGameParametersService} from "./passGameParametersService";
import {StartGameDialogComponent} from './startGameDialog.component';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html'
})
export class GameComponent implements OnInit, OnDestroy {
  private http: HttpClient;
  private readonly baseUrl: string;

  protected dialog: MatDialog;
  protected dataService: PassGameParametersService;
  protected router: Router;

  protected timerId: number;

  public regularityTypes: { type: number; enabled: boolean; label: string; shortLabel: string; regularityNumberHint: string; }[];
  public difficultyLevels: { type: number; label: string; timer: number; bonusTime: number; }[];

  public startGame: StartModel;
  public game: ProgressModel;
  public endGame: EndModel;

  public difficultyLevel: number;
  public hintsEnabled: boolean;
  public understandDescription: boolean;

  public number: { value: number; selected: boolean; disabled: boolean; }[];
  public positions: { value: number; selected: boolean; }[];

  public timerSet: boolean;
  public requestProgress: boolean;

  constructor(http: HttpClient, @Inject('BASE_API_URL') baseUrl: string, dialog: MatDialog, router: Router, dataService: PassGameParametersService) {
    this.initLists();

    this.dialog = dialog;
    this.router = router;
    this.dataService = dataService;

    this.timerSet = false;
    this.timerId = null;

    this.http = http;
    this.baseUrl = baseUrl;

    this.requestProgress = false;
  }

  ngOnInit() {
    if (this.dataService.parameter){
      this.difficultyLevel = this.dataService.parameter.difficultyLevel;

      this.start();
    }
    else{
      this.dataService.parameter = {
        difficultyLevel: null,
        totalTutorialLevels: null
      };
      this.router.navigateByUrl('/');
    }
  }

  ngOnDestroy() {
    this.dataService.parameter = {
      difficultyLevel: this.difficultyLevel,
      totalTutorialLevels: null
    };
    if (this.timerId !== null){
      clearInterval(this.timerId);
    }
  }


  public start(){
    let url = this.getStartUrl();
    if (!this.requestProgress) {
      this.requestProgress = true;
      this.http.get<StartModel>(url).subscribe(result => {
        this.successStart(result);
      }, error => console.error(error), () => this.requestProgress = false );
    }
  }

  protected getStartUrl(){
    return this.baseUrl + 'Game/Start?difficultyLevel=' + this.difficultyLevel;
  }

  protected successStart(result){
    this.hintsEnabled = true;
    this.understandDescription = false;
    this.endGame = null;
    this.startGame = result;
    this.game = new ProgressModel();
    this.game.ProgressRegularityInfos = {};
    this.game.Score = 0;

    this.number = [];

    let tempNumber = result.Number;
    while(tempNumber > 0){
      let currentDigit = tempNumber % 10;
      tempNumber = (tempNumber - currentDigit) / 10;

      this.number.push({value: currentDigit, selected: false, disabled: false });
    }
    this.number = this.number.reverse();

    this.positions = [];
    for(let pos = 0; pos < this.startGame.Length; pos++){
      this.positions.push({value: pos, selected: false});
    }

    this.regularityTypes.map(x => x.type).forEach(type => {
      this.game.ProgressRegularityInfos[type] = [];
      this.startGame.ExistRegularityInfos.filter(x => x.Type == type).forEach(regularityInfo =>{
        let progressInfo = new ProgressRegularityInfo();
        progressInfo.FoundStatus = FoundStatus.NotFound;
        progressInfo.Numbers = null;
        progressInfo.RegularityNumber = regularityInfo.RegularityNumber;
        progressInfo.ReverseRegularityNumber = regularityInfo.ReverseRegularityNumber;

        this.game.ProgressRegularityInfos[type].push(progressInfo);
      });
    });

    this.game.TimerSeconds = this.getCurrentDifficultyLevel().timer;
    if (!this.timerSet){
      if (this.timerId !== null){
        clearInterval(this.timerId);
      }
      this.timerId = setInterval(() => {
        if (this.game && this.game.TimerSeconds && this.game.TimerSeconds > 0 && !this.endGame){
          if (--this.game.TimerSeconds == 0){
            this.end(false);
          }
        }
      }, 1000);
      this.timerSet = true;
    }
  }


  public toggleSelected(digit, index){
    if (this.endGame) return;

    digit.selected = !digit.selected;
    for(let pos = 0; pos < this.number.length; pos++){
      if (this.number[pos] === digit){
        this.positions[pos].selected = digit.selected;
      }
    }
  }


  public check(regularityType){
    let selectedPositions = this.positions.filter(x => x.selected).map(x => x.value);
    if (selectedPositions.length > 1){
      if (!this.requestProgress){
        this.requestProgress = true;
        this.http.post<CheckResultModel>(this.baseUrl + 'Game/Check', {
          GameId: this.startGame.GameId,
          Type: regularityType,
          Positions: selectedPositions
        }).subscribe(result => {
          if (result.Match){
            let progressInfos = this.game.ProgressRegularityInfos[regularityType];
            let info = progressInfos.find(x => x.RegularityNumber == result.RegularityNumber && x.FoundStatus == FoundStatus.NotFound);

            info.Numbers = result.FoundNumbers;
            info.FoundStatus = result.Hinted ? FoundStatus.Hinted : FoundStatus.Found;

            this.alertDialog(result.PointsAdded + ' очков заработано!', 'Поздравляем!', 'points-added-dialog', () =>{
              this.successCongratulations(result, regularityType);
            });
          }
          else {
            let hintMessage = GameComponent.composeHintMessage(result);

            if (hintMessage){
              this.alertDialog(hintMessage, 'Автоподсказка!', null, () =>{
                this.successAutoHint(regularityType);
              });
            }
          }
        }, error => console.error(error), () => this.requestProgress = false );
      }
    }
    else {
      this.alertDialog("Выберите минимум 2 цифры.", 'Внимание!');
    }
  }

  protected successCongratulations(result, regularityType){
    this.game.Score = result.NewTotalPoints;

    if (!result.Hinted){
      this.game.TimerSeconds += this.getCurrentDifficultyLevel().bonusTime;
    }

    this.clearSelections();

    if (!this.regularityTypes.some(x => this.game.ProgressRegularityInfos[x.type].some(info => info.FoundStatus == FoundStatus.NotFound))){
      this.end(false);
    }
  }

  protected successAutoHint(regularityType){
  }


  public end(needConfirm){
    let self = this;
    let endBody = function(){
      if (!self.requestProgress) {
        self.requestProgress = true;
        self.http.post<EndModel>(self.baseUrl + 'Game/End?gameId=' + self.startGame.GameId + '&remainingSeconds=' + self.game.TimerSeconds, null).subscribe(result => {
          self.successEnd(result);
          self.successEndOpenUpdateDialog();
        }, error => console.error(error), () => self.requestProgress = false );
      }
    }

    if (!this.endGame){
      if (!needConfirm){
        endBody();
      }
      else{
        this.confirmDialog('Вы уверены, что хотите завершить игру?', () => {
          endBody();
        });
      }
    }
  }

  protected successEnd(result){
    this.endGame = result;

    this.game.Score = this.endGame.TotalScore;

    this.clearSelections();
  }

  protected successEndOpenUpdateDialog(){
    // this.dialog.open(UpdateRecordDialogComponent, {
    //   data: {
    //     gameId: this.startGame.GameId,
    //     totalScore: this.endGame.TotalScore,
    //     spentMinutes: this.endGame.SpentMinutes,
    //     spentSeconds: this.endGame.SpentSeconds
    //   }
    // }).afterClosed().subscribe(result => {
    //   this.showNotFound();
    // });
  }


  public endSession(){
    this.confirmDialog('Завершить сеанс и вернуться на главную страницу?', () => {
      this.successEndSession();
    });
  }

  public newGame() {
    this.dialog.open(StartGameDialogComponent, {
      data: {
        previousDifficultyLevel: this.difficultyLevel
      }
    });
  }

  protected successEndSession() {
    this.startGame = null;
    this.game = null;
    this.number = null;
    this.positions = null;

    this.router.navigateByUrl('/');
  }


  public showNotFound(){
    if (this.endGame.NotFoundRegularityInfos.length){
      this.confirmDialog('Показать ненайденные закономерности?', () =>{
        this.successShowNotFound();
      });
    }
    else{
      this.endGame.HintsIterated = true;
    }
  }

  protected successShowNotFound(){
    this.endGame.NotFoundRegularityInfos.forEach(hint => {
      let progressInfos = this.game.ProgressRegularityInfos[hint.Type];
      let info = progressInfos.find(x => x.RegularityNumber == hint.RegularityNumber && x.FoundStatus == FoundStatus.NotFound);

      if (info) {
        info.Numbers = hint.Numbers;
        info.FoundStatus = FoundStatus.Hinted;
      }
    });
    this.endGame.HintsIterated = true;
  }


  public toggleHints(){
    this.hintsEnabled = !this.hintsEnabled;
  }


  public startHintMode(regularityType, regularityNumber, needConfirm){
    let self = this;
    let startHintModeBody = function (regularityType, regularityNumber){
      if (!self.requestProgress) {
        self.requestProgress = true;
        self.http.post<HintResultModel>(self.baseUrl + 'Game/Hint', {
          GameId: self.startGame.GameId,
          Type: regularityType,
          RegularityNumber: regularityNumber
        }).subscribe(result => {
          if (result) {
            self.successHintMode(result, regularityType);
          }
        }, error => console.error(error), () => self.requestProgress = false );
      }
    }

    if (!needConfirm){
      startHintModeBody(regularityType, regularityNumber);
    }
    else{
      this.confirmDialog('Вам нужна подсказка?', () =>{
        startHintModeBody(regularityType, regularityNumber);
      })
    }
  }

  protected successHintMode(result, regularityType){
    this.clearSelections();

    for (let pos = 0; pos < this.startGame.Length; pos++) {
      let selected = result.Numbers.some(n => {
        for (var i = 0; i < n.Length; i++){
          if (n.Position + i == pos){
            return true;
          }
        }

        return false;
      });

      this.positions[pos].selected = selected;
      this.number[pos].selected = selected;
      this.number[pos].disabled = true;
    }

    this.regularityTypes.forEach(regType => {
      regType.enabled = result.Type == regType.type;
    });
  }


  public understand(){
    this.understandDescription = true;
  }


  public toggleTooltip(tooltip, index){
    tooltip.toggle();
  }


  public filterByFound(progressRegularityInfos, type){
    let infos = progressRegularityInfos[type];
    if (!infos) {
      return [];
    }

    return infos.filter(x => x.FoundStatus == FoundStatus.Hinted || x.FoundStatus == FoundStatus.Found);
  }


  public filterByExists(regularityTypes){
    return regularityTypes.filter(regType => this.startGame.ExistRegularityTypeCounts[regType.type]);
  }


  public getCurrentDifficultyLevel(){
    let result;
    this.difficultyLevels.forEach((item) =>{
      if (item.type == this.difficultyLevel){
        result = item;
      }
    });

    return result;
  }


  public getCurrentLength(numbers: FoundNumber[], position){
    for (let i = 0; i < numbers.length; i++){
      if (numbers[i].Position == position){
        return {
          value: numbers[i].Length,
          exist: true
        }; // colspan cell
      }
      else if (numbers[i].Position < position && numbers[i].Position + numbers[i].Length > position){
        return {
          value: 0,
          exist: true
        }; // skip cell
      }
    }

    return {
      value: 1,
      exist: false
    }; // empty cell
  }


  public range(length){
    let result = [];
    for (let i = 0; i < length; i++){
      result.push(i);
    }
    return result;
  }


  public getHintClass(status){
    return status == FoundStatus.Found
      ? 'plus'
      : status == FoundStatus.NotFound
        ? 'minus'
        : 'hint';
  }


  public getHintChar(status){
    return status == FoundStatus.NotFound ? '-' : '+';
  }


  public getHintDisabled(status) {
    return status != FoundStatus.NotFound;
  }


  public getAnyDisabled(regularityTypes){
    return regularityTypes.some(x => !x.enabled);
  }


  private static composeHintMessage(result){
    let digitsToAddMessage = result.AddHint == CheckHint.No
      ? '' :
      result.AddHint == CheckHint.AddOneDigit
        ? 'добавьте одну цифру'
        : result.AddHint == CheckHint.AddMoreThanOneDigit
        ? 'добавьте несколько цифр'
        : '';
    let digitsToRemoveMessage = result.RemoveHint == CheckHint.No
      ? '' :
      result.RemoveHint == CheckHint.RemoveOneDigit
        ? 'удалите одну цифру'
        : result.RemoveHint == CheckHint.RemoveMoreThanOneDigit
        ? 'удалите несколько цифр'
        : '';

    return digitsToAddMessage == ''
      ? digitsToRemoveMessage
      : digitsToAddMessage + (digitsToRemoveMessage == ''
      ? ''
      : ', ' + digitsToRemoveMessage);
  }


  private initLists(){
    this.regularityTypes = [];
    this.regularityTypes.push({type: 1, enabled: true, label: "Одинаковые цифры", regularityNumberHint: "Количество цифр в закономерности. Например, для числа '333' будет число-подсказка, равное 3.", shortLabel: "ОЦ"});
    this.regularityTypes.push({type: 2, enabled: true, label: "Одинаковые числа (минимум 2-значные)", regularityNumberHint: "Количество чисел в закономерности. Например, для числа '4040' будет число-подсказка, равное 2: два числа, равных 40).", shortLabel: "ОЧ"});
    this.regularityTypes.push({type: 3, enabled: true, label: "Зеркальные цифры", regularityNumberHint: "Количество цифр между внутренними крайними цифрами. Например, для числа '13831' будет числа-подсказка, равное 1: между тройками одна цифра - 8).", shortLabel: "ЗЦ"});
    this.regularityTypes.push({type: 4, enabled: true, label: "Кратные числа (минимум 2-значные)", regularityNumberHint: "Кратность чисел - большее число разделить на меньшее. Например, для числа '8421' будет число-подсказка, равное 84/21 = 4.", shortLabel: "КЧ"});
    this.regularityTypes.push({type: 5, enabled: true, label: "Арифметическая прогрессия", regularityNumberHint: "Шаг (разность) прогрессии. Например, для числа '234' будет число-подсказка, 4-3 = 3-2 = 1.", shortLabel: "АП"});
    this.regularityTypes.push({type: 6, enabled: true, label: "Геометрическая прогрессия", regularityNumberHint: "Знаменатель прогрессии. Например, для числа '1684' будет число-подсказка, 4/8 = 8/16 = 1/2.", shortLabel: "ГП"});

    this.difficultyLevels = [];
    this.difficultyLevels.push({type: 0, label: "Обучение", timer: 600, bonusTime: 10 });
    this.difficultyLevels.push({type: 1, label: "Лёгкий", timer: 180, bonusTime: 10 });
    this.difficultyLevels.push({type: 2, label: "Средний", timer: 240, bonusTime: 9 });
    this.difficultyLevels.push({type: 3, label: "Тяжелый", timer: 300, bonusTime: 8 });
  }


  private clearSelections(){
    this.positions.forEach(pos => {
      pos.selected = false;
    });
    this.number.forEach(num => {
      num.selected = false;
      num.disabled = false;
    });
    this.regularityTypes.forEach(regType => {
      regType.enabled = true;
    });
  }


  private confirmDialog(message, successCallback){
    this.dialog.open(ConfirmDialogComponent, {
      data: {
        content: message
      },
      autoFocus: true
    }).afterClosed().subscribe(result => {
      if (result){
        successCallback();
      }
    });
  }


  protected alertDialog(message, title = null, cssClass = null, successCallback = null){
    this.dialog.open(AlertDialogComponent, {
      data: {
        title: title,
        content: message,
        cssClass: cssClass
      },
      autoFocus: true
    }).afterClosed().subscribe(result => {
      if (successCallback) {
        successCallback();
      }
    });
  }


  /* tutorial */

  public currentLevel: TutorialLevel;
  public countLevels: number;
  public currentTaskIndex: number;
  public tasks: { controlName: string, anySubtask: boolean, subtasks: number[], additionalCondition: Predicate<number, number>, text: string}[]; // if not any - then all


  public rowClick(regularityType){
  }

  public showCurrentTask(){
  }

  public getTutorialClass(controlName, subIndex = 0, type = null) {
    return null;
  }

  public getOverOverlayClass(controlName, subIndex = 0, type = null){
    return null;
  }

  public isTutorialActiveElement(controlName, subIndex = 0, type = null) {
    return false;
  }
}

interface StartModel {
  GameId: string;
  Number: number;
  Length: number;
  DifficultyLevel: number;
  ExistRegularityInfos: StartRegularityInfo[];
  ExistRegularityTypeCounts: { [key: number]: number };
  TutorialLevel: TutorialLevel;
}

interface StartRegularityInfo {
  Type: number;
  RegularityNumber: number;
  ReverseRegularityNumber: number;
}

class ProgressModel
{
  ProgressRegularityInfos: { [type: number]: ProgressRegularityInfo[] }
  Score: number;
  TimerSeconds: number;
}

class ProgressRegularityInfo
{
  RegularityNumber: number;
  ReverseRegularityNumber: number;
  FoundStatus: FoundStatus;
  Numbers: FoundNumber[];
}

class FoundNumber{
  Position: number;
  Length: number;
}

interface EndModel {
  TotalScore: number;
  SpentMinutes: number;
  SpentSeconds: number;
  NotFoundRegularityInfos: HintResultModel[];
  FoundRegularityInfos: EndRegularityInfo[];
  HintsIterated: boolean;
}

interface EndRegularityInfo {
  Type: number;
  Count: number;
}

interface HintResultModel{
  Numbers: FoundNumber[];
  Type: number;
  RegularityNumber: number;
}

interface CheckResultModel {
  RegularityNumber: number;
  Match: boolean;
  Hinted: boolean;
  PointsAdded: number;
  NewTotalPoints: number;
  AddHint: CheckHint;
  RemoveHint: CheckHint;
  FoundNumbers: FoundNumber[];
}

enum CheckHint{
  No = 0,
  AddOneDigit = 11,
  AddMoreThanOneDigit = 12,
  RemoveOneDigit = 21,
  RemoveMoreThanOneDigit = 22
}

enum FoundStatus{
  Found = 1,
  Hinted = 2,
  NotFound = 3
}

interface TutorialLevel{
  Level: number;
  Title: string;
  Text: string;
  Tasks: TutorialTask[];
}

interface TutorialTask {
  Order: number;
  Text: string;
  Name: string;
  AnySubtask: boolean;
  Subtasks: number[];
  ApplyCondition: string;
  ConditionParameter: string;
}

interface Predicate<T1, T2> {
  (value1: T1, value2: T2): boolean;
}
