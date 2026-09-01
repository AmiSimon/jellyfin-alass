using System.IO;
using System.Threading.Tasks;

namespace SubtitleSync.Shared.Interfaces
{
    /// <summary>
    /// Abstraction for file system operations.
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True if the file exists, false otherwise.</returns>
        bool FileExists(string path);

        /// <summary>
        /// Checks if a directory exists.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True if the directory exists, false otherwise.</returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Reads all text from a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The file content as a string.</returns>
        Task<string> ReadAllTextAsync(string path);

        /// <summary>
        /// Writes all text to a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="content">The content to write.</param>
        Task WriteAllTextAsync(string path, string content);

        /// <summary>
        /// Copies a file from one location to another.
        /// </summary>
        /// <param name="sourcePath">The source file path.</param>
        /// <param name="destinationPath">The destination file path.</param>
        /// <param name="overwrite">Whether to overwrite if the destination exists.</param>
        Task CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = false);

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        Task DeleteFileAsync(string path);

        /// <summary>
        /// Gets the files in a directory.
        /// </summary>
        /// <param name="path">The directory path.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="searchOption">The search option.</param>
        /// <returns>An array of file paths.</returns>
        string[] GetFiles(string path, string searchPattern = "*", System.IO.SearchOption searchOption = System.IO.SearchOption.TopDirectoryOnly);

        /// <summary>
        /// Gets the creation time of a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The creation time.</returns>
        System.DateTime GetCreationTime(string path);

        /// <summary>
        /// Gets the last write time of a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The last write time.</returns>
        System.DateTime GetLastWriteTime(string path);
    }
}
