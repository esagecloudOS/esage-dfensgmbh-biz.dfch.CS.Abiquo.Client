/**
 * Copyright 2016 d-fens GmbH
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Management.Automation;
using System.Text;
using biz.dfch.CS.Abiquo.Client.v1.Model;
using biz.dfch.CS.PowerShell.Commons;

namespace biz.dfch.PS.Abiquo.Client
{
    /// <summary>
    /// This class defines the GetMachine Cmdlet that retrieves a list of machines
    /// </summary>
    [Cmdlet(
         VerbsCommon.Get, "Machine"
         ,
         ConfirmImpact = ConfirmImpact.Low
         ,
         DefaultParameterSetName = ParameterSets.LIST
         ,
         SupportsShouldProcess = true
         ,
         HelpUri = "http://dfch.biz/biz/dfch/PS/Abiquo/Client/Get-Machine/"
     )]
    [OutputType(typeof(VirtualMachine))]
    public class GetMachine : PsCmdletBase
    {
        /// <summary>
        /// Defines all valid parameter sets for this cmdlet
        /// </summary>
        public static class ParameterSets
        {
            /// <summary>
            /// ParameterSetName used when specifying a credential object
            /// </summary>
            public const string LIST = "list";

            /// <summary>
            /// ParameterSetName used when specifying a single machine by its id
            /// </summary>
            public const string ID = "id";

            /// <summary>
            /// ParameterSetName used when specifying an OAuth2 token
            /// </summary>
            public const string NAME = "name";
        }

        /// <summary>
        /// Specifies the machine id
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSets.ID)]
        [ValidateRange(1, int.MaxValue)]
        public int Id { get; set; }

        /// <summary>
        /// Specifies the name name
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSets.NAME)]
        public string Name { get; set; }

        /// <summary>
        /// VirtualDataCenterId
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSets.LIST)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSets.ID)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSets.NAME)]
        [ValidateRange(1, int.MaxValue)]
        [Alias("vdc")]
        public int VirtualDataCenterId { get; set; }

        /// <summary>
        /// VirtualApplianceId
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSets.LIST)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSets.ID)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSets.NAME)]
        [ValidateRange(1, int.MaxValue)]
        [Alias("vapp")]
        public int VirtualApplianceId { get; set; }

        /// <summary>
        /// Retrieve all machines for the current enterprise
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSets.LIST)]
        public SwitchParameter ListAvailable { get; set; }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var isValidVirtualDataCenterIdAndVirtualApplianceIdCombination =
                !(0 < VirtualApplianceId && 0 >= VirtualDataCenterId);
            Contract.Assert(isValidVirtualDataCenterIdAndVirtualApplianceIdCombination);

            Contract.Assert(null != ModuleConfiguration.Current.Client);
            Contract.Assert(ModuleConfiguration.Current.Client.IsLoggedIn);

            var shouldProcessMessage = string.Format(Messages.GetMachineShouldProcess, ParameterSetName);
            if (!ShouldProcess(shouldProcessMessage))
            {
                return;
            }

            switch (ParameterSetName)
            {
                case ParameterSets.LIST:
                {
                    ProcessParameterSetList();
                    return;
                }

                case ParameterSets.ID:
                {
                    ProcessParameterSetId();
                    return;
                }

                case ParameterSets.NAME:
                {
                    ProcessParameterSetName();
                    return;
                }

                default:
                    const bool isValidParameterSetName = false;
                    Contract.Assert(isValidParameterSetName, ParameterSetName);
                    break;
            }

        }

        private void ProcessParameterSetId()
        {
            var result = default(VirtualMachine);

            if (MyInvocation.BoundParameters.ContainsKey("VirtualDataCenterId") ||
                MyInvocation.BoundParameters.ContainsKey("VirtualApplianceId"))
            {
                try
                {
                    result = ModuleConfiguration.Current.Client.GetVirtualMachine(VirtualDataCenterId, VirtualApplianceId, Id);
                }
                catch (Exception ex)
                {
                    WriteError(ErrorRecordFactory.GetGeneric(ex));
                }
            }
            else
            {
                var collection = ModuleConfiguration.Current.Client
                                     .GetAllVirtualMachines()
                                     .Collection ?? new List<VirtualMachine>();

                result = collection.FirstOrDefault(e => e.Id.HasValue && e.Id.Value == Id);
                
            }

            if (null == result)
            {
                WriteError(ErrorRecordFactory.GetNotFound(Messages.GetMachineIdNotFound, Constants.EventId.GetMachineIdNotFound.ToString(), Id));
                return;
            }

            WriteObject(result);
        }

        private void ProcessParameterSetName()
        {
            var collection = ModuleConfiguration.Current.Client
                                 .GetAllVirtualMachines()
                                 .Collection ?? new List<VirtualMachine>();
            var results = collection
                .Where(e => Name.Equals(e.Name, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            if (0 == results.Count)
            {
                WriteError(ErrorRecordFactory.GetNotFound(Messages.GetMachineNameNotFound, Constants.EventId.GetMachineNameNotFound.ToString(), Name));
                return;
            }

            results.ForEach(WriteObject);
        }

        private void ProcessParameterSetList()
        {
            var collection = new List<VirtualMachine>();

            if (0 >= VirtualDataCenterId && 0 >= VirtualApplianceId)
            {
                collection = ModuleConfiguration.Current.Client
                                 .GetAllVirtualMachines()
                                 .Collection ?? new List<VirtualMachine>();
                collection.ForEach(WriteObject);
                return;
            }

            try
            {
                var virtualAppliances = new List<VirtualAppliance>();
                if (0 < VirtualApplianceId)
                {
                    virtualAppliances.Add(new VirtualAppliance() { Id = VirtualApplianceId });
                }
                else
                {
                    virtualAppliances.AddRange(ModuleConfiguration.Current.Client.GetVirtualAppliances(VirtualDataCenterId).Collection ?? new List<VirtualAppliance>());
                }

                foreach (var virtualAppliance in virtualAppliances)
                {
                    collection.AddRange(ModuleConfiguration.Current.Client.GetVirtualMachines(VirtualDataCenterId, virtualAppliance.Id).Collection ?? new List<VirtualMachine>());
                }

                collection.ForEach(WriteObject);
            }
            catch (Exception ex)
            {
                WriteError(ErrorRecordFactory.GetGeneric(ex));
                WriteError(0 < VirtualApplianceId
                    ? ErrorRecordFactory.GetNotFound(Messages.GetMachineVdcVappNotFound,
                        Constants.EventId.GetMachineVirtualDataCenterOrVirtualApplianceNotFound.ToString(),
                        VirtualDataCenterId, VirtualApplianceId)
                    : ErrorRecordFactory.GetNotFound(Messages.GetMachineVdcNotFound,
                        Constants.EventId.GetMachineVirtualDataCenterOrVirtualApplianceNotFound.ToString(),
                        VirtualDataCenterId));
            }
        }
    }
}
