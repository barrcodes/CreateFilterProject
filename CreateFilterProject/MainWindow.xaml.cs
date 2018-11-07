using CreateFilterProject.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CreateFilterProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ProjectController ProjectController { get; private set; } = new ProjectController();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Browse_Click(object sender, RoutedEventArgs e)
        {
            await ProjectController.BrowseForSourceProject();
            SourceProjectName.Text = ProjectController.SourceProjectDirectory.FullName;
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            ProjectController.NewProjectName = NewProjectName.Text;
            await ProjectController.CreateProject();

            if (ProjectController.HasError)
            {
                MessageBox.Show(ProjectController.ErrorText, "Error", MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show("Project generated successfully", "Success", MessageBoxButton.OK);
                Application.Current.Shutdown();
            }
        }

        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "To properly use this application, browse to an existing Photoshop SDK filter project. If you don't have a filter project to duplicate, try using a Photoshop SDK sample filter project (i.e. the \"dissolve\" project).\n\n" +
                "The project you choose will be used as the source project, and its contents will be duplicated and renamed according to the name you define in \"New Project Name\"",
                "About",
                MessageBoxButton.OK);
        }
    }
}
