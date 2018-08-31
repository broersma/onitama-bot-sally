$APIKEY = Get-Content $PSScriptRoot/apikey.txt

$sourcePath = ($PSScriptRoot + "/publish")
$targetPath = ($PSScriptRoot + "/published.zip")

$uri = "https://botchallenge.intern.infi.nl/api/upload/bot/$APIKEY"

if(Test-Path $targetPath)  {
    Remove-Item $targetPath
}

Push-Location $PSScriptRoot
dotnet publish -o $sourcePath
Pop-Location

Add-Type -Assembly System.IO.Compression.FileSystem
$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
[System.IO.Compression.ZipFile]::CreateFromDirectory($sourcePath, $targetPath, $compressionLevel, $false)

Remove-Item -Recurse -Force $sourcePath

$bytes = (New-Object Net.WebClient).UploadFile($uri, $targetPath)

# Writes response from server to command line:
[System.Text.Encoding]::UTF8.GetString($bytes)
