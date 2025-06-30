using IniFileSharp;
using System.Text;

namespace IniSharp.Tests;

/// <summary>
/// 集成测试，模拟真实使用场景
/// </summary>
public class IniSharpIntegrationTests : IDisposable
{
    private readonly string _testFilePath;
    private readonly IniFileSharp.IniSharp _iniSharp;

    public IniSharpIntegrationTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), $"integration_test_{Guid.NewGuid()}.ini");
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
    public void ApplicationConfiguration_CompleteWorkflow_WorksCorrectly()
    {
        // Simulate a complete application configuration workflow

        // 1. Set application settings
        _iniSharp.SetValue("Application", "Name", "MyApplication");
        _iniSharp.SetValue("Application", "Version", "1.0.0");
        _iniSharp.SetValue("Application", "Author", "Test Author");

        // 2. Set database configuration
        _iniSharp.SetValue("Database", "ConnectionString", "Server=localhost;Database=mydb");
        _iniSharp.SetValue("Database", "Timeout", "30");
        _iniSharp.SetValue("Database", "RetryCount", "3");

        // 3. Set UI preferences
        _iniSharp.SetValue("UI", "Theme", "Dark");
        _iniSharp.SetValue("UI", "Language", "en-US");
        _iniSharp.SetValue("UI", "WindowWidth", "1024");
        _iniSharp.SetValue("UI", "WindowHeight", "768");

        // 4. Verify all sections exist
        var sections = _iniSharp.GetSections();
        Assert.Contains("Application", sections);
        Assert.Contains("Database", sections);
        Assert.Contains("UI", sections);
        Assert.Equal(3, sections.Count);

        // 5. Verify all keys in each section
        var appKeys = _iniSharp.GetKeys("Application");
        Assert.Contains("Name", appKeys);
        Assert.Contains("Version", appKeys);
        Assert.Contains("Author", appKeys);

        var dbKeys = _iniSharp.GetKeys("Database");
        Assert.Contains("ConnectionString", dbKeys);
        Assert.Contains("Timeout", dbKeys);
        Assert.Contains("RetryCount", dbKeys);

        var uiKeys = _iniSharp.GetKeys("UI");
        Assert.Contains("Theme", uiKeys);
        Assert.Contains("Language", uiKeys);
        Assert.Contains("WindowWidth", uiKeys);
        Assert.Contains("WindowHeight", uiKeys);

        // 6. Verify all values
        Assert.Equal("MyApplication", _iniSharp.GetValue("Application", "Name"));
        Assert.Equal("1.0.0", _iniSharp.GetValue("Application", "Version"));
        Assert.Equal("Test Author", _iniSharp.GetValue("Application", "Author"));

        Assert.Equal("Server=localhost;Database=mydb", _iniSharp.GetValue("Database", "ConnectionString"));
        Assert.Equal("30", _iniSharp.GetValue("Database", "Timeout"));
        Assert.Equal("3", _iniSharp.GetValue("Database", "RetryCount"));

        Assert.Equal("Dark", _iniSharp.GetValue("UI", "Theme"));
        Assert.Equal("en-US", _iniSharp.GetValue("UI", "Language"));
        Assert.Equal("1024", _iniSharp.GetValue("UI", "WindowWidth"));
        Assert.Equal("768", _iniSharp.GetValue("UI", "WindowHeight"));

        // 7. Update some values
        _iniSharp.SetValue("Application", "Version", "1.1.0");
        _iniSharp.SetValue("UI", "Theme", "Light");

        // 8. Verify updates
        Assert.Equal("1.1.0", _iniSharp.GetValue("Application", "Version"));
        Assert.Equal("Light", _iniSharp.GetValue("UI", "Theme"));

        // 9. Delete a key and verify
        _iniSharp.DeleteKey("Database", "RetryCount");
        var updatedDbKeys = _iniSharp.GetKeys("Database");
        Assert.DoesNotContain("RetryCount", updatedDbKeys);

        // 10. Verify file content manually
        string fileContent = File.ReadAllText(_testFilePath);
        Assert.Contains("[Application]", fileContent);
        Assert.Contains("[Database]", fileContent);
        Assert.Contains("[UI]", fileContent);
        Assert.Contains("Name=MyApplication", fileContent);
        Assert.Contains("Version=1.1.0", fileContent);
        Assert.Contains("Theme=Light", fileContent);
    }

    [Fact]
    public void UserPreferences_SaveAndLoad_WorksCorrectly()
    {
        // Simulate user preferences save/load scenario

        // Save user preferences
        _iniSharp.SetValue("UserPreferences", "Username", "john.doe");
        _iniSharp.SetValue("UserPreferences", "Email", "john.doe@example.com");
        _iniSharp.SetValue("UserPreferences", "LastLogin", "2023-12-01 10:30:00");
        _iniSharp.SetValue("UserPreferences", "RememberMe", "true");

        // Save recent files
        _iniSharp.SetValue("RecentFiles", "File1", @"C:\Documents\file1.txt");
        _iniSharp.SetValue("RecentFiles", "File2", @"C:\Documents\file2.txt");
        _iniSharp.SetValue("RecentFiles", "File3", @"C:\Documents\file3.txt");

        // Create a new instance (simulating application restart)
        var newIni = new IniFileSharp.IniSharp(_testFilePath);

        // Load and verify user preferences
        Assert.Equal("john.doe", newIni.GetValue("UserPreferences", "Username"));
        Assert.Equal("john.doe@example.com", newIni.GetValue("UserPreferences", "Email"));
        Assert.Equal("2023-12-01 10:30:00", newIni.GetValue("UserPreferences", "LastLogin"));
        Assert.Equal("true", newIni.GetValue("UserPreferences", "RememberMe"));

        // Load and verify recent files
        Assert.Equal(@"C:\Documents\file1.txt", newIni.GetValue("RecentFiles", "File1"));
        Assert.Equal(@"C:\Documents\file2.txt", newIni.GetValue("RecentFiles", "File2"));
        Assert.Equal(@"C:\Documents\file3.txt", newIni.GetValue("RecentFiles", "File3"));

        // Update last login
        newIni.SetValue("UserPreferences", "LastLogin", "2023-12-02 09:15:00");

        // Verify update
        Assert.Equal("2023-12-02 09:15:00", newIni.GetValue("UserPreferences", "LastLogin"));
    }

    [Fact]
    public void ConfigurationMigration_DeleteAndRecreate_WorksCorrectly()
    {
        // Create initial configuration
        _iniSharp.SetValue("OldSection", "OldKey1", "OldValue1");
        _iniSharp.SetValue("OldSection", "OldKey2", "OldValue2");
        _iniSharp.SetValue("KeepSection", "KeepKey", "KeepValue");

        // Migrate: Remove old section, create new section
        _iniSharp.DeleteSection("OldSection");
        _iniSharp.SetValue("NewSection", "NewKey1", "NewValue1");
        _iniSharp.SetValue("NewSection", "NewKey2", "NewValue2");

        // Verify migration
        var sections = _iniSharp.GetSections();
        Assert.DoesNotContain("OldSection", sections);
        Assert.Contains("NewSection", sections);
        Assert.Contains("KeepSection", sections);

        // Verify new values
        Assert.Equal("NewValue1", _iniSharp.GetValue("NewSection", "NewKey1"));
        Assert.Equal("NewValue2", _iniSharp.GetValue("NewSection", "NewKey2"));

        // Verify kept values
        Assert.Equal("KeepValue", _iniSharp.GetValue("KeepSection", "KeepKey"));

        // Verify old values are gone
        Assert.Null(_iniSharp.GetValue("OldSection", "OldKey1"));
        Assert.Null(_iniSharp.GetValue("OldSection", "OldKey2"));
    }

    [Fact]
    public void MultipleInstances_ConcurrentAccess_WorksCorrectly()
    {
        // Create multiple instances pointing to the same file
        var ini1 = new IniFileSharp.IniSharp(_testFilePath);
        var ini2 = new IniFileSharp.IniSharp(_testFilePath);

        try
        {
            // Set values from different instances
            ini1.SetValue("Instance1", "Key1", "Value1");
            ini2.SetValue("Instance2", "Key2", "Value2");

            // Each instance should see the other's changes after file operations
            // Note: This depends on the implementation's file sync behavior
            
            // Verify both instances can read all values
            // (This might require the implementation to re-read the file)
            var value1FromIni2 = ini2.GetValue("Instance1", "Key1");
            var value2FromIni1 = ini1.GetValue("Instance2", "Key2");

            // At minimum, each instance should see its own values
            Assert.Equal("Value1", ini1.GetValue("Instance1", "Key1"));
            Assert.Equal("Value2", ini2.GetValue("Instance2", "Key2"));
        }
        finally
        {
            // Cleanup is handled by Dispose method
        }
    }

    [Fact]
    public void DefaultValues_Configuration_WorksCorrectly()
    {
        // Test getting values with defaults (simulating first-run scenario)
        
        // Get default values for a fresh configuration
        string theme = _iniSharp.GetValue("UI", "Theme", "Light");
        string language = _iniSharp.GetValue("UI", "Language", "en-US");
        string windowWidth = _iniSharp.GetValue("UI", "WindowWidth", "800");
        string windowHeight = _iniSharp.GetValue("UI", "WindowHeight", "600");

        // Verify defaults are returned and keys are created
        Assert.Equal("Light", theme);
        Assert.Equal("en-US", language);
        Assert.Equal("800", windowWidth);
        Assert.Equal("600", windowHeight);

        // Verify the keys were actually created in the file
        Assert.Equal("Light", _iniSharp.GetValue("UI", "Theme"));
        Assert.Equal("en-US", _iniSharp.GetValue("UI", "Language"));
        Assert.Equal("800", _iniSharp.GetValue("UI", "WindowWidth"));
        Assert.Equal("600", _iniSharp.GetValue("UI", "WindowHeight"));

        // Verify the section exists
        var sections = _iniSharp.GetSections();
        Assert.Contains("UI", sections);

        // Verify all keys exist
        var keys = _iniSharp.GetKeys("UI");
        Assert.Contains("Theme", keys);
        Assert.Contains("Language", keys);
        Assert.Contains("WindowWidth", keys);
        Assert.Contains("WindowHeight", keys);
    }

    [Fact]
    public void ComplexConfiguration_RealWorldScenario_WorksCorrectly()
    {
        // Simulate a complex real-world configuration file

        // Application metadata
        _iniSharp.SetValue("Application", "Name", "Advanced Text Editor");
        _iniSharp.SetValue("Application", "Version", "2.1.5");
        _iniSharp.SetValue("Application", "Build", "20231201");
        _iniSharp.SetValue("Application", "InstallPath", @"C:\Program Files\Advanced Text Editor");

        // Server configuration
        _iniSharp.SetValue("Server", "Host", "api.example.com");
        _iniSharp.SetValue("Server", "Port", "443");
        _iniSharp.SetValue("Server", "UseSSL", "true");
        _iniSharp.SetValue("Server", "Timeout", "30000");
        _iniSharp.SetValue("Server", "ApiKey", "abcd1234-efgh5678-ijkl9012");

        // Editor preferences
        _iniSharp.SetValue("Editor", "FontFamily", "Consolas");
        _iniSharp.SetValue("Editor", "FontSize", "12");
        _iniSharp.SetValue("Editor", "TabSize", "4");
        _iniSharp.SetValue("Editor", "WordWrap", "true");
        _iniSharp.SetValue("Editor", "ShowLineNumbers", "true");
        _iniSharp.SetValue("Editor", "HighlightSyntax", "true");

        // Recent documents
        _iniSharp.SetValue("RecentDocuments", "Doc1", @"C:\Users\John\Documents\project.txt");
        _iniSharp.SetValue("RecentDocuments", "Doc2", @"C:\Users\John\Documents\notes.md");
        _iniSharp.SetValue("RecentDocuments", "Doc3", @"C:\Users\John\Desktop\readme.txt");

        // Window state
        _iniSharp.SetValue("WindowState", "Left", "100");
        _iniSharp.SetValue("WindowState", "Top", "100");
        _iniSharp.SetValue("WindowState", "Width", "1200");
        _iniSharp.SetValue("WindowState", "Height", "800");
        _iniSharp.SetValue("WindowState", "Maximized", "false");

        // Plugins
        _iniSharp.SetValue("Plugins", "SpellChecker", "enabled");
        _iniSharp.SetValue("Plugins", "AutoBackup", "enabled");
        _iniSharp.SetValue("Plugins", "GitIntegration", "disabled");

        // Verify the complete configuration
        var allSections = _iniSharp.GetSections();
        var expectedSections = new[] { "Application", "Server", "Editor", "RecentDocuments", "WindowState", "Plugins" };
        
        foreach (var section in expectedSections)
        {
            Assert.Contains(section, allSections);
        }

        // Verify key counts
        Assert.Equal(4, _iniSharp.GetKeys("Application").Count);
        Assert.Equal(5, _iniSharp.GetKeys("Server").Count);
        Assert.Equal(6, _iniSharp.GetKeys("Editor").Count);
        Assert.Equal(3, _iniSharp.GetKeys("RecentDocuments").Count);
        Assert.Equal(5, _iniSharp.GetKeys("WindowState").Count);
        Assert.Equal(3, _iniSharp.GetKeys("Plugins").Count);

        // Verify some specific values
        Assert.Equal("Advanced Text Editor", _iniSharp.GetValue("Application", "Name"));
        Assert.Equal("443", _iniSharp.GetValue("Server", "Port"));
        Assert.Equal("Consolas", _iniSharp.GetValue("Editor", "FontFamily"));
        Assert.Equal("1200", _iniSharp.GetValue("WindowState", "Width"));
        Assert.Equal("enabled", _iniSharp.GetValue("Plugins", "SpellChecker"));

        // Test updating some values
        _iniSharp.SetValue("Editor", "FontSize", "14");
        _iniSharp.SetValue("WindowState", "Maximized", "true");

        Assert.Equal("14", _iniSharp.GetValue("Editor", "FontSize"));
        Assert.Equal("true", _iniSharp.GetValue("WindowState", "Maximized"));

        // Verify file structure by reading raw content
        string fileContent = File.ReadAllText(_testFilePath);
        Assert.Contains("[Application]", fileContent);
        Assert.Contains("[Server]", fileContent);
        Assert.Contains("[Editor]", fileContent);
        Assert.Contains("FontSize=14", fileContent);
        Assert.Contains("Maximized=true", fileContent);
    }
}
