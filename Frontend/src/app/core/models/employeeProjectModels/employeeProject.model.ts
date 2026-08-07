export interface CreateEmployeeProjectRequest {
  userId: string;
  projectId: string;
}

export interface EmployeeProjectResponse {
  id: string;
  userId: string;
  userName: string;
  userEmail: string;
  roleName: string;
  projectId: string;
  projectName: string;
  assignedDate: Date;
}
