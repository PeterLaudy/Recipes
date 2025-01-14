import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { GerechtSummaryList } from '../data/gerecht.model';
import { AuthService } from '../auth-guard/auth-service';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.css']
})
export class HomeComponent {
    public recepten: GerechtSummaryList[];
    authenticated: boolean = false;

    constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, authService: AuthService) {
        http.get<GerechtSummaryList[]>(baseUrl + 'api/Data/Recepten', { withCredentials: true }).subscribe(result => {
            this.recepten = result;
        }, error => console.error(error));

        authService.IsAuthenticated().then(isAuth => { this.authenticated = isAuth; });
    }
}
