import { HttpClient } from '@angular/common/http';
import { Component, Inject } from '@angular/core';

@Component({
    selector: 'app-admin-page',
    templateUrl: './admin-page.component.html',
    styleUrls: ['./admin-page.component.css']
})
export class AdminPageComponent {
    isExpanded = false;
    
    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }
    
    cleanup() {
        this.http.get<{ status: string, removed: string[] }>(this.baseUrl + 'api/Data/Cleanup')
            .subscribe(result => {
                if (result.removed.length > 0) {
                    alert(result.removed);
                } else {
                    alert("Nothing to cleanup.");
                }
            }, error => console.error(error));
    }
}
