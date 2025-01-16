import { Component, Input, Directive, forwardRef, Inject, ViewChildren } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { Categorie, CategorieDB } from '../data/categorie.model';
import { map } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
    selector: 'app-categorieen',
    templateUrl: './edit-categorieen.component.html',
    styleUrls: ['./edit-categorieen.component.css']
})
export class EditCategorieenComponent {

    @ViewChildren('categorie') naamInput;

    @Input() value: Categorie[] = [];
    newCategorieName: string = "";
    newIconName: string = "";
    disableSave: boolean = true;

    availableicons: string[] = [
        "assets/icons/brood.svg",
        "assets/icons/cocktail.svg",
        "assets/icons/fastfood.svg",
        "assets/icons/fruit.svg",
        "assets/icons/gebak.svg",
        "assets/icons/glutenfree.svg",
        "assets/icons/ijs.svg",
        "assets/icons/nagerecht.svg",
        "assets/icons/noodles.svg",
        "assets/icons/noten.svg",
        "assets/icons/oven.svg",
        "assets/icons/pizza.svg",
        "assets/icons/rund.svg",
        "assets/icons/varken.svg",
        "assets/icons/vegan.svg",
        "assets/icons/vis.svg",
        "assets/icons/vis2.svg",
        "assets/icons/quiche.svg",
        "assets/icons/drank.svg",
        "assets/icons/bbq.svg",
        "assets/icons/soep.svg"
    ];

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) {
        // Get all the available values for the Categorien
        this.getDataFromServer();
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
            newValue.iconPath = this.baseUrl + this.availableicons[i];
            this.value.push(newValue);
        }
 
        this.newCategorieName = "";
        this.focusEditElement();
    }

    changeCategorie(i: number): void {
        if ("" == this.newCategorieName)
        {
            this.value.splice(i, 1);
        }
        else
        {
            this.value[i].naam = this.newCategorieName;
        }
        this.newCategorieName = "";
        this.focusEditElement();
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