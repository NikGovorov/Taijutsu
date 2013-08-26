@echo Off
set key=%1
if "%key%" == "" (
   set key="Empty"
)
mkdir Output
mkdir Packages
.\Sources\.nuget\nuget.exe restore .\Sources\Taijutsu.sln
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Build\Taijutsu.proj /p:NugetKey=%key% /v:M /fl /flp:LogFile=Output\msbuild.log;Verbosity=Normal /nr:false