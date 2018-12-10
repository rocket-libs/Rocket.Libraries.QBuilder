
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
	Copy-Item -Path:.\bin\Debug\*.nupkg -Destination:\\I27SIMBA\AutoKenya2\nugets
    "Package deployed to local nuget"
}

Clean
Pack
Deploy