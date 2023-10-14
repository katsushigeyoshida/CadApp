using CoreLib;
using System;
using System.Collections.Generic;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps;

namespace CadApp
{
    /// <summary>
    /// 要素データ表示クラス
    /// </summary>
    public class DataDrawing
    {
        public int mGridMinmumSize = 8;                            //  グリッドの最小表示スクリーンサイズ

        private BitmapSource mBitmapSource;                         //  画像データ(描画データのバッファリング)
        private Brush mDraggingColor = Brushes.Green;               //  ドラッギング時の色

        public DrawingPara mPara = new DrawingPara();

        public string mTextString = "";                             //  文字列データ
        public Box mCopyArea;
        public List<Entity> mCopyEntityList;

        private Canvas mCanvas;
        private MainWindow mMainWindow;
        private YWorldDraw ydraw;
        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="canvas">Canvas</param>
        /// <param name="mainWindow">親ウィンドウ</param>
        public DataDrawing(Canvas canvas, MainWindow mainWindow)
        {
            mCanvas = canvas;
            mMainWindow = mainWindow;
            ydraw = new YWorldDraw(canvas);
        }

        /// <summary>
        /// 表示画面の初期化
        /// </summary>
        /// <param name="dispArea"></param>
        public void initDraw(Box dispArea)
        {
            ydraw = new YWorldDraw(mCanvas);

            ydraw.setViewArea(0, 0, mCanvas.ActualWidth, mCanvas.ActualHeight);
            ydraw.mAspectFix = true;
            ydraw.mClipping = true;
            ydraw.setWorldWindow(dispArea);
        }

        /// <summary>
        /// 印刷処理
        /// </summary>
        /// <param name="entityData">要素データ</param>
        /// <param name="dispArea">表示領域</param>
        public void setPrint(EntityData entityData, Box dispArea, PageOrientation orient = PageOrientation.Landscape)
        {
            LocalPrintServer lps = new LocalPrintServer();
            PrintQueue queue = lps.DefaultPrintQueue;
            XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(queue);
            //  用紙サイズ
            PrintTicket ticket = queue.DefaultPrintTicket;
            ticket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);
            ticket.PageOrientation = orient;
            //  印刷領域
            var area = queue.GetPrintCapabilities().PageImageableArea;

            //  Canvasにデータを設定
            Canvas canvas = new Canvas();
            double width = area.ExtentWidth;
            double height = area.ExtentHeight;
            if (ticket.PageOrientation == PageOrientation.Landscape) {
                YLib.Swap(ref width, ref height);
            }

            initPrint(canvas, dispArea, 30, -30, width, height);
            ydraw.clear();
            entityData.drawingAll(ydraw);

            FixedPage page = new FixedPage();
            page.Children.Add(canvas);

            //  印刷実行
            writer.Write(page, ticket);
        }

        /// <summary>
        /// 印刷領域の設定
        /// </summary>
        /// <param name="canvas">Canvas</param>
        /// <param name="dispArea">作図領域</param>
        /// <param name="offsetWidth">印刷の位置</param>
        /// <param name="offsetHeight">印刷の位置</param>
        /// <param name="width">印刷幅</param>
        /// <param name="height">印刷高さ</param>
        public void initPrint(Canvas canvas, Box dispArea, double offsetWidth, double offsetHeight, double width, double height)
        {
            ydraw = new YWorldDraw(canvas);
            System.Diagnostics.Debug.WriteLine($"initPrint: {width} {height} {offsetHeight}");
            //  ViewとWorld領域を設定
            ydraw.setViewArea(offsetWidth, offsetHeight, width, height);
            ydraw.setWorldWindow(dispArea);
            ydraw.mAspectFix = true;
            ydraw.mClipping = true;
        }

        /// <summary>
        /// 要素データの表示
        /// </summary>
        /// <param name="entityData">要素データリスト</param>
        /// <param name="backColor">背景色</param>
        /// <param name="gridSize">グリッドサイズ</param>
        public void disp(EntityData entityData, Brush backColor, double gridSize)
        {
            if (entityData == null)
                return;
            ydraw.setBackColor(backColor);
            ydraw.clear();
            dispGrid(gridSize);
            entityData.drawingAll(ydraw);
            mBitmapSource = canvas2Bitmap(mCanvas);
            //  コピーしたイメージを貼り付けなおすことで文字のクリッピングする
            ydraw.clear();
            moveImage(mBitmapSource, 0, 0);
        }

        /// <summary>
        /// 領域指定の四角形のドラッギング
        /// </summary>
        /// <param name="loc">座標リスト</param>
        public void areaDragging(List<PointD> loc)
        {
            mCanvas.Children.Clear();
            mMainWindow.imScreen.Source = mBitmapSource;
            mCanvas.Children.Add(mMainWindow.imScreen);

            ydraw.mBrush = mDraggingColor;
            Box b = new Box(loc[0], loc[1]);
            List<PointD> plist = b.ToPointDList();
            ydraw.drawWPolygon(plist);
        }

        /// <summary>
        /// 要素のプロパティデータを取り込む
        /// </summary>
        /// <param name="ope"></param>
        public void setEntityProperty(CommandOpe ope)
        {
            mPara = ope.mPara.toCopy();
            mCopyArea = ope.mCopyArea;
            mCopyEntityList = ope.mCopyEntityList;
        }

        /// <summary>
        /// ドラッギング処理
        /// </summary>
        /// <param name="entityData">要素データリスト</param>
        /// <param name="points">ロケイト点リスト</param>
        /// <param name="pickList">ピック要素リスト</param>
        /// <param name="operation">操作</param>
        public void dragging(EntityData entityData, List<PointD> points, List<(int no, PointD pos)> pickList, OPERATION operation)
        {
            if (points.Count < 1 || operation == OPERATION.non)
                return;

            PartsD parts;
            ArcD arc;
            preDragging();
            switch (operation) {
                case OPERATION.createPoint:
                    ydraw.drawWPoint(points[0]);
                    return;
                case OPERATION.createLine:
                    if (1 < points.Count) {
                        ydraw.drawWLine(new LineD(points[0], points[1]));
                    }
                    break;
                case OPERATION.createHVLine:
                    {
                        PolylineD polyline = new PolylineD(points);
                        List<PointD> plist = polyline.toHVLine();
                        ydraw.drawWPolyline(plist);
                    }
                    break;
                case OPERATION.createRect:
                    if (1 < points.Count) {
                        Box b = new Box(points[0], points[1]);
                        List<PointD> plist = b.ToPointDList();
                        ydraw.drawWPolygon(plist);
                    }
                    break;
                case OPERATION.createPolyline:
                    ydraw.drawWPolyline(points);
                    break;
                case OPERATION.createPolygon:
                    ydraw.drawWPolygon(points, false);
                    break;
                case OPERATION.createArc:
                    if (points.Count == 2) {
                        ydraw.drawWLine(new LineD(points[0], points[1]));
                    } else if (points.Count == 3 && 0 < points[1].length(points[2])) {
                        arc = new ArcD(points[0], points[2], points[1]);
                        if (arc.mCp != null) {
                            ydraw.drawWArc(arc, false);
                        }
                    }
                    break;
                case OPERATION.createCircle:
                    if (1 < points.Count) {
                        ydraw.drawWCircle(points[0], points[0].length(points[1]), false);
                    }
                    break;
                case OPERATION.createEllipse:
                    if (1 < points.Count) {
                        EllipseD ellipse = new EllipseD(points[0], points[1]);
                        ydraw.drawWEllipse(ellipse);
                    }
                    break;
                case OPERATION.createTangentCircle:
                    arc = entityData.tangentCircle(pickList, points);
                    if (arc != null) {
                        ydraw.drawWArc(arc);
                    }
                    break;
                case OPERATION.createText:
                    TextD text = new TextD(mMainWindow.tbTextString.Text, points[0], mPara.mTextSize, 
                        mPara.mTextRotate, mPara.mHa, mPara.mVa, mPara.mLinePitchRate);
                    ydraw.drawWText(text);
                    break;
                case OPERATION.createArrow:
                    parts = new PartsD();
                    parts.mArrowSize = mPara.mArrowSize;
                    if (1 < points.Count) {
                        parts.createArrow(points[0], points[1]);
                        drawWParts(parts);
                    }
                    break;
                case OPERATION.createLabel:
                    parts = new PartsD();
                    parts.mTextSize = mPara.mTextSize;
                    parts.mArrowSize = mPara.mArrowSize;
                    if (1 < points.Count) {
                        parts.createLabel(points, mMainWindow.tbTextString.Text);
                        drawWParts(parts);
                    }
                    break;
                case OPERATION.createLocDimension:
                    locDimensionDragging(entityData, points, pickList);
                    break;
                case OPERATION.createLinearDimension:
                    dimensionDragging(entityData, points, pickList);
                    break;
                case OPERATION.createAngleDimension:
                    angleDimensionDragging(entityData, points, pickList);
                    break;
                case OPERATION.createDiameterDimension:
                    diameterDimensionDragging(entityData, points, pickList);
                    break;
                case OPERATION.createRadiusDimension:
                    radiusDimensionDragging(entityData, points, pickList);
                    break;
                case OPERATION.translate:
                case OPERATION.copyTranslate:
                    translateDragging(entityData, points, pickList);
                    break;
                case OPERATION.rotate:
                case OPERATION.copyRotate:
                    rotateDragging(entityData, points, pickList);
                    break;
                case OPERATION.mirror:
                case OPERATION.copyMirror:
                    mirrorDragging(entityData, points, pickList);
                    break;
                case OPERATION.scale:
                case OPERATION.copyScale:
                    scaleDragging(entityData, points, pickList);
                    break;
                case OPERATION.offset:
                case OPERATION.copyOffset:
                    offsetDragging(entityData, points, pickList);
                    break;
                case OPERATION.trim:
                case OPERATION.copyTrim:
                    trimDragging(entityData, points, pickList);
                    break;
                case OPERATION.divide:
                    break;
                case OPERATION.stretch:
                    stretchDragging(entityData, points, pickList);
                    break;
                case OPERATION.pasteEntity:
                    if (mCopyArea != null) {
                        Box b = new Box(points[0], mCopyArea.Size);
                        List<PointD> plist = b.ToPointDList();
                        ydraw.drawWPolygon(plist);
                    }
                    break;
                case OPERATION.createSymbol: {
                        if (mCopyEntityList != null && 0 < mCopyEntityList.Count) {
                            PointD vec = points[0] - mCopyEntityList[0].mArea.getCenter();
                            Entity ent = mCopyEntityList[0].toCopy();
                            ent.mColor = mDraggingColor;
                            ent.translate(vec);
                            ent.draw(ydraw);
                        }
                    }
                    break;
                case OPERATION.measureDistance:
                case OPERATION.measureAngle:
                    break;
                default:
                    return;
            }
            // ロケイト点表示
            ydraw.mPointType = 2;
            ydraw.mPointSize = 3;
            for (int i = 0; i < points.Count; i++) {
                ydraw.drawWPoint(points[i]);
            }
        }

        /// <summary>
        /// ドラッギング表示の前処理
        /// </summary>
        private void preDragging()
        {
            mCanvas.Children.Clear();
            mMainWindow.imScreen.Source = mBitmapSource;
            mCanvas.Children.Add(mMainWindow.imScreen);
            ydraw.mBrush = mDraggingColor;
            ydraw.mTextColor = mDraggingColor;
            ydraw.mThickness = mPara.mThickness;
            ydraw.mLineType = mPara.mLineType;
            ydraw.mPointType = mPara.mPointType;
            ydraw.mPointSize = mPara.mPointSize;
        }

        /// <summary>
        /// 位置寸法線のドラッギング表示
        /// </summary>
        /// <param name="entityData">要素データリスト</param>
        /// <param name="loc">ロケイト点リスト</param>
        /// <param name="pickList">ピック要素リスト</param>
        private void locDimensionDragging(EntityData entityData, List<PointD> loc, List<(int no, PointD pos)> pickList)
        {
            if (loc.Count == 2) {
                ydraw.drawWLine(new LineD(loc[0], loc[1]));
            } else if (loc.Count == 3) {
                PartsD parts = new PartsD();
                parts.mTextSize = mPara.mTextSize;
                parts.mArrowSize = mPara.mArrowSize;
                parts.createDimension(loc);
                drawWParts(parts);
            }
        }

        /// <summary>
        /// 寸法線のドラッギング表示
        /// </summary>
        /// <param name="entityData">要素データリスト</param>
        /// <param name="loc">ロケイト点リスト</param>
        /// <param name="pickList">ピック要素リスト</param>
        private void dimensionDragging(EntityData entityData, List<PointD> loc, List<(int no, PointD pos)> pickList)
        {
            if (0 < loc.Count && 1 < pickList.Count) {
                Entity ent0 = entityData.mEntityList[pickList[0].no];
                Entity ent1 = entityData.mEntityList[pickList[1].no];
                PointD ps = ent0.getEndPoint(pickList[0].pos);
                PointD pe = ent1.getEndPoint(pickList[1].pos);
                List<PointD> plist = new List<PointD>() {
                            ps, pe, loc[0]
                        };
                PartsD parts = new PartsD();
                parts.mTextSize = mPara.mTextSize;
                parts.mArrowSize = mPara.mArrowSize;
                parts.createDimension(plist);
                drawWParts(parts);
            }
        }

        /// <summary>
        /// 角度寸法線のドラッギング表示
        /// </summary>
        /// <param name="entityData">要素データリスト</param>
        /// <param name="loc">ロケイト点リスト</param>
        /// <param name="pickList">ピック要素リスト</param>
        private void angleDimensionDragging(EntityData entityData, List<PointD> loc, List<(int no, PointD pos)> pickList)
        {
            if (0 < loc.Count && 1 < pickList.Count) {
                Entity ent0 = entityData.mEntityList[pickList[0].no];
                Entity ent1 = entityData.mEntityList[pickList[1].no];
                PointD cp = ent0.getLine(pickList[0].pos).intersection(ent1.getLine(pickList[1].pos));
                PointD ps = ent0.getEndPoint(pickList[0].pos, cp);
                PointD pe = ent1.getEndPoint(pickList[1].pos, cp);
                if (!cp.isNaN()) {
                    PartsEntity partsEnt = new PartsEntity();
                    partsEnt.mParts = new PartsD();
                    List<PointD> plist = new List<PointD>() { cp, ps, pe, loc[0] };
                    PartsD parts = new PartsD();
                    parts.mTextSize = mPara.mTextSize;
                    parts.mArrowSize = mPara.mArrowSize;
                    parts.createAngleDimension(plist);
                    drawWParts(parts);
                }
            }
        }

        /// <summary>
        /// 直径寸法線のドラッギング表示
        /// </summary>
        /// <param name="entityData">要素データリスト</param>
        /// <param name="loc">ロケイト点リスト</param>
        /// <param name="pickList">ピック要素リスト</param>
        private void diameterDimensionDragging(EntityData entityData, List<PointD> loc, List<(int no, PointD pos)> pickList)
        {
            if (0 < loc.Count && 0 < pickList.Count) {
                Entity ent = entityData.mEntityList[pickList[0].no];
                if (ent.mEntityId == EntityId.Arc) {
                    ArcEntity arcEnt = (ArcEntity)ent;
                    PartsD parts = new PartsD();
                    parts.mTextSize = mPara.mTextSize;
                    parts.mArrowSize = mPara.mArrowSize;
                    parts.createDiameterDimension(arcEnt.mArc, loc);
                    drawWParts(parts);
                }
            }
        }

        /// <summary>
        /// 半径寸法線のドラッギング表示
        /// </summary>
        /// <param name="entityData">要素データリスト</param>
        /// <param name="loc">ロケイト点リスト</param>
        /// <param name="pickList">ピック要素リスト</param>
        private void radiusDimensionDragging(EntityData entityData, List<PointD> loc, List<(int no, PointD pos)> pickList)
        {
            if (0 < loc.Count && 0 < pickList.Count) {
                Entity ent = entityData.mEntityList[pickList[0].no];
                if (ent.mEntityId == EntityId.Arc) {
                    ArcEntity arcEnt = (ArcEntity)ent;
                    PartsD parts = new PartsD();
                    parts.mTextSize = mPara.mTextSize;
                    parts.mArrowSize = mPara.mArrowSize;
                    parts.createRadiusDimension(arcEnt.mArc, loc);
                    drawWParts(parts);
                }
            }
        }


        /// <summary>
        /// 移動ドラッギング
        /// </summary>
        /// <param name="entityData">要素データリスト</param>
        /// <param name="loc">ロケイト点リスト</param>
        /// <param name="pickList">ピック要素リスト</param>
        private void translateDragging(EntityData entityData, List<PointD> loc, List<(int no, PointD pos)> pickList)
        {
            if (loc.Count < 2) return;

            ydraw.mBrush = mDraggingColor;
            for (int i = 1; i < loc.Count; i++) {
                PointD vec = loc[0].vector(loc[i]);

                foreach ((int no, PointD pos) pickEnt in pickList) {
                    if (pickEnt.no < entityData.mEntityList.Count) {
                        Entity ent = entityData.mEntityList[pickEnt.no].toCopy();
                        ent.mColor = mDraggingColor;
                        ent.translate(vec);
                        ent.draw(ydraw);
                    }
                }
            }
        }

        /// <summary>
        /// 回転ドラッギング
        /// </summary>
        /// <param name="entityData">要素データリスト</param>
        /// <param name="loc">ロケイト点リスト</param>
        /// <param name="pickList">ピック要素リスト</param>
        private void rotateDragging(EntityData entityData, List<PointD> loc, List<(int no, PointD pos)> pickList)
        {
            if (loc.Count < 2) return;

            ydraw.mBrush = mDraggingColor;
            for (int i = 1; i< loc.Count; i++) {
                foreach ((int no, PointD pos) pickEnt in pickList) {
                    if (pickEnt.no < entityData.mEntityList.Count) {
                        Entity ent = entityData.mEntityList[pickEnt.no].toCopy();
                        ent.mColor = mDraggingColor;
                        ent.rotate(loc[0], loc[i]);
                        ent.draw(ydraw);
                    }
                }
            }
        }

        /// <summary>
        /// ミラードラッギング
        /// </summary>
        /// <param name="entityData">要素データリスト</param>
        /// <param name="loc">ロケイト点リスト</param>
        /// <param name="pickList">ピック要素リスト</param>
        private void mirrorDragging(EntityData entityData, List<PointD> loc, List<(int no, PointD pos)> pickList)
        {
            if (loc.Count < 2) return;

            ydraw.mBrush = mDraggingColor;

            foreach ((int no, PointD pos) pickEnt in pickList) {
                if (pickEnt.no < entityData.mEntityList.Count) {
                    Entity ent = entityData.mEntityList[pickEnt.no].toCopy();
                    ent.mColor = mDraggingColor;
                    ent.mirror(loc[0], loc[1]);
                    ent.draw(ydraw);
                }
            }
        }

        /// <summary>
        /// 拡大縮小ドラッギング
        /// </summary>
        /// <param name="entityData">要素データリスト</param>
        /// <param name="loc">ロケイト点リスト</param>
        /// <param name="pickList">ピック要素リスト</param>
        private void scaleDragging(EntityData entityData, List<PointD> loc, List<(int no, PointD pos)> pickList)
        {
            if (loc.Count < 3) return;

            ydraw.mBrush = mDraggingColor;
            double scale = loc[0].length(loc[2]) / loc[0].length(loc[1]);

            foreach ((int no, PointD pos) pickEnt in pickList) {
                if (pickEnt.no < entityData.mEntityList.Count) {
                    Entity ent = entityData.mEntityList[pickEnt.no].toCopy();
                    ent.mColor = mDraggingColor;
                    ent.scale(loc[0], scale);
                    ent.draw(ydraw);
                }
            }
        }

        /// <summary>
        /// オフセットドラッギング
        /// </summary>
        /// <param name="entityData">要素データリスト</param>
        /// <param name="loc">ロケイト点リスト</param>
        /// <param name="pickList">ピック要素リスト</param>
        private void offsetDragging(EntityData entityData, List<PointD> loc, List<(int no, PointD pos)> pickList)
        {
            if (loc.Count < 2) return;

            ydraw.mBrush = mDraggingColor;

            foreach ((int no, PointD pos) pickEnt in pickList) {
                if (pickEnt.no < entityData.mEntityList.Count) {
                    Entity ent = entityData.mEntityList[pickEnt.no].toCopy();
                    ent.mColor = mDraggingColor;
                    ent.offset(loc[0], loc[1]);
                    ent.draw(ydraw);
                }
            }
        }

        /// <summary>
        /// トリムドラッギング
        /// </summary>
        /// <param name="entityData">要素データリスト</param>
        /// <param name="loc">ロケイト点リスト</param>
        /// <param name="pickList">ピック要素リスト</param>
        private void trimDragging(EntityData entityData, List<PointD> loc, List<(int no, PointD pos)> pickList)
        {
            if (loc.Count < 2) return;

            ydraw.mBrush = mDraggingColor;

            foreach ((int no, PointD pos) pickEnt in pickList) {
                if (pickEnt.no < entityData.mEntityList.Count) {
                    Entity ent = entityData.mEntityList[pickEnt.no].toCopy();
                    ent.mColor = mDraggingColor;
                    ent.trim(loc[0], loc[1]);
                    ent.draw(ydraw);
                }
            }
        }

        /// <summary>
        /// ストレッチのドラッギング
        /// </summary>
        /// <param name="entityData">要素データリスト</param>
        /// <param name="loc">ロケイト点リスト</param>
        /// <param name="pickList">ピック要素リスト</param>
        private void stretchDragging(EntityData entityData, List<PointD> loc, List<(int no, PointD pos)> pickList)
        {
            if (loc.Count < 2) return;

            ydraw.mBrush = mDraggingColor;
            PointD vec = loc[0].vector(loc[loc.Count - 1]);

            foreach ((int no, PointD pos) pickEnt in pickList) {
                if (pickEnt.no < entityData.mEntityList.Count) {
                    Entity ent = entityData.mEntityList[pickEnt.no].toCopy();
                    ent.mColor = mDraggingColor;
                    ent.stretch(vec, pickEnt.pos);
                    ent.draw(ydraw);
                }
            }
        }


        /// <summary>
        /// Partsの表示
        /// </summary>
        /// <param name="parts"></param>
        private void drawWParts(PartsD parts)
        {
            if (parts.mLines != null) {
                foreach (var line in parts.mLines)
                    ydraw.drawWLine(line);
            }
            if (parts.mArcs != null) {
                foreach (var arc in parts.mArcs)
                    ydraw.drawWArc(arc, false);
            }
            if (parts.mTexts != null) {
                foreach (var text in parts.mTexts)
                    ydraw.drawWText(text);
            }
        }

        /// <summary>
        /// 要素をピックした時ピック色にして表示
        /// </summary>
        /// <param name="entNo"></param>
        public void pickDisp(EntityData entityData, List<(int, PointD)> pickList)
        {
            mMainWindow.imScreen.Source = mBitmapSource;
            mCanvas.Children.Clear();
            mCanvas.Children.Add(mMainWindow.imScreen);
            foreach ((int no, PointD pos) entNo in pickList) {
                if (entNo.no < entityData.mEntityList.Count) {
                    Entity ent = entityData.mEntityList[entNo.no];
                    ent.mPick = true;
                    ent.draw(ydraw);
                    ent.mPick = false;
                }
            }
        }

        /// <summary>
        /// 画面スクロール
        /// スクロールはビットマップを移動する形で移動によりできた空白部分だけを再描画する
        /// </summary>
        /// <param name="dx">移動量X(screen)</param>
        /// <param name="dy">移動量Y(screen)</param>
        public void scroll(EntityData entityData, double dx, double dy, double gridSize)
        {
            //  全体再描画
            //PointD v = new PointD(ydraw.screen2worldXlength(dx), ydraw.screen2worldYlength(dy));
            //ydraw.mWorld.offset(v.inverse());
            //disp();

            //  状態を保存
            PointD v = new PointD(ydraw.screen2worldXlength(dx), ydraw.screen2worldYlength(dy));
            ydraw.mWorld.offset(v.inverse());

            ydraw.clear();
            //  移動した位置にBitmapの貼付け
            moveImage(mBitmapSource, dx, dy);

            //  横空白部分を描画
            ydraw.mClipBox = ydraw.mWorld.toCopy();
            if (0 > dx) {
                ydraw.mClipBox.Left = ydraw.mWorld.Right + v.x;
                ydraw.mClipBox.Width = -v.x;
            } else if (0 < dx) {
                ydraw.mClipBox.Width = v.x;
            }
            if (dx != 0) {
                dispGrid(gridSize);
                entityData.drawingAll(ydraw);
            }

            //  縦空白部分を描画
            ydraw.mClipBox = ydraw.mWorld.toCopy();
            if (0 > dy) {
                ydraw.mClipBox.Top -= ydraw.mWorld.Height - v.y;
                ydraw.mClipBox.Height = v.y;
            } else if (0 < dy) {
                ydraw.mClipBox.Height = -v.y;
            }
            if (dy != 0) {
                dispGrid(gridSize);
                entityData.drawingAll(ydraw);
            }

            //  Windowの設定を元に戻す
            ydraw.mClipBox = ydraw.mWorld.toCopy();
            mBitmapSource = canvas2Bitmap(mCanvas);

            //  コピーしたイメージを貼り付けなおすことで文字のクリッピングする
            ydraw.clear();
            moveImage(mBitmapSource, 0, 0);
        }

        /// <summary>
        /// グリッドの表示
        /// グリッド10個おきに大玉を表示
        /// </summary>
        /// <param name="size">グリッドの間隔</param>
        public void dispGrid(double size)
        {
            if (0 < size && size < 1000) {
                ydraw.mBrush = ydraw.getColor("Black");
                ydraw.mThickness = 1.0;
                ydraw.mPointType = 0;
                while (mGridMinmumSize > ydraw.world2screenXlength(size) && size < 1000) {
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
                                //  10個おきの点
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
        /// Bitmap 図形を移動させる
        /// </summary>
        /// <param name="bitmapSource">Bitmap</param>
        /// <param name="dx">移動量</param>
        /// <param name="dy">移動量</param>
        private void moveImage(BitmapSource bitmapSource, double dx, double dy)
        {
            System.Drawing.Bitmap bitmap = ylib.cnvBitmapSource2Bitmap(mBitmapSource);
            double width = bitmap.Width - Math.Abs(dx);
            double height = bitmap.Height - Math.Abs(dy);
            Point sp = new Point(dx > 0 ? 0 : -dx, dy > 0 ? 0 : -dy);
            Point ep = new Point(sp.X + width, sp.Y + height);
            System.Drawing.Bitmap moveBitmap = ylib.trimingBitmap(bitmap, sp, ep);
            BitmapImage bitmapImage = ylib.cnvBitmap2BitmapImage(moveBitmap);
            ylib.setCanvasBitmapImage(mCanvas, bitmapImage, dx > 0 ? dx : 0, dy > 0 ? dy : 0, width, height);
        }

        /// <summary>
        /// 指定した座標を中心にしてスケーリングする
        /// </summary>
        /// <param name="wp">スケーリングの中心</param>
        /// <param name="zoom">倍率</param>
        /// <param name="inverse">反転</param>
        public void setWorldZoom(PointD wp, double zoom, bool inverse = false)
        {
            ydraw.setWorldZoom(wp, zoom, inverse);
        }

        /// <summary>
        /// 領域の中心からスケーリングする
        /// </summary>
        /// <param name="zoom">倍率</param>
        public void setWorldZoom(double zoom)
        {
            ydraw.setWorldZoom(zoom);
        }

        /// <summary>
        /// スクリーン座標を論理座標(ワールド座標)に変換
        /// </summary>
        /// <param name="sp">スクリーン座標</param>
        /// <returns>ワールド座標</returns>
        public PointD cnvScreen2World(PointD sp)
        {
            return ydraw.cnvScreen2World(sp);
        }

        /// <summary>
        /// ワールド領域の取得
        /// </summary>
        /// <returns>ワールド領域</returns>
        public Box getWorldArea()
        {
            return ydraw.mWorld;
        }

        /// <summary>
        /// 論理座標(ワールド座標)をスクリーン座標に変換
        /// </summary>
        /// <param name="wp">ワールド座標</param>
        /// <returns>スクリーン座標</returns>
        public PointD cnvWorld2Screen(PointD wp)
        {
            return ydraw.cnvWorld2Screen(wp);
        }

        /// <summary>
        /// スクリーン座標のX方向長さをワールド座標の長さに変換
        /// </summary>
        /// <param name="x">スクリーン座標の長さ</param>
        /// <returns>ワールド座標の長さ</returns>
        public double screen2worldXlength(double x)
        {
            return ydraw.screen2worldXlength(x);
        }

        /// <summary>
        /// ワールド座標のX方向長さをスクリーン座標の長さに変換
        /// </summary>
        /// <param name="x">ワールド座標の長さ</param>
        /// <returns>スクリーン座標の長さ</returns>
        public double world2creenXlength(double x)
        {
            return ydraw.world2screenXlength(x);
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
            Point preLoc = new Point(mMainWindow.lbCommand.ActualWidth + 10, 0);
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
