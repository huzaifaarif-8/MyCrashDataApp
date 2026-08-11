import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { authInterceptor } from './interceptors/auth.interceptor';
import { authGuard } from './guards/auth.guard';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { LoginComponent } from './components/login/login.component';
import { UsersComponent } from './components/users/users.component';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withFetch(), withInterceptors([authInterceptor])),
    provideRouter([
      { path: 'login', component: LoginComponent },
      { path: '', component: DashboardComponent, canActivate: [authGuard] },
      { path: 'users', component: UsersComponent, canActivate: [authGuard] },
      { path: '**', redirectTo: '' },
    ]),
  ]
};
