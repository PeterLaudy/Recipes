import { Component, Inject } from '@angular/core';
import { RegisterUserData, IRegisterUserData } from '../data/register-user.model';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
    selector: 'app-register-user',
    templateUrl: './register-user.component.html'
})
export class RegisterUsedrComponent {

    firstName: string = "";
    lastName: string = "";
    userName: string = "";
    emailAddress: string = "";

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) { }

    register(): void {
        console.log(`Register: ${this.userName}`);

        var registerUserData: RegisterUserData =
            new RegisterUserData(this.firstName, this.lastName, this.userName, this.emailAddress);

        this.http.post<string>(this.baseUrl + 'api/register-user', registerUserData)
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