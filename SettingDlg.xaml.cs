using CoreLib;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CadApp
{
    /// <summary>
    /// SettingDlg.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingDlg : Window
    {
        public Box mWorldWindow = new Box(0,0,100,100);
        public double mPointSize = 1;
        public int mPointType = 1;
        public double mThickness = 1;
        public int mLineType = 0;
        public double mTextSize = 12;
        public double mArrowSize = 6;
        public double mArrowAngle = 30;
        public double mGridSize = 1;
        public string mDataFolder = "";
        public string mBackupFolder = "";
        public string mDiffTool = "";

        private List<string> mPointTypeMenu = new List<string>() {
            "・点", "X クロス", "+ 十字", "□ 四角", "〇 円", "△ 三角"
        };
        private List<string> mLineTypeMenu = new List<string>() {
            "実線", "破線", "一点鎖線", "二点鎖線"
        };
        
        private YLib ylib = new YLib();

        public SettingDlg()
        {
            InitializeComponent();

            cbPointType.ItemsSource = mPointTypeMenu;
            cbLineType.ItemsSource = mLineTypeMenu;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbAreaLeft.Text = mWorldWindow.Left.ToString();
            tbAreaBottum.Text = mWorldWindow.Bottom.ToString();
            tbAreaRight.Text = mWorldWindow.Right.ToString();
            tbAreaTop.Text = mWorldWindow.Top.ToString();
            tbPointSize.Text = mPointSize.ToString();
            cbPointType.SelectedIndex = mPointType;
            tbThickness.Text = mThickness.ToString();
            cbLineType.SelectedIndex = mLineType;
            tbTextSize.Text = mTextSize.ToString();
            tbArrowSize.Text = mArrowSize.ToString();
            tbArrowAngle.Text = ylib.R2D(mArrowAngle).ToString();
            tbGridSize.Text = mGridSize.ToString();
            tbDataFolder.Text = mDataFolder;
            tbBackupFolder.Text = mBackupFolder;
            tbDiffTool.Text = mDiffTool;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mWorldWindow.Left = ylib.string2double(tbAreaLeft.Text);
            mWorldWindow.Bottom = ylib.string2double(tbAreaBottum.Text);
            mWorldWindow.Right = ylib.string2double(tbAreaRight.Text);
            mWorldWindow.Top = ylib.string2double(tbAreaTop.Text);
            mPointSize = ylib.string2double(tbPointSize.Text);
            mPointType = cbPointType.SelectedIndex;
            mThickness = ylib.string2double(tbThickness.Text);
            mLineType = cbLineType.SelectedIndex;
            mTextSize = ylib.string2double(tbTextSize.Text);
            mArrowSize = ylib.string2double(tbArrowSize.Text);
            mArrowAngle = ylib.D2R(ylib.string2double(tbArrowAngle.Text));
            mGridSize = ylib.string2double(tbGridSize.Text);
            mDataFolder = tbDataFolder.Text;
            mBackupFolder = tbBackupFolder.Text;
            mDiffTool = tbDiffTool.Text;

            DialogResult = true;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void tbDataFolder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string folder = ylib.folderSelect("データフォルダ", mDataFolder);
            if (folder != null && 0 < folder.Length) 
                tbDataFolder.Text = folder;
        }

        private void tbBackupFolder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string folder = ylib.folderSelect("バックアップフォルダ", mBackupFolder);
            if (folder != null && 0 < folder.Length)
                tbBackupFolder.Text = folder;
        }

        private void tbDiffTool_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            List<string[]> filters = new List<string[]>() {
                    new string[] { "実行ファイル", "*.exe" },
                    new string[] { "すべてのファイル", "*.*"}
                };
            string filePath = ylib.fileOpenSelectDlg("ツール選択", Path.GetDirectoryName(mDiffTool), filters);
            if (0 < filePath.Length)
                tbDiffTool.Text = filePath;
        }
    }
}
