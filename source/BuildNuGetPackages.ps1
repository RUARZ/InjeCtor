param 
(
    [Parameter(Mandatory=$true)][string]$version,
    [string]$pushTarget = ""
)

echo "Start clean..."
dotnet clean -c Release

echo "Start build..."
dotnet build -c Release

if ($lastexitcode -ne 0)
{
    echo "Build failed, terminating execution..."
    exit
}

echo "Start tests..."
dotnet test -c Release

if ($lastexitcode -ne 0)
{
    echo "Tests failed, terminating execution..."
    exit
}


echo "Packing NuGet packages..."
nuget pack .\nuspec\InjeCtor.Core.nuspec -BasePath .\src\InjeCtor.Core\bin\Release\netstandard2.0\ -OutputDirectory Release -Version $version
nuget pack .\nuspec\InjeCtor.Configuration.nuspec -BasePath .\src\InjeCtor.Configuration\bin\Release\netstandard2.0\ -OutputDirectory Release -Version $version

if ($publishTarget -ne "")
{
    if ($lastexitcode -ne 0)
    {
        echo "Failed to create nuget packages and therfore can't push nuget packages!"
        exit
    }

    echo "Push nuget packages..."
    nuget push .\Release\*.nupkg -src $pushTarget
}