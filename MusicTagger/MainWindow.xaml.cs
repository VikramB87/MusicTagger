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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace MusicTagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Serializable]
    public partial class MainWindow : Window
    {
        private const int NUM_COLUMNS = 7;
        private ContextMenu addCtxMenu;
        AudioFileCollection audioFiles;
        private System.Collections.ObjectModel.ObservableCollection<Tag> tags;
        private int lastTagID;
        private bool updatingSelection;
        private System.Timers.Timer timer;
        private bool isDirty;
        private System.Threading.ManualResetEvent savingEvent = new System.Threading.ManualResetEvent(true);
        private Object lockDirtyFlag = new Object();

        private const string APPDATA_FOLDER = "MusicTagger";
        private const string TAGS_FILE = "tags";
        private const string AUDIO_FILE = "audio_files";

        public MainWindow()
        {
            InitializeComponent();
            InitializeContextMenu();
            audioFiles = new AudioFileCollection();
            tags = new SortedObservableCollection<MusicTagger.Tag>();

            LoadAudioFilesData();
            libraryGrid.DataContext = audioFiles;
            LoadTagsFromFile();
            lbTags.DataContext = tags;
            foreach (Tag t in tags)
            {
                t.PropertyChanged += TagSelectionChanged;
            }
            
            SetGridColumnWidth();
            timer = new System.Timers.Timer(2000);
            timer.Start();
            timer.Elapsed += TimerElapsed;

            txtSearch.KeyUp += OnTextSearchKeyPress;
        }

       
        private void InitializeContextMenu()
        {
            MenuItem[] items = new MenuItem[2];
            items[0] = new MenuItem();
            items[1] = new MenuItem();

            items[0].Header = "Add File";
            items[1].Header = "Add Folder";

            items[0].Click += OnAddFileMenuItemClicked;
            items[1].Click += OnAddFolderMenuItemClicked;

            addCtxMenu = new ContextMenu();
            addCtxMenu.Items.Add(items[0]);
            addCtxMenu.Items.Add(items[1]);

        }

        private void SetGridColumnWidth()
        {

            double totWidth = libraryGrid.Width;
            double[] PERC = new double[NUM_COLUMNS] { .20, .15, .20, .10, .05, .05, .30 };

            for (int i = 0; i < NUM_COLUMNS; ++i)
            {
                libraryGrid.Columns[i].Width = PERC[i] * totWidth;
            }

        }

        void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!MarkDirty(false)) return;
            savingEvent.Reset();
            SaveAudioFilesData();
            savingEvent.Set();
        }

        private bool MarkDirty (bool dirty)
        {
            lock (lockDirtyFlag)
            {
                bool ret = isDirty;
                isDirty = dirty;
                return ret;
            }
            
        }

        private void ScanFolder(string folder, bool recurse, string[] extensions)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(folder);


            foreach (var fi in di.EnumerateFiles())
            {
                String fileext = (fi.Extension != null && fi.Extension != String.Empty) ? fi.Extension.ToLower() : "";
                if (fileext.Length > 0 && fileext[0] == '.') fileext = fileext.Substring(1);

                if (extensions.Length == 0 || extensions.Contains(fileext))
                {

                    if (!audioFiles.ContainsFile(fi.FullName)) audioFiles.Add(new AudioFile(fi.FullName));

                }

            }

            if (!recurse) return;

            foreach (var fi in di.EnumerateDirectories())
            {
                ScanFolder(fi.FullName, recurse, extensions);
            }
        }

        private void ShowAddAudioContextMenu(object sender, RoutedEventArgs e)
        {
            addCtxMenu.IsOpen = true;
        }

        private void OnAddFileMenuItemClicked (object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "Select Audio Files";
            fileDialog.Filter = "Audio Files (*.MP3, *.FLAC, *.WMA, *.WAV)|*.MP3;*.FLAC;*.WMA;*.WAV|All Files (*.*)|*.*";
            fileDialog.ShowDialog();
        }

        private void OnAddFolderMenuItemClicked (object sender, RoutedEventArgs e)
        {
            AddFolderWindow addFolderWindow = new AddFolderWindow();
            addFolderWindow.ResizeMode = System.Windows.ResizeMode.NoResize;
            if (addFolderWindow.ShowDialog() == true)
            {
                char[] sep = new char[1] { ',' };
                String[] exts =  addFolderWindow.GetExtensions().Split(sep, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < exts.Length; ++i)
                {
                    exts[i] = exts[i].ToLower();
                }
                int count = audioFiles.Count;
                ScanFolder(addFolderWindow.GetSelectedFolder(), addFolderWindow.GetSearchSubDirectories(), exts);
                MessageBox.Show(audioFiles.Count - count + " files added.", "Info", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            
        }

        private void RowEdited(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                AudioFile f = (AudioFile)e.Row.DataContext;
                f.Save();
            }
            
        }

        private void MainWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {

           // libraryGrid.Width = this.Width - 50;
        }

        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {
            SetMetadataFromFilename(true);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SetMetadataFromFilename(false);
        }

        private void SetMetadataFromFilename (bool isAlbumFirstField)
        {
            foreach (AudioFile f in libraryGrid.SelectedItems)
            {
                String filename;
                int index;
                index = f.Location.LastIndexOf('\\');
                if (index != -1)
                {
                    filename = f.Location.Substring(index + 1);
                }
                else
                {
                    filename = f.Location;
                }

                String[] parts = filename.Split('-');
                if (parts.Length == 2)
                {
                    f.Title = parts[1].Trim();
                    index = f.Title.LastIndexOf('.');
                    if (index != -1)
                    {
                        f.Title = f.Title.Substring(0, index);
                    }
                    if (isAlbumFirstField) f.Album = parts[0].Trim();
                    else f.Artist = parts[0].Trim();
                }

                f.Save();
            }
           
        }
       
        void TagSelectionChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (updatingSelection) return;
            if (libraryGrid.SelectedIndex == -1) return;
 	        if (e.PropertyName == "IsChecked")
            {
                Tag t = (Tag) sender;
                AudioFile f = audioFiles[libraryGrid.SelectedIndex];
                if (f == null) return;
                if (t.IsChecked)
                {
                    if (!f.Tags.Contains(t.ID))
                    {
                        f.Tags.Add(t.ID);
                    }
                }
                else
                {
                    f.Tags.Remove(t.ID);
                }

            }
            MarkDirty(true);
        }

        private string GetDataFolder ()
        {
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return filename + "\\" + APPDATA_FOLDER;
        }

        private string GetTagsFileName ()
        {
            return GetDataFolder() + "\\" + TAGS_FILE;
        }

        private string GetAudioFilesFileName ()
        {
            return GetDataFolder() + "\\" + AUDIO_FILE;
        }

        private void SaveAudioFilesData ()
        {
            if (!System.IO.Directory.Exists(GetDataFolder()))
            {
                System.IO.Directory.CreateDirectory(GetDataFolder());
            }
            XmlSerializer serializer = new XmlSerializer(typeof(AudioFileCollection));
            using (System.IO.TextWriter writer = new System.IO.StreamWriter (GetAudioFilesFileName()))
            {
                serializer.Serialize(writer, audioFiles);
            }
        }

        private void LoadAudioFilesData ()
        {
            if (!System.IO.File.Exists(GetAudioFilesFileName())) return;
            XmlSerializer serializer = new XmlSerializer(typeof(AudioFileCollection));
            using (System.IO.TextReader reader = new System.IO.StreamReader(GetAudioFilesFileName()))
            {
                audioFiles = (AudioFileCollection) serializer.Deserialize(reader);
            }
        }

        private void SaveTagsToFile ()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SortedObservableCollection<Tag>));
            if (!System.IO.Directory.Exists(GetDataFolder()))
            {
                System.IO.Directory.CreateDirectory(GetDataFolder());
            }
            using (System.IO.TextWriter writer = new System.IO.StreamWriter(GetTagsFileName()))
            {
                serializer.Serialize(writer, tags);
            }
        }

        private void LoadTagsFromFile ()
        {
            if (!System.IO.File.Exists(GetTagsFileName())) return;
            XmlSerializer deserializer = new XmlSerializer(typeof(SortedObservableCollection<Tag>));

            using (System.IO.TextReader reader = new System.IO.StreamReader(GetTagsFileName()))
            {
                tags = (SortedObservableCollection<Tag>) deserializer.Deserialize(reader);
                lastTagID = tags.Max(obj => obj.ID);
            }
            
        }

        private void OnAddTag(object sender, RoutedEventArgs e)
        {
            if (txtTag.Text.Trim() != String.Empty)
            {
                Tag t = new Tag(++lastTagID, txtTag.Text.Trim());
                t.PropertyChanged += TagSelectionChanged;

                tags.Add(t);

                SaveTagsToFile();

            }

        }

        private void OnDelTagButtonClicked(object sender, RoutedEventArgs e)
        {
            Tag t = (Tag)lbTags.SelectedItem;
            if (t != null)
            {
                tags.Remove(t);
                SaveTagsToFile();
                foreach (AudioFile f in audioFiles)
                {
                    f.Tags.Remove(t.ID);
                }
            }
            MarkDirty(true);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AudioFile f = (AudioFile) e.AddedItems[0];
            updatingSelection = true;
            foreach (Tag t in tags)
            {
                t.IsChecked = false;
            }
            foreach (int id in f.Tags)
            {
                Tag t = tags.FirstOrDefault(obj => obj.ID == id);
                if (t != null) t.IsChecked = true;
            }
            updatingSelection = false;
        }

        private void OnQuit(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
            savingEvent.WaitOne();
            SaveAudioFilesData();
        }

        private void OnBtnGeneratePlaylistClick(object sender, RoutedEventArgs e)
        {
            GenPlaylistWindow win = new GenPlaylistWindow(audioFiles, tags);
            win.ShowDialog();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearch != null) SearchNext(txtSearch.Text);
        }

        void OnTextSearchKeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SearchNext(txtSearch.Text);
            }
        }

        private static bool CaseInsensitiveSearch (string haystack, string needle)
        {
            if (haystack != null && haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) != -1) return true;
            return false;
        }

        private static bool AudioFileContainsText (AudioFile f, string s)
        {
            if (CaseInsensitiveSearch(f.Title, s) || CaseInsensitiveSearch(f.Artist, s) ||
                CaseInsensitiveSearch(f.Album, s) || CaseInsensitiveSearch(f.Location, s)) return true;
            return false;
        }

        private void SearchNext (string text)
        {
            int start = libraryGrid.SelectedIndex;
            for (int i = start + 1; i != start; i = (i + 1) % libraryGrid.Items.Count)
            {
                AudioFile f = (AudioFile)libraryGrid.Items[i];
                if (AudioFileContainsText (f, text))
                {
                    libraryGrid.SelectedItem = libraryGrid.Items[i];
                    libraryGrid.ScrollIntoView(libraryGrid.SelectedItem);
                    break;
                }
            }
        }

        private void OnGridFocus(object sender, RoutedEventArgs e)
        {
      //      if (libraryGrid.SelectedItem != null) libraryGrid.ScrollIntoView(libraryGrid.SelectedItem);
        }

    }

    public class AudioFileCollection : System.Collections.ObjectModel.ObservableCollection<AudioFile>
    {
        public AudioFileCollection ()
        {
            files = new HashSet<string>();
        }
        protected override void InsertItem(int index, AudioFile item)
        {
            files.Add(item.Location);
            base.InsertItem(index, item);
        }
        protected override void RemoveItem(int index)
        {

            files.Remove(Items[index].Location);
            base.RemoveItem(index);
        }
        public bool ContainsFile (string location)
        {
            return files.Contains(location);
        }

        private HashSet<string> files;
    }
    
    public class SortedObservableCollection<T> : ObservableCollection<T>
    where T : IComparable
    {
        protected override void InsertItem(int index, T item)
        {
            for (var i = 0; i < Count; i++)
            {
                switch (Math.Sign(this[i].CompareTo(item)))
                {
                    case 0:
                    case 1:
                        base.InsertItem(i, item);
                        return;
                    case -1:
                        break;

                }
            }
            base.InsertItem(Count, item);
        }
    }  
}
