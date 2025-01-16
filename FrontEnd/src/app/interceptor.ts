import { HttpEvent, HttpEventType, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { Observable } from "rxjs";
import { tap } from "rxjs/operators";

@Injectable()
export class AuthenticationInterceptor implements HttpInterceptor {

    constructor(private router: Router) { }

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
                if (token) {
                    if (token == "Logout") {
                        localStorage.removeItem("jwtToken");
                    } else {
                        if (token == "Register First") {
                            this.router.navigate(["/register-user"]);
                        } else {
                            localStorage.setItem("jwtToken", token);
                        }
                    }
                }
            }
        }));
    }
}