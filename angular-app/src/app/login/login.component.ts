import { Component, OnInit, Inject, forwardRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HomeComponent } from '../home/home.component';
import { User } from 'src/models/user';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  loginForm: FormGroup;
  message: string;

  constructor(@Inject(forwardRef(() => HomeComponent)) private _parent: HomeComponent,
    private formBuilder: FormBuilder, private _service: AuthService, private _router: Router) { }

  ngOnInit() {
    this._parent.prikaziDesniMeni();
    
    this.loginForm = this.formBuilder.group({
      email: ['', Validators.compose([Validators.required, Validators.email])],
      password: ['', Validators.required]
    });
  }

  get f() { return this.loginForm.controls; }

  clearMessage(){
    this.message = '';
  }

  login(){
    this.message = '';
    if (this.loginForm.invalid) {
        return;
    }
    else{
      var user = new User();
      user.Email = this.f.email.value;
      user.Password = this.f.password.value;
      this._service.login(user)
        .subscribe(data => {
        },
        error => {
          //console.log(error['error']['error_description']);
          this.message = error['error']['error_description'];
        })
    }   
  }
}
