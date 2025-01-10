import { HttpEvent, HttpEventType, HttpHandler, HttpHandlerFn, HttpInterceptor, HttpRequest, HttpResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { tap } from "rxjs/operators";

@Injectable()
export class AuthenticationInterceptor implements HttpInterceptor {

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
                    if (token == "Bearer") {
                        localStorage.removeItem("jwtToken");
                    } else {
                        if (token == "Register First") {
                            window.location.pathname = "/register-user;"
                        } else {
                            localStorage.setItem("jwtToken", token);
                        }
                    }
                }
            }
        }));
    }
}