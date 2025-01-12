import { Component, Directive, Input, Inject, forwardRef, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';

@Component({
    selector: 'app-select-email',
    templateUrl: './select-email.component.html'
})
export class SelectEmailComponent {

    @Input() value: string;
    @Output() valueChange: EventEmitter<string>
    @Output() valuesLoaded: EventEmitter<boolean>

    addresses: Email[] = null;

    constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
        this.valueChange = new EventEmitter<string>();
        this.valuesLoaded = new EventEmitter<boolean>();

        http.get<Email[]>(baseUrl + 'api/Data/EmailAddresses')
            .subscribe(
                result => {
                    this.addresses = result;
                    this.valuesLoaded.emit(true);
                },
                error => {
                    this.addresses = [];
                    this.valuesLoaded.emit(false);
                    console.error(error);
                });
    }

    changeInput(address: string, event): void {
        this.value = address;
        this.valueChange.emit(this.value);
    }
}

@Directive({
    selector: 'app-select-email',
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => SelectEmailComponent),
        multi: true
    }]
})
export class EmailValueAccessor implements ControlValueAccessor {
    public value: string;
    public get email(): string { return this.value }
    public set email(v: string) {
      if (v !== this.value) {     
        this.value = v;
        this.onChange(v);
      }
    }

    onChange: (_: any) => void = (_) => { };
    onTouched: () => void = () => { };

    writeValue(value: string): void {
        this.email = value;
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

class Email {
    firstName: string;
    lastName: string;
    address: string;
}