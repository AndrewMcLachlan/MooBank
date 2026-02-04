#nullable enable
using Asm.MooBank.Security;
using Asm.MooBank.Security.Authorisation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Asm.MooBank.Core.Tests.Security;

/// <summary>
/// Tests for the Policies static class and policy factory methods.
/// </summary>
[Trait("Category", "Unit")]
public class PoliciesTests
{
    #region Policy Constants

    [Fact]
    public void Admin_HasCorrectValue()
    {
        Assert.Equal("Admin", Policies.Admin);
    }

    [Fact]
    public void FamilyMember_HasCorrectValue()
    {
        Assert.Equal("FamilyMember", Policies.FamilyMember);
    }

    [Fact]
    public void InstrumentOwner_HasCorrectValue()
    {
        Assert.Equal("InstrumentOwner", Policies.InstrumentOwner);
    }

    [Fact]
    public void InstrumentViewer_HasCorrectValue()
    {
        Assert.Equal("InstrumentViewer", Policies.InstrumentViewer);
    }

    [Fact]
    public void GroupOwner_HasCorrectValue()
    {
        Assert.Equal("GroupOwner", Policies.GroupOwner);
    }

    [Fact]
    public void BudgetLine_HasCorrectValue()
    {
        Assert.Equal("BudgetLine", Policies.BudgetLine);
    }

    #endregion

    #region GetInstrumentOwnerPolicy

    [Fact]
    public void GetInstrumentOwnerPolicy_DefaultParam_ReturnsPolicy()
    {
        // Act
        var policy = Policies.GetInstrumentOwnerPolicy();

        // Assert
        Assert.NotNull(policy);
        Assert.Contains(JwtBearerDefaults.AuthenticationScheme, policy.AuthenticationSchemes);
        Assert.Single(policy.Requirements.OfType<InstrumentOwnerRequirement>());
    }

    [Fact]
    public void GetInstrumentOwnerPolicy_CustomParam_ReturnsPolicy()
    {
        // Act
        var policy = Policies.GetInstrumentOwnerPolicy("accountId");

        // Assert
        Assert.NotNull(policy);
        Assert.Single(policy.Requirements.OfType<InstrumentOwnerRequirement>());
    }

    [Fact]
    public void GetInstrumentOwnerPolicy_RequiresAuthentication()
    {
        // Act
        var policy = Policies.GetInstrumentOwnerPolicy();

        // Assert
        Assert.Contains(policy.Requirements, r => r is DenyAnonymousAuthorizationRequirement);
    }

    [Fact]
    public void GetInstrumentOwnerPolicy_ExtensionMethod_ReturnsPolicy()
    {
        // Arrange
        var builder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);

        // Act
        var policy = builder.GetInstrumentOwnerPolicy();

        // Assert
        Assert.NotNull(policy);
        Assert.Single(policy.Requirements.OfType<InstrumentOwnerRequirement>());
    }

    [Fact]
    public void GetInstrumentOwnerPolicy_ExtensionMethod_CustomParam_ReturnsPolicy()
    {
        // Arrange
        var builder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);

        // Act
        var policy = builder.GetInstrumentOwnerPolicy("customId");

        // Assert
        Assert.NotNull(policy);
        Assert.Single(policy.Requirements.OfType<InstrumentOwnerRequirement>());
    }

    #endregion

    #region GetInstrumentViewerPolicy

    [Fact]
    public void GetInstrumentViewerPolicy_DefaultParam_ReturnsPolicy()
    {
        // Act
        var policy = Policies.GetInstrumentViewerPolicy();

        // Assert
        Assert.NotNull(policy);
        Assert.Contains(JwtBearerDefaults.AuthenticationScheme, policy.AuthenticationSchemes);
        Assert.Single(policy.Requirements.OfType<InstrumentViewerRequirement>());
    }

    [Fact]
    public void GetInstrumentViewerPolicy_CustomParam_ReturnsPolicy()
    {
        // Act
        var policy = Policies.GetInstrumentViewerPolicy("viewId");

        // Assert
        Assert.NotNull(policy);
        Assert.Single(policy.Requirements.OfType<InstrumentViewerRequirement>());
    }

    [Fact]
    public void GetInstrumentViewerPolicy_RequiresAuthentication()
    {
        // Act
        var policy = Policies.GetInstrumentViewerPolicy();

        // Assert
        Assert.Contains(policy.Requirements, r => r is DenyAnonymousAuthorizationRequirement);
    }

    [Fact]
    public void GetInstrumentViewerPolicy_ExtensionMethod_ReturnsPolicy()
    {
        // Arrange
        var builder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);

        // Act
        var policy = builder.GetInstrumentViewerPolicy();

        // Assert
        Assert.NotNull(policy);
        Assert.Single(policy.Requirements.OfType<InstrumentViewerRequirement>());
    }

    [Fact]
    public void GetInstrumentViewerPolicy_ExtensionMethod_CustomParam_ReturnsPolicy()
    {
        // Arrange
        var builder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);

        // Act
        var policy = builder.GetInstrumentViewerPolicy("customViewId");

        // Assert
        Assert.NotNull(policy);
        Assert.Single(policy.Requirements.OfType<InstrumentViewerRequirement>());
    }

    #endregion

    #region GetGroupOwnerPolicy

    [Fact]
    public void GetGroupOwnerPolicy_DefaultParam_ReturnsPolicy()
    {
        // Act
        var policy = Policies.GetGroupOwnerPolicy();

        // Assert
        Assert.NotNull(policy);
        Assert.Contains(JwtBearerDefaults.AuthenticationScheme, policy.AuthenticationSchemes);
        Assert.Single(policy.Requirements.OfType<GroupOwnerRequirement>());
    }

    [Fact]
    public void GetGroupOwnerPolicy_CustomParam_ReturnsPolicy()
    {
        // Act
        var policy = Policies.GetGroupOwnerPolicy("customGroupId");

        // Assert
        Assert.NotNull(policy);
        Assert.Single(policy.Requirements.OfType<GroupOwnerRequirement>());
    }

    [Fact]
    public void GetGroupOwnerPolicy_RequiresAuthentication()
    {
        // Act
        var policy = Policies.GetGroupOwnerPolicy();

        // Assert
        Assert.Contains(policy.Requirements, r => r is DenyAnonymousAuthorizationRequirement);
    }

    [Fact]
    public void GetGroupOwnerPolicy_ExtensionMethod_ReturnsPolicy()
    {
        // Arrange
        var builder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);

        // Act
        var policy = builder.GetGroupOwnerPolicy();

        // Assert
        Assert.NotNull(policy);
        Assert.Single(policy.Requirements.OfType<GroupOwnerRequirement>());
    }

    [Fact]
    public void GetGroupOwnerPolicy_ExtensionMethod_CustomParam_ReturnsPolicy()
    {
        // Arrange
        var builder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);

        // Act
        var policy = builder.GetGroupOwnerPolicy("myGroup");

        // Assert
        Assert.NotNull(policy);
        Assert.Single(policy.Requirements.OfType<GroupOwnerRequirement>());
    }

    #endregion
}

/// <summary>
/// Tests for the ClaimTypes static class constants.
/// </summary>
[Trait("Category", "Unit")]
public class ClaimTypesTests
{
    [Fact]
    public void UserId_HasCorrectValue()
    {
        Assert.Equal("http://schemas.microsoft.com/identity/claims/objectidentifier", ClaimTypes.UserId);
    }

    [Fact]
    public void FamilyId_HasCorrectValue()
    {
        Assert.Equal("http://moobank.mclachlan.family/claims/familyid", ClaimTypes.FamilyId);
    }

    [Fact]
    public void AccountId_HasCorrectValue()
    {
        Assert.Equal("http://moobank.mclachlan.family/claims/accountid", ClaimTypes.AccountId);
    }

    [Fact]
    public void SharedAccountId_HasCorrectValue()
    {
        Assert.Equal("http://moobank.mclachlan.family/claims/sharedaccountid", ClaimTypes.SharedAccountId);
    }

    [Fact]
    public void PrimaryAccountId_HasCorrectValue()
    {
        Assert.Equal("http://moobank.mclachlan.family/claims/primaryaccountid", ClaimTypes.PrimaryAccountId);
    }

    [Fact]
    public void Currency_HasCorrectValue()
    {
        Assert.Equal("http://moobank.mclachlan.family/claims/currency", ClaimTypes.Currency);
    }

    [Fact]
    public void GroupId_HasCorrectValue()
    {
        Assert.Equal("http://moobank.mclachlan.family/claims/groupid", ClaimTypes.GroupId);
    }
}
