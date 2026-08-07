export interface projectCreateRequest {
  name: string;
  description?: string;
  startDate: string;
  endDate: string | null;
  projectManagerId: string;
}


export interface projectResponse {
  id: string;
  name: string;
  description?: string;
  startDate: string;
  endDate: string;
  projectManagerId: string;
  projectManagerName?: string;
  totalUsers?: number;
}

export interface ProjectUserResponse {
  UserId: string;
  name?: string;
  email?: string;
  dob?: string | null;
  branchName?: string;
  departmentName?: string;
  positionName?: string;
  roleName?: string;
}

export interface projectUpdateRequest {
  id: string;
  name: string;
  description?: string;
  startDate: string;
  endDate: string | null;
  projectManagerId: string;
}

export interface ManagerResponse {
  userId: string;
  name: string;
  email?: string;
}
