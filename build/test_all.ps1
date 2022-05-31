$ErrorActionPreference = "Stop"

$mode = if ($args[0] -eq 'Release') { "Release" } else { "Debug" }

Write-Output "mode = $mode"

$root = Split-Path -Parent $MyInvocation.MyCommand.Definition
$slnPath = [io.path]::combine($root, "..")
$path = [io.path]::combine($slnPath, "src/test/MixedIL.Tests")

& dotnet test $path --nologo -v q -c $mode
