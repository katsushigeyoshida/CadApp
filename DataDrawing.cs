﻿using CoreLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CadApp
{
    /// <summary>
    /// 要素データ表示クラス
    /// </summary>
    public class DataDrawing
    {
        public double mGridSize = 1.0;                              //  マウス座標の丸め値
        public int mGridMinmumSize = 10;                            //  グリッドの最小表示スクリーンサイズ

        private BitmapSource mBitmapSource;                         //  画像データ(描画データのバッファリング)
        private Brush mDraggingColor = Brushes.Green;               //  ドラッギング時の色

        public int mPointType = 0;                                  //  点種
        public int mLineType = 0;                                   //  線種
        public double mEntSize = 1;                                 //  線の太さ
        public double mPointSize = 1;                               //  点の大きさ
        public double mTextSize = 12;                               //  文字サイズ
        public double mTextRotate = 0;                              //  文字列の回転角
        public string mTextString = "";                             //  文字列データ
        public HorizontalAlignment mHa = HorizontalAlignment.Left;  //  水平アライメント
        public VerticalAlignment mVa = VerticalAlignment.Top;       //  垂直アライメント
        public double mArrowSize = 6;                               //  矢印の大きさ
        public Brush mCreateColor = Brushes.Black;                  //  要素の色

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
            ydraw = new YWorldDraw(canvas);
            mCanvas = canvas;
            mMainWindow = mainWindow;
        }

        /// <summary>
        /// 表示画面の初期化
        /// </summary>
        /// <param name="dispArea"></param>
        public void initDraw(Box dispArea)
        {
            ydraw.setViewArea(0, 0, mCanvas.ActualWidth, mCanvas.ActualHeight);
            ydraw.mAspectFix = true;
            ydraw.mClipping = true;
            ydraw.setWorldWindow(dispArea);
            System.Diagnostics.Debug.WriteLine($"View: {ydraw.mView}");
            System.Diagnostics.Debug.WriteLine($"World: {ydraw.mWorld.ToString("f2")}");
        }

        /// <summary>
        /// 要素データの表示
        /// </summary>
        /// <param name="etityData">要素データリスト</param>
        /// <param name="backColor">背景色</param>
        /// <param name="gridSize">グリッドサイズ</param>
        public void disp(EntityData etityData, Brush backColor, double gridSize)
        {
            if (etityData == null)
                return;
            ydraw.setBackColor(backColor);
            ydraw.clear();
            dispGrid(gridSize);
            foreach (var entity in etityData.mEntityList) {
                if (!entity.mRemove && entity.mEntityId != EntityId.Non)
                    entity.draw(ydraw);
            }
            mBitmapSource = canvas2Bitmap(mCanvas);
        }

        /// <summary>
        /// 要素プロパティの設定
        /// </summary>
        /// <param name="createColor">色</param>
        /// <param name="pointType">点種</param>
        /// <param name="pointSize">点のサイズ</param>
        /// <param name="lineType">線種</param>
        /// <param name="thickness">線の太さ</param>
        public void setEntityProperty(Brush createColor, int pointType, double pointSize,
            int lineType, double thickness, double textSize, double arrowSize)
        {
            mCreateColor = createColor;
            mPointType = pointType;
            mPointSize = pointSize;
            mLineType = lineType;
            mEntSize = thickness;
            mTextSize = textSize;
            mArrowSize = arrowSize;
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
            mCanvas.Children.Clear();
            mMainWindow.imScreen.Source = mBitmapSource;
            mCanvas.Children.Add(mMainWindow.imScreen);
            //ylib.setCanvasBitmapImage(cvCanvas, ylib.bitmapSource2BitmapImage(mBitmapSource),0 , 0, mBitmapSource.Width, mBitmapSource.Height);
            ydraw.mBrush     = mCreateColor;
            ydraw.mTextColor = mCreateColor;
            ydraw.mThickness = mEntSize;
            ydraw.mLineType  = mLineType;
            ydraw.mPointType = mPointType;
            ydraw.mPointSize = mPointSize;
            PartsD parts;
            switch (operation) {
                case OPERATION.createPoint:
                    ydraw.drawWPoint(points[0]);
                    break;
                case OPERATION.createLine:
                    ydraw.drawWLine(points[0], points[1]);
                    break;
                case OPERATION.createRect: {
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
                        ydraw.drawWLine(points[0], points[1]);
                    } else if (points.Count == 3 && 0 < points[1].length(points[2])) {
                        ArcD arc = new ArcD(points[0], points[1], points[2]);
                        ydraw.drawWArc(arc, false);
                    }
                    break;
                case OPERATION.createCircle:
                    ydraw.drawWCircle(points[0], points[0].length(points[1]));
                    break;
                case OPERATION.createText:
                    TextD text = new TextD(mMainWindow.tbTextString.Text, points[0], mTextSize, mTextRotate, mHa, mVa);
                    ydraw.drawWText(text);
                    break;
                case OPERATION.createArrow:
                    parts = new PartsD();
                    parts.mArrowSize = mArrowSize;
                    parts.createArrow(points[0], points[1]);
                    drawWParts(parts);
                    break;
                case OPERATION.createLabel:
                    parts = new PartsD();
                    parts.mTextSize = mTextSize;
                    parts.mArrowSize = mArrowSize;
                    parts.createLabel(points[0], points[1], mMainWindow.tbTextString.Text);
                    drawWParts(parts);
                    break;
                case OPERATION.createLocDimension:
                    if (points.Count == 2) {
                        ydraw.drawWLine(points[0], points[1]);
                    } else if (points.Count == 3) {
                        parts = new PartsD();
                        parts.mTextSize = mTextSize;
                        parts.mArrowSize = mArrowSize;
                        parts.createDimension(points);
                        drawWParts(parts);
                    }
                    break;
                case OPERATION.createAngleDimension:
                    if (0 < points.Count && 1 < pickList.Count) {
                        Entity ent0 = entityData.mEntityList[pickList[0].no];
                        Entity ent1 = entityData.mEntityList[pickList[1].no];
                        PointD cp = entityData.getLine(ent0, pickList[0].pos).intersection(entityData.getLine(ent1, pickList[1].pos));
                        PointD ps = entityData.getEndPoint(ent0, pickList[0].pos, cp);
                        PointD pe = entityData.getEndPoint(ent1, pickList[1].pos, cp);
                        if (!cp.isNaN()) {
                            PartsEntity partsEnt = new PartsEntity();
                            partsEnt.mParts = new PartsD();
                            List<PointD> plist = new List<PointD>() { cp, ps, pe, points[0] };
                            parts = new PartsD();
                            parts.mTextSize = mTextSize;
                            parts.mArrowSize = mArrowSize;
                            parts.createAngleDimension(plist);
                            drawWParts(parts);
                        }
                    }
                    break;
                case OPERATION.createDimension:
                    if (0 < points.Count && 1 < pickList.Count) {
                        Entity ent0 = entityData.mEntityList[pickList[0].no];
                        Entity ent1 = entityData.mEntityList[pickList[1].no];
                        PointD ps = entityData.getEndPoint(ent0, pickList[0].pos);
                        PointD pe = entityData.getEndPoint(ent1, pickList[1].pos);
                        List<PointD> plist = new List<PointD>() {
                            ps, pe, points[0]
                        };
                        parts = new PartsD();
                        parts.mTextSize = mTextSize;
                        parts.mArrowSize = mArrowSize;
                        parts.createDimension(plist);
                        drawWParts(parts);
                    }
                    break;
                case OPERATION.createDiameterDimension:
                    if (0 < points.Count && 0 < pickList.Count) {
                        Entity ent = entityData.mEntityList[pickList[0].no];
                        if (ent.mEntityId == EntityId.Arc) {
                            ArcEntity arcEnt = (ArcEntity)ent;
                            parts = new PartsD();
                            parts.mTextSize = mTextSize;
                            parts.mArrowSize = mArrowSize;
                            parts.createDiameterDimension(arcEnt.mArc, points);
                            drawWParts(parts);
                        }
                    }
                    break;
                case OPERATION.createRadiusDimension:
                    if (0 < points.Count && 0 < pickList.Count) {
                        Entity ent = entityData.mEntityList[pickList[0].no];
                        if (ent.mEntityId == EntityId.Arc) {
                            ArcEntity arcEnt = (ArcEntity)ent;
                            parts = new PartsD();
                            parts.mTextSize = mTextSize;
                            parts.mArrowSize = mArrowSize;
                            parts.createRadiusDimension(arcEnt.mArc, points);
                            drawWParts(parts);
                        }
                    }
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
                case OPERATION.offset:
                case OPERATION.copyOffset:
                    offsetDragging(entityData, points, pickList);
                    break;
                case OPERATION.trim:
                    trimDragging(entityData, points, pickList);
                    break;
                case OPERATION.divide:
                    break;
                case OPERATION.stretch:
                    stretchDragging(entityData, points, pickList);
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
        /// <param name="entityData">要素データリスト</param>
        /// <param name="loc">ロケイト点リスト</param>
        /// <param name="pickList">ピック要素リスト</param>
        private void translateDragging(EntityData entityData, List<PointD> loc, List<(int no, PointD pos)> pickList)
        {
            if (loc.Count < 2) return;

            ydraw.mBrush = mDraggingColor;
            PointD vec = loc[0].vector(loc[loc.Count - 1]);

            foreach ((int, PointD) entNo in pickList) {
                Entity ent = entityData.mEntityList[entNo.Item1];
                //ydraw.mBrush = ent.mColor;
                ydraw.mThickness = ent.mThickness;
                switch (ent.mEntityId) {
                    case EntityId.Point:
                        PointEntity pointEnt = (PointEntity)ent;
                        PointD point = pointEnt.mPoint.toCopy();
                        point.offset(vec);
                        ydraw.mPointType = pointEnt.mType;
                        ydraw.mPointSize = pointEnt.mThickness;
                        ydraw.drawWPoint(point);
                        break;
                    case EntityId.Line:
                        LineEntity lineEnt = (LineEntity)ent;
                        LineD line = lineEnt.mLine.toCopy();
                        line.translate(vec);
                        ydraw.drawWLine(line);
                        break;
                    case EntityId.Polyline:
                        PolylineEntity polylineEnt = (PolylineEntity)ent;
                        PolylineD polyline = polylineEnt.mPolyline.toCopy();
                        polyline.translate(vec);
                        ydraw.drawWPolyline(polyline);
                        break;
                    case EntityId.Polygon:
                        PolygonEntity polygonEnt = (PolygonEntity)ent;
                        PolygonD polygon = polygonEnt.mPolygon.toCopy();
                        polygon.translate(vec);
                        ydraw.drawWPolygon(polygon);
                        break;
                    case EntityId.Arc:
                        ArcEntity arcEnt = (ArcEntity)ent;
                        ArcD arc = arcEnt.mArc.toCopy();
                        arc.mCp.offset(vec);
                        ydraw.drawWArc(arc, false);
                        break;
                    case EntityId.Text:
                        TextEntity textEnt = (TextEntity)ent;
                        TextD text = textEnt.mText.toCopy();
                        text.mPos.offset(vec);
                        ydraw.drawWText(text);
                        break;
                    case EntityId.Parts:
                        PartsEntity partsEnt = (PartsEntity)ent;
                        PartsD parts = partsEnt.mParts.toCopy();
                        parts.translate(vec);
                        drawWParts(parts);
                        break;
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

            foreach ((int, PointD) entNo in pickList) {
                Entity ent = entityData.mEntityList[entNo.Item1];
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
                    case EntityId.Polyline:
                        PolylineEntity polylineEnt = (PolylineEntity)ent;
                        PolylineD polyline = polylineEnt.mPolyline.toCopy();
                        polyline.rotate(loc[0], loc[1]);
                        ydraw.drawWPolyline(polyline);
                        break;
                    case EntityId.Polygon:
                        PolygonEntity polygonEnt = (PolygonEntity)ent;
                        PolygonD polygon = polygonEnt.mPolygon.toCopy();
                        polygon.rotate(loc[0], loc[1]);
                        ydraw.drawWPolygon(polygon);
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
                    case EntityId.Parts:
                        PartsEntity partsEnt = (PartsEntity)ent;
                        PartsD parts = partsEnt.mParts.toCopy();
                        parts.rotate(loc[0], loc[1]);
                        drawWParts(parts);
                        break;
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

            foreach ((int, PointD) entNo in pickList) {
                Entity ent = entityData.mEntityList[entNo.Item1];
                //ydraw.mBrush = ent.mColor;
                ydraw.mThickness = ent.mThickness;
                switch (ent.mEntityId) {
                    case EntityId.Point:
                        PointEntity pointEnt = (PointEntity)ent;
                        PointD point = pointEnt.mPoint.toCopy();
                        point.mirror(loc[0], loc[1]);
                        ydraw.mPointType = pointEnt.mType;
                        ydraw.mPointSize = pointEnt.mThickness;
                        ydraw.drawWPoint(point);
                        break;
                    case EntityId.Line:
                        LineEntity lineEnt = (LineEntity)ent;
                        LineD line = lineEnt.mLine.toCopy();
                        line.mirror(loc[0], loc[1]); ;
                        ydraw.drawWLine(line);
                        break;
                    case EntityId.Polyline:
                        PolylineEntity polylineEnt = (PolylineEntity)ent;
                        PolylineD polyline = polylineEnt.mPolyline.toCopy();
                        polyline.mirror(loc[0], loc[1]);
                        ydraw.drawWPolyline(polyline);
                        break;
                    case EntityId.Polygon:
                        PolygonEntity polygonEnt = (PolygonEntity)ent;
                        PolygonD polygon = polygonEnt.mPolygon.toCopy();
                        polygon.mirror(loc[0], loc[1]);
                        ydraw.drawWPolygon(polygon);
                        break;
                    case EntityId.Arc:
                        ArcEntity arcEnt = (ArcEntity)ent;
                        ArcD arc = arcEnt.mArc.toCopy();
                        arc.mirror(loc[0], loc[1]);
                        ydraw.drawWArc(arc, false);
                        break;
                    case EntityId.Text:
                        TextEntity textEnt = (TextEntity)ent;
                        TextD text = textEnt.mText.toCopy();
                        text.mirror(loc[0], loc[1]);
                        ydraw.drawWText(text);
                        break;
                    case EntityId.Parts:
                        PartsEntity partsEnt = (PartsEntity)ent;
                        PartsD parts = partsEnt.mParts.toCopy();
                        parts.mirror(loc[0], loc[1]);
                        drawWParts(parts);
                        break;
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

            foreach ((int, PointD) entNo in pickList) {
                Entity ent = entityData.mEntityList[entNo.Item1];
                ydraw.mThickness = ent.mThickness;
                switch (ent.mEntityId) {
                    case EntityId.Point:
                        PointEntity pointEnt = (PointEntity)ent;
                        PointD point = pointEnt.mPoint.toCopy();
                        point.translate(loc[0], loc[1]);
                        ydraw.mPointType = pointEnt.mType;
                        ydraw.mPointSize = pointEnt.mThickness;
                        ydraw.drawWPoint(point);
                        break;
                    case EntityId.Line:
                        LineEntity lineEnt = (LineEntity)ent;
                        LineD line = lineEnt.mLine.toCopy();
                        line.offset(loc[0], loc[1]);
                        ydraw.drawWLine(line);
                        break;
                    case EntityId.Polyline:
                        PolylineEntity polylineEnt = (PolylineEntity)ent;
                        PolylineD polyline = polylineEnt.mPolyline.toCopy();
                        polyline.offset(loc[0], loc[1]);
                        ydraw.drawWPolyline(polyline);
                        break;
                    case EntityId.Polygon:
                        PolygonEntity polygonEnt = (PolygonEntity)ent;
                        PolygonD polygon = polygonEnt.mPolygon.toCopy();
                        polygon.offset(loc[0], loc[1]);
                        ydraw.drawWPolygon(polygon);
                        break;
                    case EntityId.Arc:
                        ArcEntity arcEnt = (ArcEntity)ent;
                        ArcD arc = arcEnt.mArc.toCopy();
                        arc.offset(loc[0], loc[1]);
                        ydraw.drawWArc(arc, false);
                        break;
                    case EntityId.Text:
                        //TextEntity textEnt = (TextEntity)ent;
                        //TextD text = textEnt.mText.toCopy();
                        //text.mirror(loc[0], loc[1]);
                        //ydraw.drawWText(text);
                        break;
                    case EntityId.Parts:
                        //PartsEntity partsEnt = (PartsEntity)ent;
                        //PartsD parts = partsEnt.mParts.toCopy();
                        //parts.mirror(loc[0], loc[1]);
                        //drawWParts(parts);
                        break;
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

            foreach ((int, PointD) entNo in pickList) {
                Entity ent = entityData.mEntityList[entNo.Item1];
                //ydraw.mBrush = ent.mColor;
                ydraw.mThickness = ent.mThickness;
                switch (ent.mEntityId) {
                    case EntityId.Point:
                        break;
                    case EntityId.Line:
                        LineEntity lineEnt = (LineEntity)ent;
                        LineD line = lineEnt.mLine.toCopy();
                        line.trim(loc[0], loc[1]); ;
                        ydraw.drawWLine(line);
                        break;
                    case EntityId.Polyline:
                        PolylineEntity polylineEnt = (PolylineEntity)ent;
                        PolylineD polyline = polylineEnt.mPolyline.toCopy();
                        polyline.trim(loc[0], loc[1]); ;
                        ydraw.drawWPolyline(polyline);
                        break;
                    case EntityId.Polygon:
                        break;
                    case EntityId.Arc:
                        ArcEntity arcEnt = (ArcEntity)ent;
                        ArcD arc = arcEnt.mArc.toCopy();
                        arc.trim(loc[0], loc[1]);
                        ydraw.drawWArc(arc, false);
                        break;
                    case EntityId.Text:
                        break;
                    case EntityId.Parts:
                        break;
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
                Entity ent = entityData.mEntityList[pickEnt.Item1];
                //ydraw.mBrush = ent.mColor;
                ydraw.mThickness = ent.mThickness;
                switch (ent.mEntityId) {
                    case EntityId.Point:
                        PointEntity pointEnt = (PointEntity)ent;
                        PointD point = pointEnt.mPoint.toCopy();
                        point.offset(vec);
                        ydraw.mPointType = pointEnt.mType;
                        ydraw.mPointSize = pointEnt.mThickness;
                        ydraw.drawWPoint(point);
                        break;
                    case EntityId.Line:
                        LineEntity lineEnt = (LineEntity)ent;
                        LineD line = lineEnt.mLine.toCopy();
                        line.stretch(vec, pickEnt.pos);
                        ydraw.drawWLine(line);
                        break;
                    case EntityId.Polyline:
                        PolylineEntity polylineEnt = (PolylineEntity)ent;
                        PolylineD polyline = polylineEnt.mPolyline.toCopy();
                        polyline.stretch(vec, pickEnt.pos);
                        ydraw.drawWPolyline(polyline);
                        break;
                    case EntityId.Polygon:
                        PolygonEntity polygonEnt = (PolygonEntity)ent;
                        PolygonD polygon = polygonEnt.mPolygon.toCopy();
                        polygon.stretch(vec, pickEnt.pos);
                        ydraw.drawWPolygon(polygon);
                        break;
                    case EntityId.Arc:
                        ArcEntity arcEnt = (ArcEntity)ent;
                        ArcD arc = arcEnt.mArc.toCopy();
                        arc.stretch(vec, pickEnt.pos);
                        ydraw.drawWArc(arc, false);
                        break;
                    case EntityId.Text:
                        //TextEntity textEnt = (TextEntity)ent;
                        //TextD text = textEnt.mText.toCopy();
                        //text.rotate(loc[0], loc[1]);
                        //ydraw.drawWText(text);
                        break;
                    case EntityId.Parts:
                        PartsEntity partsEnt = (PartsEntity)ent;
                        PartsD parts = partsEnt.mParts.toCopy();
                        parts.stretch(vec, pickEnt.pos);
                        drawWParts(parts);
                        break;
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
                    ydraw.drawWArc(arc);
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
            if (pickList.Count == 0)
                return;
            mMainWindow.imScreen.Source = mBitmapSource;
            mCanvas.Children.Clear();
            mCanvas.Children.Add(mMainWindow.imScreen);
            foreach ((int, PointD) entNo in pickList) {
                Entity ent = entityData.mEntityList[entNo.Item1];
                ent.mPick = true;
                ent.draw(ydraw);
                ent.mPick = false;
            }
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
        /// 画面スクロール
        /// スクロールはビットマップを移動する形で移動によりできた空白部分だけを再描画する
        /// </summary>
        /// <param name="dx">移動量X(screen)</param>
        /// <param name="dy">移動量Y(screen)</param>
        public void scroll(EntityData entityData, double dx, double dy)
        {
#if MYTEST
            //  全体再描画
            PointD v = new PointD(ydraw.screen2worldXlength(dx), ydraw.screen2worldYlength(dy));
            ydraw.mWorld.offset(v.inverse());
            disp();
#else

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
                dispGrid(mGridSize);
                //dispGrid(mCommandOpe.mGridSize);
                foreach (var entity in entityData.mEntityList) {
                    entity.draw(ydraw);
                }
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
                dispGrid(mGridSize);
                //dispGrid(mCommandOpe.mGridSize);
                foreach (var entity in entityData.mEntityList) {
                    entity.draw(ydraw);
                }
            }

            //  Windowの設定を元に戻す
            ydraw.mClipBox = ydraw.mWorld.toCopy();
            mBitmapSource = canvas2Bitmap(mCanvas);
#endif
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
        /// スクリーン座標のX方向長さをワールド座標の長さに変換
        /// </summary>
        /// <param name="x">長さ</param>
        /// <returns>変換長さ</returns>
        public double screen2worldXlength(double x)
        {
            return ydraw.screen2worldXlength(x);
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