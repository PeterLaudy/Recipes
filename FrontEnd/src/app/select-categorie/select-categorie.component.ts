import { Component, Directive, Input, Inject, forwardRef, Output, EventEmitter, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { Categorie } from '../data/categorie.model';

@Component({
    selector: 'app-select-categorie',
    templateUrl: './select-categorie.component.html',
    styleUrls: ['./select-categorie.component.css']
})
export class SelectCategorieComponent {

    @Input() value: Categorie[];
    @Input() readonly: boolean = false;
    @Output() change: EventEmitter<Categorie[]>

    imgClass: string[] = [];
    selectedCategorieen: number[] = [];
    categorieen: Categorie[];

    constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
        this.change = new EventEmitter<Categorie[]>();

        http.get<Categorie[]>(baseUrl + 'api/Data/Categorieen').pipe(
            map(data => {
                return data.sort((a, b) => a.naam.localeCompare(b.naam));
            })
        )
        .subscribe(result => {
            if (!this.readonly && this.value) {
                this.selectedCategorieen = [];
                for (const v of this.value) {
                    this.selectedCategorieen.push(v.categorieID);
                }
            }
            
            this.categorieen = result;
            for (const c of this.categorieen) {
                if (-1 != this.selectedCategorieen.indexOf(c.categorieID)) {
                    this.imgClass.push("img-fluid border");
                } else {
                    this.imgClass.push("img-fluid no-border");
                }
            }
        }, error => console.error(error));
    }

    ngOnInit(): void {
        if (!this.readonly && this.value) {
            this.selectedCategorieen = [];
            for (const v of this.value) {
                this.selectedCategorieen.push(v.categorieID);
            }
        }
    }

    toggleCategorie(index: number): void {
        if (!this.readonly) {
            let indexInValue = this.selectedCategorieen.indexOf(this.categorieen[index].categorieID);
            if (-1 != indexInValue) {
                this.value.splice(indexInValue, 1);
                this.selectedCategorieen.splice(indexInValue, 1);
                this.imgClass[index] = "img-fluid no-border";
            } else {
                this.value.push(this.categorieen[index]);
                this.selectedCategorieen.push(this.categorieen[index].categorieID);
                this.imgClass[index] = "img-fluid border";
            }

            this.change.emit(this.value);
        }
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