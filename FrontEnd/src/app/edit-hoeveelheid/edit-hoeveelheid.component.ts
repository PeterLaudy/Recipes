import { Component, ViewChildren, AfterViewInit, Input, Directive, forwardRef, EventEmitter, Output } from '@angular/core';
import { Hoeveelheid } from '../data/hoeveelheid.model';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { SelectionLists } from '../data/selection-lists.model';

@Component({
    selector: 'app-edit-hoeveelheid',
    templateUrl: './edit-hoeveelheid.component.html'
})
export class EditHoeveelheidComponent implements AfterViewInit {

    @Input() value: Hoeveelheid;
    @Output() valueChange: EventEmitter<Hoeveelheid>;
    @Output() deleted: EventEmitter<Hoeveelheid>;
    @Input() cachedLists: SelectionLists;
    @ViewChildren('qty') aantalUIElement;

    constructor() {
        this.valueChange = new EventEmitter<Hoeveelheid>();
        this.deleted = new EventEmitter<Hoeveelheid>();
    }

    ngAfterViewInit(): void {
        this.aantalUIElement.first.nativeElement.focus();
    }
}

@Directive({
    selector: 'app-edit-hoeveelheden',
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => EditHoeveelheidComponent),
        multi: true
    }]
})
export class HoeveelheidValueAccessor implements ControlValueAccessor {
    public value: Hoeveelheid;
    public get ngModel(): Hoeveelheid { return this.value }
    public set ngModel(v: Hoeveelheid) {
      if (v !== this.value) {     
        this.value = v;
        this.onChange(v);
      }
    }

    onChange: (_: any) => void = (_) => { };
    onTouched: () => void = () => { };

    writeValue(value: Hoeveelheid): void {
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