import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Ingredient, IngredientDB } from '../data/ingredient.model';
import { GerechtSummaryList } from '../data/gerecht.model';

@Component({
    selector: 'app-search-component',
    templateUrl: './search.component.html',
    styleUrls: ['./search.component.css']
})
export class SearchComponent {

    Ingredienten: Ingredient[] = [];
    Recepten: GerechtSummaryList[];

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
        this.Ingredienten.push(new Ingredient(null));
    }

    addIngredient(): void {
        this.Ingredienten.push(new Ingredient(null));
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

        this.http.post<GerechtSummaryList[]>(this.baseUrl + 'api/Data/FindRecept', result)
        .subscribe(result => {
            this.Recepten = result;
        }, error => console.error(error));
    }
}