powershell -Command "(gc .\PRJReports\App.config) -replace '<Database connection string>', 'bar' | Out-File -encoding ASCII .\PRJReports\App.config"
nuget restore .\JiraReporters.sln 
msbuild .\PRJReports\PRJReports.csproj