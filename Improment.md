- Refactor Program.cs file
	'<Main>$' is coupled with '52' different types from '29' different namespaces. Rewrite or refactor the code to decrease its class coupling below '41'. (https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1506) [d:\Playground\Mentor\mm-PracticeProject\backend.csproj]

- JWT tokens are being logged, also it is there in appsettings 

- Now you already has Global exception MIddleware and validator, so we can avoid multiple/repetative failure condition, catch statement from controllers

- We can avoid Tuples and use some better solution for that, because item1, item2, ...., itemN not comes under best practice.

- Introduce CancellationToken in Application

- notice repeated null check condition 
  eg. CreateEmployeeProjectAsync(in controller) alrady check for null then again cheking for null in service
  
- reduce exception (not all) from controller and delegate it to Services

- Duplicate error code 9005

- Move ErrorCode to Domain, as it will be persistance
