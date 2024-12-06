using System;
using project_downloads.Models;

namespace project_downloads.Data;

public static class FileRepository
{
    public static List<FileData> Files { get; set; } = new List<FileData>();
}
