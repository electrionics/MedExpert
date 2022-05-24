import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {AnalysesCheckComponent} from './analyses-check.component';

const routes: Routes = [
  { path: '', component: AnalysesCheckComponent, pathMatch: 'full' } //analyses-check
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AnalysesCheckRoutingModule { }
