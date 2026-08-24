import { Component, HostListener, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, RouterLink } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  isLoggedIn$!: Observable<boolean>;
  isSidebarOpen = true;
  isMobile = false;
  userName = '';
  email = '';
  role = '';
  menuItems = [
    { label: 'Dashboard', icon: 'bi bi-speedometer2', route: '/Dashboard' },
    { label: 'Products', icon: 'bi bi-box-seam', route: '/Products' },
    { label: 'Categories', icon: 'bi bi-grid', route: '/Categories' },
    { label: 'Suppliers', icon: 'bi bi-truck', route: '/Suppliers' },
    { label: 'Customers', icon: 'bi bi-people', route: '/Customers' },
    { label: 'Orders', icon: 'bi bi-cart-check', route: '/Orders' },
     {
      label: 'Login',
      icon: 'bi bi-box-arrow-in-right',
      route: '/Login',
      guestOnly: true,
    },
    {
      label: 'Register',
      icon: 'bi bi-person-plus',
      route: '/Register',
      guestOnly: true,
    },
  ];

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.userName = localStorage.getItem('userName') ?? '';
    this.email = localStorage.getItem('email') ?? '';
    this.role = localStorage.getItem('role') ?? '';
    // initialize observable after authService is available
    this.isLoggedIn$ = this.authService.isLoggedIn$;
    this.checkScreen();
  }

  @HostListener('window:resize')
  onResize() {
    this.checkScreen();
  }

  checkScreen() {
    const mobile = window.innerWidth < 768;

    if (mobile !== this.isMobile) {
      this.isMobile = mobile;

      this.isSidebarOpen = !mobile;
    }
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  logout() {
    this.authService.logout().subscribe(() => {
      this.router.navigate(['/Login']);
    });
  }
}
