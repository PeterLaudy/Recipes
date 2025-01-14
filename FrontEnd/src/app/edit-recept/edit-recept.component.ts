import { Component, ViewChildren, Inject, AfterViewInit, Input, Directive, forwardRef } from '@angular/core';
import { Recept, ReceptDB } from '../data/recept.model';
import { HttpClient } from '@angular/common/http';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
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
        console.log(`Gerecht: ${this.value.gerecht.name}`);
        this.value.hoeveelheden.forEach(h => {
            console.log(`${h.aantal} ${h.Eenheid.name} ${h.Ingredient.name}`);
            h.eenheidIndex = h.Eenheid.index;
            h.ingredientIndex = h.Ingredient.index;
            h.gerechtIndex = this.value.gerecht.index;
        });
        console.log(`${this.value.gerecht.minutes} minuten`);
        console.log(`${this.value.gerecht.description}`);

        var recept: Recept = new Recept(null);
        recept.gerecht = this.value.gerecht;
        recept.hoeveelheden = this.value.hoeveelheden;

        this.http.post<string>(this.baseUrl + 'api/Data/AddRecept', new ReceptDB(recept))
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

@Directive({
    selector: 'app-edit-recept',
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => EditReceptComponent),
        multi: true
    }]
})
export class ReceptValueAccessor implements ControlValueAccessor {
    public value: Recept;
    public get ngModel(): Recept { return this.value }
    public set ngModel(v: Recept) {
      if (v !== this.value) {     
        this.value = v;
        this.onChange(v);
      }
    }

    onChange: (_: any) => void = (_) => { };
    onTouched: () => void = () => { };

    writeValue(value: Recept): void {
        this.ngModel = value;
    }

    registerOnChange(fn: (_: any) => void): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: () => void): void {
        this.onTouched = fn;
    }

    setDisabledState?(isDisabled: boolean): void {
        throw new Error("Method not implemented.");
    }
}