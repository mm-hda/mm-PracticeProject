export const statusCodeMessages: Record<number, string> = {
  // Success
  200: 'Data retrieved successfully.',

  700: 'Role created successfully.',
  701: 'User created successfully.',
  702: 'Branch created successfully.',
  703: 'Project created successfully.',
  704: 'Position created successfully.',
  705: 'Project updated successfully.',
  706: 'Position updated successfully.',
  707: 'Employee project created successfully.',
  709: 'Employee project removed successfully.',
  710: 'Department created successfully.',
  711: 'Department updated successfully.',
  712: 'Branch updated successfully.',
  713: 'Login successful.',

  // Validation
  900: 'Password is too short.',
  901: 'Password is too long.',
  902: 'Password is not strong enough.',
  903: 'Invalid email address.',
  904: 'Name is required.',
  905: 'Name is too short.',
  906: 'Name is too long.',
  907: 'Full name is required.',
  908: 'Invalid name format.',
  909: 'Potential SQL injection detected.',
  910: 'HTML content is not allowed.',
  911: 'Email is required.',
  912: 'Email is too long.',
  913: 'Password is required.',
  914: 'Invalid password format.',
  915: 'Date of birth is required.',
  916: 'Invalid age.',
  917: 'Location is required.',
  918: 'Location is too short.',
  919: 'Location is too long.',
  920: 'Invalid location format.',
  921: 'Branch name is required.',
  922: 'Branch name is too short.',
  923: 'Branch name is too long.',
  924: 'Invalid branch name.',
  925: 'Department name is required.',
  926: 'Department name is too short.',
  927: 'Department name is too long.',
  928: 'Invalid department name.',
  929: 'Position name is required.',
  930: 'Position name is too short.',
  931: 'Position name is too long.',
  932: 'Invalid position name.',
  933: 'Invalid start date.',
  934: 'Invalid end date.',
  935: 'Project name is required.',
  936: 'Project name is too short.',
  937: 'Project name is too long.',
  938: 'Description is too long.',
  939: 'Invalid project name.',
  940: 'Start date is required.',
  941: 'Role name is required.',
  942: 'Role name is too short.',
  943: 'Role name is too long.',
  944: 'Invalid role name.',

  // Authentication
  2001: 'Invalid credentials.',
  2002: 'Unauthorized access.',
  2003: 'Access forbidden.',

  // General Errors
  1002: 'Database dependency is not available.',
  3002: 'Invalid input provided.',
  3003: 'Required input data was not found.',

  // Already Exists
  4001: 'User already exists.',
  4002: 'Branch already exists.',
  4003: 'Role already exists.',
  4004: 'Position already exists.',
  4005: 'Department already exists.',
  6001: 'Project already exists.',

  // Not Found
  5001: 'User not found.',
  5002: 'Branch not found.',
  5003: 'Role not found.',
  5004: 'Department not found.',
  5005: 'Position not found.',
  5006: 'Project not found.',
  5007: 'Employee project assignment not found.',
  5008: 'Project manager not found.',
  5009: 'User is already assigned to the project.',

  // Business Rules
  7001: 'Requested page number exceeds available pages.',
  7002: 'Project has already ended.',

  // System Errors
  9000: 'An internal server error occurred.',
  9001: 'Role creation failed.',
  9002: 'User creation failed.',
  9003: 'Project creation failed.',
  9004: 'Branch creation failed.',
  9005: 'Branch update failed.',
  9006: 'Position creation failed.',
  9007: 'Project update failed.',
  9008: 'Employee project creation failed or position update failed.',
  9009: 'Department creation failed.',
  9010: 'Department update failed.',
  9011: 'Employee project removal failed.',

  // Operation
  9999: 'Operation was cancelled.'
};

export function getStatusCodeMessage(statusCode: number): string {
  return statusCodeMessages[statusCode] ?? 'Request failed. Please try again.';
}
