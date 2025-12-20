param(
	[Parameter(Position=0, Mandatory=$false)]
	[string]$Environment = 'Local'
)

# Map environment to profile file (expects files like Local.publish.xml, Local.publish.xml, etc.)
$profileFile = "$Environment.publish.xml"

Write-Host "Building database project..."

dotnet build src\Asm.MooBank.Database\Asm.MooBank.Database.sqlproj -c Release -v m

Write-Host "Using environment: $Environment"
Write-Host "Using publish profile: $profileFile"

sqlpackage /Action:Publish /Profile:src\Asm.MooBank.Database\$profileFile /SourceFile:src\Asm.MooBank.Database\bin\Release\Asm.MooBank.Database.dacpac
