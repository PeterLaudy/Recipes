import { Component, ViewChildren, Inject } from '@angular/core';
import { LoginData } from '../data/login.model';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html'
})
export class LoginComponent {

    @ViewChildren('name') naamInput;

    userName: string = "";
    password: string = "";
    status: string = "";

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) { }

    ngAfterViewInit(): void {
        if (this.naamInput && this.naamInput.first) {
            this.naamInput.first.nativeElement.focus();
        }
    }

    login(): void {
        console.log(`Login: ${this.userName}`);

        var loginData: LoginData = new LoginData(this.userName, this.password);

        this.http.post(this.baseUrl + 'api/login', loginData)
        .subscribe(response => {
            let result: any = response;
            if (result.status != 'OK') {
                console.log(result.reason);
                alert(result.reason);
            } else {
                // TODO: Check if we have the editor role, before sending us to this page.
                // For now it is OK, as every user has this role.
                this.router.navigate(['/add']);
            }
        }, error => console.error(error));
    }

    resetpassword(): void {
        console.log(`Reset password: ${this.userName}`);

        var loginData: LoginData = new LoginData(this.userName, this.password);

        this.http.post(this.baseUrl + 'api/request-password-change', loginData)
        .subscribe(response => {
            let result: any = response;
            if (result.status != 'OK') {
                console.log(result.reason);
                alert(result.reason);
            } else {
                this.status = "Check your mailbox for a link which with you can reset your password.";
            }
        }, error => console.error(error));
    }
}