import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IIngredientDB, Ingredient, IngredientDB } from '../data/ingredient.model';
import { GerechtSummary } from '../data/gerecht.model';
import { map } from 'rxjs/operators';
import { AuthService } from '../auth-guard/auth-service';

@Component({
    selector: 'app-search-component',
    templateUrl: './search.component.html',
    styleUrls: ['./search.component.css']
})
export class SearchComponent {

    Ingredienten: Ingredient[] = [];
    Recepten: GerechtSummary[];
    cachedLists: Ingredient[];
    authenticated: boolean = false;

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, authService: AuthService) {

        http.get<IIngredientDB[]>(baseUrl + 'api/Data/Ingredienten').pipe(
            map(data => {
                const result: Ingredient[] = [];
                for (const d of data) {
                    const i = new Ingredient(d);
                    result.push(i);
                }
                return result.sort((a, b) => a.name.localeCompare(b.name));
            })
        )
        .subscribe(result => {
            // Since we pass the cachedLists on the child components using a binding,
            // we need to change its value. We cannot only change the field members
            // of the cachedLists, as this would not trigger the binding.
            this.cachedLists = result;
            this.Ingredienten.push(this.cachedLists[0]);
        }, error => console.error(error));

        authService.IsAuthenticated().then(isAuth => { this.authenticated = isAuth; });
    }

    addIngredient(): void {
        this.Ingredienten.push(this.cachedLists[0]);
    }

    deleteIngredient(ingredient: Ingredient): void {
        this.Ingredienten.splice(this.Ingredienten.indexOf(ingredient), 1);
    }

    findRecept(): void {
        this.Ingredienten.forEach(i => {
            console.log(`${i.name}`);
        });

        let result: IngredientDB[] = [];
        this.Ingredienten.forEach(val => {
            result.push(new IngredientDB(val));
        })

        this.http.post<GerechtSummary[]>(this.baseUrl + 'api/Data/FindRecept', result)
        .subscribe(result => {
            this.Recepten = result;
        }, error => console.error(error));
    }
}