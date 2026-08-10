import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

import { apiEndpoints } from '@app/core/config/api-endpoints';
import { ServiceResponse } from '@app/core/models/service-response.model';
import {
  BranchResponse,
  BranchUserResponse,
  CreateBranchRequest,
  UpdateBranchRequest
} from '@app/core/models/branchModels/branch.model';

@Injectable({
  providedIn: 'root'
})
export class BranchApiService {
  public constructor(
    private readonly httpClient: HttpClient
  ) { }

  public getAllBranches(): Observable<ServiceResponse<BranchResponse[]>> {
    return this.httpClient.get<ServiceResponse<BranchResponse[]>>(
      `${apiEndpoints.branch}/GetAllBranches`
    );
  }

  public getBranchById(branchId: string): Observable<ServiceResponse<BranchResponse>> {
    return this.httpClient.get<ServiceResponse<BranchResponse>>(
      `${apiEndpoints.branch}/GetBranchById/${branchId}`
    );
  }

  public getBranchEmployees(branchId: string): Observable<ServiceResponse<BranchUserResponse[]>> {
    return this.httpClient.get<ServiceResponse<BranchUserResponse[]>>(
      `${apiEndpoints.branch}/GetBranchUsers/${branchId}`
    );
  }

  public createBranch(request: CreateBranchRequest): Observable<ServiceResponse<string>> {
    return this.httpClient.post<ServiceResponse<string>>(
      `${apiEndpoints.branch}/CreateBranch`,
      request
    );
  }

  public updateBranch(request: UpdateBranchRequest): Observable<ServiceResponse<string>> {
    return this.httpClient.put<ServiceResponse<string>>(
      `${apiEndpoints.branch}/UpdateBranch`,
      request
    );
  }
}
