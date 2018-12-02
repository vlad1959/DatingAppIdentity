import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { FileUploader } from 'ng2-file-upload';
import { environment } from 'src/environments/environment';
import { AuthService } from 'src/app/_servicies/auth.service';
import { UserService } from 'src/app/_servicies/user.service';
import { AlertifyService } from 'src/app/_servicies/alertify.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() photos: Photo[];
  @Output() getMemberPhotoChanged = new EventEmitter<string>(); // signal that main photo has changed, sent new main photo URL
  uploader: FileUploader;
  public hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  currentMainPhoto: Photo;

  constructor(private authService: AuthService, private userService: UserService,
    private alertify: AlertifyService) { }

  ngOnInit() {
    this.initializeUploader();
  }

  fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  // upload to API URL
  initializeUploader() {
    this.uploader = new FileUploader({
        url: this.baseUrl + 'users/' + this.authService.decodedToken.nameid + '/photos',
        authToken: 'Bearer ' + localStorage.getItem('token'),
        allowedFileType: ['image'],
        removeAfterUpload: true,
        autoUpload: false,
        maxFileSize: 10 * 1024 * 1024
    });

    this.uploader.onAfterAddingFile = (file) => {file.withCredentials = false; };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        // photo object comes from API after photo is uploaded
        const res: Photo = JSON.parse(response); // converted string to object 
        const photo = {
          id: res.id,
          url: res.url,
          dateAdded: res.dateAdded,
          description: res.description,
          isMain: res.isMain
        };
        this.photos.push(photo);
        if (photo.isMain) {
          this.authService.changeMemberPhoto(photo.url);
          this.authService.currentUser.photoUrl = photo.url;
          localStorage.setItem('user', JSON.stringify(this.authService.currentUser));
        }
      }
    };
  }

  setMainPhoto(photo: Photo) {
    this.userService.setMainPhoto(this.authService.decodedToken.nameid, photo.id).subscribe(
      () => {
        // console.log('Successfully set to main');
        this.currentMainPhoto = this.photos.filter(p => p.isMain === true)[0];
        this.currentMainPhoto.isMain = false;
        photo.isMain = true;
        // this.getMemberPhotoChanged.emit(photo.url); //this is replced with subject observable
        this.authService.changeMemberPhoto(photo.url);
        this.authService.currentUser.photoUrl = photo.url;
        // update local storage so that when photo is changed and browser is refreshed 
        // it persists the change
        localStorage.setItem('user', JSON.stringify(this.authService.currentUser));
      }, error => {
        this.alertify.error(error);
      });
  }
  deletePhoto(photo: Photo) {
    this.alertify.confirm('Are you sure you want to delete this photo?', () => {
      this.userService.deletePhoto(this.authService.decodedToken.nameid, photo.id).subscribe(
        () => {
          const indexToDelete = this.photos.indexOf(photo);
          this.photos.splice(indexToDelete, 1);
          this.alertify.success('Photo has been deleted');
        }, error => {
          this.alertify.error('Failed to delete the photo');
        });
    });
  }
}
