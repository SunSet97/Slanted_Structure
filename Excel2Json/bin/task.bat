set CURPATH=%cd%

..\Excel2Json.exe 
..\Excel2Json -f %CURPATH%\task.xlsx -o .\

pause