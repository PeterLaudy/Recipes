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
        this.value = new Ingredient(null);
        this.value.index = 0;
        this.value.name = naam;
        this.ingredienten.forEach(i => {
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