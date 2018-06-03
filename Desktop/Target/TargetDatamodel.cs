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
                OnPropertyChanged();
            }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
