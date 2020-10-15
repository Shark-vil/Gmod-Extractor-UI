using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GmodExtractorUI.Models
{
    public class ComboCheckBox : INotifyPropertyChanged
    {
        public string _Key;
        public string _Name;
        public bool _IsChecked;

        public string Key
        {
            get { return _Key; }
            set
            {
                if (_Key != value)
                {
                    _Key = value;
                    OnPropertyChanged("Key");
                }
            }
        }

        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public bool IsChecked
        {
            get { return _IsChecked; }
            set
            {
                if (_IsChecked != value)
                {
                    _IsChecked = value;
                    OnPropertyChanged("IsChecked");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
