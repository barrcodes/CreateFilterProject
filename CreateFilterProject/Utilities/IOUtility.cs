using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CreateFilterProject.Utilities
{
    public static class IOUtility
    {
        /// <summary>
        /// Copies files recursively and synchronously. 
        /// Use this method to test getNameCallbacks, as the asynchronous method will call many callbacks at once.
        /// </summary>
        /// <param name="source">Source Directory</param>
        /// <param name="target">Target Directory</param>
        /// <param name="getNameCallback">A callback for renaming the copied file / directory</param>
        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target, Func<string, string> getNameCallback = null)
        {
            foreach (DirectoryInfo dirInfo in source.EnumerateDirectories())
            {
                var dirName = getNameCallback?.Invoke(dirInfo.Name) ?? dirInfo.Name;
                CopyFilesRecursively(dirInfo, target.CreateSubdirectory(dirName), getNameCallback);
            }
            foreach (FileInfo fileInfo in source.EnumerateFiles())
            {
                var fileName = getNameCallback?.Invoke(fileInfo.Name) ?? fileInfo.Name;
                fileInfo.CopyTo(Path.Combine(target.FullName, fileName));
            }
        }

        /// <summary>
        /// Copies files recursively and asynchronously. 
        /// </summary>
        /// <param name="source">Source Directory</param>
        /// <param name="target">Target Directory</param>
        /// <param name="getNameCallback">An async callback for renaming the copied file / directory on the UI thread.</param>
        public static async Task CopyFilesRecursivelyAsync(DirectoryInfo source, DirectoryInfo target, Func<string, Task<string>> getNameCallback = null)
        {
            List<Task> tasks = new List<Task>();

            foreach (DirectoryInfo dirInfo in source.EnumerateDirectories())
            {
                var dirName = await getNameCallback?.Invoke(dirInfo.Name) ?? dirInfo.Name;
                var recursiveTask = CopyFilesRecursivelyAsync(dirInfo, target.CreateSubdirectory(dirName), getNameCallback);
                tasks.Add(recursiveTask);
            }
            foreach (FileInfo fileInfo in source.EnumerateFiles())
            {
                var fileName = await getNameCallback?.Invoke(fileInfo.Name) ?? fileInfo.Name;
                fileInfo.CopyTo(Path.Combine(target.FullName, fileName));
            }

            await Task.WhenAll(tasks.ToArray());
        }

        /// <summary>
        /// Synchronously recurse through the files in a given directory, and serve-up callbacks for each directory and file found.
        /// </summary>
        /// <param name="directoryInfo">Directory to recurse</param>
        /// <param name="directoryCallback">Callback for each directory</param>
        /// <param name="fileCallback">Callback for each file</param>
        public static void RecurseFiles(DirectoryInfo directoryInfo, Action<DirectoryInfo> directoryCallback = null, Action<FileInfo> fileCallback = null)
        {
            foreach (DirectoryInfo dirInfo in directoryInfo.EnumerateDirectories())
            {
                RecurseFiles(dirInfo, directoryCallback, fileCallback);
                directoryCallback?.Invoke(dirInfo);
            }
            foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles())
            {
                fileCallback?.Invoke(fileInfo);
            }
        }

        /// <summary>
        /// Asynchronously recurse through the files in a given directory, and serve-up async callbacks for each directory and file found.
        /// </summary>
        /// <param name="directoryInfo">Directory to recurse</param>
        /// <param name="directoryCallback">Async callback for each directory</param>
        /// <param name="fileCallback">Async callback for each file</param>
        public static async Task RecurseFilesAsync(DirectoryInfo directoryInfo, CancellationToken ct, Func<DirectoryInfo, Task> directoryCallback = null, Func<FileInfo, Task> fileCallback = null)
        {
            if (ct.IsCancellationRequested)
                return;

            List<Task> tasks = new List<Task>();

            foreach (DirectoryInfo dirInfo in directoryInfo.EnumerateDirectories())
            {
                if (ct.IsCancellationRequested)
                    break;

                var directoryCallbackTask = directoryCallback?.Invoke(dirInfo);
                if (directoryCallbackTask != null)
                    tasks.Add(directoryCallbackTask);
                
                // recurse after directory callback, which allows the caller to invoke the cancellation token based on directory depth
                tasks.Add(RecurseFilesAsync(dirInfo, ct, directoryCallback, fileCallback));
            }
            foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles())
            {
                if (ct.IsCancellationRequested)
                    break;

                var fileCallbackTask = fileCallback?.Invoke(fileInfo);
                if (fileCallbackTask != null)
                    tasks.Add(fileCallbackTask);
            }
            
            await Task.WhenAll(tasks.ToArray());
        }

        /// <summary>
        /// Synchronously transform each line in inputFile before writing the line into outputFile
        /// </summary>
        /// <param name="inputFile">Input file path</param>
        /// <param name="outputFile">Output file path</param>
        /// <param name="lineCallback">Callback to transform the input line</param>
        public static void TransformEachLine(string inputFile, string outputFile, Func<string, string> lineCallback)
        {
            using (FileStream sourceFS = new FileStream(inputFile, FileMode.Open))
            using (FileStream destFS = new FileStream(outputFile, FileMode.Create))
            using (StreamReader reader = new StreamReader(sourceFS))
            using (StreamWriter writer = new StreamWriter(destFS))
            {
                string readLine;

                while (null != (readLine = reader.ReadLine()))
                {
                    string writeLine = lineCallback?.Invoke(readLine) ?? string.Empty;
                    writer.WriteLine(writeLine);
                }
            }
        }

        /// <summary>
        /// Asynchronously transform each line in inputFile before writing the line into outputFile
        /// </summary>
        /// <param name="inputFile">Input file path</param>
        /// <param name="outputFile">Output file path</param>
        /// <param name="lineCallback">Async callback to transform the input line</param>
        /// <returns></returns>
        public static async Task TransformEachLineAsync(string inputFile, string outputFile, Func<string, Task<string>> lineCallback)
        {
            using (FileStream sourceFS = new FileStream(inputFile, FileMode.Open))
            using (FileStream destFS = new FileStream(outputFile, FileMode.Create))
            using (StreamReader reader = new StreamReader(sourceFS))
            using (StreamWriter writer = new StreamWriter(destFS))
            {
                string readLine;

                while (null != (readLine = await reader.ReadLineAsync()))
                {
                    string writeLine = await lineCallback?.Invoke(readLine) ?? string.Empty;
                    await writer.WriteLineAsync(writeLine);
                }
            }
        }

        /// <summary>
        /// Test method - creates a random file structure for testing task cancellation / validity in project creation
        /// </summary>
        public static DirectoryInfo CreateStructure(string parentPath, int folderDepth)
        {
            DirectoryInfo deepestDirectory = Directory.CreateDirectory(parentPath);
            StringBuilder pathBuilder = new StringBuilder(parentPath);

            for (int i = 0; i < folderDepth; i++)
            {
                pathBuilder.Append($"\\{i}");
                deepestDirectory = Directory.CreateDirectory(pathBuilder.ToString());
            }

            return deepestDirectory;
        }
    }
}
