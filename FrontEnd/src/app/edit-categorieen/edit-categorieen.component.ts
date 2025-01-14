import { Component, Input, Directive, forwardRef, EventEmitter, Output } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { Categorie } from '../data/categorie.model';

@Component({
    selector: 'app-categorieen',
    templateUrl: './edit-categorieen.component.html',
    styleUrls: ['./edit-categorieen.component.css']
})
export class EditCategorieenComponent {

    @Input() value: Categorie[] = [];
    newTypeName: string = "";
    newIconName: string = "";

    availableicons: string[] = [
        "/assets/icons/brood.svg",
        "/assets/icons/cocktail.svg",
        "/assets/icons/fastfood.svg",
        "/assets/icons/fruit.svg",
        "/assets/icons/gebak.svg",
        "/assets/icons/glutenfree.svg",
        "/assets/icons/ijs.svg",
        "/assets/icons/nagerecht.svg",
        "/assets/icons/noodles.svg",
        "/assets/icons/noten.svg",
        "/assets/icons/oven.svg",
        "/assets/icons/pizza.svg",
        "/assets/icons/rund.svg",
        "/assets/icons/varken.svg",
        "/assets/icons/vegan.svg",
        "/assets/icons/vis.svg",
        "/assets/icons/vis2.svg",
        "/assets/icons/quiche.svg",
        "/assets/icons/drank.svg",
        "/assets/icons/bbq.svg",
        "/assets/icons/soep.svg"
    ];

    constructor() { }

    addCategorie(i: number): void {
        let newValue = new Categorie(null);
        newValue.index = 0;
        newValue.name = this.newTypeName;
        newValue.iconPath = this.availableicons[i];
        this.value.push(newValue);
    }

    deleteCategorie(i: number): void {
        this.value.splice(i, 1);
    }

    drop(event: any): void {
        for(var key in event) {
            console.log(`${key} => ${event[key]}`);
        }
    }
}

@Directive({
    selector: 'app-edit-categorieen',
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => EditCategorieenComponent),
        multi: true
    }]
})
export class EditCategorieValueAccessor implements ControlValueAccessor {
    public value: Categorie[];
    public get ngModel(): Categorie[] { return this.value }
    public set ngModel(v: Categorie[]) {
      if (v !== this.value) {     
        this.value = v;
        this.onChange(v);
      }
    }

    onChange: (_: any) => void = (_) => { };
    onTouched: () => void = () => { };

    writeValue(value: Categorie[]): void {
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