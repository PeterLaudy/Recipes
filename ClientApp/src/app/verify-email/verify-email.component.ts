import { Component, Inject } from '@angular/core';
import { VerifyEmailData, IVerifyEmailData } from '../data/verify-email.model';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
    selector: 'app-verify-email',
    templateUrl: './verify-email.component.html'
})
export class VerifyEmailComponent {

    token: string = "-";
    userName: string = "";

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) { }

    ngAfterViewInit(): void {
        console.log(`Verify email: ${this.userName}`);

        var verifyData: VerifyEmailData =
            new VerifyEmailData(this.token, this.userName);

        this.http.post<string>(this.baseUrl + 'api/verify-email', verifyData)
        .subscribe(result => {
            if (result == 'NOK') {
                console.log(result);
                alert("Email verification failed!");
            } else {
                this.router.navigate([`/change-password?token=${result}`]);
            }
        }, error => console.error(error));
    }
}