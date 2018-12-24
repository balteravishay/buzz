
Param (
	[string]$url
 )
 
function InstallScheduler
{
    $taskPath = "\wget\"
    $name = 'wget'
    $exe = 'wget.exe'
    $location = "C:\temp\"
	$params = " --post-data=\`"{\\\`"name\\\`":'\\\`"azure'\\\`"}\`" $url"
    Unregister-ScheduledTask -TaskName $name -TaskPath $taskPath -Confirm:$false -ErrorAction:SilentlyContinue  
   
	&schtasks /create /tn $name /sc minute /mo 1 /tr "$location$exe$params" /NP /RU SYSTEM
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

#Install applications
New-Item -ItemType directory -Path "C:\temp\"
Copy-Item "wget.exe" -Destination "C:\temp\"

#Run Scheduler
InstallScheduler

