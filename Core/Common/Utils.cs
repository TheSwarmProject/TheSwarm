namespace TheSwarm.Common.Utils;

/// <summary>
/// Utilities for working with files and folders
/// </summary>
public static class FileUtils {
    public static void Copy(string sourceDirectory, string targetDirectory){
        DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
        DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

        CopyAll(diSource, diTarget);
    }

    public static void CopyAll(DirectoryInfo source, DirectoryInfo target){
        Directory.CreateDirectory(target.FullName);

        foreach (FileInfo fi in source.GetFiles()){
            fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
        }

        // Copy each subdirectory using recursion.
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()){
            DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
            CopyAll(diSourceSubDir, nextTargetSubDir);
        }
    }
}