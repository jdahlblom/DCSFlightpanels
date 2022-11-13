﻿
namespace DCSFlightpanels.PanelUserControls.PreProgrammed
{

    using System.Windows;
    using System.Windows.Controls;


    using NonVisuals;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;

    using NonVisuals.CockpitMaster.Preprogrammed;
    using Interfaces;
    using System;

    /// <summary>
    /// Logique d'interaction pour Cdu737UserControlAH64D.xaml
    /// </summary>
    /// 
    public partial class Cdu737UserControlAH64D : UserControlBase,
        IGamingPanelListener,
        IGamingPanelUserControl
    {
        private readonly CDU737PanelAH64D _CDU737PanelAH64D;

        public Cdu737UserControlAH64D(HIDSkeleton hidSkeleton, TabItem parentTabItem)
        {
            InitializeComponent();
            ParentTabItem = parentTabItem;

            _CDU737PanelAH64D = new CDU737PanelAH64D(hidSkeleton);
            
            AppEventHandler.AttachGamingPanelListener(this);

            HideAllImages();

        }
        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _CDU737PanelAH64D.Dispose();
                    AppEventHandler.DetachGamingPanelListener(this);

                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        private bool _once = true;


        private void CDU737UserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            UserControlLoaded = true;


            if (_once)
            {
                _once = false;
            }
        }

        public override GamingPanel GetGamingPanel()
        {
            return _CDU737PanelAH64D;
        }


        public void ProfileEvent(object sender, ProfileEventArgs e)
        {

        }

        public void SettingsApplied(object sender, PanelInfoArgs e)
        {

        }

        public void SettingsModified(object sender, PanelInfoArgs e)
        {

        }

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            string[] lines = _CDU737PanelAH64D.CDULines;
            Dispatcher?.BeginInvoke(
            (Action)
            (() => {
                CDU737UserControl.displayLines(lines, 14);
            }
            ));

        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e)
        {
            string[] lines = _CDU737PanelAH64D.CDULines;
            Dispatcher?.BeginInvoke(
            (Action)
            (() => {
                CDU737UserControl.displayLines(lines,14);
            }
            ));

        }
        private static void HideAllImages()
        {
        }

        public string GetName()
        {
            return GetType().Name;
        }
    }
}

