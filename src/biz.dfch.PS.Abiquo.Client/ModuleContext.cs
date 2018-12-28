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
using System.Management.Automation;
using biz.dfch.CS.Abiquo.Client;
using biz.dfch.CS.Abiquo.Client.Factory;
using biz.dfch.CS.Commons.Diagnostics;
using biz.dfch.CS.PowerShell.Commons;
using TraceSource = biz.dfch.CS.Commons.Diagnostics.TraceSource;

namespace biz.dfch.PS.Abiquo.Client
{
    /// <summary>
    /// ModuleContext
    /// </summary>
    public class ModuleContext
    {
        /// <summary>
        /// Specifies the API version to use
        /// </summary>
        public string ApiVersion { get; internal set; }

        /// <summary>
        /// Uri of Abiquo endpoint
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Credentials to use when using authentication type plain
        /// </summary>
        public PSCredential Credential { get; set; }

        /// <summary>
        /// Token to use when using authentication type oauth2
        /// </summary>
        public string OAuth2Token { get; set; }

        /// <summary>
        /// Specifies the authentication type to use
        /// </summary>
        public string AuthenticationType { get; set; }

        private static readonly Lazy<BaseAbiquoClient> _client = new Lazy<BaseAbiquoClient>(() =>
        {
            Contract.Ensures(null != Contract.Result<BaseAbiquoClient>());

            var apiVersion = ModuleConfiguration.Current.ApiVersion;
            var client = String.IsNullOrWhiteSpace(apiVersion) 
                ? AbiquoClientFactory.GetByVersion() 
                : AbiquoClientFactory.GetByVersion(apiVersion);
            return client;
        });

        /// <summary>
        /// Returns a reference to the underlying Abiquo client
        /// </summary>
        public BaseAbiquoClient Client
        {
            get
            {
                Contract.Ensures(null != Contract.Result<BaseAbiquoClient>());
                
                return _client.Value;
            }
        }

        private static readonly Lazy<TraceSource> _traceSource = new Lazy<TraceSource>(() =>
        {
            Contract.Ensures(null != Contract.Result<TraceSource>());

            var traceSource = Logger.Get(ModuleConfiguration.LOGGER_NAME);

            ContractFailedEventHandler.RegisterTraceSource(traceSource);

            return traceSource;
        });

        /// <summary>
        /// Returns a reference to the TraceSource instance of this module
        /// </summary>
        public TraceSource TraceSource
        {
            get { return _traceSource.Value; } 
        }
    }
}
