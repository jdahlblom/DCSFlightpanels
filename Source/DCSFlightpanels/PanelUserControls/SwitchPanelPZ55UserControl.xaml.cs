﻿namespace DCSFlightpanels.PanelUserControls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using ClassLibraryCommon;

    using DCSFlightpanels.Bills;
    using DCSFlightpanels.CustomControls;
    using DCSFlightpanels.Interfaces;
    using DCSFlightpanels.Windows;

    using NonVisuals;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;
    using NonVisuals.Saitek;
    using NonVisuals.Saitek.Panels;
    using NonVisuals.Saitek.Switches;
    
    using Brush = System.Windows.Media.Brush;
    using Brushes = System.Windows.Media.Brushes;
    using Image = System.Windows.Controls.Image;
    using SwitchPanelPZ55Keys = MEF.SwitchPanelPZ55Keys;

    /// <summary>
    /// Interaction logic for SwitchPanelPZ55UserControl.xaml
    /// </summary>
    public partial class SwitchPanelPZ55UserControl : UserControlBase, IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl, IPanelUI
    {

        private readonly SwitchPanelPZ55 _switchPanelPZ55;

        private readonly Image[] _imageArrayUpper = new Image[4];
        private readonly Image[] _imageArrayLeft = new Image[4];
        private readonly Image[] _imageArrayRight = new Image[4];
        private bool _textBoxBillsSet;




        public SwitchPanelPZ55UserControl(HIDSkeleton hidSkeleton, TabItem parentTabItem)
        {
            InitializeComponent();
            hidSkeleton.HIDReadDevice.Removed += DeviceRemovedHandler;

            ParentTabItem = parentTabItem;
            _switchPanelPZ55 = new SwitchPanelPZ55(hidSkeleton);

            AppEventHandler.AttachGamingPanelListener(this);
            _imageArrayUpper[0] = ImagePZ55LEDDarkUpper;
            _imageArrayUpper[1] = ImagePZ55LEDGreenUpper;
            _imageArrayUpper[2] = ImagePZ55LEDYellowUpper;
            _imageArrayUpper[3] = ImagePZ55LEDRedUpper;

            _imageArrayLeft[0] = ImagePZ55LEDDarkLeft;
            _imageArrayLeft[1] = ImagePZ55LEDGreenLeft;
            _imageArrayLeft[2] = ImagePZ55LEDYellowLeft;
            _imageArrayLeft[3] = ImagePZ55LEDRedLeft;

            _imageArrayRight[0] = ImagePZ55LEDDarkRight;
            _imageArrayRight[1] = ImagePZ55LEDGreenRight;
            _imageArrayRight[2] = ImagePZ55LEDYellowRight;
            _imageArrayRight[3] = ImagePZ55LEDRedRight;
        }


        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _switchPanelPZ55.Dispose();
                    AppEventHandler.DetachGamingPanelListener(this);
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }
        
        private void SwitchPanelPZ55UserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            SetTextBoxBills();
            LoadComboBoxesManualLeds();
            SetContextMenuClickHandlers();
            UserControlLoaded = true;
            ShowGraphicConfiguration();
        }

        private void LoadComboBoxesManualLeds()
        {
            ManualLedUpCombo.ItemsSource = Enum.GetValues(typeof(PanelLEDColor));
            ManualLedDownCombo.ItemsSource = Enum.GetValues(typeof(PanelLEDColor));
            ManualLedTransCombo.ItemsSource = Enum.GetValues(typeof(PanelLEDColor));
            for (int i = 1; i <= 30; i++)
            {
                ManualLedTransSecondsCombo.Items.Add(i);
            }

            ManualLedUpCombo.SelectionChanged -= ManualLedUpCombo_SelectionChanged;
            ManualLedTransCombo.SelectionChanged -= ManualLedTransCombo_SelectionChanged;
            ManualLedDownCombo.SelectionChanged -= ManualLedDownCombo_SelectionChanged;
            ManualLedTransSecondsCombo.SelectionChanged -= ManualLedTransSecondsCombo_SelectionChanged;

            ManualLedUpCombo.SelectedValue = _switchPanelPZ55.ManualLandingGearLedsColorUp;
            ManualLedDownCombo.SelectedValue = _switchPanelPZ55.ManualLandingGearLedsColorDown;
            ManualLedTransCombo.SelectedValue = _switchPanelPZ55.ManualLandingGearLedsColorTrans;
            ManualLedTransSecondsCombo.SelectedValue = _switchPanelPZ55.ManualLandingGearTransTimeSeconds;

            ManualLedUpCombo.SelectionChanged += ManualLedUpCombo_SelectionChanged;
            ManualLedTransCombo.SelectionChanged += ManualLedTransCombo_SelectionChanged;
            ManualLedDownCombo.SelectionChanged += ManualLedDownCombo_SelectionChanged;
            ManualLedTransSecondsCombo.SelectionChanged += ManualLedTransSecondsCombo_SelectionChanged;
        }
        

        public override GamingPanel GetGamingPanel()
        {
            return _switchPanelPZ55;
        }

        public override GamingPanelEnum GetPanelType()
        {
            return GamingPanelEnum.PZ55SwitchPanel;
        }

        public string GetName()
        {
            return GetType().Name;
        }

        public void ProfileSelected(object sender, AirframeEventArgs e)
        {
            try
            {
                foreach (var image in Common.FindVisualChildren<Image>(this))
                {
                    if (image.Name.StartsWith("ImagePZ55LED") && Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly))
                    {
                        image.ContextMenu = null;
                    }
                    else
                        if (image.Name.StartsWith("ImagePZ55LED") && image.ContextMenu == null && Common.IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled))
                    {
                        image.ContextMenu = (ContextMenu)Resources["PZ55LEDContextMenu"];
                        if (image.ContextMenu != null)
                        {
                            image.ContextMenu.Tag = image.Name;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e)
        {
            try
            {
                //
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                if (e.PanelType == GamingPanelEnum.PZ55SwitchPanel && e.HidInstance.Equals(_switchPanelPZ55.HIDInstanceId))
                {
                    NotifySwitchChanges(e.Switches);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void PanelBindingReadFromFile(object sender, PanelBindingReadFromFileEventArgs e)
        {
            try
            {
                if (e.PanelBinding.PanelType == GamingPanelEnum.PZ55SwitchPanel && _switchPanelPZ55.HIDInstanceId == e.PanelBinding.HIDInstance)
                {
                    ShowGraphicConfiguration();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void LedLightChanged(object sender, LedLightChangeEventArgs e)
        {
            try
            {
                if (_switchPanelPZ55.HIDInstanceId.Equals(e.HIDInstanceId))
                {
                    var position = (SwitchPanelPZ55LEDPosition)e.LEDPosition.Position;
                    var imageArray = _imageArrayUpper;

                    switch (position)
                    {
                        case SwitchPanelPZ55LEDPosition.UP:
                            {
                                HideLedImages(SwitchPanelPZ55LEDPosition.UP);
                                break;
                            }
                        case SwitchPanelPZ55LEDPosition.LEFT:
                            {
                                HideLedImages(SwitchPanelPZ55LEDPosition.LEFT);
                                imageArray = _imageArrayLeft;
                                break;
                            }
                        case SwitchPanelPZ55LEDPosition.RIGHT:
                            {
                                HideLedImages(SwitchPanelPZ55LEDPosition.RIGHT);
                                imageArray = _imageArrayRight;
                                break;
                            }
                    }

                    switch (e.LEDColor)
                    {
                        case PanelLEDColor.DARK:
                            {
                                Dispatcher?.BeginInvoke((Action)(() => imageArray[0].Visibility = Visibility.Visible));
                                break;
                            }
                        case PanelLEDColor.GREEN:
                            {
                                Dispatcher?.BeginInvoke((Action)(() => imageArray[1].Visibility = Visibility.Visible));
                                break;
                            }
                        case PanelLEDColor.YELLOW:
                            {
                                Dispatcher?.BeginInvoke((Action)(() => imageArray[2].Visibility = Visibility.Visible));
                                break;
                            }
                        case PanelLEDColor.RED:
                            {
                                Dispatcher?.BeginInvoke((Action)(() => imageArray[3].Visibility = Visibility.Visible));
                                break;
                            }
                    }

                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void HideLedImages(SwitchPanelPZ55LEDPosition switchPanelPZ55LEDPosition)
        {
            var imageArray = _imageArrayUpper;
            switch (switchPanelPZ55LEDPosition)
            {
                case SwitchPanelPZ55LEDPosition.UP:
                    {

                        break;
                    }
                case SwitchPanelPZ55LEDPosition.LEFT:
                    {
                        imageArray = _imageArrayLeft;
                        break;
                    }
                case SwitchPanelPZ55LEDPosition.RIGHT:
                    {
                        imageArray = _imageArrayRight;
                        break;
                    }
            }
            for (int i = 0; i < 4; i++)
            {
                var image1 = imageArray[i];
                Dispatcher?.BeginInvoke((Action)(() => image1.Visibility = Visibility.Collapsed));
            }
        }
        
        public void DeviceAttached(object sender, PanelEventArgs e)
        {
            try
            {
                if (e.PanelType == GamingPanelEnum.PZ55SwitchPanel && e.HidInstance.Equals(_switchPanelPZ55.HIDInstanceId))
                {
                    //Dispatcher?.BeginInvoke((Action)(() => _parentTabItem.Header = ParentTabItemHeader + " (connected)"));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void DeviceDetached(object sender, PanelEventArgs e)
        {
            try
            {
                if (e.PanelType == GamingPanelEnum.PZ55SwitchPanel && e.HidInstance.Equals(_switchPanelPZ55.HIDInstanceId))
                {
                    //Dispatcher?.BeginInvoke((Action)(() => _parentTabItem.Header = ParentTabItemHeader + " (disconnected)"));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SettingsApplied(object sender, PanelEventArgs e)
        {
            try
            {
                if (e.PanelType == GamingPanelEnum.PZ55SwitchPanel && e.HidInstance.Equals(_switchPanelPZ55.HIDInstanceId))
                {
                    Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher?.BeginInvoke((Action)(() => TextBoxLogPZ55.Text = string.Empty));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SettingsModified(object sender, PanelEventArgs e)
        {
            try
            {
                if (_switchPanelPZ55.HIDInstanceId == e.HidInstance)
                {
                    Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MouseDownFocusLogTextBox(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBoxLogPZ55.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ClearAll(bool clearAlsoProfile)
        {
            foreach (var textBox in Common.FindVisualChildren<PZ55TextBox>(this))
            {
                if (textBox == TextBoxLogPZ55 || textBox.Bill == null)
                {
                    continue;
                }
                textBox.Bill.ClearAll();
            }
            if (clearAlsoProfile)
            {
                _switchPanelPZ55.ClearSettings(true);
            }

            ShowGraphicConfiguration();
        }

        private void SetTextBoxBills()
        {
            if (_textBoxBillsSet || !Common.FindVisualChildren<PZ55TextBox>(this).Any())
            {
                return;
            }
            foreach (var textBox in Common.FindVisualChildren<PZ55TextBox>(this))
            {
                if (textBox.Bill != null || textBox == TextBoxLogPZ55)
                {
                    continue;
                }

                textBox.Bill = new BillPZ55(this, _switchPanelPZ55, textBox);
            }
            _textBoxBillsSet = true;
        }

        private void SetContextMenuClickHandlers()
        {
            if (Common.IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled))
            {
                foreach (var image in Common.FindVisualChildren<Image>(this))
                {
                    if (image.ContextMenu == null && image.Name.StartsWith("ImagePZ55LED"))
                    {
                        image.ContextMenu = (ContextMenu)Resources["PZ55LEDContextMenu"];
                        if (image.ContextMenu != null)
                        {
                            image.ContextMenu.Tag = image.Name;
                        }
                    }
                }
            }
        }

        private void ContextConfigureLandingGearLEDClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var menuItem = (MenuItem)sender;
                var contextMenu = (ContextMenu)menuItem.Parent;
                var imageName = contextMenu.Tag.ToString();
                var position = SwitchPanelPZ55LEDPosition.LEFT;

                if (imageName.Contains("Upper"))
                {
                    position = SwitchPanelPZ55LEDPosition.UP;
                }
                else if (imageName.Contains("Left"))
                {
                    position = SwitchPanelPZ55LEDPosition.LEFT;
                }
                else if (imageName.Contains("Right"))
                {
                    position = SwitchPanelPZ55LEDPosition.RIGHT;
                }
                var ledConfigsWindow = new LEDConfigsWindow( "Set configuration for LED : " + position, new SaitekPanelLEDPosition(position), _switchPanelPZ55.GetLedDcsBiosOutputs(position), _switchPanelPZ55);
                if (ledConfigsWindow.ShowDialog() == true)
                {
                    //must include position because if user has deleted all entries then there is nothing to go after regarding position
                    _switchPanelPZ55.SetLedDcsBiosOutput(position, ledConfigsWindow.ColorOutputBindings);
                }
                SetConfigExistsImageVisibility();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void ImageLEDClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //Only left mouse clicks here!
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    return;
                }
                var imageArray = new Image[4];
                var clickedImage = (Image)sender;
                var position = SwitchPanelPZ55LEDPosition.UP;
                //Name of graphics file
                var imageSource = (string)clickedImage.Tag;

                if (clickedImage.Name.Contains("Upper"))
                {
                    position = SwitchPanelPZ55LEDPosition.UP;
                    imageArray = _imageArrayUpper;
                }
                else if (clickedImage.Name.Contains("Left"))
                {
                    position = SwitchPanelPZ55LEDPosition.LEFT;
                    imageArray = _imageArrayLeft;
                }
                else if (clickedImage.Name.Contains("Right"))
                {
                    position = SwitchPanelPZ55LEDPosition.RIGHT;
                    imageArray = _imageArrayRight;
                }
                var nextImageIndex = 0;
                HideLedImages(position);
                for (int i = 0; i < 4; i++)
                {
                    var image = imageArray[i];
                    if (clickedImage.Equals(image))
                    {
                        nextImageIndex = i + 1;
                        if (nextImageIndex > 3)
                        {
                            nextImageIndex = 0;
                        }
                    }
                }

                imageArray[nextImageIndex].Visibility = Visibility.Visible;


                if (imageArray[nextImageIndex].Name.Contains("Dark"))
                {
                    _switchPanelPZ55.SetLandingGearLED(position, PanelLEDColor.DARK);
                }
                else if (imageArray[nextImageIndex].Name.Contains("Green"))
                {
                    _switchPanelPZ55.SetLandingGearLED(position, PanelLEDColor.GREEN);
                }
                else if (imageArray[nextImageIndex].Name.Contains("Yellow"))
                {
                    _switchPanelPZ55.SetLandingGearLED(position, PanelLEDColor.YELLOW);
                }
                else if (imageArray[nextImageIndex].Name.Contains("Red"))
                {
                    _switchPanelPZ55.SetLandingGearLED(position, PanelLEDColor.RED);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonClearAllClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Clear all settings for the Switch Panel?", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    ClearAll(true);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void NotifySwitchChanges(HashSet<object> switches)
        {
            try
            {
                //Set focus to this so that virtual keypresses won't affect settings
                Dispatcher?.BeginInvoke((Action)(() => TextBoxLogPZ55.Focus()));
                foreach (var switchPanelKey in switches)
                {
                    var key = (SwitchPanelKey)switchPanelKey;

                    if (_switchPanelPZ55.ForwardPanelEvent)
                    {
                        if (!string.IsNullOrEmpty(_switchPanelPZ55.GetKeyPressForLoggingPurposes(key)))
                        {
                            Dispatcher?.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogPZ55.Text =
                                 TextBoxLogPZ55.Text.Insert(0, _switchPanelPZ55.GetKeyPressForLoggingPurposes(key) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher?.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogPZ55.Text =
                             TextBoxLogPZ55.Text = TextBoxLogPZ55.Text.Insert(0, "No action taken, panel events Disabled.\n")));
                    }
                }
                SetGraphicsState(switches);
            }
            catch (Exception ex)
            {
                Dispatcher?.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogPZ55.Text = TextBoxLogPZ55.Text.Insert(0, "0x16" + ex.Message + ".\n")));
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetGraphicsState(HashSet<object> switches)
        {
            try
            {
                foreach (var switchPanelKeyObject in switches)
                {
                    var switchPanelKey = (SwitchPanelKey)switchPanelKeyObject;
                    switch (switchPanelKey.SwitchPanelPZ55Key)
                    {
                        case SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageAvMasterOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //This button is special. The Panel reports the button ON when it us switched upwards towards [CLOSE]. This is confusing semantics.
                                        //The button is considered OFF by the program when it is upwards which is opposite to the other buttons which all are considered ON when upwards.
                                        ImageCowlClosed.Visibility = !key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageDeIceOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageFuelPumpOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.LEVER_GEAR_DOWN:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageGearUp.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.LEVER_GEAR_UP:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageGearUp.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageBeaconOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLandingOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageNavOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImagePanelOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageStrobeOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageTaxiOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageMasterAltOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageMasterBatOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImagePitotHeatOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                    }
                    if (switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH || switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT ||
                        switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT || switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_OFF ||
                        switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START)
                    {
                        var key = switchPanelKey;
                        Dispatcher?.BeginInvoke(
                            (Action)delegate
                            {
                                if (key.IsOn)
                                {
                                    ImageKnobAll.Visibility = key.IsOn && key.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH ? Visibility.Visible : Visibility.Collapsed;
                                    ImageKnobL.Visibility = key.IsOn && key.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT ? Visibility.Visible : Visibility.Collapsed;
                                    ImageKnobR.Visibility = key.IsOn && key.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT ? Visibility.Visible : Visibility.Collapsed;
                                    ImageKnobStart.Visibility = key.IsOn && key.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START ? Visibility.Visible : Visibility.Collapsed;
                                }
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ShowGraphicConfiguration()
        {
            try
            {
                if (!UserControlLoaded || !_textBoxBillsSet)
                {
                    return;
                }
                foreach (var keyBinding in _switchPanelPZ55.KeyBindingsHashSet)
                {
                    var textBox = (PZ55TextBox)GetTextBox(keyBinding.SwitchPanelPZ55Key, keyBinding.WhenTurnedOn);
                    if (keyBinding.OSKeyPress != null)
                    {
                        textBox.Bill.KeyPress = keyBinding.OSKeyPress;
                    }
                    else
                    {
                        textBox.Bill.KeyPress = null;
                    }
                }

                foreach (var operatingSystemCommand in _switchPanelPZ55.OSCommandList)
                {
                    var textBox = (PZ55TextBox)GetTextBox(operatingSystemCommand.SwitchPanelPZ55Key, operatingSystemCommand.WhenTurnedOn);
                    if (operatingSystemCommand.OSCommandObject != null)
                    {
                        textBox.Bill.OSCommandObject = operatingSystemCommand.OSCommandObject;
                    }
                    else
                    {
                        textBox.Bill.OSCommandObject = null;
                    }
                }

                foreach (var dcsBiosBinding in _switchPanelPZ55.DCSBiosBindings)
                {
                    var textBox = (PZ55TextBox)GetTextBox(dcsBiosBinding.SwitchPanelPZ55Key, dcsBiosBinding.WhenTurnedOn);
                    if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        textBox.Bill.DCSBIOSBinding = dcsBiosBinding;
                    }
                    else
                    {
                        textBox.Bill.DCSBIOSBinding = null;
                    }
                }

                SetTextBoxBackgroundColors(Brushes.White); //Maybe we can remove this function and only retain the _textBoxBillsSet = true; ?
                foreach (var bipLinkPZ55 in _switchPanelPZ55.BIPLinkHashSet)
                {
                    var textBox = (PZ55TextBox)GetTextBox(bipLinkPZ55.SwitchPanelPZ55Key, bipLinkPZ55.WhenTurnedOn);
                    if (bipLinkPZ55.BIPLights.Count > 0)
                    {
                        textBox.Bill.BipLink = bipLinkPZ55;
                    }
                    else
                    {
                        textBox.Bill.BipLink = null;
                    }
                }

                CheckBoxManualLeDs.IsChecked = _switchPanelPZ55.ManualLandingGearLeds;
                SetManualLedColorsSelectionVisibility(_switchPanelPZ55.ManualLandingGearLeds);
                SetConfigExistsImageVisibility();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetTextBoxBackgroundColors(Brush brush)
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (!textBox.IsFocused && textBox.Background != Brushes.Yellow)
                {
                    textBox.Background = brush;
                }
            }
            _textBoxBillsSet = true;
        }

        private void SetConfigExistsImageVisibility()
        {
            foreach (SwitchPanelPZ55LEDPosition value in Enum.GetValues(typeof(SwitchPanelPZ55LEDPosition)))
            {
                var hasConfiguration = _switchPanelPZ55.LedIsConfigured(value);
                switch (value)
                {
                    case SwitchPanelPZ55LEDPosition.UP:
                        {
                            ImageConfigFoundUpper.Visibility = hasConfiguration ? Visibility.Visible : Visibility.Hidden;
                            break;
                        }
                    case SwitchPanelPZ55LEDPosition.LEFT:
                        {
                            ImageConfigFoundLeft.Visibility = hasConfiguration ? Visibility.Visible : Visibility.Hidden;
                            break;
                        }
                    case SwitchPanelPZ55LEDPosition.RIGHT:
                        {
                            ImageConfigFoundRight.Visibility = hasConfiguration ? Visibility.Visible : Visibility.Hidden;
                            break;
                        }
                }
            }
        }

        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_switchPanelPZ55 != null)
                {
                    TextBoxLogPZ55.Text = string.Empty;
                    TextBoxLogPZ55.Text = _switchPanelPZ55.HIDInstanceId;
                    Clipboard.SetText(_switchPanelPZ55.HIDInstanceId);
                    MessageBox.Show("The Instance Id for the panel has been copied to the Clipboard.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void CheckBoxManualLEDs_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_switchPanelPZ55 != null)
                {
                    _switchPanelPZ55.ManualLandingGearLeds = CheckBoxManualLeDs.IsChecked.HasValue && CheckBoxManualLeDs.IsChecked.Value;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public PanelSwitchOnOff GetSwitch(TextBox textBox)
        {
            return textBox switch
            {
                TextBox t when t.Equals(TextBoxKnobOff) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_OFF, true),
                TextBox t when t.Equals(TextBoxKnobR) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT, true),
                TextBox t when t.Equals(TextBoxKnobL) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT, true),
                TextBox t when t.Equals(TextBoxKnobAll) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH, true),
                TextBox t when t.Equals(TextBoxKnobStart) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_START, true),
                TextBox t when t.Equals(TextBoxCowlOpen) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, true),
                TextBox t when t.Equals(TextBoxCowlClose) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, false),
                TextBox t when t.Equals(TextBoxPanelOn) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, true),
                TextBox t when t.Equals(TextBoxPanelOff) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, false),
                TextBox t when t.Equals(TextBoxBeaconOn) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, true),
                TextBox t when t.Equals(TextBoxBeaconOff) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, false),
                TextBox t when t.Equals(TextBoxNavOn) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, true),
                TextBox t when t.Equals(TextBoxNavOff) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, false),
                TextBox t when t.Equals(TextBoxStrobeOn) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, true),
                TextBox t when t.Equals(TextBoxStrobeOff) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, false),
                TextBox t when t.Equals(TextBoxTaxiOn) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, true),
                TextBox t when t.Equals(TextBoxTaxiOff) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, false),
                TextBox t when t.Equals(TextBoxLandingOn) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, true),
                TextBox t when t.Equals(TextBoxLandingOff) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, false),
                TextBox t when t.Equals(TextBoxMasterBatOn) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, true),
                TextBox t when t.Equals(TextBoxMasterBatOff) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, false),
                TextBox t when t.Equals(TextBoxMasterAltOn) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, true),
                TextBox t when t.Equals(TextBoxMasterAltOff) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, false),
                TextBox t when t.Equals(TextBoxAvionicsMasterOn) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, true),
                TextBox t when t.Equals(TextBoxAvionicsMasterOff) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, false),
                TextBox t when t.Equals(TextBoxFuelPumpOn) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, true),
                TextBox t when t.Equals(TextBoxFuelPumpOff) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, false),
                TextBox t when t.Equals(TextBoxDeIceOn) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, true),
                TextBox t when t.Equals(TextBoxDeIceOff) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, false),
                TextBox t when t.Equals(TextBoxPitotHeatOn) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, true),
                TextBox t when t.Equals(TextBoxPitotHeatOff) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, false),
                TextBox t when t.Equals(TextBoxGearUp) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.LEVER_GEAR_UP, true),
                TextBox t when t.Equals(TextBoxGearDown) => new PZ55SwitchOnOff(SwitchPanelPZ55Keys.LEVER_GEAR_DOWN, true),
                _ => throw new Exception($"Failed to find key based on text box (SwitchPanelPZ55UserControl) {textBox.Name}")
            };
        }

        public TextBox GetTextBox(object panelSwitch, bool isTurnedOn)
        {
            var key = (SwitchPanelPZ55Keys)panelSwitch;
            return (key, isTurnedOn) switch
            {
                (SwitchPanelPZ55Keys.KNOB_ENGINE_OFF, true) => TextBoxKnobOff,
                (SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT, true) => TextBoxKnobR,
                (SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT, true) => TextBoxKnobL,
                (SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH, true) => TextBoxKnobAll,
                (SwitchPanelPZ55Keys.KNOB_ENGINE_START, true) => TextBoxKnobStart,
                (SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, true) => TextBoxCowlOpen,
                (SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, false) => TextBoxCowlClose,
                (SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, true) => TextBoxPanelOn,
                (SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, false) => TextBoxPanelOff,
                (SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, true) => TextBoxBeaconOn,
                (SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, false) => TextBoxBeaconOff,
                (SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, true) => TextBoxNavOn,
                (SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, false) => TextBoxNavOff,
                (SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, true) => TextBoxStrobeOn,
                (SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, false) => TextBoxStrobeOff,
                (SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, true) => TextBoxTaxiOn,
                (SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, false) => TextBoxTaxiOff,
                (SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, true) => TextBoxLandingOn,
                (SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, false) => TextBoxLandingOff,
                (SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, true) => TextBoxMasterBatOn,
                (SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, false) => TextBoxMasterBatOff,
                (SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, true) => TextBoxMasterAltOn,
                (SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, false) => TextBoxMasterAltOff,
                (SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, true) => TextBoxAvionicsMasterOn,
                (SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, false) => TextBoxAvionicsMasterOff,
                (SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, true) => TextBoxFuelPumpOn,
                (SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, false) => TextBoxFuelPumpOff,
                (SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, true) => TextBoxDeIceOn,
                (SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, false) => TextBoxDeIceOff,
                (SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, true) => TextBoxPitotHeatOn,
                (SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, false) => TextBoxPitotHeatOff,
                (SwitchPanelPZ55Keys.LEVER_GEAR_UP, true) => TextBoxGearUp,
                (SwitchPanelPZ55Keys.LEVER_GEAR_DOWN, true) => TextBoxGearDown,
                _ => throw new Exception($"Failed to find text box based on key (SwitchPanelPZ55UserControl) {key} and value {isTurnedOn}")
            };
        }

        private void ButtonDEV_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                /*var panelEventHandler = new PanelEventHandler();
                KeyBindingPZ55 kkeyBindingPZ55 = new KeyBindingPZ55();
                foreach (var keyBindingPZ55 in _switchPanelPZ55.KeyBindingsHashSet)
                {
                    kkeyBindingPZ55 = keyBindingPZ55;
                }*/

                //panelEventHandler.PanelEvent("A-10C", "HIDID", (int)PluginGamingPanelEnum.PZ55SwitchPanel, (int)SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH, true, kkeyBindingPZ55.OSKeyPress.KeySequence);
                //var pluginManager = new PluginManager();
                //pluginManager.LoadPlugins();
                //pluginManager.PanelEventHandler.PanelEvent("A-10C", "HIDID", (int)PluginGamingPanelEnum.PZ55SwitchPanel, (int)SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH, 1, 0);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonGetIdentify_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _switchPanelPZ55.Identify();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonClearSettings_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Clear all settings?", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    ClearAll(true);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ManualLedUpCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _switchPanelPZ55.ManualLandingGearLedsColorUp = (PanelLEDColor)((ComboBox)sender).SelectedValue;
        }

        private void ManualLedTransCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _switchPanelPZ55.ManualLandingGearLedsColorTrans = (PanelLEDColor)((ComboBox)sender).SelectedValue;
        }

        private void ManualLedDownCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _switchPanelPZ55.ManualLandingGearLedsColorDown = (PanelLEDColor)((ComboBox)sender).SelectedValue;
        }

        private void ManualLedTransSecondsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _switchPanelPZ55.ManualLandingGearTransTimeSeconds = (int)((ComboBox)sender).SelectedValue;
        }

        private void SetManualLedColorsSelectionVisibility(bool isVisible)
        {
            var visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
            ManualLedUpCombo.Visibility = visibility;
            ManualLedTransCombo.Visibility = visibility;
            ManualLedDownCombo.Visibility = visibility;
            ManualLedTransSecondsCombo.Visibility = visibility;

            ManualLedUpLabel.Visibility = visibility;
            ManualLedTransLabel.Visibility = visibility;
            ManualLedDownLabel.Visibility = visibility;
            ManualLedTransSecondsLabel.Visibility = visibility;
        }
    }
}
