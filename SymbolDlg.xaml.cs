﻿using CoreLib;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CadApp
{
    /// <summary>
    /// SymbolDlg.xaml の相互作用ロジック
    /// 
    /// シンボルデータ管理ダイヤログ
    /// 
    /// </summary>
    public partial class SymbolDlg : Window
    {
        private double mWindowWidth;            //  ウィンドウの高さ
        private double mWindowHeight;           //  ウィンドウ幅
        private double mPrevWindowWidth;        //  変更前のウィンドウ幅
        private WindowState mWindowState = WindowState.Normal;  //  ウィンドウの状態(最大化/最小化)

        public string mSymbolFolder;
        public string mCategory;
        public string mSymbolName;
        public Entity mEntity;

        public SymbolData mSymbolData;
        public bool mCancelEnable = true;
        private YWorldDraw ydraw;
        private Canvas mCanvas;
        private YLib ylib = new YLib();

        public SymbolDlg()
        {
            InitializeComponent();

            mSymbolData = new SymbolData(this);
            mCanvas = cvSymbolCanvas;
            ydraw = new YWorldDraw(mCanvas);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mSymbolData.mSymbolFolder = mSymbolFolder;
            cbCategory.ItemsSource = mSymbolData.getCategoryList();
            if (0 < cbCategory.Items.Count) {
                cbCategory.SelectedIndex = 0;
                string path = mSymbolData.getSymbolFilePath(cbCategory.Items[cbCategory.SelectedIndex].ToString() ?? "");
                lbSymbolName.ItemsSource = mSymbolData.getSymbolList(path);
            }
            btCancel.Visibility = mCancelEnable ? Visibility.Visible : Visibility.Collapsed;
            Thickness margin = btOK.Margin;
            btOK.Margin = new Thickness(margin.Left, margin.Top, mCancelEnable ? margin.Right : 20, margin.Bottom);

            if (0 < lbSymbolName.Items.Count)
                lbSymbolName.SelectedIndex = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Window_LayoutUpdated(object sender, System.EventArgs e)
        {
            if (WindowState != mWindowState && WindowState == WindowState.Maximized) {
                //  ウィンドウの最大化時
                mWindowWidth = SystemParameters.WorkArea.Width;
                mWindowHeight = SystemParameters.WorkArea.Height;
            } else if (WindowState != mWindowState ||
                mWindowWidth != Width || mWindowHeight != Height) {
                //  ウィンドウサイズが変わった時
                mWindowWidth = Width;
                mWindowHeight = Height;
            } else {
                //  ウィンドウサイズが変わらない時は何もしない
                mWindowState = WindowState;
                return;
            }
            mWindowState = WindowState;

            dispSymbol(mCategory, mSymbolName);
        }

        /// <summary>
        /// [分類]の変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (0 <= cbCategory.SelectedIndex) {
                string path = mSymbolData.getSymbolFilePath(cbCategory.Items[cbCategory.SelectedIndex].ToString() ?? "");
                lbSymbolName.ItemsSource = mSymbolData.getSymbolList(path);
            }
        }

        /// <summary>
        /// [シンボルの選択]
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbSymbolName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (0 <= lbSymbolName.SelectedIndex) {
                mCategory = cbCategory.Items[cbCategory.SelectedIndex].ToString() ?? "";
                mSymbolName = lbSymbolName.Items[lbSymbolName.SelectedIndex].ToString() ?? "";
                dispSymbol(mCategory, mSymbolName);
            }
        }

        /// <summary>
        /// [シンボルリスト]のコンテキストメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbSymbolNameMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            if (0 <= lbSymbolName.SelectedIndex) {
                mCategory = cbCategory.Items[cbCategory.SelectedIndex].ToString() ?? "";
                mSymbolName = lbSymbolName.Items[lbSymbolName.SelectedIndex].ToString() ?? "";
            }

            if (menuItem.Name.CompareTo("lbSymbolNameRenameMenu") == 0) {
                renameSymbol(mCategory, mSymbolName);
            } else if (menuItem.Name.CompareTo("lbSymbolNameRemoveMenu") == 0) {
                removeSymbol(mCategory, mSymbolName);
            } else if (menuItem.Name.CompareTo("lbSymbolNameCopyMenu") == 0) {
                copySymbol(mCategory, mSymbolName);
            } else if (menuItem.Name.CompareTo("lbSymbolNameMoveMenu") == 0) {
                copySymbol(mCategory, mSymbolName, true);
            }
        }

        /// <summary>
        /// シンボル名の変更
        /// </summary>
        /// <param name="category">分類</param>
        /// <param name="symbolName">シンボル名</param>
        private void renameSymbol(string category, string symbolName)
        {
            InputBox dlg = new InputBox();
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "名称変更";
            dlg.mEditText = symbolName;
            if (dlg.ShowDialog() == true) {
                string newName = dlg.mEditText;
                if (mSymbolData.renameSymbolData(category, symbolName, newName)) {
                    string path = mSymbolData.getSymbolFilePath(category);
                    lbSymbolName.ItemsSource = mSymbolData.getSymbolList(path);
                    int index = lbSymbolName.Items.IndexOf(newName);
                    lbSymbolName.SelectedIndex = index;
                }
            }
        }

        /// <summary>
        /// シンボルの削除
        /// </summary>
        /// <param name="category">分類</param>
        /// <param name="symbolName">シンボル名</param>
        private void removeSymbol(string category, string symbolName)
        {
            if (ylib.messageBox(this, symbolName + " 削除します", "", "確認", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                if (mSymbolData.removeSymbolData(category, symbolName)) {
                    string path = mSymbolData.getSymbolFilePath(category);
                    lbSymbolName.ItemsSource = mSymbolData.getSymbolList(path);
                    if (0 < lbSymbolName.Items.Count)
                        lbSymbolName.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// シンボルのコピー/移動
        /// </summary>
        /// <param name="category">分類</param>
        /// <param name="symbolName">シンボル名</param>
        /// <param name="move">移動</param>
        private void copySymbol(string category, string symbolName, bool move = false)
        {
            InputSelect dlg = new InputSelect();
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "分類選択";
            dlg.mTextList = mSymbolData.getCategoryList();
            if (dlg.ShowDialog() == true) {
                if (0 < dlg.mText.Length) {
                    mSymbolData.copySymbolData(category, symbolName, dlg.mText, move);
                    cbCategory.ItemsSource = mSymbolData.getCategoryList();
                }
            }
        }

        /// <summary>
        /// [OK]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btOK_Click(object sender, RoutedEventArgs e)
        {
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
        /// [回転]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btRotate_Click(object sender, RoutedEventArgs e)
        {
            PointD vec = new PointD(0, 1);
            PointD center = mEntity.mArea.getCenter();
            mEntity.rotate(center, center + vec);
            dispEntity(mEntity);
        }

        /// <summary>
        /// [反転]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btMirror_Click(object sender, RoutedEventArgs e)
        {
            PointD vec;
            Button btn = (Button)e.Source;
            if (btn.Name == "btMirrorTopDown")
                vec = new PointD(1, 0);
            else
                vec = new PointD(0, 1);
            PointD center = mEntity.mArea.getCenter();
            mEntity.mirror(center, center + vec);
            dispEntity(mEntity);
        }

        /// <summary>
        /// シンボルの表示
        /// </summary>
        /// <param name="category">分類</param>
        /// <param name="symbolName">シンボル名</param>
        private void dispSymbol(string category, string symbolName)
        {
            if (category == null || category.Length == 0
                || symbolName == null || symbolName.Length == 0)
                return;

            ydraw.setViewArea(0, 0, mCanvas.ActualWidth, mCanvas.ActualHeight);
            ydraw.mAspectFix = true;
            ydraw.mClipping = true;

            mEntity = mSymbolData.getSymbolData(category, symbolName).toCopy();
            dispEntity(mEntity);
        }

        /// <summary>
        /// シンボル要素の表示
        /// </summary>
        /// <param name="entity">要素データ</param>
        private void dispEntity(Entity entity)
        {
            entity.updateArea();
            entity.mArea.normalize();
            ydraw.setWorldWindow(entity.mArea);
            mCanvas.Children.Clear();
            entity.draw(ydraw);
        }
    }
}