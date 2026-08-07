import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

import { apiEndpoints } from '@app/core/config/api-endpoints';
import { ServiceResponse } from '@app/core/models/service-response.model';
import { CreatePositionRequest, PositionResponse, PositionUserResponse, UpdatePositionRequest } from '../models/positionModels/position.model';

@Injectable({
  providedIn: 'root'
})

export class PositionApiService {
  public constructor(
    private readonly httpClient: HttpClient
  ) { }

  public getAllPositions(): Observable<ServiceResponse<PositionResponse[]>> {
    return this.httpClient.get<ServiceResponse<PositionResponse[]>>(`${apiEndpoints.position}/GetAllPositions`);
  }

  public getPositionById(PositionId: string): Observable<ServiceResponse<PositionResponse>> {
    return this.httpClient.get<ServiceResponse<PositionResponse>>(`${apiEndpoints.position}/GetPositionById/${PositionId}`);
  }

  public getPositionEmployees(PositionId: string): Observable<ServiceResponse<PositionUserResponse[]>> {
    return this.httpClient.get<ServiceResponse<PositionUserResponse[]>>(`${apiEndpoints.position}/GetPositionUsers/${PositionId}`);
  }

  public createPosition(request: CreatePositionRequest): Observable<ServiceResponse<string>> {
    return this.httpClient.post<ServiceResponse<string>>(`${apiEndpoints.position}/CreatePosition`, request);
  }

  public updatePosition(request: UpdatePositionRequest): Observable<ServiceResponse<string>> {
    return this.httpClient.put<ServiceResponse<string>>(`${apiEndpoints.position}/UpdatePosition`, request);
  }

  public getPositionByDepartment(departmentId: string): Observable<ServiceResponse<PositionResponse[]>> {
    return this.httpClient.get<ServiceResponse<PositionResponse[]>>(`${apiEndpoints.position}/GetPositionsByDepartment/${departmentId}`);
  }
}
