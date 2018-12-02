import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_servicies/auth.service';
import { AlertifyService } from '../_servicies/alertify.service';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap';
import { User } from '../_models/user';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  // @Input() valuesFromHome: any;
  @Output() cancelRegister = new EventEmitter();

  user: User;

  registerForm: FormGroup;
  // Partial makes all the fields in a class optional, since we only want to change the color (theme)
  bsConfig: Partial<BsDatepickerConfig>;

  constructor(private authService: AuthService, private router: Router, 
    private alertify: AlertifyService, private fb: FormBuilder) { }

  ngOnInit() {
    this.bsConfig = {
      containerClass: 'theme-red'
    };
    this.createRegisterForm();
  }

  createRegisterForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: [null, Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      userpassword: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', Validators.required]
    }, {validator: this.passwordMatchValidator});
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('userpassword').value === g.get('confirmPassword').value ? null : {'mismatch': true};
  }
  register() {

    if (this.registerForm.valid) {
      this.user = Object.assign({}, this.registerForm.value);
      console.log(this.user);
    }

    this.authService.register(this.user).subscribe(() => {
      this.alertify.success('registration successeful');
    }, error => {
       this.alertify.error(error); // this will come from itereceptor
    }, () => {
       this.authService.login(this.user).subscribe(
         () => {this.router.navigate(['/members']); }
       );
    });

  }
  cancel() {
    this.cancelRegister.emit(false);
    console.log('cancel');
  }
}
