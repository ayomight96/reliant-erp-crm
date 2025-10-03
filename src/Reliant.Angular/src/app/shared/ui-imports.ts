// src/app/shared/ui-imports.ts
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { DividerModule } from 'primeng/divider';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageModule } from 'primeng/message';
import { SkeletonModule } from 'primeng/skeleton';
import { DialogModule } from 'primeng/dialog';
import { ProgressBarModule } from 'primeng/progressbar';
import { TooltipModule } from 'primeng/tooltip';
import { RouterModule } from '@angular/router';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

export const UI_IMPORTS = [
  CommonModule,
  FormsModule,
  ReactiveFormsModule,
  InputTextModule,
  PasswordModule,
  ButtonModule,
  CardModule,
  TableModule,
  TagModule,
  DropdownModule,
  InputNumberModule,
  ToastModule,
  DividerModule,
  CheckboxModule,
  MessageModule,
  SkeletonModule,
  DialogModule,
  ProgressBarModule,
  TooltipModule,
  RouterModule,
  ConfirmDialogModule
];

export const UI_PROVIDERS = [DecimalPipe];
