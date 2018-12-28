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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Management.Automation;
using System.Net;
using biz.dfch.CS.Abiquo.Client;
using biz.dfch.CS.Abiquo.Client.Authentication;
using biz.dfch.CS.PowerShell.Commons;

namespace biz.dfch.PS.Abiquo.Client
{
    /// <summary>
    /// This class defines the EnterServer Cmdlet that performs a login to an Abiquo instance
    /// </summary>
    [Cmdlet(
         VerbsCommon.Enter, "Server"
         ,
         ConfirmImpact = ConfirmImpact.Medium
         ,
         DefaultParameterSetName = ParameterSets.PLAIN
         ,
         SupportsShouldProcess = true
         ,
         HelpUri = "http://dfch.biz/biz/dfch/PS/Abiquo/Client/Enter-Server/"
    )]
    [OutputType(typeof(BaseAbiquoClient))]
    public class EnterServer : PsCmdletBase
    {
        /// <summary>
        /// Defines all valid parameter sets for this cmdlet
        /// </summary>
        public static class ParameterSets
        {
            /// <summary>
            /// ParameterSetName used when specifying a credential object
            /// </summary>
            public const string CREDENTIAL = "cred";

            /// <summary>
            /// ParameterSetName used when specifying plain username and password
            /// </summary>
            public const string PLAIN = ModuleContextSection.AUTHENTICATION_TYPE_PLAIN;

            /// <summary>
            /// ParameterSetName used when specifying an OAuth2 token
            /// </summary>
            public const string OAUTH2 = "oauth2";

            /// <summary>
            /// ParameterSetName used when using settings from ModuleContext
            /// </summary>
            public const string MODULE_CONTEXT = "config";
        }

        /// <summary>
        /// The default tenant id value (if not specified by the user)
        /// </summary>
        public const int TENANT_ID_DEFAULT_VALUE = 1;

        /// <summary>
        /// Specifies the base url of the Abiquo endpoint
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSets.PLAIN)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSets.CREDENTIAL)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSets.OAUTH2)]
        [Alias("ConnectionUri")]
        public Uri Uri { get; set; }

        /// <summary>
        /// Specifies the username to log in with
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ParameterSets.PLAIN)]
        public string Username { get; set; }

        /// <summary>
        /// Specifies the password to log in with
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = ParameterSets.PLAIN)]
        public string Password { get; set; }

        /// <summary>
        /// Specifies the PSCredential object containing username and password to log in with
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ParameterSets.CREDENTIAL)]
        [Alias("cred")]
        public PSCredential Credential { get; set; }

        /// <summary>
        /// Specifies the OAUTH2 token to login in with
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ParameterSets.OAUTH2)]
        [Alias("token")]
        public string OAuth2Token { get; set; }

        /// <summary>
        /// Specifies the tenant id for the user to log in with. If you do not specify this parameter it will be retrieved at runtime
        /// </summary>
        [Parameter(Mandatory = false)]
        [Alias("tid")]
        [ValidateRange(TENANT_ID_DEFAULT_VALUE, int.MaxValue)]
        [PSDefaultValue(Value = TENANT_ID_DEFAULT_VALUE)]
        public int TenantId { get; set; }

        /// <summary>
        /// Use settings from ModuleContext variable to log in
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSets.MODULE_CONTEXT)]
        [Alias("ctx", "context")]
        [PSDefaultValue(Value = false)]
        public SwitchParameter UseModuleContext { get; set; }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var parameterSetName = ParameterSetName;

            var moduleContext = ModuleConfiguration.Current;
            var client = moduleContext.Client;

            if (ParameterSets.MODULE_CONTEXT.Equals(parameterSetName))
            {
                switch (moduleContext.AuthenticationType)
                {
                    case ParameterSets.PLAIN:
                    case ParameterSets.CREDENTIAL:
                        Credential = moduleContext.Credential;
                        parameterSetName = ParameterSets.CREDENTIAL;
                        break;
                    case ParameterSets.OAUTH2:
                        OAuth2Token = moduleContext.OAuth2Token;
                        parameterSetName = ParameterSets.OAUTH2;
                        break;
                    default:
                        const bool isValidAuthenticationType = false;
                        Contract.Assert(isValidAuthenticationType, moduleContext.AuthenticationType);
                        break;
                }
                Uri = moduleContext.Uri;
                Contract.Assert(null != Uri);
            }

            var shouldProcessMessage = string.Format(Messages.EnterServerShouldProcess, Uri.AbsoluteUri, parameterSetName);
            if (!ShouldProcess(shouldProcessMessage))
            {
                return;
            }

            // perform login
            var authInfo = GetAuthenticationInformation(parameterSetName);
            try
            {
                ModuleConfiguration.Current.TraceSource.TraceEvent(TraceEventType.Verbose, (int) Constants.EventId.EnterServer, Messages.EnterServerLogin, Uri.AbsoluteUri);
                var hasLoginSucceeded1 = client.Login(Uri.AbsoluteUri, authInfo);
                if (hasLoginSucceeded1)
                {
                    ModuleConfiguration.Current.TraceSource.TraceEvent(TraceEventType.Information, (int) Constants.EventId.EnterServer, Messages.EnterServerLoginSucceeded, Uri.AbsoluteUri);
                }
                else
                {
                    ModuleConfiguration.Current.TraceSource.TraceEvent(TraceEventType.Error, (int) Constants.EventId.EnterServer, Messages.EnterServerLoginFailed0, Uri.AbsoluteUri);
                }

                Contract.Assert(hasLoginSucceeded1, string.Format(Messages.EnterServerLoginFailed1, Uri.AbsoluteUri));
            }
            catch (AggregateException aggrex)
            {
                if (null == ProcessAggregateException(aggrex))
                {
                    return;
                }

                throw;
            }

            if (!MyInvocation.BoundParameters.ContainsKey("TenantId"))
            {
                // return client
                WriteObject(client);

                return;
            }

            // switch to specified tenant
            try
            {
                client.SwitchEnterprise(TenantId);
            }
            catch (AggregateException aggrex)
            {
                if (null == ProcessAggregateException(aggrex))
                {
                    return;
                }

                throw;
            }

            // return client
            WriteObject(client);
        }

        private IAuthenticationInformation GetAuthenticationInformation(string parameterSetName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(parameterSetName));
            Contract.Ensures(null != Contract.Result<IAuthenticationInformation>());

            IAuthenticationInformation authInfo;
            switch (parameterSetName)
            {
                case ParameterSets.PLAIN:
                    authInfo = new BasicAuthenticationInformation(Username, Password);
                    break;
                case ParameterSets.CREDENTIAL:
                    Username = Credential.UserName;
                    Password = Credential.GetNetworkCredential().Password;
                    authInfo = new BasicAuthenticationInformation(Username, Password);
                    break;
                case ParameterSets.OAUTH2:
                    authInfo = new OAuth2AuthenticationInformation(OAuth2Token);
                    break;
                default:
                    // ReSharper disable once NotResolvedInText
                    throw new ArgumentOutOfRangeException("ParameterSetName", ParameterSetName, string.Format(Messages.EnterServerInvalidParameterSetName, ParameterSetName));
            }

            return authInfo;
        }

        // this exception is rather nasty - so let's try to extract something useful from it
        private Exception ProcessAggregateException(AggregateException aggrex)
        {
            Contract.Requires(null != aggrex);

            var httpReqEx = aggrex.InnerExceptions.FirstOrDefault();
            if (null == httpReqEx)
            {
                return aggrex;
            }
            var ex = httpReqEx.InnerException as WebException;
            if (null == ex)
            {
                return aggrex;
            }

            var errorRecord = new ErrorRecord(ex, Constants.EventId.EnterServerFailed.ToString(), ErrorCategory.ConnectionError, this);
            WriteError(errorRecord);

            return null;
        }
    }
}
