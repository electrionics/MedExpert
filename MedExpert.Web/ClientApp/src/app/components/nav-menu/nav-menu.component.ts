import { Component } from '@angular/core';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;
  subMenuExpanded = false;

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
}
