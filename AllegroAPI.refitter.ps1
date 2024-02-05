Write-Host "Downloading latest swagger.yaml from Allegro.pl ... " -ForegroundColor Yellow -NoNewline
Invoke-WebRequest https://developer.allegro.pl/swagger.yaml -OutFile swagger.yaml
Write-Host "Done ✅" -ForegroundColor Green

Get-ChildItem -Recurse -Filter "*.refitter" -ErrorAction "SilentlyContinue" | ForEach-Object {
    Write-Host "Compiling $($_.FullName) ..." -ForegroundColor Yellow -NoNewline
    $output = & dotnet refitter --settings-file $_.FullName --no-logging --no-banner
    # Write-Output $output # Write out OpenAPI statistics
    Write-Host "Done ✅" -ForegroundColor Green
    $generatedFilePath = $output | Select-String -Pattern "Output\:\s(.*\.cs)" | ForEach-Object { $_.Matches.Groups[1].value }
    Write-Host "Postprocessing output for $($generatedFilePath) ..." -ForegroundColor Yellow -NoNewline
    # Use improved String Enum converter that can handle underscores, spaces and hyphens
    (Get-Content -Path $generatedFilePath) -Replace 'JsonStringEnumConverter', 'JsonStringEnumMemberConverter' | Set-Content -Path $generatedFilePath
    Write-Host "Done ✅" -ForegroundColor Green
}

Write-Host "... Code generation completed!" -ForegroundColor Green
