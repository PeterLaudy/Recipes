import { Component, Inject } from '@angular/core';
import { RegisterData, IRegisterData } from '../data/register.model';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
    selector: 'app-verifyuser',
    templateUrl: './verifyuser.component.html'
})
export class VerifyUserComponent {

    token: string = "-";
    firstName: string = "";
    lastName: string = "";
    userName: string = "";
    emailAddress: string = "";
    password: string = "";
    confirmPassword: string = "";

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) { }

    register(): void {
        console.log(`Register: ${this.userName}`);

        var registerData: RegisterData =
            new RegisterData(this.token, this.firstName, this.lastName, this.userName, this.emailAddress, this.password, this.confirmPassword);

        this.http.post<string>(this.baseUrl + 'api/register', registerData)
        .subscribe(result => {
            if (result != 'OK') {
                console.log(result);
                alert(result);
            } else {
                this.router.navigate(['/']);
            }
        }, error => console.error(error));
    }
}