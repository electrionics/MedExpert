import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AnalysesCheckRoutingModule } from './analyses-check-routing.module';
import { AnalysesCheckComponent } from './analyses-check.component';
import {ReactiveFormsModule} from '@angular/forms';
import {NgSelectModule} from '@ng-select/ng-select';

@NgModule({
  declarations: [
    AnalysesCheckComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AnalysesCheckRoutingModule,
    NgSelectModule
  ]
})
export class AnalysesCheckModule { }
