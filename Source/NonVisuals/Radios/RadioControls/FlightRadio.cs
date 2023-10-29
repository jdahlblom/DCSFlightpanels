﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NonVisuals.Helpers;
using NonVisuals.Radios.RadioSettings;

namespace NonVisuals.Radios.RadioControls
{
    internal enum FlightRadioFrequencyBand
    {
        HF = 0,
        VHF1 = 1,
        VHF2 = 2,
        UHF = 3
    }

    internal class FlightRadio
    {
        private uint _integerFrequencyStandby;
        private uint _decimalFrequencyStandby;
        private string _cockpitFrequency;
        private readonly uint[] _savedCockpitIntegerFrequencyPerBand = new uint[4];
        private readonly uint[] _savedCockpitDecimalFrequencyPerBand = new uint[4];
        private readonly uint[] _savedIntegerFrequencyPerBand = new uint[4];
        private readonly uint[] _savedDecimalFrequencyPerBand = new uint[4];
        private readonly FlightRadioFrequencyBand _initialFrequencyBand;
        private FlightRadioFrequencyBand _currentFrequencyBand;
        private FlightRadioFrequencyBand _tempFrequencyBand;
        private readonly FlightRadioSettings _settings;

        internal FlightRadio(FlightRadioFrequencyBand initialFrequencyBand, FlightRadioSettings flightRadioSettings)
        {
            _initialFrequencyBand = initialFrequencyBand;
            _settings = flightRadioSettings;
        }

        internal void InitRadio()
        {
            _settings.VerifySettings();
            _currentFrequencyBand = _initialFrequencyBand;
            _tempFrequencyBand = _initialFrequencyBand;
            PopulateSavedValues();
            FetchStandbyFrequencyFromArray(_initialFrequencyBand);
            _cockpitFrequency = FetchCockpitFrequencyFromArray(_initialFrequencyBand);
        }

        internal void IntegerFrequencyUp(bool changeFaster = false)
        {
            if (_settings.IntegerFrequencySkippers[(int)_currentFrequencyBand].ShouldSkip()) return;

            if (GetIntegerFrequencyStandby() >= _settings.HighIntegerFrequencyBounds[(int)_currentFrequencyBand] ||
                (changeFaster && GetIntegerFrequencyStandby() + _settings.IntegerHighChangeRates[(int)_currentFrequencyBand] >= _settings.HighIntegerFrequencyBounds[(int)_currentFrequencyBand]))
            {
                SetIntegerFrequencyStandby(_settings.LowIntegerFrequencyBounds[(int)_currentFrequencyBand]);
                return;
            }

            AddIntegerFrequencyStandby(changeFaster ? _settings.IntegerHighChangeRates[(int)_currentFrequencyBand] : _settings.IntegerChangeRates[(int)_currentFrequencyBand]);
        }

        internal void IntegerFrequencyDown(bool changeFaster = false)
        {
            if (_settings.IntegerFrequencySkippers[(int)_currentFrequencyBand].ShouldSkip()) return;

            if (GetIntegerFrequencyStandby() <= _settings.LowIntegerFrequencyBounds[(int)_currentFrequencyBand] ||
                (changeFaster && GetIntegerFrequencyStandby() - _settings.IntegerHighChangeRates[(int)_currentFrequencyBand] <= _settings.LowIntegerFrequencyBounds[(int)_currentFrequencyBand]))
            {
                SetIntegerFrequencyStandby(_settings.HighIntegerFrequencyBounds[(int)_currentFrequencyBand]);
                return;
            }

            SubtractIntegerFrequencyStandby(changeFaster ? _settings.IntegerHighChangeRates[(int)_currentFrequencyBand] : _settings.IntegerChangeRates[(int)_currentFrequencyBand]);
        }

        internal void DecimalFrequencyUp(bool changeFaster = false)
        {
            if (GetDecimalFrequencyStandby() >= _settings.HighDecimalFrequencyBounds[(int)_currentFrequencyBand] ||
                (changeFaster && GetDecimalFrequencyStandby() + _settings.DecimalHighChangeRates[(int)_currentFrequencyBand] >= _settings.HighDecimalFrequencyBounds[(int)_currentFrequencyBand]))
            {
                SetDecimalFrequencyStandby(_settings.LowDecimalFrequencyBounds[(int)_currentFrequencyBand]);
                return;
            }

            AddDecimalFrequencyStandby(changeFaster ? _settings.DecimalHighChangeRates[(int)_currentFrequencyBand] : _settings.DecimalChangeRates[(int)_currentFrequencyBand]);
        }

        internal void DecimalFrequencyDown(bool changeFaster = false)
        {
            if (GetDecimalFrequencyStandby() <= _settings.LowDecimalFrequencyBounds[(int)_currentFrequencyBand] ||
                (changeFaster && GetDecimalFrequencyStandby() - _settings.DecimalHighChangeRates[(int)_currentFrequencyBand] <= _settings.LowDecimalFrequencyBounds[(int)_currentFrequencyBand]))
            {
                SetDecimalFrequencyStandby(_settings.HighDecimalFrequencyBounds[(int)_currentFrequencyBand]);
                return;
            }

            SubtractDecimalFrequencyStandby(changeFaster ? _settings.DecimalHighChangeRates[(int)_currentFrequencyBand] : _settings.DecimalChangeRates[(int)_currentFrequencyBand]);
        }

        internal string GetStandbyFrequency()
        {
            return GetIntegerFrequencyStandby() + "." + GetDecimalFrequencyStandby().ToString().PadLeft(3, '0').Trim();
        }

        internal string GetCockpitFrequency()
        {
            return _cockpitFrequency.Trim();
        }

        internal string GetFrequencyBandId()
        {
            return ((int)_currentFrequencyBand).ToString();
        }

        internal string GetTemporaryFrequencyBandId()
        {
            return ((int)_tempFrequencyBand).ToString();
        }

        internal void SetCockpitFrequency(string frequency)
        {
            Debug.WriteLine(LastFrequencies());
            if (!IsFrequencyBandSupported(frequency)) return;
            if (frequency == GetCockpitFrequency()) return;

            var oldCockpitFrequency = _cockpitFrequency;
            Debug.WriteLine($"Old cockpit : {oldCockpitFrequency}");
            _cockpitFrequency = frequency;

            SaveCockpitFrequencyToArray();
            SetStandbyFrequency(oldCockpitFrequency);
            SaveStandByFrequencyToArray();


            var newBand = GetFrequencyBand(_cockpitFrequency);
            var oldBand = GetFrequencyBand(GetStandbyFrequency());

            if (newBand != oldBand || newBand != _currentFrequencyBand)
            {
                FetchStandbyFrequencyFromArray(newBand);
                _currentFrequencyBand = newBand;
                _tempFrequencyBand = newBand;
            }
            Debug.WriteLine(LastFrequencies());
        }

        public void SwitchFrequencyBand()
        {
            if (_tempFrequencyBand == _currentFrequencyBand) return;

            Debug.WriteLine(LastFrequencies());
            SaveCockpitFrequencyToArray();
            SaveStandByFrequencyToArray();
            Debug.WriteLine(LastFrequencies());
            FetchStandbyFrequencyFromArray(_tempFrequencyBand);
            _cockpitFrequency = FetchCockpitFrequencyFromArray(_tempFrequencyBand);
            _currentFrequencyBand = _tempFrequencyBand;
            VerifyStandbyFrequencyBand();
            Debug.WriteLine(LastFrequencies());
        }

        private void SetStandbyFrequency(string frequency)
        {
            var array = frequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
            SetIntegerFrequencyStandby(uint.Parse(array[0]));
            SetDecimalFrequencyStandby(uint.Parse(array[1]));
        }

        internal string GetDCSBIOSCommand()
        {
            return $"{_settings.DCSBIOSIdentifier} {GetStandbyFrequency()}\n";
        }

        private FlightRadioFrequencyBand GetFrequencyBand(string frequency)
        {
            var array = frequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var integerFrequencyStandby = uint.Parse(array[0]);

            if (integerFrequencyStandby >= _settings.LowDecimalFrequencyBounds[(int)FlightRadioFrequencyBand.HF] && integerFrequencyStandby <= _settings.HighIntegerFrequencyBounds[(int)FlightRadioFrequencyBand.HF])
            {
                return FlightRadioFrequencyBand.HF;
            }
            if (integerFrequencyStandby >= _settings.LowDecimalFrequencyBounds[(int)FlightRadioFrequencyBand.VHF1] && integerFrequencyStandby <= _settings.HighIntegerFrequencyBounds[(int)FlightRadioFrequencyBand.VHF1])
            {
                return FlightRadioFrequencyBand.VHF1;
            }
            if (integerFrequencyStandby >= _settings.LowDecimalFrequencyBounds[(int)FlightRadioFrequencyBand.VHF2] && integerFrequencyStandby <= _settings.HighIntegerFrequencyBounds[(int)FlightRadioFrequencyBand.VHF2])
            {
                return FlightRadioFrequencyBand.VHF2;
            }
            if (integerFrequencyStandby >= _settings.LowDecimalFrequencyBounds[(int)FlightRadioFrequencyBand.UHF] && integerFrequencyStandby <= _settings.HighIntegerFrequencyBounds[(int)FlightRadioFrequencyBand.UHF])
            {
                return FlightRadioFrequencyBand.UHF;
            }

            throw new Exception("FlightRadio : Frequency not matching any frequency bands.");
        }

        internal void TemporaryFrequencyBandUp()
        {
            if (_settings.FrequencyBandSkipper.ShouldSkip())
            {
                return;
            }

            for (var i = 0; i < _settings.SupportedFrequencyBands.Length; i++)
            {
                //   E
                // C E H K     O    X
                // 0 1 2 3     4    5
                if (_tempFrequencyBand == _settings.SupportedFrequencyBands[i] && i < _settings.SupportedFrequencyBands.Length - 1)
                {
                    _tempFrequencyBand = _settings.SupportedFrequencyBands[i + 1];
                    break;
                }

                //       K
                // C E H K
                // 0 1 2 3
                if (_tempFrequencyBand == _settings.SupportedFrequencyBands[i] && i >= _settings.SupportedFrequencyBands.Length - 1)
                {
                    _tempFrequencyBand = _settings.SupportedFrequencyBands[0];
                    break;
                }
            }
        }

        internal void TemporaryFrequencyBandDown()
        {
            if (_settings.FrequencyBandSkipper.ShouldSkip())
            {
                return;
            }

            Debug.WriteLine($"Temp Band Before : {_tempFrequencyBand}");
            for (var i = 0; i < _settings.SupportedFrequencyBands.Length; i++)
            {
                //   E
                // C E H K     O    X
                // 0 1 2 3     4    5
                if (_tempFrequencyBand == _settings.SupportedFrequencyBands[i] && i > 0)
                {
                    _tempFrequencyBand = _settings.SupportedFrequencyBands[i - 1];
                    break;
                }

                // C
                // C E H K
                // 0 1 2 3
                if (_tempFrequencyBand == _settings.SupportedFrequencyBands[i] && i == 0)
                {
                    _tempFrequencyBand = _settings.SupportedFrequencyBands[^1];
                    break;
                }
            }
            Debug.WriteLine($"Temp Band After : {_tempFrequencyBand}");
        }

        private void FetchStandbyFrequencyFromArray(FlightRadioFrequencyBand frequencyBand)
        {
            SetIntegerFrequencyStandby(_savedIntegerFrequencyPerBand[(int)frequencyBand]);
            SetDecimalFrequencyStandby(_savedDecimalFrequencyPerBand[(int)frequencyBand]);
            VerifyStandbyFrequencyBand();
        }

        private void SaveStandByFrequencyToArray()
        {
            SaveStandByFrequencyToArray(GetStandbyFrequency());
            VerifyStandbyFrequencyBand();
        }

        private void SaveStandByFrequencyToArray(string frequency)
        {
            Debug.WriteLine($"Saving : {frequency}");
            var frequencyBand = GetFrequencyBand(frequency);

            var array = frequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var integerFrequency = uint.Parse(array[0]);
            var decimalFrequency = uint.Parse(array[1]);

            _savedIntegerFrequencyPerBand[(int)frequencyBand] = integerFrequency;
            _savedDecimalFrequencyPerBand[(int)frequencyBand] = decimalFrequency;
        }

        private void SaveCockpitFrequencyToArray()
        {
            SaveCockpitFrequencyToArray(_cockpitFrequency);
            VerifyStandbyFrequencyBand();
        }

        private void SaveCockpitFrequencyToArray(string frequency)
        {
            Debug.WriteLine($"Saving : {frequency}");
            var frequencyBand = GetFrequencyBand(frequency);

            var array = frequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var integerFrequency = uint.Parse(array[0]);
            var decimalFrequency = uint.Parse(array[1]);

            _savedCockpitIntegerFrequencyPerBand[(int)frequencyBand] = integerFrequency;
            _savedCockpitDecimalFrequencyPerBand[(int)frequencyBand] = decimalFrequency;
        }

        private string FetchCockpitFrequencyFromArray(FlightRadioFrequencyBand frequencyBand)
        {
            return $"{_savedCockpitIntegerFrequencyPerBand[(int)frequencyBand]}.{_savedCockpitDecimalFrequencyPerBand[(int)frequencyBand].ToString().PadLeft(3, '0')}";
        }

        private void VerifyStandbyFrequencyBand()
        {
            if (!IsFrequencyBandSupported(GetFrequencyBand(GetStandbyFrequency())))
            {
                throw new ArgumentOutOfRangeException($"FlightRadio:VerifyFrequencyBand => Frequency band {GetFrequencyBand(GetStandbyFrequency())} (Standby = {GetStandbyFrequency()}) is not supported.");
            }
        }

        private uint GetIntegerFrequencyCockpit()
        {
            return uint.Parse(_cockpitFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries)[0]);
        }

        private uint GetDecimalFrequencyCockpit()
        {
            return uint.Parse(_cockpitFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries)[1]);
        }

        private void SetIntegerFrequencyStandby(uint value)
        {
            _integerFrequencyStandby = value;
        }

        private void AddIntegerFrequencyStandby(uint value)
        {
            _integerFrequencyStandby += value;
        }

        private void SubtractIntegerFrequencyStandby(uint value)
        {
            _integerFrequencyStandby -= value;
        }

        private uint GetIntegerFrequencyStandby()
        {
            return _integerFrequencyStandby;
        }

        private void SetDecimalFrequencyStandby(uint value)
        {
            _decimalFrequencyStandby = value;
        }

        private void AddDecimalFrequencyStandby(uint value)
        {
            _decimalFrequencyStandby += value;
        }

        private void SubtractDecimalFrequencyStandby(uint value)
        {
            _decimalFrequencyStandby -= value;
        }

        private uint GetDecimalFrequencyStandby()
        {
            return _decimalFrequencyStandby;
        }

        private bool IsFrequencyBandSupported(FlightRadioFrequencyBand frequencyBand)
        {
            return _settings.SupportedFrequencyBands.Any(supportedBand => frequencyBand == supportedBand);
        }

        private bool IsFrequencyBandSupported(string frequency)
        {
            return _settings.SupportedFrequencyBands.Any(supportedBand => GetFrequencyBand(frequency) == supportedBand);
        }

        internal FlightRadioFrequencyBand[] SupportedFrequencyBands()
        {
            return _settings.SupportedFrequencyBands;
        }

        public FlightRadioFrequencyBand ActiveFrequencyBand => _currentFrequencyBand;


        internal string GetLastStandbyFrequency(FlightRadioFrequencyBand frequencyBand)
        {
            return _savedIntegerFrequencyPerBand[(int)frequencyBand] + "." + _savedDecimalFrequencyPerBand[(int)frequencyBand].ToString().PadLeft(3, '0');
        }

        private void PopulateSavedValues()
        {
            for (var i = 0; i < _settings.LowIntegerFrequencyBounds.Length; i++)
            {
                _savedIntegerFrequencyPerBand[i] = _settings.LowIntegerFrequencyBounds[i];
                _savedCockpitIntegerFrequencyPerBand[i] = _settings.LowIntegerFrequencyBounds[i];
            }
            for (var i = 0; i < _settings.LowDecimalFrequencyBounds.Length; i++)
            {
                _savedDecimalFrequencyPerBand[i] = _settings.LowDecimalFrequencyBounds[i];
                _savedCockpitDecimalFrequencyPerBand[i] = _settings.LowDecimalFrequencyBounds[i];
            }
        }

        internal string LastFrequencies()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("\nStandby :");
            stringBuilder.AppendLine("HF   : " + GetLastStandbyFrequency(FlightRadioFrequencyBand.HF));
            stringBuilder.AppendLine("VHF1 : " + GetLastStandbyFrequency(FlightRadioFrequencyBand.VHF1));
            stringBuilder.AppendLine("VHF2 : " + GetLastStandbyFrequency(FlightRadioFrequencyBand.VHF2));
            stringBuilder.AppendLine("UHF  : " + GetLastStandbyFrequency(FlightRadioFrequencyBand.UHF));
            stringBuilder.AppendLine("\nCockpit :");
            stringBuilder.AppendLine("HF   : " + FetchCockpitFrequencyFromArray(FlightRadioFrequencyBand.HF));
            stringBuilder.AppendLine("VHF1 : " + FetchCockpitFrequencyFromArray(FlightRadioFrequencyBand.VHF1));
            stringBuilder.AppendLine("VHF2 : " + FetchCockpitFrequencyFromArray(FlightRadioFrequencyBand.VHF2));
            stringBuilder.AppendLine("UHF  : " + FetchCockpitFrequencyFromArray(FlightRadioFrequencyBand.UHF));
            return stringBuilder.ToString();
        }
    }
}