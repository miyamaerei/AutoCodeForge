<#
.SYNOPSIS
    保存代码安全审计报告和证据文件
.DESCRIPTION
    将审计报告保存为历史记录，并保存 L1-L4 证据 JSON 文件
.PARAMETER FileName
    文件名（不含扩展名），如 "OrderService"
.PARAMETER ReportContent
    完整的审计报告 Markdown 内容
.PARAMETER L1Evidence
    L1 证据 JSON 字符串（可选）
.PARAMETER L2Evidence
    L2 证据 JSON 字符串（可选）
.PARAMETER L3Evidence
    L3 证据 JSON 字符串（可选）
.PARAMETER L4Evidence
    L4 证据 JSON 字符串（可选）
.PARAMETER BasePath
    证据基础路径，默认为 .autoCodeForge/security-audit
.EXAMPLE
    .\save-report.ps1 -FileName "OrderService" -ReportContent "## 代码安全审计报告..." -L1Evidence "{...}"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$FileName,
    
    [Parameter(Mandatory=$true)]
    [string]$ReportContent,
    
    [string]$L1Evidence = "",
    [string]$L2Evidence = "",
    [string]$L3Evidence = "",
    [string]$L4Evidence = "",
    
    [string]$BasePath = ".autoCodeForge/security-audit"
)

$evidenceDir = Join-Path $BasePath $FileName
$historyDir = Join-Path $evidenceDir "history"
$timestamp = Get-Date -Format "yyyy-MM-dd-HHmm"
$historyFile = Join-Path $historyDir "$timestamp.md"

try {
    if (-Not (Test-Path $evidenceDir)) {
        New-Item -ItemType Directory -Path $evidenceDir -Force | Out-Null
    }
    if (-Not (Test-Path $historyDir)) {
        New-Item -ItemType Directory -Path $historyDir -Force | Out-Null
    }

    $ReportContent | Out-File -FilePath $historyFile -Encoding utf8
    Write-Output "✅ 保存历史报告: $historyFile"

    if ($L1Evidence) {
        $l1File = Join-Path $evidenceDir "L1-evidence.json"
        $L1Evidence | Out-File -FilePath $l1File -Encoding utf8
        Write-Output "✅ 保存 L1 证据: $l1File"
    }

    if ($L2Evidence) {
        $l2File = Join-Path $evidenceDir "L2-evidence.json"
        $L2Evidence | Out-File -FilePath $l2File -Encoding utf8
        Write-Output "✅ 保存 L2 证据: $l2File"
    }

    if ($L3Evidence) {
        $l3File = Join-Path $evidenceDir "L3-evidence.json"
        $L3Evidence | Out-File -FilePath $l3File -Encoding utf8
        Write-Output "✅ 保存 L3 证据: $l3File"
    }

    if ($L4Evidence) {
        $l4File = Join-Path $evidenceDir "L4-evidence.json"
        $L4Evidence | Out-File -FilePath $l4File -Encoding utf8
        Write-Output "✅ 保存 L4 证据: $l4File"
    }

    Write-Output "`n📊 保存完成"
    Write-Output "历史报告: $historyFile"
    Write-Output "证据文件: $evidenceDir\L[1-4]-evidence.json"

} catch {
    Write-Error "保存报告失败: $_"
    exit 1
}