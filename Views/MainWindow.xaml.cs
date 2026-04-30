using Drum_Machine.Core;
using Drum_Machine.Data;
using Drum_Machine.Data.Entities;
using Drum_Machine.Models;
using Drum_Machine.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Drum_Machine.Views
{
    public partial class MainWindow : Window
    {
        private DrumMachine drumMachine;
        private DispatcherTimer? timer;
        private Point _dragStartPoint;
        private List<List<ToggleButton>> stepButtons = new List<List<ToggleButton>>();

        private int? _currentProjectId = null;

        public MainWindow()
        {
            InitializeComponent();

            drumMachine = new DrumMachine();
            LoopButton.IsChecked = true;

            InitTimer();
            UpdateAccountDisplay();
            RenderTracks();
            LoadSamplesFromDb();
        }

        private void InitTimer()
        {
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;

            UpdateBPM();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            drumMachine.PlayStep();
            HighlightStep(drumMachine.CurrentStep);
        }

        private void UpdateBPM()
        {
            if (timer == null || BpmSlider == null) return;

            double bpm = BpmSlider.Value;
            double interval = 60000.0 / bpm / 4;

            timer.Interval = TimeSpan.FromMilliseconds(interval);
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (!timer.IsEnabled)
                timer.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            drumMachine.Reset();
        }

        private void Loop_Checked(object sender, RoutedEventArgs e)
        {
            if (drumMachine == null) return;

            drumMachine.IsLooping = true;
        }

        private void Loop_Unchecked(object sender, RoutedEventArgs e)
        {
            if (drumMachine == null) return;

            drumMachine.IsLooping = false;
        }

        private void BpmSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateBPM();

            if (BpmTextBox != null)
                BpmTextBox.Text = ((int)e.NewValue).ToString();
        }

        private void LoadSample(int trackIndex)
        {
            var dialog = new OpenFileDialog { Filter = "WAV Files (*.wav)|*.wav" };

            if (dialog.ShowDialog() == true)
            {
                if (drumMachine.Tracks[trackIndex] is DrumTrack dt)
                {
                    dt.SamplePath = dialog.FileName;
                    RenderTracks();
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "WAV file (*.wav)|*.wav";

            if (dialog.ShowDialog() == true)
            {
                var buffer = drumMachine.RenderToBuffer();
                var exporter = new WavExporter();
                exporter.SaveWav(dialog.FileName, buffer);

                MessageBox.Show("Saved!");
            }
        }

        private void SaveProjectButton_Click(object sender, RoutedEventArgs e)
        {
            string projectName = $"Beat_{DateTime.Now:yyyyMMdd_HHmmss}";
            int savedId = SaveFullProject(projectName);

            if (savedId > 0)
            {
                _currentProjectId = savedId;
                MessageBox.Show($"Проєкт '{projectName}' збережено в базу.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExportWavButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProjectId == null)
            {
                MessageBox.Show("Будь ласка, спочатку збережіть проєкт (натисніть 'Save Project'), щоб ми могли прив'язати до нього цей WAV-файл.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "WAV file (*.wav)|*.wav",
                FileName = "MyExportedBeat.wav"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string wavFilePath = saveFileDialog.FileName;

                try
                {
                    using (var db = new AppDbContext())
                    {
                        var exportedTrack = new ExportedTrack
                        {
                            FilePath = wavFilePath,
                            ProjectId = _currentProjectId.Value
                        };

                        db.ExportedTracks.Add(exportedTrack);
                        db.SaveChanges();
                    }

                    MessageBox.Show("WAV файл успішно експортовано та прив'язано до проєкту!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при експорті: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddTrack_Click(object sender, RoutedEventArgs e)
        {
            string name = "New Track";

            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter track name:",
                "New Track",
                "Track " + (drumMachine.Tracks.Count + 1)
            );

            if (!string.IsNullOrWhiteSpace(input))
                name = input;

            drumMachine.AddTrack(name);
            RenderTracks();
        }

        private void RemoveTrack_Click(object sender, RoutedEventArgs e)
        {
            if (drumMachine.Tracks.Count > 0)
            {
                drumMachine.RemoveTrack(drumMachine.Tracks.Count - 1);
                RenderTracks();
            }
        }

        private void SampleListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SampleListBox.SelectedItem is SampleViewModel selectedSample)
            {
                var player = new System.Windows.Media.MediaPlayer();
                player.Open(new Uri(selectedSample.FullPath));
                player.Play();
            }
        }

        private void SampleListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void SampleListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = _dragStartPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (SampleListBox.SelectedItem is SampleViewModel selectedSample)
                    {
                        DragDrop.DoDragDrop(SampleListBox, selectedSample, DragDropEffects.Copy);
                    }
                }
            }
        }

        private void AddSample_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "WAV Audio (*.wav)|*.wav",
                Title = "Оберіть звук для додавання"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string sourcePath = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(sourcePath);

                string relativeDir = System.IO.Path.Combine("Users", AppSession.CurrentUser.Id.ToString(), "Samples");
                string dbRelativePath = System.IO.Path.Combine(relativeDir, fileName);

                string destDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeDir);
                string destPath = System.IO.Path.Combine(destDir, fileName);

                try
                {
                    System.IO.Directory.CreateDirectory(destDir);
                    System.IO.File.Copy(sourcePath, destPath, true);

                    using (var db = new AppDbContext())
                    {
                        var newSample = new Data.Entities.SampleEntity
                        {
                            Name = System.IO.Path.GetFileNameWithoutExtension(fileName),
                            FilePath = dbRelativePath,
                            UserId = AppSession.CurrentUser.Id
                        };

                        db.Samples.Add(newSample);
                        db.SaveChanges();
                    }

                    LoadSamplesFromDb();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при додаванні семплу: {ex.Message}");
                }
            }
        }

        private void DeleteSample_Click(object sender, RoutedEventArgs e)
        {
            if (SampleListBox.SelectedItem is SampleViewModel selectedSample)
            {
                var result = MessageBox.Show($"Видалити семпл {selectedSample.Name}?", "Підтвердження", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    using (var db = new AppDbContext())
                    {
                        var sampleInDb = db.Samples.FirstOrDefault(s => s.FilePath == selectedSample.FullPath);
                        if (sampleInDb != null)
                        {
                            db.Samples.Remove(sampleInDb);
                            db.SaveChanges();
                        }
                    }

                    string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, selectedSample.FullPath);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }

                    LoadSamplesFromDb();
                }
            }
        }

        private void HighlightStep(int step)
        {
            if (stepButtons == null) return;

            for (int i = 0; i < stepButtons.Count; i++)
            {
                for (int j = 0; j < stepButtons[i].Count; j++)
                {
                    var btn = stepButtons[i][j];

                    if (j == step)
                    {
                        btn.Opacity = 1.0;
                        btn.BorderBrush = System.Windows.Media.Brushes.Yellow;
                        btn.BorderThickness = new Thickness(2);
                    }
                    else
                    {
                        btn.Opacity = 0.5;
                        btn.BorderThickness = new Thickness(0);
                    }
                }
            }
        }

        private void RenderTracks()
        {
            if (TracksPanel == null || drumMachine == null)
                return;

            TracksPanel.Children.Clear();
            stepButtons.Clear();

            for (int i = 0; i < drumMachine.Tracks.Count; i++)
            {
                int trackIndex = i;
                var track = drumMachine.Tracks[i];
                var drumTrack = track as DrumTrack;

                var grid = new Grid { Margin = new Thickness(0, 5, 0, 5) };

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                var deleteBtn = new Button
                {
                    Content = "X",
                    Background = System.Windows.Media.Brushes.DarkRed,
                    Foreground = System.Windows.Media.Brushes.White
                };

                deleteBtn.Click += (s, e) =>
                {
                    drumMachine.RemoveTrack(trackIndex);
                    RenderTracks();
                };

                var nameBox = new TextBox
                {
                    Text = track.Name,
                    Margin = new Thickness(2)
                };

                nameBox.TextChanged += (s, e) =>
                {
                    track.Name = nameBox.Text;
                };

                bool hasSample = drumTrack != null && !string.IsNullOrEmpty(drumTrack.SamplePath);

                var sampleSlot = new Border
                {
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40)),
                    BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(80, 80, 80)),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(2),
                    AllowDrop = true,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Height = 25
                };

                var sampleStatusText = new TextBlock
                {
                    Text = hasSample ? System.IO.Path.GetFileName(drumTrack.SamplePath) : "Drag sample here",
                    Foreground = hasSample ? System.Windows.Media.Brushes.Orange : System.Windows.Media.Brushes.Gray,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    FontSize = 11,
                    Margin = new Thickness(5, 0, 5, 0),
                    TextTrimming = TextTrimming.CharacterEllipsis
                };

                sampleSlot.Child = sampleStatusText;
                sampleSlot.DragEnter += (s, e) =>
                {
                    if (e.Data.GetDataPresent(typeof(SampleViewModel)))
                    {
                        sampleSlot.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(80, 80, 80));
                    }
                };

                sampleSlot.DragLeave += (s, e) =>
                {
                    sampleSlot.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40));
                };

                sampleSlot.Drop += (s, e) =>
                {
                    if (e.Data.GetDataPresent(typeof(SampleViewModel)))
                    {
                        var sample = (SampleViewModel)e.Data.GetData(typeof(SampleViewModel));

                        sampleStatusText.Text = sample.Name;
                        sampleStatusText.Foreground = System.Windows.Media.Brushes.Orange;
                        sampleSlot.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30));

                        if (drumTrack != null)
                        {
                            drumTrack.SamplePath = sample.FullPath;
                        }
                    }
                };

                var leftPanel = new StackPanel();
                leftPanel.Children.Add(nameBox);
                leftPanel.Children.Add(sampleSlot);

                var slider = new Slider
                {
                    Minimum = 0,
                    Maximum = 1,
                    Value = track.Volume
                };

                slider.ValueChanged += (s, e) =>
                {
                    track.Volume = e.NewValue;
                };

                var stepsGrid = new System.Windows.Controls.Primitives.UniformGrid { Columns = 16 };
                var rowButtons = new List<System.Windows.Controls.Primitives.ToggleButton>();

                for (int j = 0; j < 16; j++)
                {
                    int step = j;

                    var tbtn = new System.Windows.Controls.Primitives.ToggleButton
                    {
                        Style = (Style)FindResource("StepButtonStyle"),
                        IsChecked = track.Steps[step]
                    };

                    tbtn.Checked += (s, e) => track.Steps[step] = true;
                    tbtn.Unchecked += (s, e) => track.Steps[step] = false;

                    stepsGrid.Children.Add(tbtn);
                    rowButtons.Add(tbtn);
                }

                stepButtons.Add(rowButtons);

                Grid.SetColumn(deleteBtn, 0);
                Grid.SetColumn(leftPanel, 1);
                Grid.SetColumn(slider, 2);
                Grid.SetColumn(stepsGrid, 3);

                grid.Children.Add(deleteBtn);
                grid.Children.Add(leftPanel);
                grid.Children.Add(slider);
                grid.Children.Add(stepsGrid);

                TracksPanel.Children.Add(grid);
            }
        }

        private void LoadSamplesFromDb()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var userSamples = db.Samples
                        .Where(s => s.UserId == AppSession.CurrentUser.Id)
                        .ToList();

                    SampleListBox.Items.Clear();

                    foreach (var sample in userSamples)
                    {
                        SampleListBox.Items.Add(new SampleViewModel
                        {
                            Name = sample.Name,
                            FullPath = sample.FilePath
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні бази даних: {ex.Message}");
            }
        }

        private void SampleSlot_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(SampleViewModel)))
            {
                Border slot = sender as Border;
                slot.Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));
            }
        }

        private void SampleSlot_DragLeave(object sender, DragEventArgs e)
        {
            Border slot = sender as Border;
            slot.Background = new SolidColorBrush(Color.FromRgb(40, 40, 40));
        }

        private void SampleSlot_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(SampleViewModel)))
            {
                var sample = (SampleViewModel)e.Data.GetData(typeof(SampleViewModel));

                Border slot = sender as Border;
                TextBlock text = slot.Child as TextBlock;

                text.Text = sample.Name;
                text.Foreground = Brushes.Orange;
                slot.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                slot.Tag = sample.FullPath;
            }
        }

        private void UpdateAccountDisplay()
        {
            var user = AppSession.CurrentUser;

            if (user != null)
            {
                UserNameTextBlock.Text = user.Username;

                if (user.Username.ToLower().Contains("guest") || user.Id == 0)
                {
                    AccountActionButton.Content = "Увійти";
                    AccountActionButton.Background = new SolidColorBrush(Color.FromRgb(0, 120, 215));
                }
                else
                {
                    AccountActionButton.Content = "Вийти";
                    AccountActionButton.Background = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                }
            }
        }

        private void AccountAction_Click(object sender, RoutedEventArgs e)
        {
            AppSession.CurrentUser = null;
            var authWindow = new LoginWindow();
            authWindow.Show();

            this.Close();
        }

        public class SampleViewModel
        {
            public string Name { get; set; }
            public string FullPath { get; set; }
            public bool IsUserSample { get; set; }
        }

        private int SaveFullProject(string title)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var projectEntity = new ProjectEntity
                    {
                        Title = title,
                        BPM = 120,
                        UserId = AppSession.CurrentUser.Id,
                        Tracks = new List<TrackEntity>()
                    };

                    foreach (var baseTrack in drumMachine.Tracks)
                    {
                        if (baseTrack is DrumTrack drumTrack)
                        {
                            string pattern = string.Join(",", drumTrack.Steps.Select(s => s ? "1" : "0"));

                            var trackEntity = new TrackEntity
                            {
                                Name = drumTrack.Name,
                                SamplePath = drumTrack.SamplePath,
                                Volume = drumTrack.Volume,
                                StepsData = pattern
                            };

                            projectEntity.Tracks.Add(trackEntity);
                        }
                    }

                    db.Projects.Add(projectEntity);
                    db.SaveChanges();

                    MessageBox.Show($"Проєкт '{title}' успішно збережено!");
                    return projectEntity.Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження: {ex.Message}");
                return -1;
            }
        }

    }
}