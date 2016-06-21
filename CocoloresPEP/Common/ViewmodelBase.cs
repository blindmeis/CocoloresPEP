using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common
{
    public abstract class ViewmodelBase : INotifyPropertyChanged
    {
        private bool _isBusy;
        private string _focusToBindingPath;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string FocusToBindingPath
        {
            get { return _focusToBindingPath; }
            set
            {
                _focusToBindingPath = value; 
                OnPropertyChanged();
            }
        }
    }
}
