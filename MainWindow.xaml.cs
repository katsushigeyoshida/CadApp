//#define MYTEST

using CoreLib;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CadApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// CadApp メイン
    /// </summary>
    public partial class MainWindow : Window
    {
        private double mWindowWidth;            //  ウィンドウの高さ
        private double mWindowHeight;           //  ウィンドウ幅
        private double mPrevWindowWidth;        //  変更前のウィンドウ幅
        private WindowState mWindowState = WindowState.Normal;  //  ウィンドウの状態(最大化/最小化)

        private string mHelpFile = "CadAppManual.pdf";       //  PDFのヘルプファイル
        private double[] mGridSizeMenu = {
            0, 0.1, 0.2, 0.5, 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000
        };
        private double[] mTextSizeMenu = {
            1, 2, 4, 6, 8, 9, 10, 11, 12, 13, 14, 16, 18, 20, 24, 28, 32,
            36, 40, 48, 56, 64, 96, 128, 196, 256, 384, 512, 768, 1024, 1536,
            2048, 3072, 4096, 6144, 8192, 12288, 16384
        };
        private double[] mEntSizeMenu = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10
        };
        private List<string> mPointTypeMenu = new List<string>() {
            "・点", "X クロス", "+ 十字", "□ 四角", "〇 円"
        };
        private List<string> mLineTypeMenu = new List<string>() {
            "実線", "破線", "一点鎖線", "二点鎖線"
        };
        private List<string> mLocMenu = new List<string>() {
            "座標入力", "相対座標入力"
        };
        private List<string> mLocSelectMenu = new List<string>() {
            "端点・中間点",　"3分割点", "4分割点", "8分割点", "16分割点", "垂点"
        };
        private List<string> mSystemSetMenu = new List<string>() {
            "データフォルダ"
        };
        private Point mPrevPosition;                        //  マウスの前回位置(画面スクロール)
        private bool mMouseLeftButtonDown = false;          //  マウス左ボタン状態
        private BitmapSource mBitmapSource;                 //  画像データ(描画データのバッファリング)
        private int mPickBoxSize = 10;                      //  ピック領域サイズ
        private int mScrollSize = 19;                       //  キーによるスクロール単位
        private List<string> mKeyCommandList = new();       //  キー入力コマンドの履歴

        private EntityData mEntityData;                     //  要素データ

        public enum OPEMODE { non, pick, loc }
        private CommandOpe mCommandOpe;
        private CommandData mCommandData = new CommandData();
        private OPERATION mOperation = OPERATION.non;
        private OPEMODE mLocMode = OPEMODE.pick;            //  マウスのLocモードとPickモード

        private FileData mFileData;
        private DataDrawing mDataDrawing;
        private YLib ylib = new YLib();

        public MainWindow()
        {
            InitializeComponent();

            mDataDrawing = new DataDrawing(cvCanvas, this);
            mEntityData = new EntityData();
            mCommandOpe = new CommandOpe(mEntityData, this);
            mFileData = new FileData(this);

            WindowFormLoad();
        }

        /// <summary>
        /// Windows表示直後の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //  コントロール初期設定
            lbCommand.ItemsSource     = mCommandData.getMainCommand();
            cbColor.DataContext       = ylib.mColorList;
            cbGridSize.ItemsSource    = mGridSizeMenu;
            cbPointType.ItemsSource   = mPointTypeMenu;
            cbPointSize.ItemsSource   = mEntSizeMenu;
            cbLineType.ItemsSource    = mLineTypeMenu;
            cbEntSize.ItemsSource     = mEntSizeMenu;
            cbTextSize.ItemsSource    = mTextSizeMenu;

            setSystemProperty();

            mFileData.setBaseDataFolder();

            cbGenre.ItemsSource = mFileData.getGenreList();
            int index = cbGenre.Items.IndexOf(mFileData.mGenreName);
            if (0 <= index) {
                cbGenre.SelectedIndex = index;
                index = lbCategoryList.Items.IndexOf(mFileData.mCategoryName);
                if (0 <= index) {
                    lbCategoryList.SelectedIndex = index;
                    index = lbItemList.Items.IndexOf(mFileData.mDataName);
                    if (0 <= index) {
                        lbItemList.SelectedIndex = index;
                    }
                }
            } else {
                if (0 < cbGenre.Items.Count) {
                    mFileData.mGenreName = cbGenre.Items[0].ToString();
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    if (0 < lbCategoryList.Items.Count) {
                        mFileData.mCategoryName = lbCategoryList.Items[0].ToString();
                        lbItemList.ItemsSource = mFileData.getItemFileList();
                    }
                }
            }

            mDataDrawing.initDraw(mCommandOpe.mDispArea);
            dispMode();
        }

        /// <summary>
        /// Windows サイズ変更処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            mDataDrawing.initDraw(mCommandOpe.mDispArea);
            disp(mEntityData);
        }

        /// <summary>
        /// Windows 終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mCommandOpe.saveFile(true);

            WindowFormSave();
        }

        /// <summary>
        /// Windowの状態を前回の状態にする
        /// </summary>
        private void WindowFormLoad()
        {
            //  前回のWindowの位置とサイズを復元する(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.MainWindowWidth < 100 ||
                Properties.Settings.Default.MainWindowHeight < 100 ||
                SystemParameters.WorkArea.Height < Properties.Settings.Default.MainWindowHeight) {
                Properties.Settings.Default.MainWindowWidth = mWindowWidth;
                Properties.Settings.Default.MainWindowHeight = mWindowHeight;
            } else {
                Top    = Properties.Settings.Default.MainWindowTop;
                Left   = Properties.Settings.Default.MainWindowLeft;
                Width  = Properties.Settings.Default.MainWindowWidth;
                Height = Properties.Settings.Default.MainWindowHeight;
            }
            //  図面データ保存フォルダ
            string baseDataFolder = Properties.Settings.Default.BaseDataFolder;
            //  図面分類
            mFileData.mBaseDataFolder = baseDataFolder == "" ? Path.GetFullPath("Zumen") : baseDataFolder;
            mFileData.mGenreName = Properties.Settings.Default.GenreName;
            mFileData.mCategoryName = Properties.Settings.Default.CategoryName;
            mFileData.mDataName = Properties.Settings.Default.DataName;

            //  初期作図表示エリア
            loadDispArea();

            //  製図機能の初期値
            loadDataProperty();
        }

        /// <summary>
        /// Window状態を保存する
        /// </summary>
        private void WindowFormSave()
        {
            //  図面分類
            Properties.Settings.Default.BaseDataFolder = mFileData.mBaseDataFolder;
            Properties.Settings.Default.GenreName = mFileData.mGenreName;
            Properties.Settings.Default.CategoryName = mFileData.mCategoryName;
            Properties.Settings.Default.DataName = mFileData.mDataName;
            //  Windowの位置とサイズを保存(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.MainWindowTop    = Top;
            Properties.Settings.Default.MainWindowLeft   = Left;
            Properties.Settings.Default.MainWindowWidth  = Width;
            Properties.Settings.Default.MainWindowHeight = Height;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            keyCommand(e.Key, e.KeyboardDevice.Modifiers == ModifierKeys.Control, e.KeyboardDevice.Modifiers == ModifierKeys.Shift);
        }

        /// <summary>
        /// コマンドリストボックス選択変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbCommand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = lbCommand.SelectedIndex;
            if (0 <= index) {
                commandMenu(lbCommand.Items[index].ToString());
            }
        }

        /// <summary>
        /// [マウス左ボタンダウン] Window_MouseLeftButtonDown 
        /// ロケイト点処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mMouseLeftButtonDown = true;
            if (!onControlKey()) {
                //  要素追加
                PointD wp = mDataDrawing.cnvScreen2World(new PointD(e.GetPosition(cvCanvas)));
                wp.round(Math.Abs(mCommandOpe.mGridSize));
                mCommandOpe.mLocPos.Add(wp);
                mCommandOpe.mTextString = tbTextString.Text;
                if (mOperation != OPERATION.createPolyline && mOperation != OPERATION.createPolygon) {
                    if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                        commandClear();
                }
            }
            dispMode();
        }

        /// <summary>
        /// [マウス右ボタンダウン] Window_MouseRightButtonDown 
        /// ピック処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            mPrevPosition = e.GetPosition(cvCanvas);
            PointD pickPos = mDataDrawing.cnvScreen2World(new PointD(e.GetPosition(cvCanvas)));
            List<int> picks = getPickNo(pickPos);
            if (mLocMode == OPEMODE.loc) {
                //  ロケイト処理
                if (0 < mCommandOpe.mLocPos.Count &&
                    (mOperation == OPERATION.createPolyline || mOperation == OPERATION.createPolygon)) {
                    //  ロケイト数不定の要素作成(ポリライン、ポリゴン)
                    if (!pickEndMenu(picks)) {
                        //  要素追加(Polyline<Polygon)
                        if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                            commandClear();
                        return;
                    }
                }
                if (0 < picks.Count) {
                    PointD wp = null;
                    if (onControlKey()) {
                        //  メニューで位置を選定
                        wp = locSelect(pickPos, picks);
                    } else {
                        if (picks.Count == 1) {
                            //  ピックされているときは位置を自動判断
                            wp = autoLoc(pickPos, picks[0]);
                        } else if (2 <= picks.Count) {
                            //  2要素の時は交点位置
                            wp = mEntityData.intersection(picks[0], picks[1], pickPos);
                        }
                    }
                    if (wp != null)
                        mCommandOpe.mLocPos.Add(wp);
                }
                if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                    commandClear();
            } else {
                //  ピック処理
                if (0 < picks.Count) {
                    int pickNo = pickSelect(picks);
                    if (0 <= pickNo) {
                        //  ピック要素の登録
                        mCommandOpe.mPickEnt.Add((pickNo, pickPos));
                        mDataDrawing.pickDisp(mEntityData, mCommandOpe.mPickEnt);
                    }
                }
            }
            dispMode();
        }

        /// <summary>
        /// [マウス左ボタンアップ] Window_MouseLeftButtonUp 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mMouseLeftButtonDown = false;
        }

        /// <summary>
        /// [マウス移動] Window_MouseMove 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(cvCanvas);
            if (point == mPrevPosition)
                return;
            PointD wp = mDataDrawing.cnvScreen2World(new PointD(point));
            wp.round(Math.Abs(mCommandOpe.mGridSize));
            tbPosition.Text = $"{wp.x.ToString("F2")},{wp.y.ToString("F2")}";   //  マウス座標表示
            if (mMouseLeftButtonDown && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                //  画面の移動(Ctrl + 左ボタン)
                if (ylib.distance(point, mPrevPosition) > 5) {
                    scroll(mPrevPosition, point);
                } else {
                    return;
                }
            } else if ((0 < mCommandOpe.mLocPos.Count && mOperation != OPERATION.non) ||
                (0 == mCommandOpe.mLocPos.Count && 
                (mOperation == OPERATION.createPoint || mOperation == OPERATION.createText
                || mOperation == OPERATION.createDimension
                || mOperation == OPERATION.createAngleDimension || mOperation == OPERATION.createDiameterDimension
                || mOperation == OPERATION.createRadiusDimension))) {
                //  ドラッギング表示
                mCommandOpe.mLocPos.Add(wp);
                mDataDrawing.setEntityProperty(mCommandOpe.mCreateColor, mCommandOpe.mPointType, mCommandOpe.mPointSize,
                    mCommandOpe.mLineType, mCommandOpe.mThickness, mCommandOpe.mTextSize, mCommandOpe.mArrowSize);
                mDataDrawing.dragging(mEntityData, mCommandOpe.mLocPos, mCommandOpe.mPickEnt, mOperation);
                mCommandOpe.mLocPos.RemoveAt(mCommandOpe.mLocPos.Count - 1);
            } else {
            }
            mPrevPosition = point;
        }

        /// <summary>
        /// [マウスホイール] 縮小拡大
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (0 != e.Delta) {
                double scaleStep = e.Delta > 0 ? 1.1 : 1 / 1.1;
                Point point = e.GetPosition(cvCanvas);
                PointD wp = mDataDrawing.cnvScreen2World(new PointD(point));
                mDataDrawing.setWorldZoom(wp, scaleStep, true);
                disp(mEntityData);
            }
        }

        /// <summary>
        /// [ジャンル]の選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGenre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbGenre.SelectedIndex;
            if (0 <= index) {
                mFileData.setGenreFolder(cbGenre.SelectedItem.ToString());
                lbCategoryList.ItemsSource = mFileData.getCategoryList();
            }
        }

        /// <summary>
        /// [ジャンル選択]のコンテキストメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGenreMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            if (menuItem.Name.CompareTo("cbGenreAddMenu") == 0) {
                //  大分類(Genre)の追加
                string genre = mFileData.addGenre();
                if (0 < genre.Length) {
                    cbGenre.ItemsSource = mFileData.getGenreList();
                    int index = cbGenre.Items.IndexOf(genre);
                    if (0 <= index)
                        cbGenre.SelectedIndex = index;
                }
            } else if (menuItem.Name.CompareTo("cbGenreRenameMenu") == 0) {
                //  大分類名の変更
                string genre = mFileData.renameGenre(cbGenre.SelectedItem.ToString());
                if (0 < genre.Length) {
                    cbGenre.ItemsSource = mFileData.getGenreList();
                    int index = cbGenre.Items.IndexOf(genre);
                    if (0 <= index)
                        cbGenre.SelectedIndex = index;
                }
            } else if (menuItem.Name.CompareTo("cbGenreRemoveMenu") == 0) {
                //  大分類名の削除
                if (mFileData.removeGenre(cbGenre.SelectedItem.ToString())) {
                    cbGenre.ItemsSource = mFileData.getGenreList();
                    if (0 < cbGenre.Items.Count)
                        cbGenre.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// [カテゴリ]の選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbCategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = lbCategoryList.SelectedIndex;
            if (0 <= index) {
                mFileData.setCategoryFolder(lbCategoryList.SelectedItem.ToString());
                lbItemList.ItemsSource = mFileData.getItemFileList();
            }
        }

        /// <summary>
        /// [カテゴリ選択]のコンテキストメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbCategoryMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            string category = null;
            if (0 <= lbCategoryList.SelectedIndex)
                category = lbCategoryList.SelectedItem.ToString();

            if (menuItem.Name.CompareTo("lbCategoryAddMenu") == 0) {
                //  分類(Category)の追加
                category = mFileData.addCategory();
                if (0 < category.Length) {
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    int index = lbCategoryList.Items.IndexOf(category);
                    if (0 <= index)
                        lbCategoryList.SelectedIndex = index;
                }
            } else if (menuItem.Name.CompareTo("lbCategoryRenameMenu") == 0) {
                //  分類名の変更
                category = mFileData.renameCategory(category);
                if (0 < category.Length) {
                    lbCategoryList.SelectedIndex = -1;
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    int index = lbCategoryList.Items.IndexOf(category);
                    if (0 <= index)
                        lbCategoryList.SelectedIndex = index;
                }
            } else if (menuItem.Name.CompareTo("lbCategoryRemoveMenu") == 0) {
                //  分類の削除
                if (mFileData.removeCategory(lbCategoryList.SelectedItem.ToString())) {
                    lbCategoryList.SelectedIndex = -1;
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    if (0 < lbCategoryList.Items.Count)
                        lbCategoryList.SelectedIndex = 0;
                }
            } else if (menuItem.Name.CompareTo("lbCategoryCopyMenu") == 0) {
                //  分類のコピー
                mFileData.copyCategory(category);
            } else if (menuItem.Name.CompareTo("lbCategoryMoveMenu") == 0) {
                //  分類の移動
                if (mFileData.copyCategory(category, true)) {
                    lbCategoryList.SelectedIndex = -1;
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    if (0 < lbCategoryList.Items.Count)
                        lbCategoryList.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// [図面]の選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = lbItemList.SelectedIndex;
            if (0 <= index) {
                mCommandOpe.saveFile(true);
                if (mCommandOpe.openFile(mFileData.getItemFilePath(lbItemList.Items[index].ToString()))) {
                    mFileData.mDataName = lbItemList.Items[index].ToString();
                    Title = lbItemList.Items[index].ToString();
                    setSystemProperty();
                    commandClear();
                    dispMode();
                    dispFit();
                }
            }
        }

        /// <summary>
        /// [図面選択]のコンテキストメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbItemMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            string itemName = null;
            if (0 <= lbItemList.SelectedIndex)
                itemName = lbItemList.SelectedItem.ToString();

            if (menuItem.Name.CompareTo("lbItemAddMenu") == 0) {
                //  図面(Item)の追加
                itemName = mFileData.addItem();
                if (0 < itemName.Length) {
                    mCommandOpe.newData(mFileData.getItemFilePath(itemName));
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    lbItemList.SelectedIndex = lbItemList.Items.IndexOf(itemName);
                }
            } else if (menuItem.Name.CompareTo("lbItemRenameMenu") == 0 && itemName != null) {
                //  図面名の変更
                itemName = mFileData.renameItem(itemName);
                if (0 < itemName.Length) {
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    lbItemList.SelectedIndex = lbItemList.Items.IndexOf(itemName);
                }
            } else if (menuItem.Name.CompareTo("lbItemRemoveMenu") == 0 && itemName != null) {
                //  図面の削除
                if (mFileData.removeItem(itemName)) {
                    mCommandOpe.mCurFilePath = "";
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    if (0 < lbItemList.Items.Count)
                        lbItemList.SelectedIndex = 0;
                }
            } else if (menuItem.Name.CompareTo("lbItemCopyMenu") == 0 && itemName != null) {
                //  図面のコピー
                mFileData.copyItem(itemName);
            } else if (menuItem.Name.CompareTo("lbItemMoveMenu") == 0 && itemName != null) {
                //  図面の移動
                if (mFileData.copyItem(itemName, true)) {
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    if (0 < lbItemList.Items.Count)
                        lbItemList.SelectedIndex = 0;
                }
            } else if (menuItem.Name.CompareTo("lbItemPropertyMenu") == 0 && itemName != null) {
                //  図面のプロパティ
                string buf = mFileData.getItemFileProperty(itemName);
                MessageBox.Show(buf, "ファイルプロパティ");
            }
        }

        /// <summary>
        /// 色設定コンボボックス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbColor.SelectedIndex;
            if (0 <= index)
                mCommandOpe.mCreateColor = ylib.mColorList[index].brush; ;
        }

        /// <summary>
        /// グリッドサイズ設定コンボボックス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGridSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbGridSize.SelectedIndex;
            if (0 <= index) {
                mCommandOpe.mGridSize = mGridSizeMenu[index];
                mDataDrawing.dispGrid(mCommandOpe.mGridSize);
                disp(mEntityData);
            }
        }

        /// <summary>
        /// 点種設定コンボボックス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbPointType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbPointType.SelectedIndex;
            if (0 <= index) {
                mCommandOpe.mPointType = index;
            }
        }

        /// <summary>
        /// 点サイズの設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbPointSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbPointSize.SelectedIndex;
            if (0 <= index)
                mCommandOpe.mPointSize = mEntSizeMenu[index];
        }

        /// <summary>
        /// 線種設定コンボボックス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbLineType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbLineType.SelectedIndex;
            if (0 <= index) {
                mCommandOpe.mLineType = index;
            }
        }

        /// <summary>
        /// 点の大きさ/線の太さの設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbEntSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbEntSize.SelectedIndex;
            if (0 <= index)
                mCommandOpe.mThickness = mEntSizeMenu[index];
        }

        /// <summary>
        /// 文字サイズの設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTextSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbTextSize.SelectedIndex;
            if (0 <= index) {
                mCommandOpe.mTextSize = mTextSizeMenu[index];
                mCommandOpe.mArrowSize = mCommandOpe.mTextSize * 6 / 12;
            }
        }

        /// <summary>
        /// ロケイトメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btMenu_Click(object sender, RoutedEventArgs e)
        {
                locMenu();
        }

        /// <summary>
        /// 再表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btZoomOriginal_Click(object sender, RoutedEventArgs e)
        {
            disp(mEntityData);
        }

        /// <summary>
        /// [ZoomIn] 拡大表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btZoomIn_Click(object sender, RoutedEventArgs e)
        {
            mDataDrawing.setWorldZoom(1.5);
            disp(mEntityData);
        }

        /// <summary>
        /// [ZoomOut] 縮小表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btZoomOut_Click(object sender, RoutedEventArgs e)
        {
            mDataDrawing.setWorldZoom(0.75);
            disp(mEntityData);
        }

        /// <summary>
        /// [ZoomFit] 表示領域に全体を表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btZoomFit_Click(object sender, RoutedEventArgs e)
        {
            dispFit();
        }

        /// <summary>
        /// [Open] データの読込
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btOpen_Click(object sender, RoutedEventArgs e)
        {
            mOperation = OPERATION.open;
            mLocMode = mCommandOpe.executeCmd(mOperation);
            dispMode();
        }

        /// <summary>
        /// [Save] データの保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            mOperation = OPERATION.save;
            mLocMode = mCommandOpe.executeCmd(mOperation);
            dispMode();
        }

        /// <summary>
        /// [画面コピー]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btScreenCopy_Click(object sender, RoutedEventArgs e)
        {
            screenCopy();
        }

        /// <summary>
        /// [Setting] 設定ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSetting_Click(object sender, RoutedEventArgs e)
        {
            //systemMenu();
            systemSettingdlg();
        }

        /// <summary>
        /// [ヘルプ]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btHelp_Click(object sender, RoutedEventArgs e)
        {
            ylib.openUrl(mHelpFile);
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="key">キーコード</param>
        /// <param name="control">コントロールキーの有無</param>
        private void keyCommand(Key key, bool control, bool shift)
        {
            if (control) {
                switch (key) {
                    case Key.F1: mCommandOpe.mGridSize *= -1; disp(mEntityData); break;        //  グリッド表示反転
                    case Key.Left: mDataDrawing.scroll(mEntityData, -mScrollSize, 0); break;
                    case Key.Right: mDataDrawing.scroll(mEntityData, mScrollSize, 0); break;
                    case Key.Up: mDataDrawing.scroll(mEntityData, 0, -mScrollSize); break;
                    case Key.Down: mDataDrawing.scroll(mEntityData, 0, mScrollSize); break;
                    case Key.S: mCommandOpe.saveFile(); break;
                    case Key.Z: mEntityData.undo(); disp(mEntityData); break;
                }
            } else if (shift) {
                switch (key) {
                    case Key.F1: break;
                }
            } else {
                switch (key) {
                    case Key.Escape: commandClear(); break;             //  ESCキーでキャンセル
                    case Key.Return:
                        if (mCommandOpe.keyCommand(cbCommand.Text)) {
                            keyCommandList(cbCommand.Text);
                            disp(mEntityData);
                        }
                        break;
                    case Key.F1: disp(mEntityData); break;              //  再表示
                    case Key.F3: dispFit(); break;                      //  全体表示
                    case Key.F4: mDataDrawing.setWorldZoom(1.5); disp(mEntityData); break;  //  拡大表示
                    case Key.F5: mDataDrawing.setWorldZoom(0.75); disp(mEntityData); break; //  縮小表示
                    case Key.F11: tbTextString.Focus(); break;          //  テキスト入力ボックスにフォーカス
                    case Key.F12: cbCommand.Focus(); break;             //  コマンド入力ボックスにフォーカス
                    case Key.Back:                                      //  ロケイト点を一つ戻す
                        if (0 < mCommandOpe.mLocPos.Count) {
                            mCommandOpe.mLocPos.RemoveAt(mCommandOpe.mLocPos.Count - 1);
                            disp(mEntityData);
                        }
                        break;
                    case Key.Apps: locMenu(); break;                    //  コンテキストメニューキー
                }
            }
        }

        /// <summary>
        /// コントロールキーの確認
        /// </summary>
        /// <returns></returns>
        private bool onControlKey()
        {
            return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        }


        /// <summary>
        /// ピックした要素Noを求める
        /// </summary>
        /// <param name="pickPos">ピック位置(Worls座標)</param>
        /// <returns>要素No</returns>
        private List<int> getPickNo(PointD pickPos)
        {
            double xd = mDataDrawing.screen2worldXlength(mPickBoxSize);    //  ピック領域
            Box b = new Box(pickPos, xd);
            return mEntityData.findIndex(b);
        }

        /// <summary>
        /// コマンドメニュー処理
        /// </summary>
        /// <param name="command">コマンド名</param>
        private void commandMenu(string command)
        {
            //  コマンドの選択
            if (mCommandData.mCommandLevel == 0) {
                //  メインコマンド
                mCommandData.mMain = command;
                List<string> subCmd = mCommandData.getSubCommand(mCommandData.mMain);
                if (1 < subCmd.Count) {
                    //  サブコマンドメニュー設定
                    lbCommand.ItemsSource = subCmd;
                    mCommandData.mCommandLevel = 1;
                } else {
                    //  メインコマンド処理
                    Command cmd = mCommandData.getCommand(mCommandData.mMain);
                    mOperation = cmd.operation;
                    mLocMode = mCommandOpe.executeCmd(cmd.operation);
                }
            } else if (mCommandData.mCommandLevel == 1) {
                //  サブコマンド処理
                mCommandData.mSub = command;
                Command cmd = mCommandData.getCommand(mCommandData.mMain, mCommandData.mSub);
                mOperation = cmd.operation;
                mLocMode = mCommandOpe.executeCmd(mOperation);
            }
            dispMode();
        }

        /// <summary>
        /// コマンドデータをクリアする
        /// </summary>
        public void commandClear()
        {
            //  メインコマンドに戻る
            mCommandOpe.mPickEnt.Clear();
            mCommandOpe.mLocPos.Clear();
            mLocMode = OPEMODE.pick;
            mCommandData.mCommandLevel = 0;
            mOperation = OPERATION.non;
            lbCommand.ItemsSource = mCommandData.getMainCommand();
            lbCommand.SelectedIndex = -1;
            disp(mEntityData);
        }

        /// <summary>
        /// ピックした要素上の点を取得(端点、中点,1/4点から一番近い点)
        /// </summary>
        /// <param name="p">ピック位置</param>
        /// <param name="entNo">要素No</param>
        /// <returns>座標(World座標)</returns>
        private PointD autoLoc(PointD p, int entNo = -1)
        {
            if (entNo < 0)
                return p;
            PointD pos = new PointD(p);
            Entity ent = mEntityData.mEntityList[entNo];
            switch (ent.mEntityId) {
                case EntityId.Point:
                    PointEntity point = (PointEntity)ent;
                    pos = point.mPoint;
                    break;
                case EntityId.Line:
                    LineEntity line = (LineEntity)ent;
                    pos = line.mLine.nearPoint(p, 4);
                    break;
                case EntityId.Polyline:
                    PolylineEntity polyline = (PolylineEntity)ent;
                    LineD pl = polyline.mPolyline.nearLine(p);
                    pos = pl.nearPoint(p, 4);
                    break;
                case EntityId.Polygon:
                    PolygonEntity polygon = (PolygonEntity)ent;
                    LineD pgl = polygon.mPolygon.nearLine(p);
                    pos = pgl.nearPoint(p, 4);
                    break;
                case EntityId.Arc:
                    ArcEntity arc = (ArcEntity)ent;
                    pos = arc.mArc.nearPoints(p, Math.PI * 2 <= arc.mArc.mOpenAngle ? 8 : 4);
                    break;
                case EntityId.Text:
                    TextEntity text = (TextEntity)ent;
                    pos = text.mText.nearPeakPoint(p);
                    break;
            }
            return pos;
        }

        /// <summary>
        /// ピックの継続か終了の選択メニュー
        /// </summary>
        /// <param name="picks">ピックリスト</param>
        /// <returns>継続/終了</returns>
        private bool pickEndMenu(List<int> picks)
        {
            if (0 < picks.Count) {
                List<string> menu = new List<string>() { "終了", "要素ピック" };
                MenuDialog dlg = new MenuDialog();
                dlg.mMainWindow = this;
                dlg.mHorizontalAliment = 1;
                dlg.mVerticalAliment = 1;
                dlg.mOneClick = true;
                dlg.mMenuList = menu;
                dlg.ShowDialog();
                if (dlg.mResultMenu == menu[1])
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 複数ピックした時の要素選択メニュー
        /// </summary>
        /// <param name="picks">ピックリスト</param>
        /// <returns>選択ピック要素番号</returns>
        private int pickSelect(List<int> picks)
        {
            if (picks.Count == 1)
                return picks[0];
            List<int> sqeezePicks = picks.Distinct().ToList();
            List<string> menu = new List<string>();
            for (int i = 0; i < sqeezePicks.Count; i++) {
                Entity ent = mEntityData.mEntityList[sqeezePicks[i]];
                menu.Add(ent.getSummary());
            }
            if (mLocMode == OPEMODE.loc)
                menu.Add("交点");
            MenuDialog dlg = new MenuDialog();
            dlg.mMainWindow = this;
            dlg.mHorizontalAliment = 1;
            dlg.mVerticalAliment = 1;
            dlg.mOneClick = true;
            dlg.mMenuList = menu;
            dlg.ShowDialog();
            if (dlg.mResultMenu == "")
                return -1;
            else if (dlg.mResultMenu == "交点")
                return -2;
            else
                return ylib.string2int(dlg.mResultMenu);
        }

        /// <summary>
        /// ピックした要素の分割点を求めるメニュー
        /// </summary>
        /// <param name="pos">ピック点</param>
        /// <param name="picks">ピック要素リスト</param>
        /// <returns>ロケイト点</returns>
        private PointD locSelect(PointD pos, List<int> picks)
        {
            if (picks.Count == 0) return pos;
            List<string> locMenu = new();
            locMenu.AddRange(mLocSelectMenu);
            Entity ent = mEntityData.mEntityList[picks[0]];
            if (picks.Count == 1) {
                if (ent.mEntityId == EntityId.Arc)
                    locMenu.Add("中心点");
            } else if (1 < picks.Count) {
                locMenu.Add("交点");
            }
            MenuDialog dlg = new MenuDialog();
            dlg.Title = "ロケイトメニュー";
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mMenuList = locMenu;
            dlg.ShowDialog();
            if (0 < dlg.mResultMenu.Length) {
                pos = getLocSelectPos(dlg.mResultMenu, pos, picks);
            }

            return pos;
        }

        /// <summary>
        /// メニュー選択されたロケイト点を求める
        /// </summary>
        /// <param name="selectMenu">選択メニュー</param>
        /// <param name="pos">ピック点</param>
        /// <param name="picks">ピック要素番号リスト</param>
        /// <returns>ロケイト点</returns>
        private PointD getLocSelectPos(string selectMenu, PointD pos, List<int> picks)
        {
            Entity ent = mEntityData.mEntityList[picks[0]];
            switch (selectMenu) {
                case "端点・中間点": pos = ent.dividePos(2, pos); break;
                case "3分割点": pos = ent.dividePos(3, pos); break;
                case "4分割点": pos = ent.dividePos(4, pos); break;
                case "8分割点": pos = ent.dividePos(8, pos); break;
                case "16分割点": pos = ent.dividePos(16, pos); break;
                case "垂点":
                    PointD lastLoc = mCommandOpe.mLocPos[mCommandOpe.mLocPos.Count - 1];
                    pos = ent.onPoint(lastLoc);
                    break;
                case "中心点":
                    if (ent.mEntityId == EntityId.Arc) {
                        ArcEntity arcEnt = (ArcEntity)ent;
                        pos = arcEnt.mArc.mCp;
                    }
                    break;
                case "交点":
                    if (2 <= picks.Count) {
                        List<PointD> plist = mEntityData.intersection(picks[0], picks[1]);
                        pos = plist.MinBy(p => p.length(pos));
                    }
                    break;
            }
            return pos;
        }

        /// <summary>
        /// ロケイトメニューの表示
        /// </summary>
        private void locMenu()
        {
            if (mLocMode == OPEMODE.loc) {
                List<string> locMenu = new List<string>();
                locMenu.AddRange(mLocMenu);
                if (mOperation == OPERATION.translate || mOperation == OPERATION.copyTranslate) {
                    locMenu.Add("平行距離");
                    locMenu.Add("スライド距離");
                } else if (mOperation == OPERATION.offset || mOperation == OPERATION.copyOffset) {
                    locMenu.Add("平行距離");
                } else if (mOperation == OPERATION.rotate || mOperation == OPERATION.copyRotate) {
                    locMenu.Add("回転角");
                }
                MenuDialog dlg = new MenuDialog();
                dlg.Title = "ロケイトメニュー";
                dlg.Owner = this;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlg.mMenuList = locMenu;
                dlg.ShowDialog();
                if (0 < dlg.mResultMenu.Length) {
                    getInputLoc(dlg.mResultMenu);
                }
            }
        }

        /// <summary>
        /// ロケイトメニューの処理
        /// </summary>
        /// <param name="title"></param>
        private void getInputLoc(string title)
        {
            InputBox dlg = new InputBox();
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = title;
            if (dlg.ShowDialog() == true) {
                string[] valstr;
                double val;
                PointD wp;
                LineD line;
                Entity entity;
                switch (title) {
                    case "座標入力":
                        //  xxx,yyy で入力
                        valstr = dlg.mEditText.Split(',');
                        if (1 < valstr.Length) {
                            wp = new PointD(ylib.string2double(valstr[0]), ylib.string2double(valstr[1]));
                            mCommandOpe.mLocPos.Add(wp);
                            if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                                commandClear();
                        }
                        break;
                    case "相対座標入力":
                        //  xxx,yyy で入力
                        valstr = dlg.mEditText.Split(',');
                        if (1 < valstr.Length) {
                            wp = new PointD(ylib.string2double(valstr[0]), ylib.string2double(valstr[1]));
                            if (0 < mCommandOpe.mLocPos.Count) {
                                wp += mCommandOpe.mLocPos[mCommandOpe.mLocPos.Count - 1];
                            }
                            mCommandOpe.mLocPos.Add(wp);
                            if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                                commandClear();
                        }
                        break;
                    case "平行距離":
                        //  移動またはコピー移動の時のみ
                        if (0 == mCommandOpe.mLocPos.Count)     //  方向を決めるロケイトが必要
                            break;
                        valstr = dlg.mEditText.Split(',');
                        val = ylib.string2double(valstr[0]);
                        entity = mEntityData.mEntityList[mCommandOpe.mPickEnt[mCommandOpe.mPickEnt.Count - 1].no];
                        if (entity.mEntityId == EntityId.Line) {
                            line = ((LineEntity)entity).mLine;
                        } else if (entity.mEntityId == EntityId.Polyline){
                            PolylineD polyline = ((PolylineEntity)entity).mPolyline;
                            line = polyline.getLine(mCommandOpe.mPickEnt[mCommandOpe.mPickEnt.Count - 1].pos);
                        } else if (entity.mEntityId == EntityId.Polygon) {
                            PolygonD polygon = ((PolygonEntity)entity).mPolygon;
                            line = polygon.getLine(mCommandOpe.mPickEnt[mCommandOpe.mPickEnt.Count - 1].pos);
                        } else {
                            break;
                        }
                        wp = line.offset(mCommandOpe.mLocPos[mCommandOpe.mLocPos.Count - 1], val);
                        mCommandOpe.mLocPos.Add(wp);
                        if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                            commandClear();
                        break;
                    case "スライド距離":
                        //  移動またはコピー移動の時のみ
                        valstr = dlg.mEditText.Split(',');
                        val = ylib.string2double(valstr[0]);
                        entity = mEntityData.mEntityList[mCommandOpe.mPickEnt[mCommandOpe.mPickEnt.Count - 1].Item1];
                        if (entity.mEntityId == EntityId.Line) {
                            line = ((LineEntity)entity).mLine;
                            wp = line.getVectorAngle(0, val);
                            if (0 == mCommandOpe.mLocPos.Count)
                                mCommandOpe.mLocPos.Add(new PointD());
                            wp += mCommandOpe.mLocPos[mCommandOpe.mLocPos.Count - 1];
                            mCommandOpe.mLocPos.Add(wp);
                            if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                                commandClear();
                        }
                        break;
                    case "回転角":
                        valstr = dlg.mEditText.Split(',');
                        val = ylib.string2double(valstr[0]);
                        PointD vec = new PointD(1, 0);
                        vec.rotate(ylib.D2R(val));
                        if (0 < mCommandOpe.mLocPos.Count)
                            wp = mCommandOpe.mLocPos[mCommandOpe.mLocPos.Count - 1] + vec;
                        else
                            wp = vec;
                        mCommandOpe.mLocPos.Add(wp);
                        if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                            commandClear();
                        break;
                }
            }
        }

        /// <summary>
        /// システム設定
        /// </summary>
        private void systemMenu()
        {
            MenuDialog dlg = new MenuDialog();
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "システム設定";
            dlg.mMenuList = mSystemSetMenu;
            dlg.ShowDialog();
            switch (dlg.mResultMenu) {
                case "データフォルダ":
                    mFileData.mBaseDataFolder = ylib.folderSelect("データフォルダ", mFileData.mBaseDataFolder);
                    mFileData.setBaseDataFolder(mFileData.mBaseDataFolder);
                    cbGenre.SelectedIndex = -1;
                    cbGenre.ItemsSource = mFileData.getGenreList();
                    lbCategoryList.Items.Clear();
                    lbItemList.Items.Clear();
                    if (0 < cbGenre.Items.Count)
                        cbGenre.SelectedIndex = 0;
                    break;
            }
        }

        /// <summary>
        /// データ表示
        /// </summary>
        private void disp(EntityData entyityData)
        {
            mDataDrawing.disp(entyityData, mCommandOpe.mBackColor, mCommandOpe.mGridSize);
        }

        /// <summary>
        /// キーコマンドをコンボボックスに追加
        /// </summary>
        /// <param name="command">コマンド</param>
        private void keyCommandList(string command)
        {
            int n = mKeyCommandList.IndexOf(command);
            if (0 <= n)
                mKeyCommandList.RemoveAt(n);
            mKeyCommandList.Insert(0, command);
            cbCommand.ItemsSource = mKeyCommandList;
        }

        /// <summary>
        /// 画面スクロール
        /// </summary>
        /// <param name="ps">始点</param>
        /// <param name="pe">終点</param>
        private void scroll(Point ps, Point pe)
        {
            double dx = pe.X - ps.X;
            double dy = pe.Y - ps.Y;
            //scroll(dx, dy);
            mDataDrawing.scroll(mEntityData, dx, dy);
        }

        /// <summary>
        /// 図面全体を表示する
        /// </summary>
        private void dispFit()
        {
            if (mEntityData.mArea != null) {
                mCommandOpe.setDispArea(mEntityData.mArea);
                mDataDrawing.initDraw(mCommandOpe.mDispArea);
                disp(mEntityData);
            }
        }

        /// <summary>
        /// ロケイト/ピックモードとロケイト数、ピック数の表示
        /// </summary>
        private void dispMode()
        {
            tbStatusInfo.Text = $"Mode:[{mLocMode}] Loc:[{mCommandOpe.mLocPos.Count}] Pick:[{mCommandOpe.mPickEnt.Count}]";
        }

        /// <summary>
        /// プロパティの初期値を設定
        /// </summary>
        private void systemSettingdlg()
        {
            SettingDlg dlg = new SettingDlg();
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mWorldWindow.Left = Properties.Settings.Default.WorldWindowLeft;
            //  表示エリア
            dlg.mWorldWindow.Bottom = Properties.Settings.Default.WorldWindowBottom;
            dlg.mWorldWindow.Right = Properties.Settings.Default.WorldWindowRight;
            dlg.mWorldWindow.Top = Properties.Settings.Default.WorldWindowTop;
            //  データプロパティ
            dlg.mPointSize = Properties.Settings.Default.PointSize;
            dlg.mPointType = Properties.Settings.Default.PointType;
            dlg.mThickness = Properties.Settings.Default.Thickness;
            dlg.mLineType = Properties.Settings.Default.LineType;
            dlg.mTextSize = Properties.Settings.Default.TextSize;
            dlg.mArrowSize = Properties.Settings.Default.ArrowSize;
            dlg.mArrowAngle = Properties.Settings.Default.ArrowAngle;
            //  システムプロパティ
            dlg.mGridSize = Properties.Settings.Default.GridSize;
            //  図面保存基準フォルダ
            dlg.mDataFolder = mFileData.mBaseDataFolder;
            if (dlg.ShowDialog() == true) {
                //  表示エリア
                Properties.Settings.Default.WorldWindowLeft = dlg.mWorldWindow.Left;
                Properties.Settings.Default.WorldWindowBottom = dlg.mWorldWindow.Bottom;
                Properties.Settings.Default.WorldWindowRight = dlg.mWorldWindow.Right;
                Properties.Settings.Default.WorldWindowTop = dlg.mWorldWindow.Top;
                loadDispArea();
                //  データプロパティ
                Properties.Settings.Default.PointSize = dlg.mPointSize;
                Properties.Settings.Default.PointType = dlg.mPointType;
                Properties.Settings.Default.Thickness = dlg.mThickness;
                Properties.Settings.Default.LineType = dlg.mLineType;
                Properties.Settings.Default.TextSize = dlg.mTextSize;
                Properties.Settings.Default.ArrowSize = dlg.mArrowSize;
                Properties.Settings.Default.ArrowAngle = dlg.mArrowAngle;
                //  システムプロパティ
                Properties.Settings.Default.GridSize = dlg.mGridSize;
                //  図面保存基準フォルダ
                if (mFileData.mBaseDataFolder != dlg.mDataFolder) {
                    mFileData.setBaseDataFolder(dlg.mDataFolder);
                    cbGenre.SelectedIndex = -1;
                    cbGenre.ItemsSource = mFileData.getGenreList();
                    if (0 < cbGenre.Items.Count) {
                        lbCategoryList.ItemsSource = mFileData.getCategoryList();
                        lbItemList.ItemsSource = mFileData.getItemFileList();
                        if (0 < lbCategoryList.Items.Count)
                            cbGenre.SelectedIndex = 0;
                        if (0 < lbItemList.Items.Count)
                            lbItemList.SelectedIndex = 0;
                    }
                }
            }
        }

        /// <summary>
        /// コントロールバーのシステム属性を設定する
        /// </summary>
        public void setSystemProperty()
        {
            cbColor.SelectedIndex = ylib.mColorList.FindIndex(p => p.brush == mCommandOpe.mCreateColor);
            cbGridSize.SelectedIndex = mGridSizeMenu.FindIndex(mCommandOpe.mGridSize);
            cbPointType.SelectedIndex = mCommandOpe.mPointType;
            cbPointSize.SelectedIndex = mEntSizeMenu.FindIndex(p => mCommandOpe.mPointSize <= p);
            cbLineType.SelectedIndex = mCommandOpe.mLineType;
            cbEntSize.SelectedIndex = mEntSizeMenu.FindIndex(p => mCommandOpe.mThickness <= p);
            cbTextSize.SelectedIndex = mTextSizeMenu.FindIndex((p) => mCommandOpe.mTextSize <= p);
        }

        /// <summary>
        /// 表示エリアの初期値を取得
        /// </summary>
        private void loadDispArea()
        {
            mCommandOpe.mInitArea.Left = Properties.Settings.Default.WorldWindowLeft;
            mCommandOpe.mInitArea.Bottom = Properties.Settings.Default.WorldWindowBottom;
            mCommandOpe.mInitArea.Right = Properties.Settings.Default.WorldWindowRight;
            mCommandOpe.mInitArea.Top = Properties.Settings.Default.WorldWindowTop;
        }

        /// <summary>
        /// 製図機能の初期値を取得
        /// </summary>
        private void loadDataProperty()
        {
            mCommandOpe.mPointSize = Properties.Settings.Default.PointSize;
            mCommandOpe.mPointType = Properties.Settings.Default.PointType;
            mCommandOpe.mThickness = Properties.Settings.Default.Thickness;
            mCommandOpe.mLineType = Properties.Settings.Default.LineType;
            mCommandOpe.mTextSize = Properties.Settings.Default.TextSize;
            mCommandOpe.mArrowSize = Properties.Settings.Default.ArrowSize;
            mCommandOpe.mArrowAngle = Properties.Settings.Default.ArrowAngle;
            mCommandOpe.mGridSize = Properties.Settings.Default.GridSize;
        }

        /// <summary>
        /// CAD画面をクリップボードにコピー
        /// </summary>
        public void screenCopy()
        {
            BitmapSource bitmapSource = canvas2Bitmap(cvCanvas);
            Clipboard.SetImage(bitmapSource);
        }

        /// <summary>
        /// CanvasをBitmapに変換
        /// 参照  https://qiita.com/tricogimmick/items/894914f6bbe224a45d49
        /// </summary>
        /// <param name="canvas"></param>
        /// <returns></returns>
        private BitmapSource canvas2Bitmap(Canvas canvas)
        {
            //  位置は CanvasのVisaulOffset値を設定したいが直接取れないので
            Point preLoc = new Point(lbCommand.ActualWidth + 10, 0);
            // レイアウトを再計算させる
            var size = new Size(canvas.ActualWidth, canvas.ActualHeight);
            canvas.Measure(size);
            canvas.Arrange(new Rect(new Point(0, 0), size));

            // VisualObjectをBitmapに変換する
            var renderBitmap = new RenderTargetBitmap((int)size.Width,       // 画像の幅
                                                      (int)size.Height,      // 画像の高さ
                                                      96.0d,                 // 横96.0DPI
                                                      96.0d,                 // 縦96.0DPI
                                                      PixelFormats.Pbgra32); // 32bit(RGBA各8bit)
            renderBitmap.Render(canvas);
            //  Canvasの位置を元に戻す(Canvas.VisualOffset値)
            canvas.Arrange(new Rect(preLoc, size));
            return renderBitmap;
        }
    }
}
