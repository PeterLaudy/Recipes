import { HttpClient } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { map } from "rxjs/operators";

@Injectable({
    providedIn: "root"
})
export class AuthService {
    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) { }

    IsAuthenticated(): Promise<boolean> {
        return new Promise<boolean>(resolve => {
            this.http
                .get<string>(this.baseUrl + 'api/isauthenticated')
                .pipe<boolean>(
                    map((data: string) => {
                        let result = data == "OK";
                        if (!result) {
                            this.router.navigate(["/login"]);
                        }
                        return result;
                    })
                ).subscribe(result => {
                    resolve(result);
                });
        });
    }
}
