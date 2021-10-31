﻿namespace ClassLibraryCommon
{
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    [Flags]
    public enum EmulationMode
    {
        DCSBIOSInputEnabled = 1,
        DCSBIOSOutputEnabled = 2,
        KeyboardEmulationOnly = 4,
        SRSEnabled = 8,
        NS430Enabled = 16
    }

    public static class Common
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();
        public static void PlaySoundFile(bool showException, string soundFile, double volume) //Volume 0 - 100
        {
            try
            {
                if (string.IsNullOrEmpty(soundFile) || !File.Exists(soundFile))
                {
                    return;
                }
                var mediaPlayer = new MediaPlayer();
                mediaPlayer.Open(new Uri(soundFile));
                mediaPlayer.Volume = volume / 100.0f;
                mediaPlayer.Play();
            }
            catch (Exception ex)
            {
                if (showException)
                {
                    ShowErrorMessageBox(ex);
                }
            }
        }

        public static bool IsStreamDeck(GamingPanelEnum panelType)
        {
            return panelType == GamingPanelEnum.StreamDeckMini || panelType == GamingPanelEnum.StreamDeck || panelType == GamingPanelEnum.StreamDeckXL || panelType == GamingPanelEnum.StreamDeckV2 || panelType == GamingPanelEnum.StreamDeckMK2;
        }

        public static Key RealKey(this KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.System:
                    return e.SystemKey;

                case Key.ImeProcessed:
                    return e.ImeProcessedKey;

                case Key.DeadCharProcessed:
                    return e.DeadCharProcessedKey;

                default:
                    return e.Key;
            }
        }

        public static string RemoveRControl(string keySequence)
        {
            if (keySequence.Contains(@"RMENU + LCONTROL"))
            {
                keySequence = keySequence.Replace(@"+ LCONTROL", string.Empty);
            }
            if (keySequence.Contains(@"LCONTROL + RMENU"))
            {
                keySequence = keySequence.Replace(@"LCONTROL +", string.Empty);
            }

            return keySequence;
        }

        public static readonly List<GamingPanelSkeleton> GamingPanelSkeletons = new List<GamingPanelSkeleton>
        {
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ55SwitchPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ69RadioPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ70MultiPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.BackLitPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.TPM),
            new GamingPanelSkeleton(GamingPanelVendorEnum.MadCatz, GamingPanelEnum.FarmingPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckMini),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeck),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckV2),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckMK2),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckXL),
        };

        private static NumberFormatInfo _pz69NumberFormatInfoFullDisplay;
        private static NumberFormatInfo _pz69NumberFormatInfoEmpty;

        private static int _emulationModesFlag = 0;

        private static void ValidateFlag()
        {
            if (IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly))
            {
                if (IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled) ||
                    IsEmulationModesFlagSet(EmulationMode.DCSBIOSInputEnabled))
                {
                    throw new Exception("Invalid emulation modes flag : " + _emulationModesFlag);
                }
            }
        }

        public static void SetEmulationModesFlag(int flag)
        {
            _emulationModesFlag = flag;
            ValidateFlag();
        }

        public static int GetEmulationModesFlag()
        {
            ValidateFlag();
            return _emulationModesFlag;
        }

        public static void SetEmulationModes(EmulationMode emulationMode)
        {
            _emulationModesFlag = _emulationModesFlag | (int)emulationMode;
            ValidateFlag();
        }

        public static bool IsEmulationModesFlagSet(EmulationMode flagValue)
        {
            return (_emulationModesFlag & (int)flagValue) > 0;
        }

        public static void ClearEmulationModesFlag(EmulationMode flagValue)
        {
            _emulationModesFlag &= ~((int)flagValue);
        }

        public static void ResetEmulationModesFlag()
        {
            _emulationModesFlag = 0;
        }

        public static bool NoDCSBIOSEnabled()
        {
            ValidateFlag();
            return !IsEmulationModesFlagSet(EmulationMode.DCSBIOSInputEnabled) && !IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled);
        }

        public static bool KeyEmulationOnly()
        {
            ValidateFlag();
            return IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly);
        }

        public static bool FullDCSBIOSEnabled()
        {
            ValidateFlag();
            return IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled) && IsEmulationModesFlagSet(EmulationMode.DCSBIOSInputEnabled);
        }

        public static bool PartialDCSBIOSEnabled()
        {
            ValidateFlag();
            return IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled) || IsEmulationModesFlagSet(EmulationMode.DCSBIOSInputEnabled);
        }

        public static NumberFormatInfo GetPZ69FullDisplayNumberFormat()
        {
            if (_pz69NumberFormatInfoFullDisplay == null)
            {
                _pz69NumberFormatInfoFullDisplay = new NumberFormatInfo
                {
                    NumberDecimalSeparator = ".",
                    NumberDecimalDigits = 4,
                    NumberGroupSeparator = string.Empty
                };
            }
            return _pz69NumberFormatInfoFullDisplay;
        }

        public static string GetMd5Hash(string input)
        {

            var md5 = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString().ToUpperInvariant();
        }

        public static string GetRandomMd5Hash()
        {
            var bytes = new byte[16];
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(bytes);
            }

            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
        }

        public static NumberFormatInfo GetPZ69EmptyDisplayNumberFormat()
        {
            if (_pz69NumberFormatInfoEmpty == null)
            {
                _pz69NumberFormatInfoEmpty = new NumberFormatInfo
                {
                    NumberDecimalSeparator = ".",
                    NumberGroupSeparator = string.Empty
                };
            }
            return _pz69NumberFormatInfoEmpty;
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return string.Empty;
        }

        public static string GetDescriptionField(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attributes.Length > 0)
            {
                return ((DescriptionAttribute)attributes[0]).Description;
            }
            return string.Empty;
        }

        public static APIModeEnum APIMode = 0;

        public static void ShowErrorMessageBox(Exception ex, string message = null)
        {
            logger.Error(ex, message);
            MessageBox.Show(ex.Message, $"Details logged to error log.{Environment.NewLine}{ex.Source}", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowMessageBox(string text, string header = "Information")
        {
            MessageBox.Show(text, header, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static string PrintBitStrings(byte[] array)
        {
            var result = string.Empty;
            for (int i = 0; i < array.Length; i++)
            {
                var str = Convert.ToString(array[i], 2).PadLeft(8, '0');
                result = result + "  " + str;
            }

            return result;
        }

        public static void WaitMilliSeconds(int millisecs)
        {
            var startMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var nowMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            while (nowMilliseconds - startMilliseconds < millisecs)
            {
                nowMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            }
        }

        public static long MilliSecsNow()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public static string GetApplicationPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetRelativePath(string relativeTo, string path)
        {
            var uri = new Uri(relativeTo);
            var rel = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(path)).ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (rel.Contains(Path.DirectorySeparatorChar.ToString()) == false)
            {
                rel = $".{ Path.DirectorySeparatorChar }{ rel }";
            }

            return rel;
        }


        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
                {
                    var child = VisualTreeHelper.GetChild(dependencyObject, i);
                    if (child is T o)
                    {
                        yield return o;
                    }

                    foreach (var childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
