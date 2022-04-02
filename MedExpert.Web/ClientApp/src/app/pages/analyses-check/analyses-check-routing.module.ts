import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {AnalysesCheckComponent} from './analyses-check.component';

const routes: Routes = [
  { path: 'analyses-check', component: AnalysesCheckComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AnalysesCheckRoutingModule { }
