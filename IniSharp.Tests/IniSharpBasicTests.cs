using IniFileSharp;
using System.Text;

namespace IniSharp.Tests;

/// <summary>
/// 基本功能测试，避免已知问题的删除操作
/// </summary>
public class IniSharpBasicTests : IDisposable
{
    private readonly string _testFilePath;
    private readonly IniFileSharp.IniSharp _iniSharp;

    public IniSharpBasicTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), $"basic_test_{Guid.NewGuid()}.ini");
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
    public void Constructor_CreatesFileSuccessfully()
    {
        // Assert
        Assert.True(File.Exists(_testFilePath));
    }

    [Fact]
    public void SetValue_NewSectionAndKey_CreatesCorrectly()
    {
        // Act
        bool result = _iniSharp.SetValue("TestSection", "TestKey", "TestValue");

        // Assert
        Assert.True(result);
        string value = _iniSharp.GetValue("TestSection", "TestKey");
        Assert.Equal("TestValue", value);
    }

    [Fact]
    public void SetValue_UpdateExistingKey_WorksCorrectly()
    {
        // Arrange
        _iniSharp.SetValue("TestSection", "TestKey", "InitialValue");

        // Act
        bool result = _iniSharp.SetValue("TestSection", "TestKey", "UpdatedValue");

        // Assert
        Assert.True(result);
        string value = _iniSharp.GetValue("TestSection", "TestKey");
        Assert.Equal("UpdatedValue", value);
    }

    [Fact]
    public void GetValue_NonExistentKey_ReturnsNull()
    {
        // Act
        string value = _iniSharp.GetValue("NonExistentSection", "NonExistentKey");

        // Assert
        Assert.Null(value);
    }

    [Fact]
    public void GetValue_WithDefaultValue_ReturnsDefaultWhenNotFound()
    {
        // Act
        string value = _iniSharp.GetValue("NonExistentSection", "NonExistentKey", "DefaultValue");

        // Assert
        Assert.Equal("DefaultValue", value);
    }

    [Fact]
    public void GetValue_WithDefaultValue_CreatesKeyWhenNotFound()
    {
        // Act
        string value = _iniSharp.GetValue("NewSection", "NewKey", "DefaultValue");

        // Assert
        Assert.Equal("DefaultValue", value);
        
        // Verify the key was created
        string verifyValue = _iniSharp.GetValue("NewSection", "NewKey");
        Assert.Equal("DefaultValue", verifyValue);
    }

    [Fact]
    public void GetSections_ReturnsCreatedSections()
    {
        // Arrange
        _iniSharp.SetValue("Section1", "Key1", "Value1");
        _iniSharp.SetValue("Section2", "Key2", "Value2");
        _iniSharp.SetValue("Section3", "Key3", "Value3");

        // Act
        var sections = _iniSharp.GetSections();

        // Assert
        Assert.Contains("Section1", sections);
        Assert.Contains("Section2", sections);
        Assert.Contains("Section3", sections);
        Assert.Equal(3, sections.Count);
    }

    [Fact]
    public void GetKeys_ReturnsKeysInSection()
    {
        // Arrange
        _iniSharp.SetValue("TestSection", "Key1", "Value1");
        _iniSharp.SetValue("TestSection", "Key2", "Value2");
        _iniSharp.SetValue("TestSection", "Key3", "Value3");

        // Act
        var keys = _iniSharp.GetKeys("TestSection");

        // Assert
        Assert.Contains("Key1", keys);
        Assert.Contains("Key2", keys);
        Assert.Contains("Key3", keys);
        Assert.Equal(3, keys.Count);
    }

    [Fact]
    public void GetKeys_NonExistentSection_ReturnsEmptyList()
    {
        // Act
        var keys = _iniSharp.GetKeys("NonExistentSection");

        // Assert
        Assert.Empty(keys);
    }

    [Fact]
    public void SetValue_MultipleKeysInSameSection_WorksCorrectly()
    {
        // Act
        _iniSharp.SetValue("TestSection", "Key1", "Value1");
        _iniSharp.SetValue("TestSection", "Key2", "Value2");
        _iniSharp.SetValue("TestSection", "Key3", "Value3");

        // Assert
        Assert.Equal("Value1", _iniSharp.GetValue("TestSection", "Key1"));
        Assert.Equal("Value2", _iniSharp.GetValue("TestSection", "Key2"));
        Assert.Equal("Value3", _iniSharp.GetValue("TestSection", "Key3"));

        var keys = _iniSharp.GetKeys("TestSection");
        Assert.Equal(3, keys.Count);
    }

    [Fact]
    public void SetValue_MultipleSections_WorksCorrectly()
    {
        // Act
        _iniSharp.SetValue("Section1", "Key1", "Value1");
        _iniSharp.SetValue("Section2", "Key2", "Value2");
        _iniSharp.SetValue("Section3", "Key3", "Value3");

        // Assert
        Assert.Equal("Value1", _iniSharp.GetValue("Section1", "Key1"));
        Assert.Equal("Value2", _iniSharp.GetValue("Section2", "Key2"));
        Assert.Equal("Value3", _iniSharp.GetValue("Section3", "Key3"));

        var sections = _iniSharp.GetSections();
        Assert.Equal(3, sections.Count);
    }

    [Fact]
    public void SetValue_EmptyValue_WorksCorrectly()
    {
        // Act
        bool result = _iniSharp.SetValue("TestSection", "EmptyKey", "");

        // Assert
        Assert.True(result);
        string value = _iniSharp.GetValue("TestSection", "EmptyKey");
        Assert.Equal("", value);
    }

    [Fact]
    public void SetValue_ValueWithSpaces_PreservesSpaces()
    {
        // Arrange
        string valueWithSpaces = "  Value with spaces  ";

        // Act
        bool result = _iniSharp.SetValue("TestSection", "SpaceKey", valueWithSpaces);

        // Assert
        Assert.True(result);
        string value = _iniSharp.GetValue("TestSection", "SpaceKey");
        Assert.Equal(valueWithSpaces, value);
    }

    [Fact]
    public void GetValue_CaseInsensitive_WorksCorrectly()
    {
        // Arrange
        _iniSharp.SetValue("TestSection", "TestKey", "TestValue");

        // Act
        string value1 = _iniSharp.GetValue("testsection", "testkey");
        string value2 = _iniSharp.GetValue("TESTSECTION", "TESTKEY");

        // Assert
        Assert.Equal("TestValue", value1);
        Assert.Equal("TestValue", value2);
    }

    [Fact]
    public void SetValue_ValueWithEquals_WorksCorrectly()
    {
        // Arrange
        string valueWithEquals = "key=value=another";

        // Act
        bool result = _iniSharp.SetValue("TestSection", "EqualsKey", valueWithEquals);

        // Assert
        Assert.True(result);
        string value = _iniSharp.GetValue("TestSection", "EqualsKey");
        Assert.Equal(valueWithEquals, value);
    }

    [Fact]
    public void FileEncoding_PropertyIsSet()
    {
        // Act
        var encoding = _iniSharp.FileEncoding;

        // Assert
        Assert.NotNull(encoding);
    }

    [Fact]
    public void FileContent_ManualVerification_IsCorrect()
    {
        // Arrange
        _iniSharp.SetValue("Application", "Name", "MyApp");
        _iniSharp.SetValue("Application", "Version", "1.0");
        _iniSharp.SetValue("Database", "Host", "localhost");

        // Act
        string content = File.ReadAllText(_testFilePath);

        // Assert
        Assert.Contains("[Application]", content);
        Assert.Contains("[Database]", content);
        Assert.Contains("Name=MyApp", content);
        Assert.Contains("Version=1.0", content);
        Assert.Contains("Host=localhost", content);
    }

    [Fact]
    public void Constructor_WithCustomCommentChar_WorksCorrectly()
    {
        // Arrange
        string customFile = Path.Combine(Path.GetTempPath(), $"custom_{Guid.NewGuid()}.ini");
        var customIni = new IniFileSharp.IniSharp(customFile, ';');

        try
        {
            // Act
            customIni.SetValue("TestSection", "TestKey", "TestValue");
            string value = customIni.GetValue("TestSection", "TestKey");

            // Assert
            Assert.Equal("TestValue", value);
        }
        finally
        {
            if (File.Exists(customFile))
                File.Delete(customFile);
        }
    }

    [Fact]
    public void Constructor_WithCustomEncoding_WorksCorrectly()
    {
        // Arrange
        string customFile = Path.Combine(Path.GetTempPath(), $"encoding_{Guid.NewGuid()}.ini");
        var customIni = new IniFileSharp.IniSharp(customFile, Encoding.UTF8);

        try
        {
            // Act
            customIni.SetValue("TestSection", "TestKey", "TestValue");
            string value = customIni.GetValue("TestSection", "TestKey");

            // Assert
            Assert.Equal("TestValue", value);
            Assert.Equal(Encoding.UTF8, customIni.FileEncoding);
        }
        finally
        {
            if (File.Exists(customFile))
                File.Delete(customFile);
        }
    }

    [Fact]
    public void ArgumentValidation_NullSection_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _iniSharp.GetValue(null!, "TestKey"));
        Assert.Throws<ArgumentException>(() => _iniSharp.SetValue(null!, "TestKey", "TestValue"));
    }

    [Fact]
    public void ArgumentValidation_EmptySection_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _iniSharp.GetValue("", "TestKey"));
        Assert.Throws<ArgumentException>(() => _iniSharp.SetValue("", "TestKey", "TestValue"));
    }

    [Fact]
    public void ArgumentValidation_NullKey_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _iniSharp.GetValue("TestSection", null!));
        Assert.Throws<ArgumentException>(() => _iniSharp.SetValue("TestSection", null!, "TestValue"));
    }

    [Fact]
    public void ArgumentValidation_EmptyKey_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _iniSharp.GetValue("TestSection", ""));
        Assert.Throws<ArgumentException>(() => _iniSharp.SetValue("TestSection", "", "TestValue"));
    }

    [Fact]
    public void ArgumentValidation_NullValue_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _iniSharp.SetValue("TestSection", "TestKey", null!));
    }

    [Fact]
    public void PersistenceTest_DataSurvivesRestart()
    {
        // Arrange
        _iniSharp.SetValue("Persistence", "Key1", "Value1");
        _iniSharp.SetValue("Persistence", "Key2", "Value2");

        // Create new instance (simulating restart)
        var newIniSharp = new IniFileSharp.IniSharp(_testFilePath);

        // Act & Assert
        Assert.Equal("Value1", newIniSharp.GetValue("Persistence", "Key1"));
        Assert.Equal("Value2", newIniSharp.GetValue("Persistence", "Key2"));

        var sections = newIniSharp.GetSections();
        Assert.Contains("Persistence", sections);

        var keys = newIniSharp.GetKeys("Persistence");
        Assert.Contains("Key1", keys);
        Assert.Contains("Key2", keys);
    }
}
