import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from '../../environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
baseUrl = environment.apiUrl + 'auth/'; // 'http://localhost:5000/api/auth/';
jwtHelper = new JwtHelperService();
decodedToken: any;
currentUser: User; // user object sent back by the API after login
photoUrl = new BehaviorSubject<string>('../../assets/user.png'); // set intitial value of subject observable
currentPhotoUrl = this.photoUrl.asObservable();

constructor(private http: HttpClient) { }

  changeMemberPhoto(photoUrl: string) {
     this.photoUrl.next(photoUrl); // set value of Subject
  }

  login(model: any) {

    return this.http.post(this.baseUrl + 'login', model)
    .pipe(
      map((response: any) => {
         const apiResponse = response;
         if (apiResponse) {
           localStorage.setItem('token', apiResponse.token);
           localStorage.setItem('user', JSON.stringify(apiResponse.user)); // user is coming from API along with token
           this.decodedToken = this.jwtHelper.decodeToken(apiResponse.token);
           this.currentUser = apiResponse.user;
           this.changeMemberPhoto(this.currentUser.photoUrl);
         }
      })
    );
  }

  register(user: User) {
    return this.http.post(this.baseUrl + 'register', user);
  }

  loggedIn() {
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token); // will check if it is a valid token in addition
  }
}
