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


namespace MusicTagger
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AddFolderWindow : Window
    {
        public AddFolderWindow()
        {
            InitializeComponent();
            txtFolderName.Text = System.IO.Directory.GetCurrentDirectory();
            
            txtFolderName.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            txtFolderName.TextWrapping = TextWrapping.NoWrap;

            txtExtensions.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            txtExtensions.TextWrapping = TextWrapping.NoWrap;

            this.PreviewKeyUp += CloseOnEsc;
        }

        void CloseOnEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void OnBtnSelectFolderClicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.ShowNewFolderButton = false;
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtFolderName.Text = folderBrowser.SelectedPath;
            }
        }

        private void OnBtnScanClicked(object sender, RoutedEventArgs e)
        {
            String folder = txtFolderName.Text;
            folder.Trim();
            if (folder == String.Empty)
            {
                MessageBox.Show("Select a folder to add.", "No folder provided", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (!System.IO.Directory.Exists(folder))
            {
                MessageBox.Show("Folder '" + folder + "' doesn't exist!", "Invalid folder", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            
            DialogResult = true;
            this.Close();
        }

        public String GetSelectedFolder ()
        {
            return txtFolderName.Text;
        }

        public bool GetSearchSubDirectories ()
        {
            return chkRecurse.IsChecked == true;
        }

        public string GetExtensions ()
        {
            return txtExtensions.Text;
        }

    }
}
