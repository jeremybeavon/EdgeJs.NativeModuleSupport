rem Argument 1: packages directory
rem Argument 2: project directory
setlocal
set nodejspath=%1\Ncapsulate.Node.0.10.28\nodejs
if "%python%"=="" set python=C:\tools\python2\python.exe
set path=%path%;%nodejspath%
cd %2\edge
%nodejspath%\node.exe %nodejspath%\node_modules\npm\bin\npm-cli.js install node-gyp