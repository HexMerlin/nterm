using Nterm.Core.Buffer;
using Nterm.Common;

namespace Nterm.Core.Controls;

public record DirectoryOptions
{
    public string[] FileExtensions { get; init; } = [];
    public bool IncludeParentDirectoryAtStart { get; init; }
    public bool ShowOnlyFiles { get; init; }
    public bool ShowOnlyDirectories { get; init; }
    public bool ShowHiddenFilesAndDirectories { get; init; }
    public Color DirectoryColor { get; init; } = Color.Beige;
}

public class DirectoryItem : TextItem<FileSystemInfo>
{
    public bool IsDirectory => Value is DirectoryInfo;
    public bool IsFile => Value is FileInfo;

    private const string CurrentDir = ".";
    private const string ParentDir = "..";

    public static List<DirectoryItem> ListShallow(string startDirectory, DirectoryOptions options)
    {
        DirectoryInfo dir = new(ResolveStartDirectory(startDirectory));
        return [.. EnumerateShallow(dir, dir.FullName, options)];
    }

    public static List<DirectoryItem> ListDeep(string startDirectory, DirectoryOptions options)
    {
        DirectoryInfo dir = new(ResolveStartDirectory(startDirectory));
        return [.. EnumerateDeep(dir, dir.FullName, options).OrderBy(TypeAndName)];
    }

    private static string TypeAndName(DirectoryItem item)
    {
        if (item.Value is DirectoryInfo)
        {
            return $"a-{item.Text}";
        }

        return $"b-{item.Text}";
    }

    private static IEnumerable<DirectoryItem> EnumerateShallow(
        DirectoryInfo dir,
        string root,
        DirectoryOptions options
    )
    {
        if (!options.ShowOnlyFiles)
        {
            yield return ToHeader(dir, options.DirectoryColor);

            if (options.IncludeParentDirectoryAtStart && dir.Parent is not null)
            {
                yield return new DirectoryItem
                {
                    Text = new TextBuffer(ParentDir),
                    Prefix = new TextBuffer($"📁 "),
                    Description = GetPathDescriptor(root, dir.Parent.FullName),
                    Value = dir.Parent
                };
            }

            foreach (
                DirectoryInfo sub in EnumerateDirectories(
                    dir,
                    options.ShowHiddenFilesAndDirectories
                )
            )
            {
                yield return ToDirectoryItem(root, sub, options.DirectoryColor);
            }
        }

        if (!options.ShowOnlyDirectories)
        {
            foreach (
                FileInfo f in EnumerateFiles(
                    dir,
                    options.ShowHiddenFilesAndDirectories,
                    options.FileExtensions
                )
            )
            {
                yield return ToFileItem(root, f, dir);
            }
        }
    }

    private static IEnumerable<DirectoryItem> EnumerateDeep(
        DirectoryInfo dir,
        string root,
        DirectoryOptions options
    )
    {
        IEnumerable<DirectoryInfo> subDirs = EnumerateDirectories(
            dir,
            options.ShowHiddenFilesAndDirectories
        );

        foreach (DirectoryInfo sub in subDirs)
        {
            if (!options.ShowOnlyFiles)
                yield return ToDirectoryItem(root, sub, options.DirectoryColor);

            foreach (DirectoryItem item in EnumerateDeep(sub, root, options))
                yield return item;
        }

        if (!options.ShowOnlyDirectories)
        {
            foreach (
                FileInfo f in EnumerateFiles(
                    dir,
                    options.ShowHiddenFilesAndDirectories,
                    options.FileExtensions
                )
            )
            {
                yield return ToFileItem(root, f, dir);
            }
        }
    }

    private static DirectoryItem ToHeader(DirectoryInfo dir, Color color) =>
        new()
        {
            Text = new TextBuffer(CurrentDir, color),
            Prefix = new TextBuffer($"📁 "),
            Description = dir.Name.Length == 0 ? dir.FullName : dir.Name,
            Value = dir
        };

    private static DirectoryItem ToDirectoryItem(string root, DirectoryInfo d, Color color) =>
        new()
        {
            Text = new TextBuffer($"{d.Name}/", color),
            Prefix = new TextBuffer($"📁 "),
            Description = GetPathDescriptor(root, d.FullName),
            Value = d
        };

    private static DirectoryItem ToFileItem(string root, FileInfo f, DirectoryInfo fallbackDir) =>
        new()
        {
            Text = f.Name,
            Description = GetPathDescriptor(root, f.Directory?.FullName ?? fallbackDir.FullName),
            Value = f
        };

    private static string ResolveStartDirectory(string? startDirectory)
    {
        if (string.IsNullOrWhiteSpace(startDirectory))
            return Environment.CurrentDirectory;

        return Path.GetFullPath(startDirectory);
    }

    private static IEnumerable<DirectoryInfo> EnumerateDirectories(
        DirectoryInfo dir,
        bool showHiddenDirectories
    )
    {
        IEnumerable<DirectoryInfo> directories = dir.EnumerateDirectories();
        if (!showHiddenDirectories)
        {
            directories = directories.Where(d => !d.Attributes.HasFlag(FileAttributes.Hidden));
        }
        return directories.OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<FileInfo> EnumerateFiles(
        DirectoryInfo dir,
        bool showHiddenFiles,
        string[] fileExtensions
    )
    {
        IEnumerable<FileInfo> files = dir.EnumerateFiles();
        if (!showHiddenFiles)
        {
            files = files.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
        }
        if (fileExtensions.Length > 0)
        {
            files = files.Where(f => fileExtensions.Contains(f.Extension));
        }
        return files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase);
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
}
