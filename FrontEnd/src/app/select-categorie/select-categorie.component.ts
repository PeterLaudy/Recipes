import { Component, Directive, Input, Inject, forwardRef, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { Categorie, ICategorieDB } from '../data/categorie.model';

@Component({
    selector: 'app-select-categorie',
    templateUrl: './select-categorie.component.html'
})
export class SelectCategorieComponent {

    @Input() value: Categorie;
    @Output() valueChange: EventEmitter<Categorie>

    categorieen: Categorie[];

    constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
        this.valueChange = new EventEmitter<Categorie>();

        http.get<Categorie[]>(baseUrl + 'api/Data/Categorieen').pipe(
            map(data => {
                return data.sort((a, b) => a.naam.localeCompare(b.naam));
            })
        )
        .subscribe(result => {
            this.categorieen = result;
        }, error => console.error(error));
    }

    changeSelect(index: number, event): void {
        this.value = this.categorieen[index];
        this.valueChange.emit(this.value);
    }

    changeInput(naam: string, event): void {
        this.value = new Categorie(null);
        this.value.categorieID = 0;
        this.value.naam = naam;
        this.categorieen.forEach(c => {
            if (c.naam === naam) {
                this.value = c;
            }
        });
        this.valueChange.emit(this.value);
    }
}

@Directive({
    selector: 'app-select-categorie',
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => SelectCategorieComponent),
        multi: true
    }]
})
export class CategorieValueAccessor implements ControlValueAccessor {
    public value: number;
    public get categorie(): number { return this.value }
    public set categorie(v: number) {
      if (v !== this.value) {     
        this.value = v;
        this.onChange(v);
      }
    }

    onChange: (_: any) => void = (_) => { };
    onTouched: () => void = () => { };

    writeValue(value: number): void {
        this.categorie = value;
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