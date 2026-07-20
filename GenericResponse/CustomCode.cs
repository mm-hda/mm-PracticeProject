using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace backend.GenericResponse;

internal static class CustomCodes
{
    public const int DatabaseConnectionFailed = 1001;
    public const int DatabaseDependencyNotFound = 1002;

    public const int InvalidCredentials = 2001;
    public const int JwtExpired = 2002;
    public const int Unauthorized = 2003;

    public const int ValidationFailed = 3001;
    public const int InvalidInput = 3002;
    public const int InputsNotFound = 3003;

    public const int UserAlreadyExists = 4001;
    public const int BranchAlreadyExists = 4002;
    public const int RoleAlreadyExists = 4003;
    public const int PositionAlreadyExists = 4004;
    public const int DepartmentAlreadyExists = 4005;

    public const int UserNotFound = 5001;
    public const int BranchNotFound = 5002;
    public const int RoleNotFound = 5003;
    public const int DepartmentNotFound = 5004;
    public const int PositionNotFound = 5005;
    public const int ProjectNotFound = 5006;
    public const int EmployeeProjectNotFound = 5007;
    public const int ProjectManagerNotFound = 5008;
    public const int UserNotAssignedToProject = 5009;
    public const int UserAlreadyAssignedToProject = 5010;

    public const int ProjectAlreadyExists = 6001;

    public const int PageNumberExceeds = 7001;
    public const int PageSizeExceeds = 7002;
    public const int ProjectEnded = 7003;

    public const int InternalServerError = 9000;
    public const int RoleCreationFailed = 9001;
    public const int UserCreationFailed = 9002;
    public const int ProjectCreationFailed = 9003;
    public const int EmployeeProjectCreationFailed = 9008;
    public const int BranchCreationFailed = 9004;
    public const int BranchUpdateFailed = 9005;
    public const int PositionCreationFailed = 9005;
    public const int ProjectUpdateFailed = 9006;
    public const int PositionUpdateFailed = 9007;
    public const int DepartmentCreationFailed = 9010;
    public const int DepartmentUpdateFailed = 9011;
    public const int EmployeeProjectRemovalFailed = 9009;

    public const int RoleCreatedSuccessfully = 700;
    public const int UserCreatedSuccessfully = 701;
    public const int BranchCreatedSuccessfully = 702;
    public const int ProjectCreatedSuccessfully = 703;
    public const int PositionCreatedSuccessfully = 704;
    public const int EmployeeProjectCreatedSuccessfully = 707;
    public const int ProjectUpdatedSuccessfully = 705;
    public const int PositionUpdatedSuccessfully = 706;
    public const int EmployeeProjectRemovedSuccessfully = 709;
    public const int DepartmentCreatedSuccessfully = 710;
    public const int DepartmentUpdatedSuccessfully = 711;
    public const int BranchUpdatedSuccessfully = 712;
    public const int LoginSuccessfully = 713;

    public const int DataRetrieved = 200;
}

