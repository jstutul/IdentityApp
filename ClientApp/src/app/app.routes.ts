import { Routes } from '@angular/router';
import { authorizationGuard } from './shared/guards/authorization-guard';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('./home/home').then((m) => m.Home),
        title: 'Home'
    }, 
    // {
    //     path: 'play',
    //     loadComponent: () => import('./play/play').then((m) => m.Play),
    //     title: 'Play'
    // },
    {
        path:'not-found',
        loadComponent: () => import('./shared/components/errors/not-found/not-found').then((m) => m.NotFound),
        title: 'Not Found'
    },
    {
        path: 'accounts',
        loadComponent: () => import('./accounts/accounts').then(m => m.Accounts),
        children: [
            {
                path: 'login',
                loadComponent: () => import('./pages/login/login').then(m => m.Login)
            },
            {
                path: 'register',
                loadComponent: () => import('./pages/register/register').then(m => m.Register)
            }
        ]
    },   
    {
        path: '',
        runGuardsAndResolvers:'always',
        canActivate:[authorizationGuard],
        children:[
            {
                path:'play',
                loadComponent: () => import('./play/play').then((m) => m.Play),
                title: 'Play'
            }
        ]
       
    }, 
    {
        path: '**',
        redirectTo: 'not-found'
    },
];
