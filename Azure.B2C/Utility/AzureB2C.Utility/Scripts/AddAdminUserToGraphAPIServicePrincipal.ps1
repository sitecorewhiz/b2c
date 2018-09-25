#Please refer the description of items in {} in the Readme.txt

Connect-AzureAD -TenantId '{tenant_name}'
$role = Get-AzureADDirectoryRole | Where-Object {$_.displayName -eq 'Company Administrator'} #AdminRole
$sp = Get-AzureADServicePrincipal | Where-Object {$_.appId -eq '{b2c_tenant_id}'} #ServicePrincipal (i.e. the Graph API)
Add-AzureADDirectoryRoleMember -ObjectId $role.ObjectId -RefObjectId $sp.ObjectId #Add the admin role to the API's service principal account