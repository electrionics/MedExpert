import {Component, Input, OnInit} from '@angular/core';

@Component({
  selector: 'app-progress-bar',
  templateUrl: './progress-bar.component.html',
  styleUrls: ['./progress-bar.component.css']
})
export class ProgressBarComponent implements OnInit {
  @Input()
  percentage: number;

  constructor() { }

  ngOnInit(): void {
  }

  get isGood(): boolean {
    return this.percentage < 33.33
  }

  get isNormal(): boolean {
    return this.percentage >= 33.33 && this.percentage < 66.66;
  }

  get isBad(): boolean {
    return this.percentage >= 66.66
  }

}
