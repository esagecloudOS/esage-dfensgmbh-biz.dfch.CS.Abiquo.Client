#
# Script.ps1
#

Push-Location 'C:\Github\biz.dfch.CS.Abiquo.Client\src\biz.dfch.PS.Abiquo.Client\bin\Debug';
Import-Module '.\biz.dfch.PS.Abiquo.Client.dll';
Get-Command -Module 'biz.dfch.PS.Abiquo.Client';

$testSomething = [biz.dfch.PS.Abiquo.Client.TestSomething]::new();
$testSomething | gm
