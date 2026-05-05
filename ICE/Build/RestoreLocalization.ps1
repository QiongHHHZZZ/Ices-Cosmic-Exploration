param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectDir
)

$ErrorActionPreference = 'Stop'

$l10nPath = Join-Path $ProjectDir 'Localization\L10n.cs'
if (-not (Test-Path $l10nPath)) {
    return
}

$l10nContent = Get-Content $l10nPath -Raw
$keyRegex = [regex]::new('\["((?:\\.|[^"\\])*)"\]\s*=')
$knownKeys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)

foreach ($match in $keyRegex.Matches($l10nContent)) {
    $rawKey = [System.Text.RegularExpressions.Regex]::Unescape($match.Groups[1].Value)
    [void]$knownKeys.Add($rawKey)
}

$targetRoots = @(
    (Join-Path $ProjectDir 'Ui'),
    (Join-Path $ProjectDir 'Utilities\ImGuiTools')
)

$methodNames = 'TextWrapped|TextDisabled|Text|Button|Checkbox|RadioButton|Selectable|SetTooltip|InputText|InputUInt|InputInt|InputFloat3|DragInt|DragFloat|SliderInt|SliderUInt|BeginPopup|BeginTable|BeginChild|CollapsingHeader|BeginCombo'
$imGuiPattern = '(?<call>\bImGui(?:Ex)?\.(?:' + $methodNames + ')\()\s*"(?<text>(?:\\.|[^"\\])*)"'
$chatPattern = '(?<call>\bSvc\.Chat\.Print\()\s*"(?<text>(?:\\.|[^"\\])*)"'
$imGuiRegex = [regex]::new($imGuiPattern, [System.Text.RegularExpressions.RegexOptions]::Compiled)
$chatRegex = [regex]::new($chatPattern, [System.Text.RegularExpressions.RegexOptions]::Compiled)

function Restore-FileLocalization {
    param([string]$Path)

    $original = Get-Content $Path -Raw
    $updated = $imGuiRegex.Replace($original, {
        param($m)
        $rawKey = [System.Text.RegularExpressions.Regex]::Unescape($m.Groups['text'].Value)
        if ($knownKeys.Contains($rawKey)) {
            return $m.Groups['call'].Value + 'T("' + $m.Groups['text'].Value + '")'
        }
        return $m.Value
    })

    $updated = $chatRegex.Replace($updated, {
        param($m)
        $rawKey = [System.Text.RegularExpressions.Regex]::Unescape($m.Groups['text'].Value)
        if ($knownKeys.Contains($rawKey)) {
            return $m.Groups['call'].Value + 'T("' + $m.Groups['text'].Value + '")'
        }
        return $m.Value
    })

    if ($updated -ne $original) {
        Set-Content -Path $Path -Value $updated -Encoding UTF8
    }
}

foreach ($root in $targetRoots) {
    if (-not (Test-Path $root)) {
        continue
    }

    Get-ChildItem -Path $root -Recurse -Filter '*.cs' | ForEach-Object {
        Restore-FileLocalization -Path $_.FullName
    }
}
