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
        // First show all.
        for (let i = 0; i < this.visible.length; i++) {
            this.visible[i] = true;
        }

        // If some categories are selected, we hide any
        // recipe not being tagged with all categories.
        if (0 != this.categorieen.length) {
            for (let i = 0; i < this.recepten.length; i++) {
                if (this.recepten[i].categorieen.length != 0) {
                    for (let j = 0; j < this.categorieen.length; j++) {
                        var found = false;
                        for (let k = 0; k < this.recepten[i].categorieen.length; k++) {
                            if (this.recepten[i].categorieen[k].categorieID == this.categorieen[j].categorieID) {
                                found = true;
                            }
                        }
                        // If the categorie is not found, we hide the recipe.
                        this.visible[i] = this.visible[i] && found;
                    }
                } else {
                    this.visible[i] = false;
                }
            }
        }
    }
}
