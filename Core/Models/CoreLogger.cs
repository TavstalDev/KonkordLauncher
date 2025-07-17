namespace Tavstal.KonkordLauncher.Core.Models;

/// <summary>
/// A logger class for handling console and debug logging with support for module-specific logging.
/// </summary>
public class CoreLogger
{
    private readonly string _moduleName; // The name of the module associated with the logger.
    private readonly bool _isDebug; // Indicates whether debug logging is enabled.

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreLogger"/> class with a module name and debug flag.
    /// </summary>
    /// <param name="moduleName">The name of the module.</param>
    /// <param name="isDebug">Whether debug logging is enabled.</param>
    public CoreLogger(string moduleName, bool isDebug)
    {
        _moduleName = moduleName;
        _isDebug = isDebug;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreLogger"/> class using a module type and debug flag.
    /// </summary>
    /// <param name="moduleType">The type of the module.</param>
    /// <param name="isDebug">Whether debug logging is enabled.</param>
    public CoreLogger(Type moduleType, bool isDebug) : this(moduleType.Name, isDebug) { }

    /// <summary>
    /// Creates a new <see cref="CoreLogger"/> instance with a specified module name.
    /// </summary>
    /// <param name="moduleName">The name of the module.</param>
    /// <param name="isDebug">Whether debug logging is enabled (default is false).</param>
    /// <returns>A new <see cref="CoreLogger"/> instance.</returns>
    // ReSharper disable once MethodOverloadWithOptionalParameter
    public static CoreLogger WithModuleName(string moduleName, bool isDebug = false)
    {
        return new CoreLogger(moduleName, isDebug);
    }

    /// <summary>
    /// Creates a new <see cref="CoreLogger"/> instance with a specified module type.
    /// </summary>
    /// <param name="moduleType">The type of the module.</param>
    /// <param name="isDebug">Whether debug logging is enabled (default is false).</param>
    /// <returns>A new <see cref="CoreLogger"/> instance.</returns>
    public static CoreLogger WithModuleType(Type moduleType, bool isDebug = false)
    {
        return new CoreLogger(moduleType.Name, isDebug);
    }

    /// <summary>
    /// Creates a new <see cref="CoreLogger"/> instance with a new module name, retaining the current debug flag.
    /// </summary>
    /// <param name="moduleName">The new module name.</param>
    /// <returns>A new <see cref="CoreLogger"/> instance.</returns>
    public CoreLogger WithModuleName(string moduleName)
    {
        return new CoreLogger(moduleName, _isDebug);
    }

    /// <summary>
    /// Creates a new <see cref="CoreLogger"/> instance with a new module type, retaining the current debug flag.
    /// </summary>
    /// <param name="moduleType">The new module type.</param>
    /// <returns>A new <see cref="CoreLogger"/> instance.</returns>
    public CoreLogger WithModuleeType(Type moduleType)
    {
        return new CoreLogger(moduleType.Name, _isDebug);
    }

    /// <summary>
    /// Logs a message to the console with a specified color and optional prefix.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="color">The console color for the message (default is white).</param>
    /// <param name="prefix">An optional prefix for the message.</param>
    public void Log(object message, ConsoleColor color = ConsoleColor.White, string prefix = "")
    {
        string text = $"{prefix}{message}";
        if (!string.IsNullOrEmpty(_moduleName))
            text = $"[{_moduleName}] {text}";

        // TODO: Save to file
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            // If console output fails, fallback to Debug.WriteLine
            System.Diagnostics.Debug.WriteLine($"{prefix} {message}");
            System.Diagnostics.Debug.WriteLine($"Error logging message: {ex.Message}");
        }
        finally
        {
            // Ensure the console color is reset
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Logs an informational message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="color">The console color for the message (default is dark cyan).</param>
    public void Info(object message, ConsoleColor color = ConsoleColor.DarkCyan)
    {
        Log(message, color, "[INFO] : ");
    }

    /// <summary>
    /// Logs a success message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="color">The console color for the message (default is green).</param>
    public void Ok(object message, ConsoleColor color = ConsoleColor.Green)
    {
        Log(message, color, "[OK] : ");
    }

    /// <summary>
    /// Logs a warning message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="color">The console color for the message (default is yellow).</param>
    public void Warn(object message, ConsoleColor color = ConsoleColor.Yellow)
    {
        Log(message, color, "[WARNING] : ");
    }

    /// <summary>
    /// Logs an exception message to the console.
    /// </summary>
    /// <param name="message">The exception message to log.</param>
    /// <param name="color">The console color for the message (default is dark yellow).</param>
    public void Exc(object message, ConsoleColor color = ConsoleColor.DarkYellow)
    {
        Log(message, color, "[EXCEPTION] : ");
    }

    /// <summary>
    /// Logs an error message to the console.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <param name="color">The console color for the message (default is red).</param>
    public void Error(object message, ConsoleColor color = ConsoleColor.Red)
    {
        Log(message, color, "[ERROR] : ");
    }

    /// <summary>
    /// Logs a debug message to the console if debug logging is enabled.
    /// </summary>
    /// <param name="message">The debug message to log.</param>
    /// <param name="color">The console color for the message (default is magenta).</param>
    public void Debug(object message, ConsoleColor color = ConsoleColor.Magenta)
    {
        if (!_isDebug)
            return;
        Log(message, color, "[DEBUG] : ");
    }
}