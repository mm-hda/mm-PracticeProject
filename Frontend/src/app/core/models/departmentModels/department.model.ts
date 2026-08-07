export interface CreateDepartmentRequest {
  name: string;
}

export interface UpdateDepartmentRequest {
  id: string;
  name: string;
}

export interface DepartmentResponse {
  id: string;
  name?: string;
  totalPositions: number;
  totalUsers: number;
}

export interface DepartmentUserResponse {
  userId: string;
  name?: string;
  email?: string;
  dob?: string | null;
  branchName?: string;
  departmentName?: string;
  positionName?: string;
  roleName?: string;
}
