import { Component, Inject } from '@angular/core';
import { ChangePasswordData } from '../data/change-password.model';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
    selector: 'app-change-password',
    templateUrl: './change-password.component.html'
})
export class ChangePasswordComponent {

    token: string = "-";
    userName: string = "";
    password: string = "";
    confirmPassword: string = "";

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) {
        let t = window.location.search;
        if (t.indexOf("token=") > 0) {
            this.token = t.substring(t.indexOf("token=") + 6);
            console.log(this.token);
        }
    }

    changePassword(): void {
        console.log(`Change password: ${this.userName}`);

        if (this.password != this.confirmPassword) {
            alert("Passwords are not the same!");
            return;
        }

        var changePasswordData: ChangePasswordData =
            new ChangePasswordData(this.token, this.userName, this.password, this.confirmPassword);

        this.http.post(this.baseUrl + 'api/change-password', changePasswordData)
        .subscribe(response => {
            let result: any = response;
            if (result.status != 'OK') {
                console.log(result.reason);
                alert(result.reason);
            } else {
                this.router.navigate(['/']);
            }
        }, error => console.error(error));
    }
}