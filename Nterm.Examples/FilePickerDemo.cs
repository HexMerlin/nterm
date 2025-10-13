using Nterm.Core.Controls;

namespace Nterm.Examples;

public class FilePickerDemo
{
    public static void Run()
    {
        TextItem<FileSystemInfo> picked = FilePicker.Show(
            options: new()
            {
                FileExtensions = [".cs"],
                ShowOnlyFiles = false,
                ShowOnlyDirectories = false,
                ShowHiddenFilesAndDirectories = false,
                AllowNavigationAboveStartDirectory = false,
                FlattenDirectories = true,
                FilterOnDescription = true
            }
        );
        if (!picked.IsEmpty())
        {
            FileSystemInfo fs = picked.Value; // FileInfo or DirectoryInfo
            Terminal.WriteLine($"Picked: {fs.FullName}");
            // handle result
        }
    }
}
