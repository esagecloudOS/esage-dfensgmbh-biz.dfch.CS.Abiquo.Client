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
using biz.dfch.CS.Abiquo.Client.General;
using biz.dfch.CS.Abiquo.Client.v1;

namespace biz.dfch.CS.Abiquo.Client.Factory
{
    public static class AbiquoClientFactory
    {
        public const string ABIQUO_CLIENT_VERSION_V1 = "v1";

        public static BaseAbiquoClient GetByVersion()
        {
            return GetByVersion(ABIQUO_CLIENT_VERSION_V1);
        }

        public static BaseAbiquoClient GetByVersion(string version)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(version));

            Logger.Current.TraceEvent(TraceEventType.Start, (int) Constants.EventId.GetByVersion, Messages.AbiquoClientFactoryGetByVersionStart, version);

            AbiquoClient abiquoClient;

            switch (version)
            {
                case ABIQUO_CLIENT_VERSION_V1:
                    abiquoClient = new AbiquoClient();
                    break;
                default:
                    Logger.Current.TraceEvent(TraceEventType.Error, (int) Constants.EventId.GetByVersion, Messages.AbiquoClientFactoryGetByVersionConnectionFailed, version);

                    return null;
            }

            Logger.Current.TraceEvent(TraceEventType.Information, (int) Constants.EventId.GetByVersion, Messages.AbiquoClientFactoryGetByVersionConnectionSucceeded, version, abiquoClient.AbiquoApiVersion);

            return abiquoClient;
        }

        public static BaseAbiquoClient GetByCommitHash(string gitCommitHash)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(gitCommitHash));

            var version = LookupAbiquoClientVersion(gitCommitHash);

            return GetByVersion(version);
        }

        private static string LookupAbiquoClientVersion(string gitCommitHash)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(gitCommitHash));

            throw new NotImplementedException();
        }
    }
}
