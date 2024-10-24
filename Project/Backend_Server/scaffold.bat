@echo off
setlocal enabledelayedexpansion

:: Setup logging
set "LOG_FILE=scaffold_%date:~10,4%%date:~4,2%%date:~7,2%_%time:~0,2%%time:~3,2%%time:~6,2%.log"
set "LOG_FILE=%LOG_FILE: =0%"

echo [%date% %time%] Starting database scaffolding utility... >> %LOG_FILE%
echo Starting database scaffolding utility...

:: Check AWS CLI
where aws >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [%date% %time%] Error: AWS CLI not found >> %LOG_FILE%
    echo Error: AWS CLI not found
    goto :cleanup
)

:: Get AWS Secrets
echo [%date% %time%] Getting AWS secrets... >> %LOG_FILE%
echo Getting AWS secrets...

aws secretsmanager get-secret-value --secret-id "team16/rds-instance/db-credentials" --region "us-east-2" --query "SecretString" --output text > temp_db_secrets.json
if %ERRORLEVEL% NEQ 0 (
    echo [%date% %time%] Error: Failed to get database credentials >> %LOG_FILE%
    echo Error: Failed to get database credentials
    goto :cleanup
)

aws secretsmanager get-secret-value --secret-id "team16/ec2-instance/ssh-credentials" --region "us-east-2" --query "SecretString" --output text > temp_ssh_secrets.json
if %ERRORLEVEL% NEQ 0 (
    echo [%date% %time%] Error: Failed to get SSH credentials >> %LOG_FILE%
    echo Error: Failed to get SSH credentials
    goto :cleanup
)

:: Setup SSH tunnel
echo [%date% %time%] Setting up SSH tunnel... >> %LOG_FILE%
echo Setting up SSH tunnel...

:: Extract host from secrets and start tunnel
type temp_ssh_secrets.json > temp_creds.txt
set /p SSH_HOST=<temp_creds.txt
start /b ssh -N -L 3306:localhost:3306 -i %SSH_HOST% ec2-user@your-ec2-instance

:: Wait for tunnel
timeout /t 5 /nobreak > nul

:: Run scaffolding
echo [%date% %time%] Running database scaffolding... >> %LOG_FILE%
echo Running database scaffolding...

dotnet ef dbcontext scaffold "Server=127.0.0.1;Port=3306;Database=Team16_GIDP_DB;User=%DB_USER%;Password=%DB_PASS%" Pomelo.EntityFrameworkCore.MySql -o Models -f --no-onconfiguring

if %ERRORLEVEL% NEQ 0 (
    echo [%date% %time%] Error: Scaffolding failed >> %LOG_FILE%
    echo Error: Scaffolding failed
) else (
    echo [%date% %time%] Scaffolding completed successfully >> %LOG_FILE%
    echo Scaffolding completed successfully
)

:cleanup
echo [%date% %time%] Performing cleanup... >> %LOG_FILE%
echo Performing cleanup...

:: Kill SSH tunnel if exists
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":3306"') do taskkill /F /PID %%a >nul 2>&1

:: Delete temp files
del /f /q temp_*.* >nul 2>&1

echo [%date% %time%] Cleanup completed >> %LOG_FILE%
echo Cleanup completed

endlocal
exit /b 0