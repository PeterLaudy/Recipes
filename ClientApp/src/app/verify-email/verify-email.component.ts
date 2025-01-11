import { AfterViewInit, Component, Inject } from '@angular/core';
import { VerifyEmailData } from '../data/verify-email.model';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
    selector: 'app-verify-email',
    templateUrl: './verify-email.component.html'
})
export class VerifyEmailComponent {

    token: string = "-";
    userName: string = "";
    status: string = "";

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) {
        let t = window.location.search;
        if (t.indexOf("token=") > 0) {
            this.token = t.substring(t.indexOf("token=") + 6);
            console.log(this.token);
        }
    }

    verifyEmail(): void {
        console.log(`Verify email: ${this.userName}`);

        this.status = "Contacting server...";

        var verifyData: VerifyEmailData =
            new VerifyEmailData(this.userName, this.token);

        this.http.post(this.baseUrl + 'api/verify-email', verifyData)
            .subscribe(response => {
                let result: any = response;
                if (result.status != 'OK') {
                    console.log(result.reason);
                    this.status = result.reason;
                } else {
                    this.router.navigate(
                        ["/change-password"],
                        { queryParams: { token: result.token } });
            }
        }, error => console.error(error));
    }
}