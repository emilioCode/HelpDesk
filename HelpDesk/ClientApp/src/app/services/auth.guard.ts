import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { ApiService } from './api.service';

// Keeps a user out of the screens their role does not allow. The route says the minimum level in data.minLevel.
// The server enforces the same rules on the API; this only avoids showing an empty screen.
@Injectable()
export class AuthGuard implements CanActivate {
  constructor(private service: ApiService, private router: Router) { }

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const user = this.service.getUser();
    if (!user) {
      return false;   // the login screen is shown instead of the application
    }

    const minLevel = route.data && route.data.minLevel ? route.data.minLevel : 0;
    const role = this.service.levels.filter(x => x.name === user.acceso)[0];
    const level = role ? role.value : 0;

    if (level < minLevel) {
      this.service.swal(this.service.messages.titleAccessDenied, this.service.messages.noPermissionToAccess, 'warning');
      this.router.navigateByUrl('/');
      return false;
    }
    return true;
  }
}
