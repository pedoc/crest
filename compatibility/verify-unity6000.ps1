param(
 [string]$UnityEditor='C:\Program Files\Unity 6000.6.1f1\Editor\Unity.exe',
 [switch]$BuildOnly
)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$project=Join-Path $root 'crest';$logs=Join-Path $project 'Logs'
New-Item -ItemType Directory -Path $logs -Force | Out-Null
$log=Join-Path $logs 'unity6000-build.log'
$p=Start-Process -FilePath $UnityEditor -ArgumentList @('-batchmode','-nographics','-projectPath',('"'+$project+'"'),'-executeMethod','Crest.Compatibility.Editor.Unity6000SmokeBuild.BuildWindows','-quit','-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
$p.WaitForExit();if($p.ExitCode -ne 0){throw "Editor/build failed: $log"}
if($BuildOnly){return}
$exe=Join-Path $root 'builds\unity6000-smoke\Crest.Unity6000Smoke.exe'
$output=Join-Path $root 'compatibility\evidence';New-Item -ItemType Directory -Path $output -Force | Out-Null
$playerLog=Join-Path $logs 'unity6000-player.log'
# This opens an interactive render test window; it exits after producing evidence.
$p=Start-Process -FilePath $exe -ArgumentList @('-screen-fullscreen','0','-screen-width','1280','-screen-height','800','-crestCompatibilityAutoTest','-crestCompatibilityOutput',('"'+$output+'"'),'-logFile',('"'+$playerLog+'"')) -WorkingDirectory (Split-Path $exe -Parent) -WindowStyle Normal -PassThru
$p.WaitForExit();if($p.ExitCode -ne 0){throw "Runtime smoke failed: $playerLog"}
$result=Get-Content -LiteralPath (Join-Path $output 'runtime-verification.json') -Raw | ConvertFrom-Json
if(-not $result.passed){throw 'Runtime evidence did not pass.'}
$result | ConvertTo-Json -Depth 5
