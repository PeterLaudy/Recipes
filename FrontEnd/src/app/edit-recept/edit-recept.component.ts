import { Component, ViewChildren, Inject, AfterViewInit, Input } from '@angular/core';
import { Recept, ReceptDB } from '../data/recept.model';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Eenheid, IEenheidDB } from '../data/eenheid.model';
import { map } from 'rxjs/operators';
import { IIngredientDB, Ingredient } from '../data/ingredient.model';
import { SelectionLists } from '../data/selection-lists.model';

@Component({
    selector: 'app-edit-recept',
    templateUrl: './edit-recept.component.html'
})
export class EditReceptComponent implements AfterViewInit {

    @Input() value: Recept;
    @ViewChildren('name') naamInput;

    cachedLists: SelectionLists = new SelectionLists([], []);

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) {

        // Get all the available values for the Eenheid and Ingredient selectors
        http.get<IEenheidDB[]>(baseUrl + 'api/Data/Eenheden').pipe(
            map(data => {
                const result: Eenheid[] = [];
                for (const d of data) {
                    const e = new Eenheid(d);
                    result.push(e);
                }
                return result.sort((a, b) => a.name.localeCompare(b.name));
            })
        )
        .subscribe(result => {
            // Since we pass the cachedLists on the child components using a binding,
            // we need to change its value. We cannot only change the field members
            // of the cachedLists, as this would not trigger the binding.
            this.cachedLists = new SelectionLists(result, this.cachedLists.Ingredienten);
        }, error => console.error(error));


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
            this.cachedLists = new SelectionLists(this.cachedLists.Eenheden, result);
        }, error => console.error(error));
    }

    ngAfterViewInit(): void {
        if (this.naamInput && this.naamInput.first) {
            this.naamInput.first.nativeElement.focus();
        }
    }

    save(): void {
        this.value.hoeveelheden.forEach(h => {
            h.eenheidIndex = h.Eenheid.index;
            h.ingredientIndex = h.Ingredient.index;
            h.gerechtIndex = this.value.gerecht.index;
        });

        this.http.post<string>(this.baseUrl + 'api/Data/AddRecept', new ReceptDB(this.value))
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