@echo off
setlocal

rem Define paths and project details
set "bootstrap_project=..\src\ClassicUO.Bootstrap\src\ClassicUO.Bootstrap.csproj"
set "client_project=..\src\ClassicUO.Client"
set "output_directory=..\bin\dist"
set "target="

rem Determine the platform
if /I "%OS%"=="Windows_NT" (
    set "target=win-x64"
) else (
    echo Unsupported platform: %OS%
    exit /b 1
)


dotnet publish "%bootstrap_project%" -c Release -o "%output_directory%"
dotnet publish "%client_project%" -c Release -p:NativeLib=Shared -p:OutputType=Library -r %target% -o "%output_directory%"

endlocal

