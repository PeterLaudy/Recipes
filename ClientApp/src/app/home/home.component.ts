import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { GerechtSummaryList } from '../data/gerecht.model';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.css']
})
export class HomeComponent {
    public recepten: GerechtSummaryList[];

    constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
        http.get<GerechtSummaryList[]>(baseUrl + 'api/Data/Recepten').subscribe(result => {
            this.recepten = result;
        }, error => console.error(error));
    }
}
