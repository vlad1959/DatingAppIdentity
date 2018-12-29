import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from '../_servicies/auth.service';
import { AlertifyService } from '../_servicies/alertify.service';


@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router, private alertify: AlertifyService){}

  canActivate(next: ActivatedRouteSnapshot): boolean {

    // need firstChild since all routes are children of '' route
    // data attribute with array of roles is added to routes.ts from admin route
    const roles = next.firstChild.data['roles'] as Array<string>; // 'Admim', 'Moderator'

    // checks if a route has any roles supplied to this guard and if it does checks if a user has one ofthese roles
    if (roles) {
      const match = this.authService.roleMatch(roles);
      if (match) {
        return true;
      } else {
         this.router.navigate(['members']);
         this.alertify.error('You are not authorised to access this area');
      }
    }

    if (this.authService.loggedIn()) {
      return true;
    }

    this.alertify.error('You shal not pass!!!');
    this.router.navigate(['/home']);
    return false;
  }
}
