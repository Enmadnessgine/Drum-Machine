using Drum_Machine.Core;
using Drum_Machine.Data;
using Drum_Machine.Data.Repositories;
using System.Windows;

namespace Drum_Machine.Views
{
    public partial class LoadProjectWindow : Window
    {
        public int? SelectedProjectId { get; private set; }

        public LoadProjectWindow()
        {
            InitializeComponent();
            LoadProjectsIntoList();
        }

        private void LoadProjectsIntoList()
        {
            using (var db = new AppDbContext())
            {
                var repo = new ProjectRepository(db);
                var projects = repo.GetUserProjects(AppSession.CurrentUser.Id);

                ProjectsListBox.ItemsSource = projects;
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsListBox.SelectedValue != null)
            {
                SelectedProjectId = (int)ProjectsListBox.SelectedValue;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть проєкт зі списку.");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}