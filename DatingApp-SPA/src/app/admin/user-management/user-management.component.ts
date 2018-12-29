import { Component, OnInit } from '@angular/core';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_servicies/admin.service';
import { error } from '@angular/compiler/src/util';
import { BsModalService, BsModalRef } from 'ngx-bootstrap';
import { RolesModalComponent } from '../roles-modal/roles-modal.component';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {

  users: User[];

  bsModalRef: BsModalRef; // modal dialog

  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  ngOnInit() {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {

    this.adminService.getUsersWithRoles().subscribe((users: User[]) => {
        this.users = users;
      // tslint:disable-next-line:no-shadowed-variable
      }, error => {
        console.log(error);
      });
  }

  // open modal dialog to update roles
  editRolesModal(user: User) {
    const initialState = {
      user,
      roles: this.getRolesArray(user)
    };
    this.bsModalRef = this.modalService.show(RolesModalComponent, { initialState });
    // use content property to subscribe to event updateSelecetdRoles emitted by modal dialog
    this.bsModalRef.content.updateSelectedRoles.subscribe(
      (values) => {
        // create an array that contains only checked roles
        // and then extract only role names
        // '...' is spread operator that creates new array.
        const rolesToUpdate = {
          roleNames: [...values.filter(el => el.checked === true).map(el => el.name)]
        };

        if (rolesToUpdate) {
          this.adminService.updateUserRoles(user, rolesToUpdate).subscribe(
            () => {
              user.roles = [...rolesToUpdate.roleNames]; // this will update browser with new selected roles
            // tslint:disable-next-line:no-shadowed-variable
            }, error => {
              console.log(error);
            }
          );

        }
      }
    );
  }

  private getRolesArray(user: User) {
    const roles = [];
    const userRoles = user.roles;
    const availableRoles: any[] = [
      {name: 'Admin', value: 'Admin'},
      {name: 'Moderator', value: 'Moderator'},
      {name: 'Member', value: 'Member'},
      {name: 'VIP', value: 'VIP'}
    ];

    for (let i = 0; i < availableRoles.length; i++) {

      let isMatch = false;

      for (let j = 0; j < userRoles.length; j++) {
        if (availableRoles[i].name === userRoles[j]) {
          isMatch = true;
          availableRoles[i].checked = true;
          roles.push(availableRoles[i]);
          break;
        }
      }

      if (!isMatch) {
        availableRoles[i].checked = false;
        roles.push(availableRoles[i]);
      }
    }
    return roles;
  }
}
