import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { AllCategoriesComponent } from './components/category/all-categories/all-categories.component';
import { AllSuppliersComponent } from './components/supplier/all-suppliers/all-suppliers.component';
import { AllCustomersComponent } from './components/customer/all-customers/all-customers.component';
import { RegisterComponent } from './components/auth/register/register.component';
import { LoginComponent } from './components/auth/login/login.component';
import { authGuard } from './guard/auth.guard';
import { AllOrdersComponent } from './components/order/all-orders/all-orders.component';
import { AddOrderComponent } from './components/order/add-order/add-order.component';
import { EditOrderComponent } from './components/order/edit-order/edit-order.component';
import { CartComponent } from './components/cart/cart/cart.component';

export const routes: Routes = [
    {
        path: '',
        component: DashboardComponent,
        
    },
    {
        path: 'Products',
        loadComponent: () => import('./components/all-products/all-products.component').then(m => m.AllProductsComponent),
        canActivate:[authGuard]
    },
    {
        path: 'Products/:catId',
        loadComponent: () => import('./components/all-products/all-products.component').then(m => m.AllProductsComponent),
        canActivate:[authGuard]
    },
    {
        path:'Categories',
        component:AllCategoriesComponent,
        canActivate:[authGuard]
    },
    {
        path:'Suppliers',
        component:AllSuppliersComponent,
        canActivate:[authGuard]

    },
    {
        path:'Customers',
        component:AllCustomersComponent,
        canActivate:[authGuard]

    },
    {
        path:'Orders',
        component:AllOrdersComponent,
        canActivate:[authGuard]
    },
    {
        path:'Orders/AddOrder',
        component:AddOrderComponent,
        canActivate:[authGuard],
    },
    {
        path:'Orders/EditOrder/:orderId',
        component:EditOrderComponent,
        canActivate:[authGuard],
    },
    {
        path:'Cart/:orderId',
        component:CartComponent,
        canActivate:[authGuard]
    },
    {
        path:'Register',
        component: RegisterComponent
    },
    {
        path:'Login',
        component:LoginComponent
    },
    
    {
        path: 'Dashboard',
        component: DashboardComponent,
        
    }
];
