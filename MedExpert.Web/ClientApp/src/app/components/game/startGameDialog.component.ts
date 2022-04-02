import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { HttpClient } from "@angular/common/http";
import { Router } from '@angular/router';
import { PassGameParametersService } from "../../services/passGameParametersService";

@Component({
  selector: 'app-start-game-dialog',
  templateUrl: './startGameDialog.component.html',
})
export class StartGameDialogComponent {

  public difficultyLevel: number;
  public difficultyLevels: { type: number; label: string; }[];
  public dialogReference: MatDialogRef<StartGameDialogComponent>;

  constructor(dialogRef: MatDialogRef<StartGameDialogComponent>, private router: Router, public dataService: PassGameParametersService, @Inject(MAT_DIALOG_DATA) public data: {
    previousDifficultyLevel: number
  }) {
    dialogRef.addPanelClass('start-game-dialog');

    this.dialogReference = dialogRef;
    this.difficultyLevels = [];

    this.difficultyLevels.push({ type: 1, label: "Лёгкий" });
    this.difficultyLevels.push({ type: 2, label: "Средний" });
    this.difficultyLevels.push({ type: 3, label: "Тяжелый" });
  }

  ngOnInit() {
    const defaultDifficultyLevel = this.data.previousDifficultyLevel;

    if (this.dataService.parameter) {
      this.difficultyLevel = this.dataService.parameter.difficultyLevel;
      if (!this.difficultyLevel) {
        this.difficultyLevel = defaultDifficultyLevel;
      }
    }
    else {
      this.difficultyLevel = defaultDifficultyLevel;
    }
  }

  public navigateStart() {
    this.dialogReference.close(true);
    this.router.navigateByUrl('/gameRedirect?difficultyLevel=' + this.difficultyLevel);
  }
}
