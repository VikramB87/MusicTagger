using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace MusicTagger
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class GenPlaylistWindow : Window
    {
        public GenPlaylistWindow(ObservableCollection<AudioFile> audioFiles, ObservableCollection<Tag> tags)
        {
            InitializeComponent();
            _audioFiles = audioFiles;
            _tags = new ObservableCollection<MusicTagger.Tag>();
            foreach (Tag t in tags)
            {
                _tags.Add((Tag) t.Clone());
            }
            _filteredAudioFiles = new ObservableCollection<AudioFile>();

            foreach (Tag t in _tags)
            {
                t.IsChecked = false;
                t.PropertyChanged += OnTagCheckedOrUnchecked;
            }
            lstTags.DataContext = _tags;
            gridAudioFiles.DataContext = _filteredAudioFiles;
        }

        void OnTagCheckedOrUnchecked(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            List<int> selected_tags = new List<int>();
            if (e.PropertyName != "IsChecked") return;
            
            foreach (Tag t in _tags.Where(obj => obj.IsChecked))
            {
                selected_tags.Add(t.ID);
            }

            _filteredAudioFiles.Clear();
            if (selected_tags.Count == 0) return;
            foreach (AudioFile f in _audioFiles)
            {
                if (selected_tags.All(id => f.Tags.Contains(id)))
                {
                    _filteredAudioFiles.Add(f);
                }
            }
        }

        private void OnBtnGenerateClicked(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDiag = new Microsoft.Win32.SaveFileDialog();
            saveDiag.Filter = "M3U Playlist (*.m3u) |*.m3u";
            if (saveDiag.ShowDialog() == true)
            {
                GeneratePlaylist(saveDiag.FileName);
            }
        }

        private void GeneratePlaylist (string filename)
        {
            System.IO.TextWriter writer = new System.IO.StreamWriter(filename);
            StringBuilder s = new StringBuilder();
            foreach (Tag t in _tags.Where(t => t.IsChecked))
            {
                if (s.Length == 0)
                {
                    s.Append("# ");
                    s.Append(t.Name);
                }
                else
                {
                    s.Append(", ");
                    s.Append(t.Name);
                }
            }
            writer.WriteLine(s.ToString());
            foreach (AudioFile f in _filteredAudioFiles)
            {
                writer.WriteLine(f.Location);
            }
            writer.Dispose();
        }

        private ObservableCollection<AudioFile> _audioFiles;
        private ObservableCollection<Tag> _tags;
        private ObservableCollection<AudioFile> _filteredAudioFiles;
    }
}
