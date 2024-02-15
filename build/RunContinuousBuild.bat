@ECHO OFF

SET "release="
FOR /F "skip=1 delims=" %%i IN (version.txt) DO IF NOT DEFINED release SET "release=%%i"

RunBuild.bat %release% alpha%1
