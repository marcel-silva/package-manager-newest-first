param(
    [Parameter(Mandatory = $true)][string]$UnityEditorData,
    [Parameter(Mandatory = $true)][string]$NUnitFramework
)
$ErrorActionPreference = 'Stop'
$packageRoot = Split-Path $PSScriptRoot -Parent
$outDir = Join-Path $packageRoot 'TestResults~'
New-Item -ItemType Directory -Force $outDir | Out-Null
$framework = Join-Path $UnityEditorData 'UnityReferenceAssemblies/unity-4.8-api'
$compiler = Join-Path $UnityEditorData 'DotNetSdkRoslyn/csc.dll'
$runtime = Join-Path $UnityEditorData 'netcorerun/netcorerun.exe'
$exe = Join-Path $outDir 'NewestFirstTests.exe'
$compilerArgs = @(
    $compiler, '/nologo', '/noconfig', '/nostdlib+', '/target:exe', "/out:$exe",
    "/r:$(Join-Path $framework 'mscorlib.dll')",
    "/r:$(Join-Path $framework 'System.dll')",
    "/r:$(Join-Path $framework 'System.Core.dll')",
    "/r:$NUnitFramework",
    (Join-Path $packageRoot 'Editor/SortPolicy.cs'),
    (Join-Path $packageRoot 'Editor/FactoryProxy.cs'),
    (Join-Path $packageRoot 'Tests/Editor/SortPolicyTests.cs'),
    (Join-Path $packageRoot 'Tests/Editor/FactoryProxyTests.cs'),
    (Join-Path $PSScriptRoot 'StandaloneRunner.cs')
)
& $runtime @compilerArgs
if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed.' }
Copy-Item -LiteralPath $NUnitFramework -Destination (Join-Path $outDir 'nunit.framework.dll') -Force
& $exe | Tee-Object -FilePath (Join-Path $outDir 'standalone-results.txt')
if ($LASTEXITCODE -ne 0) { throw 'Standalone tests failed.' }
