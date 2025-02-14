import { Component, Input, Inject, Output, EventEmitter, AfterViewInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { Categorie } from '../data/categorie.model';

@Component({
    selector: 'app-select-categorie',
    templateUrl: './select-categorie.component.html',
    styleUrls: ['./select-categorie.component.css']
})
export class SelectCategorieComponent implements AfterViewInit {

    @Input() value: Categorie[];
    @Input() readonly: boolean = false;
    @Output() change: EventEmitter<Categorie[]>

    selectedCategorieen: number[] = [];
    categorieen: Categorie[];
    colors: string[] = [];

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
            this.colors = [];
            for (const c of this.categorieen) {
                if (this.selectedCategorieen.includes(c.categorieID)) {
                    this.colors.push("#000000");
                } else {
                    this.colors.push("#C0C0C0");
                }
            }
        }, error => console.error(error));
    }

    ngAfterViewInit(): void {
        if (!this.readonly && this.value) {
            this.selectedCategorieen = [];
            for (const v of this.value) {
                this.selectedCategorieen.push(v.categorieID);
            }
            if (0 < this.categorieen.length) {
                this.colors = [];
                for (const c of this.categorieen) {
                    if (this.selectedCategorieen.includes(c.categorieID)) {
                        this.colors.push("#000000");
                    } else {
                        this.colors.push("#C0C0C0");
                    }
                }
            }
        }
    }

    toggleCategorie(index: number): void {
        if (!this.readonly) {
            let indexInValue = this.selectedCategorieen.indexOf(this.categorieen[index].categorieID);
            if (-1 != indexInValue) {
                this.value.splice(indexInValue, 1);
                this.selectedCategorieen.splice(indexInValue, 1);
            } else {
                this.value.push(this.categorieen[index]);
                this.selectedCategorieen.push(this.categorieen[index].categorieID);
            }

            for (let i = 0; i < this.colors.length; i++) {
                this.colors[i] = this.selectedCategorieen.includes(this.categorieen[i].categorieID) ? "#000000" : "#C0C0C0";
            }

            this.change.emit(this.value);
        }
    }
}