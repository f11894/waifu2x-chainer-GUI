@echo off
set UA="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 Safari/537.36"

cd tools
nvcc -V >nul 2>&1 || goto Check_if_python_is_installed
nvcc -V|find "Cuda compilation tools, release 8.0" >nul&&set cuda_ver=80
nvcc -V|find "Cuda compilation tools, release 9.0" >nul&&set cuda_ver=90
nvcc -V|find "Cuda compilation tools, release 9.1" >nul&&set cuda_ver=91
nvcc -V|find "Cuda compilation tools, release 9.2" >nul&&set cuda_ver=92

:Check_if_python_is_installed
python -h >nul 2>&1||goto install_python

:not_install_python
python -m pip install -U pip
pip install chainer
if defined cuda_ver pip install cupy-cuda%cuda_ver%
pip install wand
pip install pillow
goto install_waifu2x-chainer

:install_python
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

echo start /wait "" "%TEMP%\Anaconda_Windows-setup.exe" /InstallationType=JustMe /RegisterPython=1 /AddToPath=1 /S "/D=%UserProfile%\Anaconda3">"%TEMP%\Anaconda_Windows-setup.bat"
echo exit /b>>"%TEMP%\Anaconda_Windows-setup.bat"
powershell Start-Process "%TEMP%\Anaconda_Windows-setup.bat" -Wait -Verb runas
del "%TEMP%\Anaconda_Windows-setup.bat"
del "%TEMP%\Anaconda_Windows-setup.exe"

cd /d "%UserProfile%\Anaconda3\"
call "%UserProfile%\Anaconda3\Scripts\activate.bat" "%UserProfile%\Anaconda3"
call conda update conda -y
call conda update --all -y
call pip install chainer
if defined cuda_ver call pip install cupy-cuda%cuda_ver%
call pip install wand
call pip install pillow
call "%UserProfile%\Anaconda3\Scripts\deactivate.bat"

goto install_waifu2x-chainer

:install_waifu2x-chainer
echo.
echo Download waifu2x-chainer
echo.
curl -H %UA% --fail --retry 5 -o "%TEMP%\waifu2x-chainer.zip" -L "https://github.com/tsurumeso/waifu2x-chainer/archive/master.zip"
if not "%ERRORLEVEL%"=="0" call :error_end 3
7za.exe x -y -o"%TEMP%\" "%TEMP%\waifu2x-chainer.zip"
del "%TEMP%\waifu2x-chainer.zip"
xcopy /E /I /H /y "%TEMP%\waifu2x-chainer-master" "C:\waifu2x-chainer"
rd /s /q "%TEMP%\waifu2x-chainer-master"
:end
cls
echo successful Installation.
pause
exit

:error_end
if "%~1"=="1" echo URL acquisition failed.
if "%~1"=="2" echo failed to download anaconda.
if "%~1"=="3" echo failed to download waifu2x-chainer.
pause
exit
