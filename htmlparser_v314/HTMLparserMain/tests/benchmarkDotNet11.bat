@echo off
rem Simple benchmark batch file 
cd ..
start bin\debug\htmlparserDotNet11.exe 10000 -f=majestic12.html
cd tests