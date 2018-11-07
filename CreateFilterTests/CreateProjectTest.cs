using CreateFilterProject.Utilities;
using CreateFilterProject.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateFilterTests
{
    [TestClass]
    public class CreateProjectTest
    {
        private static readonly string PARENT_PATH = Environment.ExpandEnvironmentVariables(@"%HomePath%\UnitTests\CreateProjectTest");

        private string GetChildPath(string childDirectoryName)
        {
            return $"{PARENT_PATH}\\{childDirectoryName}";
        }

        public void TestCreateProject()
        {
            TestFolderDepthSuccess();
            TestFolderDepthFailure();
            TestExtremeFolderDepthFailure();
        }
        
        /// <summary>
        /// This tests a project with a reasonable number of subdirectories, and should return true
        /// </summary>
        [TestMethod]
        public void TestFolderDepthSuccess()
        {
            string sourceName = "FolderDepthSuccess";
            DirectoryInfo directory = CreateStructure(sourceName, 10);
            CreateFile($"{directory.FullName}\\{sourceName}.sln");
            var projectController = CreateProject(sourceName);

            if (projectController.HasError)
                throw new Exception($"Could not create project. {projectController.ErrorText}");
        }

        /// <summary>
        /// This tests a project that has folders > MAX_FOLDER_DEPTH, and should return false
        /// </summary>
        [TestMethod]
        public void TestFolderDepthFailure()
        {
            string sourceName = "FolderDepthFailure";
            DirectoryInfo directory = CreateStructure(sourceName, ProjectController.MAX_FOLDER_DEPTH + 1);
            CreateFile($"{directory.FullName}\\{sourceName}.sln");
            var projectController = CreateProject(sourceName);

            if (!projectController.HasError)
                throw new Exception($"ProjectController should not be capable of finding the solution due to max folder depth being reached");
        }

        /// <summary>
        /// This tests an extremem case of 50 recursions (what if the user selected the C drive as the source repo?
        /// </summary>
        [TestMethod]
        public void TestExtremeFolderDepthFailure()
        {
            string sourceName = "ExtremeFolderDepthFailure";
            DirectoryInfo directory = CreateStructure(sourceName, ProjectController.MAX_FOLDER_DEPTH + 20);
            CreateFile($"{directory.FullName}\\{sourceName}.sln");
            var projectController = CreateProject(sourceName);

            if (!projectController.HasError)
                throw new Exception($"ProjectController should not be capable of finding the solution due to max folder depth being reached");
        }

        private DirectoryInfo CreateStructure(string sourceName, int depth)
        {
            string sourcePath = GetChildPath(sourceName.ToLower());

            // clean up old test structure
            if (Directory.Exists(sourcePath))
                Directory.Delete(sourcePath, true);

            DirectoryInfo directory = IOUtility.CreateStructure(sourcePath, depth);
            return directory;
        }

        private void CreateFile(string filePath)
        {
            using (var fs = File.Create(filePath))
            {
                fs.Close();
            }
        }

        private ProjectController CreateProject(string sourceName)
        {
            // generate name parameters from source name
            string newName = $"{sourceName}Copy";
            string sourceProjectPath = GetChildPath(sourceName.ToLower());
            string newProjectPath = GetChildPath(newName.ToLower());

            // clean up old test structure
            if (Directory.Exists(newProjectPath))
                Directory.Delete(newProjectPath, true);

            // run controller test
            var projectController = new ProjectController
            {
                SourceProjectDirectory = new DirectoryInfo(sourceProjectPath),
                NewProjectName = newName
            };

            var createProjectTask = projectController.CreateProject();
            createProjectTask.Wait();
            return projectController;
        }
    }
}
