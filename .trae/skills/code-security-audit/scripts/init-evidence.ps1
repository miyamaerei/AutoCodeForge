<#
.SYNOPSIS
    初始化代码安全审计证据目录结构
.DESCRIPTION
    为指定文件创建安全审计证据存储目录和历史记录目录
.PARAMETER FileName
    文件名（不含扩展名），如 "OrderService"
.PARAMETER BasePath
    证据基础路径，默认为 .autoCodeForge/security-audit
.EXAMPLE
    .\init-evidence.ps1 -FileName "OrderService"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$FileName,
    
    [string]$BasePath = ".autoCodeForge/security-audit"
)

$evidenceDir = Join-Path $BasePath $FileName
$historyDir = Join-Path $evidenceDir "history"

try {
    if (-Not (Test-Path $evidenceDir)) {
        New-Item -ItemType Directory -Path $evidenceDir -Force | Out-Null
        Write-Output "✅ 创建证据目录: $evidenceDir"
    } else {
        Write-Output "ℹ️  证据目录已存在: $evidenceDir"
    }

    if (-Not (Test-Path $historyDir)) {
        New-Item -ItemType Directory -Path $historyDir -Force | Out-Null
        Write-Output "✅ 创建历史目录: $historyDir"
    } else {
        Write-Output "ℹ️  历史目录已存在: $historyDir"
    }

    Write-Output "`n📁 目录结构:"
    Write-Output "$evidenceDir/"
    Write-Output "├── L1-evidence.json (待创建)"
    Write-Output "├── L2-evidence.json (待创建)"
    Write-Output "├── L3-evidence.json (待创建)"
    Write-Output "├── L4-evidence.json (待创建)"
    Write-Output "└── history/"

} catch {
    Write-Error "初始化目录失败: $_"
    exit 1
}