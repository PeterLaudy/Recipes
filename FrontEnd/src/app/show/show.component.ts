import { Component, Inject } from '@angular/core';
import { Recept, IReceptDB } from '../data/recept.model';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
    selector: 'app-show-data',
    templateUrl: './show.component.html'
})
export class ShowComponent {

    recept: Recept;

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private activatedRout: ActivatedRoute) {
        this.activatedRout.paramMap.subscribe(url => {
            var index = url.get('index');
            http.get<IReceptDB>(`${baseUrl}api/Data/Recept?index=${index}`)
            .subscribe(result => {
                this.recept = new Recept(result);
            }, error => console.error(error));
        });
    }
}