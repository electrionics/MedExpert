import { Component } from '@angular/core';
import {AuthService} from "../../services/auth.service";
import {Router} from "@angular/router";

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;
  subMenuExpanded = false;

  constructor(private authService: AuthService, private router: Router) {
  }

  collapse() {
    this.isExpanded = false;
    this.subMenuExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  toggleSubMenu(){
    this.subMenuExpanded = !this.subMenuExpanded;
  }

  logout(){
    this.authService.logout();
  }
}
