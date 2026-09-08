# Parity: the game's own scripts against a mod that replaced them.
#
# Runs the same drive twice on the same map - once with --nomods (the game as shipped), once
# with the mods folder applied (a conversion, a scene file) - with --trace on both, and diffs
# the traces: flags set, messages shown (by id, with the item/gold codes), sounds, maps,
# battles, and at every "say" of the drive a snapshot of everyone on the map (cast, model,
# position, motion) and the party's gil and bag. Frame numbers are not compared.
#
#   Tools\parity.ps1 -Map t01_01 -Drive Docs\Drives\ur-chest.drive -Pos -110,0,-112 -Rot 180
#   Tools\parity.ps1 -Map t01_01 -Drive x.drive -Keep         # leaves the traces in %TEMP%\parity
#   Tools\parity.ps1 ... -Ignore 'model o001 at 8,'         # a line pattern not to count (a mod's own object)
#
# Exit code 0 when the traces agree, 1 when they differ (the differences are printed, the
# game's own on the left, the mod's on the right), 2 when a run failed.

param(
	[Parameter(Mandatory = $true)] [string] $Map,
	[Parameter(Mandatory = $true)] [string] $Drive,
	[string] $Game = 'ff3',
	[string] $Pos,
	[string] $Rot,
	[string] $Exe,
	[string] $Out,
	[switch] $Keep,
	[string] $Ignore,
	[int] $Show = 40
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
if (-not $Exe) { $Exe = Join-Path $root 'OpenFF\bin\Debug\net8.0\OpenFF.exe' }
if (-not (Test-Path $Exe)) { Write-Error "no client at $Exe - dotnet build OpenFF.sln first"; exit 2 }
$Drive = (Resolve-Path $Drive).Path
if (-not $Out) { $Out = Join-Path $env:TEMP 'parity' }
New-Item -ItemType Directory -Force $Out | Out-Null

function Run([string] $name, [string[]] $extra) {
	$trace = Join-Path $Out "$Map.$name.trace"
	$log = Join-Path $Out "$Map.$name.log"
	foreach ($f in $trace, $log) { if (Test-Path $f) { Remove-Item $f -Force } }
	$args = @("--game=$Game", "--map=$Map", "--drive=$Drive", "--trace=$trace", "--log=general,file", "--log-file=$log", '--novoice') + $extra
	if ($Pos) { $args += "--pos=$Pos" }
	if ($Rot) { $args += "--rot=$Rot" }
	Write-Host "parity: $name  OpenFF.exe $($args -join ' ')"
	& $Exe @args | Out-Null
	if (-not (Test-Path $trace)) { Write-Error "parity: the $name run wrote no trace ($log)"; exit 2 }
	$bad = Select-String -Path $log -Pattern '^\s*[\d.]+\s+Exception\s' | Select-Object -First 1
	if ($bad) { Write-Host "parity: $name run logged an exception: $($bad.Line)" -ForegroundColor Yellow }
	return $trace
}

$native = Run 'native' @('--nomods')
$modded = Run 'modded' @()

# The frame column off; blank lines out.
function Lines([string] $path) {
	Get-Content $path | ForEach-Object { if ($_ -match '^\d+\s+(.*)$') { $Matches[1].TrimEnd() } } | Where-Object { $_ -and (-not $Ignore -or $_ -notmatch $Ignore) }
}
$a = @(Lines $native)
$b = @(Lines $modded)

$diff = Compare-Object -ReferenceObject $a -DifferenceObject $b -SyncWindow 50
if (-not $diff) {
	Write-Host "parity: $Map agrees - $($a.Count) trace line(s), $(@($a | Where-Object { $_ -like 'mark*' }).Count) mark(s)" -ForegroundColor Green
	if (-not $Keep) { Remove-Item $native, $modded -Force }
	exit 0
}

$only = @($diff | Where-Object { $_.SideIndicator -eq '<=' })
$onlyMod = @($diff | Where-Object { $_.SideIndicator -eq '=>' })
Write-Host "parity: $Map differs - $($only.Count) line(s) only in the game's run, $($onlyMod.Count) only in the mod's ($native, $modded)" -ForegroundColor Red
$shown = 0
foreach ($d in $diff) {
	if ($shown++ -ge $Show) { Write-Host "  ... $($diff.Count - $Show) more"; break }
	$side = if ($d.SideIndicator -eq '<=') { 'game ' } else { 'mod  ' }
	Write-Host "  $side | $($d.InputObject)"
}
exit 1
