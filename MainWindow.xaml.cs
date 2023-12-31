﻿
using CoreLib;
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
        private string mShortCutPath = "ShortCut.csv";
        private double[] mGridSizeMenu = {
            0, 0.1, 0.2, 0.25, 0.3, 0.4, 0.5, 1, 1.25, 1.5, 2, 2.5, 3, 4, 5, 10,
            20, 30, 40, 50, 100, 200, 300, 400, 500, 1000
        };
        private double[] mTextSizeMenu = {
            0.1, 0.2, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 5, 6, 7, 8, 9, 
            10, 11, 12, 13, 14, 16, 18, 20, 24, 28, 32, 36,
            40, 48, 56, 64, 96, 128, 196, 256, 384,
            512, 768, 1024, 1536, 2048, 3072, 4096, 6144, 8192, 12288, 16384
        };
        private double[] mEntSizeMenu = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10
        };
        private List<string> mPointTypeMenu = new List<string>() {
            "・点", "X クロス", "+ 十字", "□ 四角", "〇 円", "△ 三角"
        };
        private List<string> mLineTypeMenu = new List<string>() {
            "実線", "破線", "一点鎖線", "二点鎖線"
        };
        private string[] mHorizontalAlignmentMenu = { "左", "中", "右" };
        private string[] mVerticalAlignmentMenu = { "上", "中", "下" };
        private double[] mTextRotateMenu = { 
            0, 30, 45, 75, 90, 120, 135, 150, 180, 210, 235, 240, 270, 300, 315, 330
        };
        private List<string> mLocMenu = new List<string>() {
            "座標入力", "相対座標入力"
        };
        private List<string> mLocSelectMenu = new List<string>() {
            "端点・中間点",　"3分割点", "4分割点", "5分割点", "6分割点", "8分割点", "16分割点",
            "垂点", "端点距離"
        };
        private List<string> mSystemSetMenu = new List<string>() {
            "システム設定", "データバックアップ",
            //"図面データバックアップ", "シンボルバックアップ", "イメージファイルバックアップ",
            "図面データバックアップ管理", "シンボルバックアップ管理",
            "イメージファイルバックアップ管理", "未使用キャッシュイメージファイル削除"
        };
        private List<string> mPrintTypeMenu = new List<string>() {
            "A4 横", "A4 縦"
        };
        private Point mPrevPosition;                        //  マウスの前回位置(画面スクロール)
        private bool mMouseLeftButtonDown = false;          //  マウス左ボタン状態
        private int mPickBoxSize = 10;                      //  ピック領域サイズ
        private int mScrollSize = 19;                       //  キーによるスクロール単位
        private List<string> mKeyCommandList = new();       //  キー入力コマンドの履歴

        private double mDispLeftMargine = 0.01;             //  全体表示の左マージン
        private double mDispRightMargine = 0.03;            //  全体表示の右マージン

        private EntityData mEntityData;                     //  要素データ

        public enum OPEMODE { non, pick, loc, areaDisp, areaPick }
        private List<PointD> mAreaLoc = new List<PointD>() {
            new PointD(), new PointD()
        };
        private CommandOpe mCommandOpe;
        private CommandData mCommandData = new CommandData();
        public OPERATION mOperation = OPERATION.non;
        public OPEMODE mLocMode = OPEMODE.pick;            //  マウスのLocモードとPickモード
        public OPEMODE mPrevMode = OPEMODE.pick;

        private FileData mFileData;
        private DataDrawing mDataDrawing;
        public SymbolData mSymbolData;
        public ImageData mImageData;

        private YCalc ycalc = new YCalc();
        private YLib ylib = new YLib();

        public MainWindow()
        {
            InitializeComponent();

            mDataDrawing = new DataDrawing(cvCanvas, this);
            mEntityData  = new EntityData();
            mCommandOpe  = new CommandOpe(mEntityData, this);
            mFileData    = new FileData(this);
            mSymbolData  = new SymbolData(this);
            mImageData   = new ImageData(this);

            mCommandData.loadShortCut(mShortCutPath);
            mCommandOpe.mImageData = mImageData;
            mEntityData.mImageData = mImageData;

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
            cbTextHorizontal.ItemsSource = mHorizontalAlignmentMenu;
            cbTextVertical.ItemsSource   = mVerticalAlignmentMenu;
            cbTextRotate.ItemsSource     = mTextRotateMenu;

            setZumenProperty();

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
                    mFileData.mGenreName = cbGenre.Items[0].ToString() ?? "";
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    if (0 < lbCategoryList.Items.Count) {
                        mFileData.mCategoryName = lbCategoryList.Items[0].ToString() ?? "";
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

            Box dispArea = mDataDrawing.getWorldArea();
            dispArea = dispArea == null ? mCommandOpe.mDispArea : dispArea;
            mDataDrawing.initDraw(dispArea);
            disp(mEntityData);
        }

        /// <summary>
        /// Windows 終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (ylib.messageBox(this, "終了します", "", "確認", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            //    e.Cancel = true;
            if (mCommandOpe.mChkListDlg != null)
                mCommandOpe.mChkListDlg.Close();
            if (mCommandOpe.mSymbolDlg != null)
                mCommandOpe.mSymbolDlg.Close();
            mCommandOpe.saveFile(true);
            if (!File.Exists(mShortCutPath))
                mCommandData.saveShortCut(mShortCutPath);
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
                Properties.Settings.Default.MainWindowWidth  = mWindowWidth;
                Properties.Settings.Default.MainWindowHeight = mWindowHeight;
            } else {
                Top    = Properties.Settings.Default.MainWindowTop;
                Left   = Properties.Settings.Default.MainWindowLeft;
                Width  = Properties.Settings.Default.MainWindowWidth;
                Height = Properties.Settings.Default.MainWindowHeight;
            }
            //  図面データ保存フォルダ
            string baseDataFolder   = Properties.Settings.Default.BaseDataFolder;
            mFileData.mBackupFolder = Properties.Settings.Default.BackupFolder;
            mFileData.mDiffTool     = Properties.Settings.Default.DiffTool;
            mSymbolData.mDiffTool   = Properties.Settings.Default.DiffTool;
            //  図面分類
            mFileData.mBaseDataFolder = baseDataFolder == "" ? Path.GetFullPath("Zumen") : baseDataFolder;
            mFileData.mGenreName      = Properties.Settings.Default.GenreName;
            mFileData.mCategoryName   = Properties.Settings.Default.CategoryName;
            mFileData.mDataName       = Properties.Settings.Default.DataName;
            //  シンボルフォルダ
            string symbolFolder = Properties.Settings.Default.SymbolFolder;
            mSymbolData.mSymbolFolder = symbolFolder == "" ? Path.GetFullPath("Symbol") : symbolFolder;
            mSymbolData.mBackupFolder = Properties.Settings.Default.BackupFolder;
            //  イメージフォルダ
            string imageCacheFolder   = Properties.Settings.Default.ImageCacheFolder;
            mImageData.mImageFolder  = imageCacheFolder == "" ? Path.GetFullPath(mImageData.mImageFolder) : imageCacheFolder;
            mImageData.mBackupFolder = Properties.Settings.Default.BackupFolder;

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
            Properties.Settings.Default.DiffTool       = mFileData.mDiffTool;
            Properties.Settings.Default.BackupFolder   = mFileData.mBackupFolder;
            Properties.Settings.Default.BaseDataFolder = mFileData.mBaseDataFolder;
            Properties.Settings.Default.GenreName      = mFileData.mGenreName;
            Properties.Settings.Default.CategoryName   = mFileData.mCategoryName;
            Properties.Settings.Default.DataName       = mFileData.mDataName;
            //  シンボルフォルダ
            Properties.Settings.Default.SymbolFolder   = mSymbolData.mSymbolFolder;
            Properties.Settings.Default.ImageCacheFolder = mImageData.mImageFolder;
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
        /// [文字入力欄]のキー入力処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbTextString_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
                textInput();
            }
        }

        /// <summary>
        /// [作成レイヤー]コンボボックス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbCreateLayer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                setCreateLayer(cbCreateLayer.Text);
            }
        }

        /// <summary>
        /// 作成レイヤー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbCreateLayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (0 <= cbCreateLayer.SelectedIndex) {
                setCreateLayer(cbCreateLayer.Items[cbCreateLayer.SelectedIndex].ToString() ?? "");
            }
        }

        /// <summary>
        /// 単レイヤー表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chOneLayer_Click(object sender, RoutedEventArgs e)
        {
            if (chOneLayer.IsChecked == true) {
                mCommandOpe.setOneLayerDisp(true);
                //  表示レイヤーダイヤログ表示
                if (mCommandOpe.mChkListDlg != null && mCommandOpe.mChkListDlg.IsVisible)
                    mCommandOpe.setDispLayer();
                mEntityData.updateData();

            } else {
                mCommandOpe.setOneLayerDisp(false);
            }
            disp(mEntityData);
        }

        /// <summary>
        /// [Keyコマンド]コンボボックス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbCommand_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                if (mCommandOpe.keyCommand(cbCommand.Text, tbTextString.Text)) {
                    keyCommandList(cbCommand.Text);
                    disp(mEntityData);
                }
            }
        }

        /// <summary>
        /// コマンドリストボックス選択変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbCommand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (onKey(ylib.VK_ESCAPE) || onKey(ylib.VK_BACK))
                return;
            int index = lbCommand.SelectedIndex;
            if (lbCommand.Items !=null && 0 <= index) {
                commandMenu(lbCommand.Items[index].ToString() ?? "");
            }
        }

        /// <summary>
        /// [マウスのダブルクリック]処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mLocMode == OPEMODE.pick) {
                PointD wp = mDataDrawing.cnvScreen2World(new PointD(e.GetPosition(cvCanvas)));
                doubleClicKOpe(wp);
                dispMode();
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
            PointD wp = mDataDrawing.cnvScreen2World(new PointD(e.GetPosition(cvCanvas)));
            if (mLocMode == OPEMODE.areaDisp || mLocMode == OPEMODE.areaPick) {
                //  領域拡大、領域ピック
                if (areaOpe(wp, mLocMode))
                    mLocMode = mPrevMode;
            } else if (mLocMode == OPEMODE.loc && !onControlKey()) {
                //  要素追加(ロケイト数不定コマンドを除く)
                if (0 < mCommandOpe.mEntityData.mPara.mGridSize)
                    wp.round(Math.Abs(mCommandOpe.mEntityData.mPara.mGridSize));
                mCommandOpe.mLocPos.Add(wp);
                mCommandOpe.mTextString = tbTextString.Text;
                if (mOperation != OPERATION.createPolyline
                    && mOperation != OPERATION.createPolygon
                    && mOperation != OPERATION.createHVLine
                    && mOperation != OPERATION.copyTranslate
                    && mOperation != OPERATION.copyRotate) {
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
                //  ロケイトモード
                mCommandOpe.mTextString = tbTextString.Text;
                if (0 < mCommandOpe.mLocPos.Count &&
                    (mOperation == OPERATION.createPolyline
                    || mOperation == OPERATION.createPolygon
                    || mOperation == OPERATION.createHVLine
                    || mOperation == OPERATION.copyTranslate
                    || mOperation == OPERATION.copyRotate)) {
                    //  ロケイト数不定の要素作成(ポリライン、ポリゴン)
                    if (picks.Count == 0) {
                        //  要素追加(Polyline<Polygon)
                        if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                            commandClear();
                        return;
                    } else {
                        //  オートロケイト
                        PointD wp = autoLoc(pickPos, picks);
                        if (wp != null)
                            mCommandOpe.mLocPos.Add(wp);
                    }
                } else {
                    //  要素上でのロケイト処理
                    if (0 < picks.Count) {
                        //  ピックされているときは位置を自動判断(Ctrlキーのメニュー表示も含む)
                        PointD wp = autoLoc(pickPos, picks);
                        if (wp != null)
                            mCommandOpe.mLocPos.Add(wp);
                    }
                    if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                        commandClear();
                }
            } else {
                //  ピックモード
                if (0 < picks.Count) {
                    int pickNo = pickSelect(picks);
                    if (0 <= pickNo) {
                        //  ピック要素の登録
                        mCommandOpe.addPick((pickNo, pickPos.toCopy()), onControlKey());
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
            if ((mLocMode == OPEMODE.areaDisp || mLocMode == OPEMODE.areaPick) && !mAreaLoc[0].isNaN()) {
                mAreaLoc[1] = wp;
                mDataDrawing.areaDragging(mAreaLoc);
            } else {
                if (0 < mCommandOpe.mEntityData.mPara.mGridSize)
                    wp.round(Math.Abs(mCommandOpe.mEntityData.mPara.mGridSize));
                tbPosition.Text = $"{wp.x.ToString("F2")},{wp.y.ToString("F2")}";   //  マウス座標表示
                if (mMouseLeftButtonDown && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
                    || (Mouse.MiddleButton == MouseButtonState.Pressed)) {
                    //  画面の移動(Ctrl + 左ボタン, 中ボタン)
                    if (ylib.distance(point, mPrevPosition) > 10) {
                        scroll(mPrevPosition, point);
                    } else {
                        return;
                    }
                } else {
                    //  ドラッギング表示
                    mCommandOpe.mLocPos.Add(wp);
                    mDataDrawing.setEntityProperty(mCommandOpe);
                    mDataDrawing.dragging(mEntityData, mCommandOpe.mLocPos, mCommandOpe.mPickEnt, mOperation);
                    mCommandOpe.mLocPos.RemoveAt(mCommandOpe.mLocPos.Count - 1);
                }
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
                if (onControlKey()) {
                    double scaleStep = e.Delta > 0 ? 1 : -1;
                    scroll(0, mScrollSize * scaleStep);
                } else if (onAltKey()) {
                    double scaleStep = e.Delta > 0 ? 1 : -1;
                    scroll(mScrollSize * scaleStep, 0);
                } else {
                    double scaleStep = e.Delta > 0 ? 1.2 : 1 / 1.2;
                    Point point = e.GetPosition(cvCanvas);
                    PointD wp = mDataDrawing.cnvScreen2World(new PointD(point));
                    zoom(wp, scaleStep);
                }
            }
        }

        /// <summary>
        /// [文字列入力欄]ダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbTextString_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            textInput();
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
                mFileData.setGenreFolder(cbGenre.SelectedItem.ToString() ?? "");
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
                string genre = mFileData.renameGenre(cbGenre.SelectedItem.ToString() ?? "");
                if (0 < genre.Length) {
                    cbGenre.ItemsSource = mFileData.getGenreList();
                    int index = cbGenre.Items.IndexOf(genre);
                    if (0 <= index)
                        cbGenre.SelectedIndex = index;
                }
            } else if (menuItem.Name.CompareTo("cbGenreRemoveMenu") == 0) {
                //  大分類名の削除
                if (mFileData.removeGenre(cbGenre.SelectedItem.ToString() ?? "")) {
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
                mFileData.setCategoryFolder(lbCategoryList.SelectedItem.ToString() ?? "");
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
            string category = "";
            if (0 <= lbCategoryList.SelectedIndex)
                category = lbCategoryList.SelectedItem.ToString() ?? "";

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
                if (mFileData.removeCategory(lbCategoryList.SelectedItem.ToString() ?? "")) {
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
                if (mCommandOpe.mChkListDlg != null)
                    mCommandOpe.mChkListDlg.Close();
                if (mCommandOpe.mSymbolDlg != null)
                    mCommandOpe.mSymbolDlg.Close();
                if (mCommandOpe.openFile(mFileData.getItemFilePath(lbItemList.Items[index].ToString() ?? ""))) {
                    mFileData.mDataName = lbItemList.Items[index].ToString() ?? "";
                    Title = lbItemList.Items[index].ToString();
                    setZumenProperty();
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
                itemName = lbItemList.SelectedItem.ToString() ?? "";

            mCommandOpe.saveFile(true);
            if (menuItem.Name.CompareTo("lbItemAddMenu") == 0) {
                //  図面(Item)の追加
                itemName = mFileData.addItem();
                if (0 < itemName.Length) {
                    setSystemProperty();
                    mCommandOpe.newData(mFileData.getItemFilePath(itemName));
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    lbItemList.SelectedIndex = lbItemList.Items.IndexOf(itemName);
                }
            } else if (menuItem.Name.CompareTo("lbItemRenameMenu") == 0 && itemName != null) {
                //  図面名の変更
                itemName = mFileData.renameItem(itemName);
                if (0 < itemName.Length) {
                    mCommandOpe.mCurFilePath = "";
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
                    mCommandOpe.mCurFilePath = "";
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    if (0 < lbItemList.Items.Count)
                        lbItemList.SelectedIndex = 0;
                }
            } else if (menuItem.Name.CompareTo("lbItemImportMenu") == 0 && itemName != null) {
                //  インポート
                string item = importAsFile();
                mCommandOpe.mCurFilePath = "";
                lbItemList.ItemsSource = mFileData.getItemFileList();
                if (0 < lbItemList.Items.Count)
                    lbItemList.SelectedIndex = lbItemList.Items.IndexOf(item);
            } else if (menuItem.Name.CompareTo("lbItemPropertyMenu") == 0 && itemName != null) {
                //  図面のプロパティ
                string buf = mFileData.getItemFileProperty(itemName);
                ylib.messageBox(this, buf, "ファイルプロパティ");
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
                mCommandOpe.mEntityData.mPara.mColor = ylib.mColorList[index].brush; ;
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
                mCommandOpe.mEntityData.mPara.mGridSize = mGridSizeMenu[index];
                mDataDrawing.dispGrid(mCommandOpe.mEntityData.mPara.mGridSize);
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
                mCommandOpe.mEntityData.mPara.mPointType = index;
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
                mCommandOpe.mEntityData.mPara.mPointSize = mEntSizeMenu[index];
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
                mCommandOpe.mEntityData.mPara.mLineType = index;
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
                mCommandOpe.mEntityData.mPara.mThickness = mEntSizeMenu[index];
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
                mCommandOpe.mEntityData.mPara.mTextSize = mTextSizeMenu[index];
                mCommandOpe.mEntityData.mPara.mArrowSize = mCommandOpe.mEntityData.mPara.mTextSize * 6 / 12;
            }
        }

        /// <summary>
        /// 文字水平アライメントの設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTextHorizontal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbTextHorizontal.SelectedIndex;
            if (0 <= index) {
                mCommandOpe.mEntityData.mPara.mHa = index == 1 ? HorizontalAlignment.Center : index == 2 ?HorizontalAlignment.Right : HorizontalAlignment.Left;
            }
        }

        /// <summary>
        /// 文字垂直アライメントの設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTextVertical_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbTextVertical.SelectedIndex;
            if (0 <= index) {
                mCommandOpe.mEntityData.mPara.mVa = index == 1 ? VerticalAlignment.Center : index == 2 ? VerticalAlignment.Bottom : VerticalAlignment.Top;
            }
        }

        /// <summary>
        /// 文字列の回転角の設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTextRotate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbTextRotate.SelectedIndex;
            if (0 <= index) {
                mCommandOpe.mEntityData.mPara.mTextRotate = ylib.D2R(mTextRotateMenu[index]);
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
        /// 表示ズームボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btZoom_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (button.Name.CompareTo("btZoomArea") == 0) {
                mPrevMode = mLocMode;
                mLocMode = OPEMODE.areaDisp;
            } else if (button.Name.CompareTo("btZoomIn") == 0) {
                mDataDrawing.setWorldZoom(1.5);
                disp(mEntityData);
            } else if (button.Name.CompareTo("btZoomOut") == 0) {
                mDataDrawing.setWorldZoom(0.75);
                disp(mEntityData);
            } else if (button.Name.CompareTo("btZoomFit") == 0) {
                dispFit();
            } else if (button.Name.CompareTo("btZoomWidthFit") == 0) {
                dispWidthFit();
            }
        }

        /// <summary>
        /// [アンドゥ] ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btUndo_Click(object sender, RoutedEventArgs e)
        {
            mOperation = OPERATION.undo;
            mLocMode = mCommandOpe.executeCmd(mOperation);
            dispMode();
        }

        /// <summary>
        /// [要素データコピー] ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btEntityCopy_Click(object sender, RoutedEventArgs e)
        {
            mOperation = OPERATION.copyEntity;
            mLocMode = mCommandOpe.executeCmd(mOperation);
            dispMode();
        }

        /// <summary>
        /// [要素データ貼り付け] ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btEntityPaste_Click(object sender, RoutedEventArgs e)
        {
            mOperation = OPERATION.pasteEntity;
            mLocMode = mCommandOpe.executeCmd(mOperation);
            dispMode();
        }

        /// <summary>
        /// [領域ピック]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btAreaPick_Click(object sender, RoutedEventArgs e)
        {
            mPrevMode = mLocMode;
            mLocMode = OPEMODE.areaPick;
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
            systemMenu();
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
                    case Key.F1: mCommandOpe.mEntityData.mPara.mGridSize *= -1; disp(mEntityData); break;   //  グリッド表示反転
                    case Key.F2: break;
                    case Key.F3: break;
                    case Key.F4: break;
                    case Key.F5: break;
                    case Key.Left: scroll(mScrollSize, 0); break;
                    case Key.Right: scroll(-mScrollSize, 0); break;
                    case Key.Up: scroll(0, mScrollSize); break;
                    case Key.Down: scroll(0, -mScrollSize); break;
                    case Key.PageUp: zoom(1.1); break;
                    case Key.PageDown: zoom(1 / 1.1); break;
                    default:
                        //  ショートカットキー(Ctrl + 〇)
                        if (mCommandData.mShortCutList.ContainsKey(key))
                            commandExec(mCommandData.mShortCutList[key]);
                        break;
                }
            } else if (shift) {
                switch (key) {
                    case Key.F1: break;
                    default: break;
                }
            } else {
                switch (key) {
                    case Key.Escape: commandClear(); break;             //  ESCキーでキャンセル
                    case Key.Return: break;                             //  コントロールごとに実行
                    case Key.F1: disp(mEntityData); break;              //  再表示
                    case Key.F2: mPrevMode = mLocMode; mLocMode = OPEMODE.areaDisp; break;  //  領域拡大
                    case Key.F3: dispFit(); break;                      //  全体表示
                    case Key.F4: zoom(1.2); break;                      //  拡大表示
                    case Key.F5: zoom(1 / 1.2); break;                  //  縮小表示
                    case Key.F6: dispWidthFit(); break;                 //  全幅表示
                    case Key.F7: mPrevMode = mLocMode; mLocMode = OPEMODE.areaPick; break;  //  領域ピック
                    case Key.F8: locMenu(); break;                      //  ロケイトメニュー
                    case Key.F9: break;
                    case Key.F10: break;
                    case Key.F11: tbTextString.Focus(); break;          //  テキスト入力ボックスにフォーカス
                    case Key.F12: cbCommand.Focus(); break;             //  コマンド入力ボックスにフォーカス
                    case Key.Back:                                      //  ロケイト点を一つ戻す
                        if (0 < mCommandOpe.mLocPos.Count) {
                            mCommandOpe.mLocPos.RemoveAt(mCommandOpe.mLocPos.Count - 1);
                            mDataDrawing.dragging(mEntityData, mCommandOpe.mLocPos, mCommandOpe.mPickEnt, mOperation);
                        }
                        break;
                    case Key.Apps: locMenu(); break;                    //  コンテキストメニューキー
                    default: break;
                }
            }
            dispMode();
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
        /// Altキーの確認
        /// </summary>
        /// <returns></returns>
        private bool onAltKey()
        {
            return (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;
        }

        /// <summary>
        /// キーの確認
        /// </summary>
        /// <param name="keyCode">キーのコード</param>
        /// <returns></returns>
        private bool onKey(int keyCode)
        {
            return ylib.isGetKeyState(keyCode);
        }

        /// <summary>
        /// 領域を指定して拡大またはピック
        /// </summary>
        /// <param name="wp"></param>
        private bool areaOpe(PointD wp, OPEMODE opeMode)
        {
            if (mAreaLoc[0].isNaN()) {
                //  領域指定開始
                mAreaLoc[0] = wp;
            } else {
                //  領域決定
                if (1 < mAreaLoc.Count) {
                    Box dispArea = new Box(mAreaLoc[0], mAreaLoc[1]);
                    dispArea.normalize();
                    if (1 < mDataDrawing.world2creenXlength(dispArea.Width)) {
                        if (opeMode == OPEMODE.areaDisp) {
                            //  領域拡大表示
                            mDataDrawing.initDraw(dispArea);
                            disp(mEntityData);
                        } else if (opeMode == OPEMODE.areaPick) {
                            //  領域ピック
                            PointD pickPos = dispArea.getCenter();
                            List<int> picks = getPickNo(dispArea);
                            foreach (int pickNo in picks)
                                mCommandOpe.addPick((pickNo, pickPos), onControlKey());
                            mDataDrawing.pickDisp(mEntityData, mCommandOpe.mPickEnt);
                        }
                    }
                    mAreaLoc[0] = new PointD();
                    mAreaLoc[1] = new PointD();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// ピックした要素Noを求める
        /// </summary>
        /// <param name="pickPos">ピック位置(World座標)</param>
        /// <returns>要素Noリスト</returns>
        private List<int> getPickNo(PointD pickPos)
        {
            double xd = mDataDrawing.screen2worldXlength(mPickBoxSize);    //  ピック領域
            Box b = new Box(pickPos, xd);
            return getPickNo(b);
        }

        /// <summary>
        /// ピックした要素Noを求める
        /// </summary>
        /// <param name="b">ピック領域(World座標)</param>
        /// <returns>要素Noリスト</returns>
        private List<int> getPickNo(Box b)
        {
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
                commandExec(cmd.operation);
            }
            dispMode();
        }

        /// <summary>
        /// マウスのダブルクリックの処理
        /// ピックした要素の文字列変更または属性変更
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        private void doubleClicKOpe(PointD pickPos)
        {
            List<int> picks = getPickNo(pickPos);
            if (0 < picks.Count) {
                int pickNo = pickSelect(picks);
                if (0 <= pickNo) {
                    List<(int no, PointD pos)> pickEnt = new() {
                        (pickNo, pickPos.toCopy())
                    };
                    if (!mCommandOpe.changeText(pickEnt))
                        mCommandOpe.changeProperty(pickEnt);
                }
                commandClear();
            }
        }

        /// <summary>
        /// コマンドを実行する
        /// </summary>
        /// <param name="operation">OPRATIONコード</param>
        public void commandExec(OPERATION operation)
        {
            mOperation = operation;
            mLocMode = mCommandOpe.executeCmd(mOperation);
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
            chOneLayer.IsChecked = mCommandOpe.mEntityData.mPara.mOneLayerDisp;
            disp(mEntityData);
        }

        /// <summary>
        /// ピック要素に対しての自動ロケイト
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="picks">要素Noリスト</param>
        /// <returns>ロケイト位置</returns>
        private PointD autoLoc(PointD pickPos, List<int> picks)
        {
            PointD? wp = null;
            if (onControlKey()) {
                //  Ctrlキーでのメニュー表示で位置を選定
                wp = locSelect(pickPos, picks);
            } else {
                //  ピックされているときは位置を自動判断
                if (picks.Count == 1) {
                    //  ピックされているときは位置を自動判断
                    wp = autoLoc(pickPos, picks[0]);
                } else if (2 <= picks.Count) {
                    //  2要素の時は交点位置
                    wp = mEntityData.intersection(picks[0], picks[1], pickPos);
                    if (wp == null)
                        wp = autoLoc(pickPos, picks[0]);
                    if (wp == null)
                        wp = autoLoc(pickPos, picks[1]);
                }
            }
            return wp;
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
                case EntityId.Ellipse:
                    EllipseEntity ellipse = (EllipseEntity)ent;
                    pos = ellipse.mEllipse.nearPoints(p, Math.PI * 2 <= ellipse.mEllipse.mOpenAngle ? 8 : 4);
                    break;
                case EntityId.Text:
                    TextEntity text = (TextEntity)ent;
                    pos = text.mText.nearPeakPoint(p);
                    break;
                case EntityId.Parts:
                    PartsEntity parts = (PartsEntity)ent;
                    pos = parts.mParts.nearPoint(p, 4);
                    break;
                case EntityId.Image:
                    ImageEntity image = (ImageEntity)ent;
                    pos = image.mDispPosSize.nearPoint(p, 4);
                    break;
            }
            return pos;
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
                if (ent.mEntityId == EntityId.Arc || ent.mEntityId == EntityId.Ellipse) {
                    locMenu.Add("中心点");
                    locMenu.Add("頂点");
                    locMenu.Add("接点");
                } else if (ent.mEntityId == EntityId.Parts) {
                    locMenu.Add("中心点");
                }
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
            PointD lastLoc = pos;
            if (0 < mCommandOpe.mLocPos.Count)
                lastLoc = mCommandOpe.mLocPos[mCommandOpe.mLocPos.Count - 1];
            List<PointD> plist = new List<PointD>();
            switch (selectMenu) {
                case "端点・中間点": pos = ent.dividePos(2, pos); break;
                case "3分割点": pos = ent.dividePos(3, pos); break;
                case "4分割点": pos = ent.dividePos(4, pos); break;
                case "5分割点": pos = ent.dividePos(5, pos); break;
                case "6分割点": pos = ent.dividePos(6, pos); break;
                case "8分割点": pos = ent.dividePos(8, pos); break;
                case "16分割点": pos = ent.dividePos(16, pos); break;
                case "垂点":
                    pos = ent.onPoint(lastLoc);
                    break;
                case "接点":
                    if (ent.mEntityId == EntityId.Arc) {
                        ArcEntity arcEnt = (ArcEntity)ent;
                        plist = arcEnt.mArc.tangentPoint(lastLoc);
                    } else if (ent.mEntityId == EntityId.Ellipse) {
                        EllipseEntity ellipseEnt = (EllipseEntity)ent;
                        plist = ellipseEnt.mEllipse.tangentPoint(lastLoc);
                    }
                    if (plist != null && 0 < plist.Count)
                        pos = plist.MinBy(p => p.length(pos));  //  最短位置
                    break;
                case "頂点":
                    if (ent.mEntityId == EntityId.Arc) {
                        ArcEntity arcEnt = (ArcEntity)ent;
                        plist = arcEnt.mArc.toPeakList();
                    } else if (ent.mEntityId == EntityId.Ellipse) {
                        EllipseEntity ellipseEnt = (EllipseEntity)ent;
                        plist = ellipseEnt.mEllipse.toPeakList();
                    }
                    if (plist != null && 0 < plist.Count)
                        pos = plist.MinBy(p => p.length(pos));  //  最短位置
                    break;
                case "端点距離":
                    pos = getEndPoint(selectMenu, ent, pos);
                    break;
                case "中心点":
                    if (ent.mEntityId == EntityId.Arc) {
                        ArcEntity arcEnt = (ArcEntity)ent;
                        pos = arcEnt.mArc.mCp;
                    } else if (ent.mEntityId == EntityId.Ellipse) {
                        EllipseEntity ellipseEnt = (EllipseEntity)ent;
                        pos = ellipseEnt.mEllipse.mCp;
                    } else if (ent.mEntityId == EntityId.Parts) {
                        pos = ent.mArea.getCenter();
                    }
                    break;
                case "交点":
                    if (2 <= picks.Count) {
                        plist = mEntityData.intersection(picks[0], picks[1]);
                        pos = plist.MinBy(p => p.length(pos));  //  最短位置
                    }
                    break;
            }
            return pos;
        }

        /// <summary>
        /// 端点から距離指定の座標
        /// 負数だと延長線上の点
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="ent">要素</param>
        /// <param name="pos">ピック店</param>
        /// <returns>座標</returns>
        private PointD getEndPoint(string title, Entity ent, PointD pos)
        {
            double dis = getInputVal(title);
            if (!double.IsNaN(dis)) {
                LineD l = ent.getLine(pos);
                if (!l.isNaN()) {
                    if (l.ps.length(pos) > l.pe.length(pos))
                        l.inverse();
                    l.setLength(dis);
                    pos = l.pe;
                } else if (ent.mEntityId == EntityId.Arc) {
                    ArcD arc = ((ArcEntity)ent).mArc;
                    double ang = dis / arc.mR;
                    if (pos.length(arc.startPoint()) < pos.length(arc.endPoint()))
                        pos = arc.getPoint(arc.mSa + ang);
                    else
                        pos = arc.getPoint(arc.mEa - ang);
                }
            }
            return pos;
        }

        /// <summary>
        /// 数値入力ダイヤログ
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <returns>数値(cancelはNaN)</returns>
        private double getInputVal(string title)
        {
            InputBox dlg = new InputBox();
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = title;
            if (dlg.ShowDialog() == true)
                return ycalc.expression(dlg.mEditText);
            else
                return double.NaN;
        }

        /// <summary>
        /// ロケイトメニューの表示(Windowsメニューキー)
        /// </summary>
        private void locMenu()
        {
            if (mLocMode == OPEMODE.loc) {
                List<string> locMenu = new List<string>();
                locMenu.AddRange(mLocMenu);
                if (mOperation == OPERATION.translate || mOperation == OPERATION.copyTranslate) {
                    locMenu.Add("平行距離,繰返し数");
                    locMenu.Add("スライド距離");
                } else if (mOperation == OPERATION.offset || mOperation == OPERATION.copyOffset) {
                    locMenu.Add("平行距離,繰返し数");
                } else if (mOperation == OPERATION.createCircle || mOperation == OPERATION.createTangentCircle) {
                    locMenu.Add("半径");
                } else if (mOperation == OPERATION.rotate || mOperation == OPERATION.copyRotate) {
                    locMenu.Add("回転角,繰返し数");
                } else if (mOperation == OPERATION.scale) {
                    locMenu.Add("スケール");
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
        /// ロケイトメニューの処理(Windowsメニューキー)
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
                int repeat = 1;
                PointD wp = new PointD();
                PointD p1, p2;
                LineD line;
                Entity entity;
                PointD lastLoc = new PointD(0, 0);
                if (0 < mCommandOpe.mLocPos.Count)
                    lastLoc = mCommandOpe.mLocPos[mCommandOpe.mLocPos.Count - 1];
                switch (title) {
                    case "座標入力":
                        //  xxx,yyy で入力
                        valstr = dlg.mEditText.Split(',');
                        if (1 < valstr.Length) {
                            wp = new PointD(ycalc.expression(valstr[0]), ycalc.expression(valstr[1]));
                            mCommandOpe.mLocPos.Add(wp);
                            if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                                commandClear();
                        }
                        break;
                    case "相対座標入力":
                        //  xxx,yyy で入力
                        valstr = dlg.mEditText.Split(',');
                        if (1 < valstr.Length && 0 < mCommandOpe.mLocPos.Count) {
                            wp = new PointD(ycalc.expression(valstr[0]), ycalc.expression(valstr[1]));
                            if (2 < valstr.Length)
                                repeat = (int)ycalc.expression(valstr[2]);
                            for (int i = 0; i < repeat; i++) 
                                mCommandOpe.mLocPos.Add(wp + mCommandOpe.mLocPos.Last());
                            if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                                commandClear();
                        }
                        break;
                    case "平行距離,繰返し数":
                        //  移動またはコピー移動の時のみ
                        if (0 == mCommandOpe.mLocPos.Count)     //  方向を決めるロケイトが必要
                            break;
                        valstr = dlg.mEditText.Split(',');
                        val = ycalc.expression(valstr[0]);
                        if (1 < valstr.Length)
                            repeat = (int)ycalc.expression(valstr[1]);
                        entity = mEntityData.mEntityList[mCommandOpe.mPickEnt[mCommandOpe.mPickEnt.Count - 1].no];
                        if (entity.mEntityId == EntityId.Arc) {
                            ArcEntity arcEnt = (ArcEntity)entity;
                            LineD la = new LineD(arcEnt.mArc.mCp, lastLoc);
                            for (int i = 1; i < repeat + 1; i++) {
                                la.setLength(la.length() + val);
                                wp = la.pe.toCopy();
                                mCommandOpe.mLocPos.Add(wp);
                            }
                        } else if (entity.mEntityId == EntityId.Ellipse) {
                            EllipseEntity ellipseEnt = (EllipseEntity)entity;
                            LineD la = new LineD(ellipseEnt.mEllipse.mCp, lastLoc);
                            for (int i = 1; i < repeat + 1; i++) {
                                la.setLength(la.length() + val);
                                wp = la.pe.toCopy();
                                mCommandOpe.mLocPos.Add(wp);
                            }
                        } else {
                            line = entity.getLine(mCommandOpe.mPickEnt[mCommandOpe.mPickEnt.Count - 1].pos);
                            if (!line.isNaN()) {
                                for (int i = 1; i < repeat + 1; i++) {
                                    wp = line.offset(lastLoc, val * i);
                                    mCommandOpe.mLocPos.Add(wp);
                                }
                            }
                        }
                        if (0 < mCommandOpe.mLocPos.Count) {
                            if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                                commandClear();
                        }
                        break;
                    case "スライド距離":
                        //  移動またはコピー移動の時のみ
                        if (0 == mCommandOpe.mLocPos.Count)     //  方向を決めるロケイトが必要
                            break;
                        valstr = dlg.mEditText.Split(',');
                        val = ycalc.expression(valstr[0]);
                        entity = mEntityData.mEntityList[mCommandOpe.mPickEnt[mCommandOpe.mPickEnt.Count - 1].no];
                        line = entity.getLine(mCommandOpe.mPickEnt[mCommandOpe.mPickEnt.Count - 1].pos);
                        if (!line.isNaN()) {
                            LineD directLine = new LineD(line.centerPoint(), line.intersection(lastLoc));
                            wp = lastLoc + directLine.getVectorAngle(0, val);
                            mCommandOpe.mLocPos.Add(wp);
                            if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                                commandClear();
                        }
                        break;
                    case "半径":
                        valstr = dlg.mEditText.Split(',');
                        val = ycalc.expression(valstr[0]);
                        if (mOperation == OPERATION.createCircle) {
                            //  円の作成
                            wp = lastLoc + new PointD(val, 0);
                        } else if (mOperation == OPERATION.createTangentCircle && 2 <= mCommandOpe.mPickEnt.Count) {
                            //  接円の作成
                            mCommandOpe.mLocPos.Add(mDataDrawing.cnvScreen2World(new PointD(mPrevPosition)));
                            ArcD arc = mEntityData.tangentCircle(mCommandOpe.mPickEnt, mCommandOpe.mLocPos, val);
                            wp = arc.mCp;
                            mCommandOpe.mLocPos.RemoveAt(mCommandOpe.mLocPos.Count - 1);
                        } else {
                            break;
                        }
                        if (!wp.isNaN()) {
                            mCommandOpe.mLocPos.Add(wp);
                            if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                                commandClear();
                        }
                        break;
                    case "回転角,繰返し数":
                        valstr = dlg.mEditText.Split(',');
                        val = ycalc.expression(valstr[0]);
                        if (1 < valstr.Length)
                            repeat = (int)ycalc.expression(valstr[1]);
                        PointD vec = new PointD(1, 0);
                        for (int i = 1; i < repeat + 1; i++) {
                            vec.rotate(ylib.D2R(val));
                            wp = lastLoc + vec;
                            mCommandOpe.mLocPos.Add(wp);
                        }
                        if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                            commandClear();
                        break;
                    case "スケール":
                        valstr = dlg.mEditText.Split(',');
                        val = ycalc.expression(valstr[0]);
                        if (1 == mCommandOpe.mLocPos.Count) {
                            wp = lastLoc + new PointD(1, 0);
                            mCommandOpe.mLocPos.Add(wp);
                        } 
                        if (2 == mCommandOpe.mLocPos.Count) {
                            double dis = mCommandOpe.mLocPos[0].length(mCommandOpe.mLocPos[1]);
                            line = new LineD(mCommandOpe.mLocPos[0], mCommandOpe.mLocPos[1]);
                            line.setLength(dis * val);
                            mCommandOpe.mLocPos.Add(line.pe);
                            if (mCommandOpe.entityCommand(mOperation, mCommandOpe.mLocPos, mCommandOpe.mPickEnt))
                                commandClear();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 文字入力欄の文字列入力ダイヤログを開く
        /// </summary>
        private void textInput()
        {
            InputBox dlg = new InputBox();
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mMultiLine = true;
            dlg.mWindowSizeOutSet = true;
            dlg.Title = "文字列入力";
            dlg.mEditText = tbTextString.Text;
            if (dlg.ShowDialog() == true) {
                tbTextString.Text = dlg.mEditText;
            }
        }

        /// <summary>
        /// 作成レイヤーを設定する
        /// </summary>
        /// <param name="layerName">作成レイヤー名</param>
        private void setCreateLayer(string layerName)
        {
            mEntityData.addDispLayer(layerName);
            mEntityData.mPara.mCreateLayerName = layerName;
            mCommandOpe.mEntityData.mPara.mCreateLayerName = layerName;
            if (mCommandOpe.mEntityData.mPara.mOneLayerDisp) {
                //  1レイヤー表示
                mEntityData.mPara.mDispLayerBit &= mEntityData.getLayerBit(layerName);
            }
            mCommandOpe.mEntityData.mPara.mDispLayerBit = mEntityData.mPara.mDispLayerBit;
            cbCreateLayer.ItemsSource = mEntityData.getLayerNameList();
            cbCreateLayer.SelectedIndex = cbCreateLayer.Items.IndexOf(mCommandOpe.mEntityData.mPara.mCreateLayerName);
            if (mCommandOpe.mChkListDlg != null && mCommandOpe.mChkListDlg.IsVisible) {
                //  表示レイヤーダイヤログ表示
                mCommandOpe.setDispLayer();
            }
            mEntityData.updateData();
            disp(mEntityData);
            btDummy.Focus();         //  ダミーでフォーカスを外す
        }

        /// <summary>
        /// DXFファイルの選択とインポート
        /// </summary>
        /// <returns></returns>
        public string importAsFile()
        {
            List<string[]> filters = new List<string[]>() {
                    new string[] { "図面ファイル", "*.dxf" },
                    new string[] { "すべてのファイル", "*.*"}
                };
            string filePath = ylib.fileOpenSelectDlg("データ読込", ".", filters);
            if (filePath == null || filePath.Length == 0)
                return "";
            return importDxf(filePath);
        }

        /// <summary>
        /// DXFファイルのインポート
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string importDxf(string filePath)
        {
            //  DXFファイルの読込
            DxfReader dxfReader = new DxfReader(filePath);
            //  DXFデータをEntityDataに変換
            EntityData entityData = new EntityData();
            foreach (var ent in dxfReader.mEntityList) {
                switch (ent.mEntityName) {
                    case "LINE":
                        LineD line = ent.getLine();
                        Entity lineEntity = new LineEntity(line);
                        entityData.mEntityList.Add(lineEntity);
                        break;
                    case "ARC":
                    case "CIRCLE":
                        ArcD arc = ent.getArc();
                        Entity arcEntity = new ArcEntity(arc);
                        entityData.mEntityList.Add(arcEntity);
                        break;
                    case "LWPOLYLINE":
                    case "POLYLINE":
                        PolylineD polyline = ent.getPolyline();
                        Entity polylineEntity  = new PolylineEntity(polyline);
                        entityData.mEntityList.Add(polylineEntity);
                        break;
                    case "DIMENSION":
                        break;
                    case "MTEXT":
                    case "TEXT":
                        TextD text = ent.getText();
                        Entity textEntity = new TextEntity(text);
                        entityData.mEntityList.Add(textEntity);
                        break;
                }
            }
            entityData.updateData();
            //  CSVファイルに保存
            string path = mFileData.getItemFilePath(Path.GetFileNameWithoutExtension(filePath));
            if (File.Exists(path)) {
                if (ylib.messageBox(this,"ファイルが既に存在します。上書きしてもよいですか?", "", "確認", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                    return "";
            }
            entityData.saveData(path);

            return Path.GetFileNameWithoutExtension(filePath);
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
        /// データ表示
        /// </summary>
        private void disp(EntityData entyityData)
        {
            mDataDrawing.disp(entyityData, mCommandOpe.mBackColor, mCommandOpe.mEntityData.mPara.mGridSize);
            mDataDrawing.pickDisp(mEntityData, mCommandOpe.mPickEnt);
        }

        /// <summary>
        /// 図面全体を表示する
        /// </summary>
        private void dispFit()
        {
            if (mEntityData.mArea != null) {
                mEntityData.mArea.normalize();
                Box area = mEntityData.mArea.toCopy();
                area.Left = mEntityData.mArea.Left - mEntityData.mArea.Width * mDispLeftMargine;
                area.Right = mEntityData.mArea.Right + mEntityData.mArea.Width * mDispRightMargine;
                mCommandOpe.setDispArea(area);
                mDataDrawing.initDraw(mCommandOpe.mDispArea);
                disp(mEntityData);
            }
        }

        /// <summary>
        /// 図面幅に合わせて表示
        /// </summary>
        private void dispWidthFit()
        {
            if (mEntityData.mArea != null) {
                mEntityData.mArea.normalize();
                Box area = mDataDrawing.getWorldArea().toCopy();
                PointD center = area.getCenter();
                double rate = mEntityData.mArea.Width * area.Height / area.Width / mEntityData.mArea.Height;
                area.Left = mEntityData.mArea.Left - mEntityData.mArea.Width * mDispLeftMargine;
                area.Right = mEntityData.mArea.Right + mEntityData.mArea.Width * mDispRightMargine;
                if (rate < 1) {
                    area.Top = (area.Top - center.y) * rate + center.y;
                    area.Bottom = (area.Bottom - center.y) * rate + center.y;
                } else {
                    area.Top = mEntityData.mArea.Top;
                    area.Bottom = mEntityData.mArea.Bottom;
                }
                mCommandOpe.setDispArea(area);
                mDataDrawing.initDraw(mCommandOpe.mDispArea);
                disp(mEntityData);
            }
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
            scroll(dx, dy);
        }

        /// <summary>
        /// 画面スクロール
        /// </summary>
        /// <param name="dx">X移動量</param>
        /// <param name="dy">Y移動量</param>
        private void scroll(double dx, double dy)
        {
            mDataDrawing.scroll(mEntityData, dx, dy, mCommandOpe.mEntityData.mPara.mGridSize);
            mDataDrawing.pickDisp(mEntityData, mCommandOpe.mPickEnt);
        }

        /// <summary>
        /// 画面の拡大縮小
        /// </summary>
        /// <param name="scaleStep">拡大率</param>
        private void zoom(double scaleStep)
        {
            PointD wp = mDataDrawing.getWorldArea().getCenter();
            zoom(wp, scaleStep);
        }

        /// <summary>
        /// 画面の拡大縮小
        /// </summary>
        /// <param name="wp">スケール中心</param>
        /// <param name="scaleStep">拡大率</param>
        private void zoom(PointD wp, double scaleStep)
        {
            mDataDrawing.setWorldZoom(wp, scaleStep, true);
            disp(mEntityData);
        }

        /// <summary>
        /// ロケイト/ピックモードとロケイト数、ピック数の表示
        /// </summary>
        private void dispMode()
        {
            tbStatusInfo.Text = $"Mode:[{mLocMode}] Loc:[{mCommandOpe.mLocPos.Count}] Pick:[{mCommandOpe.mPickEnt.Count}]";
        }

        /// <summary>
        /// [システム設定]プロパティの初期値を設定
        /// </summary>
        private void systemSettingdlg()
        {
            SettingDlg dlg = new SettingDlg();
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "図面設定";
            dlg.mWorldWindow.Left = Properties.Settings.Default.WorldWindowLeft;
            //  表示エリア
            dlg.mWorldWindow.Bottom = Properties.Settings.Default.WorldWindowBottom;
            dlg.mWorldWindow.Right  = Properties.Settings.Default.WorldWindowRight;
            dlg.mWorldWindow.Top    = Properties.Settings.Default.WorldWindowTop;
            //  データプロパティ
            dlg.mPointSize  = Properties.Settings.Default.PointSize;
            dlg.mPointType  = Properties.Settings.Default.PointType;
            dlg.mThickness  = Properties.Settings.Default.Thickness;
            dlg.mLineType   = Properties.Settings.Default.LineType;
            dlg.mTextSize   = Properties.Settings.Default.TextSize;
            dlg.mArrowSize  = Properties.Settings.Default.ArrowSize;
            dlg.mArrowAngle = Properties.Settings.Default.ArrowAngle;
            //  システムプロパティ
            dlg.mGridSize = Properties.Settings.Default.GridSize;
            //  図面保存基準フォルダ
            dlg.mDataFolder   = mFileData.mBaseDataFolder;
            dlg.mSymbolFolder = mSymbolData.mSymbolFolder;
            dlg.mImageFolder  = mImageData.mImageFolder;
            dlg.mBackupFolder = mFileData.mBackupFolder;
            dlg.mDiffTool     = mFileData.mDiffTool;
            dlg.mShortCutFilePath = mShortCutPath;
            if (dlg.ShowDialog() == true) {
                //  表示エリア
                Properties.Settings.Default.WorldWindowLeft   = dlg.mWorldWindow.Left;
                Properties.Settings.Default.WorldWindowBottom = dlg.mWorldWindow.Bottom;
                Properties.Settings.Default.WorldWindowRight  = dlg.mWorldWindow.Right;
                Properties.Settings.Default.WorldWindowTop    = dlg.mWorldWindow.Top;
                loadDispArea();
                //  データプロパティ
                Properties.Settings.Default.PointSize  = dlg.mPointSize;
                Properties.Settings.Default.PointType  = dlg.mPointType;
                Properties.Settings.Default.Thickness  = dlg.mThickness;
                Properties.Settings.Default.LineType   = dlg.mLineType;
                Properties.Settings.Default.TextSize   = dlg.mTextSize;
                Properties.Settings.Default.ArrowSize  = dlg.mArrowSize;
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
                mFileData.mBackupFolder = dlg.mBackupFolder;
                if (!Directory.Exists(mFileData.mBackupFolder)) {
                    Directory.CreateDirectory(mFileData.mBackupFolder);
                }
                mSymbolData.mSymbolFolder = dlg.mSymbolFolder;
                if (!Directory.Exists(mSymbolData.mSymbolFolder))
                    Directory.CreateDirectory(mSymbolData.mSymbolFolder);
                mSymbolData.mBackupFolder = dlg.mBackupFolder;
                if (!Directory.Exists(mSymbolData.mBackupFolder)) {
                    Directory.CreateDirectory(mSymbolData.mBackupFolder);
                }
                mImageData.mImageFolder = dlg.mImageFolder;
                if (!Directory.Exists(mImageData.mImageFolder))
                    Directory.CreateDirectory(mImageData.mImageFolder);
                mFileData.mDiffTool = dlg.mDiffTool;
                mCommandData.loadShortCut(mShortCutPath);
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
                case "システム設定":
                    systemSettingdlg();
                    break;
                case "データバックアップ":
                    int count = 0;
                    count += mFileData.dataBackUp(false);
                    count += mSymbolData.dataBackUp(false);
                    count += mImageData.dataBackUp(false);
                    ylib.messageBox(this, $"{count} ファイルのバックアップを更新しました。");
                    break;
                case "図面データバックアップ":
                    mFileData.dataBackUp();
                    break;
                case "シンボルバックアップ":
                    mSymbolData.dataBackUp();
                    break;
                case "イメージファイルバックアップ":
                    mImageData.dataBackUp();
                    break;
                case "図面データバックアップ管理":
                    mFileData.dataRestor();
                    break;
                case "シンボルバックアップ管理":
                    mSymbolData.dataRestor();
                    break;
                case "イメージファイルバックアップ管理":
                    mImageData.dataRestor();
                    break;
                case "未使用キャッシュイメージファイル削除":
                    mFileData.squeezeImageCache(mImageData.mImageFolder);
                    break;
            }
        }

        /// <summary>
        /// システム値を表示図面に反映する
        /// </summary>
        public void setSystemProperty()
        {
            loadDispArea();
            loadDataProperty();
        }

        /// <summary>
        /// コントロールバーのシステム属性を設定する
        /// </summary>
        public void setZumenProperty()
        {
            cbColor.SelectedIndex     = ylib.mColorList.FindIndex(p => p.brush == mCommandOpe.mEntityData.mPara.mColor);
            cbGridSize.SelectedIndex  = mGridSizeMenu.FindIndex(Math.Abs(mCommandOpe.mEntityData.mPara.mGridSize));
            cbPointType.SelectedIndex = mCommandOpe.mEntityData.mPara.mPointType;
            cbPointSize.SelectedIndex = mEntSizeMenu.FindIndex(p => mCommandOpe.mEntityData.mPara.mPointSize <= p);
            cbLineType.SelectedIndex  = mCommandOpe.mEntityData.mPara.mLineType;
            cbEntSize.SelectedIndex   = mEntSizeMenu.FindIndex(p => mCommandOpe.mEntityData.mPara.mThickness <= p);
            cbTextSize.SelectedIndex  = mTextSizeMenu.FindIndex((p) => mCommandOpe.mEntityData.mPara.mTextSize <= p);
            cbTextHorizontal.SelectedIndex = mCommandOpe.mEntityData.mPara.mHa == HorizontalAlignment.Left ? 0 :
                                         mCommandOpe.mEntityData.mPara.mHa == HorizontalAlignment.Center ? 1 : 2;
            cbTextVertical.SelectedIndex   = mCommandOpe.mEntityData.mPara.mVa == VerticalAlignment.Top ? 0 :
                                         mCommandOpe.mEntityData.mPara.mVa == VerticalAlignment.Center ? 1 : 2;
            cbTextRotate.SelectedIndex = mTextRotateMenu.FindIndex(p => ylib.R2D(mCommandOpe.mEntityData.mPara.mTextRotate) <= p);
            cbCreateLayer.ItemsSource  = mEntityData.getLayerNameList();
            setCreateLayer(mEntityData.mPara.mCreateLayerName);
        }

        /// <summary>
        /// 表示エリアの初期値を取得
        /// </summary>
        private void loadDispArea()
        {
            mCommandOpe.mInitArea.Left   = Properties.Settings.Default.WorldWindowLeft;
            mCommandOpe.mInitArea.Bottom = Properties.Settings.Default.WorldWindowBottom;
            mCommandOpe.mInitArea.Right  = Properties.Settings.Default.WorldWindowRight;
            mCommandOpe.mInitArea.Top    = Properties.Settings.Default.WorldWindowTop;
        }

        /// <summary>
        /// 製図機能の初期値を取得
        /// </summary>
        private void loadDataProperty()
        {
            mCommandOpe.mEntityData.mPara = new DrawingPara();
            mCommandOpe.mEntityData.mPara.mPointSize  = Properties.Settings.Default.PointSize;
            mCommandOpe.mEntityData.mPara.mPointType  = Properties.Settings.Default.PointType;
            mCommandOpe.mEntityData.mPara.mThickness  = Properties.Settings.Default.Thickness;
            mCommandOpe.mEntityData.mPara.mLineType   = Properties.Settings.Default.LineType;
            mCommandOpe.mEntityData.mPara.mTextSize   = Properties.Settings.Default.TextSize;
            mCommandOpe.mEntityData.mPara.mArrowSize  = Properties.Settings.Default.ArrowSize;
            mCommandOpe.mEntityData.mPara.mArrowAngle = Properties.Settings.Default.ArrowAngle;
            mCommandOpe.mEntityData.mPara.mGridSize   = Properties.Settings.Default.GridSize;
        }

        /// <summary>
        /// CAD画面をクリップボードにコピー
        /// </summary>
        public void screenCopy()
        {
            BitmapSource bitmapSource = ylib.canvas2Bitmap(cvCanvas, lbCommand.ActualWidth + 10);
            Clipboard.SetImage(bitmapSource);
        }

        /// <summary>
        /// CAD画面をファイルに保存
        /// </summary>
        public void screenSave()
        {
            BitmapSource bitmapSource = ylib.canvas2Bitmap(cvCanvas, lbCommand.ActualWidth + 10);
            string path = ylib.fileSaveSelectDlg("イメージ保存", ".", mCommandOpe.mImageFilters);
            if (0 < path.Length) {
                if (Path.GetExtension(path).Length == 0)
                    path += ".png"; 
                ylib.saveBitmapImage(bitmapSource, path);
            }
        }

        /// <summary>
        /// 印刷処理
        /// </summary>
        public void print()
        {
            Box dispArea = mCommandOpe.mDispArea.toCopy();
            SelectMenu dlg = new SelectMenu();
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "印刷の形式";
            dlg.mMenuList = mPrintTypeMenu.ToArray();
            if (dlg.ShowDialog() == true) {
                //  印刷の実行
                if (dlg.mSelectIndex == 0)
                    mDataDrawing.setPrint(mEntityData, mCommandOpe.mDispArea);
                else
                    mDataDrawing.setPrint(mEntityData, mCommandOpe.mDispArea, System.Printing.PageOrientation.Portrait);
                //  画面表示に戻す
                mDataDrawing.initDraw(dispArea);
                disp(mEntityData);
            }
        }
    }
}
