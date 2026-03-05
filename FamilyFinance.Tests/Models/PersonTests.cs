using FamilyFinance.Models;

namespace FamilyFinance.Tests.Models;

public class PersonTests
{
    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var person = new Person { Name = "" };
        Assert.NotNull(person.Validate());
    }

    [Fact]
    public void Validate_ValidName_ReturnsNull()
    {
        var person = new Person { Name = "John" };
        Assert.Null(person.Validate());
    }

    [Fact]
    public void Validate_NameTooLong_ReturnsError()
    {
        var person = new Person { Name = new string('A', 201) };
        Assert.NotNull(person.Validate());
    }

    [Fact]
    public void Validate_InvalidEmail_ReturnsError()
    {
        var person = new Person { Name = "John", Email = "invalid" };
        Assert.NotNull(person.Validate());
    }

    [Fact]
    public void Create_Valid_ReturnsPerson()
    {
        var person = Person.Create("John", "555-1234", "john@test.com");
        Assert.Equal("John", person.Name);
        Assert.Equal("555-1234", person.Phone);
        Assert.Equal("john@test.com", person.Email);
    }

    [Fact]
    public void Create_InvalidName_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => Person.Create(""));
    }

    [Fact]
    public void Update_ChangesNameAndUpdatedAt()
    {
        var person = Person.Create("John");
        var originalUpdatedAt = person.UpdatedAt;

        System.Threading.Thread.Sleep(10);
        person.Update("Jane", null, null, null);

        Assert.Equal("Jane", person.Name);
        Assert.True(person.UpdatedAt > originalUpdatedAt);
    }
}
