import { Component, OnInit, Input } from '@angular/core';
import { User } from '../../_models/user';
import { AuthService } from 'src/app/_servicies/auth.service';
import { UserService } from 'src/app/_servicies/user.service';
import { AlertifyService } from 'src/app/_servicies/alertify.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {

  @Input() user: User;

  constructor(private authService: AuthService, private userService: UserService, private alertify: AlertifyService) { }

  ngOnInit() {
  }

  sendLike(recipientId: number) {
    this.userService.sendLike(this.authService.decodedToken.nameid, recipientId).subscribe(
      data => {
        this.alertify.success('You have liked user ' + this.user.knownAs);
      }, error => {
        this.alertify.error(error);
      }
    );
  }
}
