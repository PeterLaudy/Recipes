import { inject, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { SearchComponent } from './search/search.component';
import { AddComponent } from './add/add.component';
import { SelectEenheidComponent, EenheidValueAccessor } from './select-eenheid/select-eenheid.component';
import { SelectIngredientComponent, IngredientValueAccessor } from './select-ingredient/select-ingredient.component';
import { EditReceptComponent, ReceptValueAccessor } from './edit-recept/edit-recept.component';
import { EditComponent } from './edit/edit.component';
import { ShowReceptComponent } from './show-recept/show-recept.component';
import { ShowComponent } from './show/show.component';
import { LoginComponent } from './login/login.component';
import { AuthGuard } from './auth-guard/auth-guard';
import { RegisterComponent } from './register/register.component';
import { CategorieValueAccessor, SelectCategorieComponent } from './select-categorie/select-categorie.component';
import { SelectEmailComponent, EmailValueAccessor } from './select-email/select-email.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { EditHoeveelhedenComponent, HoeveelhedenValueAccessor } from './edit-hoeveelheden/edit-hoeveelheden.component';
import { EditHoeveelheidComponent, HoeveelheidValueAccessor } from './edit-hoeveelheid/edit-hoeveelheid.component';
import { AuthenticationInterceptor } from './interceptor';

@NgModule({
    declarations: [
        AppComponent,
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
        EditComponent,
        ShowReceptComponent,
        ShowComponent,
        LoginComponent,
        RegisterComponent,
        EenheidValueAccessor,
        IngredientValueAccessor,
        HoeveelhedenValueAccessor,
        HoeveelheidValueAccessor,
        CategorieValueAccessor,
        EmailValueAccessor,
        ReceptValueAccessor
    ],
    imports: [
        HttpClientModule,
        FormsModule,
        RouterModule.forRoot([
            { path: '', component: HomeComponent, pathMatch: 'full' },
            { path: 'search', component: SearchComponent },
            { path: 'add', component: AddComponent, canActivate: [AuthGuard] },
            { path: 'edit/:index', component: EditComponent },
            { path: 'show/:index', component: ShowComponent },
            { path: 'login', component: LoginComponent },
            { path: 'register', component: RegisterComponent }
        ]),
        BrowserAnimationsModule
    ],
    providers: [
        { provide: HTTP_INTERCEPTORS, useClass: AuthenticationInterceptor, multi: true }
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
