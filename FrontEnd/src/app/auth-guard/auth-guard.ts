import { inject, Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivate, CanActivateFn, Router, RouterStateSnapshot, UrlTree } from "@angular/router";
import { AuthService } from "./auth-service";

export function IsAuthenticated(): CanActivateFn {
    return (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
        const authService: AuthService = inject(AuthService);
        const router: Router = inject(Router);
        return authService.IsAuthenticated().then(isAuth => {
            if (!isAuth) {
                return router.parseUrl(`/login?redirect=${state.url}`);
            }
            return isAuth;
        });
    }
}

export function IsAdmin(): CanActivateFn {
    return (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
        const authService: AuthService = inject(AuthService);
        const router: Router = inject(Router);
        return authService.IsAdmin().then(isAdmin => {
            if (!isAdmin) {
                return router.parseUrl(`/login?redirect=${state.url}`);
            }
            return isAdmin;
        });
    }
}