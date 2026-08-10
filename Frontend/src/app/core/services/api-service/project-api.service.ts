import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { apiEndpoints } from "../../config/api-endpoints";
import { ServiceResponse } from "../../models/service-response.model";
import { Observable } from "rxjs/internal/Observable";
import { projectCreateRequest, projectResponse, projectUpdateRequest, ProjectUserResponse } from "../../models/projectModels/project.model";

@Injectable({
  providedIn: "root"
})
export class ProjectApiService {
  public constructor(private readonly httpClient: HttpClient) { }

  public createProject(request: projectCreateRequest): Observable<ServiceResponse<string>> {
    return this.httpClient.post<ServiceResponse<string>>(`${apiEndpoints.project}/CreateProject`, request);
  }

  public getAllProjects(): Observable<ServiceResponse<projectResponse[]>> {
    return this.httpClient.get<ServiceResponse<projectResponse[]>>(`${apiEndpoints.project}/GetAllProjects`);
  }

  public getProjectById(projectId: string): Observable<ServiceResponse<projectResponse>> {
    return this.httpClient.get<ServiceResponse<projectResponse>>(`${apiEndpoints.project}/GetProjectById/${projectId}`);
  }

  public updateProject(request: projectUpdateRequest): Observable<ServiceResponse<string>> {
    return this.httpClient.put<ServiceResponse<string>>(`${apiEndpoints.project}/UpdateProject`, request);
  }

  public getProjectEmployees(projectId: string): Observable<ServiceResponse<ProjectUserResponse[]>> {
    return this.httpClient.get<ServiceResponse<ProjectUserResponse[]>>(`${apiEndpoints.project}/GetProjectEmployees/${projectId}`);
  }

  public getProjectsByManagerId(managerId: string): Observable<ServiceResponse<projectResponse[]>> {
    return this.httpClient.get<ServiceResponse<projectResponse[]>>(`${apiEndpoints.project}/GetProjectsByManagerId/${managerId}`);
  }

  public getEmployeeProjects(userId: string): Observable<ServiceResponse<projectResponse[]>> {
    return this.httpClient.get<ServiceResponse<projectResponse[]>>(`${apiEndpoints.project}/GetEmployeeProjects/${userId}`);
  }
}
