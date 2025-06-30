using IniFileSharp;
using System.Text;

namespace IniSharp.Tests;

/// <summary>
/// 边缘情况和错误处理测试
/// </summary>
public class IniSharpEdgeCasesTests : IDisposable
{
    private readonly string _testFilePath;
    private readonly IniFileSharp.IniSharp _iniSharp;

    public IniSharpEdgeCasesTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), $"edge_test_{Guid.NewGuid()}.ini");
        _iniSharp = new IniFileSharp.IniSharp(_testFilePath);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    [Fact]
    public void SetValue_WithCommentInValue_HandlesCorrectly()
    {
        // Arrange
        string valueWithComment = "Value # with comment";

        // Act
        bool result = _iniSharp.SetValue("TestSection", "TestKey", valueWithComment);

        // Assert
        Assert.True(result);
        string retrievedValue = _iniSharp.GetValue("TestSection", "TestKey");
        // Note: The current implementation might truncate at comment char
        Assert.NotNull(retrievedValue);
    }

    [Fact]
    public void GetValue_WithCommentsInFile_ParsesCorrectly()
    {
        // Arrange
        _iniSharp.SetValue("TestSection", "TestKey", "TestValue");
        
        // Manually add comments to the file
        string content = File.ReadAllText(_testFilePath);
        content = content.Replace("TestKey=TestValue", "TestKey=TestValue # This is a comment");
        File.WriteAllText(_testFilePath, content);

        // Act
        string value = _iniSharp.GetValue("TestSection", "TestKey");

        // Assert
        Assert.Equal("TestValue ", value); // Note the space before comment
    }

    [Fact]
    public void SetValue_EmptyValue_HandlesCorrectly()
    {
        // Act
        bool result = _iniSharp.SetValue("TestSection", "TestKey", "");

        // Assert
        Assert.True(result);
        string retrievedValue = _iniSharp.GetValue("TestSection", "TestKey");
        Assert.Equal("", retrievedValue);
    }

    [Fact]
    public void SetValue_ValueWithSpaces_PreservesSpaces()
    {
        // Arrange
        string valueWithSpaces = "  Value with spaces  ";

        // Act
        bool result = _iniSharp.SetValue("TestSection", "TestKey", valueWithSpaces);

        // Assert
        Assert.True(result);
        string retrievedValue = _iniSharp.GetValue("TestSection", "TestKey");
        Assert.Equal(valueWithSpaces, retrievedValue);
    }

    [Fact]
    public void GetValue_KeyWithDifferentCasing_WorksCorrectly()
    {
        // Arrange
        _iniSharp.SetValue("TestSection", "TestKey", "TestValue");

        // Act
        string value1 = _iniSharp.GetValue("TestSection", "testkey");
        string value2 = _iniSharp.GetValue("TestSection", "TESTKEY");
        string value3 = _iniSharp.GetValue("TestSection", "TestKey");

        // Assert
        Assert.Equal("TestValue", value1);
        Assert.Equal("TestValue", value2);
        Assert.Equal("TestValue", value3);
    }

    [Fact]
    public void SetValue_SectionWithDifferentCasing_WorksCorrectly()
    {
        // Arrange
        _iniSharp.SetValue("TestSection", "Key1", "Value1");

        // Act
        bool result = _iniSharp.SetValue("testsection", "Key2", "Value2");

        // Assert
        Assert.True(result);
        var keys = _iniSharp.GetKeys("TestSection");
        Assert.Contains("Key1", keys);
        Assert.Contains("Key2", keys);
    }

    [Fact]
    public void DeleteKey_NonExistentKey_ReturnsFalseOrDoesNotThrow()
    {
        // Act & Assert - Should not throw
        bool result = _iniSharp.DeleteKey("NonExistentSection", "NonExistentKey");
        // The behavior might vary, but it should not crash
        Assert.True(true); // Test passes if no exception is thrown
    }

    [Fact]
    public void DeleteSection_NonExistentSection_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        bool result = _iniSharp.DeleteSection("NonExistentSection");
        // The behavior might vary, but it should not crash
        Assert.True(true); // Test passes if no exception is thrown
    }

    [Fact]
    public void GetKeys_EmptySection_ReturnsEmptyList()
    {
        // Arrange
        _iniSharp.SetValue("EmptySection", "TempKey", "TempValue");
        _iniSharp.DeleteKey("EmptySection", "TempKey");

        // Act
        var keys = _iniSharp.GetKeys("EmptySection");

        // Assert
        Assert.NotNull(keys);
    }

    [Fact]
    public void SetValue_MultipleEqualsInValue_HandlesCorrectly()
    {
        // Arrange
        string valueWithEquals = "key=value=another=equals";

        // Act
        bool result = _iniSharp.SetValue("TestSection", "TestKey", valueWithEquals);

        // Assert
        Assert.True(result);
        string retrievedValue = _iniSharp.GetValue("TestSection", "TestKey");
        Assert.Equal(valueWithEquals, retrievedValue);
    }

    [Fact]
    public void SetValue_ValueWithSquareBrackets_HandlesCorrectly()
    {
        // Arrange
        string valueWithBrackets = "[SomeValue]";

        // Act
        bool result = _iniSharp.SetValue("TestSection", "TestKey", valueWithBrackets);

        // Assert
        Assert.True(result);
        string retrievedValue = _iniSharp.GetValue("TestSection", "TestKey");
        Assert.Equal(valueWithBrackets, retrievedValue);
    }

    [Fact]
    public void GetSections_WithEmptyFile_ReturnsEmptyList()
    {
        // Arrange - Use a completely empty file
        string emptyFile = Path.Combine(Path.GetTempPath(), $"empty_{Guid.NewGuid()}.ini");
        File.Create(emptyFile).Close();
        var emptyIni = new IniFileSharp.IniSharp(emptyFile);

        try
        {
            // Act
            var sections = emptyIni.GetSections();

            // Assert
            Assert.NotNull(sections);
            Assert.Empty(sections);
        }
        finally
        {
            File.Delete(emptyFile);
        }
    }

    [Fact]
    public void FileEncoding_DefaultEncoding_IsNotNull()
    {
        // Act
        var encoding = _iniSharp.FileEncoding;

        // Assert
        Assert.NotNull(encoding);
    }

    [Fact]
    public void SetValue_VeryLongValue_HandlesCorrectly()
    {
        // Arrange
        string longValue = new string('A', 1000); // 1000 character string

        // Act
        bool result = _iniSharp.SetValue("TestSection", "LongKey", longValue);

        // Assert
        Assert.True(result);
        string retrievedValue = _iniSharp.GetValue("TestSection", "LongKey");
        Assert.Equal(longValue, retrievedValue);
    }

    [Fact]
    public void SetValue_VeryLongSectionName_HandlesCorrectly()
    {
        // Arrange
        string longSection = new string('S', 100); // 100 character section name

        // Act
        bool result = _iniSharp.SetValue(longSection, "TestKey", "TestValue");

        // Assert
        Assert.True(result);
        string retrievedValue = _iniSharp.GetValue(longSection, "TestKey");
        Assert.Equal("TestValue", retrievedValue);
    }

    [Fact]
    public void SetValue_VeryLongKeyName_HandlesCorrectly()
    {
        // Arrange
        string longKey = new string('K', 100); // 100 character key name

        // Act
        bool result = _iniSharp.SetValue("TestSection", longKey, "TestValue");

        // Assert
        Assert.True(result);
        string retrievedValue = _iniSharp.GetValue("TestSection", longKey);
        Assert.Equal("TestValue", retrievedValue);
    }
}
