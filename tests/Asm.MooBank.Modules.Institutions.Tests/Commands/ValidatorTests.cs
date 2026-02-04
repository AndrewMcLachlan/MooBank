#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institutions.Commands;
using FluentValidation.TestHelper;

namespace Asm.MooBank.Modules.Institutions.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateValidatorTests
{
    private readonly CreateValidator _validator = new();

    [Fact]
    public void Validate_EmptyName_HasValidationError()
    {
        // Arrange
        var command = new Create("", InstitutionType.Bank);

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
        var command = new Create(longName, InstitutionType.Bank);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 100 characters");
    }

    [Fact]
    public void Validate_NameExactly100Characters_Passes()
    {
        // Arrange
        var name = new string('a', 100);
        var command = new Create(name, InstitutionType.Bank);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        // Arrange
        var command = new Create("Valid Institution Name", InstitutionType.Bank);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhitespaceName_HasValidationError()
    {
        // Arrange
        var command = new Create("   ", InstitutionType.CreditUnion);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}

[Trait("Category", "Unit")]
public class UpdateValidatorTests
{
    private readonly UpdateValidator _validator = new();

    [Fact]
    public void Validate_ZeroId_HasValidationError()
    {
        // Arrange
        var command = new Update(0, "Valid Name", InstitutionType.Bank);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Institution ID is required");
    }

    [Fact]
    public void Validate_NegativeId_HasValidationError()
    {
        // Arrange
        var command = new Update(-1, "Valid Name", InstitutionType.Bank);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_EmptyName_HasValidationError()
    {
        // Arrange
        var command = new Update(1, "", InstitutionType.Bank);

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
        var command = new Update(1, longName, InstitutionType.Bank);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 100 characters");
    }

    [Fact]
    public void Validate_NameExactly100Characters_Passes()
    {
        // Arrange
        var name = new string('a', 100);
        var command = new Update(1, name, InstitutionType.Bank);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        // Arrange
        var command = new Update(1, "Valid Institution Name", InstitutionType.Bank);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_PositiveId_Passes()
    {
        // Arrange
        var command = new Update(42, "Valid Name", InstitutionType.CreditUnion);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
