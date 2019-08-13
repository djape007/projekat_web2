import { Component, OnInit, forwardRef, Inject } from '@angular/core';
import { HomeComponent } from '../home/home.component';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { User } from 'src/models/user';
import { ProfileService } from '../services/profile.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {

  id: string;
  role: string;

  constructor(@Inject(forwardRef(() => HomeComponent)) private _parent: HomeComponent,
    private _auth: AuthService, private _router: Router) { }

  ngOnInit() {
    this._parent.prikaziDesniMeni();
    this.id = this._auth.getId();
    this.role = this._auth.getRole();
  }

  logout(){
    this._auth.logout();
    this._router.navigate(['/home/login']);
  }

}
