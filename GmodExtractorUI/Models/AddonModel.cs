using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GmodExtractorUI.Models
{
    public class AddonModel : INotifyPropertyChanged
    {
        private int _Id;
        private string _AddonId;
        private string _Name;
        private string _Description;
        private Uri _Image;
        private bool _IsChecked;

        public int Id
        {
            get { return _Id; }
            set
            {
                if (_Id != value)
                {
                    _Id = value;
                    OnPropertyChanged("Id");
                }
            }
        }
        public string AddonId
        {
            get { return _AddonId; }
            set
            {
                if (_AddonId != value)
                {
                    _AddonId = value;
                    OnPropertyChanged("AddonId");
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
        public string Description
        {
            get { return _Description; }
            set
            {
                if (_Description != value)
                {
                    _Description = value;
                    OnPropertyChanged("Description");
                }
            }
        }
        public Uri Image
        {
            get { return _Image; }
            set
            {
                if (_Image != value)
                {
                    _Image = value;
                    OnPropertyChanged("Image");
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
