@echo off
set executable=ConsoleApp1.exe
set process=ConsoleApp1.exe 
:begin
tasklist |>nul findstr /b /l /i /c:%process% || start "" "%executable%"
timeout /t 3 /nobreak >nul
goto :begin
