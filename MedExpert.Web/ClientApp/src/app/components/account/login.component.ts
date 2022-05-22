import {Component, Inject} from '@angular/core';
import {ApiService} from "../../services/api.service";
import {FormControl, Validators} from "@angular/forms";

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

  constructor(apiService: ApiService) {
    this.apiService = apiService;

    this.model = new LoginFormModel();

    this.Email = new FormControl(this.model.email, [Validators.required, Validators.email]);
    this.Password = new FormControl(this.model.password, [Validators.required]);
  }

  ngOnInit() {
    this.apiService.get<LoginFormModel>('Account/Login').subscribe(result =>{
      this.model = result;
    }, error => console.error(error) );
  }

  public login(){
    this.updateControls();

    if (this.Email.valid && this.Password.valid){
      this.apiService.post<LoginResultModel>('Account/Login', this.model).subscribe(result => {
        if (result.success){
          //result.RedirectUrl
        }
        else {
          this.errorMessage = result.errorMessage;
        }
      }, error => console.error(error) );
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

class LoginResultModel{
  success: boolean;
  errorMessage: string;
  redirectUrl: string;
}
