import { Component, OnInit, signal } from '@angular/core';
import { ICategory } from '../../../models/ICategory';
import { CommonModule } from '@angular/common';
import { TopBarComponent } from '../../top-bar/top-bar.component';
import { FormsModule } from '@angular/forms';
import { CategoryApiService } from '../../../services/category-api.service';
import { RouterLink } from '@angular/router';
import { AddEditCategoryComponent } from '../add-edit-category/add-edit-category.component';
import Swal from 'sweetalert2';
import { Actions } from '../../../models/breadcrumb';

@Component({
  selector: 'app-all-categories',
  imports: [
    CommonModule,
    TopBarComponent,
    FormsModule,
    RouterLink,
    AddEditCategoryComponent,
  ],
  templateUrl: './all-categories.component.html',
  styleUrl: './all-categories.component.css',
})
export class AllCategoriesComponent implements OnInit {
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  categories: any[] = [];
  filteredCategory: any;
  searchText: string = '';
  showModal = false;
  modalMode: 'add' | 'edit' = 'add';
  selectedCategory?: ICategory;
  deleteTarget = signal<any | null>(null);
  protected readonly Math = Math;
  action: Actions[] = [
    {
      label: 'New Category',
      icon: 'bi bi-plus',
      style: { 'background-color': 'blue', color: 'white' },
      func: 'openAddModel',
    },
  ];  
  constructor(private categoryApi: CategoryApiService) {}
  ngOnInit(): void {
    this.loadCategories();
  }
  loadCategories() {
    this.categoryApi
      .getAllCategories(this.pageNumber, this.pageSize)
      .subscribe({
        next: (data) => {
          this.categories = data.items; // or response.data
          this.filteredCategory = data.items;
          this.totalCount = data.totalCount;
          console.log(this.filteredCategory);
        },
        error: (err) => {
          console.error('Error fetching categories:', err);
        },
      });
    this.showModal = false;
  }
  applyFilters() {
    this.filteredCategory = this.categories.filter((cat) => {
      const matchesSearch =
        this.searchText === '' ||
        cat.id.toString().includes(this.searchText) ||
        cat.name.toLowerCase().includes(this.searchText.toLowerCase());
      return matchesSearch;
    });
  }
  openAddModel() {
    this.showModal = true;
    this.modalMode = 'add';
  }
  closeModel() {
    this.showModal = false;
    this.modalMode = 'add';
  }
  confirmDelete(cat: ICategory) {
    this.deleteTarget.set(cat);
    this.doDelete();
  }
  doDelete() {
    const target = this.deleteTarget();
    if (!target) return;
    Swal.fire({
      title: 'Are you sure?',
      text: 'You will not be able to recover this category!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Yes, delete it!',
      cancelButtonText: 'No, keep it',
      preConfirm: () => {
        return new Promise((resolve, reject) => {
          this.categoryApi.deleteCategory(target.id).subscribe({
            next: () => resolve(true),
            error: () => reject(new Error('Delete failed')),
          });
        });
      },
    })
      .then((result) => {
        if (result.isConfirmed) {
          this.deleteTarget.set(null);
          this.loadCategories();
          Swal.fire({
            title: 'Deleted!',
            text: 'Your product has been deleted.',
            icon: 'success',
          });
        }
      })
      .catch(() => {
        Swal.fire({
          title: 'Error',
          text: 'Failed to delete the product.',
          icon: 'error',
        });
      });
  }
  openEdit(selectedCat: ICategory) {
    this.selectedCategory = selectedCat;
    this.modalMode = 'edit';
    this.showModal = true;
  }
  nextPage() {
    if (this.pageNumber * this.pageSize < this.totalCount) {
      this.pageNumber++;
      this.loadCategories();
    }
  }
  previousPage() {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.loadCategories();
    }
  }
}
