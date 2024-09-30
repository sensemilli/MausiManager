@echo off

set Action_Type=%1
set Tree=%2
Set Number=%3
Set Number_xxx=%4
rem ======================================
rem  Action_Type = 1 Job
rem  Action_Type = 2 Plate
rem  Action_Type = 3 Part
rem ======================================

rem echo Action_Type=%Action_Type% Tree=%Tree% Number=%Number% Number_xxx=%Number_xxx% >> jobManagerBatch.log

if "%Action_Type%"=="1" goto Job
if "%Action_Type%"=="2" goto Plate

:Job
rem start %tree%\Job.dat
rem echo start %tree%\Job.htm >> jobManagerBatch.log
start %tree%\Job.htm
rem start %tree%\Job.pdf
goto Ende

:Plate
rem %tree%\PLAT_%Number_xxx%.dat
rem echo start %tree%\PLATE_%Number%.htm >> jobManagerBatch.log
start %tree%\PLATE_%Number%.htm
rem  start %tree%\PLAT_%Number%.pdf
goto Ende

:Part
rem %tree%\Part_%Number_xxx%.dat
rem echo start %tree%\Part_%Number%.htm >> jobManagerBatch.log
start %tree%\Part_%Number%.htm
rem  start %tree%\Part_%Number%.pdf
goto Ende

:ende
exit