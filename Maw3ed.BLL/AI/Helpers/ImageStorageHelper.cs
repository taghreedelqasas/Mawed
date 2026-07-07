using System;

namespace Maw3ed.BLL.AI.Helpers;

public static class ImageStorageHelper
{
    public static async Task<string> SaveImageAsync(
        Stream imageStream,
        string fileName,
        string rootPath)
    {
        var folder = Path.Combine(rootPath, "MedicalImages");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var uniqueName =
            $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";

        var filePath = Path.Combine(folder, uniqueName);

        using var stream = new FileStream(filePath, FileMode.Create);

        await imageStream.CopyToAsync(stream);

        return uniqueName;
    }
}