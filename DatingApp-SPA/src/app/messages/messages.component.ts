import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination, PaginatedResult } from '../_models/pagination';
import { AuthService } from '../_servicies/auth.service';
import { UserService } from '../_servicies/user.service';
import { ActivatedRoute } from '@angular/router';
import { AlertifyService } from '../_servicies/alertify.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[];
  pagination: Pagination;
  messageContainer = 'Unread';

  constructor(private authService: AuthService, private userService: UserService,
    private route: ActivatedRoute, private alertify: AlertifyService) { }

  ngOnInit() {
    this.route.data.subscribe(
      data => {
        this.messages = data['messages'].result; // this comes from class PaginatedResult
        this.pagination = data['messages'].pagination; // this is sent by api
      }
    );
  }
  loadMessages() {
    this.userService.getMessages(this.authService.decodedToken.nameid, this.pagination.currentPage,
      this.pagination.itemsPerPage, this.messageContainer)
      .subscribe((res: PaginatedResult<Message[]>) => {
        this.messages = res.result;
        this.pagination = res.pagination;
        // console.log(this.messages);
      }, error => {
        this.alertify.error(error);
      });
  }
  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    this.loadMessages();
  }

  deleteMessage(id: number) {
    this.alertify.confirm('Are you sure you want to delete this message ?',  () => {
    this.userService.deleteMessage(id, this.authService.decodedToken.nameid).subscribe(
      () => {
        this.messages.splice(this.messages.findIndex(m => m.id === id), 1);
        this.alertify.success('Message has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the message');
      });
    });
  }
}