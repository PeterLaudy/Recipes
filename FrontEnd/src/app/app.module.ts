import { inject, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { SearchComponent } from './search/search.component';
import { AddComponent } from './add/add.component';
import { SelectEenheidComponent } from './select-eenheid/select-eenheid.component';
import { SelectIngredientComponent } from './select-ingredient/select-ingredient.component';
import { EditReceptComponent } from './edit-recept/edit-recept.component';
import { EditComponent } from './edit/edit.component';
import { ShowReceptComponent } from './show-recept/show-recept.component';
import { ShowComponent } from './show/show.component';
import { IsAdmin, IsAuthenticated } from './auth-guard/auth-guard';
import { LoginComponent } from './login/login.component';
import { LogoutComponent } from './logout/logout.component';
import { RegisterUserComponent } from './register-user/register-user.component';
import { VerifyEmailComponent } from './verify-email/verify-email.component';
import { ChangePasswordComponent } from './change-password/change-password.component';
import { SelectCategorieComponent } from './select-categorie/select-categorie.component';
import { SelectEmailComponent } from './select-email/select-email.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { EditHoeveelhedenComponent } from './edit-hoeveelheden/edit-hoeveelheden.component';
import { EditHoeveelheidComponent } from './edit-hoeveelheid/edit-hoeveelheid.component';
import { AuthenticationInterceptor } from './interceptor';
import { EditCategorieenComponent } from './edit-categorieen/edit-categorieen.component';
import { AdminPageComponent } from './admin-page/admin-page.component';
import { IconComponent } from './icon/icon.component';

@NgModule({
    declarations: [
        AppComponent,
        AdminPageComponent,
        NavMenuComponent,
        HomeComponent,
        SearchComponent,
        AddComponent,
        SelectEenheidComponent,
        SelectIngredientComponent,
        SelectCategorieComponent,
        SelectEmailComponent,
        EditReceptComponent,
        EditHoeveelhedenComponent,
        EditHoeveelheidComponent,
        EditCategorieenComponent,
        EditComponent,
        IconComponent,
        ShowReceptComponent,
        ShowComponent,
        LoginComponent,
        LogoutComponent,
        RegisterUserComponent,
        VerifyEmailComponent,
        ChangePasswordComponent
    ],
    imports: [
        HttpClientModule,
        FormsModule,
        RouterModule.forRoot([
            { path: '', component: HomeComponent, pathMatch: 'full' },
            { path: 'search', component: SearchComponent },
            { path: 'add', component: AddComponent, canActivate: [IsAuthenticated()] },
            { path: 'edit/:index', component: EditComponent, canActivate: [IsAuthenticated()] },
            { path: 'show/:index', component: ShowComponent },
            { path: 'login', component: LoginComponent },
            { path: 'logout', component: LogoutComponent },
            { path: 'register-user', component: RegisterUserComponent, canActivate: [IsAdmin()] },
            { path: 'verify-email', component: VerifyEmailComponent },
            { path: 'change-password', component: ChangePasswordComponent },
            { path: 'edit-categorieen', component: EditCategorieenComponent, canActivate: [IsAdmin()] },
            { path: 'admin-page', component: AdminPageComponent, canActivate: [IsAdmin()] }
        ]),
        BrowserAnimationsModule
    ],
    providers: [
        { provide: HTTP_INTERCEPTORS, useClass: AuthenticationInterceptor, multi: true }
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
