import { Component, Inject } from '@angular/core';
import { RegisterUserData } from '../data/register-user.model';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
    selector: 'app-register-user',
    templateUrl: './register-user.component.html'
})
export class RegisterUserComponent {

    firstName: string = "";
    lastName: string = "";
    userName: string = "";
    emailAddress: string = "";

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) { }

    registerUser(): void {
        console.log(`Register: ${this.userName}`);

        var registerUserData: RegisterUserData =
            new RegisterUserData(this.firstName, this.lastName, this.userName, this.emailAddress);

        this.http.post(this.baseUrl + 'api/register-user', registerUserData)
        .subscribe(response => {
            var result: any = response;
            if (result.status != 'OK') {
                console.log(result.reason);
                alert(result.reason);
            } else {
                this.router.navigate(['/']);
            }
        }, error => console.error(error));
    }
}