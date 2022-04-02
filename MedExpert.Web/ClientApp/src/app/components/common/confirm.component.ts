import {Component, Inject} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm.component.html',
})
export class ConfirmDialogComponent {
  constructor(http: HttpClient, @Inject('BASE_API_URL') baseUrl: string, public dialogRef: MatDialogRef<ConfirmDialogComponent>, @Inject(MAT_DIALOG_DATA) public data: {
    content: string,
    cssClass: string
  }) {
    this.content = data.content;
    this.dialogReference = dialogRef;

    dialogRef.addPanelClass('confirm-dialog');
    if (data.cssClass) {
      dialogRef.addPanelClass(data.cssClass);
    }
  }

  public content: string;
  private dialogReference: MatDialogRef<ConfirmDialogComponent>;

  public ok(){
    this.dialogReference.close(true);
  }
}
