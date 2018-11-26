
function Clean
{
    if((Test-Path -Path .\bin )){
        Remove-Item -Recurse -Confirm:$false -Path:.\bin -Force:$true
    }
}
function Pack
{
    dotnet pack
}

function Deploy
{
    Copy-Item -Path:.\bin\Debug\*.nupkg -Destination:E:\Work\Code\Nugets
    "Package deployed to local nuget"
}

Clean
Pack
Deploy