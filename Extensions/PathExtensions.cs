namespace Extensions;

/// <summary>
/// Contains extension methods for working with file paths.
/// </summary>
public static class PathExtensions
{
    /// <summary>
    /// Converts a user file name to a database file name based on the provided user ID.
    /// </summary>
    /// <param name="userFileName">The user file name to convert.</param>
    /// <param name="userId">The user ID to append to the file name.</param>
    /// <returns>Returns the converted database file name.</returns>
    public static string UserToDBFileName(this string userFileName, string userId) 
        => Path.Combine("../../../../", "data", "user files", Path.GetFileNameWithoutExtension(userFileName) + userId + Path.GetExtension(userFileName));

    /// <summary>
    /// Converts a database file name to a user "friendly" file name based on the provided user ID.
    /// </summary>
    /// <param name="dbFileName">The database file name to convert.</param>
    /// <param name="userId">The user ID to remove from the file name.</param>
    /// <returns>Returns the converted user file name.</returns>
    public static string DBToUserFileName(this string dbFileName, string userId)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dbFileName); 
        return fileNameWithoutExtension.Remove(fileNameWithoutExtension.Length - userId.Length) +
                                                 Path.GetExtension(dbFileName);
    }
        
}