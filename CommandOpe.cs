using CoreLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CadApp
{
    /// <summary>
    /// 図面のパラメータ
    /// </summary>
    public class DrawingPara
    {
        public int mPointType = 0;                                  //  点種
        public double mPointSize = 1;                               //  点の大きさ
        public int mLineType = 0;                                   //  線種
        public double mThickness = 1;                               //  線の太さ
        public double mTextSize = 12;                               //  文字サイズ
        public double mTextRotate = 0;                              //  文字列の回転角
        public double mLinePitchRate = 1.2;                         //  文字列の改行幅率
        public HorizontalAlignment mHa = HorizontalAlignment.Left;  //  水平アライメント
        public VerticalAlignment mVa = VerticalAlignment.Top;       //  垂直アライメント
        public string mFontFamily = "";                             //  フォント種別(Yu Gothic UI)
        public FontStyle mFontStyle = FontStyles.Normal;            //  斜体 Normal,Italic
        public FontWeight mFontWeight = FontWeights.Normal;         //  太字 Thin,Normal,Bold

        public double mArrowAngle = Math.PI / 6;                    //  矢印の角度
        public double mArrowSize = 5;                               //  矢印の大きさ
        public Brush mColor = Brushes.Black;                        //  要素の色
        public double mGridSize = 1.0;                              //  マウス座標の丸め値
        public string mComment = "";                                //  図面のコメント
        public ulong mDispLayerBit = 0xffffffff;                    //  表示レイヤービットフィルタ
        public string mCreateLayerName = "BaseLayer";               //  作成レイヤー名
        public bool mOneLayerDisp = false;                          //  1レイヤーのみの表示
        public int mSymbolCategoryIndex = 0;                        //  シンボル分類No
        public Brush mBackColor = Brushes.White;                    //  背景色

        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DrawingPara()
        {
            mFontFamily = SystemFonts.MessageFontFamily.Source;
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns>DrawimgPara</returns>
        public DrawingPara toCopy()
        {
            DrawingPara para = new DrawingPara();
            para.mPointType = mPointType;
            para.mPointSize = mPointSize;
            para.mLineType = mLineType;
            para.mThickness = mThickness;
            para.mTextSize = mTextSize;
            para.mTextRotate = mTextRotate;
            para.mLinePitchRate = mLinePitchRate;
            para.mHa = mHa;
            para.mVa = mVa;
            para.mFontFamily = mFontFamily;
            para.mFontStyle = mFontStyle;
            para.mFontWeight = mFontWeight;
            para.mArrowAngle = mArrowAngle;
            para.mArrowSize = mArrowSize;
            para.mColor = mColor;
            para.mGridSize = mGridSize;
            para.mComment = mComment;
            para.mDispLayerBit = mDispLayerBit;
            para.mCreateLayerName = mCreateLayerName;
            para.mOneLayerDisp = mOneLayerDisp;
            para.mSymbolCategoryIndex = mSymbolCategoryIndex;
            para.mBackColor = mBackColor;
            return para;
        }

        /// <summary>
        /// 初期値に設定
        /// </summary>
        public void init()
        {
            mPointType = 0;                                 //  点種
            mPointSize = 1;                                 //  点の大きさ
            mLineType = 0;                                  //  線種
            mThickness = 1;                                 //  線の太さ
            mTextSize = 12;                                 //  文字サイズ
            mTextRotate = 0;                                //  文字列の回転角
            mLinePitchRate = 1.2;                           //  文字列の改行幅率
            mHa = HorizontalAlignment.Left;                 //  水平アライメント
            mVa = VerticalAlignment.Top;                    //  垂直アライメント
            mFontFamily = SystemFonts.MessageFontFamily.Source; //  フォントファミリ
            mFontStyle = FontStyles.Normal;                 //  斜体
            mFontWeight = FontWeights.Normal;               //  太さ
            mArrowAngle = Math.PI / 6;                      //  矢印の角度
            mArrowSize = 5;                                 //  矢印の大きさ
            mColor = Brushes.Black;                         //  要素の色
            mGridSize = 1.0;                                //  マウス座標の丸め値
            mComment = "";                                  //  図面のコメント
            mDispLayerBit = 0xffffffff;                     //  表示レイヤービットフィルタ
            mCreateLayerName = "BaseLayer";                 //  作成レイヤー名
            mOneLayerDisp = false;                          //  1レイヤーのみの表示
            mSymbolCategoryIndex = 0;                       //  シンボル分類No
            mBackColor = Brushes.White;                     //  背景色
        }

        /// <summary>
        /// パラメータを文字列に変換
        /// </summary>
        /// <returns></returns>
        public string propertyToString()
        {
            return $"Prperty,Color,{ylib.getColorName(mColor)},PointType,{mPointType},PointSize,{mPointSize}," +
                $"LineType,{mLineType},Thickness,{mThickness},TextSize,{mTextSize}," +
                $"TextRotate,{mTextRotate},LinePitchRate,{mLinePitchRate},HA,{mHa},VA,{mVa}," +
                $"ArrowSize,{mArrowSize},ArrowAngle,{mArrowAngle},GridSize,{mGridSize},DispLaerBit,{mDispLayerBit}," +
                $"CreateLayer,{ylib.strControlCodeCnv(mCreateLayerName)},OneLayerDisp,{mOneLayerDisp}," +
                $"FontFamily,{mFontFamily},FontStyle,{mFontStyle},FontWeight,{mFontWeight}," +
                $"SymbolCategoryIindex,{mSymbolCategoryIndex},BackColor,{ylib.getBrushName(mBackColor)}";
        }

        /// <summary>
        /// 文字列配列をプロパティ設定値に変換
        /// </summary>
        /// <param name="data"></param>
        public void setPropertyData(string[] data)
        {
            try {
                if (1 < data.Length && data[0] == "Prperty") {
                    for (int i = 1; i < data.Length; i++) {
                        switch (data[i]) {
                            case "Color":
                                mColor = ylib.getColor(data[++i]);
                                break;
                            case "PointType":
                                mPointType = int.Parse(data[++i]);
                                break;
                            case "PointSize":
                                mPointSize = double.Parse(data[++i]);
                                break;
                            case "LineType":
                                mLineType = int.Parse(data[++i]);
                                break;
                            case "Thickness":
                                mThickness = double.Parse(data[++i]);
                                break;
                            case "TextSize":
                                mTextSize = double.Parse(data[++i]);
                                break;
                            case "TextRotate":
                                mTextRotate = double.Parse(data[++i]);
                                break;
                            case "LinePitchRate":
                                mLinePitchRate = double.Parse(data[++i]);
                                break;
                            case "HA":
                                mHa = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), data[++i]);
                                break;
                            case "VA":
                                mVa = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), data[++i]);
                                break;
                            case "FontFamily":
                                mFontFamily = data[++i];
                                break;
                            case "FontStyle":
                                mFontStyle = ylib.convFontStyle(data[++i]);
                                break;
                            case "FontWeight":
                                mFontWeight = ylib.convFontWeight(data[++i]);
                                break;
                            case "ArrowSize":
                                mArrowSize = double.Parse(data[++i]);
                                break;
                            case "ArrowAngle":
                                mArrowAngle = double.Parse(data[++i]);
                                break;
                            case "GridSize":
                                mGridSize = double.Parse(data[++i]);
                                break;
                            case "DispLaerBit":
                                mDispLayerBit = ulong.Parse(data[++i]);
                                break;
                            case "CreateLayer":
                                mCreateLayerName = ylib.strControlCodeRev(data[++i]);
                                break;
                            case "OneLayerDisp":
                                mOneLayerDisp = bool.Parse(data[++i]);
                                break;
                            case "SymbolCategoryIindex":
                                mSymbolCategoryIndex = int.Parse(data[++i]);
                                break;
                            case "BackColor":
                                mBackColor = ylib.getBrsh(data[++i]);
                                break;
                        }
                    }
                } else {
                    mColor      = ylib.getColor(data[0]);
                    mPointType  = int.Parse(data[1]);
                    mPointSize  = double.Parse(data[2]);
                    mLineType   = int.Parse(data[3]);
                    mThickness  = double.Parse(data[4]);
                    mTextSize   = double.Parse(data[5]);
                    mArrowSize  = double.Parse(data[6]);
                    mArrowAngle = double.Parse(data[7]);
                    mGridSize   = double.Parse(data[8]);
                    if (mArrowAngle == 0)
                        mArrowAngle = 30 * Math.PI / 180;
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                ylib.messageBox(null, e.Message, "SetPropertyData 例外エラー");
            }
        }

        /// <summary>
        /// 図面情報を文字列に変換
        /// </summary>
        /// <returns></returns>
        public string[] commentToString()
        {
            string[] buf = { "Comment", "Comment", ylib.strControlCodeCnv(mComment) };
            return buf;
        }

        /// <summary>
        /// 図面情報のコメントをパラメータに設定
        /// </summary>
        /// <param name="data"></param>
        public void setCommentData(string[] data)
        {
            try {
                if (1 < data.Length && data[0] == "Comment") {
                    for (int i = 1; i < data.Length; i++) {
                        switch (data[i]) {
                            case "Comment":
                                mComment = ylib.strControlCodeRev(data[++i]);
                                break;
                        }
                    }
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
    }

    /// <summary>
    /// コマンド操作
    /// </summary>
    public class CommandOpe
    {
        public EntityData mEntityData;                              //  要素データ
        public ImageData mImageData;

        public string mTextString = "";                             //  文字列データ
        public string mCurFilePath = "";                            //  編集中のファイルパス
        public int mGridMinmumSize = 10;                            //  グリッドの最小表示スクリーンサイズ

        public Box mCopyArea;                                       //  クリップボードにコピーした要素の領域
        public List<Entity> mCopyEntityList;                        //  クリップボードにコピー下要素リスト

        public Box mInitArea = new(-10, 150, 250, -10);             //  初期表示領域
        public Box mDispArea;                                       //  表示領域
        private readonly double mEps = 1E-8;

        private KeyCommand mKeyCommand = new();

        public ChkListDialog mChkListDlg = null;                    //  表示レイヤー設定ダイヤログ
        public SymbolDlg mSymbolDlg = null;                         //  シンボル選択配置ダイヤログ
        public int mSaveOperationCount = 10;                        //  定期保存の操作回数

        public MainWindow mMainWindow;
        public List<string[]> mImageFilters = new List<string[]>() {
                    new string[] { "PNGファイル", "*.png" },
                    new string[] { "JPGファイル", "*.jpg" },
                    new string[] { "JPEGファイル", "*.jpeg" },
                    new string[] { "GIFファイル", "*.gif" },
                    new string[] { "BMPファイル", "*.bmp" },
                    new string[] { "すべてのファイル", "*.*"}
                };
        public string mClipImagePath = "";

        private LocPick mLocPick;
        private WindowState mWinState;
        private YCalc ycalc = new YCalc();
        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="entityData">要素データ</param>
        /// <param name="canvas">Canvas</param>
        public CommandOpe(EntityData entityData, LocPick locPick, MainWindow mainWindow)
        {
            mEntityData = entityData;
            mKeyCommand.mEntityData = mEntityData;
            mDispArea = new Box(mInitArea);
            mLocPick = locPick;
            mMainWindow = mainWindow;
        }

        /// <summary>
        /// 表示領域の設定
        /// </summary>
        /// <param name="dispArea"></param>
        public void setDispArea(Box dispArea)
        {
            mDispArea = dispArea.toCopy();
            mDispArea.normalize();
        }

        /// <summary>
        /// 新規データの作成(要素データなしファイル)
        /// </summary>
        /// <param name="filePath">データファイルパス</param>
        public void newData(string filePath)
        {
            mCurFilePath = filePath;
            mEntityData.clear();
            mEntityData.mArea = mInitArea.toCopy();
            saveFile();
        }

        /// <summary>
        /// コマンド処理
        /// </summary>
        /// <param name="command">コマンド</param>
        public OPEMODE executeCmd(OPERATION operation)
        {
            bool commandInit = true;
            OPEMODE locMode = OPEMODE.pick;

            switch (operation) {
                case OPERATION.createPoint:
                case OPERATION.createLine:
                case OPERATION.createHVLine:
                case OPERATION.createRect:
                case OPERATION.createPolyline:
                case OPERATION.createPolygon:
                case OPERATION.createArc:
                case OPERATION.createCircle:
                case OPERATION.createEllipse:
                case OPERATION.createText:
                case OPERATION.createArrow:
                case OPERATION.createLabel:
                case OPERATION.createLocDimension:
                    locMode = OPEMODE.loc;
                    mLocPick.mPickEnt.Clear();
                    mLocPick.mLocPos.Clear();
                    commandInit = false;
                    break;
                case OPERATION.createTangentCircle:
                case OPERATION.createLinearDimension:
                case OPERATION.createAngleDimension:
                case OPERATION.createDiameterDimension:
                case OPERATION.createRadiusDimension:
                case OPERATION.offset:
                case OPERATION.translate:
                case OPERATION.rotate:
                case OPERATION.mirror:
                case OPERATION.scale:
                case OPERATION.trim:
                case OPERATION.divide:
                case OPERATION.stretch:
                case OPERATION.copyTranslate:
                case OPERATION.copyRotate:
                case OPERATION.copyMirror:
                case OPERATION.copyScale:
                case OPERATION.copyTrim:
                case OPERATION.copyOffset:
                case OPERATION.measureDistance:             //  距離測定
                case OPERATION.measureAngle:                //  角度測定
                    locMode = OPEMODE.loc;
                    mLocPick.mLocPos.Clear();
                    commandInit = false;
                    break;
                case OPERATION.createTangentLine:           //  接線
                    if (mLocPick.mPickEnt.Count ==2) {
                        createTangentLine(mLocPick.mPickEnt);
                    } else {
                        locMode = OPEMODE.loc;
                        mLocPick.mLocPos.Clear();
                        commandInit = false;
                    }
                    break;
                case OPERATION.createSymbol:                //  シンボル配置
                    if (mSymbolDlg == null || !mSymbolDlg.IsActive)
                        setSymbol();
                    break;
                case OPERATION.connect:                     //  接続
                    connect(mLocPick.mPickEnt);
                    break;
                case OPERATION.createImage:                 //  イメージデータの貼付け
                    locMode = setImage();
                    commandInit = false;
                    break;
                case OPERATION.changeText:                  //  文字列変更
                    changeText(mLocPick.mPickEnt);
                    break;
                case OPERATION.changeRadius:                //  半径変更
                    changeRadius(mLocPick.mPickEnt);
                    break;
                case OPERATION.changeProperty:              //  属性変更
                    changeProperty(mLocPick.mPickEnt);
                    break;
                case OPERATION.copyProperty:                //  属性変更+コピー
                    changeProperty(mLocPick.mPickEnt, true);
                    break;
                case OPERATION.changeProperties:            //  属性一括変更
                    changeProperties(mLocPick.mPickEnt);
                    break;
                case OPERATION.copyEntity:                  //  要素コピー
                    entitiesCopy(mLocPick.mPickEnt);
                    break;
                case OPERATION.pasteEntity:                 //  要素貼付け
                    entitiesPaste();
                    locMode = OPEMODE.loc;
                    mLocPick.mLocPos.Clear();
                    commandInit = false;
                    break;
                case OPERATION.symbolAssemble:             //  シンボル化
                    cnvSymbol(mLocPick.mPickEnt);
                    break;
                case OPERATION.disassemble:                 //  分解
                    if (0 < mLocPick.mPickEnt.Count) {
                        mEntityData.mOperationCount++;
                        mEntityData.disassemble(mLocPick.mPickEnt);
                    }
                    break;
                case OPERATION.remove:                      //  削除
                    if (0 < mLocPick.mPickEnt.Count) {
                        mEntityData.mOperationCount++;
                        mEntityData.removeEnt(mLocPick.mPickEnt);
                    }
                    break;
                case OPERATION.removeAll:                   //  全削除
                    break;
                case OPERATION.measure:                     //  距離・角度測定
                    measureDisp(mLocPick.mPickEnt);
                    break;
                case OPERATION.info:                        //  要素情報
                    infoEntity(mLocPick.mPickEnt);
                    break;
                case OPERATION.infoData:                    //  要素データ情報
                    infoEntityData(mLocPick.mPickEnt);
                    break;
                case OPERATION.zumenComment:                //  図面のコメント設定
                    zumenComment();
                    break;
                case OPERATION.zumenInfo:                   //  図面設定
                    if (zumenProperty(mEntityData.mPara))
                        mMainWindow.setZumenProperty();     //  コントロールバーの設定
                    break;
                case OPERATION.createLayer:                 //  作成レイヤー設定
                    setCreateLayer();
                    break;
                case OPERATION.setDispLayer:                //  表示レイヤー設定
                    setDispLayer();
                    break;
                case OPERATION.setAllDispLayer:             //  全レイヤー表示
                    setFulleDispLayer();
                    break;
                case OPERATION.oneLayerDisp:                //  1レイヤー表示
                    setOneLayerDisp(!mEntityData.mPara.mOneLayerDisp);
                    break;
                case OPERATION.changeLayerName:             //  レイヤー名変更
                    changeLayerName();
                    break;
                case OPERATION.setSymbol:                   //  シンボル登録
                    setSymbol(mLocPick.mPickEnt);
                    break;
                case OPERATION.manageSymbol:                //  シンボル管理
                    manageSymbol();
                    break;
                case OPERATION.undo:                        //  アンドゥ
                    mEntityData.undo();
                    mEntityData.updateData();
                    break;
                case OPERATION.redo:                        //  リドゥ
                    break;
                case OPERATION.copyScreen:                  //  製図領域のイメージコピー
                    mMainWindow.screenCopy();
                    break;
                case OPERATION.saveScreen:                  //  製図領域のイメージ保存
                    mMainWindow.screenSave();
                    break;
                case OPERATION.screenCapture:               //  スクリーンキャプチャ
                    screenCapture(mMainWindow, 0);
                    break;
                case OPERATION.imageTrimming:               //  クリップボードのイメージトリミング
                    imageTrimming(mMainWindow);
                    break;
                case OPERATION.print:                       //  印刷
                    mMainWindow.print();
                    break;
                case OPERATION.cancel:                      //  キャンセル
                    mLocPick.mLocPos.Clear();
                    mLocPick.mPickEnt.Clear();
                    break;
                case OPERATION.close:                       //  終了
                    mMainWindow.Close();
                    break;
                case OPERATION.save:                        //  図面データ保存
                    saveFile();
                    break;
                case OPERATION.saveAs:                      //  図面データ保存
                    saveAsFile();
                    break;
                case OPERATION.gridSize:                    //  グリッド設定
                    gridSet();
                    break;
            }
            if (commandInit) {
                mMainWindow.commandClear();
            }
            if (mEntityData.mOperationCount % mSaveOperationCount == 0)
                saveFile();
            return locMode;
        }

        /// <summary>
        /// 要素追加編集コマンド
        /// </summary>
        /// <param name="operation">操作コード</param>
        /// <param name="locPos">ロケイトリスト</param>
        /// <param name="pickEnt">ピックリスト</param>
        /// <param name="lastLoc">ロケイト確定</param>
        /// <returns>実行可否</returns>
        public bool entityCommand(OPERATION operation, List<PointD> locPos, List<(int no, PointD pos)> pickEnt, bool lastLoc)
        {
            try {
                mEntityData.mOperationCount++;
                if (pickEnt.Count == 0) {
                    //  要素の追加 (Ctrlキーなし)
                    if (createData(locPos, operation, lastLoc)) {
                        return true;
                    }
                } else {
                    //  編集コマンド
                    if (changeData(locPos, pickEnt, operation, lastLoc)) {
                        return true;
                    }
                }
            } catch (Exception e) {
                ylib.messageBox(mMainWindow, e.Message, operation.ToString(), "例外エラー");
            }
            return false;
        }

        /// <summary>
        /// 要素作成データ追加
        /// </summary>
        /// <param name="points">ロケイト点配列</param>
        /// <param name="operation">操作</param>
        /// <returns></returns>
        public bool createData(List<PointD> points, OPERATION operation, bool lastLoc = false)
        {
            if (operation == OPERATION.createPoint && points.Count == 1) {
                //  点要素作成
                mEntityData.addPoint(points[0]);
            } else if (operation == OPERATION.createLine && points.Count == 2) {
                //  線分要素作成
                mEntityData.addLine(points[0], points[1]);
            } else if (operation == OPERATION.createRect && points.Count == 2) {
                //  四角形の作成
                mEntityData.addRect(points[0], points[1]);
            } else if (operation == OPERATION.createArc && points.Count == 3) {
                //  円弧要素の作成
                if (0 <  points[0].length(points[1]) && 0 < points[1].length(points[2]))
                    mEntityData.addArc(new ArcD(points[0], points[2], points[1]));
            } else if (operation == OPERATION.createCircle && points.Count == 2) {
                //  円の作成
                mEntityData.addArc(new ArcD(points[0], points[0].length(points[1])));
            } else if (operation == OPERATION.createEllipse && points.Count == 2) {
                //  楕円の作成
                mEntityData.addEllipse(new EllipseD(points[0], points[1]));
            } else if (operation == OPERATION.createText && points.Count == 1) {
                //  テキスト要素の作成
                TextD text = new TextD(mTextString, points[0], mEntityData.mPara.mTextSize, mEntityData.mPara.mTextRotate,
                    mEntityData.mPara.mHa, mEntityData.mPara.mVa, mEntityData.mPara.mLinePitchRate);
                mEntityData.addText(text);
            } else if (operation == OPERATION.createHVLine && lastLoc) {
                //  水平垂直線分
                if (1 < points.Count) {
                    PolylineD polyline = new PolylineD(points);
                    mEntityData.addPolyline(polyline.toHVLine());
                } else {
                    mEntityData.mOperationCount--;
                    return false;
                }
            } else if (operation == OPERATION.createPolyline && lastLoc) {
                //  ポリライン要素の作成
                if (1 < points.Count)
                    mEntityData.addPolyline(points);
                else {
                    mEntityData.mOperationCount--;
                    return false;
                }
            } else if (operation == OPERATION.createPolygon && lastLoc) {
                //  ポリゴンの作成
                if (1 < points.Count)
                    mEntityData.addPolygon(points);
                else {
                    mEntityData.mOperationCount--;
                    return false;
                }
            } else if (operation == OPERATION.createImage && points.Count == 2) {
                //  イメージの貼付け
                createImage(points[0], points[1]);
            } else if (operation == OPERATION.createArrow && points.Count == 2) {
                //  矢印の作成
                mEntityData.addArrow(points[0], points[1]);
            } else if (operation == OPERATION.createLabel && points.Count == 2) {
                //  ラベルの作成
                mEntityData.addLabel(points[0], points[1], mTextString);
            } else if (operation == OPERATION.createLocDimension && points.Count == 3) {
                //  寸法線の作成
                mEntityData.addLocDimension(points[0], points[1], points[2]);
            } else if (operation == OPERATION.pasteEntity && points.Count == 1 && 0 < mCopyEntityList.Count) {
                //  クリップボードの要素を貼り付け
                entityPaste(points[0]);
            } else if (operation == OPERATION.createSymbol && points.Count == 1) {
                //  シンボルの追加
                createSymbol(points[0]);
            } else if (operation == OPERATION.measureDistance && points.Count == 2) {
                //  2点間の距離
                measureDistance(points[0], points[1]);
            } else if (operation == OPERATION.measureAngle && points.Count == 3) {
                //  3点の角度
                measureAngle(points[0], points[1], points[2]);
            } else {
                mEntityData.mOperationCount--;
                return false;
            }
            mEntityData.updateLayerList();
            return true;
        }

        /// <summary>
        /// ピックした要素のデータ更新(移動,回転,ミラー)
        /// </summary>
        /// <param name="loc">ロケイト点リスト</param>
        /// <returns></returns>
        public bool changeData(List<PointD> loc, List<(int, PointD)> pickEnt, OPERATION operation, bool lastLoc = false)
        {
            if (loc.Count == 1) {
                if (operation == OPERATION.divide) {
                    //  分割
                    mEntityData.divide(pickEnt, loc[0]);
                } else if (operation == OPERATION.createTangentLine) {
                    //  接線
                    LineD line = mEntityData.tangentLine(pickEnt[0], loc[0]);
                    if (line != null)
                        mEntityData.addLine(line);
                } else if (operation == OPERATION.createTangentCircle) {
                    //  接円
                    ArcD arc = mEntityData.tangentCircle(pickEnt, loc);
                    if (arc != null)
                        mEntityData.addArc(arc);
                } else if (operation == OPERATION.createLinearDimension) {
                    //  寸法線
                    mEntityData.addDimension(pickEnt, loc[0]);
                } else if (operation == OPERATION.createAngleDimension) {
                    //  角度寸法線
                    mEntityData.addAngleDimension(pickEnt, loc[0]);
                } else if (operation == OPERATION.createDiameterDimension) {
                    //  直径寸法線
                    mEntityData.addDiameterDimension(pickEnt, loc[0]);
                } else if (operation == OPERATION.createRadiusDimension) {
                    //  半径寸法線
                    mEntityData.addRadiusDimension(pickEnt, loc[0]);
                } else {
                    mEntityData.mOperationCount--;
                    return false;
                }
            } else if (2 <= loc.Count && lastLoc) {
                for (int i = 1; i < loc.Count; i++) {
                    if (operation == OPERATION.copyTranslate) {
                        //  コピー移動
                        PointD vec = loc[0].vector(loc[i]);
                        mEntityData.translate(pickEnt, vec, true);
                    } else if (operation == OPERATION.copyRotate) {
                        //  コピー回転
                        mEntityData.rotate(pickEnt, loc[0], loc[i], true);
                    } else if (operation == OPERATION.copyOffset) {
                        //  コピーオフセット
                        mEntityData.offset(pickEnt, loc[0], loc[i], true);
                    } else {
                        mEntityData.mOperationCount--;
                        return false;
                    }
                }
            } else if (loc.Count == 2) {
                if (operation == OPERATION.translate) {
                    //  移動
                    PointD vec = loc[0].vector(loc[1]);
                    mEntityData.translate(pickEnt, vec);
                } else if (operation == OPERATION.rotate) {
                    //  回転
                    mEntityData.rotate(pickEnt, loc[0], loc[1]);
                } else if (operation == OPERATION.mirror) {
                    //  ミラー
                    mEntityData.mirror(pickEnt, loc[0], loc[1]);
                } else if (operation == OPERATION.copyMirror) {
                    //  コピーミラー
                    mEntityData.mirror(pickEnt, loc[0], loc[1], true);
                } else if (operation == OPERATION.offset) {
                    //  オフセット
                    mEntityData.offset(pickEnt, loc[0], loc[1]);
                } else if (operation == OPERATION.trim) {
                    //  トリム
                    mEntityData.trim(pickEnt, loc[0], loc[1]);
                } else if (operation == OPERATION.copyTrim) {
                    //  トリム
                    mEntityData.trim(pickEnt, loc[0], loc[1], true);
                } else if (operation == OPERATION.divide) {
                    //  分割
                    mEntityData.divide(pickEnt, loc[0]);
                } else if (operation == OPERATION.stretch) {
                    //  ストレッチ
                    PointD vec = loc[0].vector(loc[1]);
                    mEntityData.stretch(pickEnt, vec);
                } else if (operation == OPERATION.stretchArc) {
                    //  ストレッチ(円弧)
                    PointD vec = loc[0].vector(loc[1]);
                    mEntityData.stretch(pickEnt, vec, true);
                } else {
                    mEntityData.mOperationCount--;
                    return false;
                }
            } else if (loc.Count == 3 &&
                (operation == OPERATION.scale || operation == OPERATION.copyScale)) {
                if (operation == OPERATION.scale) {
                    //  拡大縮小
                    double scale = loc[0].length(loc[2]) / loc[0].length(loc[1]);
                    mEntityData.scale(pickEnt, loc[0], scale);
                } else if (operation == OPERATION.copyScale) {
                    //  拡大縮小
                    double scale = loc[0].length(loc[2]) / loc[0].length(loc[1]);
                    mEntityData.scale(pickEnt, loc[0], scale, true);
                } else {
                    mEntityData.mOperationCount--;
                    return false;
                }
            } else {
                mEntityData.mOperationCount--;
                return false;
            }
            mEntityData.updateLayerList();
            return true;
        }

        /// <summary>
        /// キー入力によるコマンド処理
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <returns></returns>
        public bool keyCommand(string command, string text)
        {
            mEntityData.mOperationCount++;
            mKeyCommand.mTextString = text;
            return mKeyCommand.setCommand(command, mEntityData.mPara);
        }

        /// <summary>
        /// ピックしたテキスト要素の文字列を変更する
        /// </summary>
        /// <param name="pickEnt">ピック要素リスト</param>
        /// <returns></returns>
        public bool changeText(List<(int, PointD)> pickEnt)
        {
            mEntityData.mOperationCount++;
            int changeCount = 0;
            foreach ((int no, PointD pos) pickNo in pickEnt) {
                InputBox dlg = new InputBox();
                dlg.Owner = mMainWindow;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlg.mMultiLine = true;
                dlg.mWindowSizeOutSet = true;
                dlg.Title = "文字列変更";
                if (mEntityData.mEntityList[pickNo.no].mRemove)
                    continue;
                if (mEntityData.mEntityList[pickNo.no].mEntityId == EntityId.Text) {
                    //  文字列要素
                    TextEntity text = (TextEntity)mEntityData.mEntityList[pickNo.no];
                    dlg.mEditText = text.mText.mText;
                    if (dlg.ShowDialog() == true) {
                        if (0 < dlg.mEditText.Length) {
                            mEntityData.mEntityList.Add(mEntityData.mEntityList[pickNo.no].toCopy());
                            text = (TextEntity)mEntityData.mEntityList[mEntityData.mEntityList.Count - 1];
                            text.mText.mText = dlg.mEditText;
                            mEntityData.mEntityList[mEntityData.mEntityList.Count - 1].mOperationCount = mEntityData.mOperationCount;
                            mEntityData.removeEnt(pickNo.no);
                        }
                        changeCount++;
                    }
                } else if (mEntityData.mEntityList[pickNo.no].mEntityId == EntityId.Parts) {
                    //  パーツ要素
                    PartsEntity parts = (PartsEntity)mEntityData.mEntityList[pickNo.no];
                    if (0 < parts.mParts.mTexts.Count) {
                        (int no, dlg.mEditText) = parts.mParts.getPickText(pickNo.pos);
                        if (0 <= no && dlg.ShowDialog() == true) {
                            if (0 < dlg.mEditText.Length) {
                                mEntityData.mEntityList.Add(mEntityData.mEntityList[pickNo.no].toCopy());
                                parts = (PartsEntity)mEntityData.mEntityList[mEntityData.mEntityList.Count - 1];
                                parts.mParts.mTexts[no].mText = dlg.mEditText;
                                mEntityData.mEntityList[mEntityData.mEntityList.Count - 1].mOperationCount = mEntityData.mOperationCount;
                                mEntityData.removeEnt(pickNo.no);
                            }
                            changeCount++;
                        }
                    }
                }
            }
            mEntityData.updateData();
            if (0 < changeCount)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 円弧の半径を数値入力で変更
        /// </summary>
        /// <param name="pickEnt">ピック要素リスト</param>
        /// <returns></returns>
        public bool changeRadius(List<(int, PointD)> pickEnt)
        {
            mEntityData.mOperationCount++;
            foreach ((int no, PointD pos) pickNo in pickEnt) {
                if (mEntityData.mEntityList[pickNo.no].mEntityId == EntityId.Arc) {
                    ArcEntity arc = (ArcEntity)mEntityData.mEntityList[pickNo.no];
                    InputBox dlg = new InputBox();
                    dlg.Owner = mMainWindow;
                    dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    dlg.Title = "半径変更";
                    dlg.mEditText = arc.mArc.mR.ToString();
                    if (dlg.ShowDialog() == true) {
                        mEntityData.mEntityList.Add(mEntityData.mEntityList[pickNo.no].toCopy());
                        arc = (ArcEntity)mEntityData.mEntityList[mEntityData.mEntityList.Count - 1];
                        arc.mArc.mR = ycalc.expression(dlg.mEditText);
                        mEntityData.mEntityList[mEntityData.mEntityList.Count - 1].mOperationCount = mEntityData.mOperationCount;
                        mEntityData.removeEnt(pickNo.no);
                    }
                }
            }
            mEntityData.updateData();
            mMainWindow.cbCreateLayer.ItemsSource = mEntityData.getLayerNameList();
            mMainWindow.cbCreateLayer.SelectedIndex = mMainWindow.cbCreateLayer.Items.IndexOf(mEntityData.mPara.mCreateLayerName);
            return true;
        }

        /// <summary>
        /// ピックした要素の属性変更
        /// </summary>
        /// <returns></returns>
        public bool changeProperty(List<(int, PointD)> pickEnt, bool copy = false )
        {
            mEntityData.mOperationCount++;
            foreach ((int no, PointD pos) pickNo in pickEnt) {
                EntProperty dlg = new EntProperty();
                dlg.Owner = mMainWindow;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                //  共通属性
                mEntityData.updateLayerList();
                dlg.mLayerNameList = mEntityData.getLayerNameList();
                Entity entity = mEntityData.mEntityList[pickNo.no];
                dlg.mEntityId  = entity.mEntityId;
                dlg.mColor     = entity.mColor;
                dlg.mLineType  = entity.mType;
                dlg.mThickness = entity.mThickness;
                dlg.mLayerName = entity.mLayerName;
                dlg.mBackDisp  = entity.mBackDisp;
                dlg.Title = entity.mEntityId + "要素属性";
                //  Text要素
                if (entity.mEntityId == EntityId.Text) {
                    TextEntity text = (TextEntity)entity;
                    dlg.mTextSize   = text.mText.mTextSize;
                    dlg.mHa         = text.mText.mHa;
                    dlg.mVa         = text.mText.mVa;
                    dlg.mTextRotate = text.mText.mRotate;
                    dlg.mLinePitchRate = text.mText.mLinePitchRate;
                    dlg.mFontFamily = text.mText.mFontFamily;
                    dlg.mFontStyle  = text.mText.mFontStyle.ToString();
                    dlg.mFontWeight = text.mText.mFontWeight.ToString();
                }
                //  Parts要素
                if (entity.mEntityId == EntityId.Parts) {
                    PartsEntity parts = (PartsEntity)entity;
                    dlg.mTextSize      = parts.mParts.mTextSize;
                    dlg.mTextRotate    = parts.mParts.mTextRotate;
                    dlg.mLinePitchRate = parts.mParts.mLinePitchRate;
                    dlg.mArrowSize     = parts.mParts.mArrowSize;
                    dlg.mArrowAngle    = parts.mParts.mArrowAngle;
                    dlg.mPartsName     = parts.mParts.mName;
                    dlg.mFontFamily    = parts.mParts.mFontFamily;
                    dlg.mFontStyle     = parts.mParts.mFontStyle.ToString();
                    dlg.mFontWeight    = parts.mParts.mFontWeight.ToString();
                }
                //  Image要素
                if (entity.mEntityId == EntityId.Image) {
                    ImageEntity image = (ImageEntity)entity;
                    if (!System.IO.File.Exists(image.mImagePath) &&
                        !File.Exists(image.getCashPath())) {
                        string filePath = ylib.fileOpenSelectDlg("イメージファイルの選択",
                            System.IO.Path.GetDirectoryName(image.mImagePath), mImageFilters);
                        if (0 < filePath.Length) {
                            image.fileUpdate(filePath);
                        }
                    }
                }
                //  Polygon要素
                if (entity.mEntityId == EntityId.Polygon) {
                    PolygonEntity polygon = (PolygonEntity)entity;
                    dlg.mFillOn    = polygon.mFillOn;
                    dlg.mFillColor = polygon.mFillColor;
                    dlg.mFillEntity = true;
                }
                //  Arc要素
                if (entity.mEntityId == EntityId.Arc) {
                    ArcEntity arc = (ArcEntity)entity;
                    dlg.mFillOn = arc.mFillOn;
                    dlg.mFillColor = arc.mFillColor;
                    dlg.mFillEntity = true;
                }
                //  Ellipse要素
                if (entity.mEntityId == EntityId.Ellipse) {
                    EllipseEntity ellipse = (EllipseEntity)entity;
                    dlg.mFillOn = ellipse.mFillOn;
                    dlg.mFillColor = ellipse.mFillColor;
                    dlg.mFillEntity = true;
                }
                if (dlg.ShowDialog() == true) {
                    //  共通属性
                    mEntityData.mEntityList.Add(mEntityData.mEntityList[pickNo.no].toCopy());
                    entity = mEntityData.mEntityList[mEntityData.mEntityList.Count - 1];
                    entity.mColor = dlg.mColor;
                    entity.mType = dlg.mLineType;
                    entity.mThickness = dlg.mThickness;
                    entity.mLayerName = dlg.mLayerName;
                    entity.mLayerBit = mEntityData.setLayerBit(entity.mLayerName);
                    entity.mBackDisp = dlg.mBackDisp;
                    //  Text要素
                    if (entity.mEntityId == EntityId.Text) {
                        TextEntity text = (TextEntity)entity;
                        text.mText.mTextSize      = dlg.mTextSize;
                        text.mText.mHa            = dlg.mHa;
                        text.mText.mVa            = dlg.mVa;
                        text.mText.mRotate        = dlg.mTextRotate;
                        text.mText.mLinePitchRate = dlg.mLinePitchRate;
                        text.mText.mFontFamily    = dlg.mFontFamily;
                        text.mText.mFontStyle     = ylib.convFontStyle(dlg.mFontStyle);
                        text.mText.mFontWeight    = ylib.convFontWeight(dlg.mFontWeight);
                    }
                    //  Parts要素
                    if (entity.mEntityId == EntityId.Parts) {
                        PartsEntity parts = (PartsEntity)entity;
                        //parts.mParts.mRefValue = new List<double>() { 6, Math.PI / 6, 12, 1.2, 0 };
                        parts.mParts.mArrowSize     = dlg.mArrowSize;
                        parts.mParts.mArrowAngle    = dlg.mArrowAngle;
                        parts.mParts.mTextSize      = dlg.mTextSize;
                        parts.mParts.mLinePitchRate = dlg.mLinePitchRate;
                        parts.mParts.mTextRotate    = dlg.mTextRotate;
                        parts.mParts.mName          = dlg.mPartsName;
                        parts.mParts.mFontFamily    = dlg.mFontFamily;
                        parts.mParts.mFontStyle     = ylib.convFontStyle(dlg.mFontStyle);
                        parts.mParts.mFontWeight    = ylib.convFontWeight(dlg.mFontWeight);
                        parts.mParts.remakeData();
                    }
                    //  Polygon要素
                    if (entity.mEntityId == EntityId.Polygon) {
                        PolygonEntity polygon = (PolygonEntity)entity;
                        polygon.mFillOn    = dlg.mFillOn;
                        polygon.mFillColor = dlg.mFillColor;
                    }
                    //  Arc要素
                    if (entity.mEntityId == EntityId.Arc) {
                        ArcEntity arc = (ArcEntity)entity;
                        arc.mFillOn = dlg.mFillOn;
                        arc.mFillColor = dlg.mFillColor;
                    }
                    //  Ellipse要素
                    if (entity.mEntityId == EntityId.Ellipse) {
                        EllipseEntity ellipse = (EllipseEntity)entity;
                        ellipse.mFillOn = dlg.mFillOn;
                        ellipse.mFillColor = dlg.mFillColor;
                    }
                    //  Undo処理
                    entity.mOperationCount = mEntityData.mOperationCount;
                    if (!copy)
                        mEntityData.removeEnt(pickNo.no);
                }
                dlg.Close();
            }
            if (mChkListDlg != null && mChkListDlg.IsVisible)
                setDispLayer();
            mEntityData.updateData();
            mMainWindow.cbCreateLayer.ItemsSource = mEntityData.getLayerNameList();
            mMainWindow.cbCreateLayer.SelectedIndex = mMainWindow.cbCreateLayer.Items.IndexOf(mEntityData.mPara.mCreateLayerName);
            return true;
        }

        /// <summary>
        /// ピックした要素の属性一括変更
        /// </summary>
        /// <param name="pickEnt">ピック要素リスト</param>
        /// <returns></returns>
        public bool changeProperties(List<(int, PointD)> pickEnt)
        {
            SysProperty dlg = new SysProperty();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "属性一括変更";
            dlg.mShowCheckBox = true;
            mEntityData.updateLayerList();
            dlg.mLayerNameList = mEntityData.getLayerNameList();
            if (dlg.ShowDialog() == true) {
                mEntityData.mOperationCount++;
                foreach ((int no, PointD pos) pickNo in pickEnt) {
                    mEntityData.mEntityList.Add(mEntityData.mEntityList[pickNo.no].toCopy());
                    Entity entity = mEntityData.mEntityList[mEntityData.mEntityList.Count - 1];
                    //  共通属性
                    if (dlg.mColorChk)
                       entity.mColor = dlg.mColor;
                    if (dlg.mLineTypeChk && entity.mEntityId != EntityId.Point)
                        entity.mType = dlg.mLineType;
                    if (dlg.mThicknessChk && entity.mEntityId != EntityId.Point)
                        entity.mThickness = dlg.mThickness;
                    if (dlg.mPointTypeChk && entity.mEntityId == EntityId.Point)
                        entity.mType = dlg.mPointType;
                    if (dlg.mPointSizeChk && entity.mEntityId == EntityId.Point)
                        entity.mThickness = dlg.mPointSize;
                    if (dlg.mLayerNameChk) {
                        entity.mLayerName = dlg.mLayerName;
                        entity.mLayerBit = mEntityData.setLayerBit(entity.mLayerName);
                    }
                    if (dlg.mBackDispChk)
                        entity.mBackDisp = dlg.mBackDisp;
                    //  テキスト要素
                    if (entity.mEntityId == EntityId.Text) {
                        TextEntity textEnt = (TextEntity)entity;
                        if (dlg.mTextSizeChk)
                            textEnt.mText.mTextSize = dlg.mTextSize;
                        if (dlg.mHaChk)
                            textEnt.mText.mHa = dlg.mHa;
                        if (dlg.mVaChk)
                            textEnt.mText.mVa = dlg.mVa;
                        if (dlg.mTextRotateChk)
                            textEnt.mText.mRotate = dlg.mTextRotate;
                        if (dlg.mLinePitchRateChk)
                            textEnt.mText.mLinePitchRate = dlg.mLinePitchRate;
                        if (dlg.mFontFamilyChk)
                            textEnt.mText.mFontFamily = dlg.mFontFamily;
                        if (dlg.mFontStyleChk)
                            textEnt.mText.mFontStyle = ylib.convFontStyle(dlg.mFontStyle);
                        if (dlg.mFontWeightChk)
                            textEnt.mText.mFontWeight = ylib.convFontWeight(dlg.mFontWeight);
                    }
                    //  パーツ要素
                    if (entity.mEntityId == EntityId.Parts) {
                        PartsEntity partsEnt = (PartsEntity)entity;
                        if (dlg.mArrowSizeChk)
                            partsEnt.mParts.mArrowSize = dlg.mArrowSize;
                        if (dlg.mArrowAngleChk)
                            partsEnt.mParts.mArrowAngle = dlg.mArrowAngle;
                        if (dlg.mTextSizeChk)
                            partsEnt.mParts.mTextSize = dlg.mTextSize;
                        if (dlg.mTextRotateChk)
                            partsEnt.mParts.mTextRotate = dlg.mTextRotate;
                        if (dlg.mLinePitchRateChk)
                            partsEnt.mParts.mLinePitchRate = dlg.mLinePitchRate;
                        if (dlg.mFontFamilyChk)
                            partsEnt.mParts.mFontFamily = dlg.mFontFamily;
                        if (dlg.mFontStyleChk)
                            partsEnt.mParts.mFontStyle = ylib.convFontStyle(dlg.mFontStyle);
                        if (dlg.mFontWeightChk)
                            partsEnt.mParts.mFontWeight = ylib.convFontWeight(dlg.mFontWeight);
                        partsEnt.mParts.remakeData();
                    }
                    //  Polygon要素
                    if (entity.mEntityId == EntityId.Polygon && dlg.mFillColorChk) {
                        PolygonEntity polygon = (PolygonEntity)entity;
                        polygon.mFillOn = dlg.mFillColorOn;
                        polygon.mFillColor = dlg.mFillColor;
                    }
                    //  Arc要素
                    if (entity.mEntityId == EntityId.Arc && dlg.mFillColorChk) {
                        ArcEntity arc = (ArcEntity)entity;
                        arc.mFillOn = dlg.mFillColorOn;
                        arc.mFillColor = dlg.mFillColor;
                    }
                    //  Ellipse要素
                    if (entity.mEntityId == EntityId.Ellipse && dlg.mFillColorChk) {
                        EllipseEntity ellipse = (EllipseEntity)entity;
                        ellipse.mFillOn = dlg.mFillColorOn;
                        ellipse.mFillColor = dlg.mFillColor;
                    }
                    entity.mOperationCount = mEntityData.mOperationCount;
                    mEntityData.removeEnt(pickNo.no);
                }
                if (dlg.mLayerNameChk) {
                    if (mChkListDlg != null && mChkListDlg.IsVisible)
                        setDispLayer();
                }
            }
            mEntityData.updateData();
            mMainWindow.cbCreateLayer.ItemsSource = mEntityData.getLayerNameList();
            mMainWindow.cbCreateLayer.SelectedIndex = mMainWindow.cbCreateLayer.Items.IndexOf(mEntityData.mPara.mCreateLayerName);
            return true;
        }

        /// <summary>
        /// 要素同士の接続
        /// </summary>
        /// <param name="pickEnt"></param>
        public void connect(List<(int no, PointD pos)> pickEnt)
        {
            if (pickEnt.Count == 0)
                return;
            Entity entity = mEntityData.mEntityList[pickEnt[0].no];
            if (entity.mEntityId != EntityId.Line &&
                entity.mEntityId != EntityId.Arc &&
                entity.mEntityId != EntityId.Polyline)
                return;
            List<PointD> plist = entity.toPointList();
            mEntityData.mOperationCount++;
            if (pickEnt.Count == 1) {
                //  ポリゴンに変換
                mEntityData.addPolygon(plist);
                mEntityData.mEntityList[^1].setProperty(entity);
            } else if (pickEnt.Count == 2) {
                //  ピック位置に近い端点を接続
                PolylineD polyline = new PolylineD(plist);
                Entity ent2 = mEntityData.mEntityList[pickEnt[1].no];
                polyline.connect(pickEnt[0].pos, ent2.toPointList(), pickEnt[1].pos);
                mEntityData.addPolyline(polyline.mPolyline);
                mEntityData.mEntityList[^1].setProperty(entity);
            } else {
                //  近い端点同士を接続
                PolylineD polyline = new PolylineD(plist);
                for (int i = 1; i < pickEnt.Count; i++) {
                    Entity ent2 = mEntityData.mEntityList[pickEnt[i].no];
                    polyline.connect(ent2.toPointList());
                }
                mEntityData.addPolyline(polyline.mPolyline);
                mEntityData.mEntityList[^1].setProperty(entity);
            }
            for (int i = 0; i < pickEnt.Count;i++)
                mEntityData.removeEnt(pickEnt[i].no);
            mEntityData.updateData();
        }

        /// <summary>
        /// ピックした要素をクリップボードにコピーする
        /// </summary>
        /// <param name="pickEnt"></param>
        public void entitiesCopy(List<(int no, PointD pos)> pickEnt)
        {
            if (pickEnt.Count == 0)
                return;
            List<string[]> listData = new List<string[]>();
            Box area = mEntityData.mEntityList[pickEnt[0].no].mArea.toCopy();
            BitmapSource bitmap = null;
            foreach ((int no, PointD pos) pickNo in pickEnt) {
                Entity entity = mEntityData.mEntityList[pickNo.no];
                if (entity != null && !entity.mRemove && entity.mEntityId != EntityId.Non) {
                    listData.Add(entity.toList().ToArray());
                    listData.Add(entity.toDataList().ToArray());
                    area.extension(entity.mArea);
                }
                if (bitmap == null && entity != null && entity.mEntityId == EntityId.Image) {
                    ImageEntity ent = (ImageEntity)entity;
                    bitmap = ylib.cnvBitmap2BitmapImage(ent.mBitmap);
                }
            }
            string buf = mMainWindow.mAppName + "\n";
            buf += "area," + area.ToString() +"\n";
            foreach (string[] str in listData) {
                buf += ylib.arrayStr2CsvData(str) + "\n";
            }
            //  Clipboardにコピー
            DataObject data = new DataObject(DataFormats.Text, buf);
            if (bitmap != null)
                data.SetData(DataFormats.Bitmap, bitmap);
            Clipboard.SetDataObject(data, true);
        }

        /// <summary>
        /// クリップボードにコピーされた要素を取得する
        /// </summary>
        public void entitiesPaste()
        {
            string buf = Clipboard.GetText();
            mCopyEntityList = new List<Entity>();
            if (0 < buf.Length) {
                string[] dataList = buf.Split(new char[] { '\n' });
                if (1 < dataList.Length) {
                    string[] appNameStr = ylib.csvData2ArrayStr(dataList[0]);
                    if (appNameStr[0] != mMainWindow.mAppName)
                        return;
                    string[] areaStr = ylib.csvData2ArrayStr(dataList[1]);
                    if (4 < areaStr.Length && areaStr[0] == "area") {
                        mCopyArea = new Box($"{areaStr[1]},{areaStr[2]},{areaStr[3]},{areaStr[4]}");
                        mCopyArea.normalize();
                    }
                    for (int i = 2; i < dataList.Length -1; i++) {
                        string[] property = ylib.csvData2ArrayStr(dataList[i]);
                        if (property.Length <= 4)
                            continue;
                        string[] data = ylib.csvData2ArrayStr(dataList[++i]);
                        Entity ent = mEntityData.setStringEntityData(property, data);
                        ent.mLayerName = mEntityData.mPara.mCreateLayerName;
                        if (ent != null)
                            mCopyEntityList.Add(ent);
                    }
                }
            }
        }

        /// <summary>
        /// クリップボードにコピーされたデータを貼り付ける
        /// </summary>
        /// <param name="loc">貼り付け位置</param>
        public void entityPaste(PointD loc)
        {
            if (mCopyArea == null)
                return;
            PointD vec = loc - mCopyArea.Location;
            for (int i = 0; i < mCopyEntityList.Count; i++) {
                Entity entity = mCopyEntityList[i];
                entity.translate(vec);
                entity.mOperationCount = mEntityData.mOperationCount;
                mEntityData.addEntity(entity);
            }
            mEntityData.updateData();
        }

        /// <summary>
        /// 接線を作成
        /// </summary>
        /// <param name="pickEnt"></param>
        public void createTangentLine(List<(int no, PointD pos)> pickEnt)
        {
            LineD line = mEntityData.tangentLine(pickEnt);
            if (line != null) {
                mEntityData.mOperationCount++;
                mEntityData.addLine(line);
                mEntityData.updateData();
            }
        }

        /// <summary>
        /// 選択したシンボルの配置
        /// </summary>
        /// <param name="loc">配置位置</param>
        public void createSymbol(PointD loc)
        {
            if (mCopyEntityList != null && 0 < mCopyEntityList.Count) {
                PointD vec = loc - mCopyEntityList[0].mArea.getCenter();
                Entity entity = mCopyEntityList[0].toCopy();
                entity.translate(vec);
                entity.mOperationCount = mEntityData.mOperationCount;
                mEntityData.addEntity(entity);
                mEntityData.updateData();
            }
        }

        /// <summary>
        /// シンボルの選択コピー
        /// </summary>
        public void selectSymbol()
        {
            SymbolDlg dlg = new SymbolDlg();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "シンボル選択";
            dlg.mSymbolFolder = mMainWindow.mSymbolData.mSymbolFolder;
            dlg.mDefualtCategory = mEntityData.mPara.mSymbolCategoryIndex;
            if (dlg.ShowDialog() == true) {
                Entity ent = dlg.mEntity;
                ent.setProperty(mEntityData.mPara);
                mCopyEntityList = new List<Entity>();
                if (ent != null)
                    mCopyEntityList.Add(ent);
                mEntityData.mPara.mSymbolCategoryIndex = dlg.mDefualtCategory;
            }
        }

        /// <summary>
        /// シンボル選択のモードレスダイヤログ
        /// </summary>
        public void setSymbol()
        {
            if (mSymbolDlg != null)
                mSymbolDlg.Close();
            mSymbolDlg = new SymbolDlg();
            mSymbolDlg.Topmost = true;
            mSymbolDlg.Title = "シンボル選択";
            mSymbolDlg.mSymbolFolder = mMainWindow.mSymbolData.mSymbolFolder;
            mSymbolDlg.mDefualtCategory = mEntityData.mPara.mSymbolCategoryIndex;
            mSymbolDlg.mCallBackOn = true;
            mSymbolDlg.callback = setSymbolData;
            mSymbolDlg.Show();
        }

        /// <summary>
        /// シンボル配置の実行関数(SymbolDlgから呼ばれる)
        /// </summary>
        public void setSymbolData()
        {
            Entity ent = mSymbolDlg.mEntity;
            ent.setProperty(mEntityData.mPara);
            mCopyEntityList = new List<Entity>();
            if (ent != null)
                mCopyEntityList.Add(ent);
            mEntityData.mPara.mSymbolCategoryIndex = mSymbolDlg.mDefualtCategory;
            mMainWindow.mOperation = OPERATION.createSymbol;
            mMainWindow.mLocMode = OPEMODE.loc;
            mLocPick.mLocPos.Clear();
        }

        /// <summary>
        /// イメージファイルの貼付け
        /// </summary>
        /// <param name="sp">貼付け位置始点</param>
        /// <param name="ep">貼付け位置終点</param>
        public void createImage(PointD sp, PointD ep)
        {
            if (mCopyEntityList != null && 0 < mCopyEntityList.Count) {
                ImageEntity entity = (ImageEntity)mCopyEntityList[0].toCopy();
                entity.setPostion(sp, ep);
                entity.mOperationCount = mEntityData.mOperationCount;
                mEntityData.addEntity(entity);
                mEntityData.updateData();
            }
        }


        /// <summary>
        /// イメージ要素を設定
        /// </summary>
        public OPEMODE setImage()
        {
            string filePath = "";
            if (0 < mClipImagePath.Length) {
                if (ylib.messageBox(mMainWindow, "キャプチャしたイメージを貼り付けますか", 
                    "", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                    filePath = mClipImagePath;
                }
            } else if (Clipboard.ContainsImage()) {
                if (ylib.messageBox(mMainWindow, "クリップボードのイメージを貼り付けますか",
                    "", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                    BitmapSource bitmapSource = Clipboard.GetImage();
                    filePath = mImageData.saveBitmapCash(ylib.cnvBitmapSource2Bitmap(bitmapSource));
                }
            }
            if (filePath.Length == 0)
                filePath = ylib.fileOpenSelectDlg("イメージファイルの選択", ".", mImageFilters);
            if (File.Exists(filePath)) {
                Entity ent = new ImageEntity(mImageData, filePath);
                ent.setProperty(mEntityData.mPara);
                mCopyEntityList = new List<Entity>();
                if (ent != null)
                    mCopyEntityList.Add(ent);
                mLocPick.mLocPos.Clear();
                mClipImagePath = "";
                return OPEMODE.loc;
            }
            return OPEMODE.non;
        }

        /// <summary>
        /// ピックした要素をシンボルに変換する
        /// </summary>
        /// <param name="pickEnt"></param>
        public void cnvSymbol(List<(int no, PointD pos)> pickEnt)
        {
            List<PointD> points = new List<PointD>();
            List<LineD> lines = new List<LineD>();
            List<ArcD> arcs = new List<ArcD>();
            List<TextD> texts = new List<TextD>();
            for (int i = 0; i < pickEnt.Count; i++) {
                int no = pickEnt[i].no;
                if (mEntityData.mEntityList[no].mEntityId == EntityId.Point) {
                    PointEntity PointEnt = (PointEntity)mEntityData.mEntityList[no];
                    points.Add(PointEnt.mPoint);
                } else if (mEntityData.mEntityList[no].mEntityId == EntityId.Line) {
                    LineEntity lineEnt = (LineEntity)mEntityData.mEntityList[no];
                    lines.Add(lineEnt.mLine);
                } else if (mEntityData.mEntityList[no].mEntityId == EntityId.Arc) {
                    ArcEntity arcEnt = (ArcEntity)mEntityData.mEntityList[no];
                    arcs.Add(arcEnt.mArc);
                } else if (mEntityData.mEntityList[no].mEntityId == EntityId.Polyline) {
                    PolylineEntity polylineEnt = (PolylineEntity)mEntityData.mEntityList[no];
                    lines.AddRange(polylineEnt.mPolyline.toLineList(true));
                    arcs.AddRange(polylineEnt.mPolyline.toArcList());
                } else if (mEntityData.mEntityList[no].mEntityId == EntityId.Polygon) {
                    PolygonEntity polygonEnt = (PolygonEntity)mEntityData.mEntityList[no];
                    lines.AddRange(polygonEnt.mPolygon.toLineList(true));
                    arcs.AddRange(polygonEnt.mPolygon.toArcList());
                } else if (mEntityData.mEntityList[no].mEntityId == EntityId.Text) {
                    TextEntity textEnt = (TextEntity)mEntityData.mEntityList[no];
                    texts.Add(textEnt.mText);
                } else if (mEntityData.mEntityList[no].mEntityId == EntityId.Parts) {
                    PartsEntity partsEnt = (PartsEntity)mEntityData.mEntityList[no];
                    if (0 < partsEnt.mParts.mLines.Count)
                        lines.AddRange(partsEnt.mParts.mLines);
                    if (0 < partsEnt.mParts.mArcs.Count)
                        arcs.AddRange(partsEnt.mParts.mArcs);
                    if (0 < partsEnt.mParts.mTexts.Count)
                        texts.AddRange(partsEnt.mParts.mTexts);
                }
            }
            if (0 == lines.Count && 0 == arcs.Count && 0 == texts.Count)
                return;
            InputBox dlg = new InputBox();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "シンボルの選択";
            if (dlg.ShowDialog() == true) {
                mEntityData.mOperationCount++;
                string name = "__" + dlg.mEditText;
                PartsEntity entity = new PartsEntity(name, points, lines, arcs, texts);
                entity.setProperty(mEntityData.mPara);
                mEntityData.mEntityList.Add(entity);
                entity.mOperationCount = mEntityData.mOperationCount;
                for (int i = 0; i < pickEnt.Count; i++)
                    mEntityData.removeEnt(pickEnt[i].no);
                mEntityData.updateData();
            }
        }

        /// <summary>
        /// シンボルデータを登録する
        /// </summary>
        /// <param name="pickEnt">ピック要素</param>
        public void setSymbol(List<(int no, PointD pos)> pickEnt)
        {
            for (int i = 0; i < pickEnt.Count; i++) {
                Entity ent = mEntityData.mEntityList[pickEnt[i].no];
                if (ent.mEntityId == EntityId.Parts) {
                    PartsEntity parts = (PartsEntity)ent;
                    if (2 < parts.mParts.mName.Length && parts.mParts.mName.Substring(0,2) == "__") {
                        InputSelect dlg = new InputSelect();
                        dlg.Owner = mMainWindow;
                        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        dlg.Title = "シンボル登録";
                        dlg.mTextList = mMainWindow.mSymbolData.getCategoryList();
                        dlg.mListIndex = mEntityData.mPara.mSymbolCategoryIndex;
                        if (0 < dlg.mTextList.Count)
                            dlg.mText = dlg.mTextList[0];
                        if (dlg.ShowDialog() == true) {
                            if (0 < dlg.mText.Length)
                                mMainWindow.mSymbolData.registSymbol(dlg.mText, parts);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// シンボル管理
        /// </summary>
        public void manageSymbol()
        {
            SymbolDlg dlg = new SymbolDlg();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "シンボル管理";
            dlg.mSymbolFolder = mMainWindow.mSymbolData.mSymbolFolder;
            dlg.mDefualtCategory = mEntityData.mPara.mSymbolCategoryIndex;
            dlg.mCancelEnable = false;
            if (dlg.ShowDialog() == true) {

            }
        }

        /// <summary>
        /// ２点間の距離
        /// </summary>
        /// <param name="ps">始点</param>
        /// <param name="pe">終点</param>
        public void measureDistance(PointD ps, PointD pe)
        {
            double dis = ps.length(pe);
            Clipboard.SetText(dis.ToString());
            string buf = "距離 : " + ylib.double2StrZeroSup(dis, "F8");
            ylib.messageBox(mMainWindow, buf, "", "距離測定");
        }

        /// <summary>
        /// 3点間の角度
        /// </summary>
        /// <param name="ps">始点</param>
        /// <param name="pc">中心点</param>
        /// <param name="pe">終点</param>
        public void measureAngle(PointD ps, PointD pc, PointD pe)
        {
            LineD ls = new LineD(pc, ps);
            LineD le = new LineD(pc, pe);
            double ang = ls.angle2(le);
            Clipboard.SetText(ang.ToString());
            string buf = "角度 : " + ylib.double2StrZeroSup(ylib.R2D(ang), "F8") + "°";
            ylib.messageBox(mMainWindow, buf, "", "角度測定");
            Clipboard.SetText(ang.ToString());
        }

        /// <summary>
        /// 図面の属性値を設定
        /// </summary>
        /// <returns></returns>
        public bool zumenProperty(DrawingPara para)
        {
            SysProperty dlg = new SysProperty();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            dlg.mColor = para.mColor;
            dlg.mLineType = para.mLineType;
            dlg.mThickness = para.mThickness;
            dlg.mPointType = para.mPointType;
            dlg.mPointSize = para.mPointSize;
            dlg.mTextSize = para.mTextSize;
            dlg.mTextRotate = para.mTextRotate;
            dlg.mLinePitchRate = para.mLinePitchRate;
            dlg.mHa = para.mHa;
            dlg.mVa = para.mVa;
            dlg.mFontFamily = para.mFontFamily;
            dlg.mFontStyle = para.mFontStyle.ToString();
            dlg.mFontWeight = para.mFontWeight.ToString();
            dlg.mArrowSize = para.mArrowSize;
            dlg.mArrowAngle = para.mArrowAngle;
            dlg.mGridSize = para.mGridSize;
            dlg.mBackColor = para.mBackColor;
            if (dlg.ShowDialog() == true) {
                para.mColor = dlg.mColor;
                para.mLineType = dlg.mLineType;
                para.mThickness = dlg.mThickness;
                para.mPointType = dlg.mPointType;
                para.mPointSize = dlg.mPointSize;
                para.mTextSize = dlg.mTextSize;
                para.mTextRotate = dlg.mTextRotate;
                para.mLinePitchRate = dlg.mLinePitchRate;
                para.mHa = dlg.mHa;
                para.mVa = dlg.mVa;
                para.mFontFamily = dlg.mFontFamily;
                para.mFontStyle = ylib.convFontStyle(dlg.mFontStyle);
                para.mFontWeight = ylib.convFontWeight(dlg.mFontWeight);
                para.mArrowSize = dlg.mArrowSize;
                para.mArrowAngle = dlg.mArrowAngle;
                para.mGridSize = dlg.mGridSize;
                para.mBackColor = dlg.mBackColor;
                dlg.Close();
                mEntityData.mOperationCount++;
                return true;
            } else {
                dlg.Close();
                return false;
            }
        }

        /// <summary>
        /// 作成レイヤーを設定する
        /// </summary>
        public void setCreateLayer()
        {
            InputSelect dlg = new InputSelect();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "作成レイヤー";
            dlg.mText = mEntityData.mPara.mCreateLayerName;
            dlg.mTextList = mEntityData.getLayerNameList();
            if (dlg.ShowDialog() == true) {
                mEntityData.mPara.mCreateLayerName = dlg.mText;
                mEntityData.addDispLayer(mEntityData.mPara.mCreateLayerName);
                mMainWindow.cbCreateLayer.ItemsSource = mEntityData.getLayerNameList();
                mMainWindow.cbCreateLayer.SelectedIndex = mMainWindow.cbCreateLayer.Items.IndexOf(mEntityData.mPara.mCreateLayerName);
                mEntityData.mOperationCount++;
            }
        }

        /// <summary>
        /// 表示レイヤーのモードレスダイヤログを表示
        /// </summary>
        public void setDispLayer()
        {
            if (mChkListDlg != null)
                mChkListDlg.Close();
            mChkListDlg = new ChkListDialog();
            mChkListDlg.Topmost = true;
            mChkListDlg.mTitle = "表示レイヤー";
            mChkListDlg.mChkList = mEntityData.getLayerChkList();
            mChkListDlg.mCallBackOn = true;
            mChkListDlg.callback = setLayerChk;
            mChkListDlg.Show();
            mEntityData.mOperationCount++;
        }

        /// <summary>
        /// レイヤーチェックリストに表示を更新
        /// </summary>
        public void setLayerChk()
        {
            ulong createLayerBit = mEntityData.getLayerBit(mEntityData.mPara.mCreateLayerName);
            mEntityData.setDispLayerBit(mChkListDlg.mChkList);
            if ((createLayerBit & mEntityData.mPara.mDispLayerBit) == 0) {
                ylib.messageBox(mMainWindow, "作成レイヤーを非表示にすることはできません");
                mChkListDlg.visibleDataSet(mEntityData.mPara.mCreateLayerName);
                mEntityData.mPara.mDispLayerBit |= createLayerBit;
            }
            if (mEntityData.mPara.mDispLayerBit != createLayerBit) {
                mEntityData.mPara.mOneLayerDisp = false;
            }
            mEntityData.updateData();
            mMainWindow.commandClear();
        }

        /// <summary>
        /// 全レイヤー表示
        /// </summary>
        public void setFulleDispLayer()
        {
            mEntityData.mPara.mDispLayerBit = 0xffffffff;
            mEntityData.updateData();
            mEntityData.mPara.mOneLayerDisp = false;
            mEntityData.mOperationCount++;
        }

        /// <summary>
        /// レイヤーを1レイヤー表示に切り替える
        /// </summary>
        /// <param name="oneLayer">１レイヤー</param>
        public void setOneLayerDisp(bool oneLayer)
        {
            if (oneLayer) {
                mEntityData.mPara.mDispLayerBit &= mEntityData.getLayerBit(mEntityData.mPara.mCreateLayerName);
                mEntityData.mPara.mOneLayerDisp = true;
            } else {
                mEntityData.mPara.mOneLayerDisp = true;
            }
            mEntityData.mOperationCount++;
        }

        /// <summary>
        /// レイヤー名を変更する
        /// </summary>
        public void changeLayerName()
        {
            InputSelect2 dlg = new InputSelect2();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "レイヤー名の変更";
            dlg.mTitle1 = "変更前";
            dlg.mTitle2 = "変更後";
            dlg.mSelectList = mEntityData.getLayerNameList();
            if (dlg.ShowDialog()==true) {
                mEntityData.mOperationCount++;
                mEntityData.changeLayerName(dlg.mSelectText, dlg.mEditText);
                mMainWindow.cbCreateLayer.ItemsSource = mEntityData.getLayerNameList();
                mMainWindow.cbCreateLayer.SelectedIndex = mMainWindow.cbCreateLayer.Items.IndexOf(mEntityData.mPara.mCreateLayerName);
                mMainWindow.btDummy.Focus();    //  ダミーフォーカス
            }
        }


        /// <summary>
        /// 2要素の距離または角度を測定
        /// </summary>
        /// <param name="pickEnt"></param>
        public void measureDisp(List<(int no, PointD pos)> pickEnt)
        {
            string buf = "";
            double dis = -1;
            double ang = -1;
            if (1 < pickEnt.Count) {
                Entity entity0 = mEntityData.mEntityList[pickEnt[0].no];
                Entity entity1 = mEntityData.mEntityList[pickEnt[1].no];
                PointD ip;
                int np;
                buf += entity0.getSummary() + "\n";
                buf += entity1.getSummary() + "\n";
                switch (entity0.mEntityId) {
                    case EntityId.Point:
                        PointEntity pointEnt = (PointEntity)entity0;
                        PointD point = pointEnt.mPoint;
                        switch (entity1.mEntityId) {
                            case EntityId.Point:
                                PointEntity pe1 = (PointEntity)entity1;
                                dis = point.length(pe1.mPoint);
                                break;
                            case EntityId.Line:
                                LineEntity le1 = (LineEntity)entity1;
                                dis = le1.mLine.distance(point);
                                break;
                            case EntityId.Arc:
                                ArcEntity ae1 = (ArcEntity)entity1;
                                ip = ae1.mArc.intersection(point);
                                dis = point.length(ip);
                                break;
                            case EntityId.Polyline:
                                PolylineEntity ple1 = (PolylineEntity)entity1;
                                np = ple1.mPolyline.nearPeackPos(pickEnt[1].pos);
                                ip = ple1.mPolyline.getLine(np).intersection(point);
                                dis = point.length(ip);
                                break;
                            case EntityId.Polygon:
                                PolygonEntity pge1 = (PolygonEntity)entity1;
                                np = pge1.mPolygon.nearPeackPos(pickEnt[1].pos);
                                ip = pge1.mPolygon.getLine(np).intersection(point);
                                dis = point.length(ip);
                                break;
                        }
                        break;
                    case EntityId.Line:
                        LineEntity lineEnt = (LineEntity)entity0;
                        LineD line = lineEnt.mLine;
                        switch (entity1.mEntityId) {
                            case EntityId.Point:
                                PointEntity pe1 = (PointEntity)entity1;
                                dis = line.distance(pe1.mPoint);
                                break;
                            case EntityId.Line:
                                LineEntity le1 = (LineEntity)entity1;
                                if (line.angle(le1.mLine) < mEps)
                                    dis = line.distance(le1.mLine);
                                else
                                    ang = line.angle(le1.mLine);
                                break;
                            case EntityId.Polyline:
                                PolylineEntity ple1 = (PolylineEntity)entity1;
                                LineD polylineLine = ple1.mPolyline.nearLine(pickEnt[1].pos);
                                ip = polylineLine.intersection(line.ps);
                                if (line.angle(polylineLine) < mEps)
                                    dis = ip.length(line.ps);
                                else
                                    ang = line.angle(polylineLine);
                                break;
                            case EntityId.Polygon:
                                PolygonEntity pge1 = (PolygonEntity)entity1;
                                LineD polygonLine = pge1.mPolygon.nearLine(pickEnt[1].pos);
                                ip = polygonLine.intersection(line.ps);
                                if (line.angle(polygonLine) < mEps)
                                    dis = ip.length(line.ps);
                                else
                                    ang = line.angle(polygonLine);
                                break;
                        }
                        break;
                    case EntityId.Polyline:
                        PolylineEntity ple0 = (PolylineEntity)entity0;
                        LineD pline = ple0.mPolyline.nearLine(pickEnt[0].pos);
                        switch (entity1.mEntityId) {
                            case EntityId.Point:
                                PointEntity pe1 = (PointEntity)entity1;
                                dis = pline.distance(pe1.mPoint);
                                break;
                            case EntityId.Line:
                                LineEntity le1 = (LineEntity)entity1;
                                if (pline.angle(le1.mLine) < mEps)
                                    dis = pline.distance(le1.mLine);
                                else
                                    ang = pline.angle(le1.mLine);
                                break;
                            case EntityId.Polyline:
                                PolylineEntity ple1 = (PolylineEntity)entity1;
                                LineD polylineLine = ple1.mPolyline.nearLine(pickEnt[1].pos);
                                ip = polylineLine.intersection(pline.ps);
                                if (pline.angle(polylineLine) < mEps)
                                    dis = ip.length(pline.ps);
                                else
                                    ang = pline.angle(polylineLine);
                                break;
                            case EntityId.Polygon:
                                PolygonEntity pge1 = (PolygonEntity)entity1;
                                LineD polygonLine = pge1.mPolygon.nearLine(pickEnt[1].pos);
                                ip = polygonLine.intersection(pline.ps);
                                if (pline.angle(polygonLine) < mEps)
                                    dis = ip.length(pline.ps);
                                else
                                    ang = pline.angle(polygonLine);
                                break;
                        }
                        break;
                    case EntityId.Polygon:
                        PolygonEntity pge0 = (PolygonEntity)entity0;
                        LineD pgLine = pge0.mPolygon.nearLine(pickEnt[0].pos);
                        switch (entity1.mEntityId) {
                            case EntityId.Point:
                                PointEntity pe1 = (PointEntity)entity1;
                                dis = pgLine.distance(pe1.mPoint);
                                break;
                            case EntityId.Line:
                                LineEntity le1 = (LineEntity)entity1;
                                if (pgLine.angle(le1.mLine) < mEps)
                                    dis = pgLine.distance(le1.mLine);
                                else
                                    ang = pgLine.angle(le1.mLine);
                                break;
                            case EntityId.Polyline:
                                PolylineEntity ple1 = (PolylineEntity)entity1;
                                LineD polylineLine = ple1.mPolyline.nearLine(pickEnt[1].pos);
                                ip = polylineLine.intersection(pgLine.ps);
                                if (pgLine.angle(polylineLine) < mEps)
                                    dis = ip.length(pgLine.ps);
                                else
                                    ang = pgLine.angle(polylineLine);
                                break;
                            case EntityId.Polygon:
                                PolygonEntity pge1 = (PolygonEntity)entity1;
                                LineD polygonLine = pge1.mPolygon.nearLine(pickEnt[1].pos);
                                ip = polygonLine.intersection(pgLine.ps);
                                if (pgLine.angle(polygonLine) < mEps)
                                    dis = ip.length(pgLine.ps);
                                else
                                    ang = pgLine.angle(polygonLine);
                                break;
                        }
                        break;
                    case EntityId.Arc:
                        ArcEntity arcEnt = (ArcEntity)entity0;
                        ArcD arc = arcEnt.mArc;
                        switch (entity1.mEntityId) {
                            case EntityId.Point:
                                break;
                        }
                        break;
                    case EntityId.Text:
                        switch (entity1.mEntityId) {
                            case EntityId.Point:
                                break;
                        }
                        break;
                }
                if (0 <= dis) {
                    buf += "距離 : " + ylib.double2StrZeroSup(dis, "F8");
                    Clipboard.SetText(dis.ToString());
                }
                if (0 <= ang) {
                    buf += (0 <= dis ? "\n" : "") + "角度 : " + ylib.double2StrZeroSup(ylib.R2D(ang), "F8");
                    Clipboard.SetText(ang.ToString());
                }
                ylib.messageBox(mMainWindow, buf, "距離・角度測定");
            }
        }

        /// <summary>
        /// ピックした要素の情報表示
        /// </summary>
        public bool infoEntity(List<(int no, PointD pos)> pickEnt)
        {
            foreach ((int no, PointD pos) entNo in pickEnt) {
                ylib.messageBox(mMainWindow, mEntityData.mEntityList[entNo.no].entityInfo(),"", "要素情報");
            }
            return true;
        }

        /// <summary>
        /// 要素データをテキストで編集
        /// </summary>
        /// <param name="pickEnt"></param>
        /// <returns></returns>
        public bool infoEntityData(List<(int no, PointD pos)> pickEnt)
        {
            mEntityData.mOperationCount++;

            foreach ((int no, PointD pos) entNo in pickEnt) {
                string propertyStr = mEntityData.mEntityList[entNo.no].toString();
                string dataStr = mEntityData.mEntityList[entNo.no].toDataString();
                InputBox dlg = new InputBox();
                dlg.Owner = mMainWindow;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlg.Title = $"[{entNo.no}] {propertyStr}";
                dlg.mEditText = dataStr;
                if (dlg.ShowDialog() == true) {
                    mEntityData.mOperationCount++;
                    dataStr = dlg.mEditText;
                    string[] property = propertyStr.Split(new char[] { ',' });
                    string[] data = dataStr.Split(new char[] { ',' });
                    Entity ent = mEntityData.setStringEntityData(property, data);
                    mEntityData.mEntityList.Add(ent);
                    mEntityData.mEntityList[mEntityData.mEntityList.Count - 1].mOperationCount = mEntityData.mOperationCount;
                    mEntityData.removeEnt(entNo.no);
                }
            }
            mEntityData.updateData();
            return true;
        }

        /// <summary>
        /// 図面情報の表示と編集(ダイヤログ表示)
        /// </summary>
        public void  zumenComment()
        {
            InputBox dlg = new InputBox();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mMultiLine = true;
            dlg.mWindowSizeOutSet = true;
            dlg.Title = "図面のコメント";
            dlg.mEditText = mEntityData.mPara.mComment;
            if (dlg.ShowDialog() == true) {
                mEntityData.mPara.mComment = dlg.mEditText;
                mEntityData.mOperationCount++;
            }
        }

        /// <summary>
        /// グリッドのサイズ(座標の丸め)の設定
        /// </summary>
        public void gridSet()
        {
            InputBox dlg = new InputBox();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "グリッドのサイズの設定";
            dlg.mEditText = Math.Abs(mEntityData.mPara.mGridSize).ToString();
            if (dlg.ShowDialog() == true) {
                mEntityData.mPara.mGridSize = ylib.string2double(dlg.mEditText);
            }
        }

        /// <summary>
        /// スクリーンキャプチャ
        /// </summary>
        private void screenCapture(Window window,int interval = 0)
        {
            System.Threading.Thread.Sleep(interval * 1000);
            screenCapture(window);
            imageTrimming(window);
        }

        /// <summary>
        /// 画面の一部を切り取ってクリップボードに貼り付ける
        /// </summary>
        /// <param name="window">親Window</param>
        private void screenCapture(Window window)
        {
            //  自アプリ退避
            mWinState = window.WindowState;
            window.WindowState = WindowState.Minimized;
            System.Threading.Thread.Sleep(500);
            //  全画面をキャプチャ
            BitmapSource bitmapSource = ylib.bitmap2BitmapSource(ylib.getFullScreenCapture()); ;
            //  自アプリを元に戻す
            window.WindowState = mWinState;
            window.Activate();
            //  キャプチャしたイメージを全画面表示し領域を切り取る
            FullView dlg = new FullView();
            dlg.mBitmapSource = bitmapSource;
            if (dlg.ShowDialog() == true) {
                System.Drawing.Bitmap bitmap = ylib.cnvBitmapSource2Bitmap(bitmapSource);
                bitmap = ylib.trimingBitmap(bitmap, dlg.mStartPoint, dlg.mEndPoint);
                if (bitmap != null) {
                    //  クリップボードに張り付ける
                    Clipboard.SetImage(ylib.bitmap2BitmapSource(bitmap));
                    mClipImagePath = mImageData.saveBitmapCash(bitmap);
                }
            }
        }

        /// <summary>
        /// クリップボードのイメージをトリミングする
        /// </summary>
        /// <param name="window"></param>
        private void imageTrimming(Window window)
        {
            ImagePaste dlg = new ImagePaste();
            dlg.Owner = window;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "画像のサイズ設定";
            dlg.mCancelEnable = false;
            if (dlg.ShowDialog() == true) {
                //Image image = new Image();
                //image.Stretch = Stretch.Fill;
                //image.Width = dlg.mWidth;
                //image.Height = dlg.mHeight;
                //image.Source = dlg.mBitmapSource;
                //var tp = rc.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);
                //new InlineUIContainer(image, tp);
            }
        }


        /// <summary>
        /// 図面ファイルを選択して読みだす
        /// </summary>
        public bool openAsFile()
        {
            List<string[]> filters = new List<string[]>() {
                    new string[] { "図面ファイル", "*.csv" },
                    new string[] { "すべてのファイル", "*.*"}
                };
            string filePath = ylib.fileOpenSelectDlg("データ読込", ".", filters);
            return openFile(filePath);
        }

        /// <summary>
        /// 図面ファイルを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns></returns>
        public bool openFile(string filePath)
        {
            if (0 < filePath.Length) {
                mEntityData.loadData(filePath);
                mCurFilePath = filePath;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 上書き保存/ファイル選択保存
        /// </summary>
        /// <param name="saveonly">上書き保存</param>
        public void saveFile(bool saveonly = false)
        {
            if (0 < mCurFilePath.Length) {
                if (mCurFilePath.IndexOf(".csv") < 0)
                    mCurFilePath = mCurFilePath + ".csv";
                mEntityData.saveData(mCurFilePath);
            } else if (!saveonly) {
                saveAsFile();
            }
        }

        /// <summary>
        /// ファイル名を指定して保存する
        /// </summary>
        public void saveAsFile(string initFolder = ".")
        {
            List<string[]> filters = new List<string[]>() {
                    new string[] { "CSVファイル", "*.csv" },
                    new string[] { "すべてのファイル", "*.*"}
                };
            string filePath = ylib.fileSaveSelectDlg("データ保存", initFolder, filters);
            if (0 < filePath.Length) {
                if (filePath.IndexOf(".csv") < 0)
                    filePath = filePath + ".csv";
                mEntityData.saveData(filePath);
                mCurFilePath = filePath;
            }
        }
    }
}
