#nullable enable
using Asm.MooBank.Modules.Groups.Commands;
using Asm.MooBank.Modules.Groups.Tests.Support;
using FluentValidation.TestHelper;

namespace Asm.MooBank.Modules.Groups.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateValidatorTests
{
    private readonly CreateValidator _validator = new();

    [Fact]
    public void Validate_EmptyName_HasValidationError()
    {
        // Arrange
        var command = new Create("", "Description", false);

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
        var command = new Create(longName, "Description", false);

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
        var command = new Create(name, "Description", false);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_DescriptionTooLong_HasValidationError()
    {
        // Arrange
        var longDescription = new string('a', 501);
        var command = new Create("Valid Name", longDescription, false);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 500 characters");
    }

    [Fact]
    public void Validate_DescriptionExactly500Characters_Passes()
    {
        // Arrange
        var description = new string('a', 500);
        var command = new Create("Valid Name", description, false);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_NullDescription_Passes()
    {
        // Arrange
        var command = new Create("Valid Name", null!, false);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        // Arrange
        var command = new Create("Valid Group Name", "A valid description", true);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

[Trait("Category", "Unit")]
public class UpdateValidatorTests
{
    private readonly UpdateValidator _validator = new();

    [Fact]
    public void Validate_NullGroup_HasValidationError()
    {
        // Arrange
        var command = new Update(null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Group);
    }

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        // Arrange
        var group = TestEntities.CreateGroupModel(name: "Valid Group");
        var command = new Update(group);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_GroupWithEmptyName_HasValidationError()
    {
        // Arrange
        var group = TestEntities.CreateGroupModel(name: "");
        var command = new Update(group);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Group.Name")
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Validate_GroupWithLongName_HasValidationError()
    {
        // Arrange
        var longName = new string('a', 101);
        var group = TestEntities.CreateGroupModel(name: longName);
        var command = new Update(group);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Group.Name")
            .WithErrorMessage("Name must not exceed 100 characters");
    }
}

[Trait("Category", "Unit")]
public class GroupValidatorTests
{
    private readonly GroupValidator _validator = new();

    [Fact]
    public void Validate_EmptyName_HasValidationError()
    {
        // Arrange
        var group = TestEntities.CreateGroupModel(name: "");

        // Act
        var result = _validator.TestValidate(group);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Validate_NameTooLong_HasValidationError()
    {
        // Arrange
        var longName = new string('a', 101);
        var group = TestEntities.CreateGroupModel(name: longName);

        // Act
        var result = _validator.TestValidate(group);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 100 characters");
    }

    [Fact]
    public void Validate_DescriptionTooLong_HasValidationError()
    {
        // Arrange
        var longDescription = new string('a', 501);
        var group = TestEntities.CreateGroupModel(name: "Valid", description: longDescription);

        // Act
        var result = _validator.TestValidate(group);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 500 characters");
    }

    [Fact]
    public void Validate_NullDescription_Passes()
    {
        // Arrange
        var group = TestEntities.CreateGroupModel(name: "Valid", description: null);

        // Act
        var result = _validator.TestValidate(group);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_ValidGroup_Passes()
    {
        // Arrange
        var group = TestEntities.CreateGroupModel(name: "Valid Group", description: "A description");

        // Act
        var result = _validator.TestValidate(group);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
