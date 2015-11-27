using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MusicTagger
{
    public class Tag : System.ComponentModel.INotifyPropertyChanged, IComparable, ICloneable
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int CompareTo(object other)
        {
            return Name.CompareTo(((Tag)other).Name);
        }

        public Object Clone ()
        {
            return new Tag(ID, Name);
        }
        public Tag()
        {

        }
        public Tag(int id, string _name)
        {
            ID = id;
            Name = _name;
        }

        [XmlIgnore]
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }



        public int ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
                OnPropertyChanged("ID");
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private void OnPropertyChanged(string name)
        {
            System.ComponentModel.PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        private bool _isChecked;
        private int _ID;
        private string _name;
    }
}
