// app/core/auth/iflogged.component.ts
// app/core/components/navbar.component.ts
import {
  Directive, Input,
  OnDestroy,
  OnInit,
  TemplateRef,
  ViewContainerRef
} from "@angular/core";
import { Subject } from "rxjs";
import { distinctUntilChanged, map, takeUntil } from "rxjs/operators";
import { AuthService } from "../services/auth.service";

@Directive({
  selector: "[appIfLogged]"
})
export class IfLoggedDirective implements OnDestroy {
  private destroy$ = new Subject();

  constructor(
    private template: TemplateRef<any>,
    private view: ViewContainerRef,
    private authService: AuthService
  ) {}

  @Input() set appIfLogged(condition: boolean) {
    this.authService.isLogged$
      .pipe(
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(isLogged => {
        if (isLogged == condition) {
          this.view.createEmbeddedView(this.template);
        } else {
          this.view.clear();
        }
      });
  }

  ngOnDestroy() {
    this.destroy$.complete();
  }
}
