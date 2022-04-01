// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using Finbuckle.MultiTenant.Internal;

namespace Finbuckle.MultiTenant
{
    public class TenantInfo : ITenantInfo
    {
        private Guid id;

        public TenantInfo()
        {
        }

        public Guid Id
        {
            get
            {
                return id;
            }
            set
            {
                if (value != null)
                {
                    id = value;
                }
            }
        }

        public string? Identifier { get; set; }
        public string? Name { get; set; }
        public string? ConnectionString { get; set; }
    }
}