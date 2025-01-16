import { Component, Input, Inject, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Ingredient } from '../data/ingredient.model'

@Component({
    selector: 'app-select-ingredient',
    templateUrl: './select-ingredient.component.html'
})
export class SelectIngredientComponent {

    @Input() readonly: boolean;
    @Input() value: Ingredient;
    @Input() ingredienten: Ingredient[];
    @Output() valueChange: EventEmitter<Ingredient>
 
    constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
        this.valueChange = new EventEmitter<Ingredient>();
    }

    changeSelect(index: number, event): void {
        this.ingredienten.forEach(i => {
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
        this.value.index = 0;
        this.value.name = naam;
        this.ingredienten.forEach(i => {
            if (i.name === naam) {
                this.value.index = i.index;
            }
        });
        this.valueChange.emit(this.value);
    }
}