<#
.SYNOPSIS
    计算文件的 SHA256 Hash 值
.DESCRIPTION
    用于代码安全审计 Skill 中对比文件是否发生变更
.PARAMETER FilePath
    要计算 Hash 的文件路径
.EXAMPLE
    .\compute-hash.ps1 -FilePath "OrderService.cs"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$FilePath
)

if (-Not (Test-Path $FilePath)) {
    Write-Error "文件不存在: $FilePath"
    exit 1
}

try {
    $hash = Get-FileHash -Path $FilePath -Algorithm SHA256
    Write-Output $hash.Hash.ToLower()
} catch {
    Write-Error "计算 Hash 失败: $_"
    exit 1
}