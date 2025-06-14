﻿using CoreLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CadApp
{
    /// <summary>
    /// 要素データクラス
    /// </summary>
    public class EntityData
    {
        public static string[] mPointTypeMenu = new string[] {
            "・点", "X クロス", "+ 十字", "□ 四角", "〇 円"
        };
        public static string[] mLineTypeMenu = new string[] {
            "実線", "破線", "一点鎖線", "二点鎖線"
        };

        public DrawingPara mPara = new DrawingPara();   //  要素プロパティ

        public Box mArea;                       //  要素領域
        public int mOperationCount = 0;         //  操作回数
        public int mFirstEntityCount = 0;       //  編集開始時の要素数

        public List<Entity> mEntityList;        //  要素リスト
        public Layer mLayer;                    //  レイヤー管理
        public ImageData mImageData;            //  イメージデータ管理
        public Group mGroup;

        private double mEps = 1E-8;
        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EntityData() {
            mEntityList = new List<Entity>();
            mLayer = new Layer(mEntityList, mPara);
            mGroup = new Group();
        }

        /// <summary>
        /// 要素データ情報
        /// </summary>
        /// <returns></returns>
        public string getDataInfo()
        {
            string buf = "";
            buf += "要素数 : " + mEntityList.Count;
            buf += "\nデータ領域 : " + mArea.ToString("f2");
            return buf;
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
            return addEntity(pointEnt);
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
            return addEntity(lineEnt);
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
            return addEntity(polylineEnt);
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
            PolygonEntity polygonEnt = new PolygonEntity(polygon);
            polygonEnt.setProperty(mPara);
            return addEntity(polygonEnt);
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
            return addEntity(arcEnt);
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
            return addEntity(ellipseEnt);
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
            return addEntity(textEnt);
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
            return addEntity(partsEnt);
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
            return addEntity(partsEnt);
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
            return addEntity(partsEnt);
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
                return addEntity(partsEnt);
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
                return addEntity(partsEnt);
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
                return addEntity(partsEnt);
            }
            return -1;
        }

        /// <summary>
        /// 円または楕円と点との接線
        /// </summary>
        /// <param name="pick">ピック要素</param>
        /// <param name="loc">座標点</param>
        /// <returns>線分</returns>
        public LineD tangentLine((int no, PointD pos) pick, PointD loc)
        {
            Entity ent = mEntityList[pick.no];
            List<PointD> plist;
            if (ent.mEntityId == EntityId.Arc) {
                ArcEntity arc0 = (ArcEntity)ent;
                plist = arc0.mArc.tangentPoint(loc);
            } else if (ent.mEntityId == EntityId.Ellipse) {
                EllipseEntity ellipse = (EllipseEntity)ent;
                plist = ellipse.mEllipse.tangentPoint(loc);
            } else
                return null;
            if (plist.Count == 1) {
                return new LineD(loc, plist[0]);
            } else if (plist.Count == 2) {
                if (pick.pos.length(plist[0]) < pick.pos.length(plist[1]))
                    return new LineD(loc, plist[0]);
                else
                    return new LineD(loc, plist[1]);
            }
            return null;
        }

        /// <summary>
        /// 円弧同士の接線を作成する
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
            List<ArcD> arcList = null;
            if (pickList.Count == 2 && loc.Count == 1) {
                Entity ent0 = mEntityList[pickList[0].no];
                Entity ent1 = mEntityList[pickList[1].no];
                if ((ent0.mEntityId == EntityId.Line || ent0.mEntityId == EntityId.Polyline || ent0.mEntityId == EntityId.Polygon) &&
                    (ent1.mEntityId == EntityId.Line || ent1.mEntityId == EntityId.Polyline || ent1.mEntityId == EntityId.Polygon)) {
                    //  2線分に接する円
                    LineD l0 = ent0.getLine(pickList[0].pos);
                    LineD l1 = ent1.getLine(pickList[1].pos);
                    if (!l0.isNaN() && !l1.isNaN()) {
                        r = r == 0 ? l0.distance(loc[0]) : r;
                        ArcD arc = new ArcD();
                        arcList = arc.tangentCircle(l0, l1, r);
                    }
                } else if ((ent0.mEntityId == EntityId.Line || ent0.mEntityId == EntityId.Polyline || ent0.mEntityId == EntityId.Polygon) &&
                        ent1.mEntityId == EntityId.Arc) {
                    //  線分と円弧に接する円
                    LineD l0 = ent0.getLine(pickList[0].pos);
                    ArcD arc = ((ArcEntity)ent1).mArc;
                    r = r == 0 ? l0.distance(loc[0]) : r;
                    arcList = arc.tangentCircle(l0, arc, r);
                } else if ((ent1.mEntityId == EntityId.Line || ent1.mEntityId == EntityId.Polyline || ent1.mEntityId == EntityId.Polygon) &&
                        ent0.mEntityId == EntityId.Arc) {
                    //  円弧と線分に接する円
                    LineD l1 = ent1.getLine(pickList[1].pos);
                    ArcD arc = ((ArcEntity)ent0).mArc;
                    r = r == 0 ? l1.distance(loc[0]) : r;
                    arcList = arc.tangentCircle(l1, arc, r);
                } else if (ent0.mEntityId == EntityId.Arc && ent1.mEntityId == EntityId.Arc) {
                    //  2円弧に接する円
                    ArcD arc0 = ((ArcEntity)ent0).mArc;
                    ArcD arc1 = ((ArcEntity)ent1).mArc;
                    r = r == 0 ? Math.Abs(arc0.mCp.length(loc[0]) - arc0.mR) : r;
                    arcList = arc0.tangentCircle(arc0, arc1, r);
                } else {
                    return null;
                }
            } else if (pickList.Count == 3 && loc.Count == 1) {
                Entity ent0 = mEntityList[pickList[0].no];
                Entity ent1 = mEntityList[pickList[1].no];
                Entity ent2 = mEntityList[pickList[2].no];
                if ((ent0.mEntityId == EntityId.Line || ent0.mEntityId == EntityId.Polyline|| ent0.mEntityId == EntityId.Polygon)
                    && (ent1.mEntityId == EntityId.Line || ent1.mEntityId == EntityId.Polyline || ent1.mEntityId == EntityId.Polygon)
                    && (ent2.mEntityId == EntityId.Line || ent2.mEntityId == EntityId.Polyline || ent2.mEntityId == EntityId.Polygon)) {
                    //  3線分に接する円
                    ArcD arc = new ArcD();
                    arcList = arc.tangentCircle(ent0.getLine(pickList[0].pos), ent1.getLine(pickList[1].pos), ent2.getLine(pickList[2].pos));
                }
            }
            if (arcList != null)
                return arcList.MinBy(p => p.mCp.length(loc[0]));
            else
                return null;
        }

        /// <summary>
        /// 表示する要素をフィルタリング
        /// </summary>
        /// <param name="entity">要素</param>
        /// <returns></returns>
        private bool drawChk(Entity entity)
        {
            if (!entity.mRemove
                && 0 != (entity.mLayerBit & mPara.mDispLayerBit)
                && entity.mEntityId != EntityId.Link)
                return true;
            return false;
        }

        /// <summary>
        /// 表示する要素をカウント
        /// </summary>
        /// <returns>表示用素数</returns>
        public int drawEntityCount()
        {
            int count = 0;
            foreach (var entity in mEntityList) {
                if (drawChk(entity))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 全データを表示する
        /// イメージ要素を先に表示する
        /// </summary>
        /// <param name="ydraw"></param>
        public void drawingAll(YWorldDraw ydraw)
        {
            //  背景属性要素を優先に表示
            foreach (var entity in mEntityList) {
                if (drawChk(entity) && entity.mBackDisp
                    && !ydraw.mClipBox.outsideChk(entity.mArea))
                    entity.draw(ydraw);
            }
            //  背景属性以外を表示
            foreach (var entity in mEntityList) {
                if (drawChk(entity) && !entity.mBackDisp
                    && !ydraw.mClipBox.outsideChk(entity.mArea))
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
                Entity entity = mEntityList[entNo.no].toCopy();
                entity.translate(vec);
                addEntity(entity);
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
        /// <param name="sp">回転開始座標</param>
        /// <param name="ep">回転終了座標</param>
        /// <param name="copy">コピー作成</param>
        /// <returns></returns>
        public bool rotate(List<(int no, PointD pos)> pickList, PointD cp, PointD sp, PointD ep, bool copy = false)
        {
            double ang = sp.angle(cp);
            PointD mp = ep.toCopy();
            mp.rotate(cp, -ang);
            foreach ((int no, PointD pos) entNo in pickList) {
                Entity entity = mEntityList[entNo.no].toCopy();
                entity.rotate(cp, mp);
                addEntity(entity);
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
                Entity entity = mEntityList[entNo.no].toCopy();
                entity.mirror(sp, ep);
                addEntity(entity);
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
                Entity entity = mEntityList[entNo.no].toCopy();
                entity.scale(cp, scale);
                addEntity(entity);
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
                Entity entity = mEntityList[entNo.no].toCopy();
                entity.offset(sp, ep);
                addEntity(entity);
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
                Entity entity = mEntityList[entNo.no].toCopy();
                entity.trim(sp, ep);
                addEntity(entity);
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
                    addEntity(ent);
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
        /// <param name="arc">円弧ストレッチ(Polyline/Polygonのみ)</param>
        /// <param name="copy">コピー作成</param>
        /// <returns></returns>
        public bool stretch(List<(int no, PointD pos)> pickList, PointD vec, bool arc = false, bool copy = false)
        {
            foreach ((int no, PointD pos) entNo in pickList) {
                Entity entity = mEntityList[entNo.no].toCopy();
                entity.stretch(vec, entNo.pos, arc);
                addEntity(entity);
                if (!copy)
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
            List<ArcD> arcs;
            foreach ((int no, PointD pos) entNo in pickList) {
                Entity entity = mEntityList[entNo.no];
                getProperty(entity);
                switch (entity.mEntityId) {
                    case EntityId.Polyline:
                        PolylineEntity polylineEnt = (PolylineEntity)entity;
                        lines = polylineEnt.mPolyline.toLineList(true);
                        foreach (var line in lines) {
                            addLine(line);
                        }
                        arcs = polylineEnt.mPolyline.toArcList();
                        foreach (var arc in arcs) {
                            addArc(arc);
                        }
                        break;
                    case EntityId.Polygon:
                        PolygonEntity polygonEnt = (PolygonEntity)entity;
                        lines = polygonEnt.mPolygon.toLineList(true);
                        foreach (var line in lines) {
                            addLine(line);
                        }
                        arcs = polygonEnt.mPolygon.toArcList();
                        foreach (var arc in arcs) {
                            addArc(arc);
                        }
                        break;
                    case EntityId.Parts:
                        PartsEntity partsEnt = (PartsEntity)entity;
                        foreach (var point in partsEnt.mParts.mPoints) {
                            addPoint(point);
                        }
                        foreach (var line in partsEnt.mParts.mLines) {
                            addLine(line);
                        }
                        foreach (var arc in partsEnt.mParts.mArcs) {
                            addArc(arc);
                        }
                        foreach (var text in partsEnt.mParts.mTexts) {
                            addText(text);
                        }
                        break;
                }
                removeEnt(entNo.no);
            }
            updateData();
            return true;

        }

        /// <summary>
        /// 要素の追加
        /// </summary>
        /// <param name="entity">要素</param>
        /// <returns>要素番号</returns>
        public int addEntity(Entity entity)
        {
            //  最終有効要素以降の要素を削除
            for (int i = mEntityList.Count - 1; 0 <= i; i--) {
                if (mEntityList[i].mRemove)
                    mEntityList.RemoveAt(i);
                else
                    break;
            }
            //  要素の追加
            mEntityList.Add(entity);
            //  表示領域、リンクNo、オペレーションカウントの更新
            if (mArea == null) {
                mArea = entity.mArea;
            } else {
                mArea.extension(entity.mArea);
            }
            entity.mNo = mEntityList.Count - 1;
            entity.mOperationCount = mOperationCount;
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
            link.mRemove = false;
            link.mLinkNo = entNo;
            link.mOperationCount = mOperationCount;
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
                int entNo = lastEntNo();
                int opeNo = mEntityList[entNo].mOperationCount;
                while (0 <= entNo && 0 <= opeNo && opeNo == mEntityList[entNo].mOperationCount) {
                    if (mEntityList[entNo].mEntityId == EntityId.Link) {
                        LinkEntity linkEnt = (LinkEntity)mEntityList[entNo];
                        mEntityList[linkEnt.mLinkNo].mRemove = false;
                    }
                    mEntityList[entNo].mRemove = true;
                    entNo--;
                }
            }
        }

        /// <summary>
        /// リドゥ処理
        /// </summary>
        public void redo()
        {
            if (0 < mEntityList.Count) {
                int entNo = lastEntNo() + 1;
                if (mEntityList.Count <= entNo) return;
                int opeNo = mEntityList[entNo].mOperationCount;
                while (entNo < mEntityList.Count && 0 <= opeNo && opeNo == mEntityList[entNo].mOperationCount) {
                    if (mEntityList[entNo].mEntityId == EntityId.Link) {
                        LinkEntity linkEnt = (LinkEntity)mEntityList[entNo];
                        mEntityList[linkEnt.mLinkNo].mRemove = true;
                    }
                    mEntityList[entNo].mRemove = false;
                    entNo++;
                }
            }
        }

        /// <summary>
        /// 有効要素の最終位置
        /// </summary>
        /// <returns>要素位置</returns>
        private int lastEntNo()
        {
            for (int i = mEntityList.Count - 1; 0 <= i; i--)
                if (!mEntityList[i].mRemove)
                    return i;
            return 0;
        }

        /// <summary>
        /// 要素領域、要素番号、レイヤービットの更新、
        /// </summary>
        public void updateData()
        {
            mLayer.clear();
            if (mEntityList != null && 0 < mEntityList.Count) {
                //  レイヤービットの更新
                for (int i = 0; i < mEntityList.Count; i++) {
                    if (!mEntityList[i].mRemove && mEntityList[i].mEntityId != EntityId.Link) {
                        if (mEntityList[i].mLayerName.Length == 0)
                            mEntityList[i].mLayerName = mPara.mCreateLayerName;
                        mEntityList[i].mLayerBit = mLayer.setLayerBit(mEntityList[i].mLayerName);
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
            mLayer.addDispLayer(mPara.mCreateLayerName);
        }

        /// <summary>
        /// Boxの内側か交点をもつ要素を検索して最初に見つかった要素No
        /// </summary>
        /// <param name="b">検索Box</param>
        /// <returns>検索要素No</returns>
        public List<int> findIndex(Box b, EntityId entityMask = EntityId.Non)
        {
            List<int> picks = new List<int>();
            for (int i = 0; i < mEntityList.Count; i++) {
                if (mEntityList[i].mRemove || 0 == (mEntityList[i].mLayerBit & mPara.mDispLayerBit)
                    || (entityMask != EntityId.Non && mEntityList[i].mEntityId != entityMask)
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
                        newEntity.mOperationCount = mOperationCount;
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
                try {
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
                    } else if (0 <= property[0].IndexOf(EntityId.Image.ToString()) && mImageData != null) {
                        //  イメージ要素
                        ImageEntity imageEntity = new ImageEntity(mImageData);
                        imageEntity.setProperty(property);
                        imageEntity.setData(dataStr);
                        imageEntity.fileUpdate();
                        return imageEntity;
                    }
                } catch (Exception e) {
                    System.Diagnostics.Debug.WriteLine($"{property[0]} {e.Message}");
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
            mEntityList.Clear();
            mPara.init();
            List<string[]> dataList = ylib.loadCsvData(path);
            if (dataList.Count <= 0 && dataList[0][0] != "Property")
                return;
            for (int i = 0; i < dataList.Count - 1; i++) {
                Entity entity = setStringEntityData(dataList[i], dataList[i + 1]);
                if (entity != null) {
                    if (entity.mLayerName == "") {
                        entity.mLayerName = mPara.mCreateLayerName;
                    }
                    entity.mLayerBit = mLayer.setLayerBit(entity.mLayerName);
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
                    } else if (0 <= dataList[i][0].IndexOf(EntityId.Group.ToString())) {
                        //  グループリスト
                        if (i + 1 < dataList.Count) {
                            mGroup.setDataList(dataList[++i]);
                        }
                    }
                }
            }
            updateData();
            mFirstEntityCount = mEntityList.Count;
            mOperationCount = 0;
        }

        /// <summary>
        /// 要素リストのファイル保存
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="noOpeSave">非変更保存</param>
        public void saveData(string path, bool noOpeSave = true)
        {
            if (noOpeSave &&
                (mFirstEntityCount == mEntityList.Count && 0 == mOperationCount))
                return;
            List<string[]> listData = new List<string[]>();
            //  図面のプロパティ
            string[] buf = { EntityId.Property.ToString() };
            listData.Add(buf);
            buf = mPara.propertyToString().Split(new char[] { ',' });
            listData.Add(buf);
            //  図面コメント情報
            buf = new string[] { EntityId.Comment.ToString() };
            listData.Add(buf);
            listData.Add(mPara.commentToString());
            //  グループリスト情報
            buf = new string[] { EntityId.Group.ToString() };
            mGroup.squeeze(mEntityList);
            listData.Add(buf);
            listData.Add(mGroup.toDataList());
            //  要素データ
            foreach (Entity entity in mEntityList) {
                if (entity != null && !entity.mRemove && 
                    entity.mEntityId != EntityId.Non &&
                    entity.mEntityId != EntityId.Link) {
                    listData.Add(entity.toList().ToArray());
                    listData.Add(entity.toDataList().ToArray());
                }
            }
            ylib.saveCsvData(path, listData);
        }
    }
}
