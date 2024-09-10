
using CoreLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public string mAppName = "CadApp";                      //  アプリケーション名
        private string mHelpFile = "CadAppManual.pdf";          //  PDFのヘルプファイル
        private string mShortCutPath = "ShortCut.csv";          //  ショートカットキーファイル
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
        private List<string> mEntityMaskMenu = new List<string>() {
            "なし", "点", "線分", "円弧", "折線", "ポリゴン", "テキスト",
            "寸法線,矢印,ラベル,シンボル"
        };
        private List<double> mFilletSizeMenu = new List<double>(){
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 15, 16, 18, 20, 25, 30
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

        private EntityData mEntityData;                     //  要素データ

        private List<PointD> mAreaLoc = new List<PointD>() {
            new PointD(), new PointD()
        };
        public CommandOpe mCommandOpe;
        private CommandData mCommandData = new CommandData();
        public OPERATION mOperation = OPERATION.non;
        public OPEMODE mLocMode = OPEMODE.pick;            //  マウスのLocモードとPickモード
        public OPEMODE mPrevMode = OPEMODE.pick;

        private FileData mFileData;
        private DataDrawing mDataDrawing;
        private LocPick mLocPick;
        public SymbolData mSymbolData;
        public ImageData mImageData;

        private YCalc ycalc = new YCalc();
        private YLib ylib = new YLib();

        public MainWindow()
        {
            InitializeComponent();

            mDataDrawing = new DataDrawing(cvCanvas, this);
            mEntityData  = new EntityData();
            mFileData    = new FileData(this);
            mSymbolData  = new SymbolData(this);
            mImageData   = new ImageData(this);
            mLocPick     = new LocPick(this, mEntityData);
            mCommandOpe  = new CommandOpe(mEntityData, mLocPick, this);

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
            cbEntityMask.ItemsSource  = mEntityMaskMenu;
            cbTextSize.ItemsSource    = mTextSizeMenu;
            cbTextHorizontal.ItemsSource = mHorizontalAlignmentMenu;
            cbTextVertical.ItemsSource = mVerticalAlignmentMenu;
            cbTextRotate.ItemsSource   = mTextRotateMenu;
            cbFilletSize.Items.Clear();
            mFilletSizeMenu.ForEach(p => cbFilletSize.Items.Add(p));
            cbFilletSize.SelectedIndex = 0;

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
            mDataDrawing.disp(mEntityData, mLocPick.mPickEnt);
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
            if (mCommandOpe.mMemoDlg != null)
                mCommandOpe.mMemoDlg.Close();
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
            mDataDrawing.disp(mEntityData, mLocPick.mPickEnt);
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
                    mDataDrawing.disp(mEntityData, mLocPick.mPickEnt);
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
                if (ylib.onAltKey()) {
                    if (mOperation == OPERATION.stretch)
                        mOperation = OPERATION.stretchArc;
                    else if (mOperation == OPERATION.copyStretch)
                        mOperation = OPERATION.copyStretchArc;
                }
                if (0 < mCommandOpe.mEntityData.mPara.mGridSize)
                    wp.round(Math.Abs(mCommandOpe.mEntityData.mPara.mGridSize));
                mLocPick.mLocPos.Add(wp);
                mCommandOpe.mTextString = tbTextString.Text;
                if (mCommandOpe.entityCommand(mOperation, mLocPick.mLocPos, mLocPick.mPickEnt, false))
                    commandClear();
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
            List<int> picks = mLocPick.getPickNo(pickPos, mDataDrawing.screen2worldXlength(mPickBoxSize));
            if (mLocMode == OPEMODE.loc) {
                //  ロケイトモード
                mCommandOpe.mTextString = tbTextString.Text;
                if (0 < mLocPick.mLocPos.Count && picks.Count == 0) {
                    //  ロケイト数不定の要素作成(ポリライン、ポリゴン)
                    if (mCommandOpe.entityCommand(mOperation, mLocPick.mLocPos, mLocPick.mPickEnt, picks.Count == 0))
                        commandClear();
                    return;
                } else {
                    if (0 < picks.Count) {
                        //  ピックされているときは位置を自動判断(Ctrlキーのメニュー表示も含む)
                        PointD wp = mLocPick.autoLoc(pickPos, picks);
                    }
                    if (mCommandOpe.entityCommand(mOperation, mLocPick.mLocPos, mLocPick.mPickEnt, picks.Count == 0))
                        commandClear();
                }
            } else {
                //  ピックモード
                if (0 < picks.Count) {
                    int pickNo = mLocPick.pickSelect(picks, mLocMode);
                    if (0 <= pickNo) {
                        //  ピック要素の登録
                        mLocPick.addPick((pickNo, pickPos.toCopy()), true);
                        mDataDrawing.pickDisp(mEntityData, mLocPick.mPickEnt);
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
                        mDataDrawing.scroll(mPrevPosition, point, mEntityData, mLocPick.mPickEnt);
                    } else {
                        return;
                    }
                } else {
                    //  ドラッギング表示
                    mLocPick.mLocPos.Add(wp);
                    mDataDrawing.setEntityProperty(mCommandOpe);
                    mDataDrawing.dragging(mEntityData, mLocPick.mLocPos, mLocPick.mPickEnt, mOperation);
                    mLocPick.mLocPos.RemoveAt(mLocPick.mLocPos.Count - 1);
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
                    mDataDrawing.scroll(0, mScrollSize * scaleStep, mEntityData, mLocPick.mPickEnt);
                } else if (onAltKey()) {
                    double scaleStep = e.Delta > 0 ? 1 : -1;
                    mDataDrawing.scroll(mScrollSize * scaleStep, 0, mEntityData, mLocPick.mPickEnt);
                } else {
                    double scaleStep = e.Delta > 0 ? 1.2 : 1 / 1.2;
                    Point point = e.GetPosition(cvCanvas);
                    PointD wp = mDataDrawing.cnvScreen2World(new PointD(point));
                    mDataDrawing.zoom(wp, scaleStep, mEntityData, mLocPick.mPickEnt);
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
                if (mCommandOpe.mMemoDlg != null)
                    mCommandOpe.mMemoDlg.Close();
                if (mCommandOpe.mChkListDlg != null)
                    mCommandOpe.mChkListDlg.Close();
                if (mCommandOpe.mSymbolDlg != null)
                    mCommandOpe.mSymbolDlg.Close();
                if (mCommandOpe.openFile(mFileData.getItemFilePath(lbItemList.Items[index].ToString() ?? ""))) {
                    mFileData.mDataName = lbItemList.Items[index].ToString() ?? "";
                    dispTitle();
                    setZumenProperty();
                    commandClear();
                    dispMode();
                    mDataDrawing.dispFit(mEntityData, mLocPick.mPickEnt);
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
                string item = mFileData.importAsFile();
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
                mDataDrawing.disp(mEntityData, mLocPick.mPickEnt);
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
        /// ピック要素マスク
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbEntityMask_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int n = cbEntityMask.SelectedIndex;
            switch (mEntityMaskMenu[n]) {
                case "なし": mLocPick.mPickMask = EntityId.Non; break;
                case "点": mLocPick.mPickMask = EntityId.Point; break;
                case "線分": mLocPick.mPickMask = EntityId.Line; break;
                case "円弧": mLocPick.mPickMask = EntityId.Arc; break;
                case "折線": mLocPick.mPickMask = EntityId.Polyline; break;
                case "ポリゴン": mLocPick.mPickMask = EntityId.Polygon; break;
                case "テキスト": mLocPick.mPickMask = EntityId.Text; break;
                case "寸法線,矢印,ラベル,シンボル": mLocPick.mPickMask = EntityId.Parts; break;
                default: mLocPick.mPickMask = EntityId.Non; break;

            }
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
        /// R面取りの半径設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbFilletSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbFilletSize.SelectedIndex;
            if (0 <= index) {
                mCommandOpe.mEntityData.mPara.mFilletSize = mFilletSizeMenu[index];
            }
        }

        /// <summary>
        /// R面取りの半径設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbFilletSize_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                mCommandOpe.mEntityData.mPara.mFilletSize = ylib.doubleParse(cbFilletSize.Text, 0);
                if (!mFilletSizeMenu.Contains(mCommandOpe.mEntityData.mPara.mFilletSize)) {
                    mFilletSizeMenu.Add(mCommandOpe.mEntityData.mPara.mFilletSize);
                    mFilletSizeMenu.Sort();
                    cbFilletSize.Items.Clear();
                    mFilletSizeMenu.ForEach(p => cbFilletSize.Items.Add(p));
                }
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
                mDataDrawing.disp(mEntityData, mLocPick.mPickEnt);
            } else if (button.Name.CompareTo("btZoomOut") == 0) {
                mDataDrawing.setWorldZoom(0.75);
                mDataDrawing.disp(mEntityData, mLocPick.mPickEnt);
            } else if (button.Name.CompareTo("btZoomFit") == 0) {
                mDataDrawing.dispFit(mEntityData, mLocPick.mPickEnt);
            } else if (button.Name.CompareTo("btZoomWidthFit") == 0) {
                mDataDrawing.dispWidthFit(mEntityData, mLocPick.mPickEnt);
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
                    case Key.F1: mCommandOpe.mEntityData.mPara.mGridSize *= -1;
                                    mDataDrawing.disp(mEntityData, mLocPick.mPickEnt); break;       //  グリッド表示反転
                    case Key.F2: break;
                    case Key.F3: break;
                    case Key.F4: break;
                    case Key.F5: break;
                    case Key.Left: mDataDrawing.scroll(mScrollSize, 0, mEntityData, mLocPick.mPickEnt); break;  //  左画面移動
                    case Key.Right: mDataDrawing.scroll(-mScrollSize, 0, mEntityData, mLocPick.mPickEnt); break;//  右画面移動
                    case Key.Up: mDataDrawing.scroll(0, mScrollSize, mEntityData, mLocPick.mPickEnt); break;    //  上画面移動
                    case Key.Down: mDataDrawing.scroll(0, -mScrollSize, mEntityData, mLocPick.mPickEnt); break; //  下画面移動
                    case Key.PageUp: mDataDrawing.zoom(1.1, mEntityData, mLocPick.mPickEnt); break;             //  画面拡大
                    case Key.PageDown: mDataDrawing.zoom(1 / 1.1, mEntityData, mLocPick.mPickEnt); break;       //  画面縮小
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
                    case Key.F1: mDataDrawing.disp(mEntityData, mLocPick.mPickEnt); break;          //  再表示
                    case Key.F2: mPrevMode = mLocMode; mLocMode = OPEMODE.areaDisp; break;          //  領域拡大
                    case Key.F3: mDataDrawing.dispFit(mEntityData, mLocPick.mPickEnt); break;       //  全体表示
                    case Key.F4: mDataDrawing.zoom(1.2, mEntityData, mLocPick.mPickEnt); break;     //  拡大表示
                    case Key.F5: mDataDrawing.zoom(1 / 1.2, mEntityData, mLocPick.mPickEnt); break; //  縮小表示
                    case Key.F6: mDataDrawing.dispWidthFit(mEntityData, mLocPick.mPickEnt); break;  //  全幅表示
                    case Key.F7: mPrevMode = mLocMode; mLocMode = OPEMODE.areaPick; break;          //  領域ピック
                    case Key.F8: locMenu(); break;                      //  ロケイトメニュー
                    case Key.F9: break;
                    case Key.F10: break;
                    case Key.F11: tbTextString.Focus(); break;          //  テキスト入力ボックスにフォーカス
                    case Key.F12: cbCommand.Focus(); break;             //  コマンド入力ボックスにフォーカス
                    case Key.Back:                                      //  ロケイト点を一つ戻す
                        if (0 < mLocPick.mLocPos.Count) {
                            mLocPick.mLocPos.RemoveAt(mLocPick.mLocPos.Count - 1);
                            mDataDrawing.dragging(mEntityData, mLocPick.mLocPos, mLocPick.mPickEnt, mOperation);
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
                            mDataDrawing.disp(mEntityData, mLocPick.mPickEnt);
                        } else if (opeMode == OPEMODE.areaPick) {
                            //  領域ピック
                            PointD pickPos = dispArea.getCenter();
                            List<int> picks = mLocPick.getPickNo(dispArea);
                            foreach (int pickNo in picks)
                                mLocPick.addPick((pickNo, pickPos), true);
                            mDataDrawing.pickDisp(mEntityData, mLocPick.mPickEnt);
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
        /// マウスのダブルクリックの処理
        /// ピックした要素の文字列変更または属性変更
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        private void doubleClicKOpe(PointD pickPos)
        {
            List<int> picks = mLocPick.getPickNo(pickPos, mDataDrawing.screen2worldXlength(mPickBoxSize));
            if (0 < picks.Count) {
                int pickNo = mLocPick.pickSelect(picks, mLocMode);
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
            mLocPick.mPickEnt.Clear();
            mLocPick.mLocPos.Clear();
            mLocMode = OPEMODE.pick;
            mCommandData.mCommandLevel = 0;
            mOperation = OPERATION.non;
            lbCommand.ItemsSource = mCommandData.getMainCommand();
            lbCommand.SelectedIndex = -1;
            chOneLayer.IsChecked = mCommandOpe.mEntityData.mPara.mOneLayerDisp;
            mDataDrawing.disp(mEntityData, mLocPick.mPickEnt);
            dispTitle();
        }

        /// <summary>
        /// ロケイトメニューの表示(Windowsメニューキー)
        /// </summary>
        private void locMenu()
        {
            mLocPick.locMenu(mLocMode, mOperation, mDataDrawing.cnvScreen2World(new PointD(mPrevPosition)));
            if (mCommandOpe.entityCommand(mOperation, mLocPick.mLocPos, mLocPick.mPickEnt, false))
                commandClear();
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
            mEntityData.mLayer.addDispLayer(layerName);
            mEntityData.mPara.mCreateLayerName = layerName;
            mCommandOpe.mEntityData.mPara.mCreateLayerName = layerName;
            if (mCommandOpe.mEntityData.mPara.mOneLayerDisp) {
                //  1レイヤー表示
                mEntityData.mPara.mDispLayerBit &= mEntityData.mLayer.getLayerBit(layerName);
            }
            mCommandOpe.mEntityData.mPara.mDispLayerBit = mEntityData.mPara.mDispLayerBit;
            cbCreateLayer.ItemsSource = mEntityData.mLayer.getLayerNameList();
            cbCreateLayer.SelectedIndex = cbCreateLayer.Items.IndexOf(mCommandOpe.mEntityData.mPara.mCreateLayerName);
            if (mCommandOpe.mChkListDlg != null && mCommandOpe.mChkListDlg.IsVisible) {
                //  表示レイヤーダイヤログ表示
                mCommandOpe.setDispLayer();
            }
            mEntityData.updateData();
            mDataDrawing.disp(mEntityData, mLocPick.mPickEnt);
            btDummy.Focus();         //  ダミーでフォーカスを外す
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
        /// ロケイト/ピックモードとロケイト数、ピック数の表示
        /// </summary>
        private void dispMode()
        {
            tbStatusInfo.Text = $"Mode:[{mLocMode}] Loc:[{mLocPick.mLocPos.Count}] Pick:[{mLocPick.mPickEnt.Count}]";
        }

        /// <summary>
        /// タイトルバーの表示
        /// </summary>
        public void dispTitle()
        {
            Title = $"{mAppName} [{mFileData.mDataName}][{mEntityData.drawEntityCount()}/{mEntityData.mEntityList.Count}]";
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
            dlg.mFileData = mFileData;
            dlg.mSymbolData = mSymbolData;
            dlg.mImageData = mImageData;
            //  表示エリア
            dlg.mWorldWindow.Left   = Properties.Settings.Default.WorldWindowLeft;
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
                mImageData.mBackupFolder = dlg.mBackupFolder;
                mFileData.mDiffTool = dlg.mDiffTool;
                mCommandData.loadShortCut(mShortCutPath);
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
            cbColor.SelectedIndex      = ylib.mColorList.FindIndex(p => p.brush == mCommandOpe.mEntityData.mPara.mColor);
            cbGridSize.SelectedIndex   = mGridSizeMenu.FindIndex(Math.Abs(mCommandOpe.mEntityData.mPara.mGridSize));
            cbEntityMask.SelectedIndex = 0;
            cbPointType.SelectedIndex  = mCommandOpe.mEntityData.mPara.mPointType;
            cbPointSize.SelectedIndex  = mEntSizeMenu.FindIndex(p => mCommandOpe.mEntityData.mPara.mPointSize <= p);
            cbLineType.SelectedIndex   = mCommandOpe.mEntityData.mPara.mLineType;
            cbEntSize.SelectedIndex    = mEntSizeMenu.FindIndex(p => mCommandOpe.mEntityData.mPara.mThickness <= p);
            cbTextSize.SelectedIndex   = mTextSizeMenu.FindIndex((p) => mCommandOpe.mEntityData.mPara.mTextSize <= p);
            cbTextHorizontal.SelectedIndex = mCommandOpe.mEntityData.mPara.mHa == HorizontalAlignment.Left ? 0 :
                                         mCommandOpe.mEntityData.mPara.mHa == HorizontalAlignment.Center ? 1 : 2;
            cbTextVertical.SelectedIndex = mCommandOpe.mEntityData.mPara.mVa == VerticalAlignment.Top ? 0 :
                                         mCommandOpe.mEntityData.mPara.mVa == VerticalAlignment.Center ? 1 : 2;
            cbTextRotate.SelectedIndex  = mTextRotateMenu.FindIndex(p => ylib.R2D(mCommandOpe.mEntityData.mPara.mTextRotate) <= p);
            cbCreateLayer.ItemsSource   = mEntityData.mLayer.getLayerNameList();
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
            mCommandOpe.mEntityData.mPara.init();
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
                mDataDrawing.disp(mEntityData, mLocPick.mPickEnt);
            }
        }
    }
}
