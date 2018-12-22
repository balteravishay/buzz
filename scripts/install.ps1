
Param (
	[string]$url
 )
 
function InstallScheduler
{
    $taskPath = "\wget\"
    $name = 'wget'
    $exe = 'wget.exe'
    $location = "C:\temp\wget"
	$params = "--post-data 'user=azure&action=click' $url"
    Unregister-ScheduledTask -TaskName $name -TaskPath $taskPath -Confirm:$false -ErrorAction:SilentlyContinue  
    
	&schtasks /create /tn $name /sc minute /mo 5 /tr "$location$exe$params" /NP /RU SYSTEM
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

#Install applications
Unzip wget-1.20-win64.zip "C:\temp\wget"


#Run Scheduler
InstallScheduler

