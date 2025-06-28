using AsusFanControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AsusFanControlGUI
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private AsusControl asusControl;
        private DispatcherTimer refreshTimer;
        private bool isControlEnabled = false;
        private int currentFanSpeed = 0;
        
        public ObservableCollection<FanStatusItem> FanStatusItems { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            asusControl = new AsusControl();
            FanStatusItems = new ObservableCollection<FanStatusItem>();
            FanStatusList.ItemsSource = FanStatusItems;
            
            // Initialize UI
            InitializeUI();
            
            // Setup auto-refresh timer
            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(2);
            refreshTimer.Tick += RefreshTimer_Tick;
            
            // Load settings
            LoadSettings();
            
            // Handle window events
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            Closing += MainWindow_Closing;
            
            // Make window draggable
            MouseLeftButtonDown += (s, e) => DragMove();
        }
        
        private void InitializeUI()
        {
            // Initialize fan count
            var fanCount = asusControl.HealthyTable_FanCounts();
            FanCountText.Text = fanCount.ToString();
            
            // Initialize fan status items
            for (int i = 0; i < fanCount; i++)
            {
                FanStatusItems.Add(new FanStatusItem
                {
                    Name = $"FAN {i + 1}",
                    RPM = 0,
                    RPMPercent = 0
                });
            }
            
            // Initial refresh
            RefreshCpuTemp_Click(null, null);
            RefreshFanSpeeds_Click(null, null);
        }
        
        private void LoadSettings()
        {
            try
            {
                FanSpeedSlider.Value = Properties.Settings.Default.fanSpeed;
                FanControlToggle.IsChecked = false; // Start with control disabled for safety
                UpdateFanSpeedDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void SaveSettings()
        {
            try
            {
                Properties.Settings.Default.fanSpeed = (int)FanSpeedSlider.Value;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void OnProcessExit(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.turnOffControlOnExit && isControlEnabled)
            {
                asusControl.SetFanSpeeds(0);
            }
        }
        
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            refreshTimer?.Stop();
            SaveSettings();
        }
        
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshCpuTemp_Click(null, null);
            RefreshFanSpeeds_Click(null, null);
        }
        
        private void FanControlToggle_Checked(object sender, RoutedEventArgs e)
        {
            isControlEnabled = true;
            UpdateControlStatus();
            ApplyFanSpeed();
            
            if (Properties.Settings.Default.autoRefreshStats)
            {
                refreshTimer.Start();
            }
        }
        
        private void FanControlToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            isControlEnabled = false;
            UpdateControlStatus();
            
            // Turn off fan control
            asusControl.SetFanSpeeds(0);
            refreshTimer.Stop();
        }
        
        private void UpdateControlStatus()
        {
            if (isControlEnabled)
            {
                StatusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#39FF14"));
                StatusIndicator.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = (Color)ColorConverter.ConvertFromString("#39FF14"),
                    BlurRadius = 15,
                    ShadowDepth = 0,
                    Opacity = 0.8
                };
                StatusText.Text = "ACTIVE";
                StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#39FF14"));
            }
            else
            {
                StatusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30363D"));
                StatusIndicator.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Red,
                    BlurRadius = 15,
                    ShadowDepth = 0,
                    Opacity = 0.8
                };
                StatusText.Text = "INACTIVE";
                StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B949E"));
            }
        }
        
        private void FanSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Properties.Settings.Default.forbidUnsafeSettings)
            {
                if (FanSpeedSlider.Value < 40 && FanSpeedSlider.Value > 0)
                    FanSpeedSlider.Value = 40;
                else if (FanSpeedSlider.Value > 99)
                    FanSpeedSlider.Value = 99;
            }
            
            UpdateFanSpeedDisplay();
            
            if (isControlEnabled)
            {
                ApplyFanSpeed();
            }
        }
        
        private void UpdateFanSpeedDisplay()
        {
            currentFanSpeed = (int)FanSpeedSlider.Value;
            FanSpeedValueText.Text = $"{currentFanSpeed}%";
            
            // Update slider selection range for visual feedback
            var slider = FanSpeedSlider;
            if (slider.Template?.FindName("PART_SelectionRange", slider) is Border selectionRange)
            {
                selectionRange.Width = (currentFanSpeed / 100.0) * slider.ActualWidth;
            }
        }
        
        private void ApplyFanSpeed()
        {
            try
            {
                if (isControlEnabled)
                {
                    asusControl.SetFanSpeeds(currentFanSpeed);
                }
                SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying fan speed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void PresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tagValue)
            {
                if (int.TryParse(tagValue, out int presetValue))
                {
                    FanSpeedSlider.Value = presetValue;
                }
            }
        }
        
        private void RefreshCpuTemp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cpuTemp = asusControl.Thermal_Read_Cpu_Temperature();
                CpuTempText.Text = $"{cpuTemp}°C";
                
                // Update temperature gauge
                UpdateTemperatureGauge((double)cpuTemp);
            }
            catch (Exception ex)
            {
                CpuTempText.Text = "ERROR";
                MessageBox.Show($"Error reading CPU temperature: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void UpdateTemperatureGauge(double temperature)
        {
            // Assume max temp of 100°C for gauge calculation
            double maxTemp = 100.0;
            double percentage = Math.Min(temperature / maxTemp, 1.0);
            
            // Create arc path for temperature gauge
            double angle = percentage * 270; // 270 degrees max
            double radius = 52; // Slightly smaller than the background circle
            double centerX = 60;
            double centerY = 60;
            
            double startAngle = -135; // Start from bottom-left
            double endAngle = startAngle + angle;
            
            Point startPoint = GetPointOnCircle(centerX, centerY, radius, startAngle);
            Point endPoint = GetPointOnCircle(centerX, centerY, radius, endAngle);
            
            bool isLargeArc = angle > 180;
            
            string pathData = $"M {centerX},{centerY} L {startPoint.X},{startPoint.Y} A {radius},{radius} 0 {(isLargeArc ? 1 : 0)},1 {endPoint.X},{endPoint.Y} Z";
            
            if (TempGaugePath != null)
            {
                TempGaugePath.Data = Geometry.Parse(pathData);
                
                // Change color based on temperature
                if (temperature < 60)
                    TempGaugePath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#39FF14"));
                else if (temperature < 80)
                    TempGaugePath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700"));
                else
                    TempGaugePath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B35"));
            }
        }
        
        private Point GetPointOnCircle(double centerX, double centerY, double radius, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * Math.PI / 180;
            return new Point(
                centerX + radius * Math.Cos(angleInRadians),
                centerY + radius * Math.Sin(angleInRadians)
            );
        }
        
        private void RefreshFanSpeeds_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fanSpeeds = asusControl.GetFanSpeeds();
                
                for (int i = 0; i < Math.Min(fanSpeeds.Count, FanStatusItems.Count); i++)
                {
                    FanStatusItems[i].RPM = fanSpeeds[i];
                    // Calculate percentage based on typical max RPM (assume 3000 RPM max)
                    FanStatusItems[i].RPMPercent = Math.Min((fanSpeeds[i] / 3000.0) * 100, 100);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading fan speeds: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    public class FanStatusItem : INotifyPropertyChanged
    {
        private string _name;
        private int _rpm;
        private double _rpmPercent;
        
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        
        public int RPM
        {
            get => _rpm;
            set
            {
                _rpm = value;
                OnPropertyChanged(nameof(RPM));
            }
        }
        
        public double RPMPercent
        {
            get => _rpmPercent;
            set
            {
                _rpmPercent = value;
                OnPropertyChanged(nameof(RPMPercent));
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}