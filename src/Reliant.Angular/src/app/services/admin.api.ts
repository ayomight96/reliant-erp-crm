import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, of, throwError } from 'rxjs';
import { UserListItem } from './api.models';

@Injectable({ providedIn: 'root' })
export class AdminApi {
  constructor(private http: HttpClient) {}

  /** Lists all users (id, email, fullName, roles[]) */
  listUsers() {
    return this.http.get<UserListItem[]>('/api/admin/users').pipe(
      // defensive: ensure array
      map((res) => (Array.isArray(res) ? res : [])),
      catchError((err: HttpErrorResponse) => {
        return throwError(() => err); // let component show a toast
      })
    );
  }

  /** Assigns a role to a user. Backend returns 204 No Content on success. */
  assignRole(userId: number, roleName: string) {
    return this.http.post<void>(
      `/api/admin/users/${userId}/roles/${encodeURIComponent(roleName)}`,
      null,
      { observe: 'response' }
    );
  }
}
