import { Component, Input, Inject } from '@angular/core';
import { Recept } from '../data/recept.model';
import { HttpClient } from '@angular/common/http';

@Component({
    selector: 'app-show-recept',
    templateUrl: './show-recept.component.html',
    styleUrls: ['./show-recept.component.css']
})
export class ShowReceptComponent {

    @Input() value: Recept;
    mailAddress: string = "";

    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

    sendMail(): void {
        console.log("Sending mail:");
        console.log(this.mailAddress);
        console.log(this.value.gerecht.name);

        var data = { MailAddress: this.mailAddress, GerechtIndex: this.value.gerecht.index };
        this.http.post<string>(this.baseUrl + 'api/Data/MailRecept', data)
        .subscribe(result => {
            if (result != 'OK') {
                console.log(result);
                alert(result);
            } else {
                this.mailAddress = "";
            }
        }, error => console.error(error));
    }
}