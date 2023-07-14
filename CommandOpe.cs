using CoreLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CadApp
{
    class CommandOpe
    {
        public EntityData mEntityData;                              //  要素データ

        public int mPointType = 0;                                  //  点種
        public double mPointSize = 1;                               //  点の大きさ
        public int mLineType = 0;                                   //  線種
        public double mThickness = 1;                               //  線の太さ
        public double mTextSize = 12;                               //  文字サイズ
        public double mTextRotate = 0;                              //  文字列の回転角
        public string mTextString = "";                             //  文字列データ
        public HorizontalAlignment mHa = HorizontalAlignment.Left;  //  水平アライメント
        public VerticalAlignment mVa = VerticalAlignment.Top;       //  垂直アライメント
        public double mArrowAngle = Math.PI / 6;                    //  矢印の角度
        public double mArrowSize = 5;                               //  矢印の大きさ
        public double mTextScale = 1;                               //  文字や矢印の大きさの倍率
        public Brush mCreateColor = Brushes.Black;                  //  要素の色
        public double mGridSize = 1.0;                              //  マウス座標の丸め値
        public string mCurFilePath = "";                            //  編集中のファイルパス
        public int mGridMinmumSize = 10;                            //  グリッドの最小表示スクリーンサイズ
        public List<PointD> mLocPos = new();                        //  マウス指定点
        public List<(int no, PointD pos)> mPickEnt = new();         //  ピックした要素リスト

        public Box mInitArea = new(-10, 150, 250, -10);             //  初期表示領域
        public Box mDispArea;                                       //  表示領域
        public Brush mBackColor = Brushes.White;                    //  背景色
        private readonly double mEps = 1E-8;

        private KeyCommand mKeyCommand = new();

        public MainWindow mMainWindow;

        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="entityData">要素データ</param>
        /// <param name="canvas">Canvas</param>
        public CommandOpe(EntityData entityData, MainWindow mainWindow)
        {
            mEntityData = entityData;
            mKeyCommand.mEntityData = mEntityData;
            mDispArea = new Box(mInitArea);
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
            mEntityData.mArea = new Box(mInitArea);
            saveFile();
        }

        /// <summary>
        /// コマンド処理
        /// </summary>
        /// <param name="command">コマンド</param>
        public MainWindow.OPEMODE executeCmd(OPERATION operation)
        {
            bool commansInit = true;
            MainWindow.OPEMODE locMode = MainWindow.OPEMODE.pick;

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
                case OPERATION.createArrow:
                case OPERATION.createLabel:
                case OPERATION.createLocDimension:
                    locMode = MainWindow.OPEMODE.loc;
                    mPickEnt.Clear();
                    mLocPos.Clear();
                    commansInit = false;
                    break;
                case OPERATION.createDimension:
                case OPERATION.createAngleDimension:
                case OPERATION.createDiameterDimension:
                case OPERATION.createRadiusDimension:
                case OPERATION.offset:
                case OPERATION.translate:
                case OPERATION.rotate:
                case OPERATION.mirror:
                case OPERATION.trim:
                case OPERATION.divide:
                case OPERATION.stretch:
                case OPERATION.copyTranslate:
                case OPERATION.copyRotate:
                case OPERATION.copyMirror:
                case OPERATION.copyOffset:
                    locMode = MainWindow.OPEMODE.loc;
                    mLocPos.Clear();
                    commansInit = false;
                    break;
                case OPERATION.textChange:
                    changeText(mPickEnt);
                    break;
                case OPERATION.changeProperty:
                    changeProperty(mPickEnt);
                    break;
                case OPERATION.disassemble:
                    mEntityData.disassemble(mPickEnt);
                    break;
                case OPERATION.remove:
                    mEntityData.removeEnt(mPickEnt);
                    break;
                case OPERATION.measureDistance:
                    measureDisp(mPickEnt);
                    break;
                case OPERATION.measureAngle:
                    break;
                case OPERATION.info:
                    infoEntity(mPickEnt);
                    break;
                case OPERATION.infoData:
                    infoEntityData(mPickEnt);
                    break;
                case OPERATION.systemIinfo:
                    if (systemProperty())
                        mMainWindow.setSystemProperty();
                    break;
                case OPERATION.allClear:
                    break;
                case OPERATION.undo:
                    mEntityData.undo();
                    break;
                case OPERATION.redo:
                    break;
                case OPERATION.cancel:
                    mLocPos.Clear();
                    mPickEnt.Clear();
                    break;
                case OPERATION.close:
                    mMainWindow.Close();
                    break;
                case OPERATION.gridSize:
                    gridSet();
                    break;
            }
            if (commansInit) {
                mMainWindow.commandClear();
            }
            return locMode;
        }

        /// <summary>
        /// 要素追加コマンド
        /// </summary>
        /// <param name="operation">操作名</param>
        /// <param name="locPos">座標リスト</param>
        public bool entityCommand(OPERATION operation, List<PointD> locPos, List<(int no, PointD pos)> pickEnt)
        {
            mEntityData.mOperationCouunt++;
            if (operation == OPERATION.createPoint || operation == OPERATION.createLine
                || operation == OPERATION.createRect || operation == OPERATION.createArc
                || operation == OPERATION.createCircle || operation == OPERATION.createText
                || operation == OPERATION.createPolyline || operation == OPERATION.createPolygon
                || operation == OPERATION.createArrow || operation == OPERATION.createLabel
                || operation == OPERATION.createLocDimension) {
                //  要素の追加 (Ctrlキーなし)
                if (createData(locPos, operation)) {
                    return true;
                }
            } else if (locPos.Count == 1 &&
                (operation == OPERATION.divide || operation == OPERATION.createDimension
                || operation == OPERATION.createAngleDimension
                || operation == OPERATION.createDiameterDimension
                || operation == OPERATION.createRadiusDimension)) {
                //  編集コマンド
                if (changeData(locPos, pickEnt, operation)) {
                    return true;
                }
            } else if (locPos.Count == 2 &&
                (operation == OPERATION.translate || operation == OPERATION.rotate
                || operation == OPERATION.mirror || operation == OPERATION.offset
                || operation == OPERATION.copyTranslate || operation == OPERATION.copyRotate
                || operation == OPERATION.copyMirror || operation == OPERATION.copyOffset
                || operation == OPERATION.trim || operation == OPERATION.stretch)) {
                //  編集コマンド
                if (changeData(locPos, pickEnt, operation)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 要素作成データ追加
        /// </summary>
        /// <param name="points">ロケイト点はいれつ</param>
        /// <param name="operation">操作</param>
        /// <returns></returns>
        public bool createData(List<PointD> points, OPERATION operation)
        {
            mEntityData.mColor     = mCreateColor;
            mEntityData.mThickness = mThickness;
            mEntityData.mLineType  = mLineType;
            mEntityData.mTextSize  = mTextSize;
            mEntityData.mArrowSize = mArrowSize;
            if (operation == OPERATION.createPoint && points.Count == 1) {
                //  点要素作成
                mEntityData.mPointType = mPointType;
                mEntityData.mPointSize = mPointSize;
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
                    mEntityData.addArc(new ArcD(points[0], points[1], points[2]));
            } else if (operation == OPERATION.createCircle && points.Count == 2) {
                //  円の作成
                mEntityData.addArc(new ArcD(points[0], points[0].length(points[1])));
            } else if (operation == OPERATION.createText && points.Count == 1) {
                //  テキスト要素の作成
                TextD text = new TextD(mTextString, points[0], mTextSize, mTextRotate, mHa, mVa);
                mEntityData.addText(text);
            } else if (operation == OPERATION.createPolyline) {
                //  ポリライン要素の作成
                if (1 < points.Count)
                    mEntityData.addPolyline(points);
            } else if (operation == OPERATION.createPolygon) {
                //  ポリゴンの作成
                if (1 < points.Count)
                    mEntityData.addPolygon(points);
            } else if (operation == OPERATION.createArrow && points.Count == 2) {
                //  矢印の作成
                mEntityData.addArrow(points[0], points[1]);
            } else if (operation == OPERATION.createLabel && points.Count == 2) {
                //  ラベルの作成
                mEntityData.addLabel(points[0], points[1], mTextString);
            } else if (operation == OPERATION.createLocDimension && points.Count == 3) {
                //  寸法線の作成
                mEntityData.addLocDimension(points[0], points[1], points[2]);
            } else {
                mEntityData.mOperationCouunt--;
                return false;
            }
            return true;
        }

        /// <summary>
        /// ピックした要素のデータ更新(移動,回転,ミラー)
        /// </summary>
        /// <param name="loc">ロケイト点リスト</param>
        /// <returns></returns>
        public bool changeData(List<PointD> loc, List<(int, PointD)> pickEnt, OPERATION operation)
        {
            if (loc.Count == 1) {
                mEntityData.mColor = mCreateColor;
                mEntityData.mThickness = mThickness;
                mEntityData.mLineType = mLineType;
                mEntityData.mTextSize = mTextSize;
                mEntityData.mArrowSize = mArrowSize;
                if (operation == OPERATION.divide) {
                    //  分割
                    mEntityData.divide(pickEnt, loc[0]);
                } else if (operation == OPERATION.createDimension) {
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
                }
            } else if (loc.Count == 2) {
                if (operation == OPERATION.translate) {
                    //  移動
                    PointD vec = loc[0].vector(loc[1]);
                    mEntityData.translate(pickEnt, vec);
                } else if (operation == OPERATION.copyTranslate) {
                    //  コピー移動
                    PointD vec = loc[0].vector(loc[1]);
                    mEntityData.translate(pickEnt, vec, true);
                } else if (operation == OPERATION.rotate) {
                    //  回転
                    mEntityData.rotate(pickEnt, loc[0], loc[1]);
                } else if (operation == OPERATION.copyRotate) {
                    //  コピー回転
                    mEntityData.rotate(pickEnt, loc[0], loc[1], true);
                } else if (operation == OPERATION.mirror) {
                    //  ミラー
                    mEntityData.mirror(pickEnt, loc[0], loc[1]);
                } else if (operation == OPERATION.copyMirror) {
                    //  コピーミラー
                    mEntityData.mirror(pickEnt, loc[0], loc[1], true);
                } else if (operation == OPERATION.offset) {
                    //  オフセット
                    mEntityData.offset(pickEnt, loc[0], loc[1]);
                } else if (operation == OPERATION.copyOffset) {
                    //  コピーオフセット
                    mEntityData.offset(pickEnt, loc[0], loc[1], true);
                } else if (operation == OPERATION.trim) {
                    //  トリム
                    mEntityData.trim(pickEnt, loc[0], loc[1]);
                } else if (operation == OPERATION.divide) {
                    //  分割
                    mEntityData.divide(pickEnt, loc[0]);
                } else if (operation == OPERATION.stretch) {
                    //  ストレッチ
                    PointD vec = loc[0].vector(loc[1]);
                    mEntityData.stretch(pickEnt, vec);
                } else {
                    mEntityData.mOperationCouunt--;
                    return false;
                }
            } else if (loc.Count == 3) {

            } else {
                mEntityData.mOperationCouunt--;
                return false;
            }
            return true;
        }

        /// <summary>
        /// キー入力によるコマンド処理
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <returns></returns>
        public bool keyCommand(string command)
        {
            mEntityData.mOperationCouunt++;
            return mKeyCommand.setCommand(command);
        }

        /// <summary>
        /// ピックしたテキスト要素の文字列を変更する
        /// </summary>
        /// <returns></returns>
        public bool changeText(List<(int, PointD)> pickEnt)
        {
            mEntityData.mOperationCouunt++;
            foreach ((int no, PointD pos) pickNo in pickEnt) {
                InputBox dlg = new InputBox();
                if (mEntityData.mEntityList[pickNo.no].mEntityId == EntityId.Text) {
                    TextEntity text = (TextEntity)mEntityData.mEntityList[pickNo.no];
                    dlg.mEditText = text.mText.mText;
                    if (dlg.ShowDialog() == true) {
                        mEntityData.mEntityList.Add(mEntityData.mEntityList[pickNo.no].toCopy());
                        text = (TextEntity)mEntityData.mEntityList[mEntityData.mEntityList.Count - 1];
                        text.mText.mText = dlg.mEditText;
                        mEntityData.mEntityList[mEntityData.mEntityList.Count - 1].mOperationCount = mEntityData.mOperationCouunt;
                        mEntityData.removeEnt(pickNo.no);
                    }
                    dlg.Close();
                } else if (mEntityData.mEntityList[pickNo.no].mEntityId == EntityId.Parts) {
                    PartsEntity parts = (PartsEntity)mEntityData.mEntityList[pickNo.no];
                    if (parts.mParts.mName == "ラベル" && 0 < parts.mParts.mTexts.Count) {
                        dlg.mEditText = parts.mParts.mTexts[0].mText;
                        if (dlg.ShowDialog() == true) {
                            mEntityData.mEntityList.Add(mEntityData.mEntityList[pickNo.no].toCopy());
                            parts = (PartsEntity)mEntityData.mEntityList[mEntityData.mEntityList.Count - 1];
                            parts.mParts.mTexts[0].mText = dlg.mEditText;
                            mEntityData.mEntityList[mEntityData.mEntityList.Count - 1].mOperationCount = mEntityData.mOperationCouunt;
                            mEntityData.removeEnt(pickNo.no);
                        }
                        dlg.Close();
                    }
                }
            }
            mEntityData.updateData();
            return true;
        }

        /// <summary>
        /// ピックした要素の属性変更
        /// </summary>
        /// <returns></returns>
        public bool changeProperty(List<(int, PointD)> pickEnt)
        {
            mEntityData.mOperationCouunt++;
            foreach ((int no, PointD pos) pickNo in pickEnt) {
                EntProperty dlg = new EntProperty();
                dlg.Owner = mMainWindow;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlg.mEntityId = mEntityData.mEntityList[pickNo.no].mEntityId;
                dlg.mColor = mEntityData.mEntityList[pickNo.no].mColor;
                dlg.mLineType = mEntityData.mEntityList[pickNo.no].mType;
                dlg.mThickness = mEntityData.mEntityList[pickNo.no].mThickness;
                if (mEntityData.mEntityList[pickNo.no].mEntityId == EntityId.Text) {
                    TextEntity text = (TextEntity)mEntityData.mEntityList[pickNo.no];
                    dlg.mTextSize = text.mText.mTextSize;
                    dlg.mHa = text.mText.mHa;
                    dlg.mVa = text.mText.mVa;
                    dlg.mTextRotate = text.mText.mRotate;
                }
                if (mEntityData.mEntityList[pickNo.no].mEntityId == EntityId.Parts) {
                    PartsEntity parts = (PartsEntity)mEntityData.mEntityList[pickNo.no];
                    dlg.mTextSize = parts.mParts.mTextSize;
                    dlg.mArrowSize = parts.mParts.mArrowSize;
                    dlg.mArrowAngle = parts.mParts.mArrowAngle;
                }
                if (dlg.ShowDialog() == true) {
                    mEntityData.mEntityList.Add(mEntityData.mEntityList[pickNo.no].toCopy());
                    mEntityData.mEntityList[mEntityData.mEntityList.Count - 1].mColor = dlg.mColor;
                    mEntityData.mEntityList[mEntityData.mEntityList.Count - 1].mType = dlg.mLineType;
                    mEntityData.mEntityList[mEntityData.mEntityList.Count - 1].mThickness = dlg.mThickness;
                    if (mEntityData.mEntityList[mEntityData.mEntityList.Count - 1].mEntityId == EntityId.Text) {
                        TextEntity text = (TextEntity)mEntityData.mEntityList[mEntityData.mEntityList.Count - 1];
                        text.mText.mTextSize = dlg.mTextSize;
                        text.mText.mHa = dlg.mHa;
                        text.mText.mVa = dlg.mVa;
                        text.mText.mRotate = dlg.mTextRotate;
                    }
                    if (mEntityData.mEntityList[mEntityData.mEntityList.Count - 1].mEntityId == EntityId.Parts) {
                        PartsEntity parts = (PartsEntity)mEntityData.mEntityList[mEntityData.mEntityList.Count - 1];
                        parts.mParts.mTextSize = dlg.mTextSize;
                        parts.mParts.mArrowSize = dlg.mArrowSize;
                        parts.mParts.mArrowAngle = dlg.mArrowAngle;
                        parts.mParts.remakeData();
                    }
                    mEntityData.removeEnt(pickNo.no);
                }
                dlg.Close();
            }
            mEntityData.updateData();
            return true;
        }

        /// <summary>
        /// 要素の属性値の設定
        /// </summary>
        /// <returns></returns>
        public bool systemProperty()
        {
            SysProperty dlg = new SysProperty();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mColor = mCreateColor;
            dlg.mLineType = mLineType;
            dlg.mThickness = mThickness;
            dlg.mPointType = mPointType;
            dlg.mPointSize = mPointSize;
            dlg.mTextSize = mTextSize;
            dlg.mHa = mHa;
            dlg.mVa = mVa;
            dlg.mGridSize = mGridSize;
            if (dlg.ShowDialog() == true) {
                mCreateColor = dlg.mColor;
                mLineType = dlg.mLineType;
                mThickness = dlg.mThickness;
                mPointType = dlg.mPointType;
                mPointSize = dlg.mPointSize;
                mTextSize = dlg.mTextSize;
                mHa = dlg.mHa;
                mVa = dlg.mVa;
                mGridSize = dlg.mGridSize;
                dlg.Close();
                return true;
            } else {
                dlg.Close();
                return false;
            }
        }

        /// <summary>
        /// 2要素の距離または角度を測定
        /// </summary>
        /// <param name="pickEnt"></param>
        public void measureDisp(List<(int no, PointD pos)> pickEnt)
        {
            string buf = "";
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
                                buf += "距離 :" + point.length(pe1.mPoint);
                                break;
                            case EntityId.Line:
                                LineEntity le1 = (LineEntity)entity1;
                                buf += "距離 :" + le1.mLine.distance(point).ToString();
                                break;
                            case EntityId.Arc:
                                ArcEntity ae1 = (ArcEntity)entity1;
                                ip = ae1.mArc.intersection(point);
                                buf += "距離 :" + point.length(ip);
                                break;
                            case EntityId.Polyline:
                                PolylineEntity ple1 = (PolylineEntity)entity1;
                                np = ple1.mPolyline.nearPeackPos(pickEnt[1].pos);
                                ip = ple1.mPolyline.getLine(np).intersection(point);
                                buf += "距離 :" + point.length(ip);
                                break;
                            case EntityId.Polygon:
                                PolygonEntity pge1 = (PolygonEntity)entity1;
                                np = pge1.mPolygon.nearPeackPos(pickEnt[1].pos);
                                ip = pge1.mPolygon.getLine(np).intersection(point);
                                buf += "距離 :" + point.length(ip);
                                break;
                        }
                        break;
                    case EntityId.Line:
                        LineEntity lineEnt = (LineEntity)entity0;
                        LineD line = lineEnt.mLine;
                        switch (entity1.mEntityId) {
                            case EntityId.Point:
                                PointEntity pe1 = (PointEntity)entity1;
                                buf += "距離 :" + line.distance(pe1.mPoint);
                                break;
                            case EntityId.Line:
                                LineEntity le1 = (LineEntity)entity1;
                                if (line.angle(le1.mLine) < mEps)
                                    buf += "距離 :" + line.distance(le1.mLine);
                                else
                                    buf += "角度 : " + ylib.R2D(line.angle(le1.mLine));
                                break;
                            case EntityId.Polyline:
                                PolylineEntity ple1 = (PolylineEntity)entity1;
                                LineD polylineLine = ple1.mPolyline.nearLine(pickEnt[1].pos);
                                ip = polylineLine.intersection(line.ps);
                                if (line.angle(polylineLine) < mEps)
                                    buf += "距離 :" + ip.length(line.ps);
                                else
                                    buf += "角度 : " + ylib.R2D(line.angle(polylineLine));
                                break;
                            case EntityId.Polygon:
                                PolygonEntity pge1 = (PolygonEntity)entity1;
                                LineD polygonLine = pge1.mPolygon.nearLine(pickEnt[1].pos);
                                ip = polygonLine.intersection(line.ps);
                                if (line.angle(polygonLine) < mEps)
                                    buf += "距離 :" + ip.length(line.ps);
                                else
                                    buf += "角度 : " + ylib.R2D(line.angle(polygonLine));
                                break;
                        }
                        break;
                    case EntityId.Polyline:
                        PolylineEntity ple0 = (PolylineEntity)entity0;
                        LineD pline = ple0.mPolyline.nearLine(pickEnt[0].pos);
                        switch (entity1.mEntityId) {
                            case EntityId.Point:
                                PointEntity pe1 = (PointEntity)entity1;
                                buf += "距離 :" + pline.distance(pe1.mPoint);
                                break;
                            case EntityId.Line:
                                LineEntity le1 = (LineEntity)entity1;
                                if (pline.angle(le1.mLine) < mEps)
                                    buf += "距離 :" + pline.distance(le1.mLine);
                                else
                                    buf += "角度 : " + ylib.R2D(pline.angle(le1.mLine));
                                break;
                            case EntityId.Polyline:
                                PolylineEntity ple1 = (PolylineEntity)entity1;
                                LineD polylineLine = ple1.mPolyline.nearLine(pickEnt[1].pos);
                                ip = polylineLine.intersection(pline.ps);
                                if (pline.angle(polylineLine) < mEps)
                                    buf += "距離 :" + ip.length(pline.ps);
                                else
                                    buf += "角度 : " + ylib.R2D(pline.angle(polylineLine));
                                break;
                            case EntityId.Polygon:
                                PolygonEntity pge1 = (PolygonEntity)entity1;
                                LineD polygonLine = pge1.mPolygon.nearLine(pickEnt[1].pos);
                                ip = polygonLine.intersection(pline.ps);
                                if (pline.angle(polygonLine) < mEps)
                                    buf += "距離 :" + ip.length(pline.ps);
                                else
                                    buf += "角度 : " + ylib.R2D(pline.angle(polygonLine));
                                break;
                        }
                        break;
                    case EntityId.Polygon:
                        PolygonEntity pge0 = (PolygonEntity)entity0;
                        LineD pgLine = pge0.mPolygon.nearLine(pickEnt[0].pos);
                        switch (entity1.mEntityId) {
                            case EntityId.Point:
                                PointEntity pe1 = (PointEntity)entity1;
                                buf += "距離 :" + pgLine.distance(pe1.mPoint);
                                break;
                            case EntityId.Line:
                                LineEntity le1 = (LineEntity)entity1;
                                if (pgLine.angle(le1.mLine) < mEps)
                                    buf += "距離 :" + pgLine.distance(le1.mLine);
                                else
                                    buf += "角度 : " + ylib.R2D(pgLine.angle(le1.mLine));
                                break;
                            case EntityId.Polyline:
                                PolylineEntity ple1 = (PolylineEntity)entity1;
                                LineD polylineLine = ple1.mPolyline.nearLine(pickEnt[1].pos);
                                ip = polylineLine.intersection(pgLine.ps);
                                if (pgLine.angle(polylineLine) < mEps)
                                    buf += "距離 :" + ip.length(pgLine.ps);
                                else
                                    buf += "角度 : " + ylib.R2D(pgLine.angle(polylineLine));
                                break;
                            case EntityId.Polygon:
                                PolygonEntity pge1 = (PolygonEntity)entity1;
                                LineD polygonLine = pge1.mPolygon.nearLine(pickEnt[1].pos);
                                ip = polygonLine.intersection(pgLine.ps);
                                if (pgLine.angle(polygonLine) < mEps)
                                    buf += "距離 :" + ip.length(pgLine.ps);
                                else
                                    buf += "角度 : " + ylib.R2D(pgLine.angle(polygonLine));
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
                MessageBox.Show(buf, "距離測定");
            }
        }

        /// <summary>
        /// ピックした要素の情報表示
        /// </summary>
        public bool infoEntity(List<(int no, PointD pos)> pickEnt)
        {
            foreach ((int no, PointD pos) entNo in pickEnt) {
                MessageBox.Show(mEntityData.mEntityList[entNo.no].entityInfo(), "要素情報");
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
            mEntityData.mOperationCouunt++;

            foreach ((int no, PointD pos) entNo in pickEnt) {
                string propertyStr = mEntityData.mEntityList[entNo.no].toString();
                string dataStr = mEntityData.mEntityList[entNo.no].toDataString();
                InputBox dlg = new InputBox();
                dlg.Title = $"[{entNo.no}] {propertyStr}";
                dlg.mEditText = dataStr;
                if (dlg.ShowDialog() == true) {
                    dataStr = dlg.mEditText;
                    Entity ent = mEntityData.setStringEntityData(propertyStr, dataStr);
                    mEntityData.mEntityList.Add(ent);
                    mEntityData.mEntityList[mEntityData.mEntityList.Count - 1].mOperationCount = mEntityData.mOperationCouunt;
                    mEntityData.removeEnt(entNo.no);
                }
            }
            mEntityData.updateData();
            return true;
        }

        /// <summary>
        /// グリッドのサイズ(座標の丸め)の設定
        /// </summary>
        public void gridSet()
        {
            InputBox dlg = new InputBox();
            dlg.mMainWindow = mMainWindow;
            dlg.Title = "グリッドのサイズの設定";
            dlg.mEditText = Math.Abs(mGridSize).ToString();
            if (dlg.ShowDialog() == true) {
                mGridSize = ylib.string2double(dlg.mEditText);
            }
        }

        /// <summary>
        /// プロパティ値をEntityDataに設定
        /// </summary>
        public void setProperty()
        {
            mEntityData.mPointType = mPointType;
            mEntityData.mPointSize = mPointSize;
            mEntityData.mLineType = mLineType;
            mEntityData.mThickness = mThickness;
            mEntityData.mTextSize = mTextSize;
            mEntityData.mArrowSize = mArrowSize;
            mEntityData.mArrowAngle = mArrowAngle;
        }

        /// <summary>
        /// EntityDataからプロパティ値を取得
        /// </summary>
        public void getProperty()
        {
            mPointType = mEntityData.mPointType;
            mPointSize = mEntityData.mPointSize;
            mLineType = mEntityData.mLineType;
            mThickness = mEntityData.mThickness;
            mTextSize = mEntityData.mTextSize;
            mArrowSize = mEntityData.mArrowSize;
            mArrowAngle = mEntityData.mArrowAngle;
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
                getProperty();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 上書き保存
        /// </summary>
        public void saveFile(bool saveonly = false)
        {
            if (0 < mCurFilePath.Length) {
                if (0 < mCurFilePath.Length) {
                    if (mCurFilePath.IndexOf(".csv") < 0)
                        mCurFilePath = mCurFilePath + ".csv";
                    setProperty();
                    mEntityData.saveData(mCurFilePath);
                }
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
                setProperty();
                mEntityData.saveData(filePath);
                mCurFilePath = filePath;
            }
        }
    }
}
