import { Component, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PassGameParametersService } from '../../services/passGameParametersService';

@Component({
  selector: 'app-start-game-redirect',
  template: ''
})
export class StartGameRedirectComponent {
  private difficultyLevel: number;

  constructor(private router: Router, private route: ActivatedRoute, private dataService: PassGameParametersService) {
    this.difficultyLevel = this.dataService.parameter.difficultyLevel;

    this.route.queryParams.subscribe(params => {
        this.difficultyLevel = params.difficultyLevel;
      });
    this.router.navigateByUrl('/game');
  }

  ngOnDestroy() {
    this.dataService.parameter = {
      difficultyLevel: this.difficultyLevel,
      totalTutorialLevels: null
    };
  }
}
