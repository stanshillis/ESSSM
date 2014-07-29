@echo off
cd build
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '..\tools\psake-4.3.2\psake.ps1' %*"
cd ..
pause