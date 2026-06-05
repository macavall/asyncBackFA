<#
.SYNOPSIS
    Tests the asyncBack Azure Function app by starting a job and polling its status.

.DESCRIPTION
    1. Sends a POST request to /api/startjob to queue a background job.
    2. Reads the returned jobId and statusUrl.
    3. Polls GET /api/jobstatus/{jobId} until the job is Completed or Failed
       (or until the timeout is reached).

.PARAMETER BaseUrl
    The base URL of the running Function app. Defaults to the local host.

.PARAMETER PollIntervalSeconds
    How long to wait between status checks. Defaults to 2 seconds.

.PARAMETER TimeoutSeconds
    Maximum time to wait for the job to finish. Defaults to 60 seconds.

.EXAMPLE
    ./Test-FunctionApp.ps1

.EXAMPLE
    ./Test-FunctionApp.ps1 -BaseUrl "http://localhost:7071" -TimeoutSeconds 90
#>

[CmdletBinding()]
param(
    [string]$BaseUrl = "http://localhost:7071",
    [int]$PollIntervalSeconds = 2,
    [int]$TimeoutSeconds = 60
)

$ErrorActionPreference = "Stop"

# --- 1. Start the job (POST) ---
$startUrl = "$BaseUrl/api/startjob"
Write-Host "POST $startUrl" -ForegroundColor Cyan

try {
    $startResponse = Invoke-RestMethod -Method Post -Uri $startUrl
}
catch {
    Write-Error "Failed to start job: $($_.Exception.Message)"
    exit 1
}

$jobId = $startResponse.jobId
Write-Host "Job accepted. jobId = $jobId" -ForegroundColor Green
$startResponse | ConvertTo-Json

# --- 2. Poll for status (GET) ---
$statusUrl = "$BaseUrl/api/jobstatus/$jobId"
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)

do {
    Start-Sleep -Seconds $PollIntervalSeconds

    try {
        $status = Invoke-RestMethod -Uri $statusUrl
    }
    catch {
        Write-Error "Failed to query status: $($_.Exception.Message)"
        exit 1
    }

    Write-Host "Status: $($status.status)" -ForegroundColor Yellow

    if ($status.status -in @("Completed", "Failed")) {
        break
    }
}
while ((Get-Date) -lt $deadline)

# --- 3. Report final result ---
Write-Host "`nFinal job state:" -ForegroundColor Cyan
$status | ConvertTo-Json

if ($status.status -ne "Completed") {
    Write-Warning "Job did not complete within $TimeoutSeconds seconds (last status: $($status.status))."
    exit 1
}
