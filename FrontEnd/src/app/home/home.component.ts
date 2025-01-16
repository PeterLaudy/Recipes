import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { GerechtSummary } from '../data/gerecht.model';
import { AuthService } from '../auth-guard/auth-service';
import { Categorie } from '../data/categorie.model';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.css']
})
export class HomeComponent {
    public recepten: GerechtSummary[];
    categorieen: Categorie[] = [];
    visible: boolean[] = [];
    authenticated: boolean = false;

    constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, authService: AuthService) {
        http.post<GerechtSummary[]>(baseUrl + 'api/Data/Recepten', { withCredentials: true }).subscribe(result => {
            this.recepten = result;
            for (const recept in this.recepten) {
                this.visible.push(true);
            }

        }, error => console.error(error));

        authService.IsAuthenticated().then(isAuth => { this.authenticated = isAuth; });
    }
    
    categoriechanged(event): void {
        // First check if any categorie is selected at all.
        if (0 == this.categorieen.length) {
            for (let i = 0; i < this.visible.length; i++) {
                this.visible[i] = true;
            }
        } else {
            for (let i = 0; i < this.categorieen.length; i++) {
                for (let j = 0; j < this.recepten.length; j++) {
                    this.visible[j] = false;
                    if (this.recepten[j].categorieen.length != 0) {
                        for (let k = 0; k < this.recepten[j].categorieen.length; k++) {
                            if (this.recepten[j].categorieen[k].categorieID == this.categorieen[i].categorieID) {
                                this.visible[j] = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
