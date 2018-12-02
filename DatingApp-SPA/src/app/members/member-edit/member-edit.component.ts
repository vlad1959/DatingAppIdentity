import { Component, OnInit, ViewChild, HostListener } from '@angular/core';
import { User } from 'src/app/_models/user';
import { ActivatedRoute } from '@angular/router';
import { AlertifyService } from 'src/app/_servicies/alertify.service';
import { NgForm } from '@angular/forms';
import { UserService } from 'src/app/_servicies/user.service';
import { AuthService } from 'src/app/_servicies/auth.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  user: User;
  photoUrl: string;

  @ViewChild('EditForm') editForm: NgForm;
  // listening to window unload event to prevent closing window with unsaved changes
  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any) {
     if (this.editForm.dirty) {
       event.returnValue = true; // this will stop unload
     }
  }

  constructor(private route: ActivatedRoute, private alertify: AlertifyService,
    private userService: UserService, private authService: AuthService) { }

  ngOnInit() {
    this.route.data.subscribe(
      data => {
        this.user = data.user;
      }
    );
    this.authService.currentPhotoUrl.subscribe(photoUrl => this.photoUrl = photoUrl); // get user's photo from subject observable
    console.log(this.user);
  }
  updateUser() {
    this.userService.updateUser(this.authService.decodedToken.nameid, this.user).subscribe(
      next => {
        this.alertify.success('Profile updated successfully');
        this.editForm.reset(this.user); // this will make the form pristine, not dirty and restore save values
      }, error => {
      this.alertify.error(error);
    });
  }

  updateMainPhoto(photoUrl) {
    this.user.photoUrl = photoUrl;
  }
}
