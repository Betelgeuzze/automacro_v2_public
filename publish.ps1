# ----------------------------
# Clean publish folder
# ----------------------------
$publish = "bin\Release\net8.0-windows\win-x64\publish"

if (Test-Path $publish) {
    Remove-Item $publish -Recurse -Force
    Write-Host "✔ Publish folder cleaned."
} else {
    Write-Host "ℹ Publish folder not found. Continuing..."
}

# ----------------------------
# Build and publish (safe, no trimming)
# ----------------------------
Write-Host "🚀 Publishing optimized single-file build (no trimming)..."

dotnet publish -c Release -r win-x64 `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:EnableCompressionInSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true
# Note: For ILLink attributes, edit your .csproj to use <AssemblyTrimMode> and [DynamicDependency] as needed.

if ($LASTEXITCODE -eq 0) {
    Write-Host "🎉 Publish completed successfully!"
    Write-Host "📁 Output folder: $publish"
} else {
    Write-Host "❌ Publish failed!"
}
