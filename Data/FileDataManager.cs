using System;
using System.Text.Json;
using project_downloads.Models;

namespace project_downloads.Data;

public static class FileDataManager
{
    private static string filePath = Path.Combine(Directory.GetCurrentDirectory(), "filedata.json");

    public static List<FileData> GetAllFiles(){
        if(!File.Exists(filePath)){
            return new List<FileData>();
        }

        var json = File.ReadAllText(filePath);
        List<FileData> fileDatas = JsonSerializer.Deserialize<List<FileData>>(json);
        return fileDatas;
    }

    public static void AddFile(FileData fileData){
        var fileDatas = GetAllFiles();
        fileDatas.Add(fileData);
        SaveAllFiles(fileDatas);
    }

    private static void SaveAllFiles(List<FileData> fileDatas){
        var new_json = JsonSerializer.Serialize(fileDatas, new JsonSerializerOptions() { WriteIndented = true});
        File.WriteAllText(filePath, new_json);
    }

    public static FileData? GetFile(string fileName){
        var fileData = GetAllFiles().FirstOrDefault(x => x.FileName == fileName);
        return fileData;
    }

    public static void DeleteFile(FileData file){
        var fileDatas = GetAllFiles();
        fileDatas.Remove(file);
        SaveAllFiles(fileDatas);
    }
}
