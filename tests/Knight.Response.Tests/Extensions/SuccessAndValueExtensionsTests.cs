using Knight.Response.Extensions;
using Knight.Response.Factories;
using Shouldly;

namespace Knight.Response.Tests.Extensions;

public class SuccessAndValueExtensionsTests
{
    // -------------------- Null --------------------

    [Fact]
    public void IsSuccessAndValueIsNull()
    {
        Results.Success<string?>()
            .IsSuccessAndValueIsNull()
            .ShouldBeTrue();

        Results.Success("x")
            .IsSuccessAndValueIsNull()
            .ShouldBeFalse();

        Results.Failure<string>("err")
            .IsSuccessAndValueIsNull()
            .ShouldBeFalse();
    }

    // -------------------- Null / Empty --------------------

    [Fact]
    public void IsSuccessAndValueIsNullOrEmpty_Generic()
    {
        Results.Success<string?>()
            .IsSuccessAndValueIsNullOrEmpty()
            .ShouldBeTrue();

        Results.Success("")
            .IsSuccessAndValueIsNullOrEmpty()
            .ShouldBeTrue();

        Results.Success("x")
            .IsSuccessAndValueIsNullOrEmpty()
            .ShouldBeFalse();

        Results.Failure<string>("err")
            .IsSuccessAndValueIsNullOrEmpty()
            .ShouldBeFalse();
    }

    [Fact]
    public void IsSuccessAndValueIsNullOrEmpty_Collection()
    {
        Results.Success<int[]?>()
            .IsSuccessAndValueIsNullOrEmpty<int[], int>()
            .ShouldBeTrue();

        Results.Success(Array.Empty<int>())
            .IsSuccessAndValueIsNullOrEmpty<int[], int>()
            .ShouldBeTrue();

        Results.Success(new[] { 1 })
            .IsSuccessAndValueIsNullOrEmpty<int[], int>()
            .ShouldBeFalse();

        Results.Failure<int[]>("err")
            .IsSuccessAndValueIsNullOrEmpty<int[], int>()
            .ShouldBeFalse();
    }

    // -------------------- Whitespace --------------------

    [Fact]
    public void IsSuccessAndValueIsNullOrWhiteSpace()
    {
        Results.Success<string?>()
            .IsSuccessAndValueIsNullOrWhiteSpace()
            .ShouldBeTrue();

        Results.Success("")
            .IsSuccessAndValueIsNullOrWhiteSpace()
            .ShouldBeTrue();

        Results.Success("   ")
            .IsSuccessAndValueIsNullOrWhiteSpace()
            .ShouldBeTrue();

        Results.Success("x")
            .IsSuccessAndValueIsNullOrWhiteSpace()
            .ShouldBeFalse();

        Results.Failure<string>("err")
            .IsSuccessAndValueIsNullOrWhiteSpace()
            .ShouldBeFalse();
    }

    // -------------------- Not Null --------------------

    [Fact]
    public void IsSuccessAndValueIsNotNull()
    {
        Results.Success("x")
            .IsSuccessAndValueIsNotNull()
            .ShouldBeTrue();

        Results.Success<string?>()
            .IsSuccessAndValueIsNotNull()
            .ShouldBeFalse();

        Results.Failure<string>("err")
            .IsSuccessAndValueIsNotNull()
            .ShouldBeFalse();
    }

    // -------------------- Not Null / Empty --------------------

    [Fact]
    public void IsSuccessAndValueIsNotNullOrEmpty()
    {
        Results.Success(new[] { 1 })
            .IsSuccessAndValueIsNotNullOrEmpty<int[], int>()
            .ShouldBeTrue();

        Results.Success(Array.Empty<int>())
            .IsSuccessAndValueIsNotNullOrEmpty<int[], int>()
            .ShouldBeFalse();

        Results.Success<int[]?>()
            .IsSuccessAndValueIsNotNullOrEmpty<int[], int>()
            .ShouldBeFalse();

        Results.Failure<int[]>("err")
            .IsSuccessAndValueIsNotNullOrEmpty<int[], int>()
            .ShouldBeFalse();
    }

    [Fact]
    public void IsSuccessAndValueIsNotNullOrWhiteSpace()
    {
        Results.Success("x")
            .IsSuccessAndValueIsNotNullOrWhiteSpace()
            .ShouldBeTrue();

        Results.Success(" ")
            .IsSuccessAndValueIsNotNullOrWhiteSpace()
            .ShouldBeFalse();

        Results.Success<string?>()
            .IsSuccessAndValueIsNotNullOrWhiteSpace()
            .ShouldBeFalse();

        Results.Failure<string>("err")
            .IsSuccessAndValueIsNotNullOrWhiteSpace()
            .ShouldBeFalse();
    }

    // -------------------- Boolean --------------------

    [Fact]
    public void IsSuccessAndValueIsTrue_Bool()
    {
        Results.Success(true)
            .IsSuccessAndValueIsTrue()
            .ShouldBeTrue();

        Results.Success(false)
            .IsSuccessAndValueIsTrue()
            .ShouldBeFalse();

        Results.Failure<bool>("err")
            .IsSuccessAndValueIsTrue()
            .ShouldBeFalse();
    }

    [Fact]
    public void IsSuccessAndValueIsFalse_Bool()
    {
        Results.Success(false)
            .IsSuccessAndValueIsFalse()
            .ShouldBeTrue();

        Results.Success(true)
            .IsSuccessAndValueIsFalse()
            .ShouldBeFalse();

        Results.Failure<bool>("err")
            .IsSuccessAndValueIsFalse()
            .ShouldBeFalse();
    }

    // -------------------- Nullable Boolean --------------------

    [Fact]
    public void IsSuccessAndValueIsTrue_NullableBool()
    {
        Results.Success<bool?>(true)
            .IsSuccessAndValueIsTrue()
            .ShouldBeTrue();

        Results.Success<bool?>(false)
            .IsSuccessAndValueIsTrue()
            .ShouldBeFalse();

        Results.Success<bool?>()
            .IsSuccessAndValueIsTrue()
            .ShouldBeFalse();

        Results.Failure<bool?>("err")
            .IsSuccessAndValueIsTrue()
            .ShouldBeFalse();
    }

    [Fact]
    public void IsSuccessAndValueIsFalse_NullableBool()
    {
        Results.Success<bool?>(false)
            .IsSuccessAndValueIsFalse()
            .ShouldBeTrue();

        Results.Success<bool?>(true)
            .IsSuccessAndValueIsFalse()
            .ShouldBeFalse();

        Results.Success<bool?>()
            .IsSuccessAndValueIsFalse()
            .ShouldBeFalse();

        Results.Failure<bool?>("err")
            .IsSuccessAndValueIsFalse()
            .ShouldBeFalse();
    }
}
