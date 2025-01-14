import { Component, Directive, Input, Inject, ViewChildren, forwardRef, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { IEenheidDB, Eenheid } from '../data/eenheid.model';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';

@Component({
    selector: 'app-select-eenheid',
    templateUrl: './select-eenheid.component.html'
})
export class SelectEenheidComponent {

    @Input() readonly: boolean;
    @Input() value: Eenheid;
    @Input() eenheden: Eenheid[];
    @Output() valueChange: EventEmitter<Eenheid>

    constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
        this.valueChange = new EventEmitter<Eenheid>();
    }

    changeSelect(index: number, event): void {
        this.value = this.eenheden[index];
        this.valueChange.emit(this.value);
    }

    changeInput(naam: string, event): void {
        this.value = new Eenheid(null);
        this.value.index = 0;
        this.value.name = naam;
        this.eenheden.forEach(e => {
            if (e.name === naam) {
                this.value = e;
            }
        });
        this.valueChange.emit(this.value);
    }
}

@Directive({
    selector: 'app-select-eenheid',
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => SelectEenheidComponent),
        multi: true
    }]
})
export class EenheidValueAccessor implements ControlValueAccessor {
    public value: Eenheid;
    public get eenheid(): Eenheid { return this.value }
    public set eenheid(v: Eenheid) {
      if (v !== this.value) {     
        this.value = v;
        this.onChange(v);
      }
    }

    onChange: (_: any) => void = (_) => { };
    onTouched: () => void = () => { };

    writeValue(value: Eenheid): void {
        this.eenheid = value;
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