using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using Desktop.Annotations;

namespace Desktop.Target
{
    public class TargetDatamodel: INotifyPropertyChanged
    {
        private IPAddress _address;
        private bool _statusSuccess;
        private TimeSpan _roundTripTime;
        private double _change;
        private DateTime? _showUntil;

        public TargetDatamodel(IPAddress address, bool statusSuccess, TimeSpan roundTripTime)
        {
            Address = address;
            StatusSuccess = statusSuccess;
            RoundTripTime = roundTripTime;
            Change = 0;
        }

        public double Change
        {
            get => _change;
            set
            {
                if (value.Equals(_change)) return;
                _change = value;
                OnPropertyChanged();
            }
        }

        public IPAddress Address
        {
            get => _address;
            set
            {
                if (value == _address) return;
                _address = value;
                var bytes = _address.GetAddressBytes();
                Octet3 = bytes[3];
                Octet2 = bytes[2];
                Octet1 = bytes[1];
                Octet0 = bytes[0];
                OnPropertyChanged();
            }
        }

        public int Octet0{get;private set;}
        public int Octet1{get;private set;}
        public int Octet2{get;private set;}
        public int Octet3{get;private set;}

        public bool StatusSuccess
        {
            get => _statusSuccess;
            set
            {
                if (value == _statusSuccess) return;
                _statusSuccess = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan RoundTripTime
        {
            get => _roundTripTime;
            set
            {
                if (value == _roundTripTime) return;
                _roundTripTime = value;
                OnPropertyChanged();
            }
        }

        public DateTime? ShowUntil
        {
            get => _showUntil;
            set
            {
                if (value.Equals(_showUntil)) return;
                _showUntil = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
