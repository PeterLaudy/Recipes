import { Component, ViewChildren, Inject } from '@angular/core';
import { LoginData, ILoginData } from '../data/login.model';
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

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) { }

    ngAfterViewInit(): void {
        if (this.naamInput && this.naamInput.first) {
            this.naamInput.first.nativeElement.focus();
        }
    }

    login(): void {
        console.log(`Login: ${this.userName}`);

        var loginData: LoginData = new LoginData(this.userName, this.password);

        this.http.post<string>(this.baseUrl + 'api/login', loginData)
        .subscribe(result => {
            if (result != 'OK') {
                console.log("Login failed.");
                alert("Login failed!");
            } else {
                this.router.navigate(['/add']);
            }
        }, error => console.error(error));
    }
}