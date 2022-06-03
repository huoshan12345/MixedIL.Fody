Get-ChildItem .\ -include bin,obj,bld,Backup,_UpgradeReport_Files,Debug,Release,ipch -Recurse -Hidden | foreach ($_) { remove-item $_.fullname -Force -Recurse }

Write-Output "Finished. Press any key to exit."
Read-Host