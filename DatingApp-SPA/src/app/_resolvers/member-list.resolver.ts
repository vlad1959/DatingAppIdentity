import { Injectable } from '@angular/core' ;
import { User } from '../_models/user';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { UserService } from '../_servicies/user.service';
import { AlertifyService } from '../_servicies/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';


@Injectable() 
export class MemberListResolver implements Resolve<User[]> {
    pageNumber = 1; // default
    pageSize = 5; // fdefault

    constructor(private userService: UserService, private router: Router, private alertify: AlertifyService) {}

    resolve(route: ActivatedRouteSnapshot): Observable<User[]> {
        return this.userService.getUsers(this.pageNumber, this.pageSize).pipe(
            catchError(error => {
                this.alertify.error('Problem retrieving data');
                this.router.navigate(['/home']);
                return of(null); // observable of null
            })
        );
    }

}
