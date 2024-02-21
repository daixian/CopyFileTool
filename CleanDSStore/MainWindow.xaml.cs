using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CleanDSStore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Configure open folder dialog box
            Microsoft.Win32.OpenFolderDialog dialog = new();

            dialog.Multiselect = false;
            dialog.Title = "选择要清理的文件夹";

            // Show open folder dialog box
            bool? result = dialog.ShowDialog();

            // Process open folder dialog box results
            if (result == true) {
                // Get the selected folder
                string fullPathToFolder = dialog.FolderName;
                string folderNameOnly = dialog.SafeFolderName;

                int num = Clean(fullPathToFolder);
                MessageBox.Show($"一共清理了{num}个文件!");
            }
        }

        private int Clean(string dirPath)
        {
            int deleteCount = 0;
            DirectoryInfo di = new DirectoryInfo(dirPath);

            FileInfo[] fis = di.GetFiles("._*", SearchOption.AllDirectories);
            foreach (FileInfo fi in fis) {
                string fileName = fi.Name.Substring(2);
                string filePath = System.IO.Path.Combine(fi.DirectoryName, fileName);
                if (System.IO.File.Exists(filePath)) {
                    // 删除这个临时文件
                    fi.Delete();
                    deleteCount++;
                }
            }

            fis = di.GetFiles("*.DS_Store", SearchOption.AllDirectories);
            foreach (FileInfo fi in fis) {
                fi.Delete();
                deleteCount++;
            }
            return deleteCount;
        }
    }
}