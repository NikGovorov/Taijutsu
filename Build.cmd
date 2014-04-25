@echo Off
set nugeturl=%1
if "%nugeturl%" == "" (
   set nugeturl="http://nuget.org/"
)
set nugetkey=%2
if "%nugetkey%" == "" (
   set nugetkey="Empty"
)

mkdir Output
mkdir Packages
.\Sources\.nuget\nuget.exe restore .\Sources\Taijutsu.sln
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Build\Taijutsu.proj /p:NugetKey=%nugetkey%;NugetUrl=%nugeturl% /v:M /fl /flp:LogFile=Output\msbuild.log;Verbosity=Normal /nr:false