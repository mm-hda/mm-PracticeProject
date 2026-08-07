export interface CreatePositionRequest {
  name: string;
  departmentId: string;
}

export interface UpdatePositionRequest {
  id: string;
  name: string;
  departmentId: string;
}

export interface PositionResponse {
  id: string;
  name?: string;
  departmentId: string;
  departmentName?: string;
  totalUsers: number;
}

export interface PositionUserResponse {
  userId: string;
  name?: string;
  email?: string;
  dob?: string | null;
  branchName?: string;
  departmentName?: string;
  positionName?: string;
  roleName?: string;
}
