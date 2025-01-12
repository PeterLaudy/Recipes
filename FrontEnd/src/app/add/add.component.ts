import { Component } from '@angular/core';
import { Recept } from '../data/recept.model';

@Component({
    selector: 'app-add-data',
    templateUrl: './add.component.html'
})
export class AddComponent {

    recept: Recept = new Recept(null);
}