// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;

namespace Finbuckle.MultiTenant
{
    public interface ITenantInfo
    {
        Guid Id { get; set; }
        string? Identifier { get; set;  }
        string? Name { get; set; }
        string? ConnectionString { get; set; }
    }
}