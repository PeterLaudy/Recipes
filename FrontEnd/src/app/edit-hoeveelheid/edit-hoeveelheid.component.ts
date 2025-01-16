import { Component, ViewChildren, AfterViewInit, Input, EventEmitter, Output } from '@angular/core';
import { Hoeveelheid } from '../data/hoeveelheid.model';
import { SelectionLists } from '../data/selection-lists.model';

@Component({
    selector: 'app-edit-hoeveelheid',
    templateUrl: './edit-hoeveelheid.component.html'
})
export class EditHoeveelheidComponent implements AfterViewInit {

    @Input() value: Hoeveelheid;
    @Output() change: EventEmitter<Hoeveelheid>;
    @Output() deleted: EventEmitter<Hoeveelheid>;
    @Input() cachedLists: SelectionLists;
    @ViewChildren('qty') aantalUIElement;

    constructor() {
        this.change = new EventEmitter<Hoeveelheid>();
        this.deleted = new EventEmitter<Hoeveelheid>();
    }

    ngAfterViewInit(): void {
        this.aantalUIElement.first.nativeElement.focus();
    }
}