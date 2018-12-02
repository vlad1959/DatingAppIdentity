import { Injectable } from '@angular/core';
import { User } from '../_models/user';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { UserService } from '../_servicies/user.service';
import { AlertifyService } from '../_servicies/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';


@Injectable()
// this class is for the list of users who like a logged in user or users that logged in user like
export class ListsResolver implements Resolve<User[]> {
    pageNumber = 1; // default
    pageSize = 5; // default
    likesParam = 'Likers'; // this is a list of user who like currently logged in user

    constructor(private userService: UserService, private router: Router, private alertify: AlertifyService) { }

    resolve(route: ActivatedRouteSnapshot): Observable<User[]> {
        return this.userService.getUsers(this.pageNumber, this.pageSize, null, this.likesParam).pipe(
            catchError(error => {
                this.alertify.error('Problem retrieving data');
                this.router.navigate(['/home']);
                return of(null); // observable of null
            })
        );
    }

}
