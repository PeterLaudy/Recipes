import { Component, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth-guard/auth-service';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { AuthenticationInterceptor } from '../interceptor';

@Component({
    selector: 'app-nav-menu',
    templateUrl: './nav-menu.component.html',
    styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
    isExpanded = false;
    isLoggedIn = new Subject<boolean>();
    isAdmin = new Subject<boolean>();
    
    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, authService: AuthService) {
        authService.IsAuthenticated().then(loggedIn => {
            this.isLoggedIn.next(loggedIn)
            AuthenticationInterceptor.loginChanged().subscribe(login => this.isLoggedIn.next(login));
        });
        authService.IsAdmin().then(admin => {
            this.isAdmin.next(admin)
            AuthenticationInterceptor.adminChanged().subscribe(admin => this.isAdmin.next(admin));
        });
    }

    collapse() {
        this.isExpanded = false;
    }

    toggle() {
        this.isExpanded = !this.isExpanded;
    }

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