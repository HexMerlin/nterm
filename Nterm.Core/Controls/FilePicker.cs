using Nterm.Core.Buffer;

namespace Nterm.Core.Controls;

public record FilePickerOptions
{
    public Color DirectoryColor { get; init; } = Color.Beige;
    public int NumberOfVisibleItems { get; init; } = 4;
    public string[] FileExtensions { get; init; } = [];

    public bool AllowNavigationAboveStartDirectory { get; init; }

    public bool ShowOnlyFiles { get; init; }
    public bool ShowOnlyDirectories { get; init; }

    public bool ShowHiddenFilesAndDirectories { get; init; }

    public bool FlattenDirectories { get; init; }

    public bool FilterOnDescription { get; init; } = true;
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
            FileExtensions = options.FileExtensions,
            ShowOnlyFiles = options.ShowOnlyFiles,
            ShowOnlyDirectories = options.ShowOnlyDirectories,
            ShowHiddenFilesAndDirectories = options.ShowHiddenFilesAndDirectories,
            AllowNavigationAboveStartDirectory = options.AllowNavigationAboveStartDirectory,
            FlattenDirectories = options.FlattenDirectories,
            FilterOnDescription = options.FilterOnDescription
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

    public bool FilterOnDescription { get; init; } = true;

    public string[] FileExtensions { get; init; } = [];

    public bool ShowOnlyFiles { get; init; }

    public bool ShowOnlyDirectories { get; init; }

    public bool ShowHiddenFilesAndDirectories { get; init; }

    /// <summary>
    /// If true, the user can navigate above the start directory.
    /// </summary>
    /// <remarks>
    /// Cannot be true if <see cref="FlattenDirectories"/> is true.
    /// </remarks>
    public bool AllowNavigationAboveStartDirectory
    {
        get;
        init
        {
            if (value && FlattenDirectories)
            {
                throw new InvalidOperationException(
                    "AllowNavigationAboveStartDirectory and FlattenDirectories cannot be true at the same time."
                );
            }

            field = value;
        }
    }

    /// <summary>
    /// If true, the user can flatten directories and display them as files.
    /// </summary>
    /// <remarks>
    /// Cannot be true if <see cref="AllowNavigationAboveStartDirectory"/> is true.
    /// </remarks>
    public bool FlattenDirectories
    {
        get;
        init
        {
            if (value && AllowNavigationAboveStartDirectory)
            {
                throw new InvalidOperationException(
                    "FlattenDirectories and AllowNavigationAboveStartDirectory cannot be true at the same time."
                );
            }

            field = value;
        }
    }

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
            new(terminalState.OriginalCursorLeft, terminalState.OriginalCursorTop)
            {
                FilterOnDescription = FilterOnDescription
            };

        while (true)
        {
            IEnumerable<TextItem<FileSystemInfo>> items = BuildDirectoryItems(
                startRoot,
                currentDirectoryPath
            );

            TextItem<FileSystemInfo> selected = view.Show([.. items], numberOfVisibleItems);

            if (selected.IsEmpty())
            {
                // Cancelled
                return TextItem.Empty<FileSystemInfo>();
            }

            if (selected.Value is DirectoryInfo dirInfo)
            {
                // If the file picker is flattened or
                // the user selected the directory root (first item) return the selected item
                if (FlattenDirectories || dirInfo.FullName == currentDirectoryPath)
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

        return Path.GetFullPath(startDirectory);
    }

    private List<DirectoryItem> BuildDirectoryItems(string startRoot, string directoryPath)
    {
        if (FlattenDirectories)
        {
            return DirectoryItem.ListDeep(
                directoryPath,
                new()
                {
                    FileExtensions = FileExtensions,
                    ShowOnlyFiles = ShowOnlyFiles,
                    ShowOnlyDirectories = ShowOnlyDirectories,
                    ShowHiddenFilesAndDirectories = ShowHiddenFilesAndDirectories,
                    DirectoryColor = DirectoryColor,
                    IncludeParentDirectoryAtStart =
                        AllowNavigationAboveStartDirectory || directoryPath != startRoot
                }
            );
        }

        return DirectoryItem.ListShallow(
            directoryPath,
            new()
            {
                FileExtensions = FileExtensions,
                ShowOnlyFiles = ShowOnlyFiles,
                ShowOnlyDirectories = ShowOnlyDirectories,
                ShowHiddenFilesAndDirectories = ShowHiddenFilesAndDirectories,
                DirectoryColor = DirectoryColor,
                IncludeParentDirectoryAtStart =
                    AllowNavigationAboveStartDirectory || directoryPath != startRoot
            }
        );
    }
}
