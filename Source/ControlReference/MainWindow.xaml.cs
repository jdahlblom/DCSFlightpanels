﻿using ClassLibraryCommon;
using ControlReference.Properties;
using DCS_BIOS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ControlReference.Windows;
using DCS_BIOS.EventArgs;
using DCS_BIOS.Json;
using DCS_BIOS.Interfaces;

namespace ControlReference
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window , IDisposable, IDcsBiosConnectionListener
    {
        private IEnumerable<DCSBIOSControl> _loadedControls = null;
        private readonly Timer _dcsStopGearTimer = new(5000);
        private DCSBIOS _dcsBios;
        private bool _formLoaded = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool _hasBeenCalledAlready;
        protected virtual void Dispose(bool disposing)
        {
            if (!_hasBeenCalledAlready)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _dcsStopGearTimer.Dispose();
                    _dcsBios?.Shutdown();
                    _dcsBios?.Dispose();
                    BIOSEventHandler.DetachConnectionListener(this);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.

                // TODO: set large fields to null.
                _hasBeenCalledAlready = true;
            }
        }
        
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_formLoaded)
                {
                    return;
                }

                DCSBIOSControlLocator.JSONDirectory = Settings.Default.DCSBiosJSONLocation;
                DCSAircraft.FillModulesListFromDcsBios(DCSBIOSCommon.GetDCSBIOSJSONDirectory(Settings.Default.DCSBiosJSONLocation), true);
                UpdateComboBoxModules();
                CreateDCSBIOS();
                StartupDCSBIOS();
                BIOSEventHandler.AttachConnectionListener(this);
                _formLoaded = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void SetFormState()
        {

        }

        private void CreateDCSBIOS()
        {
            if (_dcsBios != null)
            {
                return;
            }
            
            _dcsBios = new DCSBIOS(Settings.Default.DCSBiosIPFrom, Settings.Default.DCSBiosIPTo, int.Parse(Settings.Default.DCSBiosPortFrom), int.Parse(Settings.Default.DCSBiosPortTo), DcsBiosNotificationMode.AddressValue);
            if (!_dcsBios.HasLastException())
            {
                RotateGear(2000);
            }

            ImageDcsBiosConnected.Visibility = Visibility.Visible;
        }

        private void StartupDCSBIOS()
        {
            if (_dcsBios.IsRunning)
            {
                return;
            }

            _dcsBios?.Startup();

            _dcsStopGearTimer.Start();
        }
        
        private void MenuSetDCSBIOSPath_OnClick(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(0);
            if (settingsWindow.ShowDialog() == true)
            {
                if (settingsWindow.DCSBIOSChanged)
                {
                    DCSBIOSControlLocator.JSONDirectory = Settings.Default.DCSBiosJSONLocation;
                    DCSAircraft.FillModulesListFromDcsBios(DCSBIOSCommon.GetDCSBIOSJSONDirectory(Settings.Default.DCSBiosJSONLocation), true);
                    UpdateComboBoxModules();
                }
            }
        }

        private void UpdateComboBoxModules()
        {
            ComboBoxModules.DataContext = DCSAircraft.Modules;
            ComboBoxModules.ItemsSource = DCSAircraft.Modules;
            ComboBoxModules.Items.Refresh();
            ComboBoxModules.SelectedIndex = 0;
            UpdateComboBoxCategories();
        }
        private void UpdateComboBoxCategories()
        {
            var categoriesList = _loadedControls.Select(o => o.Category ).DistinctBy(o => o).ToList();
            categoriesList.Insert(0,"All");
            ComboBoxCategory.DataContext = categoriesList;
            ComboBoxCategory.ItemsSource = categoriesList;
            ComboBoxCategory.Items.Refresh();
            ComboBoxCategory.SelectedIndex = 0;
        }

        private void ComboBoxModules_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedModule = (DCSAircraft)ComboBoxModules.SelectedItem;
                DCSBIOSControlLocator.DCSAircraft = selectedModule;
                _loadedControls = DCSBIOSControlLocator.GetControls(true);
                UpdateComboBoxCategories();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        public void DcsBiosConnectionActive(object sender, DCSBIOSConnectionEventArgs e)
        {
            try
            {
                Dispatcher?.BeginInvoke((Action)(() => RotateGear()));
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RotateGear(int howLong = 5000)
        {
            try
            {
                if (ImageDcsBiosConnected.IsEnabled)
                {
                    return;
                }

                ImageDcsBiosConnected.IsEnabled = true;
                if (_dcsStopGearTimer.Enabled)
                {
                    _dcsStopGearTimer.Stop();
                }

                _dcsStopGearTimer.Interval = howLong;
                _dcsStopGearTimer.Start();
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void MenuItemExit_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemDiscord_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonSearchControls_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            try
            {
                Dispose(true);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MainWindow_OnLocationChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_formLoaded)
                {
                    return;
                }

                if (Top > 0 && Left > 0)
                {
                    Settings.Default.MainWindowTop = Top;
                    Settings.Default.MainWindowLeft = Left;
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (!_formLoaded)
                {
                    return;
                }

                if (WindowState != WindowState.Minimized && WindowState != WindowState.Maximized)
                {
                    Settings.Default.MainWindowHeight = Height;
                    Settings.Default.MainWindowWidth = Width;
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

    }
}
