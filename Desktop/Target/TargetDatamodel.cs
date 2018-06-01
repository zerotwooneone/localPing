using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Desktop.Annotations;

namespace Desktop.Target
{
    public class TargetDatamodel: INotifyPropertyChanged
    {
        private string _address;
        private string _statusText;
        private TimeSpan _roundTripTime;

        public TargetDatamodel(string address, string statusText, TimeSpan roundTripTime)
        {
            Address = address;
            StatusText = statusText;
            RoundTripTime = roundTripTime;
        }

        public string Address
        {
            get => _address;
            set
            {
                if (value == _address) return;
                _address = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                if (value == _statusText) return;
                _statusText = value;
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
