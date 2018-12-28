#
# Get-MachineByVirtualMachine.ps1
#

PARAM
(
	[Parameter(Mandatory = $true, Position = 0, ParameterSetName = 'name')]
	[ValidateNotNullOrEmpty()]
	[string] $Name
	,
	[Parameter(Mandatory = $true, Position = 0, ParameterSetName = 'id')]
	[ValidateNotNullOrEmpty()]
	[int] $Id
	,
	[Parameter(Mandatory = $true)]
	[ValidateNotNullOrEmpty()]
	[biz.dfch.CS.Abiquo.Client.BaseAbiquoClient] $Client

)

$datacenterSuffix = '/admin/datacenters'

$datacenters = $Client.Invoke([uri] ('{0}/{1}' -f $Client.AbiquoApiBaseUri, $datacenterSuffix.TrimStart('/')))
Contract-Assert (!!$datacenters)

foreach($datacenter in $datacenters.collection.links) 
{ 
	if($datacenter.GetOrDefault('type', $null) -ne 'application/vnd.abiquo.racks+json')
	{ 
		continue; 
	}
	$href = $datacenter.GetOrDefault('href', $null);

	$racks = $Client.Invoke([uri] $href);
	Contract-Assert (!!$racks)
	foreach($rack in $racks.collection.links) 
	{
		if($rack.GetOrDefault('type', $null) -ne 'application/vnd.abiquo.machines+json')
		{
			continue;
		}
		$href = $rack.GetOrDefault('href', $null);
		
		$machines = $Client.Invoke([uri] $href);
		Contract-Assert (!!$machines)
		foreach($machine in $machines.collection) 
		{
			foreach($machineLink in $machine.links)
			{
				if($machineLink.GetOrDefault('type', $null) -ne 'application/vnd.abiquo.virtualmachines+json')
				{
					continue;
				}
				$href = $machineLink.GetOrDefault('href', $null);
				
				$virtualmachines = $Client.Invoke([uri] $href);
				Contract-Assert (!!$virtualmachines)
				
				foreach($virtualmachine in $virtualmachines.collection)
				{
					if( ($virtualmachine.GetOrDefault('name', $null) -ne $Name) -and ($virtualmachine.GetOrDefault('id', $null) -ne $Id) )
					{
						continue;
					}

					Write-Output $machine;
					return;
				}
			}
		}
	}
}

# $machine = Get-MachineByVirtualMachine.ps1 -Id 42;
# $machine.GetOrDefault('description', 'undefined').Trim();
