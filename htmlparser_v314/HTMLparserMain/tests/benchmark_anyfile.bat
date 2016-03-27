@echo off
rem Simple benchmark batch file - must be specified as a parameter
cd ..
start bin\debug\htmlparserDotNet20.exe 10000 -f=%1
cd tests