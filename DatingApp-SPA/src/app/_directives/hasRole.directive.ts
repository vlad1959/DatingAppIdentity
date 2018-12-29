import { Directive, Input, ViewContainerRef, TemplateRef, OnInit } from '@angular/core';
import { AuthService } from '../_servicies/auth.service';


// this is structural directive like *ngIf
// should be used as *appHasRole
@Directive({
  selector: '[appHasRole]'
})
export class HasRoleDirective implements OnInit{
  @Input() appHasRole: string[]; // passing parametrs into directive

  isVisible = false; // to check if element we are hiding or showing is already visible

  constructor(private viewContainerRef:  ViewContainerRef,
              private templateRef: TemplateRef<any>,
              private authService: AuthService

    ) { }
    ngOnInit() {

      const userRoles = this.authService.decodedToken.role as Array<string>;

      // if user doesn't have any roles, don't render element
      if (!userRoles) {
          this.viewContainerRef.clear();
      }

      // if user has needed roles, then display, otherwise - hide
      // this.appHasRole points to @Input (supplied roles to be matched)
      if (this.authService.roleMatch(this.appHasRole)) {
          if (!this.isVisible) {
            this.isVisible = true;
            // templateRef points to the element directive is applied to
            this.viewContainerRef.createEmbeddedView(this.templateRef);
          } else {
            this.isVisible = false;
            this.viewContainerRef.clear();
          }
      }

    }
}
