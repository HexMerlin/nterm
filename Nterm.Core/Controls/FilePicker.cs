using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Nterm.Core.Controls;

/// <summary>
/// Interactive file/directory picker built on top of the select dropdown view.
/// - Lists contents of a directory (current directory by default)
/// - Selecting a directory drills into it and refreshes the list
/// - The directory header appears at the top; selecting it returns the directory itself
/// - Selecting a file writes its name to the terminal and returns the file item
/// </summary>
public static class FilePicker
{
    /// <summary>
    /// Shows a file/directory picker rooted at <paramref name="startDirectory"/> or the current directory.
    /// </summary>
    /// <param name="startDirectory">Directory to start from; defaults to Environment.CurrentDirectory.</param>
    /// <param name="numberOfVisibleItems">Maximum number of items to render below the anchor.</param>
    /// <returns>The selected item, whose value is a <see cref="FileSystemInfo"/>.</returns>
    public static TextItem<FileSystemInfo> Show(
        string? startDirectory = null,
        int numberOfVisibleItems = 4
    )
    {
        using TerminalState terminalState = new();

        string currentDirectoryPath = ResolveStartDirectory(startDirectory);
        SelectDropdownView<FileSystemInfo> view =
            new(terminalState.OriginalCursorLeft, terminalState.OriginalCursorTop);

        while (true)
        {
            IReadOnlyList<TextItem<FileSystemInfo>> items = BuildDirectoryItems(
                currentDirectoryPath
            );

            TextItem<FileSystemInfo> selected = view.Show(
                items,
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
                bool isHeader = IsDirectoryHeader(selected, dirInfo);
                if (isHeader)
                {
                    RenderFinalSelection(selected.Text);
                    return selected;
                }

                // Otherwise drill into the directory and continue the loop
                currentDirectoryPath = dirInfo.FullName;
                continue;
            }

            // File selected: print and return
            RenderFinalSelection(selected.Text);
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

    private static bool IsDirectoryHeader(TextItem<FileSystemInfo> selected, DirectoryInfo dir)
    {
        // Directory header text equals the directory name (not full path), consistent with BuildDirectoryItems
        string expected = dir.Name.Length == 0 ? dir.FullName : dir.Name;
        return string.Equals(selected.Text, expected, StringComparison.Ordinal);
    }

    private static IReadOnlyList<TextItem<FileSystemInfo>> BuildDirectoryItems(string directoryPath)
    {
        var items = new List<TextItem<FileSystemInfo>>();

        DirectoryInfo dirInfo = new(directoryPath);
        string headerText = dirInfo.Name.Length == 0 ? dirInfo.FullName : dirInfo.Name;

        // Header item: selecting it returns the directory itself
        items.Add(new TextItem<FileSystemInfo> { Text = headerText, Value = dirInfo });

        // Directories first
        foreach (DirectoryInfo subDir in SafeEnumerateDirectories(dirInfo))
        {
            items.Add(new TextItem<FileSystemInfo> { Text = $"{subDir.Name}/", Value = subDir });
        }

        // Then files
        foreach (FileInfo file in SafeEnumerateFiles(dirInfo))
        {
            items.Add(new TextItem<FileSystemInfo> { Text = file.Name, Value = file });
        }

        return items;
    }

    private static IEnumerable<DirectoryInfo> SafeEnumerateDirectories(DirectoryInfo dir)
    {
        try
        {
            return dir.EnumerateDirectories()
                .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch
        {
            return Array.Empty<DirectoryInfo>();
        }
    }

    private static IEnumerable<FileInfo> SafeEnumerateFiles(DirectoryInfo dir)
    {
        try
        {
            return dir.EnumerateFiles()
                .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch
        {
            return Array.Empty<FileInfo>();
        }
    }

    private static void RenderFinalSelection(string text)
    {
        string displayText = TruncateText(
            text,
            Math.Max(0, Terminal.BufferWidth - Terminal.CursorLeft)
        );
        Terminal.Write(displayText);
    }

    private static string TruncateText(string text, int maxWidth)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxWidth)
            return text;
        return text[..Math.Min(maxWidth, text.Length)];
    }
}
