$files = @(
  "src\Tekus.Infrastructure\Persistence\Configurations\UserConfiguration.cs",
  "src\Tekus.Infrastructure\Persistence\Configurations\ProviderConfiguration.cs",
  "src\Tekus.Infrastructure\Persistence\Configurations\ServiceConfiguration.cs",
  "src\Tekus.Infrastructure\Persistence\Configurations\ProviderCustomFieldConfiguration.cs",
  "src\Tekus.Infrastructure\Persistence\Configurations\ServiceCountryConfiguration.cs",
  "src\Tekus.Infrastructure\Persistence\Configurations\AuditLogConfiguration.cs"
)
foreach ($f in $files) {
  $p = "D:\projects\tekus-provider-services\$f"
  $content = Get-Content $p -Raw
  if ($content -notmatch "ValueGeneratedNever") {
    $content = $content -replace '(builder\.HasKey\([^)]+\);\s*)', '$1' + "`n        builder.Property(x => x.Id).ValueGeneratedNever();`n        "
    Set-Content -Path $p -Value $content
  }
}
