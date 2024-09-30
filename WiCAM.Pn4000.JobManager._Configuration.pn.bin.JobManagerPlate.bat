@ECHO OFF

REM ==================================================================
REM PARAMETERS
REM      action_no : SAVE = 1, DELETE = 2
REM ==================================================================
set action_no=%1
set program_no=%2
set machine_no=%3
set dat_file=%4
set job_no=%5


IF "action"=="1" GOTO SAVE
IF "action"=="2" GOTO DELETE


REM ==================================================================
REM SAVE
REM ==================================================================
:SAVE

ECHO SAVE - JOB=%job_no% PROGRAM_NO=%program_no% MACHINE=%machine_no% DAT=%dat_file%

GOTO EXIT



REM ==================================================================
REM DELETE
REM ==================================================================
:DELETE

ECHO DELETE - JOB=%job_no% PROGRAM_NO=%program_no% MACHINE=%machine_no% DAT=%dat_file%


:EXIT

