@echo off
rem Simple benchmark batch file 
cd ..
start bin\debug\htmlparserDotNet20.exe 10000 -f=majestic12.html
cd tests