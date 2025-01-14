import { HttpClient } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { map } from "rxjs/operators";

@Injectable({
    providedIn: "root"
})
export class AuthService {
    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

    IsAuthenticated(): Promise<boolean> {
        return new Promise<boolean>(resolve => {
            this.http
                .get(this.baseUrl + 'api/isauthenticated')
                .pipe<boolean>(
                    map((data: string) => {
                        let result: any = data;
                        return result.isAuthenticated;
                    })
                ).subscribe(result => {
                    resolve(result);
                });
        });
    }
}
