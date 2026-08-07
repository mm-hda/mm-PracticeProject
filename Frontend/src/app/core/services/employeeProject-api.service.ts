import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { apiEndpoints } from "../config/api-endpoints";
import { ServiceResponse } from "../models/service-response.model";
import { Observable } from "rxjs/internal/Observable";
import { CreateEmployeeProjectRequest } from "../models/employeeProjectModels/employeeProject.model";

@Injectable({
  providedIn: "root"
})

export class EmployeeProjectApiService {
  public constructor(private readonly httpClient: HttpClient) { }

  public CreateEmployeeProject(request: CreateEmployeeProjectRequest): Observable<ServiceResponse<string>> {
    return this.httpClient.post<ServiceResponse<string>>(`${apiEndpoints.employeeProject}/CreateEmployeeProject`, request);
  }

  public DeleteEmployeeProject(employeeProjectId: string): Observable<ServiceResponse<string>> {
    return this.httpClient.delete<ServiceResponse<string>>(`${apiEndpoints.employeeProject}/RemoveEmployeeProject/${employeeProjectId}`);
  }

  public GetEmployeeProjectsByUserId(userId: string): Observable<ServiceResponse<any>> {
    return this.httpClient.get<ServiceResponse<any>>(`${apiEndpoints.employeeProject}/GetUserProjectsByUserId/${userId}`);
  }
}
