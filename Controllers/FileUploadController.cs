using System;
using Microsoft.AspNetCore.Mvc;
using project_downloads.Data;
using project_downloads.Models;
namespace project_downloads.Controllers;

public class FileUploadController : Controller
{
    public IActionResult Index(){
        return View();
    }
    
    public IActionResult Upload(IFormFile file, string password){
        // работа с файлом

        // альтернативный вариант загрузки
        //var files = HttpContext.Request.Form.Files.FirstOrDefault();

        if(file == null || file.Length == 0){
            return Content("Файл не выбран");
        }

        var currentDirectory = Directory.GetCurrentDirectory();
        var uploadsFolder = Path.Combine(currentDirectory, "UploadedFiles");

        if(!Directory.Exists(uploadsFolder)){
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, file.FileName);

        using (var stream = new FileStream(filePath, FileMode.Create)){
            file.CopyTo(stream);
        }

        // работа с паролем
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // сохранение в репозиторий
        //FileRepository.Files.Add(new FileData(){ FileName = file.FileName, PasswordHash = passwordHash });
        FileDataManager.AddFile(new FileData(){ FileName = file.FileName, PasswordHash = passwordHash});

        return Content($"Файл <{file.FileName}> загружен! Hash: {passwordHash}");
    }

    [HttpGet]
    public IActionResult DownloadFile(string fileName){
        return View("DownloadFile", fileName);
    }

    [HttpPost]
    public IActionResult DownloadFile(string fileName, string password){
        //var fileData = FileRepository.Files.FirstOrDefault(x => x.FileName == fileName);
        var fileData = FileDataManager.GetFile(fileName);
        
        if(fileData == null){
            return Content("Файл не найден!");
        }

        if(!BCrypt.Net.BCrypt.Verify(password, fileData.PasswordHash)){
            return Content("Введён неправильный пароль!");
        }

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
        var filePath = Path.Combine(uploadsFolder, fileName);

        byte[] bytes = System.IO.File.ReadAllBytes(filePath);
        return File(bytes, "application/octet-stream", fileName);
    }

    [HttpGet]
    public IActionResult Download(string fileName){
        var currentDirectory = Directory.GetCurrentDirectory();
        var uploadsFolder = Path.Combine(currentDirectory, "UploadedFiles");

        if(!Directory.Exists(uploadsFolder)){
            return Content("Ошибка! Директория не найдена!");
        }

        var files = Directory.GetFiles(uploadsFolder).
        Select(file => new FileInfo(file)).
        Select(file => new {
            Name = file.Name,
            Size = $"{(file.Length / 1024f / 1024f):F2} МБ"
        }).ToList();
        
        return View(files);
    }
    
    [HttpPost]
    public IActionResult DeleteFile(string fileName, string password){
        if(fileName == null || password == null){
            return View();
        }

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFilesw");

        var filePath = Path.Combine(uploadsFolder, fileName);
        
        if(System.IO.File.Exists(filePath)){
            var file = FileDataManager.GetFile(fileName);
            if(BCrypt.Net.BCrypt.Verify(password, file.PasswordHash)){
                FileDataManager.DeleteFile(file);

                return Content($"Файл <{fileName} удалён!");
            }else{
                return Content($"Файл <{fileName}> найден. Введён неправильный пароль!");
            }
        }

        return Content($"Файл <{fileName}> не найден.");
    }
}
