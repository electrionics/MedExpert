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

  get backgroundColor(): string {
    // returns background color for respective percentage.

    // 0% color is green: #9ae98f = rgb(154, 233, 143) = hsl(113, 67%, 73%)
    // 100% color is red: #e98f8f = rgb(233, 143, 143) = hsl(0, 67%, 73%)
    // saturation and lightness doesn't depend on percentage
    const saturation = 67, lightness = 73;
    // hue does depend on percentage
    const minPercentageHue = 113; // green (for 0% percentage)
    const maxPercentageHue = 0; // red (for 100% percentage)
    const hueRange = minPercentageHue - maxPercentageHue;
    // the more percentage is, smaller the hue value
    const hue = minPercentageHue - Math.round(hueRange * this.percentage / 100);
    // return background color in HSL format;
    return `hsl(${hue}, ${saturation}%, ${lightness}%)`;
  }

}
