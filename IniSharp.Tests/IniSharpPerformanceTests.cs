using IniFileSharp;
using System.Diagnostics;
using System.Text;

namespace IniSharp.Tests;

/// <summary>
/// 性能测试
/// </summary>
public class IniSharpPerformanceTests : IDisposable
{
    private readonly string _testFilePath;
    private readonly IniFileSharp.IniSharp _iniSharp;

    public IniSharpPerformanceTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), $"perf_test_{Guid.NewGuid()}.ini");
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
    public void SetValue_ManyKeys_PerformsReasonably()
    {
        // Arrange
        const int keyCount = 100;
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < keyCount; i++)
        {
            _iniSharp.SetValue("TestSection", $"Key{i:D3}", $"Value{i:D3}");
        }

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Setting {keyCount} keys took {stopwatch.ElapsedMilliseconds}ms, which is too slow");
        
        // Verify all keys were set correctly
        var keys = _iniSharp.GetKeys("TestSection");
        Assert.Equal(keyCount, keys.Count);
    }

    [Fact]
    public void GetValue_ManyKeys_PerformsReasonably()
    {
        // Arrange
        const int keyCount = 100;
        
        // First, create the keys
        for (int i = 0; i < keyCount; i++)
        {
            _iniSharp.SetValue("TestSection", $"Key{i:D3}", $"Value{i:D3}");
        }

        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < keyCount; i++)
        {
            string value = _iniSharp.GetValue("TestSection", $"Key{i:D3}");
            Assert.Equal($"Value{i:D3}", value);
        }

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, $"Getting {keyCount} keys took {stopwatch.ElapsedMilliseconds}ms, which is too slow");
    }

    [Fact]
    public void SetValue_ManySections_PerformsReasonably()
    {
        // Arrange
        const int sectionCount = 50;
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < sectionCount; i++)
        {
            _iniSharp.SetValue($"Section{i:D2}", "Key1", "Value1");
            _iniSharp.SetValue($"Section{i:D2}", "Key2", "Value2");
        }

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, $"Creating {sectionCount} sections took {stopwatch.ElapsedMilliseconds}ms, which is too slow");
        
        // Verify all sections were created
        var sections = _iniSharp.GetSections();
        Assert.Equal(sectionCount, sections.Count);
    }

    [Fact]
    public void GetSections_LargeFile_PerformsReasonably()
    {
        // Arrange
        const int sectionCount = 50;
        
        // Create many sections
        for (int i = 0; i < sectionCount; i++)
        {
            _iniSharp.SetValue($"Section{i:D2}", "Key1", "Value1");
            _iniSharp.SetValue($"Section{i:D2}", "Key2", "Value2");
            _iniSharp.SetValue($"Section{i:D2}", "Key3", "Value3");
        }

        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 10; i++) // Call multiple times to test consistency
        {
            var sections = _iniSharp.GetSections();
            Assert.Equal(sectionCount, sections.Count);
        }

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Getting sections 10 times took {stopwatch.ElapsedMilliseconds}ms, which is too slow");
    }

    [Fact]
    public void GetKeys_LargeSection_PerformsReasonably()
    {
        // Arrange
        const int keyCount = 100;
        const string sectionName = "LargeSection";
        
        // Create many keys in one section
        for (int i = 0; i < keyCount; i++)
        {
            _iniSharp.SetValue(sectionName, $"Key{i:D3}", $"Value{i:D3}");
        }

        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 10; i++) // Call multiple times to test consistency
        {
            var keys = _iniSharp.GetKeys(sectionName);
            Assert.Equal(keyCount, keys.Count);
        }

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Getting keys 10 times took {stopwatch.ElapsedMilliseconds}ms, which is too slow");
    }

    [Fact]
    public void UpdateValue_ManyTimes_PerformsReasonably()
    {
        // Arrange
        const int updateCount = 100;
        _iniSharp.SetValue("TestSection", "TestKey", "InitialValue");

        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < updateCount; i++)
        {
            _iniSharp.SetValue("TestSection", "TestKey", $"UpdatedValue{i}");
        }

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, $"Updating a key {updateCount} times took {stopwatch.ElapsedMilliseconds}ms, which is too slow");
        
        // Verify final value
        string finalValue = _iniSharp.GetValue("TestSection", "TestKey");
        Assert.Equal($"UpdatedValue{updateCount - 1}", finalValue);
    }

    [Fact]
    public void LargeValue_SetAndGet_PerformsReasonably()
    {
        // Arrange
        const int valueSize = 10000; // 10KB string
        string largeValue = new string('A', valueSize);

        var stopwatch = Stopwatch.StartNew();

        // Act
        _iniSharp.SetValue("TestSection", "LargeKey", largeValue);
        string retrievedValue = _iniSharp.GetValue("TestSection", "LargeKey");

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Setting and getting a {valueSize} char value took {stopwatch.ElapsedMilliseconds}ms, which is too slow");
        Assert.Equal(largeValue, retrievedValue);
    }

    [Fact]
    public void MixedOperations_WorkLoad_PerformsReasonably()
    {
        // Arrange
        const int iterations = 50;
        var stopwatch = Stopwatch.StartNew();

        // Act - Simulate a mixed workload
        for (int i = 0; i < iterations; i++)
        {
            // Set some values
            _iniSharp.SetValue($"Section{i % 10}", $"Key{i}", $"Value{i}");
            
            // Get some values
            if (i > 0)
            {
                _iniSharp.GetValue($"Section{(i - 1) % 10}", $"Key{i - 1}");
            }
            
            // Get sections every 10 iterations
            if (i % 10 == 0)
            {
                _iniSharp.GetSections();
            }
            
            // Get keys every 5 iterations
            if (i % 5 == 0 && i > 0)
            {
                _iniSharp.GetKeys($"Section{(i - 1) % 10}");
            }
        }

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, $"Mixed workload of {iterations} operations took {stopwatch.ElapsedMilliseconds}ms, which is too slow");
    }

    [Fact]
    public void FileSize_AfterManyOperations_IsReasonable()
    {
        // Arrange
        const int keyCount = 100;

        // Act
        for (int i = 0; i < keyCount; i++)
        {
            _iniSharp.SetValue($"Section{i % 10}", $"Key{i:D3}", $"Value{i:D3}");
        }

        // Assert
        var fileInfo = new FileInfo(_testFilePath);
        Assert.True(fileInfo.Exists, "INI file should exist");
        Assert.True(fileInfo.Length > 0, "INI file should not be empty");
        Assert.True(fileInfo.Length < 100000, $"INI file is {fileInfo.Length} bytes, which seems too large for {keyCount} simple entries");
    }
}
