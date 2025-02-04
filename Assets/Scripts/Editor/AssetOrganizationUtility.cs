using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.IO.Compression;

public static class AssetOrganizationUtility
{
    private static StringBuilder _logBuilder = new StringBuilder();
    private static string _logFilePath = Path.Combine(Application.dataPath, "../OrganizeAssetsLog.txt"); 
    private static string _backupZipPath = Path.Combine(Application.dataPath, "../ScriptsBackup.zip");
    
    [MenuItem("Assets/OrganizeAssets")]
    public static void Organize()
    {
        _logBuilder.Clear(); 
        Log("OrganizeAssets started.");
        
        CreateBackup();

        string assetsPath = Application.dataPath;
        
        MoveFilesToFolder(assetsPath, "*.cs", "Scripts");
        MoveFilesToFolder(assetsPath, "*.jpeg", "Content/Sprites");
        MoveFilesToFolder(assetsPath, "*.png", "Content/Sprites");
        MoveFilesToFolder(assetsPath, "*.prefab", "Prefabs");
        MoveFilesToFolder(assetsPath, "*.unity", "Scenes");

        AssetDatabase.Refresh();
        
        WriteLogToFile();
        Log("OrganizeAssets completed.");
    }

    private static void CreateBackup()
    {
        try
        {
            if (File.Exists(_backupZipPath))
            {
                File.Delete(_backupZipPath);
                Log("Old backup archive deleted.");
            }
            
            string tempScriptsFolder = Path.Combine(Application.dataPath, "TempScriptsBackup");
            
            if (Directory.Exists(tempScriptsFolder))
            {
                Directory.Delete(tempScriptsFolder, true);
            }
            
            Directory.CreateDirectory(tempScriptsFolder);
            
            foreach (string filePath in Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories))
            {
                string relativePath = filePath.Substring(Application.dataPath.Length + 1);
                string destinationPath = Path.Combine(tempScriptsFolder, relativePath);
                
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                
                File.Copy(filePath, destinationPath);
            }
            
            ZipFile.CreateFromDirectory(tempScriptsFolder, _backupZipPath);
            Log($"Backup archive created at: {_backupZipPath}");
            
            Directory.Delete(tempScriptsFolder, true);
            Log("Temporary backup folder deleted.");
        }
        catch (System.Exception e)
        {
            Log($"Backup failed: {e.Message}");
            Debug.LogError($"Backup failed: {e.Message}");
        }
    }

    private static void MoveFilesToFolder(string rootPath, string searchPattern, string targetFolder)
    {
        string fullTargetPath = Path.Combine(rootPath, targetFolder);
        
        if (!Directory.Exists(fullTargetPath))
        {
            Directory.CreateDirectory(fullTargetPath);
            Log($"Created folder: {fullTargetPath}");
        }
        
        string[] files = Directory.GetFiles(rootPath, searchPattern, SearchOption.AllDirectories);

        foreach (string file in files)
        {
            string relativePath = file.Substring(rootPath.Length + 1);
            string directoryName = Path.GetDirectoryName(relativePath);
            
            if (directoryName.StartsWith(targetFolder))
            {
                Log($"Skipped (already in target folder): {file}");
                continue;
            }

            string fileName = Path.GetFileName(relativePath);
            string destinationPath = Path.Combine(fullTargetPath, fileName);
            
            destinationPath = GetUniqueFilePath(destinationPath);
            
            File.Move(file, destinationPath);
            Log($"Moved: {file} -> {destinationPath}");
        }
    }

    private static string GetUniqueFilePath(string path)
    {
        if (!File.Exists(path))
        {
            return path;
        }

        string directory = Path.GetDirectoryName(path);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        string extension = Path.GetExtension(path);

        int counter = 1;
        string newPath;
        
        do
        {
            string newFileName = $"{fileNameWithoutExtension}_{counter}{extension}";
            newPath = Path.Combine(directory, newFileName);
            counter++;
        } 
        while (File.Exists(newPath));

        return newPath;
    }

    private static void Log(string message)
    {
        _logBuilder.AppendLine($"[{System.DateTime.Now}] {message}");
    }

    private static void WriteLogToFile()
    {
        try
        {
            File.WriteAllText(_logFilePath, _logBuilder.ToString());
            Debug.Log($"Log file created at: {_logFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to write log file: {e.Message}");
        }
    }
}