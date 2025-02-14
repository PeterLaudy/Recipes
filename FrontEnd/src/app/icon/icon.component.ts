import { Component, Input, Inject, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { Categorie } from '../data/categorie.model';

@Component({
    selector: 'app-icon',
    templateUrl: './icon.component.html',
    styleUrls: ['./icon.component.css']
})
export class IconComponent {

    @Input() index: number = 0;
    @Input() size: string = "24px";
    @Input() color: string = "#000000";

    public static MAX_ICON: number = 24; 
}