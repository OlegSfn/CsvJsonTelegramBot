namespace Extensions;

public static class PathExtensions
{
    public static string UserToDBFileName(string userFileName, string userId) 
        => Path.Combine("../../../../", "data", Path.GetFileNameWithoutExtension(userFileName) + userId + Path.GetExtension(userFileName));

    public static string DBToUserFileName(string dbFileName, string userId)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dbFileName); 
        return fileNameWithoutExtension.Remove(fileNameWithoutExtension.Length - userId.Length) +
                                                 Path.GetExtension(dbFileName);
    }
        
}