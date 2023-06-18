using CoreLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace CadApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double mWindowWidth;            //  ウィンドウの高さ
        private double mWindowHeight;           //  ウィンドウ幅
        private double mPrevWindowWidth;        //  変更前のウィンドウ幅
        private WindowState mWindowState = WindowState.Normal;  //  ウィンドウの状態(最大化/最小化)

        private double[] mGridSizeMenu = {
            0, 0.1, 0.2, 0.5, 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000
        };
        private double[] mTextSizeMenu = {
            1, 2, 4, 6, 8, 9, 10, 11, 12, 13, 14, 16, 18, 20, 24, 28, 32, 36, 40, 48, 56, 64
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

        private Box mDispArea = new Box(-50, 100, 100, -50);//  表示領域
        private Brush mBackColor = Brushes.White;           //  背景色
        private Point mPrevPosition;                        //  マウスの前回位置(画面スクロール)
        private bool mMouseLeftButtonDown = false;          //  マウス左ボタン状態
        private BitmapSource mBitmapSource;                 //  画像データ(描画データのバッファリング)
        private int mPickBoxSize = 10;                      //  ピック領域サイズ
        private int mScrollSize = 19;                       //  キーによるスクロール単位
        private int mGridMinmumSize = 10;                   //  グリッドの最小表示スクリーンサイズ
        private double mGridSize = 1.0;                     //  マウス座標の丸め値
        private List<PointD> mLocPos = new List<PointD>();  //  マウス指定点
        private List<int> mPickEnt = new List<int>();       //  ピックした要素リスト
        private Brush mDraggingColor = Brushes.Blue;        //  ドラッギング時の色

        //private int mPointType = 0;                         //  点種
        //private int mLineType = 0;                          //  線種
        //private double mEntSize = 1;                        //  線の太さ
        //private double mPointSize = 1;                      //  点の大きさ
        //private double mTextSize = 12;                      //  文字サイズ
        //private double mTextRotate = 0;                     //  文字列の回転角
        //private HorizontalAlignment mHa = HorizontalAlignment.Left; //  水平アライメント
        //private VerticalAlignment mVa = VerticalAlignment.Top;      //  垂直アライメント
        //private Brush mCreateColor = Brushes.Black;         //  要素の色

        private EntityData mEntityData;                     //  要素データ

        private CommandOpe mCommandOpe;
        private CommandData mCommandData = new CommandData();
        private OPERATION mOperation = OPERATION.non;
        private bool mLocMode = false;

        private YWorldDraw ydraw;
        private YLib ylib = new YLib();

        public MainWindow()
        {
            InitializeComponent();

            WindowFormLoad();

            ydraw = new YWorldDraw(cvCanvas);
            mEntityData = new EntityData();
            mCommandOpe = new CommandOpe(mEntityData);
            mCommandOpe.mWindow = this;
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
            cbColor.DataContext       = ydraw.mColorList;
            cbGridSize.ItemsSource    = mGridSizeMenu;
            cbPointType.ItemsSource   = mPointTypeMenu;
            cbLineType.ItemsSource    = mLineTypeMenu;
            cbEntSize.ItemsSource     = mEntSizeMenu;
            cbTextSize.ItemsSource    = mTextSizeMenu;
            setSystemProperty();

            initDraw();
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

            initDraw();
            disp();
        }

        /// <summary>
        /// Windows 終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
        }

        /// <summary>
        /// Window状態を保存する
        /// </summary>
        private void WindowFormSave()
        {
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
            keyCommand(e.Key, e.KeyboardDevice.Modifiers == ModifierKeys.Control);
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
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control) {
                //  要素作成
                addCommand(ydraw.cnvScreen2World(new PointD(e.GetPosition(cvCanvas))));
            }
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
            PointD pickPos = ydraw.cnvScreen2World(new PointD(e.GetPosition(cvCanvas)));
            List<int> picks = getPickNo(pickPos);
            if (0 < mLocPos.Count && 
                (mOperation == OPERATION.createPolyline || mOperation == OPERATION.createPolygon)) {
                if (!pickEndMenu(picks)) {
                    //  要素登録(Polyline<Polygon)
                    mCommandOpe.mTextString = tbTextString.Text;
                    mCommandOpe.createData(mLocPos, mOperation);
                    disp();
                    mLocPos.Clear();
                    return;
                }
            }
            if (0 < picks.Count) {
                int pickNo = pickSelect(picks);
                if (mLocMode) {
                    //  要素の登録または編集
                    addCommand(autoLoc(pickPos, pickNo));
                } else if(0 <= pickNo) {
                    //  ピック要素の登録
                    mPickEnt.Add(pickNo);
                    pickDisp(mPickEnt);
                }
            }
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
            PointD wp = ydraw.cnvScreen2World(new PointD(point));
            wp.round(Math.Abs(mGridSize));
            tbPosition.Text = $"{wp.x.ToString("F2")},{wp.y.ToString("F2")}";   //  マウス座標表示
            if (mMouseLeftButtonDown && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                //  画面の移動(Ctrl + 左ボタン)
                if (ylib.distance(point, mPrevPosition) > 5) {
                    scroll(mPrevPosition, point);
                } else {
                    return;
                }
            } else if ((0 < mLocPos.Count && mOperation != OPERATION.non) ||
                (0 == mLocPos.Count && (mOperation == OPERATION.createPoint || mOperation == OPERATION.createText))) {
                //  ドラッギング表示
                mLocPos.Add(wp);
                dragging(mLocPos, mOperation);
                mLocPos.RemoveAt(mLocPos.Count - 1);
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
                PointD wp = ydraw.cnvScreen2World(new PointD(point));
                ydraw.mWorld.zoom(wp, scaleStep, true);
                disp();
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
                mCommandOpe.mCreateColor = ydraw.mColorList[index].brush; ;
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
                mGridSize = mGridSizeMenu[index];
                dispGrid(mGridSize);
                disp();
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
                mCommandOpe.mEntSize = mEntSizeMenu[index];
        }

        /// <summary>
        /// 文字サイズの設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTextSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbTextSize.SelectedIndex;
            if (0 <= index)
                mCommandOpe.mTextSize = mTextSizeMenu[index];
        }

        /// <summary>
        /// 再表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btZoomOriginal_Click(object sender, RoutedEventArgs e)
        {
            disp();
        }

        /// <summary>
        /// [ZoomIn] 拡大表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ydraw.mWorld.zoom(2);
            disp();
        }

        /// <summary>
        /// [ZoomOut] 縮小表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ydraw.mWorld.zoom(0.5);
            disp();
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
            executeCmd(OPERATION.open);
        }

        /// <summary>
        /// [Save] データの保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            executeCmd(OPERATION.save);
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="key">キーコード</param>
        /// <param name="control">コントロールキーの有無</param>
        private void keyCommand(Key key, bool control)
        {
            if (control) {
                switch (key) {
                    case Key.F1:    mGridSize *= -1; disp(); break;        //  グリッド表示反転
                    case Key.Left:  scroll(-mScrollSize, 0); break;
                    case Key.Right: scroll(mScrollSize, 0); break;
                    case Key.Up:    scroll(0, -mScrollSize); break;
                    case Key.Down:  scroll(0, mScrollSize); break;
                }
            } else {
                switch (key) {
                    case Key.Escape: commandClear(); break;             //  ESCキーでキャンセル
                    case Key.Return:
                        if (mCommandOpe.keyCommand(tbCommand.Text))
                            disp();
                        break;
                    case Key.F1: disp(); break;                         //  再表示
                    case Key.F3: dispFit(); break;                      //  全体表示
                    case Key.F4: ydraw.mWorld.zoom(2); disp(); break;   //  拡大表示
                    case Key.F5: ydraw.mWorld.zoom(0.5); disp(); break; //  縮小表示
                    case Key.Back:                                      //  ロケイト点を一つ戻す
                        if (0 < mLocPos.Count) {
                            mLocPos.RemoveAt(mLocPos.Count - 1);
                            disp();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// ピックした要素Noを求める
        /// </summary>
        /// <param name="pickPos">ピック位置(Worls座標)</param>
        /// <returns>要素No</returns>
        private List<int> getPickNo(PointD pickPos)
        {
            double xd = ydraw.screen2worldXlength(mPickBoxSize);    //  ピック領域
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
                    executeCmd(cmd.operation);
                }
            } else if (mCommandData.mCommandLevel == 1) {
                //  サブコマンド処理
                mCommandData.mSub = command;
                Command cmd = mCommandData.getCommand(mCommandData.mMain, mCommandData.mSub);
                executeCmd(cmd.operation);
            }
        }

        /// <summary>
        /// コマンド処理
        /// </summary>
        /// <param name="command"></param>
        private void executeCmd(OPERATION operation)
        {
            mOperation = operation;
            bool result = false;
            mLocMode = false;

            switch (operation) {
                case OPERATION.createPoint:
                case OPERATION.createLine:
                case OPERATION.createRect:
                case OPERATION.createPolyline:
                case OPERATION.createPolygon:
                case OPERATION.createArc:
                case OPERATION.createCircle:
                case OPERATION.createEllipse:
                case OPERATION.createText:
                    mLocMode = true;
                    result = false;
                    break;
                case OPERATION.back:
                    result = true;
                    break;
                case OPERATION.remove:
                    result = mEntityData.removeEnt(mPickEnt);
                    break;
                case OPERATION.translate:
                case OPERATION.rotate:
                case OPERATION.mirror:
                    mLocMode = true;
                    result = false;
                     break;
                case OPERATION.copy:
                    break;
                case OPERATION.scaling:
                    break;
                case OPERATION.textChange:
                    result = mCommandOpe.changeText(mPickEnt);
                    break;
                case OPERATION.changeProperty:
                    result = mCommandOpe.changeProperty(mPickEnt);
                    break;
                case OPERATION.measureDistance:
                    break;
                case OPERATION.measureAngle:
                    break;
                case OPERATION.info:
                    result = mCommandOpe.infoEntity(mPickEnt);
                    break;
                case OPERATION.systemIinfo:
                    if (mCommandOpe.systemProperty())
                        setSystemProperty();
                    break;
                case OPERATION.allClear:
                    result = allClear();
                    break;
                case OPERATION.open:
                    if (mCommandOpe.openFile()) {
                        mDispArea = mEntityData.mArea.toCopy();
                        mDispArea.normalize();
                        initDraw();
                        disp();
                    }
                    result = true;
                    break;
                case OPERATION.save:
                    mCommandOpe.saveAsFile();
                    result = true;
                    break;
                case OPERATION.close:
                    Close();
                    break;
                case OPERATION.cancel:
                    cancel();
                    break;
                case OPERATION.gridSize:
                    gridSet();
                    result = true;
                    break;
                case OPERATION.screenCopy:
                    BitmapSource bitmapSource = canvas2Bitmap(cvCanvas);
                    Clipboard.SetImage(bitmapSource);
                    result = true;
                    break;
            }
            if (result) {
                commandClear();
            }
        }

        /// <summary>
        /// コマンドデータをクリアする
        /// </summary>
        private void commandClear()
        {
            //  メインコマンドに戻る
            mLocPos.Clear();
            mPickEnt.Clear();
            mLocMode = false;
            mCommandData.mCommandLevel = 0;
            mOperation = OPERATION.non;
            lbCommand.ItemsSource = mCommandData.getMainCommand();
            lbCommand.SelectedIndex = -1;
            disp();
        }

        /// <summary>
        /// 要素追加コマンド
        /// </summary>
        /// <param name="pos">座標(World座標)</param>
        private void addCommand(PointD wp)
        {
            wp.round(Math.Abs(mGridSize));
            mLocPos.Add(wp);
            if (mOperation == OPERATION.createPoint || mOperation == OPERATION.createLine
                || mOperation == OPERATION.createRect || mOperation == OPERATION.createArc
                || mOperation == OPERATION.createCircle || mOperation == OPERATION.createText) {
                //  要素の追加 (Ctrlキーなし)
                mCommandOpe.mTextString = tbTextString.Text;
                if (mCommandOpe.createData(mLocPos, mOperation)) {
                    disp();
                    mLocPos.Clear();
                }
            } else if (mLocPos.Count == 2 && 
                (mOperation == OPERATION.translate || mOperation == OPERATION.rotate || mOperation == OPERATION.mirror)) {
                if (mCommandOpe.changeData(mLocPos, mPickEnt, mOperation)) {
                    commandClear();
                }
            }
        }


        /// <summary>
        /// ドラッギング処理
        /// </summary>
        /// <param name="points">ロケイト点リスト</param>
        /// <param name="operation">操作</param>
        private void dragging(List<PointD> points, OPERATION operation)
        {
            cvCanvas.Children.Clear();
            imScreen.Source = mBitmapSource;
            cvCanvas.Children.Add(imScreen);
            //ylib.setCanvasBitmapImage(cvCanvas, ylib.bitmapSource2BitmapImage(mBitmapSource),0 , 0, mBitmapSource.Width, mBitmapSource.Height);
            ydraw.mBrush     = mCommandOpe.mCreateColor;
            ydraw.mTextColor = mCommandOpe.mCreateColor;
            ydraw.mThickness = mCommandOpe.mEntSize;
            ydraw.mLineType  = mCommandOpe.mLineType;
            ydraw.mPointType = mCommandOpe.mPointType;
            ydraw.mPointSize = mCommandOpe.mPointSize;
            switch (operation) {
                case OPERATION.createPoint:
                    ydraw.drawWPoint(points[0]);
                    break;
                case OPERATION.createLine:
                    ydraw.drawWLine(points[0], points[1]);
                    break;
                case OPERATION.createRect:
                    Box b = new Box(points[0], points[1]);
                    List<PointD> plist = b.ToPointDList();
                    ydraw.drawWPolygon(plist);
                    break;
                case OPERATION.createPolyline:
                    ydraw.drawWPolyline(points);
                    break;
                case OPERATION.createPolygon:
                    ydraw.drawWPolygon(points, false);
                    break;
                case OPERATION.createArc:
                    if (points.Count == 2) {
                        ydraw.drawWLine(points[0], points[1]);
                    } else if (points.Count == 3) {
                        ArcD arc = new ArcD(points[0], points[1], points[2]);
                        ydraw.drawWArc(arc, false);
                    }
                    break;
                case OPERATION.createCircle:
                    ydraw.drawWCircle(points[0], points[0].length(points[1]));
                    break;
                case OPERATION.createText:
                    TextD text = new TextD(tbTextString.Text, points[0], mCommandOpe.mTextSize, mCommandOpe.mTextRotate, mCommandOpe.mHa, mCommandOpe.mVa);
                    ydraw.drawWText(text);
                    break;
                case OPERATION.translate:
                    translateDragging(points);
                    break;
                case OPERATION.rotate:
                    rotateDragging(points);
                    break;
                default:
                    return;
            }
            // ロケイト点表示
            if (operation != OPERATION.createPoint) {
                ydraw.mPointType = 2;
                ydraw.mPointSize = 3;
                for (int i = 0; i < points.Count; i++) {
                    ydraw.drawWPoint(points[i]);
                }
            }
        }

        /// <summary>
        /// 移動ドラッギング
        /// </summary>
        /// <param name="loc">ロケイト点配列</param>
        private void translateDragging(List<PointD> loc)
        {
            if (loc.Count <  2) return;

            ydraw.mBrush = mDraggingColor;
            PointD vec = loc[0].vector(loc[loc.Count - 1]);

            foreach (int entNo in mPickEnt) {
                Entity ent = mEntityData.mEntityList[entNo];
                //ydraw.mBrush = ent.mColor;
                ydraw.mThickness = ent.mThickness;
                switch (ent.mEntityId) {
                    case EntityId.Point:
                        PointEntity pointEnt = (PointEntity) ent;
                        PointD point = pointEnt.mPoint.toCopy();
                        point.offset(vec);
                        ydraw.mPointType = pointEnt.mType;
                        ydraw.mPointSize = pointEnt.mThickness;
                        ydraw.drawWPoint(point);
                        break;
                    case EntityId.Line:
                        LineEntity lineEnt = (LineEntity) ent;
                        LineD line = lineEnt.mLine.toCopy();
                        line.offset(vec);
                        ydraw.drawWLine(line);
                        break;
                    case EntityId.Arc:
                        ArcEntity arcEnt = (ArcEntity) ent;
                        ArcD arc = arcEnt.mArc.toCopy();
                        arc.mCp.offset(vec);
                        ydraw.drawWArc(arc, false);
                        break;
                    case EntityId.Text:
                        TextEntity textEnt = (TextEntity) ent;
                        TextD text = textEnt.mText.toCopy();
                        text.mPos.offset(vec);
                        ydraw.drawWText(text);
                        break;
                }
            }
        }

        /// <summary>
        /// 回転ドラッギング
        /// </summary>
        /// <param name="loc">ロケイト点配列</param>
        private void rotateDragging(List<PointD> loc)
        {
            if (loc.Count < 2) return;

            ydraw.mBrush = mDraggingColor;

            foreach (int entNo in mPickEnt) {
                Entity ent = mEntityData.mEntityList[entNo];
                //ydraw.mBrush = ent.mColor;
                ydraw.mThickness = ent.mThickness;
                switch (ent.mEntityId) {
                    case EntityId.Point:
                        PointEntity pointEnt = (PointEntity)ent;
                        PointD point = pointEnt.mPoint.toCopy();
                        point.rotate(loc[0], loc[1]);
                        ydraw.mPointType = pointEnt.mType;
                        ydraw.mPointSize = pointEnt.mThickness;
                        ydraw.drawWPoint(point);
                        break;
                    case EntityId.Line:
                        LineEntity lineEnt = (LineEntity)ent;
                        LineD line = lineEnt.mLine.toCopy();
                        line.rotate(loc[0], loc[1]); ;
                        ydraw.drawWLine(line);
                        break;
                    case EntityId.Arc:
                        ArcEntity arcEnt = (ArcEntity)ent;
                        ArcD arc = arcEnt.mArc.toCopy();
                        arc.rotate(loc[0], loc[1]);
                        ydraw.drawWArc(arc, false);
                        break;
                    case EntityId.Text:
                        TextEntity textEnt = (TextEntity)ent;
                        TextD text = textEnt.mText.toCopy();
                        text.rotate(loc[0], loc[1]);
                        ydraw.drawWText(text);
                        break;
                }
            }
        }

        /// <summary>
        /// 要素をピックした時ピック色にして表示
        /// </summary>
        /// <param name="entNo"></param>
        private void pickDisp(List<int> pickList)
        {
            if (pickList.Count == 0)
                return;
            imScreen.Source = mBitmapSource;
            cvCanvas.Children.Clear();
            cvCanvas.Children.Add(imScreen);
            foreach (int entNo in pickList) {
                Entity ent = mEntityData.mEntityList[entNo];
                ent.mPick = true;
                ent.draw(ydraw);
                ent.mPick = false;
            }
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
            Entity ent = mEntityData.mEntityList[entNo];
            switch (ent.mEntityId) {
                case EntityId.Point:
                    PointEntity point = (PointEntity)ent;
                    p = point.mPoint;
                    break;
                case EntityId.Line:
                    LineEntity line = (LineEntity)ent;
                    p = line.mLine.nearPoints(p, 4);
                    break;
                case EntityId.Arc:
                    ArcEntity arc = (ArcEntity)ent;
                    p = arc.mArc.nearPoints(p, 8);
                    break;
                case EntityId.Text:
                    TextEntity text = (TextEntity)ent;
                    p = text.mText.nearPoints(p);
                    break;
            }
            return p;
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
            List<string> menu = new List<string>();
            for (int i = 0; i < picks.Count; i++) {
                Entity ent = mEntityData.mEntityList[picks[i]];
                menu.Add(ent.getSummary());
            }
            MenuDialog dlg = new MenuDialog();
            dlg.mMainWindow = this;
            dlg.mHorizontalAliment = 1;
            dlg.mVerticalAliment = 1;
            dlg.mOneClick = true;
            dlg.mMenuList = menu;
            dlg.ShowDialog();
            return ylib.string2int(dlg.mResultMenu);
        }

        /// <summary>
        /// 描画領域の初期設定
        /// </summary>
        private void initDraw()
        {
            ydraw.setViewArea(0, 0, cvCanvas.ActualWidth, cvCanvas.ActualHeight);
            ydraw.mAspectFix = true;
            ydraw.mClipping = true;
            ydraw.setWorldWindow(mDispArea);
            System.Diagnostics.Debug.WriteLine($"View: {ydraw.mView}");
            System.Diagnostics.Debug.WriteLine($"World: {ydraw.mWorld.ToString("f2")}");
        }

        /// <summary>
        /// データ表示
        /// </summary>
        private void disp()
        {
            ydraw.setBackColor(mBackColor);
            ydraw.clear();
            dispGrid(mGridSize);
            foreach (var entity in mEntityData.mEntityList) {
                entity.draw(ydraw);
            }
            mBitmapSource = canvas2Bitmap(cvCanvas);
            dispAreaInfo();
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
        /// スクロールはビットマップを移動する形で移動によりできた空白部分だけを再描画する
        /// </summary>
        /// <param name="dx">移動量X(screen)</param>
        /// <param name="dy">移動量Y(screen)</param>
        private void scroll(double dx, double dy)
        {

            //  状態を保存
            PointD v = new PointD(ydraw.screen2worldXlength(dx), ydraw.screen2worldYlength(dy));
            ydraw.mWorld.offset(v.inverse());
            Rect tmpView = ydraw.mView;
            Box tmpWorld = ydraw.mWorld.toCopy();

            ydraw.clear();

            //  移動した位置にBitmapの貼付け
            moveImage(mBitmapSource, dx, dy);

            //  横空白部分を描画
            if (0 > dx) {
                ydraw.mView.X = tmpView.Width + dx;
                ydraw.mView.Width = -dx;
                ydraw.mWorld.Left = tmpWorld.Right + v.x;
                ydraw.mWorld.Width = -v.x;
            } else if (0 < dx) {
                ydraw.mView.Width = dx;
                ydraw.mWorld.Width = v.x;
            }
            if (dx != 0) {
                dispGrid(mGridSize);
                foreach (var entity in mEntityData.mEntityList) {
                    entity.draw(ydraw);
                }
            }

            //  縦空白部分を描画
            ydraw.mView = tmpView;
            ydraw.mWorld = tmpWorld.toCopy();
            if (0 > dy) {
                ydraw.mView.Y = tmpView.Height + dy;
                ydraw.mView.Height = -dy; ;
                ydraw.mWorld.Top -= tmpWorld.Height - v.y;
                ydraw.mWorld.Height = v.y;
            } else if (0 < dy) {
                ydraw.mView.Height = dy; ;
                ydraw.mWorld.Height = -v.y;
            }
            if (dy != 0) {
                dispGrid(mGridSize);
                foreach (var entity in mEntityData.mEntityList) {
                    entity.draw(ydraw);
                }
            }

            //  Windowの設定を元に戻す
            ydraw.mView = tmpView;
            ydraw.mWorld = tmpWorld.toCopy();
            mBitmapSource = canvas2Bitmap(cvCanvas);
            dispAreaInfo();

            //  全体再描画
            //PointD v = new PointD(ydraw.screen2worldXlength(dx), ydraw.screen2worldYlength(dy));
            //ydraw.mWorld.offset(v.inverse());
            //disp();
        }

        /// <summary>
        /// Bitmap 図形を移動させる
        /// </summary>
        /// <param name="bitmapSource">Bitmap</param>
        /// <param name="dx">移動量</param>
        /// <param name="dy">移動量う</param>
        private void moveImage(BitmapSource bitmapSource, double dx, double dy)
        {
            System.Drawing.Bitmap bitmap = ylib.cnvBitmapSource2Bitmap(mBitmapSource);
            double width = bitmap.Width - Math.Abs(dx);
            double height = bitmap.Height - Math.Abs(dy);
            Point sp = new Point(dx > 0 ? 0 : -dx, dy > 0 ? 0 : -dy);
            Point ep = new Point(sp.X + width, sp.Y + height);
            System.Drawing.Bitmap moveBitmap = ylib.trimingBitmap(bitmap, sp, ep);
            BitmapImage bitmapImage = ylib.cnvBitmap2BitmapImage(moveBitmap);
            ylib.setCanvasBitmapImage(cvCanvas, bitmapImage, dx > 0 ? dx : 0, dy > 0 ? dy : 0, width, height);
        }

        /// <summary>
        /// 図面全体を表示する
        /// </summary>
        private void dispFit()
        {
            if (mEntityData.mArea != null) {
                mDispArea = mEntityData.mArea.toCopy();
                mDispArea.normalize();
                initDraw();
                disp();
            }
        }

        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        private void cancel()
        {
            mOperation = OPERATION.non;
            mLocPos.Clear();
            disp();
            lbCommand.SelectedIndex = -1;
        }

        /// <summary>
        /// グリッドの表示
        /// グリッド10個おきに大玉を表示
        /// </summary>
        /// <param name="size">グリッドの間隔</param>
        private void dispGrid(double size)
        {
            if (0 < size && size < 1000) {
                ydraw.mBrush = ydraw.getColor("Black");
                ydraw.mThickness = 1.0;
                ydraw.mPointType = 0;
                while (mGridMinmumSize > ydraw.world2screenXlength(size)) {
                    size *= 10;
                }
                if (mGridMinmumSize <= ydraw.world2screenXlength(size)) {
                    //  グリッド間隔(mGridMinmumSize)dot以上を表示
                    double y = ydraw.mWorld.Bottom - size;
                    y = Math.Floor(y / size) * size;
                    while (y < ydraw.mWorld.Top) {
                        double x = ydraw.mWorld.Left;
                        x = Math.Floor(x / size) * size;
                        while (x < ydraw.mWorld.Right) {
                            PointD p = new PointD(x, y);
                            if (x % (size * 10) == 0 && y % (size * 10) == 0) {
                                ydraw.mPointSize = 2;
                                ydraw.drawWPoint(p);
                            } else {
                                ydraw.mPointSize = 1;
                                ydraw.drawWPoint(p);
                            }
                            x += size;
                        }
                        y += size;
                    }
                }
            }
            //  原点(0,0)表示
            ydraw.mBrush = ydraw.getColor("Red");
            ydraw.mPointType = 2;
            ydraw.mPointSize = 5;
            ydraw.drawWPoint(new PointD(0, 0));
        }

        /// <summary>
        /// 表示領域(WorldWindow)の情報表示
        /// </summary>
        private void dispAreaInfo()
        {
            Box worldArea = ydraw.mWorld.toCopy();
            worldArea.normalize();
            tbArea.Text = worldArea.BottomLeft.ToString("f2") + "," + worldArea.TopRight.ToString("f2");
        }

        /// <summary>
        /// コントロールバーのシステム属性を設定する
        /// </summary>
        private void setSystemProperty()
        {
            cbColor.SelectedIndex = ydraw.mColorList.FindIndex(p => p.brush == mCommandOpe.mCreateColor);
            cbGridSize.SelectedIndex = mGridSizeMenu.FindIndex(mCommandOpe.mGridSize);
            cbPointType.SelectedIndex = mCommandOpe.mPointType;
            cbLineType.SelectedIndex = mCommandOpe.mLineType;
            cbEntSize.SelectedIndex = mEntSizeMenu.FindIndex(p => mCommandOpe.mEntSize <= p);
            cbTextSize.SelectedIndex = mTextSizeMenu.FindIndex((p) => mCommandOpe.mTextSize <= p);
        }

        /// <summary>
        /// グリッドのサイズ(座標の丸め)の設定
        /// </summary>
        private void gridSet()
        {
            InputBox dlg = new InputBox();
            dlg.mMainWindow = this;
            dlg.Title = "グリッドのサイズの設定";
            dlg.mEditText = Math.Abs(mGridSize).ToString();
            if (dlg.ShowDialog() == true) {
                mGridSize = ylib.string2double(dlg.mEditText);
                disp();
            }
        }

        /// <summary>
        /// 全データ消去
        /// </summary>
        /// <returns></returns>
        private bool allClear()
        {
            mEntityData.mEntityList.Clear();
            disp();
            return true;
        }

        /// <summary>
        /// CanvasをBitmapに変換
        /// 参照  https://qiita.com/tricogimmick/items/894914f6bbe224a45d49
        /// </summary>
        /// <param name="canvas"></param>
        /// <returns></returns>
        private BitmapSource canvas2Bitmap(Canvas canvas)
        {
            //  CanvasのVisaulOffset値が直接取れないので
            Point preLoc = new Point(lbCommand.ActualWidth, tbToolbarTray.ActualHeight);
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
