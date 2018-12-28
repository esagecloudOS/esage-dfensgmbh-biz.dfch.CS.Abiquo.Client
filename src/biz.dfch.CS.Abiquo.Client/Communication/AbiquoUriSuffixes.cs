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

namespace biz.dfch.CS.Abiquo.Client.Communication
{
    public static class AbiquoUriSuffixes
    {
        public const string LOGIN = "/login";

        #region Enterprises
        
        public const string ENTERPRISES = "/admin/enterprises";
        public const string ENTERPRISE_BY_ID = "/admin/enterprises/{0}";
        
        #endregion Enterprises


        #region Users
        
        public const string USERSWITHROLES_BY_ENTERPRISE_ID = "/admin/enterprises/{0}/users";
        public const string USER_BY_ENTERPRISE_ID_AND_USER_ID = "/admin/enterprises/{0}/users/{1}";
        public const string SWITCH_ENTERPRISE_BY_USER_ID = "/admin/enterprises/_/users/{0}";
       
        #endregion Users


        #region Roles
        
        public const string ROLES = "/admin/roles";
        public const string ROLE_BY_ID = "/admin/roles/{0}";
        
        #endregion Roles


        #region DataCentersLimits

        public const string DATACENTERS_LIMITS_BY_ENTERPRISE_ID = "/admin/enterprises/{0}/limits";
        public const string DATACENTER_LIMITS_BY_ENTERPRISE_ID_AND_DATACENTER_LIMITS_ID = "/admin/enterprises/{0}/limits/{1}";

        #endregion DataCentersLimits


        #region VirtualMachines

        public const string VIRTUALMACHINES = "/cloud/virtualmachines";
        public const string VIRTUALMACHINES_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID = "/cloud/virtualdatacenters/{0}/virtualappliances/{1}/virtualmachines";
        public const string VIRTUALMACHINE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID = "/cloud/virtualdatacenters/{0}/virtualappliances/{1}/virtualmachines/{2}";
        public const string DEPLOY_VIRTUALMACHINE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID = "/cloud/virtualdatacenters/{0}/virtualappliances/{1}/virtualmachines/{2}/action/deploy";
        public const string CHANGE_VIRTUALMACHINE_STATE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID = "/cloud/virtualdatacenters/{0}/virtualappliances/{1}/virtualmachines/{2}/state";
        public const string PROTECT_VIRTUALMACHINE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID = "/cloud/virtualdatacenters/{0}/virtualappliances/{1}/virtualmachines/{2}/action/protect";
        public const string UNPROTECT_VIRTUALMACHINE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID = "/cloud/virtualdatacenters/{0}/virtualappliances/{1}/virtualmachines/{2}/action/unprotect";
        public const string NETWORK_CONFIGURATIONS_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPPLIANCE_ID_AND_VIRTUALMACHINE_ID = "/cloud/virtualdatacenters/{0}/virtualappliances/{1}/virtualmachines/{2}/network/configurations";
        public const string NETWORK_CONFIGURATION_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPPLIANCE_ID_AND_VIRTUALMACHINE_ID_AND_NETWORK_CONFIGURATION_ID = "/cloud/virtualdatacenters/{0}/virtualappliances/{1}/virtualmachines/{2}/network/configurations/{3}";
        public const string NICS_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPPLIANCE_ID_AND_VIRTUALMACHINE_ID = "/cloud/virtualdatacenters/{0}/virtualappliances/{1}/virtualmachines/{2}/network/nics";
        public const string VIRTUALMACHINETASKS_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPPLIANCE_ID_AND_VIRTUALMACHINE_ID = "/cloud/virtualdatacenters/{0}/virtualappliances/{1}/virtualmachines/{2}/tasks";
        public const string VIRTUALMACHINETASK_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPPLIANCE_ID_AND_VIRTUALMACHINE_ID_AND_TASK_ID = "/cloud/virtualdatacenters/{0}/virtualappliances/{1}/virtualmachines/{2}/tasks/{3}";
        
        #endregion VirtualMachines


        #region VirtualMachineTemplates
        
        public const string VIRTUALMACHINETEMPLATES_BY_ENTERPISE_ID_AND_DATACENTERREPOSITORY_ID = "/admin/enterprises/{0}/datacenterrepositories/{1}/virtualmachinetemplates";
        public const string VIRTUALMACHINETEMPLATE_BY_ENTERPISE_ID_AND_DATACENTERREPOSITORY_ID_AND_VIRTUALMACHINETEMPLATE_ID = "/admin/enterprises/{0}/datacenterrepositories/{1}/virtualmachinetemplates/{2}";
        
        #endregion VirtualMachineTemplates


        #region VirtualDataCenters
        
        public const string VIRTUALDATACENTERS = "/cloud/virtualdatacenters";
        public const string VIRTUALDATACENTER_BY_ID = "/cloud/virtualdatacenters/{0}";
        
        #endregion VirtualDataCenters


        #region VirtualAppliances
        
        public const string VIRTUALAPPLIANCES_BY_VIRTUALDATACENTER_ID = "/cloud/virtualdatacenters/{0}/virtualappliances";
        public const string VIRTUALAPPLIANCE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID = "/cloud/virtualdatacenters/{0}/virtualappliances/{1}";
        
        #endregion VirtualAppliances


        #region DateCenterRepositories
        
        public const string DATACENTERREPOSITORIES_BY_ENTERPRISE_ID = "/admin/enterprises/{0}/datacenterrepositories";
        public const string DATACENTERREPOSITORIES_BY_ENTERPRISE_ID_AND_DATACENTERREPOSITORY_ID = "/admin/enterprises/{0}/datacenterrepositories/{1}";
        
        #endregion DateCenterRepositories


        #region Networks

        public const string PRIVATE_NETWORKS_BY_VIRTUALDATACENTER_ID = "/cloud/virtualdatacenters/{0}/privatenetworks";
        public const string PRIVATE_NETWORK_BY_VIRTUALDATACENTER_ID_AND_PRIVATE_NETWORK_ID = "/cloud/virtualdatacenters/{0}/privatenetworks/{1}";
        public const string IPS_OF_PRIVATE_NETWORK_BY_VIRTUALDATACENTER_ID_AND_PRIVATE_NETWORK_ID = "/cloud/virtualdatacenters/{0}/privatenetworks/{1}/ips";

        public const string EXTERNAL_NETWORKS_BY_ENTERPRISE_ID_AND_LIMIT_ID = "/admin/enterprises/{0}/limits/{1}/externalnetworks";
        public const string EXTERNAL_NETWORK_BY_ENTERPRISE_ID_AND_LIMIT_ID_AND_EXTERNAL_NETWORK_ID = "/admin/enterprises/{0}/limits/{1}/externalnetworks/{2}";
        public const string IPS_OF_EXTERNAL_NETWORK_BY_ENTERPRISE_ID_AND_LIMIT_ID_AND_EXTERNAL_NETWORK_ID = "/admin/enterprises/{0}/limits/{1}/externalnetworks/{2}/ips";

        public const string PUBLIC_NETWORKS_BY_VIRTUALDATACENTER_ID = "/cloud/virtualdatacenters/{0}/publicvlans";
        public const string PUBLIC_NETWORK_BY_VIRTUALDATACENTER_ID_AND_PUBLIC_NETWORK_ID = "/cloud/virtualdatacenters/{0}/publicvlans/{1}";
        public const string PUBLIC_IPS_TO_PURCHASE_BY_VIRTUALDATACENTER_ID = "/cloud/virtualdatacenters/{0}/publicips/topurchase";
        public const string PURCHASED_PUBLIC_IP_BY_VIRTUALDATACENTER_ID_AND_PUBLICIP_ID = "/cloud/virtualdatacenters/{0}/publicips/purchased/{1}";
        public const string PUBLIC_IP_TO_PURCHASE_BY_VIRTUALDATACENTER_ID_AND_PUBLICIP_ID = "/cloud/virtualdatacenters/{0}/publicips/topurchase/{1}";

        #endregion Networks
    }
}
