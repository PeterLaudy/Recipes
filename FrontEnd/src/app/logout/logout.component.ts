import { Component,Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
    selector: 'app-logout',
    template: ''
})
export class LogoutComponent {

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) {

        console.log("Logout");

        this.http.get(this.baseUrl + 'api/logout')
        .subscribe(response => {
            let result: any = response;
            if (result.status != 'OK') {
                console.log(result.reason);
                alert(result.reason);
            } else {
                this.router.navigate(["/"]);
            }
        }, error => console.error(error));
    }
}