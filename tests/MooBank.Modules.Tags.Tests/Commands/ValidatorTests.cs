#nullable enable
using Asm.MooBank.Modules.Tags.Commands;
using Asm.MooBank.Modules.Tags.Tests.Support;
using FluentValidation.TestHelper;

namespace Asm.MooBank.Modules.Tags.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateValidatorTests
{
    private readonly CreateValidator _validator;

    public CreateValidatorTests()
    {
        _validator = new CreateValidator();
    }

    [Fact]
    public void Validate_NullTag_HasValidationError()
    {
        // Arrange
        var command = new Create(null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Tag);
    }

    [Fact]
    public void Validate_ValidTag_Passes()
    {
        // Arrange
        var tag = TestEntities.CreateTagModel(name: "Valid Tag");
        var command = new Create(tag);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
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
    public void Validate_ZeroId_HasValidationError()
    {
        // Arrange
        var updateTag = TestEntities.CreateUpdateTag(name: "Test");
        var command = new Update(0, updateTag);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Tag ID is required");
    }

    [Fact]
    public void Validate_NegativeId_HasValidationError()
    {
        // Arrange
        var updateTag = TestEntities.CreateUpdateTag(name: "Test");
        var command = new Update(-1, updateTag);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_NullTag_HasValidationError()
    {
        // Arrange
        var command = new Update(1, null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Tag);
    }

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        // Arrange
        var updateTag = TestEntities.CreateUpdateTag(name: "Valid Tag");
        var command = new Update(1, updateTag);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

[Trait("Category", "Unit")]
public class TagValidatorTests
{
    private readonly TagValidator _validator;

    public TagValidatorTests()
    {
        _validator = new TagValidator();
    }

    [Fact]
    public void Validate_EmptyName_HasValidationError()
    {
        // Arrange
        var tag = TestEntities.CreateTagModel(name: "");

        // Act
        var result = _validator.TestValidate(tag);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Validate_NullName_HasValidationError()
    {
        // Arrange
        var tag = TestEntities.CreateTagModel();
        tag = tag with { Name = null! };

        // Act
        var result = _validator.TestValidate(tag);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooLong_HasValidationError()
    {
        // Arrange
        var longName = new string('a', 51);
        var tag = TestEntities.CreateTagModel(name: longName);

        // Act
        var result = _validator.TestValidate(tag);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 50 characters");
    }

    [Fact]
    public void Validate_NameExactly50Characters_Passes()
    {
        // Arrange
        var name = new string('a', 50);
        var tag = TestEntities.CreateTagModel(name: name);

        // Act
        var result = _validator.TestValidate(tag);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_ValidName_Passes()
    {
        // Arrange
        var tag = TestEntities.CreateTagModel(name: "Valid Tag Name");

        // Act
        var result = _validator.TestValidate(tag);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

[Trait("Category", "Unit")]
public class UpdateTagValidatorTests
{
    private readonly UpdateTagValidator _validator;

    public UpdateTagValidatorTests()
    {
        _validator = new UpdateTagValidator();
    }

    [Fact]
    public void Validate_EmptyName_HasValidationError()
    {
        // Arrange
        var updateTag = TestEntities.CreateUpdateTag(name: "");

        // Act
        var result = _validator.TestValidate(updateTag);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Validate_NameTooLong_HasValidationError()
    {
        // Arrange
        var longName = new string('a', 51);
        var updateTag = TestEntities.CreateUpdateTag(name: longName);

        // Act
        var result = _validator.TestValidate(updateTag);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 50 characters");
    }

    [Fact]
    public void Validate_NameExactly50Characters_Passes()
    {
        // Arrange
        var name = new string('a', 50);
        var updateTag = TestEntities.CreateUpdateTag(name: name);

        // Act
        var result = _validator.TestValidate(updateTag);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_ValidUpdateTag_Passes()
    {
        // Arrange
        var updateTag = TestEntities.CreateUpdateTag(name: "Valid Name");

        // Act
        var result = _validator.TestValidate(updateTag);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
