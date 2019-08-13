import { Component, OnInit, Inject, forwardRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HomeComponent } from '../home/home.component';
import { User } from 'src/models/user';
import { ProfileService } from '../services/profile.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  registerForm: FormGroup;
  selectedValue: string;
  selectedFile: File;
  imgURL: any;

  constructor(@Inject(forwardRef(() => HomeComponent)) private _parent: HomeComponent,
  private formBuilder: FormBuilder, private _service: ProfileService, private _router: Router) { }

  ngOnInit() {
    this._parent.prikaziDesniMeni();

    this.registerForm = this.formBuilder.group({
      email: ['', Validators.compose([Validators.required, Validators.email])],
      name: ['', Validators.compose([Validators.required, Validators.minLength(2)])],
      password: ['', Validators.compose([Validators.required, Validators.minLength(6)])],
      repeat_password: ['', Validators.required],
      address: ['', Validators.compose([Validators.required, Validators.minLength(2)])],
      surname: ['', Validators.compose([Validators.required, Validators.minLength(2)])],
      type: ['', Validators.required],
      birthday: ['', Validators.required],
    });
  }

  get f() { return this.registerForm.controls; }

  isSameAsPassword(){
    if (this.f.password.value != this.f.repeat_password.value)
      this.f.repeat_password.setErrors({'notsame': true});
  }

  selectChange(){
    this.selectedValue = this.registerForm.get('type').value
  }

  onFileChanged(event) {
    this.selectedFile = event.target.files[0];

    var reader = new FileReader();
    reader.readAsDataURL(this.selectedFile); 
    reader.onload = (_event) => { 
      this.imgURL = reader.result; 
    }
  }

  register(){
    if (this.registerForm.invalid) {
      return;
    }

    var user = new User();
    user.Email = this.f.email.value;
    user.Dob = this.f.birthday.value;
    user.Name = this.f.name.value;
    user.Surname = this.f.surname.value;
    user.Address = this.f.address.value;
    user.Password = this.f.password.value;
    user.Type = this.f.type.value;

    var arrMonthDayYear = user.Dob.toLocaleDateString().split('/');
    var dobString = `${arrMonthDayYear[2]}-${arrMonthDayYear[0]}-${arrMonthDayYear[1]}`;

    user['DateOfBirth'] = dobString;
    this._service.register(user, this.selectedFile)
      .subscribe(
        data => {
          this._router.navigate(['/home/login']);
        },
        err =>{
          console.log(err);
        }
      )
  }
}
