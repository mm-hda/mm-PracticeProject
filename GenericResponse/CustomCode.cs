using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace backend.GenericResponse;

internal static class CustomCodes
{
    public const int DatabaseDependencyNotFound = 1002;

    public const int InvalidCredentials = 2001;
    public const int UnauthorizedAccess = 2002;
    public const int AccessForbidden = 2003;

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
    public const int UserAlreadyAssignedToProject = 5009;

    public const int ProjectAlreadyExists = 6001;

    public const int PageNumberExceeds = 7001;
    public const int ProjectEnded = 7002;

    public const int InternalServerError = 9000;
    public const int RoleCreationFailed = 9001;
    public const int UserCreationFailed = 9002;
    public const int ProjectCreationFailed = 9003;
    public const int EmployeeProjectCreationFailed = 9008;
    public const int BranchCreationFailed = 9004;
    public const int BranchUpdateFailed = 9005;
    public const int PositionCreationFailed = 9006;
    public const int ProjectUpdateFailed = 9007;
    public const int DepartmentCreationFailed = 9009;
    public const int DepartmentUpdateFailed = 9010;
    public const int EmployeeProjectRemovalFailed = 9011;
    public const int PositionUpdateFailed = 9012;

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

    public const int PasswordTooShort = 900;
    public const int PasswordTooLong = 901;
    public const int PasswordNotStrong = 902;
    public const int InvalidEmail = 903;
    public const int NameRequired = 904;
    public const int NameTooShort = 905;
    public const int NameTooLong = 906;
    public const int FullNameRequired = 907;
    public const int InvalidNameFormat = 908;
    public const int SqlInjectionDetected = 909;
    public const int HtmlDetected = 910;
    public const int EmailRequired = 911;
    public const int EmailTooLong = 912;

    public const int PasswordRequired = 913;
    public const int InvalidPasswordFormat = 914;
    public const int DOBRequired = 915;
    public const int InvalidAge = 916;
    public const int LocationRequired = 917;
    public const int LocationTooShort = 918;
    public const int LocationTooLong = 919;
    public const int InvalidLocationFormat = 920;
    public const int BranchNameRequired = 921;
    public const int BranchNameTooShort = 922;
    public const int BranchNameTooLong = 923;
    public const int InvalidBranchName = 924;
    public const int DepartmentNameRequired = 925;
    public const int DepartmentNameTooShort = 926;
    public const int DepartmentNameTooLong = 927;
    public const int InvalidDepartmentName = 928;
    public const int PositionNameRequired = 929;
    public const int PositionNameTooShort = 930;
    public const int PositionNameTooLong = 931;
    public const int InvalidPositionName = 932;
    public const int InvalidStartDate = 933;
    public const int InvalidEndDate = 934;
    public const int ProjectNameRequired = 935;
    public const int ProjectNameTooShort = 936;
    public const int ProjectNameTooLong = 937;
    public const int DescriptionTooLong = 938;
    public const int InvalidProjectName = 939;
    public const int StartDateRequired = 940;
    public const int RoleNameRequired = 941;
    public const int RoleNameTooShort = 942;
    public const int RoleNameTooLong = 943;
    public const int InvalidRoleName = 944;

    public const int OperationCancelled = 9999;
}
