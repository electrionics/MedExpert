import {Component, Input, OnInit} from '@angular/core';

@Component({
  selector: 'app-progress-bar',
  templateUrl: './progress-bar.component.html',
  styleUrls: ['./progress-bar.component.css']
})
export class ProgressBarComponent implements OnInit {
  @Input()
  percentage: number;

  public shouldSetWidth: boolean;

  constructor() { }

  ngOnInit(): void {
    // need this in order to make css width animation to actually show
    // if we remove this and remove shouldSetWidth variable entirely, the progress bar will just show as in the end of animation
    // TODO find another solution for this, maybe use Angular animations
    /*setTimeout(() => {
      this.shouldSetWidth = true;
    }, 1000);*/
    this.shouldSetWidth = true;
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
