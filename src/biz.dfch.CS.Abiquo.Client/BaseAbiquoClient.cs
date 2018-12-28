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
using System.Net.Http.Headers;
using System.Text;
using biz.dfch.CS.Abiquo.Client.Authentication;
using biz.dfch.CS.Abiquo.Client.Communication;
using biz.dfch.CS.Abiquo.Client.General;
using biz.dfch.CS.Abiquo.Client.v1.Model;
using biz.dfch.CS.Commons;
using biz.dfch.CS.Commons.Diagnostics;
using biz.dfch.CS.Commons.Rest;
using Newtonsoft.Json;
using Logger = biz.dfch.CS.Abiquo.Client.General.Logger;

namespace biz.dfch.CS.Abiquo.Client
{
    [ContractClass(typeof(ContractClassForBaseAbiquoClient))]
    public abstract class BaseAbiquoClient
    {
        #region Awaiting Constants
        /// <summary>
        /// Default task polling wait time
        /// </summary>
        protected const int DEFAULT_TASK_POLLING_WAIT_TIME_MILLISECONDS = 5 * 1000;

        /// <summary>
        /// Default timoeut for task polling
        /// </summary>
        protected const int DEFAULT_TASK_POLLING_TIMEOUT_MILLISECONDS = 30 * 1000;

        #endregion Awaiting Constants


        #region Properties

        /// <summary>
        /// The Abiquo Api version the client is implemented for.
        /// Has to be set in the constructor of the derived class
        /// </summary>
        public string AbiquoApiVersion { get; protected set; }

        /// <summary>
        /// Indicates that the call to the /login endpoint succeeded
        /// with the provided authentication information.
        /// </summary>
        public bool IsLoggedIn { get; protected set; }

        /// <summary>
        /// Base URI of the Abiquo API
        /// </summary>
        public string AbiquoApiBaseUri { get; protected set; }

        /// <summary>
        /// Authentication information, that get injected through the Login method 
        /// </summary>
        public IAuthenticationInformation AuthenticationInformation { get; protected set; }

        /// <summary>
        /// Information about the currently logged in user, 
        /// that get injected through the login method (Contains the role of the user as link)
        /// </summary>
        public User CurrentUserInformation { get; protected set; }

        /// <summary>
        /// Returns the Id of the enterprise/tenant the current user is acting for
        /// </summary>
        public abstract int TenantId { get; }

        /// <summary>
        /// Polling wait time for task handling
        /// </summary>
        public int TaskPollingWaitTimeMilliseconds { get; set; }
        
        /// <summary>
        /// Timeout for task polling
        /// </summary>
        public int TaskPollingTimeoutMilliseconds { get; set; }

        /// <summary>
        /// Session token from Set-Cookie header
        /// Gets refreshed by Set-Cookie header of every response
        /// </summary>
        public string SessionToken { get; protected set; }

        #endregion Properties


        #region Contracts

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrWhiteSpace(AbiquoApiVersion));
            Contract.Invariant(0 < TaskPollingWaitTimeMilliseconds);
            Contract.Invariant(0 < TaskPollingTimeoutMilliseconds);
        }

        #endregion Contracts


        #region SerializationSettings

        public static void SetJsonSerializerMissingMemberHandling(MissingMemberHandling missingMemberHandling)
        {
            AbiquoBaseDto.SetJsonSerializerMissingMemberHandling(missingMemberHandling);
        }

        #endregion SerializationSettings


        #region Login

        public abstract bool Login(string abiquoApiBaseUri, IAuthenticationInformation authenticationInformation);

        public void Logout()
        {
            Logger.Current.TraceEvent(TraceEventType.Start, (int) Constants.EventId.Logout, Method.GetName());

            IsLoggedIn = false;
            AbiquoApiBaseUri = null;
            AuthenticationInformation = null;
            CurrentUserInformation = null;
            SessionToken = null;

            Logger.Current.TraceEvent(TraceEventType.Stop, (int) Constants.EventId.LogoutSucceeded, "{0} SUCCEEDED", Method.GetName());
        }

        #endregion Login


        #region ExecuteRequest

        internal string ExecuteRequest(string uriSuffix)
        {
            return ExecuteRequest(HttpMethod.Get, uriSuffix, null, null);
        }

        internal string ExecuteRequest(HttpMethod httpMethod, string uriSuffix, IDictionary<string, string> headers, string body)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(uriSuffix));
            Contract.Requires(!string.IsNullOrWhiteSpace(AbiquoApiBaseUri));
            Contract.Requires(null != AuthenticationInformation);

            var requestUri = UriHelper.ConcatUri(AbiquoApiBaseUri, uriSuffix);

            // log headers
            if (Logger.Current.Switch.ShouldTrace(TraceEventType.Start))
            {
                var headersString = new StringBuilder();
                headersString.AppendLine();
                var headersKeyCount = 0;
                if (null != headers && 0 < headers.Count)
                {
                    foreach (var header in headers)
                    {
                        headersString.AppendFormat("{0}: {1}", header.Key, header.Value);
                        headersString.AppendLine();
                    }
                    headersKeyCount = headers.Count;
                }
                var bodyLength = null != body ? body.Length : 0;
                Logger.Current.TraceEvent(TraceEventType.Start, (int) Constants.EventId.ExecuteRequest, "Executing {0} {1} ...\r\nHeaders [{2}]:{3}Body [{4}]: {5}", httpMethod, requestUri, headersKeyCount, headersString, bodyLength, body);
            }

            // add session cookie to request headers if present, otherwise add authorization header
            var requestHeaders = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(SessionToken))
            {
                requestHeaders = new Dictionary<string, string>(AuthenticationInformation.GetAuthorizationHeaders());
            }
            else
            {
                requestHeaders.Add(Constants.Authentication.COOKIE_HEADER_KEY, SessionToken);
            }

            if (null != headers)
            {
                headers.ToList().ForEach(header => requestHeaders[header.Key] = header.Value);
            }

            var restCallExecutor = new RestCallExecutor();
            var result = restCallExecutor.Invoke(httpMethod, requestUri, requestHeaders, body);

            // refresh session token with value from Set-Cookie response header
            SessionToken = GetSessionTokenFromSetCookieHeader(restCallExecutor.GetResponseHeaders());

            Logger.Current.TraceEvent(TraceEventType.Stop, (int) Constants.EventId.ExecuteRequest, "Executing {0} {1} COMPLETED.", httpMethod, requestUri);

            return result;
        }

        private string GetSessionTokenFromSetCookieHeader(HttpResponseHeaders responseHeaders)
        {
            Contract.Requires(null != responseHeaders);

            string sessionToken = null;
            IEnumerable<string> setCookieHeaderValues;
            if (responseHeaders.TryGetValues(AbiquoHeaderKeys.SET_COOKIE_HEADER_KEY, out setCookieHeaderValues))
            {
                var authCookie = setCookieHeaderValues.FirstOrDefault(cookie => cookie.StartsWith("auth="));

                if (null != authCookie)
                {
                    sessionToken = authCookie.Substring(0, authCookie.IndexOf(';'));
                }
                else
                {
                    sessionToken = SessionToken;
                }
            }

            return sessionToken;
        }

        #endregion ExecuteRequest


        #region Invoke

        public T Invoke<T>(string uriSuffix, IDictionary<string, string> headers) where T : AbiquoBaseDto
        {
            return Invoke<T>(HttpMethod.Get, uriSuffix, null, headers, default(string));
        }

        public T Invoke<T>(string uriSuffix, IDictionary<string, object> filter, IDictionary<string, string> headers)
            where T : AbiquoBaseDto
        {
            return Invoke<T>(HttpMethod.Get, uriSuffix, filter, headers, default(string));
        }

        public T Invoke<T>(HttpMethod httpMethod, string uriSuffix, IDictionary<string, object> filter, IDictionary<string, string> headers)
            where T : AbiquoBaseDto
        {
            return Invoke<T>(httpMethod, uriSuffix, filter, headers, default(string));
        }

        public T Invoke<T>(HttpMethod httpMethod, string uriSuffix, IDictionary<string, object> filter, IDictionary<string, string> headers, string body) 
            where T : AbiquoBaseDto
        {
            var stringResponse = Invoke(httpMethod, uriSuffix, filter, headers, body);
            return AbiquoBaseDto.DeserializeObject<T>(stringResponse);
        }

        public T Invoke<T>(HttpMethod httpMethod, string uriSuffix, IDictionary<string, object> filter, IDictionary<string, string> headers, AbiquoBaseDto body)
            where T : AbiquoBaseDto
        {
            var stringResponse = Invoke(httpMethod, uriSuffix, filter, headers, body);
            return AbiquoBaseDto.DeserializeObject<T>(stringResponse);
        }

        public DictionaryParameters Invoke(Uri absoluteUri)
        {
            Contract.Requires(null != absoluteUri);
            Contract.Requires(absoluteUri.IsAbsoluteUri);

            var response = Invoke(UriHelper.ExtractRelativeUri(AbiquoApiBaseUri, absoluteUri.AbsoluteUri));

            var result = new DictionaryParameters(response);
            return result;
        }

        public string Invoke(string uriSuffix)
        {
            return Invoke(HttpMethod.Get, uriSuffix, null, null, default(string));
        }

        public string Invoke(string uriSuffix, IDictionary<string, string> headers)
        {
            return Invoke(HttpMethod.Get, uriSuffix, null, headers, default(string));
        }

        public string Invoke(string uriSuffix, IDictionary<string, object> filter, IDictionary<string, string> headers)
        {
            return Invoke(HttpMethod.Get, uriSuffix, filter, headers, default(string));
        }

        public string Invoke(HttpMethod httpMethod, string uriSuffix, IDictionary<string, string> headers)
        {
            Contract.Requires(httpMethod != HttpMethod.Put);

            return Invoke(httpMethod, uriSuffix, null, headers, default(string));
        }

        public string Invoke(HttpMethod httpMethod, string uriSuffix, IDictionary<string, string> headers, AbiquoBaseDto body)
        {
            Contract.Requires(null != body);

            return Invoke(httpMethod, uriSuffix, null, headers, body.SerializeObject());
        }

        public string Invoke(HttpMethod httpMethod, string uriSuffix, IDictionary<string, object> filter, IDictionary<string, string> headers)
        {
            return Invoke(httpMethod, uriSuffix, filter, headers, default(string));
        }

        public string Invoke(HttpMethod httpMethod, string uriSuffix, IDictionary<string, object> filter, IDictionary<string, string> headers, AbiquoBaseDto body)
        {
            Contract.Requires(null != body);

            return Invoke(httpMethod, uriSuffix, filter, headers, body.SerializeObject());
        }

        public string Invoke(HttpMethod httpMethod, string uriSuffix, IDictionary<string, object> filter, IDictionary<string, string> headers, string body)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(uriSuffix));
            Contract.Requires(Uri.IsWellFormedUriString(uriSuffix, UriKind.Relative), "Invalid relative URI");
            Contract.Requires(IsLoggedIn, "Not logged in, call method login first");

            if (null != filter)
            {
                var filterString = UriHelper.CreateFilterString(filter);
                uriSuffix = string.Format("{0}?{1}", uriSuffix, filterString);
            }

            Logger.Current.TraceEvent(TraceEventType.Start, (int) Constants.EventId.Invoke, Messages.BaseAbiquoClientInvokeStart, httpMethod, uriSuffix);

            var response = ExecuteRequest(httpMethod, uriSuffix, headers, body);

            Logger.Current.TraceEvent(TraceEventType.Verbose, (int) Constants.EventId.InvokeCompleted, Messages.BaseAbiquoClientInvokeCompleted, httpMethod, uriSuffix);

            return response;
        }

        #endregion Invoke


        #region Invoke Link(s)

        /// <summary>
        /// Retrieve an entity of the specified type by invoking a GET request on the href of the provided link
        /// </summary>
        /// <typeparam name="T">Type of the entity to be returned</typeparam>
        /// <param name="link">Abiquo link entity</param>
        /// <returns>Abiquo entity of specified type</returns>
        public abstract T InvokeLink<T>(Link link) where T : AbiquoBaseDto;

        /// <summary>
        /// Retrieve an entity of type specified in link by invoking a GET request on the href of the provided link
        /// </summary>
        /// <param name="link">Abiquo link entity</param>
        /// <returns>Abiquo entity</returns>
        public abstract AbiquoBaseDto InvokeLink(Link link);

        public List<DictionaryParameters> InvokeLinksByType(ICollection<Link> links, string type)
        {
            Contract.Requires(null != links);
            Contract.Requires(!string.IsNullOrWhiteSpace(type));
            Contract.Ensures(null != Contract.Result<ICollection<DictionaryParameters>>());

            var selectedLinks = links.Where(e => type.Equals(e.Type));
            Contract.Assert(null != selectedLinks, string.Format(Messages.BaseAbiquoClientInvokeLinksByType, type));

            var dictionaryParametersList = new List<DictionaryParameters>();
            foreach (var selectedLink in selectedLinks)
            {
                var response = GetDictionaryParametersFromLink(selectedLink);
                var result = new DictionaryParameters(response);
                dictionaryParametersList.Add(result);
            }

            return dictionaryParametersList;
        }

        public DictionaryParameters InvokeLinkByRel(ICollection<Link> links, string rel)
        {
            Contract.Requires(null != links);
            Contract.Requires(!string.IsNullOrWhiteSpace(rel));
            Contract.Ensures(null != Contract.Result<DictionaryParameters>());

            var link = links.FirstOrDefault(e => rel.Equals(e.Rel));
            Contract.Assert(null != link, string.Format(Messages.BaseAbiquoClientInvokeRel, rel));

            var response = GetDictionaryParametersFromLink(link);

            var result = new DictionaryParameters(response);
            return result;
        }

        public DictionaryParameters GetDictionaryParametersFromLink(Link link)
        {
            Contract.Requires(null != link);
            Contract.Ensures(null != Contract.Result<DictionaryParameters>());

            var response = Invoke(UriHelper.ExtractRelativeUri(AbiquoApiBaseUri, link.Href));

            var result = new DictionaryParameters(response);
            return result;
        }

        #endregion Invoke Link(s)


        #region Enterprises

        /// <summary>
        /// Retrieve the list of enterprises/tenants
        /// </summary>
        /// <returns>Enterprises/tenants</returns>
        public abstract Enterprises GetEnterprises();
        
        /// <summary>
        /// Retrieve the enterprise/tenant specified in the authentication information
        /// </summary>
        /// <returns>Enterprise of the specified in the authentication information</returns>
        public abstract Enterprise GetCurrentEnterprise();

        /// <summary>
        /// Retrieve enterprise/tenant by id
        /// </summary>
        /// <param name="id">Id of the enterprise/tenant</param>
        /// <returns>Enterprise</returns>
        public abstract Enterprise GetEnterprise(int id);

        #endregion Enterprises


        #region Users

        /// <summary>
        /// Retrieve users with roles of the enterprise/tenant specified in the authentication information
        /// </summary>
        /// <returns>UsersWithRoles</returns>
        public abstract UsersWithRoles GetUsersWithRolesOfCurrentEnterprise();

        /// <summary>
        /// Retrieve users with roles of a specific enterprise/tenant
        /// </summary>
        /// <param name="enterprise">Enterprise/Tenant</param>
        /// <returns>UsersWithRoles</returns>
        public abstract UsersWithRoles GetUsersWithRoles(Enterprise enterprise);

        /// <summary>
        /// Retrieve users with roles of a specific enterprise/tenant
        /// </summary>
        /// <param name="enterpriseId">Id of the enterprise/tenant</param>
        /// <returns>UsersWithRoles</returns>
        public abstract UsersWithRoles GetUsersWithRoles(int enterpriseId);

        /// <summary>
        /// Retrieve a specific user by id of the enterprise/tenant specified in the authentication information
        /// </summary>
        /// <param name="id">Id of the user</param>
        /// <returns>User</returns>
        public abstract User GetUserOfCurrentEnterprise(int id);

        /// <summary>
        /// Retrieve a specific user by a specific enterprise/tenant
        /// </summary>
        /// <param name="enterprise">Enterprise/Tenant</param>
        /// <param name="id">Id of the user</param>
        /// <returns>User</returns>
        public abstract User GetUser(Enterprise enterprise, int id);

        /// <summary>
        /// Retrieve a specific user by id of a specific enterprise/tenant
        /// </summary>
        /// <param name="enterpriseId">Id of the enterprise/tenant</param>
        /// <param name="id">Id of the user</param>
        /// <returns>User</returns>
        public abstract User GetUser(int enterpriseId, int id);

        /// <summary>
        /// Get information about the currently authenticated user in context of current enterprise
        /// </summary>
        /// <returns>Information about the current user</returns>
        public abstract User GetUserInformation();

        /// <summary>
        /// Get information about a specific user in context of current enterprise
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Information about the specified user in context of current enterprise</returns>
        public abstract User GetUserInformation(string username);

        /// <summary>
        /// Get information about specific user in context of a specific enterprise
        /// </summary>
        /// <param name="enterpriseId">Id of the enterprise/tenant</param>
        /// <param name="username">identifier of the user</param>
        /// <returns>Information about the specified user in context of specified enterprise</returns>
        public abstract User GetUserInformation(int enterpriseId, string username);

        /// <summary>
        /// Switch to the specified enterprise/tenant
        /// 
        /// This functionality is only available to the 
        /// cloud administrator and other users with the privileges
        /// to "List all enterprises within scope" and "Allow user to switch enterprise"
        /// </summary>
        /// <param name="enterprise"></param>
        public abstract void SwitchEnterprise(Enterprise enterprise);

        /// <summary>
        /// Switch to the specified enterprise/tenant
        /// 
        /// This functionality is only available to the 
        /// cloud administrator and other users with the privileges
        /// to "List all enterprises within scope" and "Allow user to switch enterprise"
        /// </summary>
        /// <param name="id"></param>
        public abstract void SwitchEnterprise(int id);


        #endregion Users


        #region Roles

        /// <summary>
        /// Retrieve all roles
        /// </summary>
        /// <returns>Colleciton of Roles</returns>
        public abstract Roles GetRoles();

        /// <summary>
        /// Retrieve a specific role by id
        /// </summary>
        /// <param name="id">Id of the role</param>
        /// <returns>Role</returns>
        public abstract Role GetRole(int id);

        #endregion Roles


        #region DataCentersLimits

        /// <summary>
        /// Retrieve datacenters limits of the enterprise/tenant specified in the authentication information
        /// </summary>
        /// <returns>DataCentersLimits</returns>
        public abstract DataCentersLimits GetDataCentersLimitsOfCurrentEnterprise();

        /// <summary>
        /// Retrieve datacenters limits of a specific enterprise/tenant
        /// </summary>
        /// <param name="enterprise">Enterprise/Tenant</param>
        /// <returns>DataCentersLimits</returns>
        public abstract DataCentersLimits GetDataCentersLimits(Enterprise enterprise);

        /// <summary>
        /// Retrieve datacenters limits of a specific enterprise/tenant
        /// </summary>
        /// <param name="enterpriseId">Id of the enterprise/tenant</param>
        /// <returns>DataCentersLimits</returns>
        public abstract DataCentersLimits GetDataCentersLimits(int enterpriseId);

        /// <summary>
        /// Retrieve a specific datacenter limits by id of the enterprise/tenant specified in the authentication information
        /// </summary>
        /// <param name="id">Id of the datacenter limits</param>
        /// <returns>DataCentersLimits</returns>
        public abstract DataCenterLimits GetDataCenterLimitsOfCurrentEnterprise(int id);

        /// <summary>
        /// Retrieve a specific datacenter limits by a specific enterprise/tenant
        /// </summary>
        /// <param name="enterprise">Enterprise/Tenant</param>
        /// <param name="id">Id of the datacenter limits</param>
        /// <returns>DataCenterLimits</returns>
        public abstract DataCenterLimits GetDataCenterLimits(Enterprise enterprise, int id);

        /// <summary>
        /// Retrieve a specific datacenter limits by id of a specific enterprise/tenant
        /// </summary>
        /// <param name="enterpriseId">Id of the enterprise/tenant</param>
        /// <param name="id">Id of the datacenter limits</param>
        /// <returns>DataCenterLimits</returns>
        public abstract DataCenterLimits GetDataCenterLimits(int enterpriseId, int id);

        #endregion DataCentersLimits


        #region VirtualMachines

        /// <summary>
        /// Retrieve all virtual machines across all virtual datacenters the current user has access to
        /// </summary>
        /// <returns>VirtualMachines</returns>
        public abstract VirtualMachines GetAllVirtualMachines();

        /// <summary>
        /// Retrieve all virtual machines of a specific virtual appliance
        /// </summary>
        /// <param name="virtualAppliance">Virtual appliance</param>
        /// <returns>VirtualMachines</returns>
        public abstract VirtualMachines GetVirtualMachines(VirtualAppliance virtualAppliance);

        /// <summary>
        /// Retrieve all virtual machines of a specific virtual appliance of a specific virtual datacenter
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance</param>
        /// <returns>VirtualMachines</returns>
        public abstract VirtualMachines GetVirtualMachines(int virtualDataCenterId, int virtualApplianceId);

        /// <summary>
        /// Retrieve a virtual machine by id of a specific virtual appliance
        /// </summary>
        /// <param name="virtualAppliance">Virtual appliance</param>
        /// <param name="id">Id of the virtual machine</param>
        /// <returns>VirtualMachine</returns>
        public abstract VirtualMachine GetVirtualMachine(VirtualAppliance virtualAppliance, int id);

        /// <summary>
        /// Retrieve a virtual machine by id of a specific virtual appliance of a specific virtual datacenter
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance</param>
        /// <param name="id">Id of the virtual machine</param>
        /// <returns>VirtualMachine</returns>
        public abstract VirtualMachine GetVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int id);

        /// <summary>
        /// Create a virtual machine based on a virtual machine template
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance to create the virtual machine in</param>
        /// <param name="enterpriseId">Id of the enterprise/tenant the template belongs to</param>
        /// <param name="dataCenterRepositoryId">Id of the datacenter repository the template belongs to</param>
        /// <param name="virtualMachineTemplateId">Id of the virtual machine template</param>
        /// <returns>VirtualMachine</returns>
        public abstract VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int enterpriseId, int dataCenterRepositoryId, int virtualMachineTemplateId);

        /// <summary>
        /// Create a virtual machine based on a virtual machine template
        /// </summary>
        /// <param name="virtualAppliance">Virtual appliance to create the virtual machine in</param>
        /// <param name="virtualMachineTemplate">Virtual machine template</param>
        /// <returns>VirtualMachine</returns>
        public abstract VirtualMachine CreateVirtualMachine(VirtualAppliance virtualAppliance, VirtualMachineTemplate virtualMachineTemplate);

        /// <summary>
        /// Create a virtual machine based on a virtual machine template
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance to create the virtual machine in</param>
        /// <param name="virtualMachineTemplateHref">Href of the virtual machine template</param>
        /// <returns>VirtualMachine</returns>
        public abstract VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, string virtualMachineTemplateHref);

        /// <summary>
        /// Create a virtual machine based on a virtual machine template and custom configuration
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance to create the virtual machine in</param>
        /// <param name="enterpriseId">Id of the enterprise/tenant the template belongs to</param>
        /// <param name="dataCenterRepositoryId">Id of the datacenter repository the template belongs to</param>
        /// <param name="virtualMachineTemplateId">Id of the virtual machine template</param>
        /// /// <param name="virtualMachine">Virtual machine configuration</param>
        /// <returns>VirtualMachine</returns>
        public abstract VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int enterpriseId, int dataCenterRepositoryId, int virtualMachineTemplateId, VirtualMachineBase virtualMachine);

        /// <summary>
        /// Create a virtual machine based on a virtual machine template and custom configuration
        /// </summary>
        /// <param name="virtualAppliance">Virtual appliance</param>
        /// <param name="virtualMachineTemplate">Virtual machine template</param>
        /// <param name="virtualMachine">Virtual machine configuration</param>
        /// <returns>VirtualMachine</returns>
        public abstract VirtualMachine CreateVirtualMachine(VirtualAppliance virtualAppliance, VirtualMachineTemplate virtualMachineTemplate, VirtualMachine virtualMachine);

        /// <summary>
        /// Create a virtual machine based on a virtual machine template and custom configuration
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance to create the virtual machine in</param>
        /// <param name="virtualMachineTemplateHref">Href of the virtual machine template the template belongs to</param>
        /// <param name="virtualMachine">Virtual machine configuration</param>
        /// <returns>VirtualMachine</returns>
        public abstract VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, string virtualMachineTemplateHref, VirtualMachineBase virtualMachine);

        /// <summary>
        /// Initiates deplyoment of a specific virtual machine
        /// </summary>
        /// <param name="virtualMachine">Virtual machine</param>
        /// <param name="force">If true, soft limits of virtual datacenters could be surpassed</param>
        /// <returns>Task containing information about the status of the deployment</returns>
        public abstract Task DeployVirtualMachine(VirtualMachine virtualMachine, bool force);

        /// <summary>
        /// Initiates deplyoment of a specific virtual machine
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine gets deployed in</param>
        /// <param name="virtualMachineId">Id of the virtual machine</param>
        /// <param name="force">If true, soft limits of virtual datacenters could be surpassed</param>
        /// <returns>Task containing information about the status of the deployment</returns>
        public abstract Task DeployVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force);

        /// <summary>
        /// Deployment of a specific virtual machine
        /// </summary>
        /// <param name="virtualMachine">VirtualMachine to deploy</param>
        /// <param name="force">If true, soft limits of virtual datacenters could be surpassed</param>
        /// <param name="waitForCompletion">Set to true for waiting until task got completed</param>
        /// <returns>Task containing information about the status of the deployment</returns>
        public abstract Task DeployVirtualMachine(VirtualMachine virtualMachine, bool force, bool waitForCompletion);

        /// <summary>
        /// Deployment of a specific virtual machine
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine gets deployed in</param>
        /// <param name="virtualMachineId">Id of the virtual machine</param>
        /// <param name="waitForCompletion">Set to true for waiting until task got completed</param>
        /// <param name="force">If true, soft limits of virtual datacenters could be surpassed</param>
        /// <returns>Task containing information about the status of the deployment</returns>
        public abstract Task DeployVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force, bool waitForCompletion);

        /// <summary>
        /// Initiates update of a specific virtual machine
        /// </summary>
        /// <param name="virtualMachine">Virtual machine with updated configuration</param>
        /// <param name="force">If true, update is forced</param>
        /// <returns>Task containing information about the status of the update</returns>
        public abstract Task UpdateVirtualMachine(VirtualMachine virtualMachine, bool force);

        /// <summary>
        /// Initiates update of a specific virtual machine
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine belongs to</param>
        /// <param name="virtualMachineId">Id of the virtual machine</param>
        /// <param name="virtualMachine">Virtual machine configuration</param>
        /// <param name="force">If true, update is forced</param>
        /// <returns>Task containing information about the status of the update</returns>
        public abstract Task UpdateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, VirtualMachine virtualMachine, bool force);

        /// <summary>
        /// Update a specific virtual machine
        /// </summary>
        /// <param name="virtualMachine">Virtual machine with updated configuration</param>
        /// <param name="force">If true, update is forced</param>
        /// <param name="waitForCompletion">Set to true for waiting until task got completed</param>
        /// <returns>Task containing information about the status of the update</returns>
        public abstract Task UpdateVirtualMachine(VirtualMachine virtualMachine, bool force, bool waitForCompletion);

        /// <summary>
        /// Update a specific virtual machine
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine belongs to</param>
        /// <param name="virtualMachineId">Id of the virtual machine</param>
        /// <param name="virtualMachine">Virtual machine configuration</param>
        /// <param name="force">If true, update is forced</param>
        /// <param name="waitForCompletion">Set to true for waiting until task got completed</param>
        /// <returns>Task containing information about the status of the update</returns>
        public abstract Task UpdateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, VirtualMachine virtualMachine, bool force, bool waitForCompletion);

        /// <summary>
        /// Initiates state change of a specific virtual machine
        /// </summary>
        /// <param name="virtualMachine">Target virtual machine</param>
        /// <param name="state">Target state</param>
        /// <returns>Task containing information about the status of the state change</returns>
        public abstract Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineStateEnum state);

        /// <summary>
        /// Initiates state change of a specific virtual machine
        /// </summary>
        /// <param name="virtualMachine">Target virtual machine</param>
        /// <param name="state">Target state</param>
        /// <returns>Task containing information about the status of the state change</returns>
        public abstract Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineState state);

        /// <summary>
        /// Initiates state change of a specific virtual machine
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine belongs to</param>
        /// <param name="virtualMachineId">Id of the virtual machine</param>
        /// <param name="state">Target state</param>
        /// <returns>Task containing information about the status of the state change</returns>
        public abstract Task ChangeStateOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, VirtualMachineState state);

        /// <summary>
        /// Initiates state change of a specific virtual machine
        /// </summary>
        /// <param name="virtualMachine">Target virtual machine</param>
        /// <param name="state">Target state</param>
        /// <param name="waitForCompletion">Set to true for waiting until task got completed</param>
        /// <returns>Task containing information about the status of the state change</returns>
        public abstract Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineStateEnum state, bool waitForCompletion);

        /// <summary>
        /// Initiates state change of a specific virtual machine
        /// </summary>
        /// <param name="virtualMachine">Target virtual machine</param>
        /// <param name="state">Target state</param>
        /// <param name="waitForCompletion">Set to true for waiting until task got completed</param>
        /// <returns>Task containing information about the status of the state change</returns>
        public abstract Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineState state, bool waitForCompletion);
        
        /// <summary>
        /// Changes state of a specific virtual machine
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine belongs to</param>
        /// <param name="virtualMachineId">Id of the virtual machine</param>
        /// <param name="state">Target state</param>
        /// <param name="waitForCompletion">Set to true for waiting until task got completed</param>
        /// <returns>Task containing information about the status of the state change</returns>
        public abstract Task ChangeStateOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, VirtualMachineState state, bool waitForCompletion);

        /// <summary>
        /// Protects a specific virtual machine from any operation
        /// </summary>
        /// <param name="virtualMachine">Target virtual machine</param>
        public abstract void ProtectVirtualMachine(VirtualMachine virtualMachine);
        
        /// <summary>
        /// Protects a specific virtual machine from any operation
        /// </summary>
        /// <param name="virtualMachine">Target virtual machine</param>
        /// <param name="protectionCause">Cause for protection</param>
        public abstract void ProtectVirtualMachine(VirtualMachine virtualMachine, string protectionCause);

        /// <summary>
        /// Protects a specific virtual machine from any operation
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine belongs to</param>
        /// <param name="virtualMachineId">Id of the virtual machine</param>
        /// <param name="protectionCause">Cause for protection</param>
        public abstract void ProtectVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, string protectionCause);

        /// <summary>
        /// Unprotects a specific virtual machine
        /// </summary>
        /// <param name="virtualMachine">Target virtual machine</param>
        public abstract void UnprotectVirtualMachine(VirtualMachine virtualMachine);

        /// <summary>
        /// Unprotects a specific virtual machine
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine belongs to</param>
        /// <param name="virtualMachineId">Id of the virtual machine</param>
        public abstract void UnprotectVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId);

        /// <summary>
        /// Delete a virtual machine
        /// </summary>
        /// <param name="virtualMachine">Virtual machine to delete</param>
        /// <returns>True, if the virtual machine was deleted successfully</returns>
        public abstract bool DeleteVirtualMachine(VirtualMachine virtualMachine);

        /// <summary>
        /// Delete a virtual machine by Id
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine belongs to</param>
        /// <param name="virtualMachineId">Id of the virtual machine</param>
        /// <returns>True, if the virtual machine was deleted successfully</returns>
        public abstract bool DeleteVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId);

        /// <summary>
        /// Delete a virtual machine 
        /// </summary>
        /// <param name="virtualMachine">Virtual machine to delete</param>
        /// <param name="force">Indicates if deletion has to be forced</param>
        /// <returns>True, if the virtual machine was deleted successfully</returns>
        public abstract bool DeleteVirtualMachine(VirtualMachine virtualMachine, bool force);

        /// <summary>
        /// Delete a virtual machine by Id
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine belongs to</param>
        /// <param name="virtualMachineId">Id of the virtual machine</param>
        /// <param name="force">Indicates if deletion has to be forced</param>
        /// <returns>True, if the virtual machine was deleted successfully</returns>
        public abstract bool DeleteVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force);

        /// <summary>
        /// Retrieve the possible virtual machine network configurations of a specific virtual machine
        /// </summary>
        /// <param name="virtualMachine">Virtual machine</param>
        /// <returns>VmNetworkConfigurations</returns>
        public abstract VmNetworkConfigurations GetNetworkConfigurationsForVm(VirtualMachine virtualMachine);
            
        /// <summary>
        /// Retrieve the possible virtual machine network configurations of a specific virtual machine
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine belongs to</param>
        /// <param name="virtualMachineId">Id of the virtual machine</param>
        /// <returns>VmNetworkConfigurations</returns>
        public abstract VmNetworkConfigurations GetNetworkConfigurationsForVm(int virtualDataCenterId,
            int virtualApplianceId, int virtualMachineId);

        /// <summary>
        /// Retrieve a possible virtual machine network configuration by Id of a specific virtual machine
        /// </summary>
        /// <param name="virtualMachine">Virtual machine</param>
        /// <param name="id">Id of the virtual machine network configuration</param>
        /// <returns>VmNetworkConfiguration</returns>
        public abstract VmNetworkConfiguration GetNetworkConfigurationForVm(VirtualMachine virtualMachine, int id);

        /// <summary>
        /// Retrieve a possible virtual machine network configuration by Id of a specific virtual machine
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine belongs to</param>
        /// <param name="virtualMachineId">Id of the virtual machine</param>
        /// <param name="id">Id of the virtual machine network configuration</param>
        /// <returns>VmNetworkConfiguration</returns>
        public abstract VmNetworkConfiguration GetNetworkConfigurationForVm(int virtualDataCenterId,
            int virtualApplianceId, int virtualMachineId, int id);

        /// <summary>
        /// Retrieve NICs attached to a specific virtual machine
        /// </summary>
        /// <param name="virtualMachine">Virtual machine</param>
        /// <returns>Nics</returns>
        public abstract Nics GetNicsOfVirtualMachine(VirtualMachine virtualMachine);

        /// <summary>
        /// Retrieve NICs attached to a specific virtual machine
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine belongs to</param>
        /// <param name="virtualMachineId">Id of the virtual machine</param>
        /// <returns>Nics</returns>
        public abstract Nics GetNicsOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId);

        /// <summary>
        ///Retrieve tasks of a specific virtual machine
        /// </summary>
        /// <param name="virtualMachine">Virtual machine</param>
        /// <returns>Tasks</returns>
        public abstract Tasks GetAllTasksOfVirtualMachine(VirtualMachine virtualMachine);

        /// <summary>
        /// Retrieve tasks of a specific virtual machine
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine belongs to</param>
        /// <param name="virtualMachineId">Id of the virtual mahcine</param>
        /// <returns>Tasks</returns>
        public abstract Tasks GetAllTasksOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId);

        /// <summary>
        ///  Retrieve a task by Id of a specific virtual machine
        /// </summary>
        /// <param name="virtualMachine">Virtual machine</param>
        /// <param name="taskId">Id of the task</param>
        /// <returns>Task</returns>
        public abstract Task GetTaskOfVirtualMachine(VirtualMachine virtualMachine, string taskId);

        /// <summary>
        /// Retrieve a task by Id of a specific virtual machine
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the virtual appliance belongs to</param>
        /// <param name="virtualApplianceId">Id of the virtual appliance the virtual machine belongs to</param>
        /// <param name="virtualMachineId">Id of the virtual mahcine</param>
        /// <param name="taskId">Id of the task</param>
        /// <returns>Task</returns>
        public abstract Task GetTaskOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, string taskId);

        #endregion VirtualMachines


        #region VirtualMachineTemplates

        /// <summary>
        /// Get all virtual machine templaes of a specific specific datacenter repository
        /// </summary>
        /// <param name="dataCenterRepository">Datacenter repository</param>
        /// <returns>VirtualMachineTemplates</returns>
        public abstract VirtualMachineTemplates GetVirtualMachineTemplates(DataCenterRepository dataCenterRepository);

        /// <summary>
        /// Get all virtual machine templaes of a specific specific datacenter repository of the current enterprise/tenant
        /// </summary>
        /// <param name="enterpriseId">Id of the entperise/tenant</param>
        /// <param name="dataCenterRepositoryId">Id of the datacenter repository</param>
        /// <returns>VirtualMachineTemplates</returns>
        public abstract VirtualMachineTemplates GetVirtualMachineTemplates(int enterpriseId, int dataCenterRepositoryId);

        /// <summary>
        /// Get a virtual machine template by id of a specific specific datacenter repository
        /// </summary>
        /// <param name="dataCenterRepository">Datacenter repository</param>
        /// <param name="id">Id of the virtual machine template</param>
        /// <returns>VirtualMachineTemplate</returns>
        public abstract VirtualMachineTemplate GetVirtualMachineTemplate(DataCenterRepository dataCenterRepository, int id);

        /// <summary>
        /// Get a virtual machine template by id of a specific specific datacenter repository of a specific enterprise/tenant
        /// </summary>
        /// <param name="enterpriseId">Id of the enterprise/tenant</param>
        /// <param name="dataCenterRepositoryId">Id of the datacenter repository</param>
        /// <param name="id">Id of the virtual machine template</param>
        /// <returns>VirtualMachineTemplate</returns>
        public abstract VirtualMachineTemplate GetVirtualMachineTemplate(int enterpriseId, int dataCenterRepositoryId, int id);

        #endregion VirtualMachineTemplates


        #region VirtualDataCenters

        /// <summary>
        /// Retrieve all available virtual datacenters
        /// </summary>
        /// <returns>VirtualDataCenters</returns>
        public abstract VirtualDataCenters GetVirtualDataCenters();

        /// <summary>
        /// Retrieve a specific virtual datacenter by id
        /// </summary>
        /// <param name="id">Id of the virtual datacenter</param>
        /// <returns>VirtualDataCenter</returns>
        public abstract VirtualDataCenter GetVirtualDataCenter(int id);

        #endregion VirtualDataCenters


        #region VirtualAppliances

        /// <summary>
        /// Retrieve all available virtual applicance of a specific virtual datacenter
        /// </summary>
        /// <param name="virtualDataCenter">Virtual datacenter</param>
        /// <returns>VirtualApplicances</returns>
        public abstract VirtualAppliances GetVirtualAppliances(VirtualDataCenter virtualDataCenter);

        /// <summary>
        /// Retrieve all available virtual applicance of a specific virtual datacenter
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter</param>
        /// <returns>VirtualApplicances</returns>
        public abstract VirtualAppliances GetVirtualAppliances(int virtualDataCenterId);

        /// <summary>
        /// Retrieve a specific virtual appliance by id of a specific virtual datacenter
        /// </summary>
        /// <param name="virtualDataCenter">Virtual datacenter</param>
        /// <param name="id">Id of the virtual appliance</param>
        /// <returns>VirtualAppliance</returns>
        public abstract VirtualAppliance GetVirtualAppliance(VirtualDataCenter virtualDataCenter, int id);

        /// <summary>
        /// Retrieve a specific virtual appliance by id of a specific virtual datacenter
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter</param>
        /// <param name="id">Id of the virtual appliance</param>
        /// <returns>VirtualAppliance</returns>
        public abstract VirtualAppliance GetVirtualAppliance(int virtualDataCenterId, int id);

        #endregion VirtualAppliances


        #region DataCenterRepositories
        
        /// <summary>
        /// Retrieve all datacenter repositories of the current enterprise/tenant
        /// </summary>
        /// <returns>DataCenterRepositories</returns>
        public abstract DataCenterRepositories GetDataCenterRepositoriesOfCurrentEnterprise();

        /// <summary>
        /// Retrieve all datacenter repositories of an enterprise/tenant
        /// </summary>
        /// <param name="enterprise">Entreprise/Tenant</param>
        /// <returns>DataCenterRepositories</returns>
        public abstract DataCenterRepositories GetDataCenterRepositories(Enterprise enterprise);

        /// <summary>
        /// Retrieve all datacenter repositories of an enterprise/tenant
        /// </summary>
        /// <param name="enterpriseId">Id of the enterprise/tenant</param>
        /// <returns>DataCenterRepositories</returns>
        public abstract DataCenterRepositories GetDataCenterRepositories(int enterpriseId);

        /// <summary>
        /// Retrieve a specific datacenter repository by id of the current enterprise/tenant
        /// </summary>
        /// <param name="id">Id of the datacenter repository</param>
        /// <returns>DataCenterRepository</returns>
        public abstract DataCenterRepository GetDataCenterRepositoryOfCurrentEnterprise(int id);

        /// <summary>
        /// Retrieve a specific datacenter repository by id of a specific enterprise/tenant
        /// </summary>
        /// <param name="enterprise">Entreprise/Tenant</param>
        /// <param name="id">Id of the datacenter repository</param>
        /// <returns>DataCenterRepository</returns>
        public abstract DataCenterRepository GetDataCenterRepository(Enterprise enterprise, int id);

        /// <summary>
        /// Retrieve a specific datacenter repository by id of a specific enterprise/tenant
        /// </summary>
        /// <param name="enterpriseId">Id of the enterprise/tenant</param>
        /// <param name="id">Id of the datacenter repository</param>
        /// <returns>DataCenterRepository</returns>
        public abstract DataCenterRepository GetDataCenterRepository(int enterpriseId, int id);

        #endregion DataCenterRepositories


        #region Tasks

        /// <summary>
        /// Wait for a task to complete
        /// </summary>
        /// <param name="task">Task object</param>
        /// <param name="taskPollingWaitTimeMilliseconds">Polling wait time in milliseconds</param>
        /// <param name="taskPollingTimeoutMilliseconds">timeout in milliseconds</param>
        /// <returns>Completed Task</returns>
        public abstract Task WaitForTaskCompletion(Task task, int taskPollingWaitTimeMilliseconds, int taskPollingTimeoutMilliseconds);

        #endregion Tasks


        #region Networks

        /// <summary>
        /// Retrieve all private networks of a specific virtual datacenter
        /// </summary>
        /// <param name="virtualDataCenter">Virtual datacenter</param>
        /// <returns>VlanNetworks</returns>
        public abstract VlanNetworks GetPrivateNetworks(VirtualDataCenter virtualDataCenter);

        /// <summary>
        /// Retrieve all private networks of a specific virtual datacenter
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the network belongs to</param>
        /// <returns>VlanNetworks</returns>
        public abstract VlanNetworks GetPrivateNetworks(int virtualDataCenterId);

        /// <summary>
        /// Retrieve a private network by Id of a specific virtual datacenter
        /// </summary>
        /// <param name="virtualDataCenter">Virtual datacenter</param>
        /// <param name="id">Id of the private network</param>
        /// <returns>VlanNetwork</returns>
        public abstract VlanNetwork GetPrivateNetwork(VirtualDataCenter virtualDataCenter, int id);

        /// <summary>
        /// Retrieve a private network by Id of a specific virtual datacenter
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the network belongs to</param>
        /// <param name="id">Id of the private network</param>
        /// <returns>VlanNetwork</returns>
        public abstract VlanNetwork GetPrivateNetwork(int virtualDataCenterId, int id);

        /// <summary>
        /// Retrieve IPs of a specific private network
        /// </summary>
        /// <param name="vlan">Vlan</param>
        /// <param name="free">If true, return only the available IPs not used by any virtual machine; if false, return all IPs</param>
        /// <returns>PrivateIps</returns>
        public abstract PrivateIps GetIpsOfPrivateNetwork(VlanNetwork vlan, bool free);

        /// <summary>
        /// Retrieve IPs of a specific private network
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the network belongs to</param>
        /// <param name="privateNetworkId">Id of the private network</param>
        /// <param name="free">If true, return only the available IPs not used by any virtual machine; if false, return all IPs</param>
        /// <returns>PrivateIps</returns>
        public abstract PrivateIps GetIpsOfPrivateNetwork(int virtualDataCenterId, int privateNetworkId, bool free);

        /// <summary>
        /// Retrieve all external networks available for a specific limit of the enterprise/tenant specified in the authentication information
        /// </summary>
        /// <param name="dataCenterLimitsId">Id of the datacenter limits</param>
        /// <returns>VlanNetworks</returns>
        public abstract VlanNetworks GetExternalNetworksOfCurrentEnterprise(int dataCenterLimitsId);

        /// <summary>
        /// Retrieve all external networks available for a specific limit of a specific enterprise/tenant
        /// </summary>
        /// <param name="enterpriseId">Id of the enterprise/tenant</param>
        /// <param name="dataCenterLimitsId">Id of the datacenter limits</param>
        /// <returns>VlanNetworks</returns>
        public abstract VlanNetworks GetExternalNetworks(int enterpriseId, int dataCenterLimitsId);

        /// <summary>
        /// Retrieve an external network by Id of a specific limit of the enterprise/tenant specified in the authentication information
        /// </summary>
        /// <param name="dataCenterLimitsId">Id of the datacenter limits</param>
        /// <param name="id">Id of the external network</param>
        /// <returns>VlanNetwork</returns>
        public abstract VlanNetwork GetExternalNetworkOfCurrentEnterprise(int dataCenterLimitsId, int id);

        /// <summary>
        /// Retrieve an external network by Id of a specific limit of a specific enterprise/tenant
        /// </summary>
        /// <param name="enterpriseId">Id of the enterprise/tenant</param>
        /// <param name="dataCenterLimitsId">Id of the datacenter limits</param>
        /// <param name="id">Id of the external network</param>
        /// <returns>VlanNetwork</returns>
        public abstract VlanNetwork GetExternalNetwork(int enterpriseId, int dataCenterLimitsId, int id);

        /// <summary>
        ///  Retrieve IPs of a specific external network of the enterprise/tenant specified in the authentication information
        /// </summary>
        /// <param name="vlan">Vlan</param>
        /// <param name="free">If true, return only the available IPs not used by any virtual machine; if false, return all IPs</param>
        /// <returns>ExternalIps</returns>
        public abstract ExternalIps GetIpsOfExternalNetworkOfCurrentEnterprise(VlanNetwork vlan, bool free);

        /// <summary>
        /// Retrieve IPs of a specific external network of the enterprise/tenant specified in the authentication information
        /// </summary>
        /// <param name="dataCenterLimitsId">Id of the datacenter limits</param>
        /// <param name="externalNetworkId">Id of the external network</param>
        /// <param name="free">If true, return only the available IPs not used by any virtual machine; if false, return all IPs</param>
        /// <returns>ExternalIps</returns>
        public abstract ExternalIps GetIpsOfExternalNetworkOfCurrentEnterprise(int dataCenterLimitsId, int externalNetworkId, bool free);

        /// <summary>
        /// Retrieve IPs of a specific external network
        /// </summary>
        /// <param name="enterpriseId">Id of the enterprise/tenant</param>
        /// <param name="dataCenterLimitsId">Id of the datacenter limits</param>
        /// <param name="externalNetworkId">Id of the external network</param>
        /// <param name="free">If true, return only the available IPs not used by any virtual machine; if false, return all IPs</param>
        /// <returns>ExternalIps</returns>
        public abstract ExternalIps GetIpsOfExternalNetwork(int enterpriseId, int dataCenterLimitsId, int externalNetworkId, bool free);

        /// <summary>
        /// Retrieve all public networks of a specific virtual datacenter
        /// </summary>
        /// <param name="virtualDataCenter">Virtual datacenter</param>
        /// <returns>VlanNetworks</returns>
        public abstract VlanNetworks GetPublicNetworks(VirtualDataCenter virtualDataCenter);

        /// <summary>
        /// Retrieve all public networks of a specific virtual datacenter
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the network belongs to</param>
        /// <returns>VlanNetworks</returns>
        public abstract VlanNetworks GetPublicNetworks(int virtualDataCenterId);

        /// <summary>
        /// Retrieve a public network by Id of a specific virtual datacenter
        /// </summary>
        /// <param name="virtualDataCenter">Virtual datacenter</param>
        /// <param name="id">Id of the public network</param>
        /// <returns>VlanNetwork</returns>
        public abstract VlanNetwork GetPublicNetwork(VirtualDataCenter virtualDataCenter, int id);

        /// <summary>
        /// Retrieve a public network by Id of a specific virtual datacenter
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the network belongs to</param>
        /// <param name="id">Id of the public network</param>
        /// <returns>VlanNetwork</returns>
        public abstract VlanNetwork GetPublicNetwork(int virtualDataCenterId, int id);

        /// <summary>
        /// Retrieve all public IPs of a specific public network that can be purchased
        /// </summary>
        /// <param name="virtualDataCenter">Virtual datacenter</param>
        /// <param name="vlan">Public network</param>
        /// <returns>ExternalIps</returns>
        public abstract PublicIps GetPublicIpsToPurchaseOfPublicNetwork(VirtualDataCenter virtualDataCenter, VlanNetwork vlan);

        /// <summary>
        /// Retrieve all public IPs of a specific public network that can be purchased
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter the network belongs to</param>
        /// <param name="vlanId">Id of the public network</param>
        /// <returns>ExternalIps</returns>
        public abstract PublicIps GetPublicIpsToPurchaseOfPublicNetwork(int virtualDataCenterId, int vlanId);

        /// <summary>
        /// Purchase a public IP to be used in a virtual machine
        /// </summary>
        /// <param name="virtualDataCenter">Virtual datacenter</param>
        /// <param name="publicIp">Id of the public IP to be purchased</param>
        /// <returns>PublicIp</returns>
        public abstract PublicIp PurchasePublicIp(VirtualDataCenter virtualDataCenter, PublicIp publicIp);

        /// <summary>
        /// Purchase a public IP to be used in a virtual machine
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter</param>
        /// <param name="publicIpid">Id of the public IP to be purchased</param>
        /// <returns>PublicIp</returns>
        public abstract PublicIp PurchasePublicIp(int virtualDataCenterId, int publicIpid);

        /// <summary>
        /// Release a public IP
        /// </summary>
        /// <param name="virtualDataCenter">Virtual datacenter</param>
        /// <param name="publicIpid">Id of the public IP to be released</param>
        /// <returns>PublicIp</returns>
        public abstract PublicIp ReleasePublicIp(VirtualDataCenter virtualDataCenter, PublicIp publicIpid);

        /// <summary>
        /// Release a public IP
        /// </summary>
        /// <param name="virtualDataCenterId">Id of the virtual datacenter</param>
        /// <param name="publicIpid">Id of the public IP to be released</param>
        /// <returns>PublicIp</returns>
        public abstract PublicIp ReleasePublicIp(int virtualDataCenterId, int publicIpid);

        #endregion Networks
    }
}
