import {Component, Inject} from '@angular/core';
import {ApiService} from "../../services/api.service";
import {FormControl, Validators} from "@angular/forms";
import {ActivatedRoute, Router} from "@angular/router";
import {AuthService} from "../../services/auth.service";
import {filter} from 'rxjs/operators';

@Component({
  selector: 'app-import-analysis',
  templateUrl: './login.component.html'
})
export class LoginComponent{
  private readonly apiService: ApiService;

  public model: LoginFormModel;
  public errorMessage: string;

  public Email: FormControl;
  public Password: FormControl;

  constructor(apiService: ApiService, private router: Router, private authService: AuthService, private route: ActivatedRoute) {
    this.apiService = apiService;

    this.model = new LoginFormModel();

    this.Email = new FormControl(this.model.email, [Validators.required, Validators.email]);
    this.Password = new FormControl(this.model.password, [Validators.required]);
  }

  ngOnInit() {
    this.route.queryParams
      .pipe(
        filter(params => params.returnUrl))
      .subscribe(params => {
        this.apiService.get<LoginFormModel>('Account/Login?returnUrl=' + params.returnUrl).subscribe(result =>{
          this.model = result;
        }, error => console.error(error) );
    });
  }

  public login(){
    this.updateControls();

    if (this.Email.valid && this.Password.valid){
      this.authService.login(this.model, (result) => {
        if (result.success){
          if (!result.redirectUrl){
            this.router.navigate(['/import/symptoms']);
          }
          else{
            this.router.navigate([result.redirectUrl]);
          }
        }
        else {
          this.errorMessage = result.errorMessage;
        }
      });
    }
  }

  private updateControls(){
    this.Email.setValue(this.model.email);
    this.Password.setValue(this.model.password);
    this.Email.markAsDirty();
    this.Password.markAsDirty();
    this.Email.updateValueAndValidity();
    this.Password.updateValueAndValidity();
  }
}

class LoginFormModel{
  email: string;
  password: string;
  rememberMe: boolean;
  externalLogins: AuthenticationScheme[];
  returnUrl: string;
}

interface AuthenticationScheme{
  Name: string;
  DisplayName: string;
}
