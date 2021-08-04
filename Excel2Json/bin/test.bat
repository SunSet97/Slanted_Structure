set CURPATH=%cd%

..\Excel2Json.exe 
..\Excel2Json -f %CURPATH%\test.xlsx -o .\

pause