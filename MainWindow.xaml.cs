using Drum_Machine.Core;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Drum_Machine
{
    public partial class MainWindow : Window
    {
        private DrumMachine drumMachine;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            drumMachine = new DrumMachine();
            LoopButton.IsChecked = true;

            InitTimer();

            drumMachine.AddTrack("Kick");
            drumMachine.AddTrack("Snare");
            drumMachine.AddTrack("Hi-Hat");

            RenderTracks();
        }

        private void InitTimer()
        {
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;

            UpdateBPM();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (drumMachine == null) return;

            drumMachine.PlayStep();

            if (!drumMachine.IsLooping &&
                drumMachine.CurrentStep == drumMachine.StepsCount - 1)
            {
                timer.Stop();
            }
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
            var dialog = new OpenFileDialog();
            dialog.Filter = "Audio Files (*.wav;*.mp3)|*.wav;*.mp3";

            if (dialog.ShowDialog() == true)
            {
                drumMachine.Tracks[trackIndex].SamplePath = dialog.FileName;
                RenderTracks();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Save буде реалізовано пізніше");
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

        private void RenderTracks()
        {
            if (TracksPanel == null || drumMachine == null)
                return;

            TracksPanel.Children.Clear();

            for (int i = 0; i < drumMachine.Tracks.Count; i++)
            {
                int trackIndex = i;
                var track = drumMachine.Tracks[i];

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

                var sampleBtn = new Button
                {
                    Content = "Load",
                    Margin = new Thickness(2)
                };

                sampleBtn.Click += (s, e) => LoadSample(trackIndex);

                var sampleName = new TextBlock
                {
                    Text = string.IsNullOrEmpty(track.SamplePath)
                        ? "No sample"
                        : System.IO.Path.GetFileName(track.SamplePath),
                    Foreground = System.Windows.Media.Brushes.Gray,
                    FontSize = 10
                };

                var leftPanel = new StackPanel();
                leftPanel.Children.Add(nameBox);
                leftPanel.Children.Add(sampleBtn);
                leftPanel.Children.Add(sampleName);

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

                var stepsGrid = new UniformGrid { Columns = 16 };

                for (int j = 0; j < 16; j++)
                {
                    int step = j;

                    var tbtn = new ToggleButton
                    {
                        Style = (Style)FindResource("StepButtonStyle"),
                        IsChecked = track.Steps[step]
                    };

                    tbtn.Checked += (s, e) => track.Steps[step] = true;
                    tbtn.Unchecked += (s, e) => track.Steps[step] = false;

                    stepsGrid.Children.Add(tbtn);
                }

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
    }
}