set CURPATH=%cd%

..\Excel2Json.exe 
..\Excel2Json -f %CURPATH%\example.xlsx -o .\
pause