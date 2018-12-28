#
# LoginViaAppclusive.ps1
#

Import-Module biz.dfch.PS.Appclusive.Client
$svc = Enter-ApcServer
Contract-Assert (Test-ApcStatus)

$uriName = 'com.abiquo.Platform_001.endpoint';
$mgmtUri = Get-ApcManagementUri $uriName;
Contract-Assert (!!$mgmtUri)

$mgmtCred = Get-ApcManagementCredential -Id $mgmtUri.ManagementCredentialId;
Contract-Assert (!!$mgmtCred)

Import-Module biz.dfch.PS.Abiquo.Client;
$biz_dfch_PS_Abiquo_Client.AuthenticationType = "oauth2";
$biz_dfch_PS_Abiquo_Client.Uri = $mgmtUri.Value;
$biz_dfch_PS_Abiquo_Client.OAuth2Token = $mgmtCred.Password;

$client = Enter-AbqServer -UseModuleContext
Contract-Assert ($client.IsLoggedIn)
$client

#Get-AbqEnterprise | Select Id, Name
#$eps = Get-AbqEnterprise
