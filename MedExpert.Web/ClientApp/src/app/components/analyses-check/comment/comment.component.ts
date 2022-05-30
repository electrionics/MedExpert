import {Component, Input, OnInit} from '@angular/core';
import {IComment} from "../../../store/model/comment.model";

@Component({
  selector: 'app-comment',
  templateUrl: './comment.component.html',
  styleUrls: ['./comment.component.css']
})
export class CommentComponent implements OnInit {
  @Input()
  comment: IComment;

  @Input()
  specialistsMap: Map<number, string>;

  constructor() { }

  ngOnInit(): void {
  }

  public getSpecialistName(specialistId: number): string {
    if (!this.specialistsMap) {
      return;
    }
    return this.specialistsMap.get(specialistId);
  }

  public getCommentTypeName(commentTypeId: number): string {
    const commentTypeNamesMap = new Map<number, string>([
      [1, 'Диагноз'],
      [2, 'Показатель (измеренный)'],
      [3, 'Показатель (рекомендуемый)']
    ]);
    return commentTypeNamesMap.get(commentTypeId);
  }

}
