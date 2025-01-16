import { Component, Input, EventEmitter, Output } from '@angular/core';
import { Hoeveelheid } from '../data/hoeveelheid.model';
import { SelectionLists } from '../data/selection-lists.model';

@Component({
    selector: 'app-edit-hoeveelheden',
    templateUrl: './edit-hoeveelheden.component.html'
})
export class EditHoeveelhedenComponent {

    @Input() value: Hoeveelheid[];
    @Input() cachedLists: SelectionLists;
    @Output() change: EventEmitter<Hoeveelheid[]>;

    constructor() {
        this.change = new EventEmitter<Hoeveelheid[]>();
    }

    addHoeveelheid(): void {
        this.value.push(new Hoeveelheid(null));
        this.change.emit(this.value);
    }

    deleteHoeveelheid(hoeveelheid: Hoeveelheid): void {
        this.value.splice(this.value.indexOf(hoeveelheid), 1);
        this.change.emit(this.value);
    }
}