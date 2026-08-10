import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

import { apiEndpoints } from '@app/core/config/api-endpoints';
import { ServiceResponse } from '@app/core/models/service-response.model';
import {
  DepartmentResponse,
  DepartmentUserResponse,
  CreateDepartmentRequest,
  UpdateDepartmentRequest
} from '@app/core/models/departmentModels/department.model';

@Injectable({
  providedIn: 'root'
})
export class DepartmentApiService {
  public constructor(
    private readonly httpClient: HttpClient
  ) { }

  public getAllDepartments(): Observable<ServiceResponse<DepartmentResponse[]>> {
    return this.httpClient.get<ServiceResponse<DepartmentResponse[]>>(
      `${apiEndpoints.department}/GetAllDepartments`
    );
  }

  public getDepartmentById(DepartmentId: string): Observable<ServiceResponse<DepartmentResponse>> {
    return this.httpClient.get<ServiceResponse<DepartmentResponse>>(
      `${apiEndpoints.department}/GetDepartmentById/${DepartmentId}`
    );
  }

  public getDepartmentEmployees(DepartmentId: string): Observable<ServiceResponse<DepartmentUserResponse[]>> {
    return this.httpClient.get<ServiceResponse<DepartmentUserResponse[]>>(
      `${apiEndpoints.department}/GetDepartmentEmployees/${DepartmentId}`
    );
  }

  public createDepartment(request: CreateDepartmentRequest): Observable<ServiceResponse<string>> {
    return this.httpClient.post<ServiceResponse<string>>(
      `${apiEndpoints.department}/CreateDepartment`,
      request
    );
  }

  public updateDepartment(request: UpdateDepartmentRequest): Observable<ServiceResponse<string>> {
    return this.httpClient.put<ServiceResponse<string>>(
      `${apiEndpoints.department}/UpdateDepartment`,
      request
    );
  }
}
