using Nterm.Core.Buffer;

namespace Nterm.Core.Controls;

public record FilePickerOptions
{
    public Color DirectoryColor { get; init; } = Color.Beige;
    public int NumberOfVisibleItems { get; init; } = 4;
    public string[] FileExtensions { get; init; } = [];
}

public static class FilePicker
{
    public static TextItem<FileSystemInfo> Show(
        string? startDirectory = null,
        FilePickerOptions? options = default
    )
    {
        options ??= new FilePickerOptions();
        return new FilePickerControl()
        {
            DirectoryColor = options.DirectoryColor,
            FileExtensions = options.FileExtensions
        }.Show(startDirectory, options.NumberOfVisibleItems);
    }
}

/// <summary>
/// Interactive file/directory picker built on top of the select dropdown view.
/// - Lists contents of a directory (current directory by default)
/// - Selecting a directory drills into it and refreshes the list
/// - The directory header appears at the top; selecting it returns the directory itself
/// - Selecting a file writes its name to the terminal and returns the file item
/// </summary>
public class FilePickerControl
{
    public Color DirectoryColor { get; init; } = Color.Beige;

    public string[] FileExtensions { get; init; } = [];

    private const string CurrentDir = ".";
    private const string ParentDir = "..";

    /// <summary>
    /// Shows a file/directory picker rooted at <paramref name="startDirectory"/> or the current directory.
    /// </summary>
    /// <param name="startDirectory">Directory to start from; defaults to Environment.CurrentDirectory.</param>
    /// <param name="numberOfVisibleItems">Maximum number of items to render below the anchor.</param>
    /// <returns>The selected item, whose value is a <see cref="FileSystemInfo"/>.</returns>
    public TextItem<FileSystemInfo> Show(
        string? startDirectory = null,
        int numberOfVisibleItems = 4
    )
    {
        using TerminalState terminalState = new();

        string startRoot = ResolveStartDirectory(startDirectory);
        string currentDirectoryPath = startRoot;
        SelectDropdownView<FileSystemInfo> view =
            new(terminalState.OriginalCursorLeft, terminalState.OriginalCursorTop);

        while (true)
        {
            IEnumerable<TextItem<FileSystemInfo>> items = BuildDirectoryItems(
                startRoot,
                currentDirectoryPath
            );

            TextItem<FileSystemInfo> selected = view.Show(
                [.. items],
                numberOfVisibleItems,
                enableFilter: true
            );

            if (selected.IsEmpty())
            {
                // Cancelled
                return TextItem.Empty<FileSystemInfo>();
            }

            if (selected.Value is DirectoryInfo dirInfo)
            {
                // If user selected the directory header (first item), return it and write its name.
                // We detect header by comparing Text to the display name for the directory and by position 0.
                // Since SelectDropdownView returns the selected item, we can consider header by matching text.

                if (dirInfo.FullName == currentDirectoryPath)
                {
                    return selected;
                }

                // Otherwise drill into the directory and continue the loop
                currentDirectoryPath = dirInfo.FullName;
                continue;
            }

            // File selected: return
            return selected;
        }
    }

    private static string ResolveStartDirectory(string? startDirectory)
    {
        if (string.IsNullOrWhiteSpace(startDirectory))
            return Environment.CurrentDirectory;

        try
        {
            return Directory.Exists(startDirectory)
                ? Path.GetFullPath(startDirectory)
                : Environment.CurrentDirectory;
        }
        catch
        {
            return Environment.CurrentDirectory;
        }
    }

    private IEnumerable<TextItem<FileSystemInfo>> BuildDirectoryItems(
        string startRoot,
        string directoryPath
    )
    {
        DirectoryInfo dirInfo = new(directoryPath);

        // Header item: selecting it returns the directory itself
        yield return new TextItem<FileSystemInfo>
        {
            Text = new TextBuffer($"📁 {CurrentDir}", DirectoryColor),
            Description = dirInfo.Name.Length == 0 ? dirInfo.FullName : dirInfo.Name,
            Value = dirInfo
        };

        // Parent directory item: selecting it navigates up
        if (dirInfo.Parent is not null)
        {
            yield return new TextItem<FileSystemInfo>
            {
                Text = new TextBuffer($"📁 {ParentDir}", DirectoryColor),
                Description = GetPathDescriptor(startRoot, dirInfo.Parent.FullName),
                Value = dirInfo.Parent
            };
        }

        // Directories first
        foreach (DirectoryInfo subDir in SafeEnumerateDirectories(dirInfo))
        {
            yield return new TextItem<FileSystemInfo>
            {
                Text = new($"📁 {subDir.Name}/", DirectoryColor),
                Description = GetPathDescriptor(startRoot, subDir.FullName),
                Value = subDir
            };
        }

        // Then files
        foreach (FileInfo file in SafeEnumerateFiles(dirInfo))
        {
            yield return new TextItem<FileSystemInfo>
            {
                Text = file.Name,
                Description = GetPathDescriptor(
                    startRoot,
                    file.Directory?.FullName ?? dirInfo.FullName
                ),
                Value = file
            };
        }
    }

    private static string GetPathDescriptor(string root, string path)
    {
        try
        {
            string rel = Path.GetRelativePath(root, path);
            if (
                string.IsNullOrEmpty(rel)
                || rel == CurrentDir
                || rel.StartsWith(ParentDir, StringComparison.Ordinal)
            )
            {
                return path;
            }

            return rel;
        }
        catch
        {
            return path;
        }
    }

    private static DirectoryInfo[] SafeEnumerateDirectories(DirectoryInfo dir)
    {
        try
        {
            return
            [
                .. dir.EnumerateDirectories().OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
            ];
        }
        catch
        {
            return [];
        }
    }

    private FileInfo[] SafeEnumerateFiles(DirectoryInfo dir)
    {
        try
        {
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            if (FileExtensions.Length > 0)
            {
                files = files.Where(f => FileExtensions.Contains(f.Extension));
            }
            return [.. files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)];
        }
        catch
        {
            return [];
        }
    }
}
