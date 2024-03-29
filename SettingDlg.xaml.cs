﻿using CoreLib;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CadApp
{
    /// <summary>
    /// SettingDlg.xaml の相互作用ロジック
    /// 
    /// システム設定ダイヤログ
    /// 
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
        public string mSymbolFolder = "";
        public string mImageFolder = "";
        public string mBackupFolder = "";
        public string mDiffTool = "";
        public string mShortCutFilePath = "";
        public FileData mFileData;
        public ImageData mImageData;
        public SymbolData mSymbolData;

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
            tbAreaLeft.Text     = mWorldWindow.Left.ToString();
            tbAreaBottum.Text   = mWorldWindow.Bottom.ToString();
            tbAreaRight.Text    = mWorldWindow.Right.ToString();
            tbAreaTop.Text      = mWorldWindow.Top.ToString();
            tbPointSize.Text    = mPointSize.ToString();
            cbPointType.SelectedIndex = mPointType;
            tbThickness.Text    = mThickness.ToString();
            cbLineType.SelectedIndex = mLineType;
            tbTextSize.Text     = mTextSize.ToString();
            tbArrowSize.Text    = mArrowSize.ToString();
            tbArrowAngle.Text   = ylib.double2StrZeroSup(ylib.R2D(mArrowAngle), "F8");
            tbGridSize.Text     = mGridSize.ToString();
            tbDataFolder.Text   = mDataFolder;
            tbSymbolFolder.Text = mSymbolFolder;
            tbImageFolder.Text  = mImageFolder;
            tbBackupFolder.Text = mBackupFolder;
            tbDiffTool.Text     = mDiffTool;
        }

        /// <summary>
        /// [OK]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            mWorldWindow.Left   = ylib.string2double(tbAreaLeft.Text);
            mWorldWindow.Bottom = ylib.string2double(tbAreaBottum.Text);
            mWorldWindow.Right  = ylib.string2double(tbAreaRight.Text);
            mWorldWindow.Top    = ylib.string2double(tbAreaTop.Text);
            mPointSize          = ylib.string2double(tbPointSize.Text);
            mPointType          = cbPointType.SelectedIndex;
            mThickness          = ylib.string2double(tbThickness.Text);
            mLineType           = cbLineType.SelectedIndex;
            mTextSize           = ylib.string2double(tbTextSize.Text);
            mArrowSize          = ylib.string2double(tbArrowSize.Text);
            mArrowAngle         = ylib.D2R(ylib.string2double(tbArrowAngle.Text));
            mGridSize           = ylib.string2double(tbGridSize.Text);
            mDataFolder         = tbDataFolder.Text;
            mSymbolFolder       = tbSymbolFolder.Text;
            mImageFolder        = tbImageFolder.Text;
            mBackupFolder       = tbBackupFolder.Text;
            mDiffTool           = tbDiffTool.Text;

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// [Cancel]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// [データフォルダ]選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbDataFolder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string folder = ylib.folderSelect("データフォルダ", mDataFolder);
            if (folder != null && 0 < folder.Length) 
                tbDataFolder.Text = folder;
        }

        /// <summary>
        /// [シンボルフォルダ]選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbSymbolFolder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string folder = ylib.folderSelect("シンボルフォルダ", mSymbolFolder);
            if (folder != null && 0 < folder.Length)
                tbSymbolFolder.Text = folder;
        }

        /// <summary>
        /// [イメージフォルダ]選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbImageFolder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string folder = ylib.folderSelect("イメージキャッシュフォルダ", mImageFolder);
            if (folder != null && 0 < folder.Length)
                tbImageFolder.Text = folder;
        }

        /// <summary>
        /// [バックアップフォルダ]選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbBackupFolder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string folder = ylib.folderSelect("バックアップフォルダ", mBackupFolder);
            if (folder != null && 0 < folder.Length)
                tbBackupFolder.Text = folder;
        }

        /// <summary>
        /// [テキスト比較ツール]選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// [ショートカットキー]ボタンの選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btShortCut_Click(object sender, RoutedEventArgs e)
        {
            shortCutKeyEdit();
        }

        /// <summary>
        /// ショートカットキーファイルの編集
        /// </summary>
        private void shortCutKeyEdit()
        {
            string buf = "";
            if (File.Exists(mShortCutFilePath)) {
                buf = ylib.loadTextFile(mShortCutFilePath);
            }
            InputBox dlg = new InputBox();
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mMultiLine = true;
            dlg.mWindowSizeOutSet = true;
            dlg.Title = "ショートカットキー編集";
            dlg.mEditText = buf;
            if (dlg.ShowDialog() == true) {
                buf = dlg.mEditText;
                ylib.saveTextFile(mShortCutFilePath, buf);
            }
        }

        /// <summary>
        /// [バックアップ]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btBackup_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;
            count += mFileData.dataBackUp(false);
            count += mSymbolData.dataBackUp(false);
            count += mImageData.dataBackUp(false);
            ylib.messageBox(this, $"{count} ファイルのバックアップを更新しました。");
        }

        /// <summary>
        /// [図面データ復元]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDataRestor_Click(object sender, RoutedEventArgs e)
        {
            mFileData.dataRestor();
        }

        /// <summary>
        /// [シンボルデータ復元]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSymbolRestor_Click(object sender, RoutedEventArgs e)
        {
            mSymbolData.dataRestor();
        }

        /// <summary>
        /// [イメージデータ復元]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btImageRestor_Click(object sender, RoutedEventArgs e)
        {
            mImageData.dataRestor();
        }

        /// <summary>
        /// [未使用イメージキャッシュ削除]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCashClear_Click(object sender, RoutedEventArgs e)
        {
            mFileData.squeezeImageCache(mImageData.mImageFolder);
        }
    }
}
