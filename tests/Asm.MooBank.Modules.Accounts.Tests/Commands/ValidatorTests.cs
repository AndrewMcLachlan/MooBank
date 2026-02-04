#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Commands;
using Asm.MooBank.Modules.Accounts.Tests.Support;
using FluentValidation.TestHelper;

namespace Asm.MooBank.Modules.Accounts.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateValidatorTests
{
    private readonly CreateValidator _validator;

    public CreateValidatorTests()
    {
        _validator = new CreateValidator();
    }

    [Fact]
    public void Validate_EmptyName_HasValidationError()
    {
        // Arrange
        var command = new Create
        {
            Name = "",
            Description = null,
            Currency = "AUD",
            InstitutionId = 1,
            Balance = 0m,
            IncludeInBudget = true,
            GroupId = null,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Validate_NameTooLong_HasValidationError()
    {
        // Arrange
        var longName = new string('a', 101);
        var command = new Create
        {
            Name = longName,
            Description = null,
            Currency = "AUD",
            InstitutionId = 1,
            Balance = 0m,
            IncludeInBudget = true,
            GroupId = null,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 100 characters");
    }

    [Fact]
    public void Validate_DescriptionTooLong_HasValidationError()
    {
        // Arrange
        var longDescription = new string('a', 501);
        var command = new Create
        {
            Name = "Valid Name",
            Description = longDescription,
            Currency = "AUD",
            InstitutionId = 1,
            Balance = 0m,
            IncludeInBudget = true,
            GroupId = null,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 500 characters");
    }

    [Fact]
    public void Validate_NullDescription_Passes()
    {
        // Arrange
        var command = new Create
        {
            Name = "Valid Name",
            Description = null,
            Currency = "AUD",
            InstitutionId = 1,
            Balance = 0m,
            IncludeInBudget = true,
            GroupId = null,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_EmptyCurrency_HasValidationError()
    {
        // Arrange
        var command = new Create
        {
            Name = "Valid Name",
            Description = null,
            Currency = "",
            InstitutionId = 1,
            Balance = 0m,
            IncludeInBudget = true,
            GroupId = null,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency is required");
    }

    [Fact]
    public void Validate_CurrencyTooShort_HasValidationError()
    {
        // Arrange
        var command = new Create
        {
            Name = "Valid Name",
            Description = null,
            Currency = "AU",
            InstitutionId = 1,
            Balance = 0m,
            IncludeInBudget = true,
            GroupId = null,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency must be a 3-letter ISO code");
    }

    [Fact]
    public void Validate_CurrencyTooLong_HasValidationError()
    {
        // Arrange
        var command = new Create
        {
            Name = "Valid Name",
            Description = null,
            Currency = "AUDD",
            InstitutionId = 1,
            Balance = 0m,
            IncludeInBudget = true,
            GroupId = null,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void Validate_ZeroInstitutionId_HasValidationError()
    {
        // Arrange
        var command = new Create
        {
            Name = "Valid Name",
            Description = null,
            Currency = "AUD",
            InstitutionId = 0,
            Balance = 0m,
            IncludeInBudget = true,
            GroupId = null,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InstitutionId)
            .WithErrorMessage("Institution is required");
    }

    [Fact]
    public void Validate_NegativeInstitutionId_HasValidationError()
    {
        // Arrange
        var command = new Create
        {
            Name = "Valid Name",
            Description = null,
            Currency = "AUD",
            InstitutionId = -1,
            Balance = 0m,
            IncludeInBudget = true,
            GroupId = null,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InstitutionId);
    }

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        // Arrange
        var command = new Create
        {
            Name = "Valid Account Name",
            Description = "A valid description",
            Currency = "AUD",
            InstitutionId = 1,
            Balance = 1000m,
            IncludeInBudget = true,
            GroupId = null,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NameExactly100Characters_Passes()
    {
        // Arrange
        var name = new string('a', 100);
        var command = new Create
        {
            Name = name,
            Currency = "AUD",
            InstitutionId = 1,
            Balance = 0m,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_DescriptionExactly500Characters_Passes()
    {
        // Arrange
        var description = new string('a', 500);
        var command = new Create
        {
            Name = "Valid Name",
            Description = description,
            Currency = "AUD",
            InstitutionId = 1,
            Balance = 0m,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}

[Trait("Category", "Unit")]
public class UpdateValidatorTests
{
    private readonly UpdateValidator _validator;

    public UpdateValidatorTests()
    {
        _validator = new UpdateValidator();
    }

    [Fact]
    public void Validate_NullAccount_HasValidationError()
    {
        // Arrange
        var command = new Update(null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Account);
    }

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        // Arrange
        var account = TestEntities.CreateLogicalAccountModel(
            name: "Valid Account",
            currency: "AUD");
        var command = new Update(account);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

[Trait("Category", "Unit")]
public class LogicalAccountValidatorTests
{
    private readonly LogicalAccountValidator _validator;

    public LogicalAccountValidatorTests()
    {
        _validator = new LogicalAccountValidator();
    }

    [Fact]
    public void Validate_EmptyName_HasValidationError()
    {
        // Arrange
        var account = TestEntities.CreateLogicalAccountModel(name: "", currency: "AUD");

        // Act
        var result = _validator.TestValidate(account);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Validate_NameTooLong_HasValidationError()
    {
        // Arrange
        var longName = new string('a', 101);
        var account = TestEntities.CreateLogicalAccountModel(name: longName, currency: "AUD");

        // Act
        var result = _validator.TestValidate(account);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 100 characters");
    }

    [Fact]
    public void Validate_DescriptionTooLong_HasValidationError()
    {
        // Arrange
        var longDescription = new string('a', 501);
        var account = TestEntities.CreateLogicalAccountModel(
            name: "Valid Name",
            description: longDescription,
            currency: "AUD");

        // Act
        var result = _validator.TestValidate(account);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 500 characters");
    }

    [Fact]
    public void Validate_NullDescription_Passes()
    {
        // Arrange
        var account = TestEntities.CreateLogicalAccountModel(
            name: "Valid Name",
            description: null,
            currency: "AUD");

        // Act
        var result = _validator.TestValidate(account);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_EmptyCurrency_HasValidationError()
    {
        // Arrange
        var account = TestEntities.CreateLogicalAccountModel(name: "Valid Name", currency: "");

        // Act
        var result = _validator.TestValidate(account);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency is required");
    }

    [Fact]
    public void Validate_CurrencyWrongLength_HasValidationError()
    {
        // Arrange
        var account = TestEntities.CreateLogicalAccountModel(name: "Valid Name", currency: "AU");

        // Act
        var result = _validator.TestValidate(account);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency must be a 3-letter ISO code");
    }

    [Fact]
    public void Validate_ValidAccount_Passes()
    {
        // Arrange
        var account = TestEntities.CreateLogicalAccountModel(
            name: "Valid Account Name",
            description: "A valid description",
            currency: "AUD");

        // Act
        var result = _validator.TestValidate(account);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NameExactly100Characters_Passes()
    {
        // Arrange
        var name = new string('a', 100);
        var account = TestEntities.CreateLogicalAccountModel(name: name, currency: "AUD");

        // Act
        var result = _validator.TestValidate(account);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_DescriptionExactly500Characters_Passes()
    {
        // Arrange
        var description = new string('a', 500);
        var account = TestEntities.CreateLogicalAccountModel(
            name: "Valid Name",
            description: description,
            currency: "AUD");

        // Act
        var result = _validator.TestValidate(account);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
