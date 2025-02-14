import { Component, Input, Directive, forwardRef, Inject, ViewChildren, AfterViewInit } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { Categorie, CategorieDB } from '../data/categorie.model';
import { map } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { IconComponent } from '../icon/icon.component'

@Component({
    selector: 'app-categorieen',
    templateUrl: './edit-categorieen.component.html',
    styleUrls: ['./edit-categorieen.component.css']
})
export class EditCategorieenComponent implements AfterViewInit {

    @ViewChildren('categorie') naamInput;

    @Input() value: Categorie[] = [];
    iconColor: string[] = [];
    newCategorieName: string = "";
    newIconName: string = "";
    disableSave: boolean = true;
    selectedIndex: number = -1;

    maxIcon: number = IconComponent.MAX_ICON;

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) {
        // Get all the available values for the Categorien
        this.getDataFromServer();
        for (let i = 0; i < IconComponent.MAX_ICON; i++) {
            this.iconColor.push("#000000");
        }
    }

    ngAfterViewInit(): void {
        this.focusEditElement();
    }

    private focusEditElement() {
        if (this.naamInput && this.naamInput.first) {
            this.naamInput.first.nativeElement.focus();
        }
    }

    private getDataFromServer() {
        this.disableSave = true;
        this.http.get<Categorie[]>(this.baseUrl + 'api/Data/Categorieen').pipe(
            map(data => {
                return data.sort((a, b) => a.naam.localeCompare(b.naam));
            })
        )
        .subscribe(result => {
            this.value = result;
            this.disableSave = false;
        }, error => console.error(error));
    }

    addCategorie(i: number): void {
        if ("" != this.newCategorieName) {
            let newValue = new Categorie(null);
            newValue.naam = this.newCategorieName;
            newValue.iconIndex = i;
            this.value.push(newValue);
            this.deselectIcons();
        } else {
            for (let index = 0; index < this.iconColor.length; index++) {
                this.iconColor[index] = i == index ? "#000000" : "#C0C0C0";
            }
            this.selectedIndex = i;
        }
 
        this.newCategorieName = "";
        this.focusEditElement();
    }

    changeCategorie(i: number): void {
        if ("" == this.newCategorieName) {
            if (-1 == this.selectedIndex) {
                this.value.splice(i, 1);
            } else {
                this.value[i].iconIndex = this.selectedIndex;
            }
        } else {
            this.value[i].naam = this.newCategorieName;
            if (-1 != this.selectedIndex) {
                this.value[i].iconIndex = this.selectedIndex;
            }
        }

        this.newCategorieName = "";
        this.deselectIcons();
        this.focusEditElement();
    }

    deselectIcons() {
        this.selectedIndex = -1;
        for (let i = 0; i < this.iconColor.length; i++) {
            this.iconColor[i] = "#000000";
        }
    }

    saveCategorieen(): void {
        var data: CategorieDB[] = [];
        for (const v of this.value) {
            data.push(new CategorieDB(v));
        }

        this.http.post(this.baseUrl + 'api/Data/Categorieen', data)
        .subscribe(response => {
            let result: any = response;
            if ("OK" != result.status) {
                console.log(result.reason);
                alert(result.reason);
                this.getDataFromServer();
                this.focusEditElement();
            } else {
                // Go back to the admin page.
                this.router.navigate(['/admin']);
            }
        }, error => console.error(error));
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