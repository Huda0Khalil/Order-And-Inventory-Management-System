import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Actions, Breadcrumb } from '../../models/breadcrumb';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-top-bar',
  imports: [CommonModule, RouterLink],
  templateUrl: './top-bar.component.html',
  styleUrl: './top-bar.component.css'
})
export class TopBarComponent {
@Input() breadcrumbs: Breadcrumb[] = [];
@Input() title: string = '';
@Input() haveSelect: boolean = false;
@Input() selectionChange: any[] = [];
@Output() selectedValue = new EventEmitter<any>();
@Input() actions: Actions[] = [];
@Output() actionClicked = new EventEmitter<string>();

}
