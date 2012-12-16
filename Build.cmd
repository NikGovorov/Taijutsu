@echo Off
mkdir Output
mkdir Packages
.\Sources\.nuget\nuget.exe install .\Sources\.nuget\packages.config -solutionDir .\
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Build\Taijutsu.proj /v:M /fl /flp:LogFile=Output\msbuild.log;Verbosity=Normal /nr:false