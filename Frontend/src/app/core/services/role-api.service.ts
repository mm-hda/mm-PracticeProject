import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs/internal/Observable";
import { roleResponse } from "../models/roleModels/role.model";
import { ServiceResponse } from "../models/service-response.model";
import { apiEndpoints } from "../config/api-endpoints";


@Injectable({
  providedIn: "root"
})

export class RoleApiService {
  public constructor(private readonly httpClient: HttpClient) { }

  public getAllRoles(): Observable<ServiceResponse<roleResponse[]>> {
    return this.httpClient.get<ServiceResponse<roleResponse[]>>(`${apiEndpoints.role}/GetAllRoles`);
  }
}
