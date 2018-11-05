@echo off
set UA="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36"
set "date2=%date:/=%"
set "time2=%time:^:=_%"
set "time2=%time: =0%"
set date_tmp=%date:/=%
set time_tmp=%time: =0%
set yyyy=%date_tmp:~0,4%
set yy=%date_tmp:~2,2%
set mm=%date_tmp:~4,2%
set dd=%date_tmp:~6,2%
set hh=%time_tmp:~0,2%
set mi=%time_tmp:~3,2%
set ss=%time_tmp:~6,2%
set sss=%time_tmp:~9,2%
set datetime=%yyyy%%mm%%dd%%hh%%mi%%ss%%sss%
set install_log="%~dp0install_log_%datetime%.txt"

ver>>%install_log%
echo.>>%install_log%
set /p "x=PowerShell "<NUL >>%install_log%
PowerShell Get-Host | find "Version">>%install_log%||call :error_end 0

cd tools
nvcc -V >nul 2>&1 || goto Check_if_python_is_installed
nvcc -V|find "Cuda compilation tools, release 8.0" >nul&&set cuda_ver=80
nvcc -V|find "Cuda compilation tools, release 9.0" >nul&&set cuda_ver=90
nvcc -V|find "Cuda compilation tools, release 9.1" >nul&&set cuda_ver=91
nvcc -V|find "Cuda compilation tools, release 9.2" >nul&&set cuda_ver=92

:Check_if_python_is_installed
python -h >nul 2>&1||goto install_python

:not_install_python
echo install_python=no>>%install_log%
python -m pip install -U pip
pip install chainer
if defined cuda_ver pip install cupy-cuda%cuda_ver%
pip install wand
pip install pillow
goto install_waifu2x-chainer

:install_python
echo install_python=yes>>%install_log%
if "%PROCESSOR_ARCHITECTURE%" EQU "x86" (
   curl -H %UA% -s "https://www.anaconda.com/download/#windows" -o "%TEMP%\anaconda_download.txt" >nul 2>&1
   mfind /W /M "/.*\x22(https:\/\/repo\.anaconda\.com\/archive\/Anaconda3[^\/]*?Windows-x86.exe)\x22.*/$1/" "%TEMP%\anaconda_download.txt" >nul 2>&1 ||call :error_end 1
   set /p conda_URL=<"%TEMP%\anaconda_download.txt"
)
if "%PROCESSOR_ARCHITECTURE%" EQU "AMD64" (
   curl -H %UA% -s "https://www.anaconda.com/download/#windows" -o "%TEMP%\anaconda_download.txt" >nul 2>&1
   mfind /W /M "/.*\x22(https:\/\/repo\.anaconda\.com\/archive\/Anaconda3[^\/]*?Windows-x86_64.exe)\x22.*/$1/" "%TEMP%\anaconda_download.txt" >nul 2>&1 ||call :error_end 1
   set /p conda_URL=<"%TEMP%\anaconda_download.txt"
)
del "%TEMP%\anaconda_download.txt"
echo "%conda_URL%"|findstr /X ".https://repo\.anaconda\.com/archive/Anaconda3[^/]*Windows-[^/]*.exe." >nul ||call :error_end 1
echo Download Anaconda

echo.
curl -H %UA% --retry 10 --fail -o "%TEMP%\Anaconda_Windows-setup.exe" "%conda_URL%"
if not "%ERRORLEVEL%"=="0" call :error_end 2
echo.
echo Install Anaconda
echo.

echo start /wait "" "%TEMP%\Anaconda_Windows-setup.exe" /InstallationType=JustMe /RegisterPython=1 /AddToPath=1 /S /D=%UserProfile%\AppData\Local\Continuum\anaconda3>"%TEMP%\Anaconda_Windows-setup.bat"
echo exit /b>>"%TEMP%\Anaconda_Windows-setup.bat"
powershell Start-Process "%TEMP%\Anaconda_Windows-setup.bat" -Wait -Verb runas
del "%TEMP%\Anaconda_Windows-setup.bat"
del "%TEMP%\Anaconda_Windows-setup.exe"

if exist "%UserProfile%\Anaconda3" set "Anaconda_dir=%UserProfile%\Anaconda3"
if exist "%UserProfile%\AppData\Local\Continuum\anaconda3" set "Anaconda_dir=%UserProfile%\AppData\Local\Continuum\anaconda3"
if not defined Anaconda_dir call :error_end 4

pushd "%Anaconda_dir%"
call "%Anaconda_dir%\Scripts\activate.bat" "%Anaconda_dir%"
call conda update conda -y
call conda update --all -y
call pip install chainer
if defined cuda_ver call pip install cupy-cuda%cuda_ver%
call pip install wand
call pip install pillow
call "%Anaconda_dir%\Scripts\deactivate.bat"
popd

goto install_waifu2x-chainer

:install_waifu2x-chainer
echo.
echo Download waifu2x-chainer
echo.
curl -H %UA% --fail --retry 5 -o "%TEMP%\waifu2x-chainer.zip" -L "https://github.com/tsurumeso/waifu2x-chainer/archive/master.zip"
if not "%ERRORLEVEL%"=="0" call :error_end 3
7za.exe x -y -o"%TEMP%\" "%TEMP%\waifu2x-chainer.zip"
del "%TEMP%\waifu2x-chainer.zip"
xcopy /E /I /H /y "%TEMP%\waifu2x-chainer-master" "C:\waifu2x-chainer"||call :error_end 5
if not exist "C:\waifu2x-chainer\waifu2x.py" call :error_end 5
pushd "C:\waifu2x-chainer"
if defined Anaconda_dir call "%Anaconda_dir%\Scripts\activate.bat" "%Anaconda_dir%"
call Python waifu2x.py -m scale -i images\small.png -g 0 -o "%TEMP%\hoge.png" >>%install_log% 2>&1
call Python waifu2x.py -m scale -i images\small.png -o "%TEMP%\hoge.png" >>%install_log% 2>&1||call :error_end 5
if defined Anaconda_dir call "%Anaconda_dir%\Scripts\deactivate.bat"
popd

rd /s /q "%TEMP%\waifu2x-chainer-master"
:end
echo.
echo successful Installation.
pause
exit

:error_end
if "%~1"=="0" echo Error PowerShell is not installed.&echo Error PowerShell is not installed.>>%install_log%
if "%~1"=="1" echo Error URL acquisition failed.&echo Error URL acquisition failed.>>%install_log%
if "%~1"=="2" echo Error failed to download anaconda.&echo Error failed to download anaconda.>>%install_log%
if "%~1"=="3" echo Error failed to download waifu2x-chainer.&echo Error failed to download waifu2x-chainer.>>%install_log%
if "%~1"=="4" echo Error Anaconda installation location could not be found.&echo Error Anaconda installation location could not be found.>>%install_log%
if "%~1"=="5" echo Error Installation of waifu2x-chainer failed.&echo Error Installation of waifu2x-chainer failed.>>%install_log%

pause
exit
