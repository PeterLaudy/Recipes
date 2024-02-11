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

    @Input() value: number;
    @Output() valueChange: EventEmitter<number>

    categorieen: Categorie[];

    constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
        this.valueChange = new EventEmitter<number>();

        http.get<ICategorieDB[]>(baseUrl + 'api/Data/Categorieen').pipe(
            map(data => {
                const result: Categorie[] = [];
                for (const d of data) {
                    const c = new Categorie(d);
                    result.push(c);
                }
                return result.sort((a, b) => a.name.localeCompare(b.name));
            })
        )
        .subscribe(result => {
            this.categorieen = result;
        }, error => console.error(error));
    }

    changeSelect(index: number, event): void {
        this.value = +index;
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