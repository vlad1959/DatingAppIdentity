import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_servicies/auth.service';
import { AlertifyService } from '../_servicies/alertify.service';
import { Router } from '@angular/router';


@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  photoUrl: string;

// made authService public in order to avoid error in html when referencing it
  constructor(public authService: AuthService, private alertify: AlertifyService, private router: Router) { }

  ngOnInit() {
    // get current phot Url from Subject observable
    this.authService.currentPhotoUrl.subscribe(photoUrl => this.photoUrl = photoUrl);
  }

  login() {
    this.authService.login(this.model).subscribe(next => {
      this.alertify.success('Logged in successfully');
    }, error => {
      this.alertify.error(error); // error should come from interceptor
    }, () => {
      this.router.navigate(['/members']);
    });
  }

    loggedIn() {
      return this.authService.loggedIn();
    }

    logout() {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      this.authService.decodedToken = null;
      this.authService.currentUser = null;
      this.model.username = '';
      this.model.userpassword = '';
      this.alertify.message('logged out');
      this.router.navigate(['/home']);
    }
}
