// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using NuGet.Services.FeatureFlags;

namespace NuGetGallery.Auditing.AuditedEntities
{
    public class AuditedFeatureFlagFlight
    {
        public string Name { get; private set; }
        public bool All { get; private set; }
        public bool SiteAdmins { get; private set; }
        public int AccountsCount { get; private set; }
        public int DomainsCount { get; private set; }

        public static AuditedFeatureFlagFlight[] CreateFrom(FeatureFlags flags)
        {
            return flags.Flights
                .Select(f => CreateFrom(f.Key, f.Value))
                .ToArray();
        }

        public static AuditedFeatureFlagFlight CreateFrom(string name, Flight flight)
        {
            return new AuditedFeatureFlagFlight
            {
                Name = name,
                All = flight.All,
                SiteAdmins = flight.SiteAdmins,
                AccountsCount = flight.Accounts?.Count() ?? 0,
                DomainsCount = flight.Domains?.Count() ?? 0
            };
        }
    }
}
