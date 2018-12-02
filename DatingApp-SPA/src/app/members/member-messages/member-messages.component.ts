import { Component, OnInit, Input } from '@angular/core';
import { Message } from 'src/app/_models/message';
import { AuthService } from 'src/app/_servicies/auth.service';
import { UserService } from 'src/app/_servicies/user.service';
import { AlertifyService } from 'src/app/_servicies/alertify.service';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {

  @Input() recipientId: number;
  messages: Message[];
  newMessage: any = {};

  constructor(private userService: UserService, private authService: AuthService,
              private alertify: AlertifyService) { }

  ngOnInit() {
    this.loadMessages();
  }

  loadMessages() {
    const currentUserId = +this.authService.decodedToken.nameid; // '+' will make it a number type
    this.userService.getMessageThread(this.authService.decodedToken.nameid, this.recipientId)
      .pipe(
        // tap means do something before you subscribe
        tap(messages => {
          for (let i = 0; i < messages.length; i++) {
              if (messages[i].isRead === false && messages[i].recipientId === currentUserId) {
                this.userService.markAsRead(currentUserId, messages[i].id);
              }
          }
        })
      )
      .subscribe(messages => {
        this.messages = messages;
      }, error => {
        this.alertify.error(error);
      });
  }

  sendMessage() {
    this.newMessage.recipientId = this.recipientId;
    this.userService.sendMessage(this.authService.decodedToken.nameid, this.newMessage).subscribe(
      (message: Message) => {
        // debugger; //should open Chrome developer tools to see results
        this.messages.unshift(message); // unshift like push, but at the beginning of array
        this.newMessage.content = '';
      }, error => {
        this.alertify.error(error);
      });
  }
}
