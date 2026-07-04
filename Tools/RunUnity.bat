@echo off

if exist "C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe" set unityPath="C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe"
if exist "D:\Program Files\Unity\Editor\6000.3.8f1\Editor\Unity.exe" set unityPath="D:\Program Files\Unity\Editor\6000.3.8f1\Editor\Unity.exe"

set displayLogs=0


start "" %unityPath% -projectPath %cd%\.. -logFile "%cd%\..\Logs\CurrentLogs.log" 

if %displayLogs%==1 (
	powershell -Command "Get-Content '..\Logs\CurrentLogs.log' -Wait"
)
