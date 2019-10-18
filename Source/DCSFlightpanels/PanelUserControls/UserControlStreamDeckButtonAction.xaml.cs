﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using DCSFlightpanels.TagDataClasses;
using NonVisuals;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckButtonAction.xaml
    /// </summary>
    public partial class UserControlStreamDeckButtonAction : UserControlBase
    {
        private IStreamDeckButtonAction panelResult = null;
        private bool _textBoxTagsSet;

        private KeyPress _keyPress;
        private DCSBIOSActionBindingStreamDeck _dcsbiosActionBinding;
        private OSCommand _osCommand;
        private StreamDeckLayer _streamDeckLayer;

        private IStreamDeckUIParent _streamDeckUIParent;
        private IGlobalHandler _globalHandler;
        private bool _isLoaded = false;
        private bool _isDirty = false;


        public UserControlStreamDeckButtonAction()
        {
            InitializeComponent();
        }

        private void UserControlStreamDeckButtonAction_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                return;
            }
            SetTextBoxTagObjects();
            _isLoaded = true;
        }

        public void Clear()
        {
            TextBoxDCSBIOSActionButtonOff.Clear();
            TextBoxDCSBIOSActionButtonOn.Clear();
            TextBoxKeyPressButtonOff.Clear();
            TextBoxKeyPressButtonOn.Clear();
            TextBoxOSCommandButtonOff.Clear();
            TextBoxOSCommandButtonOn.Clear();
            ComboBoxLayerNavigationButtonOff.ItemsSource = null;
            ComboBoxLayerNavigationButtonOn.ItemsSource = null;
            ComboBoxLayerNavigationButtonOn.ItemsSource = null;
            ComboBoxLayerNavigationButtonOff.ItemsSource = null;
            panelResult = null;
        }



        public void SetFormState()
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }
                StackPanelButtonKeyPressSettings.Visibility = SDUIParent.GetButtonActionType() == EnumStreamDeckButtonActionType.KeyPress ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSSettings.Visibility = SDUIParent.GetButtonActionType() == EnumStreamDeckButtonActionType.DCSBIOS ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonOSCommandSettings.Visibility = SDUIParent.GetButtonActionType() == EnumStreamDeckButtonActionType.OSCommand ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonLayerNavigationSettings.Visibility = SDUIParent.GetButtonActionType() == EnumStreamDeckButtonActionType.LayerNavigation ? Visibility.Visible : Visibility.Collapsed;

                ButtonDeleteKeySequenceButtonOn.IsEnabled = ((TagDataClassStreamDeck)TextBoxKeyPressButtonOn.Tag).ContainsKeySequence() ||
                                                            ((TagDataClassStreamDeck)TextBoxKeyPressButtonOn.Tag).ContainsOSKeyPress();
                ButtonDeleteKeySequenceButtonOff.IsEnabled = ((TagDataClassStreamDeck)TextBoxKeyPressButtonOff.Tag).ContainsKeySequence() ||
                                                            ((TagDataClassStreamDeck)TextBoxKeyPressButtonOff.Tag).ContainsOSKeyPress();
                /*ButtonDeleteDCSBIOSActionButtonOn.IsEnabled = ((TagDataClassStreamDeck)ButtonDeleteDCSBIOSActionButtonOn.Tag).ContainsDCSBIOS();
                ButtonDeleteDCSBIOSActionButtonOff.IsEnabled = ((TagDataClassStreamDeck)ButtonDeleteDCSBIOSActionButtonOff.Tag).ContainsDCSBIOS();
                ButtonDeleteOSCommandButtonOn.IsEnabled = ((TagDataClassStreamDeck)ButtonDeleteOSCommandButtonOn.Tag).ContainsOSCommand();
                ButtonDeleteOSCommandButtonOff.IsEnabled = ((TagDataClassStreamDeck)ButtonDeleteOSCommandButtonOff.Tag).ContainsOSCommand();*/


            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471473, ex);
            }
        }



        private void SetTextBoxTagObjects()
        {
            TextBoxKeyPressButtonOn.Tag = new TagDataClassStreamDeck(TextBoxKeyPressButtonOn, new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), true));
            TextBoxKeyPressButtonOff.Tag = new TagDataClassStreamDeck(TextBoxKeyPressButtonOff, new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), false));
            _textBoxTagsSet = true;
        }


        private void SetIsDirty()
        {
            _isDirty = true;
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => _isDirty = value;
        }

        public bool HasConfig => _keyPress != null || _dcsbiosActionBinding != null || _osCommand != null || _streamDeckLayer != null;

        public IGlobalHandler GlobalHandler
        {
            get => _globalHandler;
            set => _globalHandler = value;
        }

        public IStreamDeckUIParent SDUIParent
        {
            get => _streamDeckUIParent;
            set => _streamDeckUIParent = value;
        }


        public StreamDeckButtonOnOff GetStreamDeckButtonOnOff(TextBox textBox)
        {
            try
            {
                switch (_streamDeckUIParent.GetButtonActionType())
                {
                    case EnumStreamDeckButtonActionType.KeyPress:
                        {
                            if (textBox.Equals(TextBoxKeyPressButtonOn))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), true);
                            }
                            if (textBox.Equals(TextBoxKeyPressButtonOff))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), false);
                            }

                            break;
                        }
                    case EnumStreamDeckButtonActionType.DCSBIOS:
                        {
                            if (textBox.Equals(TextBoxDCSBIOSActionButtonOn))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), true);
                            }
                            if (textBox.Equals(TextBoxDCSBIOSActionButtonOff))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), false);
                            }

                            break;
                        }
                    case EnumStreamDeckButtonActionType.OSCommand:
                        {
                            if (textBox.Equals(TextBoxOSCommandButtonOn))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), true);
                            }
                            if (textBox.Equals(TextBoxOSCommandButtonOff))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), false);
                            }

                            break;
                        }
                    case EnumStreamDeckButtonActionType.LayerNavigation:
                        {
                            /*if (textBox.Equals(ComboBoxLayerNavigationButtonOn))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), true);
                            }
                            if (textBox.Equals(ComboBoxLayerNavigationButtonOff))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), false);
                            }
                            */
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
            throw new Exception("Failed to determine focused component (GetStreamDeckButtonOnOff) ");
        }

        private void ButtonAddEditKeySequenceButtonOn_OnClick(object sender, RoutedEventArgs e)
        {
            AddEditKeyPress(TextBoxKeyPressButtonOn);
        }
        
        private void ButtonAddEditKeySequenceButtonOff_OnClick(object sender, RoutedEventArgs e)
        {
            AddEditKeyPress(TextBoxKeyPressButtonOff);
        }

        private void AddEditKeyPress(TextBox textBox)
        {
            try
            {
                var keySequenceWindow = ((TagDataClassStreamDeck)textBox.Tag).ContainsKeySequence() ? 
                    new KeySequenceWindow(textBox.Text, ((TagDataClassStreamDeck)textBox.Tag).GetKeySequence()) : 
                    new KeySequenceWindow();
                keySequenceWindow.ShowDialog();
                if (keySequenceWindow.DialogResult.HasValue && keySequenceWindow.DialogResult.Value)
                {
                    //Clicked OK
                    //If the user added only a single key stroke combo then let's not treat this as a sequence
                    if (!keySequenceWindow.IsDirty)
                    {
                        //User made no changes
                        return;
                    }
                    var sequenceList = keySequenceWindow.GetSequence;
                    if (sequenceList.Count > 1)
                    {
                        var osKeyPress = new KeyPress("Key press sequence", sequenceList);
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress.Information = keySequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(keySequenceWindow.GetInformation))
                        {
                            textBox.Text = keySequenceWindow.GetInformation;
                        }
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        ((TagDataClassStreamDeck)textBox.Tag).ClearAll();
                        var osKeyPress = new KeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress.Information = keySequenceWindow.GetInformation;
                        textBox.Text = sequenceList[0].VirtualKeyCodesAsString;
                    }
                    SetIsDirty();
                }
                SetFormState();
                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
            }
        }

        private void ButtonDeleteKeySequenceButtonOn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TagDataClassStreamDeck)TextBoxKeyPressButtonOn.Tag).ClearAll();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDeleteKeySequenceButtonOff_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TagDataClassStreamDeck)TextBoxKeyPressButtonOff.Tag).ClearAll();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }








        private void AddEditDCSBIOS(TextBox textBox)
        {
            try
            {
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }

                var dcsBIOSControlsConfigsWindow = ((TagDataClassStreamDeck)textBox.Tag).ContainsDCSBIOS() ? 
                    new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), ((TagDataClassStreamDeck)textBox.Tag).DCSBIOSBinding.DCSBIOSInputs, textBox.Text) : 
                    new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), null);

                dcsBIOSControlsConfigsWindow.ShowDialog();


                if (dcsBIOSControlsConfigsWindow.DialogResult.HasValue && dcsBIOSControlsConfigsWindow.DialogResult == true)
                {
                    var dcsBiosInputs = dcsBIOSControlsConfigsWindow.DCSBIOSInputs;
                    var text = string.IsNullOrWhiteSpace(dcsBIOSControlsConfigsWindow.Description) ? "DCS-BIOS" : dcsBIOSControlsConfigsWindow.Description;
                    //1 appropriate text to textbox
                    //2 update bindings
                    textBox.Text = text;
                    ((TagDataClassStreamDeck)textBox.Tag).Consume(dcsBiosInputs);
                    SetIsDirty();
                    SDUIParent.ChildChangesMade();
                }
                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
            }
        }

        private void ButtonAddEditDCSBIOSActionButtonOn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddEditDCSBIOS(TextBoxDCSBIOSActionButtonOn);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDeleteDCSBIOSActionButtonOn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TagDataClassStreamDeck)TextBoxDCSBIOSActionButtonOn.Tag).ClearAll();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonAddEditDCSBIOSActionButtonOff_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddEditDCSBIOS(TextBoxDCSBIOSActionButtonOff);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void ButtonDeleteDCSBIOSActionButtonOff_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TagDataClassStreamDeck)TextBoxDCSBIOSActionButtonOff.Tag).ClearAll();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}