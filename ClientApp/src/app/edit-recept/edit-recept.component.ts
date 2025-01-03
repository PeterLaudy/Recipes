import { Component, ViewChildren, Inject, AfterViewInit, Input, Directive, forwardRef } from '@angular/core';
import { Recept, ReceptDB } from '../data/recept.model';
import { HttpClient } from '@angular/common/http';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
    selector: 'app-edit-recept',
    templateUrl: './edit-recept.component.html'
})
export class EditReceptComponent implements AfterViewInit {

    @Input() value: Recept;
    @ViewChildren('name') naamInput;

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) { }

    ngAfterViewInit(): void {
        if (this.naamInput && this.naamInput.first) {
            this.naamInput.first.nativeElement.focus();
        }
    }

    save(): void {
        console.log(`Gerecht: ${this.value.gerecht.name} (${this.value.gerecht.categorie})`);
        this.value.hoeveelheden.forEach(h => {
            console.log(`${h.aantal} ${h.Eenheid.name} ${h.Ingredient.name}`);
            h.eenheidIndex = h.Eenheid.index;
            h.ingredientIndex = h.Ingredient.index;
            h.gerechtIndex = this.value.gerecht.index;
        });
        console.log(`${this.value.gerecht.minutes} minuten`);
        console.log(`${this.value.gerecht.description}`);

        var recept: Recept = new Recept(null);
        recept.gerecht = this.value.gerecht;
        recept.hoeveelheden = this.value.hoeveelheden;

        this.http.post<string>(this.baseUrl + 'api/Data/AddRecept', new ReceptDB(recept))
        .subscribe(result => {
            if (result != 'OK') {
                console.log(result);
                alert(result);
            } else {
                this.router.navigate(['/']);
            }
        }, error => console.error(error));
    }
}

@Directive({
    selector: 'app-edit-recept',
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => EditReceptComponent),
        multi: true
    }]
})
export class ReceptValueAccessor implements ControlValueAccessor {
    public value: Recept;
    public get ngModel(): Recept { return this.value }
    public set ngModel(v: Recept) {
      if (v !== this.value) {     
        this.value = v;
        this.onChange(v);
      }
    }

    onChange: (_: any) => void = (_) => { };
    onTouched: () => void = () => { };

    writeValue(value: Recept): void {
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