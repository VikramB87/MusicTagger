using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MusicTagger
{
    public class AudioFile : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public AudioFile()
        {

        }

        public AudioFile(string location)
        {
            Location = location;
            Tags = new List<int>();
            try
            {
                TagLib.File file = TagLib.File.Create(Location);
                _title = file.Tag.Title;
                _artist = file.Tag.Performers.Length > 0 ? file.Tag.Performers[0] : "";
                _album = file.Tag.Album;
                _genre = file.Tag.Genres.Length > 0 ? file.Tag.Genres[0] : "";
                _year = file.Tag.Year;
                int index = location.LastIndexOf('.');
                if (index != -1)
                {
                    Extension = location.Substring(index + 1).ToUpper();
                }

            }
            catch (TagLib.UnsupportedFormatException)
            {

            }
            catch (TagLib.CorruptFileException)
            {

            }

        }

        public void Save()
        {
            try
            {
                TagLib.File file = TagLib.File.Create(Location);

                file.Tag.Title = Title;
                file.Tag.Performers = new string[1] { Artist };
                file.Tag.Album = Album;
                file.Tag.Genres = new string[1] { Genre };
                file.Tag.Year = Year;

                file.Save();
                file.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnPropertyChanaged(string name)
        {
            System.ComponentModel.PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanaged("Title");
            }
        }
        public string Artist
        {
            get { return _artist; }
            set
            {
                _artist = value;
                OnPropertyChanaged("Artist");
            }
        }
        public string Album
        {
            get { return _album; }
            set
            {
                _album = value;
                OnPropertyChanaged("Album");
            }
        }
        public string Genre
        {
            get { return _genre; }
            set
            {
                _genre = value;
                OnPropertyChanaged("Genre");
            }
        }
        public uint Year
        {
            get { return _year; }
            set
            {
                _year = value;
                OnPropertyChanaged("Year");
            }
        }


        public string Extension { get; set; }
        public string Location { get; set; }
        public List<int> Tags { get; set; }

        private string _title;
        private string _artist;
        private string _genre;
        private uint _year;
        private string _album;

    }
}
