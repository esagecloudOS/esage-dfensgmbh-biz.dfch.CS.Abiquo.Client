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
using System.Diagnostics.Contracts;
using biz.dfch.CS.Abiquo.Client.Authentication;
using biz.dfch.CS.Abiquo.Client.General;
using biz.dfch.CS.Abiquo.Client.v1.Model;
﻿using Task = biz.dfch.CS.Abiquo.Client.v1.Model.Task;

namespace biz.dfch.CS.Abiquo.Client
{
    [ContractClassFor(typeof(BaseAbiquoClient))]
    abstract class ContractClassForBaseAbiquoClient : BaseAbiquoClient
    {
        public override int TenantId
        {
            get
            {
                Contract.Requires(IsLoggedIn);
                Contract.Requires(null != CurrentUserInformation);

                return default(int);
            }
        }

        #region Login

        public override bool Login(string abiquoApiBaseUri, IAuthenticationInformation authenticationInformation)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(abiquoApiBaseUri));
            Contract.Requires(null != authenticationInformation);
            Contract.Ensures(Contract.Result<bool>() == !string.IsNullOrWhiteSpace(this.AbiquoApiBaseUri));
            Contract.Ensures(Contract.Result<bool>() == (null != this.AuthenticationInformation));
            Contract.Ensures(Contract.Result<bool>() == (null != this.CurrentUserInformation));

            return default(bool);
        }

        #endregion Login


        #region Invoke Link(s)

        public override T InvokeLink<T>(Link link)
        {
            Contract.Requires(null != link);
            Contract.Requires(!string.IsNullOrWhiteSpace(link.Type));
            Contract.Requires(!string.IsNullOrWhiteSpace(link.Href));

            return default(T);
        }

        public override AbiquoBaseDto InvokeLink(Link link)
        {
            Contract.Requires(null != link);
            Contract.Requires(!string.IsNullOrWhiteSpace(link.Type));
            Contract.Requires(!string.IsNullOrWhiteSpace(link.Href));

            return default(AbiquoBaseDto);
        }

        #endregion Invoke Link(s)


        #region Enterprises

        public override Enterprises GetEnterprises()
        {
            Contract.Ensures(null != Contract.Result<Enterprises>());

            return default(Enterprises);
        }

        public override Enterprise GetCurrentEnterprise()
        {
            return default(Enterprise);
        }

        public override Enterprise GetEnterprise(int id)
        {
            Contract.Requires(0 < id);

            return default(Enterprise);
        }

        #endregion Enterprises


        #region Users

        public override UsersWithRoles GetUsersWithRolesOfCurrentEnterprise()
        {
            Contract.Ensures(null != Contract.Result<UsersWithRoles>());

            return default(UsersWithRoles);
        }

        public override UsersWithRoles GetUsersWithRoles(Enterprise enterprise)
        {
            Contract.Requires(null != enterprise);

            return default(UsersWithRoles);
        }

        public override UsersWithRoles GetUsersWithRoles(int enterpriseId)
        {
            Contract.Requires(0 < enterpriseId);
            Contract.Ensures(null != Contract.Result<UsersWithRoles>());

            return default(UsersWithRoles);
        }

        public override User GetUserOfCurrentEnterprise(int id)
        {
            Contract.Requires(0 < id);

            return default(User);
        }

        public override User GetUser(Enterprise enterprise, int id)
        {
            Contract.Requires(null != enterprise);
            Contract.Requires(0 < id);

            return default(User);
        }

        public override User GetUser(int enterpriseId, int id)
        {
            Contract.Requires(0 < enterpriseId);
            Contract.Requires(0 < id);

            return default(User);
        }

        public override User GetUserInformation()
        {
            Contract.Requires(IsLoggedIn);

            return default(User);
        }

        public override User GetUserInformation(string username)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(username));

            return default(User);
        }

        public override User GetUserInformation(int enterpriseId, string username)
        {
            Contract.Requires(0 < enterpriseId);
            Contract.Requires(!string.IsNullOrWhiteSpace(username));

            return default(User);
        }

        public override void SwitchEnterprise(Enterprise enterprise)
        {
            Contract.Requires(null != enterprise);
        }

        public override void SwitchEnterprise(int id)
        {
            Contract.Requires(0 < id);
        }

        #endregion Users


        #region Roles

        public override Roles GetRoles()
        {
            Contract.Ensures(null != Contract.Result<Roles>());

            return default(Roles);
        }

        public override Role GetRole(int id)
        {
            Contract.Requires(0 < id);

            return default(Role);
        }

        #endregion Roles


        #region DataCentersLimits

        public override DataCentersLimits GetDataCentersLimitsOfCurrentEnterprise()
        {
            return default(DataCentersLimits);
        }

        public override DataCentersLimits GetDataCentersLimits(Enterprise enterprise)
        {
            Contract.Requires(null != enterprise);

            return default(DataCentersLimits);
        }

        public override DataCentersLimits GetDataCentersLimits(int enterpriseId)
        {
            Contract.Requires(0 < enterpriseId);

            return default(DataCentersLimits);
        }

        public override DataCenterLimits GetDataCenterLimitsOfCurrentEnterprise(int id)
        {
            Contract.Requires(0 < id);

            return default(DataCenterLimits);
        }

        public override DataCenterLimits GetDataCenterLimits(Enterprise enterprise, int id)
        {
            Contract.Requires(null != enterprise);
            Contract.Requires(0 < id);

            return default(DataCenterLimits);
        }

        public override DataCenterLimits GetDataCenterLimits(int enterpriseId, int id)
        {
            Contract.Requires(0 < enterpriseId);
            Contract.Requires(0 < id);

            return default(DataCenterLimits);
        }

        #endregion DataCentersLimits


        #region VirtualMachines

        public override VirtualMachines GetAllVirtualMachines()
        {
            Contract.Ensures(null != Contract.Result<VirtualMachines>());

            return default(VirtualMachines);
        }

        public override VirtualMachines GetVirtualMachines(VirtualAppliance virtualAppliance)
        {
            Contract.Requires(null != virtualAppliance);

            return default(VirtualMachines);
        }

        public override VirtualMachine GetVirtualMachine(VirtualAppliance virtualAppliance, int id)
        {
            Contract.Requires(null != virtualAppliance);
            Contract.Requires(0 < id);

            return default(VirtualMachine);
        }

        public override VirtualMachines GetVirtualMachines(int virtualDataCenterId, int virtualApplianceId)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Ensures(null != Contract.Result<VirtualMachines>());

            return default(VirtualMachines);
        }

        public override VirtualMachine GetVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int id)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < id);

            return default(VirtualMachine);
        }

        public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int enterpriseId, int dataCenterRepositoryId,
            int virtualMachineTemplateId)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < enterpriseId);
            Contract.Requires(0 < dataCenterRepositoryId);
            Contract.Requires(0 < virtualMachineTemplateId);

            return default(VirtualMachine);
        }

        public override VirtualMachine CreateVirtualMachine(VirtualAppliance virtualAppliance, VirtualMachineTemplate virtualMachineTemplate)
        {
            Contract.Requires(null != virtualAppliance);
            Contract.Requires(null != virtualMachineTemplate);

            return default(VirtualMachine);
        }

        public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, string virtualMachineTemplateHref)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(!string.IsNullOrWhiteSpace(virtualMachineTemplateHref));
            Contract.Requires(Uri.IsWellFormedUriString(virtualMachineTemplateHref, UriKind.Absolute));

            return default(VirtualMachine);
        }

        public override VirtualMachine CreateVirtualMachine(VirtualAppliance virtualAppliance, VirtualMachineTemplate virtualMachineTemplate, VirtualMachine virtualMachine)
        {
            Contract.Requires(null != virtualAppliance);
            Contract.Requires(null != virtualMachineTemplate);
            Contract.Requires(null != virtualMachine);
            Contract.Requires(virtualMachine.IsValid());

            return default(VirtualMachine);
        }

        public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int enterpriseId, int dataCenterRepositoryId,
            int virtualMachineTemplateId, VirtualMachineBase virtualMachine)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < enterpriseId);
            Contract.Requires(0 < dataCenterRepositoryId);
            Contract.Requires(0 < virtualMachineTemplateId);
            Contract.Requires(null != virtualMachine);
            Contract.Requires(virtualMachine.IsValid());

            return default(VirtualMachine);
        }

        public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, string virtualMachineTemplateHref,
            VirtualMachineBase virtualMachine)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(!string.IsNullOrWhiteSpace(virtualMachineTemplateHref));
            Contract.Requires(Uri.IsWellFormedUriString(virtualMachineTemplateHref, UriKind.Absolute));
            Contract.Requires(null != virtualMachine);
            Contract.Requires(virtualMachine.IsValid());

            return default(VirtualMachine);
        }

        public override Task DeployVirtualMachine(VirtualMachine virtualMachine, bool force)
        {
            Contract.Requires(null != virtualMachine);

            return default(Task);
        }

        public override Task DeployVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);

            return default(Task);
        }

        public override Task DeployVirtualMachine(VirtualMachine virtualMachine, bool force, bool waitForCompletion)
        {
            Contract.Requires(null != virtualMachine);

            return default(Task);
        }

        public override Task DeployVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force, bool waitForCompletion)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);

            return default(Task);
        }

        public override Task UpdateVirtualMachine(VirtualMachine virtualMachine, bool force)
        {
            Contract.Requires(null != virtualMachine);

            return default(Task);
        }

        public override Task UpdateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
            VirtualMachine virtualMachine, bool force)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);
            Contract.Requires(null != virtualMachine);
            Contract.Requires(virtualMachine.IsValid());

            return default(Task);
        }

        public override Task UpdateVirtualMachine(VirtualMachine virtualMachine, bool force, bool waitForCompletion)
        {
            Contract.Requires(null != virtualMachine);

            return default(Task);
        }

        public override Task UpdateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
            VirtualMachine virtualMachine, bool force, bool waitForCompletion)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);
            Contract.Requires(null != virtualMachine);
            Contract.Requires(virtualMachine.IsValid());

            return default(Task);
        }

        public override Task ChangeStateOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
            VirtualMachineState state)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);
            Contract.Requires(null != state);

            return default(Task);
        }

        public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineStateEnum state)
        {
            Contract.Requires(null != virtualMachine);

            return default(Task);
        }

        public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineStateEnum state, bool waitForCompletion)
        {
            Contract.Requires(null != virtualMachine);

            return default(Task);
        }

        public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineState state, bool waitForCompletion)
        {
            Contract.Requires(null != virtualMachine);
            Contract.Requires(null != state);

            return default(Task);
        }

        public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineState state)
        {
            Contract.Requires(null != virtualMachine);
            Contract.Requires(null != state);

            return default(Task);
        }

        public override Task ChangeStateOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
            VirtualMachineState state, bool waitForCompletion)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);

            return default(Task);
        }

        public override void ProtectVirtualMachine(VirtualMachine virtualMachine)
        {
            Contract.Requires(null != virtualMachine);
        }

        public override void ProtectVirtualMachine(VirtualMachine virtualMachine, string protectionCause)
        {
            Contract.Requires(null != virtualMachine);
            Contract.Requires(!string.IsNullOrWhiteSpace(protectionCause));
        }

        public override void ProtectVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, string protectionCause)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);
            Contract.Requires(!string.IsNullOrWhiteSpace(protectionCause));
        }

        public override void UnprotectVirtualMachine(VirtualMachine virtualMachine)
        {
            Contract.Requires(null != virtualMachine);
        }

        public override void UnprotectVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);
        }

        public override bool DeleteVirtualMachine(VirtualMachine virtualMachine)
        {
            Contract.Requires(null != virtualMachine);

            return default(bool);
        }

        public override bool DeleteVirtualMachine(VirtualMachine virtualMachine, bool force)
        {
            Contract.Requires(null != virtualMachine);

            return default(bool);
        }

        public override bool DeleteVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);

            return default(bool);
        }

        public override bool DeleteVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);

            return default(bool);
        }

        public override VmNetworkConfigurations GetNetworkConfigurationsForVm(VirtualMachine virtualMachine)
        {
            Contract.Requires(null != virtualMachine);

            return default(VmNetworkConfigurations);
        }

        public override VmNetworkConfigurations GetNetworkConfigurationsForVm(int virtualDataCenterId, int virtualApplianceId,
            int virtualMachineId)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);

            return default(VmNetworkConfigurations);
        }

        public override VmNetworkConfiguration GetNetworkConfigurationForVm(VirtualMachine virtualMachine, int id)
        {
            Contract.Requires(null != virtualMachine);
            Contract.Requires(0 < id);

            return default(VmNetworkConfiguration);
        }

        public override VmNetworkConfiguration GetNetworkConfigurationForVm(int virtualDataCenterId, int virtualApplianceId,
            int virtualMachineId, int id)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);
            Contract.Requires(0 < id);

            return default(VmNetworkConfiguration);
        }

        public override Nics GetNicsOfVirtualMachine(VirtualMachine virtualMachine)
        {
            Contract.Requires(null != virtualMachine);

            return default(Nics);
        }

        public override Nics GetNicsOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);

            return default(Nics);
        }

        public override Tasks GetAllTasksOfVirtualMachine(VirtualMachine virtualMachine)
        {
            Contract.Requires(null != virtualMachine);

            return default(Tasks);
        }

        public override Tasks GetAllTasksOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);
            Contract.Ensures(null != Contract.Result<Tasks>());

            return default(Tasks);
        }

        public override Task GetTaskOfVirtualMachine(VirtualMachine virtualMachine, string taskId)
        {
            Contract.Requires(null != virtualMachine);
            Contract.Requires(!string.IsNullOrEmpty(taskId));

            return default(Task);
        }

        public override Task GetTaskOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, string taskId)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < virtualApplianceId);
            Contract.Requires(0 < virtualMachineId);
            Contract.Requires(!string.IsNullOrWhiteSpace(taskId));

            return default(Task);
        }

        #endregion VirtualMachines


        #region VirtualMachineTemplates

        public override VirtualMachineTemplates GetVirtualMachineTemplates(DataCenterRepository dataCenterRepository)
        {
            Contract.Requires(null != dataCenterRepository);

            return default(VirtualMachineTemplates);
        }

        public override VirtualMachineTemplates GetVirtualMachineTemplates(int enterpriseId, int dataCenterRepositoryId)
        {
            Contract.Requires(0 < enterpriseId);
            Contract.Requires(0 < dataCenterRepositoryId);
            Contract.Ensures(null != Contract.Result<VirtualMachineTemplates>());

            return default(VirtualMachineTemplates);
        }

        public override VirtualMachineTemplate GetVirtualMachineTemplate(DataCenterRepository dataCenterRepository, int id)
        {
            Contract.Requires(null != dataCenterRepository);
            Contract.Requires(0 < id);

            return default(VirtualMachineTemplate);
        }

        public override VirtualMachineTemplate GetVirtualMachineTemplate(int enterpriseId, int dataCenterRepositoryId, int id)
        {
            Contract.Requires(0 < enterpriseId);
            Contract.Requires(0 < dataCenterRepositoryId);
            Contract.Requires(0 < id);

            return default(VirtualMachineTemplate);
        }

        #endregion VirtualMachineTemplates


        #region VirtualDataCenters

        public override VirtualDataCenters GetVirtualDataCenters()
        {
            Contract.Ensures(null != Contract.Result<VirtualDataCenters>());

            return default(VirtualDataCenters);
        }

        public override VirtualDataCenter GetVirtualDataCenter(int id)
        {
            Contract.Requires(0 < id);

            return default(VirtualDataCenter);
        }

        #endregion VirtualDataCenters


        #region VirtualAppliances

        public override VirtualAppliances GetVirtualAppliances(VirtualDataCenter virtualDataCenter)
        {
            Contract.Requires(null != virtualDataCenter);

            return default(VirtualAppliances);
        }

        public override VirtualAppliances GetVirtualAppliances(int virtualDataCenterId)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Ensures(null != Contract.Result<VirtualAppliances>());

            return default(VirtualAppliances);
        }

        public override VirtualAppliance GetVirtualAppliance(VirtualDataCenter virtualDataCenter, int id)
        {
            Contract.Requires(null != virtualDataCenter);
            Contract.Requires(0 < id);

            return default(VirtualAppliance);
        }

        public override VirtualAppliance GetVirtualAppliance(int virtualDataCenterId, int id)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < id);

            return default(VirtualAppliance);
        }

        #endregion VirtualAppliances


        #region DataCenterRepositories

        public override DataCenterRepositories GetDataCenterRepositoriesOfCurrentEnterprise()
        {
            Contract.Ensures(null != Contract.Result<DataCenterRepositories>());

            return default(DataCenterRepositories);
        }

        public override DataCenterRepositories GetDataCenterRepositories(Enterprise enterprise)
        {
            Contract.Requires(null != enterprise);

            return default(DataCenterRepositories);
        }

        public override DataCenterRepositories GetDataCenterRepositories(int enterpriseId)
        {
            Contract.Requires(0 < enterpriseId);
            Contract.Ensures(null != Contract.Result<DataCenterRepositories>());

            return default(DataCenterRepositories);
        }

        public override DataCenterRepository GetDataCenterRepositoryOfCurrentEnterprise(int id)
        {
            Contract.Requires(0 < id);

            return default(DataCenterRepository);
        }

        public override DataCenterRepository GetDataCenterRepository(Enterprise enterprise, int id)
        {
            Contract.Requires(null != enterprise);
            Contract.Requires(0 < id);

            return default(DataCenterRepository);
        }

        public override DataCenterRepository GetDataCenterRepository(int enterpriseId, int id)
        {
            Contract.Requires(0 < enterpriseId);
            Contract.Requires(0 < id);
            
            return default(DataCenterRepository);
        }

        #endregion DataCenterRepositories


        #region Tasks

        public override Task WaitForTaskCompletion(biz.dfch.CS.Abiquo.Client.v1.Model.Task task, int taskPollingWaitTimeMilliseconds, int taskPollingTimeoutMilliseconds)
        {
            Contract.Requires(null != task);
            Contract.Requires(task.IsValid());
            Contract.Requires(0 < taskPollingWaitTimeMilliseconds);
            Contract.Requires(0 < taskPollingTimeoutMilliseconds);

            return default(Task);
        }

        #endregion Tasks


        #region Networks

        public override VlanNetworks GetPrivateNetworks(VirtualDataCenter virtualDataCenter)
        {
            Contract.Requires(null != virtualDataCenter);

            return default(VlanNetworks);
        }

        public override VlanNetworks GetPrivateNetworks(int virtualDataCenterId)
        {
            Contract.Requires(0 < virtualDataCenterId);

            return default(VlanNetworks);
        }

        public override VlanNetwork GetPrivateNetwork(VirtualDataCenter virtualDataCenter, int id)
        {
            Contract.Requires(null != virtualDataCenter);
            Contract.Requires(0 < id);

            return default(VlanNetwork);
        }

        public override VlanNetwork GetPrivateNetwork(int virtualDataCenterId, int id)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < id);

            return default(VlanNetwork);
        }

        public override PrivateIps GetIpsOfPrivateNetwork(VlanNetwork vlan, bool free)
        {
            Contract.Requires(null != vlan);

            return default(PrivateIps);
        }

        public override PrivateIps GetIpsOfPrivateNetwork(int virtualDataCenterId, int privateNetworkId, bool free)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < privateNetworkId);

            return default(PrivateIps);
        }

        public override VlanNetworks GetExternalNetworksOfCurrentEnterprise(int dataCenterLimitsId)
        {
            Contract.Requires(0 < dataCenterLimitsId);

            return default(VlanNetworks);
        }

        public override VlanNetworks GetExternalNetworks(int enterpriseId, int dataCenterLimitsId)
        {
            Contract.Requires(0 < enterpriseId);
            Contract.Requires(0 < dataCenterLimitsId);

            return default(VlanNetworks);
        }

        public override VlanNetwork GetExternalNetworkOfCurrentEnterprise(int dataCenterLimitsId, int id)
        {
            Contract.Requires(0 < dataCenterLimitsId);
            Contract.Requires(0 < id);

            return default(VlanNetwork);
        }

        public override VlanNetwork GetExternalNetwork(int enterpriseId, int dataCenterLimitsId, int id)
        {
            Contract.Requires(0 < enterpriseId);
            Contract.Requires(0 < dataCenterLimitsId);
            Contract.Requires(0 < id);

            return default(VlanNetwork);
        }

        public override ExternalIps GetIpsOfExternalNetworkOfCurrentEnterprise(VlanNetwork vlan, bool free)
        {
            Contract.Requires(null != vlan);

            return default(ExternalIps);
        }

        public override ExternalIps GetIpsOfExternalNetworkOfCurrentEnterprise(int dataCenterLimitsId, int externalNetworkId, bool free)
        {
            Contract.Requires(0 < dataCenterLimitsId);
            Contract.Requires(0 < externalNetworkId);

            return default(ExternalIps);
        }

        public override ExternalIps GetIpsOfExternalNetwork(int enterpriseId, int dataCenterLimitsId, int externalNetworkId, bool free)
        {
            Contract.Requires(0 < enterpriseId);
            Contract.Requires(0 < dataCenterLimitsId);
            Contract.Requires(0 < externalNetworkId);

            return default(ExternalIps);
        }

        public override VlanNetworks GetPublicNetworks(VirtualDataCenter virtualDataCenter)
        {
            Contract.Requires(null != virtualDataCenter);

            return default(VlanNetworks);
        }

        public override VlanNetworks GetPublicNetworks(int virtualDataCenterId)
        {
            Contract.Requires(0 < virtualDataCenterId);

            return default(VlanNetworks);
        }

        public override VlanNetwork GetPublicNetwork(VirtualDataCenter virtualDataCenter, int id)
        {
            Contract.Requires(null != virtualDataCenter);
            Contract.Requires(0 < id);

            return default(VlanNetwork);
        }

        public override VlanNetwork GetPublicNetwork(int virtualDataCenterId, int id)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < id);

            return default(VlanNetwork);
        }

        public override PublicIps GetPublicIpsToPurchaseOfPublicNetwork(VirtualDataCenter virtualDataCenter, VlanNetwork vlan)
        {
            Contract.Requires(null != virtualDataCenter);
            Contract.Requires(null != vlan);

            return default(PublicIps);
        }

        public override PublicIps GetPublicIpsToPurchaseOfPublicNetwork(int virtualDataCenterId, int vlanId)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < vlanId);

            return default(PublicIps);
        }

        public override PublicIp PurchasePublicIp(VirtualDataCenter virtualDataCenter, PublicIp publicIp)
        {
            Contract.Requires(null != virtualDataCenter);
            Contract.Requires(null != publicIp);

            return default(PublicIp);
        }

        public override PublicIp PurchasePublicIp(int virtualDataCenterId, int publicIpid)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < publicIpid);

            return default(PublicIp);
        }

        public override PublicIp ReleasePublicIp(VirtualDataCenter virtualDataCenter, PublicIp publicIp)
        {
            Contract.Requires(null != virtualDataCenter);
            Contract.Requires(null != publicIp);

            return default(PublicIp);
        }

        public override PublicIp ReleasePublicIp(int virtualDataCenterId, int publicIpid)
        {
            Contract.Requires(0 < virtualDataCenterId);
            Contract.Requires(0 < publicIpid);

            return default(PublicIp);
        }

        #endregion Networks
    }
}
