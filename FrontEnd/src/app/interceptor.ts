import { HttpEvent, HttpEventType, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { Observable, Subject } from "rxjs";
import { tap } from "rxjs/operators";
import { AuthService } from "./auth-guard/auth-service";

@Injectable()
export class AuthenticationInterceptor implements HttpInterceptor {

    private static loginSubject = new Subject<boolean>();
    private static adminSubject = new Subject<boolean>();

    constructor(private router: Router, private authService: AuthService) { }

    static loginChanged(): Observable<boolean> {
        return AuthenticationInterceptor.loginSubject.asObservable();
    }

    static adminChanged(): Observable<boolean> {
        return AuthenticationInterceptor.adminSubject.asObservable();
    }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const token = localStorage.getItem("jwtToken");
        if (token) {
            req = req.clone({
                setHeaders: { authorization: token }
            });
        }
        return next.handle(req).pipe(tap(event => {
            if (event.type === HttpEventType.Response) {
                let token = event.headers.get("authorization");

                // The Kestrel middleware can detect that the token has expired
                // or is invalid for some other reason. In that case it will add
                // a www-authenticate header. We also log-out if that happens.
                let wwwAuthenticateMsg = event.headers.get("www-authenticate");
                if (wwwAuthenticateMsg && wwwAuthenticateMsg.startsWith("Bearer error")) {
                  token = "Logout";
                }

                if (token) {
                    if (token == "Logout") {
                        localStorage.removeItem("jwtToken");
                        AuthenticationInterceptor.loginSubject.next(false);
                        AuthenticationInterceptor.adminSubject.next(false);
                    } else {
                        if (token == "Register First") {
                            this.router.navigate(["/register-user"]);
                        } else {
                            if (localStorage.getItem("jwtToken") != token) {
                                localStorage.setItem("jwtToken", token);
                                AuthenticationInterceptor.loginSubject.next(true);
                                this.authService.IsAdmin().then(isAdmin => AuthenticationInterceptor.adminSubject.next(isAdmin));
                            }
                        }
                    }
                }
            }
        }));
    }
}