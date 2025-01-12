import { Component, Directive, Input, Inject, ViewChildren, forwardRef, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { IIngredientDB, Ingredient } from '../data/ingredient.model'
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';

@Component({
    selector: 'app-select-ingredient',
    templateUrl: './select-ingredient.component.html'
})
export class SelectIngredientComponent {

    @Input() readonly: boolean;
    @Input() value: Ingredient;
    @Output() valueChange: EventEmitter<Ingredient>

    allIngredienten: Ingredient[];

    constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
        this.valueChange = new EventEmitter<Ingredient>();

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
            this.allIngredienten = result;
            if (this.readonly && (0 == this.value.index)) {
                this.value.index = this.allIngredienten[0].index;
                this.value.name = this.allIngredienten[0].name;
            }
        }, error => console.error(error));
    }

    changeSelect(index: number, event): void {
        this.allIngredienten.forEach(i => {
            if (i.index == index) {
                // Don't use value = i !
                // That will give the caller a reference to the item in the list, causing all kinds of problems.
                this.value.index = i.index;
                this.value.name = i.name;
            }
        });
        this.valueChange.emit(this.value);
    }

    changeInput(naam: string, event): void {
        this.value = new Ingredient(null);
        this.value.index = 0;
        this.value.name = naam;
        this.allIngredienten.forEach(i => {
            if (i.name === naam) {
                this.value = i;
            }
        });
        this.valueChange.emit(this.value);
    }
}

@Directive({
    selector: 'app-select-ingredient',
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => SelectIngredientComponent),
        multi: true
    }]
})
export class IngredientValueAccessor implements ControlValueAccessor {
    public value: Ingredient;
    public get ingredient(): Ingredient { return this.value }
    public set ingredient(v: Ingredient) {
      if (v !== this.value) {     
        this.value = v;
        this.onChange(v);
      }
    }

    onChange: (_: any) => void = (_) => { };
    onTouched: () => void = () => { };

    writeValue(value: Ingredient): void {
        this.ingredient = value;
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