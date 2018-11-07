using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CreateFilterProject.Utilities;
using FolderSelect;

namespace CreateFilterProject.Controllers
{
    public class ProjectController
    {
        public const int MAX_FOLDER_DEPTH = 20;

        public DirectoryInfo SourceProjectDirectory { get; set; }

        public string ParentPath => SourceProjectDirectory.Parent.FullName;

        public string NewProjectName { get; set; }

        private string TargetPath => $"{ParentPath}\\{NewProjectName}";

        public string ErrorText { get; private set; }

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorText);

        private string caseSensitiveSourceName;

        private object LOCK_OBJECT = new object();

        public async Task<bool> BrowseForSourceProject()
        {
            FolderSelectDialog dialog = new FolderSelectDialog();
            var userOK = dialog.ShowDialog();

            if (!userOK)
                // user cancelled
                return false;

            SourceProjectDirectory = new DirectoryInfo(dialog.FileName);
            return true;
        }

        public async Task<bool> CreateProject()
        {
            DirectoryInfo projectDir = await CopySourceProject();

            if (projectDir == null)
                return false;

            await RenameProject(projectDir);
            return true;
        }

        private async Task<DirectoryInfo> CopySourceProject()
        {
            ErrorText = null;

            // Can't create a project with no name
            if (string.IsNullOrWhiteSpace(NewProjectName))
            {
                ErrorText = "Can't create project without a project name.";
                return null;
            }

            await GetCaseSensitiveSourceName();

            // Can't continue without a case-sensitive filter name
            if (string.IsNullOrEmpty(caseSensitiveSourceName))
            {
                ErrorText = $"{SourceProjectDirectory.Name} solution file could not be found.";
                return null;
            }

            // Photoshop convention: lower-cased folder
            var targetPath = $"{ParentPath}\\{NewProjectName.ToLower()}";
            var targetDir = Directory.CreateDirectory(targetPath);

            // Don't try to replace the contents of a folder
            if (targetDir.GetDirectories().Length > 0 ||
                targetDir.GetFiles().Length > 0)
            {
                ErrorText = "Can't create project. Target directory is not empty.";
                return null;
            }

            await IOUtility.CopyFilesRecursivelyAsync(SourceProjectDirectory, targetDir, getNameCallback: (string name) =>
            {
                var newName = name.ReplaceVariations(caseSensitiveSourceName, NewProjectName, HandleRenameConflictResolution);
                return Task.FromResult(newName);
            });

            return targetDir;
        }

        private async Task GetCaseSensitiveSourceName()
        {
            var cts = new CancellationTokenSource();
            int dirCount = 0;
            caseSensitiveSourceName = string.Empty;

            // Get the case-sensitive name. SOURCE_PATH will be foofilter by convention, but sln will be FooFilter.sln
            await IOUtility.RecurseFilesAsync(SourceProjectDirectory, cts.Token,
                directoryCallback: (DirectoryInfo dir) =>
                {
                    // stop multiple threads from writing to dirCount at the same time
                    lock (LOCK_OBJECT)
                    {
                        dirCount++;

                        if (!cts.IsCancellationRequested && dirCount > MAX_FOLDER_DEPTH)
                            cts.Cancel();
                    }

                    return Task.CompletedTask;
                },
                fileCallback: (FileInfo file) =>
                {
                    if (file.Extension == ".sln")
                    {
                        caseSensitiveSourceName = Path.GetFileNameWithoutExtension(file.Name);
                    }

                    return Task.CompletedTask;
                });
        }

        private Task<string> HandleRenameConflictResolution(string foundString, string option1, string option2)
        {
            return Task.FromResult(option2);
        }

        private async Task RenameProject(DirectoryInfo projectDir)
        {
            await IOUtility.RecurseFilesAsync(projectDir, new CancellationToken(), fileCallback: (FileInfo file) => RenameFileContents(file.FullName));   
        }

        private async Task RenameFileContents(string filePath)
        {
            string tempFilePath = $"{filePath}.tmp";
            string tempLine;
            string newLine;

            await IOUtility.TransformEachLineAsync(filePath, tempFilePath, lineCallback: (string readLine) =>
            {
                var writeLine = readLine.ReplaceVariations(caseSensitiveSourceName, NewProjectName, HandleRenameConflictResolution);
                return Task.FromResult(writeLine);
            });

            File.Delete(filePath);
            File.Move(tempFilePath, filePath);
        }
    }
}
