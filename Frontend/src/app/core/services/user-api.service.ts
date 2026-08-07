import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

import { apiEndpoints } from '@app/core/config/api-endpoints';
import { ServiceResponse } from '@app/core/models/service-response.model';

import {
  UserResponse,
  CreateUserRequest,
  userFilterRequest,
  searchUserRequest,
  paginationRequest,
  updateUserRequest
} from '../models/userModels/user.model';
import { ManagerResponse } from '../models/projectModels/project.model';

@Injectable({
  providedIn: 'root'
})

export class UserApiService {
  public constructor(private readonly httpClient: HttpClient) { }

  public getAllUsers(request: paginationRequest): Observable<ServiceResponse<UserResponse[]>> {
    const params = new HttpParams()
      .set('pageNumber', request.pageNumber)
      .set('pageSize', request.pageSize);

    return this.httpClient.get<ServiceResponse<UserResponse[]>>(
      `${apiEndpoints.user}/GetAllUsers`,
      { params }
    );
  }

  public getUserById(userId: string): Observable<ServiceResponse<UserResponse>> {
    return this.httpClient.get<ServiceResponse<UserResponse>>(
      `${apiEndpoints.user}/GetUserById/${userId}`
    );
  }

  public getUserBySearch(request: searchUserRequest): Observable<ServiceResponse<UserResponse[]>> {

    const params = new HttpParams().set('searchTerm', request.searchTerm ?? '');

    return this.httpClient.get<ServiceResponse<UserResponse[]>>(
      `${apiEndpoints.user}/GetUserBySearch`,
      { params }
    );
  }

  public getUsersByFilter(request: userFilterRequest): Observable<ServiceResponse<UserResponse[]>> {
    return this.httpClient.post<ServiceResponse<UserResponse[]>>(
      `${apiEndpoints.user}/GetUsersByFilter`,
      { request }
    );
  }

  public createUser(request: CreateUserRequest): Observable<ServiceResponse<string>> {
    return this.httpClient.post<ServiceResponse<string>>(
      `${apiEndpoints.auth}/Register`,
      request
    );
  }

  public getManagers(): Observable<ServiceResponse<ManagerResponse[]>> {
    return this.httpClient.get<ServiceResponse<ManagerResponse[]>>(
      `${apiEndpoints.user}/GetManagers`
    );
  }

  public updateUser(userId: string, request: updateUserRequest): Observable<ServiceResponse<string>> {
    return this.httpClient.put<ServiceResponse<string>>(
      `${apiEndpoints.user}/UpdateUser/${userId}`,
      request
    );
  }
}
