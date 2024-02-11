import { Component, Input, Directive, forwardRef, EventEmitter, Output } from '@angular/core';
import { Hoeveelheid } from '../data/hoeveelheid.model';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';

@Component({
    selector: 'app-edit-hoeveelheden',
    templateUrl: './edit-hoeveelheden.component.html'
})
export class EditHoeveelhedenComponent {

    @Input() value: Hoeveelheid[];
    @Output() valueChange: EventEmitter<Hoeveelheid[]>;

    constructor() {
        this.valueChange = new EventEmitter<Hoeveelheid[]>();
    }

    addHoeveelheid(): void {
        this.value.push(new Hoeveelheid(null));
    }

    deleteHoeveelheid(hoeveelheid: Hoeveelheid): void {
        this.value.splice(this.value.indexOf(hoeveelheid), 1);
    }

    drop(event: any): void {
        for(var key in event) {
            console.log(`${key} => ${event[key]}`);
        }
    }
}

@Directive({
    selector: 'app-edit-hoeveelheden',
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => EditHoeveelhedenComponent),
        multi: true
    }]
})
export class HoeveelhedenValueAccessor implements ControlValueAccessor {
    public value: Hoeveelheid[];
    public get ngModel(): Hoeveelheid[] { return this.value }
    public set ngModel(v: Hoeveelheid[]) {
      if (v !== this.value) {     
        this.value = v;
        this.onChange(v);
      }
    }

    onChange: (_: any) => void = (_) => { };
    onTouched: () => void = () => { };

    writeValue(value: Hoeveelheid[]): void {
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