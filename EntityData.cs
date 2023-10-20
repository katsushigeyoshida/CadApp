using CoreLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace CadApp
{
    /// <summary>
    /// 要素データクラス
    /// </summary>
    public class EntityData
    {
        public DrawingPara mPara = new DrawingPara();

        public Box mArea;                       //  要素領域
        public int mOperationCouunt = 0;        //  操作回数

        private double mEps = 1E-8;
        public List<Entity> mEntityList;        //  要素リスト
        public Dictionary<string, ulong> mLayerList;    //  レイヤーリスト
        public ImageData mImageData;
        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EntityData() {
            mEntityList = new List<Entity>();
            mLayerList = new Dictionary<string, ulong>();
        }

        /// <summary>
        /// 要素データ情報
        /// </summary>
        /// <returns></returns>
        public string getDataInfo()
        {
            string buf = "";
            buf +=   "要素数 : " + mEntityList.Count;
            buf += "\nデータ領域 : " + mArea.ToString("f2");
            return buf;
        }

        /// <summary>
        /// プロパティ設定値を文字列に変換
        /// </summary>
        /// <returns></returns>
        public string propertyToString()
        {
            return $"Prperty,Color,{ylib.getColorName(mPara.mColor)},PointType,{mPara.mPointType},PointSize,{mPara.mPointSize}," +
                $"LineType,{mPara.mLineType},Thickness,{mPara.mThickness},TextSize,{mPara.mTextSize}," +
                $"TextRotate,{mPara.mTextRotate},LinePitchRate,{mPara.mLinePitchRate},HA,{mPara.mHa},VA,{mPara.mVa}," +
                $"ArrowSize,{mPara.mArrowSize},ArrowAngle,{mPara.mArrowAngle},GridSize,{mPara.mGridSize}";
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
                                mPara.mColor = ylib.getColor(data[++i]);
                                break;
                            case "PointType":
                                mPara.mPointType = int.Parse(data[++i]);
                                break;
                            case "PointSize":
                                mPara.mPointSize = double.Parse(data[++i]);
                                break;
                            case "LineType":
                                mPara.mLineType = int.Parse(data[++i]);
                                break;
                            case "Thickness":
                                mPara.mThickness = double.Parse(data[++i]);
                                break;
                            case "TextSize":
                                mPara.mTextSize = double.Parse(data[++i]);
                                break;
                            case "TextRotate":
                                mPara.mTextRotate = double.Parse(data[++i]);
                                break;
                            case "LinePitchRate":
                                mPara.mLinePitchRate = double.Parse(data[++i]);
                                break;
                            case "HA":
                                mPara.mHa = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), data[++i]);
                                break;
                            case "VA":
                                mPara.mVa = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), data[++i]);
                                break;
                            case "ArrowSize":
                                mPara.mArrowSize = double.Parse(data[++i]);
                                break;
                            case "ArrowAngle":
                                mPara.mArrowAngle = double.Parse(data[++i]);
                                break;
                            case "GridSize":
                                mPara.mGridSize = double.Parse(data[++i]);
                                break;
                        }
                    }
                } else {
                    mPara.mColor      = ylib.getColor(data[0]);
                    mPara.mPointType  = int.Parse(data[1]);
                    mPara.mPointSize  = double.Parse(data[2]);
                    mPara.mLineType   = int.Parse(data[3]);
                    mPara.mThickness  = double.Parse(data[4]);
                    mPara.mTextSize   = double.Parse(data[5]);
                    mPara.mArrowSize  = double.Parse(data[6]);
                    mPara.mArrowAngle = double.Parse(data[7]);
                    mPara.mGridSize   = double.Parse(data[8]);
                    if (mPara.mArrowAngle == 0)
                        mPara.mArrowAngle = 30 * Math.PI / 180;
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 要素からプロパティ値を取得する
        /// </summary>
        /// <param name="entity">要素データ</param>
        public void getProperty(Entity entity)
        {
            mPara.mColor     = entity.mColor;
            mPara.mPointType = entity.mType;
            mPara.mPointSize = entity.mThickness;
            mPara.mLineType  = entity.mType;
            mPara.mThickness = entity.mThickness;
        }

        /// <summary>
        /// 要素データクリア
        /// </summary>
        public void clear()
        {
            mEntityList.Clear();
        }

        /// <summary>
        /// 点要素追加
        /// </summary>
        /// <param name="p">点座標</param>
        /// <returns>要素No</returns>
        public int addPoint(PointD p)
        {
            Entity pointEnt = new PointEntity(p);
            pointEnt.setProperty(mPara);
            pointEnt.mThickness = mPara.mPointSize;
            pointEnt.mType = mPara.mPointType;
            mEntityList.Add(pointEnt);
            if (mArea == null) {
                mArea = pointEnt.mArea;
            } else {
                mArea.extension(pointEnt.mArea);
            }
            pointEnt.mNo = mEntityList.Count - 1;
            pointEnt.mOperationCount = mOperationCouunt;
            return pointEnt.mNo;
        }

        /// <summary>
        /// 線分要素追加
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <returns>要素No</returns>
        public int addLine(PointD sp, PointD ep)
        {
            return addLine(new LineD(sp, ep));
        }

        /// <summary>
        /// 線分要素の追加
        /// </summary>
        /// <param name="pline">線分データ</param>
        /// <returns>要素番号</returns>
        public int addLine(LineD pline)
        {
            if (pline == null || pline.length() < mEps)
                return -1;
            Entity lineEnt = new LineEntity(pline);
            lineEnt.setProperty(mPara);
            mEntityList.Add(lineEnt);
            if (mArea == null) {
                mArea = lineEnt.mArea.toCopy();
            } else {
                mArea.extension(lineEnt.mArea);
            }
            lineEnt.mNo = mEntityList.Count - 1;
            lineEnt.mOperationCount = mOperationCouunt;
            return lineEnt.mNo;
        }

        /// <summary>
        /// 四角形をポリゴン要素で追加
        /// </summary>
        /// <param name="sp">始点(対角点)</param>
        /// <param name="ep">終点(対角点)</param>
        /// <returns>要素番号</returns>
        public int addRect(PointD sp, PointD ep)
        {
            if (sp == null || ep == null || sp.length(ep) < mEps)
                return -1;
            Box b = new Box(sp, ep);
            List<PointD> plist = b.ToPointList();
            return addPolygon(plist);
        }

        /// <summary>
        /// ポリラインの追加
        /// </summary>
        /// <param name="polyline">座標点リスト</param>
        /// <returns>要素番号</returns>
        public int addPolyline(List<PointD> polyline)
        {
            if (polyline == null || polyline.Count < 2)
                return -1;
            Entity polylineEnt = new PolylineEntity(polyline);
            polylineEnt.setProperty(mPara);
            mEntityList.Add(polylineEnt);
            if (mArea == null) {
                mArea = polylineEnt.mArea.toCopy();
            } else {
                mArea.extension(polylineEnt.mArea);
            }
            polylineEnt.mNo = mEntityList.Count - 1;
            polylineEnt.mOperationCount = mOperationCouunt;
            return polylineEnt.mNo;
        }

        /// <summary>
        /// ポリゴンの追加
        /// </summary>
        /// <param name="polygon">座標点リスト</param>
        /// <returns>要素番号</returns>
        public int addPolygon(List<PointD> polygon)
        {
            if (polygon == null || polygon.Count < 3) 
                return -1;
            Entity polygonEnt = new PolygonEntity(polygon);
            polygonEnt.setProperty(mPara);
            mEntityList.Add(polygonEnt);
            if (mArea == null) {
                mArea = polygonEnt.mArea.toCopy();
            } else {
                mArea.extension(polygonEnt.mArea);
            }
            polygonEnt.mNo = mEntityList.Count - 1;
            polygonEnt.mOperationCount = mOperationCouunt;
            return polygonEnt.mNo;
        }

        /// <summary>
        /// 円弧要素の追加
        /// </summary>
        /// <param name="arc">円弧データ</param>
        /// <returns>要素番号</returns>
        public int addArc(ArcD arc)
        {
            if (arc == null || arc.mR < mEps || arc.mEa - arc.mSa < mEps)
                return -1;
            Entity arcEnt = new ArcEntity(arc);
            arcEnt.setProperty(mPara);
            mEntityList.Add(arcEnt);
            if (mArea == null) {
                mArea = arcEnt.mArea;
            } else {
                mArea.extension(arcEnt.mArea);
            }
            arcEnt.mNo = mEntityList.Count - 1;
            arcEnt.mOperationCount = mOperationCouunt;
            return arcEnt.mNo;
        }

        /// <summary>
        /// 楕円要素の追加
        /// </summary>
        /// <param name="ellipse">楕円データ</param>
        /// <returns></returns>
        public int addEllipse(EllipseD ellipse)
        {
            if (ellipse == null || ellipse.mRx < mEps || ellipse.mRy < mEps
                || ellipse.mEa - ellipse.mSa < mEps)
                return -1;
            Entity ellipseEnt = new EllipseEntity(ellipse);
            ellipseEnt.setProperty(mPara);
            mEntityList.Add(ellipseEnt);
            if (mArea == null) {
                mArea = ellipseEnt.mArea;
            } else {
                mArea.extension(ellipseEnt.mArea);
            }
            ellipseEnt.mNo = mEntityList.Count - 1;
            ellipseEnt.mOperationCount = mOperationCouunt;
            return ellipseEnt.mNo;
        }

        /// <summary>
        /// テキスト要素の追加
        /// </summary>
        /// <param name="text">テキスト</param>
        /// <returns>要素番号</returns>
        public int addText(TextD text)
        {
            if (text == null || text.mText.Length == 0 || text.mTextSize == 0)
                return -1;

            text.mFontFamily = mPara.mFontFamily;
            text.mFontStyle = mPara.mFontStyle;
            text.mFontWeight = mPara.mFontWeight;

            Entity textEnt = new TextEntity(text);
            textEnt.setProperty(mPara);
            mEntityList.Add(textEnt);
            if (mArea == null) {
                mArea = textEnt.mArea;
            } else {
                mArea.extension(textEnt.mArea);
            }
            textEnt.mNo = mEntityList.Count - 1;
            textEnt.mOperationCount = mOperationCouunt;
            return textEnt.mNo;
        }

        /// <summary>
        /// 矢印要素を作成
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <returns>要素番号</returns>
        public int addArrow(PointD sp, PointD ep)
        {
            PartsEntity partsEnt = new PartsEntity();
            partsEnt.setProperty(mPara);
            partsEnt.mParts.mFontFamily = mPara.mFontFamily;
            partsEnt.mParts.mFontStyle = mPara.mFontStyle;
            partsEnt.mParts.mFontWeight = mPara.mFontWeight;
            partsEnt.mParts = new PartsD();
            partsEnt.mParts.mArrowSize = mPara.mArrowSize;
            partsEnt.mParts.createArrow(sp, ep);
            partsEnt.updateArea();
            mEntityList.Add(partsEnt);
            if (mArea == null) {
                mArea = partsEnt.mArea;
            } else {
                mArea.extension(partsEnt.mArea);
            }
            partsEnt.mNo = mEntityList.Count - 1;
            partsEnt.mOperationCount = mOperationCouunt;
            return partsEnt.mNo;
        }

        /// <summary>
        /// ラベル要素の作成
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点 </param>
        /// <param name="text">文字列</param>
        /// <returns>要素番号</returns>
        public int addLabel(PointD sp, PointD ep, string text)
        {
            List<PointD> plist = new List<PointD>() { sp, ep };
            PartsEntity partsEnt = new PartsEntity();
            partsEnt.setProperty(mPara);
            partsEnt.mParts = new PartsD();
            partsEnt.mParts.mTextSize = mPara.mTextSize;
            partsEnt.mParts.mArrowSize = mPara.mArrowSize;
            partsEnt.mParts.mFontFamily = mPara.mFontFamily;
            partsEnt.mParts.mFontStyle = mPara.mFontStyle;
            partsEnt.mParts.mFontWeight = mPara.mFontWeight;
            partsEnt.mParts.createLabel(plist, text);
            partsEnt.updateArea();
            mEntityList.Add(partsEnt);
            if (mArea == null) {
                mArea = partsEnt.mArea;
            } else {
                mArea.extension(partsEnt.mArea);
            }
            partsEnt.mNo = mEntityList.Count - 1;
            partsEnt.mOperationCount = mOperationCouunt;
            return partsEnt.mNo;
        }

        /// <summary>
        /// 寸法線の作成
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="pos">寸法位置</param>
        /// <returns>要素番号</returns>
        public int addLocDimension(PointD sp, PointD ep, PointD pos)
        {
            List<PointD> plist = new List<PointD>() {
                sp, ep, pos
            };
            PartsEntity partsEnt = new PartsEntity();
            partsEnt.setProperty(mPara);
            partsEnt.mParts = new PartsD();
            partsEnt.mParts.mTextSize = mPara.mTextSize;
            partsEnt.mParts.mArrowSize = mPara.mArrowSize;
            partsEnt.mParts.mFontFamily = mPara.mFontFamily;
            partsEnt.mParts.mFontStyle = mPara.mFontStyle;
            partsEnt.mParts.mFontWeight = mPara.mFontWeight;
            partsEnt.mParts.createDimension(plist);
            partsEnt.updateArea();
            mEntityList.Add(partsEnt);
            if (mArea == null) {
                mArea = partsEnt.mArea;
            } else {
                mArea.extension(partsEnt.mArea);
            }
            partsEnt.mNo = mEntityList.Count - 1;
            partsEnt.mOperationCount = mOperationCouunt;
            return partsEnt.mNo;
        }

        /// <summary>
        /// 寸法線の作成
        /// </summary>
        /// <param name="pickList"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public int addDimension(List<(int no, PointD pos)> pickList, PointD pos)
        {
            if (pickList == null || pickList.Count < 2)
                return -1;
            Entity ent0 = mEntityList[pickList[0].no];
            Entity ent1 = mEntityList[pickList[1].no];
            PointD ps = ent0.getEndPoint(pickList[0].pos);
            PointD pe = ent1.getEndPoint(pickList[1].pos);

            return addLocDimension(ps, pe, pos);
        }

        /// <summary>
        /// 角度寸法線の作成
        /// </summary>
        /// <param name="pickList">ピック要素</param>
        /// <param name="pos">寸法位置</param>
        /// <returns>要素番号</returns>
        public int addAngleDimension(List<(int no, PointD pos)> pickList, PointD pos)
        {
            if (pickList == null || pickList.Count < 2)
                return -1;
            Entity ent0 = mEntityList[pickList[0].no];
            Entity ent1 = mEntityList[pickList[1].no];
            PointD cp = ent0.getLine(pickList[0].pos).intersection(ent1.getLine(pickList[1].pos));
            PointD ps = ent0.getEndPoint(pickList[0].pos, cp);
            PointD pe = ent1.getEndPoint(pickList[1].pos, cp);
            if (!cp.isNaN()) {
                PartsEntity partsEnt = new PartsEntity();
                partsEnt.setProperty(mPara);
                partsEnt.mParts = new PartsD();
                partsEnt.mParts.mTextSize = mPara.mTextSize;
                partsEnt.mParts.mArrowSize = mPara.mArrowSize;
                partsEnt.mParts.mFontFamily = mPara.mFontFamily;
                partsEnt.mParts.mFontStyle = mPara.mFontStyle;
                partsEnt.mParts.mFontWeight = mPara.mFontWeight;
                List<PointD> plist = new List<PointD>() { cp, ps, pe, pos };
                partsEnt.mParts.createAngleDimension(plist);
                partsEnt.updateArea();
                mEntityList.Add(partsEnt);
                if (mArea == null) {
                    mArea = partsEnt.mArea;
                } else {
                    mArea.extension(partsEnt.mArea);
                }
                partsEnt.mNo = mEntityList.Count - 1;
                partsEnt.mOperationCount = mOperationCouunt;
                return partsEnt.mNo;
            }
            return -1;
        }

        /// <summary>
        /// 直径寸法線の作成
        /// </summary>
        /// <param name="pickList">ピック要素</param>
        /// <param name="pos">寸法位置</param>
        /// <returns>要素番号</returns>
        public int addDiameterDimension(List<(int no, PointD pos)> pickList, PointD pos)
        {
            if (pickList == null || pickList.Count == 0)
                return -1;
            List<PointD> plist = new List<PointD>() { pos };
            Entity ent = mEntityList[pickList[0].no];
            if (ent.mEntityId == EntityId.Arc) {
                ArcEntity arcEnt = (ArcEntity)ent;
                PartsEntity partsEnt = new PartsEntity();
                partsEnt.setProperty(mPara);
                partsEnt.mParts = new PartsD();
                partsEnt.mParts.mTextSize = mPara.mTextSize;
                partsEnt.mParts.mArrowSize = mPara.mArrowSize;
                partsEnt.mParts.mFontFamily = mPara.mFontFamily;
                partsEnt.mParts.mFontStyle = mPara.mFontStyle;
                partsEnt.mParts.mFontWeight = mPara.mFontWeight;
                partsEnt.mParts.createDiameterDimension(arcEnt.mArc, plist);
                partsEnt.updateArea();
                mEntityList.Add(partsEnt);
                if (mArea == null) {
                    mArea = partsEnt.mArea;
                } else {
                    mArea.extension(partsEnt.mArea);
                }
                partsEnt.mNo = mEntityList.Count - 1;
                partsEnt.mOperationCount = mOperationCouunt;
                return partsEnt.mNo;
            }
            return -1;
        }

        /// <summary>
        /// 半径寸法線の作成
        /// </summary>
        /// <param name="pickList">ピック要素</param>
        /// <param name="pos">寸法位置</param>
        /// <returns>要素番号</returns>
        public int addRadiusDimension(List<(int no, PointD pos)> pickList, PointD pos)
        {
            if (pickList == null || pickList.Count == 0)
                return -1;
            List<PointD> plist = new List<PointD>() { pos };
            Entity ent = mEntityList[pickList[0].no];
            if (ent.mEntityId == EntityId.Arc) {
                ArcEntity arcEnt = (ArcEntity)ent;
                PartsEntity partsEnt = new PartsEntity();
                partsEnt.setProperty(mPara);
                partsEnt.mParts = new PartsD();
                partsEnt.mParts.mTextSize = mPara.mTextSize;
                partsEnt.mParts.mArrowSize = mPara.mArrowSize;
                partsEnt.mParts.mFontFamily = mPara.mFontFamily;
                partsEnt.mParts.mFontStyle = mPara.mFontStyle;
                partsEnt.mParts.mFontWeight = mPara.mFontWeight;
                partsEnt.mParts.createRadiusDimension(arcEnt.mArc, plist);
                partsEnt.updateArea();
                mEntityList.Add(partsEnt);
                if (mArea == null) {
                    mArea = partsEnt.mArea;
                } else {
                    mArea.extension(partsEnt.mArea);
                }
                partsEnt.mNo = mEntityList.Count - 1;
                partsEnt.mOperationCount = mOperationCouunt;
                return partsEnt.mNo;
            }
            return -1;
        }

        /// <summary>
        /// 接線を作成する
        /// </summary>
        /// <param name="pickList">ピック要素リスト</param>
        /// <returns>線分データ</returns>
        public LineD tangentLine(List<(int no, PointD pos)> pickList)
        {
            if (pickList.Count < 2)
                return null;
            Entity ent0 = mEntityList[pickList[0].no];
            Entity ent1 = mEntityList[pickList[1].no];
            if (ent0.mEntityId == EntityId.Arc && ent1.mEntityId == EntityId.Arc) {
                ArcEntity arc0 = (ArcEntity)ent0;
                ArcEntity arc1 = (ArcEntity)ent1;
                List<LineD> llist = arc0.mArc.tangentArc(arc1.mArc);
                foreach (LineD line in llist) {
                    double ang0 = arc0.mArc.mCp.angle(pickList[0].pos, line.ps);
                    double ang1 = arc1.mArc.mCp.angle(pickList[1].pos, line.pe);
                    if (ang0 < Math.PI / 2 && ang1 < Math.PI / 2)
                        return line;
                }
            }
            return null;
        }

        /// <summary>
        /// 接円を作成する
        /// </summary>
        /// <param name="pickList">ピック要素</param>
        /// <param name="loc">参照位置</param>
        /// <returns>円データ</returns>
        public ArcD tangentCircle(List<(int no, PointD pos)> pickList, List<PointD> loc, double r = 0)
        {
            if (1 < pickList.Count && loc.Count == 1) {
                List<ArcD> arcList = null;
                Entity ent0 = mEntityList[pickList[0].no];
                Entity ent1 = mEntityList[pickList[1].no];
                if ((ent0.mEntityId == EntityId.Line || ent0.mEntityId == EntityId.Polyline || ent0.mEntityId == EntityId.Polygon) &&
                    (ent1.mEntityId == EntityId.Line || ent1.mEntityId == EntityId.Polyline || ent1.mEntityId == EntityId.Polygon)) {
                    LineD l0 = ent0.getLine(pickList[0].pos);
                    LineD l1 = ent1.getLine(pickList[1].pos);
                    if (!l0.isNaN() && !l1.isNaN()) {
                        r = r == 0 ? l0.distance(loc[0]) : r;
                        ArcD arc = new ArcD();
                        arcList = arc.tangentCircle(l0, l1, r);
                    }
                } else if ((ent0.mEntityId == EntityId.Line || ent0.mEntityId == EntityId.Polyline || ent0.mEntityId == EntityId.Polygon) &&
                        ent1.mEntityId == EntityId.Arc) {
                    LineD l0 = ent0.getLine(pickList[0].pos);
                    ArcD arc = ((ArcEntity)ent1).mArc;
                    r = r == 0 ? l0.distance(loc[0]) : r;
                    arcList = arc.tangentCircle(l0, arc, r);
                } else if ((ent1.mEntityId == EntityId.Line || ent1.mEntityId == EntityId.Polyline || ent1.mEntityId == EntityId.Polygon) &&
                        ent0.mEntityId == EntityId.Arc) {
                    LineD l1 = ent1.getLine(pickList[1].pos);
                    ArcD arc = ((ArcEntity)ent0).mArc;
                    r = r == 0 ? l1.distance(loc[0]) : r;
                    arcList = arc.tangentCircle(l1, arc, r);
                } else if (ent0.mEntityId == EntityId.Arc && ent1.mEntityId == EntityId.Arc) {
                    ArcD arc0 = ((ArcEntity)ent0).mArc;
                    ArcD arc1 = ((ArcEntity)ent1).mArc;
                    r = r == 0 ? Math.Abs(arc0.mCp.length(loc[0]) - arc0.mR) : r;
                    arcList = arc0.tangentCircle(arc0, arc1, r);
                } else {
                    return null;
                }
                return arcList.MinBy(p => p.mCp.length(loc[0]));
            }
            return null;
        }

        /// <summary>
        /// 全データを表示する
        /// イメージ要素を先に表示する
        /// </summary>
        /// <param name="ydraw"></param>
        public void drawingAll(YWorldDraw ydraw)
        {
            //  イメージ要素を先に表示する
            List<int> imageNoList = new List<int>();
            for(int i = 0; i < mEntityList.Count; i++)
                if (mEntityList[i].mEntityId == EntityId.Image)
                    imageNoList.Add(i);
            for(int i = 0; i < imageNoList.Count; i++)
                if (!mEntityList[imageNoList[i]].mRemove && 0 != (mEntityList[imageNoList[i]].mLayerBit & mPara.mDispLayerBit)
                    && !ydraw.mWorld.outsideChk(mEntityList[imageNoList[i]].mArea))
                    mEntityList[imageNoList[i]].draw(ydraw);
            //  イメージ要素以外を表示
            foreach (var entity in mEntityList) {
                if (!entity.mRemove && 0 != (entity.mLayerBit & mPara.mDispLayerBit)
                    && entity.mEntityId != EntityId.Link
                    && entity.mEntityId != EntityId.Image
                    && !ydraw.mWorld.outsideChk(entity.mArea))
                    entity.draw(ydraw);
            }
        }

        /// <summary>
        /// 指定要素を移動する
        /// </summary>
        /// <param name="pickList">要素リスト</param>
        /// <param name="vec">移動量</param>
        /// <param name="copy">コピー作成</param>
        /// <returns></returns>
        public bool translate(List<(int no, PointD pos)> pickList, PointD vec, bool copy = false)
        {
            foreach ((int no, PointD pos) entNo in pickList) {
                mEntityList.Add(mEntityList[entNo.no].toCopy());
                mEntityList[mEntityList.Count - 1].translate(vec);
                mEntityList[mEntityList.Count - 1].mOperationCount = mOperationCouunt;
                if (!copy)
                    removeEnt(entNo.no);
            }
            updateData();
            return true;
        }

        /// <summary>
        /// 指定要素を回転する
        /// </summary>
        /// <param name="pickList">要素リスト</param>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        /// <param name="copy">コピー作成</param>
        /// <returns></returns>
        public bool rotate(List<(int no, PointD pos)> pickList, PointD cp, PointD mp, bool copy = false)
        {
            foreach ((int no, PointD pos) entNo in pickList) {
                mEntityList.Add(mEntityList[entNo.no].toCopy());
                mEntityList[mEntityList.Count - 1].rotate(cp, mp);
                mEntityList[mEntityList.Count - 1].mOperationCount = mOperationCouunt;
                if (!copy)
                    removeEnt(entNo.no);
            }
            updateData();
            return true;
        }

        /// <summary>
        /// 要素をミラーする
        /// </summary>
        /// <param name="pickList">要素リスト</param>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        /// <param name="copy">コピー作成</param>
        /// <returns></returns>
        public bool mirror(List<(int no, PointD pos)> pickList, PointD sp, PointD ep, bool copy = false)
        {
            foreach ((int no, PointD pos) entNo in pickList) {
                mEntityList.Add(mEntityList[entNo.no].toCopy());
                mEntityList[mEntityList.Count - 1].mirror(sp, ep);
                mEntityList[mEntityList.Count - 1].mOperationCount = mOperationCouunt;
                if (!copy)
                    removeEnt(entNo.no);
            }
            updateData();
            return true;
        }

        /// <summary>
        /// 原点を指定して拡大縮小
        /// </summary>
        /// <param name="pickList">要素リスト</param>
        /// <param name="cp">原点</param>
        /// <param name="scale">拡大率</param>
        /// <param name="copy">コピー作成</param>
        /// <returns></returns>
        public bool scale(List<(int no, PointD pos)> pickList, PointD cp, double scale, bool copy = false)
        {
            foreach ((int no, PointD pos) entNo in pickList) {
                mEntityList.Add(mEntityList[entNo.no].toCopy());
                mEntityList[mEntityList.Count - 1].scale(cp, scale);
                mEntityList[mEntityList.Count - 1].mOperationCount = mOperationCouunt;
                if (!copy)
                    removeEnt(entNo.no);
            }
            updateData();
            return true;
        }

        /// <summary>
        /// 要素をオフセットする
        /// </summary>
        /// <param name="pickList">要素リスト</param>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        /// <param name="copy">コピー作成</param>
        /// <returns></returns>
        public bool offset(List<(int no, PointD pos)> pickList, PointD sp, PointD ep, bool copy = false)
        {
            foreach ((int no, PointD pos) entNo in pickList) {
                mEntityList.Add(mEntityList[entNo.no].toCopy());
                mEntityList[mEntityList.Count - 1].offset(sp, ep);
                mEntityList[mEntityList.Count - 1].mOperationCount = mOperationCouunt;
                if (!copy)
                    removeEnt(entNo.no);
            }
            updateData();
            return true;
        }

        /// <summary>
        /// 要素をトリムする
        /// </summary>
        /// <param name="pickList">要素リスト</param>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        /// <param name="copy">コピー作成</param>
        /// <returns></returns>
        public bool trim(List<(int no, PointD pos)> pickList, PointD sp, PointD ep, bool copy = false)
        {
            foreach ((int no, PointD pos) entNo in pickList) {
                mEntityList.Add(mEntityList[entNo.no].toCopy());
                mEntityList[mEntityList.Count - 1].trim(sp, ep);
                mEntityList[mEntityList.Count - 1].mOperationCount = mOperationCouunt;
                if (!copy)
                    removeEnt(entNo.no);
            }
            updateData();
            return true;
        }

        /// <summary>
        /// 要素を分割する
        /// </summary>
        /// <param name="pickList">要素リスト</param>
        /// <param name="dp">分割参照点</param>
        /// <returns></returns>
        public bool divide(List<(int no, PointD pos)> pickList, PointD dp)
        {
            foreach ((int no, PointD pos) entNo in pickList) {
                List<Entity> entityList = mEntityList[entNo.no].divide(dp);
                if (entityList == null)
                    continue;
                foreach(Entity ent in entityList) {
                    ent.mOperationCount = mOperationCouunt;
                    mEntityList.Add(ent);
                }
                removeEnt(entNo.no);
            }
            updateData();
            return true;
        }

        /// <summary>
        /// 要素をストレッチする
        /// </summary>
        /// <param name="pickList">要素リスト</param>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <returns></returns>
        public bool stretch(List<(int no, PointD pos)> pickList, PointD vec)
        {
            foreach ((int no, PointD pos) entNo in pickList) {
                mEntityList.Add(mEntityList[entNo.no].toCopy());
                mEntityList[mEntityList.Count - 1].stretch(vec, entNo.pos);
                mEntityList[mEntityList.Count - 1].mOperationCount = mOperationCouunt;
                removeEnt(entNo.no);
            }
            updateData();
            return true;
        }

        /// <summary>
        /// ポリライン、ポリゴン、パーツを個別要素に分解する
        /// </summary>
        /// <param name="pickList"></param>
        /// <returns></returns>
        public bool disassemble(List<(int no, PointD pos)> pickList)
        {
            List<LineD> lines;
            foreach ((int no, PointD pos) entNo in pickList) {
                Entity entity = mEntityList[entNo.no];
                getProperty(entity);
                switch (entity.mEntityId) {
                    case EntityId.Polyline:
                        PolylineEntity polylineEnt = (PolylineEntity)entity;
                        lines = polylineEnt.mPolyline.toLineList();
                        foreach (var line in lines) {
                            addLine(line);
                            mEntityList[mEntityList.Count - 1].mOperationCount = mOperationCouunt;
                        }
                        break;
                    case EntityId.Polygon:
                        PolygonEntity polygonEnt = (PolygonEntity)entity;
                        lines = polygonEnt.mPolygon.toLineList();
                        foreach (var line in lines) {
                            addLine(line);
                            mEntityList[mEntityList.Count - 1].mOperationCount = mOperationCouunt;
                        }
                        break;
                    case EntityId.Parts:
                        PartsEntity partsEnt = (PartsEntity)entity;
                        foreach (var line in partsEnt.mParts.mLines) {
                            addLine(line);
                            mEntityList[mEntityList.Count - 1].mOperationCount = mOperationCouunt;
                        }
                        foreach (var arc in partsEnt.mParts.mArcs) {
                            addArc(arc);
                            mEntityList[mEntityList.Count - 1].mOperationCount = mOperationCouunt;
                        }
                        foreach (var text in partsEnt.mParts.mTexts) {
                            addText(text);
                            mEntityList[mEntityList.Count - 1].mOperationCount = mOperationCouunt;
                        }
                        break;
                }
                removeEnt(entNo.no);
            }
            updateData();
            return true;

        }

        /// <summary>
        /// 要素を追加する
        /// </summary>
        /// <param name="entity">Entityデータ</param>
        /// <returns></returns>
        public int addEntity(Entity entity)
        {
            entity.updateArea();
            mEntityList.Add(entity);
            entity.mNo = mEntityList.Count - 1;
            entity.mOperationCount = mOperationCouunt;
            return entity.mNo;
        }

        /// <summary>
        /// ピックした要素を削除する
        /// </summary>
        /// <param name="pickList">要素番号リスト</param>
        /// <returns></returns>
        public bool removeEnt(List<(int, PointD)> pickList)
        {
            foreach ((int no, PointD pos) entNo in pickList) {
                removeEnt(entNo.no);
            }
            updateData();
            return true;
        }

        /// <summary>
        /// 指定要素を削除
        /// 要素の削除はアンドゥできるように指定要素の削除フラグをたて
        /// そのリンク要素を最終要素に追加する
        /// </summary>
        /// <param name="entNo"></param>
        public void removeEnt(int entNo)
        {
            mEntityList[entNo].mRemove = true;
            LinkEntity link = new LinkEntity();
            link.mRemove = true;
            link.mLinkNo = entNo;
            link.mOperationCount = mOperationCouunt;
            mEntityList.Add(link);
        }

        /// <summary>
        /// アンドゥ処理
        /// 最終要素がリンク要素であればリンク先要素の削除フラグを解除してから採取要素を削除
        /// リンク要素でなければそのまま削除
        /// </summary>
        public void undo()
        {
            if (0 < mEntityList.Count) {
                int entNo = mEntityList.Count - 1;
                int opeNo = mEntityList[entNo].mOperationCount;
                while (0 <= entNo && 0 <= opeNo && opeNo == mEntityList[entNo].mOperationCount) {
                    if (mEntityList[entNo].mEntityId == EntityId.Link) {
                        LinkEntity linkEnt = (LinkEntity)mEntityList[entNo];
                        mEntityList[linkEnt.mLinkNo].mRemove = false;
                    }
                    mEntityList.RemoveAt(entNo);
                    entNo--;
                }
            }
        }

        /// <summary>
        /// 要素領域、要素番号、レイヤービットの更新、
        /// </summary>
        public void updateData()
        {
            mLayerList.Clear();
            if (mEntityList != null && 0 < mEntityList.Count) {
                //  レイヤービットの更新
                for (int i = 0; i < mEntityList.Count; i++) {
                    if (!mEntityList[i].mRemove && mEntityList[i].mEntityId != EntityId.Link) {
                        if (mEntityList[i].mLayerName.Length == 0)
                            mEntityList[i].mLayerName = mPara.mCreateLayerName;
                        mEntityList[i].mLayerBit = setLayerBit(mEntityList[i].mLayerName);
                    }
                }
                //  要素領域と要素番号の更新
                mArea = null;
                for (int i = 0; i < mEntityList.Count; i++) {
                    if (!mEntityList[i].mRemove && mEntityList[i].mEntityId != EntityId.Link
                        && (mEntityList[i].mLayerBit & mPara.mDispLayerBit) != 0) {
                        if (mArea == null) {
                            mArea = mEntityList[i].mArea.toCopy();
                        } else {
                            mEntityList[i].updateArea();
                            mArea.extension(mEntityList[i].mArea);
                        }
                        mEntityList[i].mNo = i;     //  要素番号
                    }
                }
            }
            addDispLayer(mPara.mCreateLayerName);
        }

        /// <summary>
        /// Boxの内側か交点をもつ要素を検索して最初に見つかった要素No
        /// </summary>
        /// <param name="b">検索Box</param>
        /// <returns>検索要素No</returns>
        public List<int> findIndex(Box b)
        {
            List<int> picks = new List<int>();
            for (int i = 0; i < mEntityList.Count; i++) {
                if (mEntityList[i].mRemove || 0 == (mEntityList[i].mLayerBit & mPara.mDispLayerBit)
                    || mEntityList[i].mEntityId == EntityId.Non
                    || mEntityList[i].mEntityId == EntityId.Link)
                    continue;
                if (b.insideChk(mEntityList[i].mArea))          //  Boxの内側
                    picks.Add(i);
                else if (b.outsideChk(mEntityList[i].mArea))    //  Boxの外側
                    continue;
                else if (mEntityList[i].intersectionChk(b))       //  Boxと交点あり
                    picks.Add(i);
            }
            return picks;
        }

        /// <summary>
        /// 要素同士の交点のうち最も近い点を求める
        /// </summary>
        /// <param name="entNo1">要素1</param>
        /// <param name="entNo2">要素２</param>
        /// <param name="pos">参照点</param>
        /// <returns>交点(ない場合はnull)</returns>
        public PointD intersection(int entNo1, int entNo2, PointD pos)
        {
            List<PointD> plist = intersection(entNo1, entNo2);
            if (plist.Count == 0)
                return null;
            else
                return plist.MinBy(p => p.length(pos));
        }

        /// <summary>
        /// 要素同士の交点を求める
        /// </summary>
        /// <param name="entNo1">要素1</param>
        /// <param name="entNo2">要素２</param>
        /// <returns>交点リスト</returns>
        public List<PointD> intersection(int entNo1, int entNo2)
        {
            Entity ent1 = mEntityList[entNo1];
            Entity ent2 = mEntityList[entNo2];
            if (ent2.mEntityId == EntityId.Parts)
                return ent2.intersection(ent1);
            else if (ent2.mEntityId == EntityId.Ellipse)
                return ent2.intersection(ent1);
            else
                return ent1.intersection(ent2);
        }

        /// <summary>
        /// レイヤー名から表示ビットを取得
        /// </summary>
        /// <param name="layerName">レイヤー名</param>
        /// <returns>表示ビット</returns>
        public ulong getLayerBit(string layerName)
        {
            if (mLayerList.ContainsKey(layerName))
                return mLayerList[layerName];
            else
                return 0;
        }

        /// <summary>
        /// 表示ビットからレイヤー名を取得
        /// </summary>
        /// <param name="bit">表示ビット</param>
        /// <returns>レイヤー名</returns>
        public string getLayerName(ulong bit)
        {
            if (mLayerList.ContainsValue(bit)) {
                foreach (KeyValuePair<string, ulong> item in mLayerList) {
                    if (item.Value == bit)
                        return item.Key;
                }
            }
            return "";
        }

        /// <summary>
        /// 新規レイヤー名に表示ビットを割り当てる
        /// </summary>
        /// <param name="layerName">レイヤー名</param>
        /// <returns>表示ビット</returns>
        public ulong setLayerBit(string layerName)
        {
            if (mLayerList.ContainsKey(layerName))
                return mLayerList[layerName];
            ulong layerBit = 0x1;
            for (int i = 0; i < 64; i++) {
                if (getLayerName(layerBit).Length ==0) {
                    mLayerList.Add(layerName, layerBit);
                    return layerBit;
                }
                layerBit <<= 1;
            }
            return 0;
        }

        /// <summary>
        /// レイヤー名のリストを取得
        /// </summary>
        /// <returns>レイヤー名リスト</returns>
        public List<string> getLayerNameList()
        {
            List<string> layerList = new List<string>();
            updateLayerList();
            foreach (string key in mLayerList.Keys) {
                layerList.Add(key);
            }
            layerList.Sort();
            return layerList;
        }

        /// <summary>
        /// チェックリスト形式でレイヤー名リストを取得
        /// </summary>
        /// <returns>レイヤー名リスト</returns>
        public List<CheckBoxListItem> getLayerChkList()
        {
            updateLayerList();
            addDispLayer(mPara.mCreateLayerName);
            List<CheckBoxListItem> chkList = new List<CheckBoxListItem>();
            foreach (KeyValuePair<string, ulong> item in mLayerList) {
                CheckBoxListItem chkItem = new CheckBoxListItem((mPara.mDispLayerBit & item.Value) != 0, item.Key);
                chkList.Add(chkItem);
            }
            chkList.Sort((a,b) => a.Text.CompareTo(b.Text));
            return chkList;
        }

        /// <summary>
        /// チェックリスト形式でレイヤー名リストから表示ビットフィルタを作成
        /// </summary>
        /// <param name="chkList"></param>
        public void setDispLayerBit(List<CheckBoxListItem> chkList)
        {
            mPara.mDispLayerBit = 0;
            foreach (CheckBoxListItem chkItem in chkList) {
                if (chkItem.Checked) {
                    mPara.mDispLayerBit |= mLayerList[chkItem.Text];
                }
            }
        }

        /// <summary>
        /// 要素全体のレイヤー名と表示ビットを更新
        /// </summary>
        public void updateLayerList()
        {
            mLayerList.Clear();
            foreach (Entity ent in mEntityList) {
                if (ent.mRemove == false && ent.mEntityId != EntityId.Link) {
                    if (ent.mLayerName.Length == 0) {
                        ent.mLayerName = mPara.mCreateLayerName;
                    }
                    ent.mLayerBit = setLayerBit(ent.mLayerName);
                }
            }
            if (!mLayerList.ContainsKey(mPara.mCreateLayerName))
                setLayerBit(mPara.mCreateLayerName);
        }

        /// <summary>
        /// 表示レイヤーの追加
        /// </summary>
        /// <param name="layerBit"></param>
        public void addDispLayer(ulong layerBit)
        {
            mPara.mDispLayerBit |= layerBit;
        }

        /// <summary>
        /// 表示レイヤーの追加
        /// </summary>
        /// <param name="layerName"></param>
        public void addDispLayer(string layerName)
        {
            ulong layerbit = setLayerBit(layerName);
            addDispLayer(layerbit);
        }

        /// <summary>
        /// レイヤー名を変更する
        /// </summary>
        /// <param name="oldName">元のレイヤー名</param>
        /// <param name="newName">新しいレイヤー名</param>
        /// <returns></returns>
        public bool changeLayerName(string oldName, string newName)
        {
            for (int i = 0; i < mEntityList.Count; i++) {
                Entity entity = mEntityList[i];
                if (entity != null && !entity.mRemove && entity.mEntityId != EntityId.Non) {
                    if (entity.mLayerName == oldName) {
                        Entity newEntity = entity.toCopy();
                        newEntity.mLayerName = newName;
                        newEntity.mOperationCount = mOperationCouunt;
                        mEntityList.Add(newEntity);
                        removeEnt(i);
                    }
                }
            }
            if (mPara.mCreateLayerName == oldName) {
                mPara.mCreateLayerName = newName;
            }
            updateData();
            return true;
        }

        /// <summary>
        /// 要素プロパティ(テキストデータ)をEntityデータに変換
        /// </summary>
        /// <param name="propertyStr">要素プロパティ(テキストデータ)</param>
        /// <param name="data">要素データのテキストデータ</param>
        /// <returns>要素データ</returns>
        public Entity setStringEntityData(string[] property, string[] dataStr)
        {
            if (0 < property.Length) {
                if (0 <= property[0].IndexOf(EntityId.Point.ToString())) {
                    //  点要素
                    PointEntity pointEntity = new PointEntity();
                    pointEntity.setProperty(property);
                    pointEntity.setData(dataStr);
                    return pointEntity;
                } else if (0 <= property[0].IndexOf(EntityId.Line.ToString())) {
                    //  線分要素
                    LineEntity lineEntity = new LineEntity();
                    lineEntity.setProperty(property);
                    lineEntity.setData(dataStr);
                    return lineEntity;
                } else if (0 <= property[0].IndexOf(EntityId.Polyline.ToString())) {
                    //  ポリライン要素
                    PolylineEntity polylineEntity = new PolylineEntity();
                    polylineEntity.setProperty(property);
                    polylineEntity.setData(dataStr);
                    return polylineEntity;
                } else if (0 <= property[0].IndexOf(EntityId.Polygon.ToString())) {
                    //  ポリゴン要素
                    PolygonEntity polygonEntity = new PolygonEntity();
                    polygonEntity.setProperty(property);
                    polygonEntity.setData(dataStr);
                    return polygonEntity;
                } else if (0 <= property[0].IndexOf(EntityId.Arc.ToString())) {
                    //  円弧要素
                    ArcEntity arcEntity = new ArcEntity();
                    arcEntity.setProperty(property);
                    arcEntity.setData(dataStr);
                    return arcEntity;
                } else if (0 <= property[0].IndexOf(EntityId.Ellipse.ToString())) {
                    //  楕円要素
                    EllipseEntity ellipseEntity = new EllipseEntity();
                    ellipseEntity.setProperty(property);
                    ellipseEntity.setData(dataStr);
                    return ellipseEntity;
                } else if (0 <= property[0].IndexOf(EntityId.Text.ToString())) {
                    //  テキスト要素
                    TextEntity textEntity = new TextEntity();
                    textEntity.setProperty(property);
                    textEntity.setData(dataStr);
                    return textEntity;
                } else if (0 <= property[0].IndexOf(EntityId.Parts.ToString())) {
                    //  パーツ要素
                    PartsEntity partsEntity = new PartsEntity();
                    partsEntity.setProperty(property);
                    partsEntity.setData(dataStr);
                    return partsEntity;
                } else if (0 <= property[0].IndexOf(EntityId.Image.ToString())) {
                    //  イメージ要素
                    ImageEntity imageEntity = new ImageEntity(mImageData);
                    imageEntity.setProperty(property);
                    imageEntity.setData(dataStr);
                    imageEntity.fileUpdate();
                    return imageEntity;
                }
            }
            return null;
        }

        /// <summary>
        /// 要素リストの読出し
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void loadData(string path)
        {
            mEntityList = new List<Entity>();
            mPara.init();
            List<string[]> dataList = ylib.loadCsvData(path);
            for (int i = 0; i < dataList.Count - 1; i++) {
                Entity entity = setStringEntityData(dataList[i], dataList[i + 1]);
                if (entity != null) {
                    if (entity.mLayerName == "") {
                        entity.mLayerName = mPara.mCreateLayerName;
                    }
                    entity.mLayerBit = setLayerBit(entity.mLayerName);
                    mEntityList.Add(entity);
                    i++;
                } else {
                    if (0 <= dataList[i][0].IndexOf(EntityId.Property.ToString())) {
                        //  システムプロパティ情報
                        if (i + 1 < dataList.Count) {
                            mPara.setPropertyData(dataList[++i]);
                        }
                    } else if (0 <= dataList[i][0].IndexOf(EntityId.Comment.ToString())) {
                        //  図面コメント情報
                        if (i + 1 < dataList.Count) {
                            mPara.setCommentData(dataList[++i]);
                        }
                    }
                }
            }
            updateData();
        }

        /// <summary>
        /// 要素リストのファイル保存
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void saveData(string path)
        {
            List<string[]> listData = new List<string[]>();
            //  図面のプロパティ
            string[] buf = { EntityId.Property.ToString() };
            listData.Add(buf);
            buf = mPara.propertyToString().Split(new char[] { ',' });
            listData.Add(buf);
            //  図面コメント情報
            buf = new string[] { EntityId.Comment.ToString() };
            listData.Add(buf);
            buf = mPara.commentToString().Split(new char[] { ',' });
            listData.Add(buf);
            //  要素データ
            foreach (Entity entity in mEntityList) {
                if (entity != null && !entity.mRemove && entity.mEntityId != EntityId.Non) {
                    listData.Add(entity.toList().ToArray());
                    listData.Add(entity.toDataList().ToArray());
                }
            }
            ylib.saveCsvData(path, listData);
        }
    }
}
