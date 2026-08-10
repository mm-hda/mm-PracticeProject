export interface userFilterRequest {
  roleId?: string;
  branchId?: string;
  departmentId?: string;
  positionId?: string;
}


export interface UserResponse {
  userId: string;
  name?: string;
  email?: string;
  dob?: string | null;
  branchName?: string;
  departmentName?: string;
  positionName?: string;
  roleName?: string;
}

export interface searchUserRequest {
  searchTerm?: string;
}

export interface paginationRequest {
  pageNumber: number;
  pageSize: number;
}

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  dob?: string | null;
  branchId: string;
  departmentId: string;
  positionId: string;
  roleId: string;
}


export interface updateUserRequest {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  dob: string | null;
  branchId: string;
  departmentId: string;
  positionId: string;
  roleId: string;
}
