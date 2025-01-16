import { Component, Input, Inject, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Eenheid } from '../data/eenheid.model';

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
        this.eenheden.forEach(e => {
            if (e.index == index) {
                // Don't use value = e !
                // That will give the caller a reference to the item in the list, causing all kinds of problems.
                this.value.index = e.index;
                this.value.name = e.name;
            }
        });
        this.valueChange.emit(this.value);
    }

    changeInput(naam: string, event): void {
        this.value.index = 0;
        this.value.name = naam;
        this.eenheden.forEach(e => {
            if (e.name === naam) {
                this.value.index = e.index;
            }
        });
        this.valueChange.emit(this.value);
    }
}