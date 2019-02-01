@echo off
set UA="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.81 Safari/537.36"
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
set Anaconda_setup_url="%TEMP%\Anaconda_setup_%RANDOM%_%RANDOM%_%RANDOM%.txt"
set Anaconda_setup_bat="%TEMP%\Anaconda_setup_%RANDOM%_%RANDOM%_%RANDOM%.bat"
set Anaconda_setup_exe="%TEMP%\Anaconda_setup_%RANDOM%_%RANDOM%_%RANDOM%.exe"

ver>>%install_log%
echo.>>%install_log%
set /p "x=PowerShell "<NUL >>%install_log%
PowerShell Get-Host | find "Version">>%install_log%||call :error_end 0

cd tools
nvcc -V >nul 2>&1 || goto Check_if_python_is_installed
nvcc -V|find "Cuda compilation tools, release 8.0" >>%install_log% &&set cuda_ver=80
nvcc -V|find "Cuda compilation tools, release 9.0" >>%install_log% &&set cuda_ver=90
nvcc -V|find "Cuda compilation tools, release 9.1" >>%install_log% &&set cuda_ver=91
nvcc -V|find "Cuda compilation tools, release 9.2" >>%install_log% &&set cuda_ver=92

:Check_if_python_is_installed
python -h >nul 2>&1||goto install_python

:not_install_python
echo install_python=no>>%install_log%
pip install chainer >>%install_log% 2>&1
if defined cuda_ver pip install cupy-cuda%cuda_ver% >>%install_log% 2>&1
pip install wand >>%install_log% 2>&1
pip install pillow >>%install_log% 2>&1
goto install_waifu2x-chainer

:install_python
echo install_python=yes>>%install_log%
if "%PROCESSOR_ARCHITECTURE%" EQU "x86" (
   curl -H %UA% -s "https://www.anaconda.com/distribution/#windows" -o %Anaconda_setup_url% >nul 2>&1
   mfind /W /M "/.*\x22 *(https:\/\/repo\.anaconda\.com\/archive\/Anaconda3[^\/]*?Windows-x86.exe)\x22.*/$1/" %Anaconda_setup_url% >nul 2>&1 ||call :error_end 1
   set /p conda_URL=<%Anaconda_setup_url%
)
if "%PROCESSOR_ARCHITECTURE%" EQU "AMD64" (
   curl -H %UA% -s "https://www.anaconda.com/distribution/#windows" -o %Anaconda_setup_url% >nul 2>&1
   mfind /W /M "/.*\x22 *(https:\/\/repo\.anaconda\.com\/archive\/Anaconda3[^\/]*?Windows-x86_64.exe)\x22.*/$1/" %Anaconda_setup_url% >nul 2>&1 ||call :error_end 1
   set /p conda_URL=<%Anaconda_setup_url%
)
del %Anaconda_setup_url%
echo "%conda_URL%"|findstr /X ".https://repo\.anaconda\.com/archive/Anaconda3[^/]*Windows-[^/]*.exe." >nul ||call :error_end 1
echo "%conda_URL%" >>%install_log%
echo Anacondaのインストーラーをダウンロードしています

echo.
curl -H %UA% --retry 10 --fail -o %Anaconda_setup_exe% "%conda_URL%"
if not "%ERRORLEVEL%"=="0" call :error_end 2
echo.
echo Anacondaをインストールしています
echo 非常に時間が掛かりますがウィンドウを閉じずにそのままお待ち下さい
echo.

echo %Anaconda_setup_exe% /InstallationType=JustMe /RegisterPython=1 /AddToPath=1 /S /D=%UserProfile%\AppData\Local\Continuum\anaconda3 >>%install_log% 2>&1
echo start /wait "" %Anaconda_setup_exe% /InstallationType=JustMe /RegisterPython=1 /AddToPath=1 /S /D=%UserProfile%\AppData\Local\Continuum\anaconda3>%Anaconda_setup_bat%
echo exit /b>>%Anaconda_setup_bat%
powershell Start-Process %Anaconda_setup_bat% -Wait -Verb runas
del %Anaconda_setup_bat%
del %Anaconda_setup_exe%

if exist "%UserProfile%\Anaconda3" set "Anaconda_dir=%UserProfile%\Anaconda3"
if exist "%UserProfile%\AppData\Local\Continuum\anaconda3" set "Anaconda_dir=%UserProfile%\AppData\Local\Continuum\anaconda3"
if not defined Anaconda_dir call :error_end 4
echo Anaconda_dir %Anaconda_dir% >>%install_log% 2>&1

pushd "%Anaconda_dir%"
call "%Anaconda_dir%\Scripts\activate.bat" "%Anaconda_dir%"
call conda update conda -y >>%install_log% 2>&1
call conda update --all -y >>%install_log% 2>&1
call pip install chainer >>%install_log% 2>&1
if defined cuda_ver call pip install cupy-cuda%cuda_ver% >>%install_log% 2>&1
call pip install wand >>%install_log% 2>&1
call pip install pillow >>%install_log% 2>&1
call "%Anaconda_dir%\Scripts\deactivate.bat"
popd

goto install_waifu2x-chainer

:install_waifu2x-chainer
echo.
echo waifu2x-chainerをインストールしています
echo.
curl -H %UA% --fail --retry 5 -o "%TEMP%\waifu2x-chainer.zip" -L "https://github.com/tsurumeso/waifu2x-chainer/archive/v1.9.0.zip"
if not "%ERRORLEVEL%"=="0" call :error_end 3
7za.exe x -y -o"%TEMP%\" "%TEMP%\waifu2x-chainer.zip" >>%install_log% 2>&1
del "%TEMP%\waifu2x-chainer.zip"
xcopy /E /I /H /y "%TEMP%\waifu2x-chainer-master" "C:\waifu2x-chainer" >>%install_log% 2>&1||call :error_end 5
if not exist "C:\waifu2x-chainer\waifu2x.py" call :error_end 5
pushd "C:\waifu2x-chainer"
if defined Anaconda_dir call "%Anaconda_dir%\Scripts\activate.bat" "%Anaconda_dir%"
call Python waifu2x.py -m scale -i images\small.png -g 0 -o "%TEMP%\hoge.png" >>%install_log% 2>&1&&set installed_cupy=true
call Python waifu2x.py -m scale -i images\small.png -o "%TEMP%\hoge.png" >>%install_log% 2>&1||call :error_end 5
if defined Anaconda_dir call "%Anaconda_dir%\Scripts\deactivate.bat"
popd

rd /s /q "%TEMP%\waifu2x-chainer-master"
:end
echo.
if "%installed_cupy%"=="true" (
   echo waifu2x-chainerとcupyのインストールに成功しました
   echo waifu2x-chainerとcupyのインストールに成功しました>>%install_log%
) else (
   echo waifu2x-chainerのインストールに成功しました
   echo waifu2x-chainerのインストールに成功しました>>%install_log%
)
pause
exit

:error_end
if "%~1"=="0" (
   echo エラー PowerShellがインストールされていません
   echo エラー PowerShellがインストールされていません>>%install_log%
)
if "%~1"=="1" (
   echo エラー URLの取得に失敗しました
   echo エラー URLの取得に失敗しました>>%install_log%
)
if "%~1"=="2" (
   echo エラー Anacondaのダウンロードに失敗しました
   echo エラー Anacondaのダウンロードに失敗しました>>%install_log%
)
if "%~1"=="3" (
   echo エラー waifu2x-chainerのダウンロードに失敗しました
   echo エラー waifu2x-chainerのダウンロードに失敗しました>>%install_log%
)
if "%~1"=="4" (
   echo エラー Anacondaのインストール場所が見つかりませんでした
   echo エラー Anacondaのインストール場所が見つかりませんでした>>%install_log%
)
if "%~1"=="5" (
   echo エラー waifu2x-chainerのインストールに失敗しました
   echo エラー waifu2x-chainerのインストールに失敗しました>>%install_log%
)
pause
exit
