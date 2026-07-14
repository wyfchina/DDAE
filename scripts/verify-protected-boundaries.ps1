[CmdletBinding()]
param(
    [string]$Baseline = "4e39ec5"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-UniqueMarkerIndex([string]$Text, [string]$Marker)
{
    if ([string]::IsNullOrEmpty($Marker))
    {
        throw "Marker must not be empty."
    }

    $first = $Text.IndexOf($Marker, [StringComparison]::Ordinal)
    if ($first -lt 0)
    {
        throw "Marker was not found: $Marker"
    }

    $second = $Text.IndexOf($Marker, $first + $Marker.Length, [StringComparison]::Ordinal)
    if ($second -ge 0)
    {
        throw "Marker is not unique: $Marker"
    }

    return $first
}

function Get-BracedBlock([string]$Text, [string]$Signature)
{
    $signatureIndex = Get-UniqueMarkerIndex $Text $Signature
    $openingBrace = $Text.IndexOf('{', $signatureIndex + $Signature.Length)
    if ($openingBrace -lt 0)
    {
        throw "Opening brace was not found after signature: $Signature"
    }

    $depth = 0
    for ($index = $openingBrace; $index -lt $Text.Length; $index++)
    {
        if ($Text[$index] -eq '{')
        {
            $depth++
        }
        elseif ($Text[$index] -eq '}')
        {
            $depth--
            if ($depth -eq 0)
            {
                return $Text.Substring($signatureIndex, $index - $signatureIndex + 1)
            }

            if ($depth -lt 0)
            {
                break
            }
        }
    }

    throw "Complete braced block was not found for signature: $Signature"
}

function Get-DelimitedBlock([string]$Text, [string]$StartMarker, [string]$EndMarker)
{
    $startIndex = Get-UniqueMarkerIndex $Text $StartMarker
    $endIndex = Get-UniqueMarkerIndex $Text $EndMarker
    $contentStart = $startIndex + $StartMarker.Length
    if ($endIndex -lt $contentStart)
    {
        throw "End marker occurs before start marker."
    }

    return $Text.Substring($contentStart, $endIndex - $contentStart)
}

function Get-BaselineFileText([string]$Path, [string]$BaselineRevision)
{
    $lines = @(& git show "$BaselineRevision`:$Path")
    if ($LASTEXITCODE -ne 0)
    {
        throw "Unable to read baseline file '$Path' from '$BaselineRevision'."
    }

    return [string]::Join("`n", $lines)
}

function Get-CurrentFileText([string]$Path)
{
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf))
    {
        throw "Current file is missing: $Path"
    }

    return Get-Content -Raw -Encoding utf8 -LiteralPath $Path
}

function Normalize-LineEndings([string]$Text)
{
    return $Text.Replace("`r`n", "`n").Replace("`r", "`n")
}

function Assert-ProtectedBlock([string]$Label, [string]$BaselineBlock, [string]$CurrentBlock)
{
    if ((Normalize-LineEndings $BaselineBlock) -cne (Normalize-LineEndings $CurrentBlock))
    {
        throw "Protected block changed: $Label"
    }

    Write-Host "PASS $Label"
}

try
{
    $repositoryRoot = Split-Path -Parent $PSScriptRoot
    Push-Location $repositoryRoot
    try
    {
        & git rev-parse --verify "$Baseline^{commit}" *> $null
        if ($LASTEXITCODE -ne 0)
        {
            throw "Baseline commit was not found: $Baseline"
        }

        $wholeFilePaths = @(
            'src/AdaptiveSopDdsop.Web/Domain/DdsopConfigInboundContract.cs',
            'src/AdaptiveSopDdsop.Web/Domain/DdsopRuntimePlanningInputContract.cs',
            'src/AdaptiveSopDdsop.Web/Domain/ProductionInventoryQualityEvidenceContract.cs',
            'src/AdaptiveSopDdsop.Web/Domain/ProductionSupplierIdentitySourceContract.cs',
            'src/AdaptiveSopDdsop.Web/Domain/SdbrExecutionObjectEvidenceContract.cs',
            'src/AdaptiveSopDdsop.Web/Domain/PublicDemoGoldenLoopService.cs',
            'src/AdaptiveSopDdsop.Web/Domain/AdventureWorksProductDemoProfileService.cs',
            'src/AdaptiveSopDdsop.Web/Domain/ContractRepositoryPathResolver.cs',
            'src/AdaptiveSopDdsop.Web/appsettings.json',
            'tests/AdaptiveSopDdsop.Tests/Fixtures/sdbr-actual-planning-run-feedback.json',
            'tests/AdaptiveSopDdsop.Tests/Fixtures/sdbr-actual-variance-analysis-feedback.json'
        )
        & git diff --exit-code --no-ext-diff $Baseline -- $wholeFilePaths
        if ($LASTEXITCODE -ne 0)
        {
            throw "One or more whole-file protected boundaries differ from baseline '$Baseline'."
        }
        Write-Host "PASS 11 whole-file protected boundaries"

        $testPath = 'tests/AdaptiveSopDdsop.Tests/Program.cs'
        $baselineTests = Get-BaselineFileText $testPath $Baseline
        $currentTests = Get-CurrentFileText $testPath
        $protectedTestSignatures = @(
            'static void TestDdsopConfigInboundPayloadAndAckInterpreter()',
            'static void TestDdsopFeedbackInboundLedgerAcceptsSdbrFixtures()',
            'static void TestDdsopRuntimePlanningInputGeneratesDdaeOwnedPackage()',
            'static void TestAdventureWorksSchedulingAdapterMetadataStaysNonDdaeOwned()',
            'static void TestAdventureWorksProductDemoProfileExposesDdaeGovernanceReadModel()',
            'static void TestContractRepositoryPathResolverPrefersConfiguredRoot()',
            'static void TestContractRepositoryPathResolverDiscoversSiblingRepository()',
            'static void TestDdsopRuntimePlanningInputCorrelatesFeedback()',
            'static void TestPublicDemoGoldenLoopServiceWritesHandoffPayload()',
            'static void TestIntegrationContractEndpointsAndRemovedOptimizationPath()'
        )
        foreach ($signature in $protectedTestSignatures)
        {
            Assert-ProtectedBlock $signature `
                (Get-BracedBlock $baselineTests $signature) `
                (Get-BracedBlock $currentTests $signature)
        }

        $programPath = 'src/AdaptiveSopDdsop.Web/Program.cs'
        $baselineProgram = Get-BaselineFileText $programPath $Baseline
        $currentProgram = Get-CurrentFileText $programPath
        $endpointStart = 'app.MapGet("/api/integration-contracts/ddsop-config-inbound-v1"'
        $endpointEnd = 'app.MapGet("/api/history-review"'
        Assert-ProtectedBlock 'integration-contract endpoint block' `
            (Get-DelimitedBlock $baselineProgram $endpointStart $endpointEnd) `
            (Get-DelimitedBlock $currentProgram $endpointStart $endpointEnd)

        $indexPath = 'src/AdaptiveSopDdsop.Web/Pages/Index.cshtml'
        $baselineIndex = Get-BaselineFileText $indexPath $Baseline
        $currentIndex = Get-CurrentFileText $indexPath
        $traceStart = '<section id="trace-panel" class="schedule-panel" data-tab-panel hidden>'
        $publicDemoStart = '<section id="public-demo-golden-loop-panel" class="workspace-section" hidden>'
        $mainEnd = '        </main>'
        Assert-ProtectedBlock 'trace-panel inner block' `
            (Get-DelimitedBlock $baselineIndex $traceStart $publicDemoStart) `
            (Get-DelimitedBlock $currentIndex $traceStart $publicDemoStart)
        Assert-ProtectedBlock 'public-demo-golden-loop-panel block' `
            (Get-DelimitedBlock $baselineIndex $publicDemoStart $mainEnd) `
            (Get-DelimitedBlock $currentIndex $publicDemoStart $mainEnd)

        $networkStart = '<section class="decision-category-card"><span class="category-code">A</span>'
        $networkEnd = '<section class="decision-category-card"><span class="category-code">B</span>'
        Assert-ProtectedBlock 'existing Network link element' `
            (Get-DelimitedBlock $baselineIndex $networkStart $networkEnd) `
            (Get-DelimitedBlock $currentIndex $networkStart $networkEnd)

        $appJsPath = 'src/AdaptiveSopDdsop.Web/wwwroot/js/app.js'
        $baselineAppJs = Get-BaselineFileText $appJsPath $Baseline
        $currentAppJs = Get-CurrentFileText $appJsPath
        $protectedJavaScriptSignatures = @(
            'function renderPreviewTrace(result)',
            'function renderTrace(data)',
            'function renderPublicDemoGoldenLoop(workspace)',
            'function renderAdventureWorksProductDemo(workspace)',
            'function renderPublicDemoSchedulingAdapter(adapter)',
            'function renderPublicDemoPayload(payload, handoff)',
            'function renderPublicDemoFeedback(feedback, nonClaimsSummary)',
            'function renderPublicDemoBusinessUserView(workspace)',
            'async function loadPublicDemoGoldenLoop()',
            'async function writePublicDemoPayload()'
        )
        foreach ($signature in $protectedJavaScriptSignatures)
        {
            Assert-ProtectedBlock $signature `
                (Get-BracedBlock $baselineAppJs $signature) `
                (Get-BracedBlock $currentAppJs $signature)
        }

        Write-Host "Protected boundaries match baseline $Baseline."
    }
    finally
    {
        Pop-Location
    }
}
catch
{
    Write-Error $_
    exit 1
}

exit 0
