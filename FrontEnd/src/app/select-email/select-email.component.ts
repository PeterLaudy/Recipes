import { Component, Input, Inject, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';

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

class Email {
    firstName: string;
    lastName: string;
    address: string;
}