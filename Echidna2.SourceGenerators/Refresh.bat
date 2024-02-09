@echo off
rmdir bin /s /q
move obj\project.assets.json .
rmdir obj /s /q
mkdir obj
move project.assets.json obj
taskkill /f /im dotnet.exe