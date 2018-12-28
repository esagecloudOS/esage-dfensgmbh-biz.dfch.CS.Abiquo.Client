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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using biz.dfch.CS.Abiquo.Client.Authentication;
using biz.dfch.CS.Abiquo.Client.Communication;
using biz.dfch.CS.Abiquo.Client.General;
using biz.dfch.CS.Abiquo.Client.v1.Model;
using Task = biz.dfch.CS.Abiquo.Client.v1.Model.Task;
using System.Threading;
using HttpMethod = biz.dfch.CS.Commons.Rest.HttpMethod;
using System.Text.RegularExpressions;
using biz.dfch.CS.Commons.Converters;
using biz.dfch.CS.Commons.Rest;

namespace biz.dfch.CS.Abiquo.Client.v1
{
    public class AbiquoClient : BaseAbiquoClient
    {
        public const string ABIQUO_API_VERSION = "3.10";

        private const string FAKE_TASK_NAME = "FakeTask";
        private const string DEFAULT_PROTECTION_CAUSE = "Protected by AbiquoClient v1 without a special cause";

        private const string FILTER_KEY_FORCE = "force";
        private const string FILTER_VALUE_FORCE = "true";
        private const string FILTER_KEY_FREE = "free";
        private const string FILTER_VALUE_FREE = "true";
        private const string FILTER_KEY_VLANID = "vlanId";

        private const string LINK_TYPE_PATTERN = @"^application/vnd\.abiquo\.(\w+)\+.+$";
        private const string DATACENTERLIMITS_ID_PATTERN = @"^.+limits/(\d+)/.+$";
        private const string FQCN_SEPARATOR = ".";

        public override int TenantId
        {
            get
            {
                var enterpriseLink = CurrentUserInformation.GetLinkByRel(AbiquoRelations.ENTERPRISE);
                Contract.Assert(null != enterpriseLink);

                return UriHelper.ExtractIdAsInt(enterpriseLink.Href);
            }
        }

        internal AbiquoClient()
        {
            AbiquoApiVersion = ABIQUO_API_VERSION;

            TaskPollingWaitTimeMilliseconds = DEFAULT_TASK_POLLING_WAIT_TIME_MILLISECONDS;
            TaskPollingTimeoutMilliseconds = DEFAULT_TASK_POLLING_TIMEOUT_MILLISECONDS;
        }


        #region Login

        public override bool Login(string abiquoApiBaseUri, IAuthenticationInformation authenticationInformation)
        {
            // sanitise Uri (and removed extra information such as port numbers etc)
            abiquoApiBaseUri = new Uri(abiquoApiBaseUri).AbsoluteUri;
            Logger.Current.TraceEvent(TraceEventType.Start, (int) Constants.EventId.Login, Messages.AbiquoClientLoginStart, abiquoApiBaseUri);

            // clear base properties
            Logout();

            AbiquoApiBaseUri = abiquoApiBaseUri;
            AuthenticationInformation = authenticationInformation;

            try
            {
                var loginResponse = ExecuteRequest(AbiquoUriSuffixes.LOGIN);
                CurrentUserInformation = AbiquoBaseDto.DeserializeObject<User>(loginResponse);

                IsLoggedIn = true;
                Logger.Current.TraceEvent(TraceEventType.Information, (int) Constants.EventId.LoginSucceeded, Messages.AbiquoClientLoginSucceeded, AbiquoApiBaseUri);
                return true;
            }
            catch (HttpRequestException ex)
            {
                var message = string.Format(Messages.AbiquoClientLoginFailed, AbiquoApiBaseUri);
                Logger.Current.TraceException(ex, (int) Constants.EventId.LoginFailed, message);

                Logout();
                
                return false;
            }
        }

        #endregion Login


        #region Invoke Link(s)

        public override T InvokeLink<T>(Link link)
        {
            var result = InvokeLink(link);

            Contract.Assert(result.GetType() == typeof(T));

            return result as T;
        }

        public override AbiquoBaseDto InvokeLink(Link link)
        {
            var match = Regex.Match(link.Type, LINK_TYPE_PATTERN);
            Contract.Assert(match.Success);
            Contract.Assert(2 == match.Groups.Count);

            var headers = new HeaderBuilder().BuildAccept(link.Type).GetHeaders();
            var result = Invoke(UriHelper.ExtractRelativeUri(AbiquoApiBaseUri, link.Href), headers);

            var type = Type.GetType(string.Concat(typeof(AbiquoV1BaseDto).Namespace, FQCN_SEPARATOR, match.Groups[1].Value), 
                throwOnError: true, 
                ignoreCase: true);
            return (AbiquoBaseDto) AbiquoBaseDto.DeserializeObject(result, type);
        }

        #endregion Invoke Link(s)


        #region Enterprises

        public override Enterprises GetEnterprises()
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISES).GetHeaders();

            return Invoke<Enterprises>(AbiquoUriSuffixes.ENTERPRISES, headers);
        }

        public override Enterprise GetCurrentEnterprise()
        {
            return GetEnterprise(TenantId);
        }

        public override Enterprise GetEnterprise(int id)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISE).GetHeaders();
            var uriSuffix = string.Format(AbiquoUriSuffixes.ENTERPRISE_BY_ID, id);

            return Invoke<Enterprise>(uriSuffix, headers);
        }

        #endregion Enterprises


        #region Users
        private const string FILTER_KEY_HAS = "has";

        public override UsersWithRoles GetUsersWithRolesOfCurrentEnterprise()
        {
            return GetUsersWithRoles(TenantId);
        }

        public override UsersWithRoles GetUsersWithRoles(Enterprise enterprise)
        {
            return GetUsersWithRoles(enterprise.Id);
        }

        public override UsersWithRoles GetUsersWithRoles(int enterpriseId)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_USERSWITHROLES).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.USERSWITHROLES_BY_ENTERPRISE_ID, enterpriseId);
            return Invoke<UsersWithRoles>(uriSuffix, headers);
        }

        public override User GetUserOfCurrentEnterprise(int id)
        {
            return GetUser(TenantId, id);
        }

        public override User GetUser(Enterprise enterprise, int id)
        {
            return GetUser(enterprise.Id, id);
        }

        public override User GetUser(int enterpriseId, int id)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_USER).GetHeaders();
            var uriSuffix = string.Format(AbiquoUriSuffixes.USER_BY_ENTERPRISE_ID_AND_USER_ID, enterpriseId, id);
            
            return Invoke<User>(uriSuffix, headers);
        }

        public override User GetUserInformation()
        {
            return CurrentUserInformation;
        }

        public override User GetUserInformation(string username)
        {
            return GetUserInformation(TenantId, username);
        }

        public override User GetUserInformation(int enterpriseId, string username)
        {
            var filter = new FilterBuilder().BuildFilterPart(FILTER_KEY_HAS, username).GetFilter();
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_USERS).GetHeaders();
            var uriSuffix = string.Format(AbiquoUriSuffixes.USERSWITHROLES_BY_ENTERPRISE_ID, enterpriseId);

            var searchResult = Invoke<Users>(uriSuffix, filter, headers);

            var errorMsg = string.Format(Messages.AbiquoClientGetUserInformationNotFound, username,
                enterpriseId);
            Contract.Assert(null != searchResult.Collection);

            var user = searchResult.Collection.FirstOrDefault(u => u.Nick == username);
            Contract.Assert(null != user, errorMsg);

            return user;
        }

        public override void SwitchEnterprise(Enterprise enterprise)
        {
            var editLink = enterprise.GetLinkByRel(AbiquoRelations.EDIT);
            var enterpriseId = UriHelper.ExtractIdAsInt(editLink.Href);
            
            SwitchEnterprise(enterpriseId);
        }

        public override void SwitchEnterprise(int id)
        {
            // load enterprise to switch to
            var enterpriseToSwitchTo = GetEnterprise(id);
            Contract.Assert(null != enterpriseToSwitchTo);
            var hrefOfEnterpriseToSwitchTo = enterpriseToSwitchTo.GetLinkByRel(AbiquoRelations.EDIT).Href;

            var currentUser = GetUserOfCurrentEnterprise(CurrentUserInformation.Id);
            Contract.Assert(null != currentUser);

            // replace enterprise link on current user with link to enterprise to switch to
            var oldEnterpriseLink = currentUser.GetLinkByRel(AbiquoRelations.ENTERPRISE);
            Contract.Assert(currentUser.Links.Remove(oldEnterpriseLink));

            var enterpriseToSwitchToLink = new LinkBuilder().BuildRel(AbiquoRelations.ENTERPRISE).BuildHref(hrefOfEnterpriseToSwitchTo).GetLink();
            currentUser.Links.Add(enterpriseToSwitchToLink);

            // update user
            var uriSuffix = string.Format(AbiquoUriSuffixes.SWITCH_ENTERPRISE_BY_USER_ID, currentUser.Id);
            var headers = new Dictionary<string, string>()
            {
                { AbiquoHeaderKeys.ACCEPT_HEADER_KEY, VersionedAbiquoMediaDataTypes.VND_ABIQUO_USER }
                ,
                { AbiquoHeaderKeys.CONTENT_TYPE_HEADER_KEY, VersionedAbiquoMediaDataTypes.VND_ABIQUO_USER }
            };

            var updatedUser = Invoke<User>(HttpMethod.Put, uriSuffix, null, headers, currentUser);
            Contract.Assert(null != updatedUser);

            // update current user information
            CurrentUserInformation = updatedUser;
        }

        #endregion Users


        #region Roles

        public override Roles GetRoles()
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_ROLES).GetHeaders();

            return Invoke<Roles>(AbiquoUriSuffixes.ROLES, headers);
        }

        public override Role GetRole(int id)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_ROLE).GetHeaders();
            
            var uriSuffix = string.Format(AbiquoUriSuffixes.ROLE_BY_ID, id);
            return Invoke<Role>(uriSuffix, headers);
        }

        #endregion Roles


        #region DataCenterLimits

        public override DataCentersLimits GetDataCentersLimitsOfCurrentEnterprise()
        {
            return GetDataCentersLimits(TenantId);
        }

        public override DataCentersLimits GetDataCentersLimits(Enterprise enterprise)
        {
            return GetDataCentersLimits(enterprise.Id);
        }

        public override DataCentersLimits GetDataCentersLimits(int enterpriseId)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_LIMITS).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.DATACENTERS_LIMITS_BY_ENTERPRISE_ID, enterpriseId);

            return Invoke<DataCentersLimits>(uriSuffix, headers);
        }

        public override DataCenterLimits GetDataCenterLimitsOfCurrentEnterprise(int id)
        {
            return GetDataCenterLimits(TenantId, id);
        }

        public override DataCenterLimits GetDataCenterLimits(Enterprise enterprise, int id)
        {
            return GetDataCenterLimits(enterprise.Id, id);
        }

        public override DataCenterLimits GetDataCenterLimits(int enterpriseId, int id)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_LIMIT).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.DATACENTER_LIMITS_BY_ENTERPRISE_ID_AND_DATACENTER_LIMITS_ID, enterpriseId, id);

            return Invoke<DataCenterLimits>(uriSuffix, headers);
        }

        #endregion DataCentersLimits


        #region VirtualMachines

        public override VirtualMachines GetAllVirtualMachines()
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALMACHINES).GetHeaders();

            return Invoke<VirtualMachines>(AbiquoUriSuffixes.VIRTUALMACHINES, headers);
        }

        public override VirtualMachines GetVirtualMachines(VirtualAppliance virtualAppliance)
        {
            var virtualDataCenterLink = virtualAppliance.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDatacenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);

            return GetVirtualMachines(virtualDatacenterId, virtualAppliance.Id);
        }

        public override VirtualMachines GetVirtualMachines(int virtualDataCenterId, int virtualApplianceId)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALMACHINES).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.VIRTUALMACHINES_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID, virtualDataCenterId, virtualApplianceId);
            return Invoke<VirtualMachines>(uriSuffix, headers);
        }

        public override VirtualMachine GetVirtualMachine(VirtualAppliance virtualAppliance, int id)
        {
            var virtualDataCenterLink = virtualAppliance.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDatacenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);

            return GetVirtualMachine(virtualDatacenterId, virtualAppliance.Id, id);
        }

        public override VirtualMachine GetVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int id)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALMACHINE).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.VIRTUALMACHINE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID, virtualDataCenterId, virtualApplianceId, id);

            return Invoke<VirtualMachine>(uriSuffix, headers);
        }

        public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int enterpriseId, int dataCenterRepositoryId,
            int virtualMachineTemplateId)
        {
            return CreateVirtualMachine(virtualDataCenterId, virtualApplianceId, enterpriseId, dataCenterRepositoryId, virtualMachineTemplateId, new VirtualMachineBase());
        }

        public override VirtualMachine CreateVirtualMachine(VirtualAppliance virtualAppliance, VirtualMachineTemplate virtualMachineTemplate)
        {
            var virtualMachineTemplateLink = virtualMachineTemplate.GetLinkByRel(AbiquoRelations.EDIT);

            var virtualDataCenterLink = virtualAppliance.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDataCenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);

            return CreateVirtualMachine(virtualDataCenterId, virtualAppliance.Id, virtualMachineTemplateLink.Href);
        }

        public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, string virtualMachineTemplateHref)
        {
            return CreateVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineTemplateHref, new VirtualMachineBase());
        }

        public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int enterpriseId, int dataCenterRepositoryId,
            int virtualMachineTemplateId, VirtualMachineBase virtualMachine)
        {
            var virtualMachineTemplateHrefSuffix = string.Format(AbiquoUriSuffixes.VIRTUALMACHINETEMPLATE_BY_ENTERPISE_ID_AND_DATACENTERREPOSITORY_ID_AND_VIRTUALMACHINETEMPLATE_ID,
                enterpriseId, dataCenterRepositoryId, virtualMachineTemplateId);
            var virtualMachineTemplateHref = UriHelper.ConcatUri(AbiquoApiBaseUri, virtualMachineTemplateHrefSuffix);

            return CreateVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineTemplateHref, virtualMachine);
        }

        public override VirtualMachine CreateVirtualMachine(VirtualAppliance virtualAppliance, VirtualMachineTemplate virtualMachineTemplate, VirtualMachine virtualMachine)
        {
            var virtualDataCenterLink = virtualAppliance.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDataCenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);
            var virtualApplianceId = virtualAppliance.Id;
            var virtualMachineTemplateHref = virtualMachineTemplate.GetLinkByRel(AbiquoRelations.EDIT).Href;

            return CreateVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineTemplateHref,
                virtualMachine);
        }

        public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, string virtualMachineTemplateHref,
            VirtualMachineBase virtualMachine)
        {
            var virtualMachineLink = new LinkBuilder()
                .BuildRel(AbiquoRelations.VIRTUALMACHINETEMPLATE)
                .BuildHref(virtualMachineTemplateHref)
                .GetLink();

            virtualMachine.Links = new List<Link>() { virtualMachineLink };
            
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALMACHINE)
                .BuildContentType(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALMACHINE).GetHeaders();

            var uriSuffix =
                string.Format(AbiquoUriSuffixes.VIRTUALMACHINES_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID,
                    virtualDataCenterId, virtualApplianceId);

            return Invoke<VirtualMachine>(HttpMethod.Post, uriSuffix, null, headers, virtualMachine);
        }


        public override Task DeployVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force)
        {
            return DeployVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId, force, false);
        }

        public override Task DeployVirtualMachine(VirtualMachine virtualMachine, bool force)
        {
            return DeployVirtualMachine(virtualMachine, force, false);
        }

        public override Task DeployVirtualMachine(VirtualMachine virtualMachine, bool force, bool waitForCompletion)
        {
            var virtualDataCenterLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDataCenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);

            var virtualApplianceLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALAPPLIANCE);
            var virtualApplianceId = UriHelper.ExtractIdAsInt(virtualApplianceLink.Href);

            var virtualMachineId = virtualMachine.Id.GetValueOrDefault();
            Contract.Assert(0 < virtualMachineId);

            return DeployVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId, force, waitForCompletion);
        }

        public override Task DeployVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force, bool waitForCompletion)
        {
            Dictionary<string, object> filter = null;
            if (force)
            {
                filter = new FilterBuilder().BuildFilterPart(FILTER_KEY_FORCE, FILTER_VALUE_FORCE).GetFilter();
            }

            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_ACCEPTEDREQUEST).GetHeaders();

            var uriSuffix =
                string.Format(AbiquoUriSuffixes.DEPLOY_VIRTUALMACHINE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID,
                    virtualDataCenterId, virtualApplianceId, virtualMachineId);

            var deployTask = Invoke<AcceptedRequest>(HttpMethod.Post, uriSuffix, filter, headers, string.Empty);
            Contract.Assert(null != deployTask);

            var link = deployTask.GetLinkByRel(AbiquoRelations.STATUS);
            var taskId = UriHelper.ExtractLastSegmentAsString(link.Href);

            var task = GetTaskOfVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId, taskId);

            if (waitForCompletion)
            {
                return WaitForTaskCompletion(task, TaskPollingWaitTimeMilliseconds, TaskPollingTimeoutMilliseconds);
            }

            return task;
        }

        public override Task UpdateVirtualMachine(VirtualMachine virtualMachine, bool force)
        {
            return UpdateVirtualMachine(virtualMachine, force, false);
        }

        public override Task UpdateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
            VirtualMachine virtualMachine, bool force)
        {
            return UpdateVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId, virtualMachine, force, false);
        }

        public override Task UpdateVirtualMachine(VirtualMachine virtualMachine, bool force, bool waitForCompletion)
        {
            var virtualDataCenterLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDataCenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);

            var virtualApplianceLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALAPPLIANCE);
            var virtualApplianceId = UriHelper.ExtractIdAsInt(virtualApplianceLink.Href);

            var virtualMachineId = virtualMachine.Id.GetValueOrDefault();
            Contract.Assert(0 < virtualMachineId);

            return UpdateVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId, virtualMachine, force,
                waitForCompletion);
        }

        public override Task UpdateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
            VirtualMachine virtualMachine, bool force, bool waitForCompletion)
        {
            Dictionary<string, object> filter = null;
            if (force)
            {
                filter = new FilterBuilder().BuildFilterPart(FILTER_KEY_FORCE, FILTER_VALUE_FORCE).GetFilter();
            }

            var headers = new HeaderBuilder()
                .BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_ACCEPTEDREQUEST)
                .BuildContentType(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALMACHINE)
                .GetHeaders();

            var uriSuffix =
                string.Format(AbiquoUriSuffixes.VIRTUALMACHINE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID,
                    virtualDataCenterId, virtualApplianceId, virtualMachineId);

            // IMPORTANT
            //
            // If the VirtualMachine is already deployed, the update request results in http status code 202 and returns a task
            // If the VirtualMachine is not yet deployed, the update request results in http status code 204 and returns an empty body
            var updateResultAsString = Invoke(HttpMethod.Put, uriSuffix, filter, headers, virtualMachine);

            // Return fake task if updated was performed against a not yet deployed VirtualMachine
            if (string.IsNullOrWhiteSpace(updateResultAsString))
            {
                return new Task()
                {
                    TaskId = FAKE_TASK_NAME,
                    State = TaskStateEnum.FINISHED_SUCCESSFULLY,
                    Type = TaskTypeEnum.RECONFIGURE,
                    Timestamp = DateTimeOffset.Now.Millisecond
                };
            }

            var updateTask = AbiquoBaseDto.DeserializeObject<AcceptedRequest>(updateResultAsString);
            Contract.Assert(null != updateTask);

            var link = updateTask.GetLinkByRel(AbiquoRelations.STATUS);
            var taskId = UriHelper.ExtractLastSegmentAsString(link.Href);

            var task = GetTaskOfVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId, taskId);

            if (waitForCompletion)
            {
                return WaitForTaskCompletion(task, TaskPollingWaitTimeMilliseconds, TaskPollingTimeoutMilliseconds);
            }

            return task;
        }

        public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineStateEnum state)
        {
            var virtualMachineState = new VirtualMachineState
            {
                State = state
            };

            return ChangeStateOfVirtualMachine(virtualMachine, virtualMachineState);
        }

        public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineState state)
        {
            return ChangeStateOfVirtualMachine(virtualMachine, state, false);
        }

        public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineStateEnum state, bool waitForCompletion)
        {
            var virtualMachineState = new VirtualMachineState
            {
                State = state
            };

            return ChangeStateOfVirtualMachine(virtualMachine, virtualMachineState, waitForCompletion);
        }

        public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineState state, bool waitForCompletion)
        {
            var virtualDataCenterLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDataCenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);

            var virtualApplianceLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALAPPLIANCE);
            var virtualApplianceId = UriHelper.ExtractIdAsInt(virtualApplianceLink.Href);

            var virtualMachineId = virtualMachine.Id.GetValueOrDefault();
            Contract.Assert(0 < virtualMachineId);

            return ChangeStateOfVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId, state, waitForCompletion);
        }

        public override Task ChangeStateOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
            VirtualMachineState state)
        {
            return ChangeStateOfVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId, state, false);
        }

        public override Task ChangeStateOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
            VirtualMachineState state, bool waitForCompletion)
        {
            var headers = new HeaderBuilder()
                .BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_ACCEPTEDREQUEST)
                .BuildContentType(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALMACHINESTATE)
                .GetHeaders();

            var uriSuffix =
                string.Format(AbiquoUriSuffixes.CHANGE_VIRTUALMACHINE_STATE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID,
                    virtualDataCenterId, virtualApplianceId, virtualMachineId);

            var changeStateTask = Invoke<AcceptedRequest>(HttpMethod.Put, uriSuffix, null, headers, state.SerializeObject());
            Contract.Assert(null != changeStateTask);

            var link = changeStateTask.GetLinkByRel(AbiquoRelations.STATUS);
            var taskId = UriHelper.ExtractLastSegmentAsString(link.Href);

            var task = GetTaskOfVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId, taskId);

            if (waitForCompletion)
            {
                return WaitForTaskCompletion(task, TaskPollingWaitTimeMilliseconds, TaskPollingTimeoutMilliseconds);
            }

            return task;
        }

        public override void ProtectVirtualMachine(VirtualMachine virtualMachine)
        {
            ProtectVirtualMachine(virtualMachine, DEFAULT_PROTECTION_CAUSE);
        }

        public override void ProtectVirtualMachine(VirtualMachine virtualMachine, string protectionCause)
        {
            var virtualDataCenterLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDataCenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);

            var virtualApplianceLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALAPPLIANCE);
            var virtualApplianceId = UriHelper.ExtractIdAsInt(virtualApplianceLink.Href);

            var virtualMachineId = virtualMachine.Id.GetValueOrDefault();
            Contract.Assert(0 < virtualMachineId);

            ProtectVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId, protectionCause);
        }

        public override void ProtectVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, string protectionCause)
        {
            var headers = new HeaderBuilder()
                .BuildAccept(ContentType.TextPlain.GetStringValue())
                .BuildContentType(ContentType.TextPlain.GetStringValue())
                .GetHeaders();

            var uriSuffix =
                string.Format(AbiquoUriSuffixes.PROTECT_VIRTUALMACHINE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID,
                    virtualDataCenterId, virtualApplianceId, virtualMachineId);

            Invoke(HttpMethod.Post, uriSuffix, null, headers, protectionCause);
        }

        public override void UnprotectVirtualMachine(VirtualMachine virtualMachine)
        {
            var virtualDataCenterLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDataCenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);

            var virtualApplianceLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALAPPLIANCE);
            var virtualApplianceId = UriHelper.ExtractIdAsInt(virtualApplianceLink.Href);

            var virtualMachineId = virtualMachine.Id.GetValueOrDefault();
            Contract.Assert(0 < virtualMachineId);

            UnprotectVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId);
        }

        public override void UnprotectVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
        {
            var headers = new HeaderBuilder()
                .BuildAccept(ContentType.TextPlain.GetStringValue())
                .BuildContentType(ContentType.TextPlain.GetStringValue())
                .GetHeaders();

            var uriSuffix =
                string.Format(AbiquoUriSuffixes.UNPROTECT_VIRTUALMACHINE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID,
                    virtualDataCenterId, virtualApplianceId, virtualMachineId);

            Invoke(HttpMethod.Post, uriSuffix, null, headers, "");
        }

        public override bool DeleteVirtualMachine(VirtualMachine virtualMachine)
        {
            return DeleteVirtualMachine(virtualMachine, false);
        }

        public override bool DeleteVirtualMachine(VirtualMachine virtualMachine, bool force)
        {
            var virtualDataCenterLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDataCenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);

            var virtualApplianceLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALAPPLIANCE);
            var virtualApplianceId = UriHelper.ExtractIdAsInt(virtualApplianceLink.Href);

            var virtualMachineId = virtualMachine.Id.GetValueOrDefault();
            Contract.Assert(0 < virtualMachineId);

            return DeleteVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId, force);
        }

        public override bool DeleteVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
        {
            return DeleteVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId, false);
        }

        public override bool DeleteVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force)
        {
            Dictionary<string, object> filter = null;
            if (force)
            {
                filter = new FilterBuilder().BuildFilterPart(FILTER_KEY_FORCE, FILTER_VALUE_FORCE).GetFilter();
            }

            var uriSuffix = 
                string.Format(AbiquoUriSuffixes.VIRTUALMACHINE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID, 
                virtualDataCenterId, virtualApplianceId, virtualMachineId);

            Invoke(HttpMethod.Delete, uriSuffix, filter, null);

            return true;
        }

        public override VmNetworkConfigurations GetNetworkConfigurationsForVm(VirtualMachine virtualMachine)
        {
            var virtualDataCenterLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDataCenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);

            var virtualApplianceLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALAPPLIANCE);
            var virtualApplianceId = UriHelper.ExtractIdAsInt(virtualApplianceLink.Href);

            var virtualMachineId = virtualMachine.Id.GetValueOrDefault();
            Contract.Assert(0 < virtualMachineId);

            return GetNetworkConfigurationsForVm(virtualDataCenterId, virtualApplianceId, virtualMachineId);
        }

        public override VmNetworkConfigurations GetNetworkConfigurationsForVm(int virtualDataCenterId, int virtualApplianceId,
            int virtualMachineId)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALMACHINENETWORKCONFIGURATIONS).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.NETWORK_CONFIGURATIONS_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPPLIANCE_ID_AND_VIRTUALMACHINE_ID,
                virtualDataCenterId, virtualApplianceId, virtualMachineId);

            return Invoke<VmNetworkConfigurations>(uriSuffix, headers);
        }

        public override VmNetworkConfiguration GetNetworkConfigurationForVm(VirtualMachine virtualMachine, int id)
        {
            var virtualDataCenterLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDataCenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);

            var virtualApplianceLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALAPPLIANCE);
            var virtualApplianceId = UriHelper.ExtractIdAsInt(virtualApplianceLink.Href);

            var virtualMachineId = virtualMachine.Id.GetValueOrDefault();
            Contract.Assert(0 < virtualMachineId);

            return GetNetworkConfigurationForVm(virtualDataCenterId, virtualApplianceId, virtualMachineId, id);
        }

        public override VmNetworkConfiguration GetNetworkConfigurationForVm(int virtualDataCenterId, int virtualApplianceId,
            int virtualMachineId, int id)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALMACHINENETWORKCONFIGURATION).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.NETWORK_CONFIGURATION_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPPLIANCE_ID_AND_VIRTUALMACHINE_ID_AND_NETWORK_CONFIGURATION_ID,
                virtualDataCenterId, virtualApplianceId, virtualMachineId, id);

            return Invoke<VmNetworkConfiguration>(uriSuffix, headers);
        }

        public override Nics GetNicsOfVirtualMachine(VirtualMachine virtualMachine)
        {
            var virtualDataCenterLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDataCenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);

            var virtualApplianceLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALAPPLIANCE);
            var virtualApplianceId = UriHelper.ExtractIdAsInt(virtualApplianceLink.Href);

            var virtualMachineId = virtualMachine.Id.GetValueOrDefault();
            Contract.Assert(0 < virtualMachineId);

            return GetNicsOfVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId);
        }

        public override Nics GetNicsOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_NICS).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.NICS_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPPLIANCE_ID_AND_VIRTUALMACHINE_ID,
                virtualDataCenterId, virtualApplianceId, virtualMachineId);

            return Invoke<Nics>(uriSuffix, headers);
        }

        public override Tasks GetAllTasksOfVirtualMachine(VirtualMachine virtualMachine)
        {
            var virtualDataCenterLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDataCenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);

            var virtualApplianceLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALAPPLIANCE);
            var virtualApplianceId = UriHelper.ExtractIdAsInt(virtualApplianceLink.Href);

            var virtualMachineId = virtualMachine.Id.GetValueOrDefault();
            Contract.Assert(0 < virtualMachineId);

            return GetAllTasksOfVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId);
        }

        public override Tasks GetAllTasksOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_TASKS).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.VIRTUALMACHINETASKS_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPPLIANCE_ID_AND_VIRTUALMACHINE_ID, virtualDataCenterId, virtualApplianceId, virtualMachineId);
            return Invoke<Tasks>(uriSuffix, headers);
        }

        public override Task GetTaskOfVirtualMachine(VirtualMachine virtualMachine, string taskId)
        {
            var virtualDataCenterLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDataCenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);

            var virtualApplianceLink = virtualMachine.GetLinkByRel(AbiquoRelations.VIRTUALAPPLIANCE);
            var virtualApplianceId = UriHelper.ExtractIdAsInt(virtualApplianceLink.Href);

            var virtualMachineId = virtualMachine.Id.GetValueOrDefault();
            Contract.Assert(0 < virtualMachineId);

            return GetTaskOfVirtualMachine(virtualDataCenterId, virtualApplianceId, virtualMachineId, taskId);
        }

        public override Task GetTaskOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, string taskId)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_TASK).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.VIRTUALMACHINETASK_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPPLIANCE_ID_AND_VIRTUALMACHINE_ID_AND_TASK_ID, virtualDataCenterId, virtualApplianceId, virtualMachineId, taskId);
            return Invoke<Task>(uriSuffix, headers);
        }

        #endregion VirtualMachines


        #region VirtualMachineTemplates

        public override VirtualMachineTemplates GetVirtualMachineTemplates(DataCenterRepository dataCenterRepository)
        {
            var dataCenterRepositoryLink = dataCenterRepository.GetLinkByRel(AbiquoRelations.EDIT);
            var dataCenterRepositoryId = UriHelper.ExtractIdAsInt(dataCenterRepositoryLink.Href);

            return GetVirtualMachineTemplates(TenantId, dataCenterRepositoryId);
        }

        public override VirtualMachineTemplates GetVirtualMachineTemplates(int enterpriseId, int dataCenterRepositoryId)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALMACHINETEMPLATES).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.VIRTUALMACHINETEMPLATES_BY_ENTERPISE_ID_AND_DATACENTERREPOSITORY_ID, enterpriseId, dataCenterRepositoryId);
            return Invoke<VirtualMachineTemplates>(uriSuffix, headers);
        }

        public override VirtualMachineTemplate GetVirtualMachineTemplate(DataCenterRepository dataCenterRepository, int id)
        {
            var dataCenterRepositoryLink = dataCenterRepository.GetLinkByRel(AbiquoRelations.EDIT);
            var dataCenterRepositoryId = UriHelper.ExtractIdAsInt(dataCenterRepositoryLink.Href);

            return GetVirtualMachineTemplate(TenantId, dataCenterRepositoryId, id);
        }

        public override VirtualMachineTemplate GetVirtualMachineTemplate(int enterpriseId, int dataCenterRepositoryId, int id)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALMACHINETEMPLATE).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.VIRTUALMACHINETEMPLATE_BY_ENTERPISE_ID_AND_DATACENTERREPOSITORY_ID_AND_VIRTUALMACHINETEMPLATE_ID, enterpriseId, dataCenterRepositoryId, id);
            return Invoke<VirtualMachineTemplate>(uriSuffix, headers);
        }

        #endregion VirtualMachineTemplates


        #region VirtualDataCenters

        public override VirtualDataCenters GetVirtualDataCenters()
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALDATACENTERS).GetHeaders();

            return Invoke<VirtualDataCenters>(AbiquoUriSuffixes.VIRTUALDATACENTERS, headers);
        }

        public override VirtualDataCenter GetVirtualDataCenter(int id)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALDATACENTER).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.VIRTUALDATACENTER_BY_ID, id);
            return Invoke<VirtualDataCenter>(uriSuffix, headers);
        }

        #endregion VirtualDataCenters


        #region VirtualAppliances

        public override VirtualAppliances GetVirtualAppliances(VirtualDataCenter virtualDataCenter)
        {
            return GetVirtualAppliances(virtualDataCenter.Id);
        }

        public override VirtualAppliances GetVirtualAppliances(int virtualDataCenterId)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALAPPLIANCES).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.VIRTUALAPPLIANCES_BY_VIRTUALDATACENTER_ID, virtualDataCenterId);
            return Invoke<VirtualAppliances>(uriSuffix, headers);
        }

        public override VirtualAppliance GetVirtualAppliance(VirtualDataCenter virtualDataCenter, int id)
        {
            return GetVirtualAppliance(virtualDataCenter.Id, id);
        }

        public override VirtualAppliance GetVirtualAppliance(int virtualDataCenterId, int id)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VIRTUALAPPLIANCE).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.VIRTUALAPPLIANCE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID, virtualDataCenterId, id);
            return Invoke<VirtualAppliance>(uriSuffix, headers);
        }

        #endregion VirtualAppliances


        #region DataCenterRepositories

        public override DataCenterRepositories GetDataCenterRepositoriesOfCurrentEnterprise()
        {
            return GetDataCenterRepositories(TenantId);
        }

        public override DataCenterRepositories GetDataCenterRepositories(Enterprise enterprise)
        {
            return GetDataCenterRepositories(enterprise.Id);
        }

        public override DataCenterRepositories GetDataCenterRepositories(int enterpriseId)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_DATACENTERREPOSITORIES).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.DATACENTERREPOSITORIES_BY_ENTERPRISE_ID, enterpriseId);
            return Invoke<DataCenterRepositories>(uriSuffix, headers);
        }

        public override DataCenterRepository GetDataCenterRepositoryOfCurrentEnterprise(int id)
        {
            return GetDataCenterRepository(TenantId, id);
        }

        public override DataCenterRepository GetDataCenterRepository(Enterprise enterprise, int id)
        {
            return GetDataCenterRepository(enterprise.Id, id);
        }

        public override DataCenterRepository GetDataCenterRepository(int enterpriseId, int id)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_DATACENTERREPOSITORY).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.DATACENTERREPOSITORIES_BY_ENTERPRISE_ID_AND_DATACENTERREPOSITORY_ID, enterpriseId, id);
            return Invoke<DataCenterRepository>(uriSuffix, headers);
        }

        #endregion DataCenterRepositories


        #region Tasks

        public override Task WaitForTaskCompletion(Task task, int taskPollingWaitTimeMilliseconds, int taskPollingTimeoutMilliseconds)
        {
            Logger.Current.TraceEvent(TraceEventType.Start, (int) Constants.EventId.WaitForTaskCompletion, Messages.AbiquoClientWaitForTaskStart,
                    task.TaskId, taskPollingWaitTimeMilliseconds, taskPollingTimeoutMilliseconds);

            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_TASK).GetHeaders();
            var taskSelfLink = task.GetLinkByRel(AbiquoRelations.SELF);
            var uriSuffix = taskSelfLink.GetUriSuffix();

            var timeLimit = DateTime.Now.AddMilliseconds(taskPollingTimeoutMilliseconds);
            var currentTaskPollingWaitTime = taskPollingWaitTimeMilliseconds;

            while (DateTime.Now < timeLimit)
            {
                var taskToWaitFor = Invoke<Task>(uriSuffix, headers);
                switch (taskToWaitFor.State)
                {
                    case TaskStateEnum.FINISHED_SUCCESSFULLY:
                    case TaskStateEnum.FINISHED_UNSUCCESSFULLY:
                    case TaskStateEnum.ABORTED:
                        Logger.Current.TraceEvent(TraceEventType.Information, (int) Constants.EventId.WaitForTaskCompletion, 
                            Messages.AbiquoClientWaitForTaskCompletionCompleted,
                            task.TaskId,
                            taskPollingWaitTimeMilliseconds, 
                            taskPollingTimeoutMilliseconds);

                        return taskToWaitFor;
                }

                Thread.Sleep(currentTaskPollingWaitTime);
                currentTaskPollingWaitTime = Convert.ToInt32(Math.Floor(currentTaskPollingWaitTime*1.5));
            }

            Logger.Current.TraceEvent(TraceEventType.Error, (int) Constants.EventId.WaitForTaskCompletion, 
                Messages.AbiquoClientWaitForTaskCompletionExceeded,
                task.TaskId,
                taskPollingTimeoutMilliseconds);

            throw new TimeoutException(string.Format(Messages.AbiquoClientWaitForTaskCompletionTimeout, task.TaskId));
        }

        #endregion Tasks


        #region Networks

        public override VlanNetworks GetPrivateNetworks(VirtualDataCenter virtualDataCenter)
        {
            return GetPrivateNetworks(virtualDataCenter.Id);
        }

        public override VlanNetworks GetPrivateNetworks(int virtualDataCenterId)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VLANS).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.PRIVATE_NETWORKS_BY_VIRTUALDATACENTER_ID, virtualDataCenterId);

            return Invoke<VlanNetworks>(uriSuffix, headers);
        }

        public override VlanNetwork GetPrivateNetwork(VirtualDataCenter virtualDataCenter, int id)
        {
            return GetPrivateNetwork(virtualDataCenter.Id, id);
        }

        public override VlanNetwork GetPrivateNetwork(int virtualDataCenterId, int id)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VLAN).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.PRIVATE_NETWORK_BY_VIRTUALDATACENTER_ID_AND_PRIVATE_NETWORK_ID, virtualDataCenterId, id);

            return Invoke<VlanNetwork>(uriSuffix, headers);
        }

        public override PrivateIps GetIpsOfPrivateNetwork(VlanNetwork vlan, bool free)
        {
            var virtualDataCenterLink = vlan.GetLinkByRel(AbiquoRelations.VIRTUALDATACENTER);
            var virtualDataCenterId = UriHelper.ExtractIdAsInt(virtualDataCenterLink.Href);            

            return GetIpsOfPrivateNetwork(virtualDataCenterId, vlan.Id, free);
        }

        public override PrivateIps GetIpsOfPrivateNetwork(int virtualDataCenterId, int privateNetworkId, bool free)
        {
            Dictionary<string, object> filter = null;
            if (free)
            {
                filter = new FilterBuilder().BuildFilterPart(FILTER_KEY_FREE, FILTER_VALUE_FREE).GetFilter();
            }

            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_PRIVATEIPS).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.IPS_OF_PRIVATE_NETWORK_BY_VIRTUALDATACENTER_ID_AND_PRIVATE_NETWORK_ID, virtualDataCenterId, privateNetworkId);

            return Invoke<PrivateIps>(uriSuffix, filter, headers);
        }

        public override VlanNetworks GetExternalNetworksOfCurrentEnterprise(int dataCenterLimitsId)
        {
            return GetExternalNetworks(TenantId, dataCenterLimitsId);
        }

        public override VlanNetworks GetExternalNetworks(int enterpriseId, int dataCenterLimitsId)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VLANS).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.EXTERNAL_NETWORKS_BY_ENTERPRISE_ID_AND_LIMIT_ID, enterpriseId, dataCenterLimitsId);

            return Invoke<VlanNetworks>(uriSuffix, headers);
        }

        public override VlanNetwork GetExternalNetworkOfCurrentEnterprise(int dataCenterLimitsId, int id)
        {
            return GetExternalNetwork(TenantId, dataCenterLimitsId, id);
        }

        public override VlanNetwork GetExternalNetwork(int enterpriseId, int dataCenterLimitsId, int id)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VLAN).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.EXTERNAL_NETWORK_BY_ENTERPRISE_ID_AND_LIMIT_ID_AND_EXTERNAL_NETWORK_ID, 
                enterpriseId, dataCenterLimitsId, id);

            return Invoke<VlanNetwork>(uriSuffix, headers);
        }

        public override ExternalIps GetIpsOfExternalNetworkOfCurrentEnterprise(VlanNetwork vlan, bool free)
        {
            var vlanEditLink = vlan.GetLinkByRel(AbiquoRelations.EDIT);

            var match = Regex.Match(vlanEditLink.Href, DATACENTERLIMITS_ID_PATTERN);
            Contract.Assert(match.Success);
            Contract.Assert(2 == match.Groups.Count);

            var limitsIdAsString = match.Groups[1].Value;

            var dataCenterLimitsId = int.Parse(limitsIdAsString);

            return GetIpsOfExternalNetworkOfCurrentEnterprise(dataCenterLimitsId, vlan.Id, free);
        }

        public override ExternalIps GetIpsOfExternalNetworkOfCurrentEnterprise(int dataCenterLimitsId, int externalNetworkId, bool free)
        {
            return GetIpsOfExternalNetwork(TenantId, dataCenterLimitsId,
                externalNetworkId, free);
        }

        public override ExternalIps GetIpsOfExternalNetwork(int enterpriseId, int dataCenterLimitsId, int externalNetworkId, bool free)
        {
            Dictionary<string, object> filter = null;
            if (free)
            {
                filter = new FilterBuilder().BuildFilterPart(FILTER_KEY_FREE, FILTER_VALUE_FREE).GetFilter();
            }

            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_EXTERNALIPS).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.IPS_OF_EXTERNAL_NETWORK_BY_ENTERPRISE_ID_AND_LIMIT_ID_AND_EXTERNAL_NETWORK_ID, enterpriseId, dataCenterLimitsId, externalNetworkId);

            return Invoke<ExternalIps>(uriSuffix, filter, headers);
        }

        public override VlanNetworks GetPublicNetworks(VirtualDataCenter virtualDataCenter)
        {
            return GetPublicNetworks(virtualDataCenter.Id);
        }

        public override VlanNetworks GetPublicNetworks(int virtualDataCenterId)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VLANS).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.PUBLIC_NETWORKS_BY_VIRTUALDATACENTER_ID, virtualDataCenterId);

            return Invoke<VlanNetworks>(uriSuffix, headers);
        }

        public override VlanNetwork GetPublicNetwork(VirtualDataCenter virtualDataCenter, int id)
        {
            return GetPublicNetwork(virtualDataCenter.Id, id);
        }

        public override VlanNetwork GetPublicNetwork(int virtualDataCenterId, int id)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_VLAN).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.PUBLIC_NETWORK_BY_VIRTUALDATACENTER_ID_AND_PUBLIC_NETWORK_ID, virtualDataCenterId, id);

            return Invoke<VlanNetwork>(uriSuffix, headers);
        }

        public override PublicIps GetPublicIpsToPurchaseOfPublicNetwork(VirtualDataCenter virtualDataCenter, VlanNetwork vlan)
        {
            return GetPublicIpsToPurchaseOfPublicNetwork(virtualDataCenter.Id, vlan.Id);
        }

        public override PublicIps GetPublicIpsToPurchaseOfPublicNetwork(int virtualDataCenterId, int vlanId)
        {
            var filter = new FilterBuilder().BuildFilterPart(FILTER_KEY_VLANID, vlanId).GetFilter();

            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_PUBLICIPS).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.PUBLIC_IPS_TO_PURCHASE_BY_VIRTUALDATACENTER_ID, virtualDataCenterId);

            return Invoke<PublicIps>(uriSuffix, filter, headers);
        }

        public override PublicIp PurchasePublicIp(VirtualDataCenter virtualDataCenter, PublicIp publicIp)
        {
            return PurchasePublicIp(virtualDataCenter.Id, publicIp.Id);
        }

        public override PublicIp PurchasePublicIp(int virtualDataCenterId, int publicIpid)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_PUBLICIP).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.PURCHASED_PUBLIC_IP_BY_VIRTUALDATACENTER_ID_AND_PUBLICIP_ID, virtualDataCenterId, publicIpid);

            return Invoke<PublicIp>(HttpMethod.Put, uriSuffix, null, headers, string.Empty);
        }

        public override PublicIp ReleasePublicIp(VirtualDataCenter virtualDataCenter, PublicIp publicIp)
        {
            return ReleasePublicIp(virtualDataCenter.Id, publicIp.Id);
        }

        public override PublicIp ReleasePublicIp(int virtualDataCenterId, int publicIpid)
        {
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_PUBLICIP).GetHeaders();

            var uriSuffix = string.Format(AbiquoUriSuffixes.PUBLIC_IP_TO_PURCHASE_BY_VIRTUALDATACENTER_ID_AND_PUBLICIP_ID, virtualDataCenterId, publicIpid);

            return Invoke<PublicIp>(HttpMethod.Put, uriSuffix, null, headers, string.Empty);
        }

        #endregion Networks
    }
}
