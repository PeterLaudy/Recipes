import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from "@angular/router";
import { AuthService } from "./auth-service";

@Injectable({
    providedIn: "root"
})
export class AuthGuard implements CanActivate {
    constructor(private authService: AuthService, private router: Router) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {

        return new Promise<boolean>(resolve => {
            this.authService.IsAuthenticated().then(isAuth => {
                if (!isAuth) {
                    this.router.navigate(["/login"]);
                }
                resolve(isAuth);
                });
        });
    }
}