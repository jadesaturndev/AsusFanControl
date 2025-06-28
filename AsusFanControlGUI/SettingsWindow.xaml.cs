using System;
using System.Diagnostics;
using System.Windows;

namespace AsusFanControlGUI
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
            
            // Make window draggable
            MouseLeftButtonDown += (s, e) => DragMove();
        }
        
        private void LoadSettings()
        {
            TurnOffControlOnExitCheckBox.IsChecked = Properties.Settings.Default.turnOffControlOnExit;
            ForbidUnsafeSettingsCheckBox.IsChecked = Properties.Settings.Default.forbidUnsafeSettings;
            MinimizeToTrayCheckBox.IsChecked = Properties.Settings.Default.minimizeToTrayOnClose;
            AutoRefreshStatsCheckBox.IsChecked = Properties.Settings.Default.autoRefreshStats;
        }
        
        private void SaveSettings()
        {
            Properties.Settings.Default.turnOffControlOnExit = TurnOffControlOnExitCheckBox.IsChecked ?? false;
            Properties.Settings.Default.forbidUnsafeSettings = ForbidUnsafeSettingsCheckBox.IsChecked ?? false;
            Properties.Settings.Default.minimizeToTrayOnClose = MinimizeToTrayCheckBox.IsChecked ?? false;
            Properties.Settings.Default.autoRefreshStats = AutoRefreshStatsCheckBox.IsChecked ?? false;
            Properties.Settings.Default.Save();
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            DialogResult = true;
            Close();
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        private void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/Karmel0x/AsusFanControl/releases",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening browser: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}