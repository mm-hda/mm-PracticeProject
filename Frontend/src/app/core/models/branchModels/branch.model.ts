
export interface CreateBranchRequest {
  name: string;
  location: string;
}

export interface UpdateBranchRequest {
  id: string;
  name: string;
  location: string;
}

export interface BranchResponse {
  id: string;
  name?: string;
  location?: string;
  totalUsers: number;
}

export interface BranchUserResponse {
  userId: string;
  name?: string;
  email?: string;
  dob?: string | null;
  branchName?: string;
  departmentName?: string;
  positionName?: string;
  roleName?: string;
}
