using CoreLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation.Text;
using System.Windows.Media;

namespace CadApp
{
    public enum EntityId { 
        Non, Link, Point, Line, Arc, Circle, Ellipse, Oval, Polyline, Polygon,
        Text, Property
    }
    public enum PointType { Dot, Cross, Plus, Squre, Circle }
    public enum LineType {  Solid, Center, Dash }

    /// <summary>
    /// 要素のベースクラス
    /// </summary>
    public abstract class Entity
    {
        //  属性
        public int mNo = -1;
        public EntityId mEntityId = EntityId.Non;
        public Brush mColor = Brushes.Black;
        public double mThickness = 1.0;
        public int mType = 0;
        public bool mRemove = false;
        public int mRemoveLink = -1;
        public int mOperationCount = -1;
        //  表示領域
        public Box mArea;
        //  ピック
        public bool mPick = false;
        public Brush mPickColor = Brushes.Red;

        private YDraw ydraw = new YDraw();

        /// <summary>
        /// 要素属性の設定
        /// </summary>
        /// <param name="color">色</param>
        /// <param name="thickness">太さ</param>
        /// <param name="type">点種/線種</param>
        public void setProperty(Brush color, double thickness, int type = 0)
        {
            mColor = color;
            mThickness = thickness;
            mType = type;
        }

        /// <summary>
        /// 要素属性の設定
        /// </summary>
        /// <param name="property">文字列データ</param>
        public void setProperty(string[] property)
        {
            if (1 < property.Length)
                mColor = ydraw.getColor(property[1].Trim());
            if (2 < property.Length)
                mThickness = double.Parse(property[2].Trim());
            if (3 < property.Length)
                mType = int.Parse(property[3].Trim());
        }

        /// <summary>
        /// Brush から色名に変換
        /// </summary>
        /// <param name="color">Brush</param>
        /// <returns></returns>
        public string getColorName(Brush color)
        {
            return ydraw.getColorName(color);
        }

        /// <summary>
        /// ラジアンから度に変換
        /// </summary>
        /// <param name="rad">ラジアン(rad)</param>
        /// <returns>度(deg)</returns>
        public double R2D(double rad)
        {
            return rad * 180.0 / Math.PI;
        }

        /// <summary>
        /// 度からラジアンに変換
        /// </summary>
        /// <param name="deg">度(deg)</param>
        /// <returns>ラジアン(rad)</returns>
        public double D2R(double deg)
        {
            return deg * Math.PI / 180.0;
        }

        /// <summary>
        /// 座標データの設定
        /// </summary>
        /// <param name="data">文字配列</param>
        abstract public void setData(string[] data);

        /// <summary>
        /// 要素の描画
        /// </summary>
        /// <param name="ydraw"></param>
        abstract public void draw(YWorldDraw ydraw);
        
        /// <summary>
        /// 要素属性を文字列に変換
        /// </summary>
        /// <returns></returns>
        public string toString()
        {
            return $"{mEntityId},{ydraw.getColorName(mColor)},{mThickness},{mType}";
        }

        /// <summary>
        /// 座標データを文字列に変換
        /// </summary>
        /// <returns></returns>
        abstract public string toDataString();

        /// <summary>
        /// 要素のコピーを作成
        /// </summary>
        /// <returns></returns>
        abstract public Entity toCopy();

        /// <summary>
        /// 要素情報を文字列に変換
        /// </summary>
        /// <param name="entNo">要素番号</param>
        /// <returns></returns>
        abstract public string entityInfo();

        /// <summary>
        /// 要素情報の要約を取得
        /// </summary>
        /// <returns></returns>
        abstract public string getSummary();

        /// <summary>
        /// 要素を移動する
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        abstract public void translate(PointD vec);

        /// <summary>
        /// 要素の回転
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        abstract public void rotate(PointD cp, PointD mp);

        /// <summary>
        /// 要素のミラー
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        abstract public void mirror(PointD sp, PointD ep);

        /// <summary>
        /// 要素のトリム
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        abstract public void trim(PointD sp, PointD ep);

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="dp">分割参照点</param>
        /// <returns>要素リスト</returns>
        abstract public List<Entity> divide(PointD dp);

        /// <summary>
        /// 要素のストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピックした位置</param>
        abstract public void stretch(PointD vec, PointD pickPos);

        /// <summary>
        /// 参照点に対する垂点を求める
        /// </summary>
        /// <param name="pos">参照点</param>
        /// <returns>垂点</returns>
        abstract public PointD onPoint(PointD pos);
        
        /// <summary>
        /// 要素同士の交点を求める
        /// </summary>
        /// <param name="entity">要素データ</param>
        /// <returns>交点リスト</returns>
        abstract public List<PointD> intersection(Entity entity);

        /// <summary>
        /// 要素上の分割位置を求める
        /// </summary>
        /// <param name="divideNo">分割数</param>
        /// <param name="pos">参照点</param>
        /// <returns>分割点</returns>
        abstract public PointD dividePos(int divideNo, PointD pos);

        /// <summary>
        /// 表示範囲の更新
        /// </summary>
        abstract public void updateArea();
    }

    public class LinkEntity : Entity
    {
        public int mLinkNo = -1;

        public LinkEntity()
        {
            mEntityId = EntityId.Link;
        }

        /// <summary>
        /// 座標データの設定
        /// </summary>
        /// <param name="data">文字配列</param>
        public override void setData(string[] data)
        {

        }

        /// <summary>
        /// 要素の描画
        /// </summary>
        /// <param name="ydraw"></param>
        public override void draw(YWorldDraw ydraw)
        {

        }

        /// <summary>
        /// 座標データを文字列に変換
        /// </summary>
        /// <returns></returns>
        public override string toDataString()
        {
            return "";
        }

        /// <summary>
        /// 要素のコピーを作成
        /// </summary>
        /// <returns></returns>
        public override Entity toCopy()
        {
            LinkEntity entity = new LinkEntity();
            entity.mColor = mColor;
            entity.mThickness = mThickness;
            entity.mType = mType;
            entity.mRemove = mRemove;
            entity.mLinkNo = mLinkNo;
            return entity;
        }

        /// <summary>
        /// 要素情報を文字列に変換
        /// </summary>
        /// <param name="entNo">要素番号</param>
        /// <returns></returns>
        public override string entityInfo()
        {
            return "";
        }

        /// <summary>
        /// 要素情報の要約を取得
        /// </summary>
        /// <returns></returns>
        public override string getSummary()
        {
            return "";
        }

        /// <summary>
        /// 要素を移動する
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        public override void translate(PointD vec)
        {

        }

        /// <summary>
        /// 要素の回転
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        public override void rotate(PointD cp, PointD mp)
        {

        }

        /// <summary>
        /// 要素のミラー
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void mirror(PointD sp, PointD ep)
        {

        }

        /// <summary>
        /// 要素のトリム
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void trim(PointD sp, PointD ep)
        {

        }

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="dp">分割参照点</param>
        /// <returns>要素リスト</returns>
        public override List<Entity> divide(PointD dp)
        {
            return null;
        }

        /// <summary>
        /// 要素のストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピックした位置</param>
        public override void stretch(PointD vec, PointD pickPos)
        {

        }

        /// <summary>
        /// 参照点に対する垂点を求める
        /// </summary>
        /// <param name="pos">参照点</param>
        /// <returns>垂点</returns>
        public override PointD onPoint(PointD pos)
        {
            return null;
        }

        /// <summary>
        /// 要素同士の交点を求める
        /// </summary>
        /// <param name="entity">要素データ</param>
        /// <returns>交点リスト</returns>
        public override List<PointD> intersection(Entity entity)
        {
            return null;
        }

        /// <summary>
        /// 要素上の分割位置を求める
        /// </summary>
        /// <param name="divideNo">分割数</param>
        /// <param name="pos">参照点</param>
        /// <returns>分割点</returns>
        public override PointD dividePos(int divideNo, PointD pos)
        {
            return null;
        }

        /// <summary>
        /// 表示範囲の更新
        /// </summary>
        public override void updateArea()
        {

        }
    }

    /// <summary>
    /// 点要素クラス
    /// </summary>
    public class PointEntity : Entity
    {
        //  座標データ
        public PointD mPoint = new PointD();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PointEntity()
        {
            mEntityId = EntityId.Point;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="p">座標データ</param>
        public PointEntity(PointD p)
        {
            mEntityId = EntityId.Point;
            mPoint = p.toCopy();
            mArea = new Box(mPoint);
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns></returns>
        public override Entity toCopy()
        {
            PointEntity point = new PointEntity(mPoint.toCopy());
            point.mColor = mColor;
            point.mThickness = mThickness;
            point.mType = mType;
            point.mRemove = mRemove;
            return point;
        }

        /// <summary>
        /// 座標データを文字列で設定
        /// </summary>
        /// <param name="data">文字配列データ</param>
        public override void setData(string[] data)
        {
            if (1 < data.Length) {
                mPoint.x = double.Parse(data[0]);
                mPoint.y = double.Parse(data[1]);
                mArea = new Box(mPoint);
            }
        }

        /// <summary>
        /// 描画処理
        /// </summary>
        /// <param name="ydraw"></param>
        public override void draw(YWorldDraw ydraw)
        {
            ydraw.mBrush = mPick ? mPickColor : mColor;
            ydraw.mPointSize = mThickness;
            ydraw.mPointType = mType;
            ydraw.drawWPoint(mPoint);
        }

        /// <summary>
        /// 座標データの文字列変換
        /// </summary>
        /// <returns></returns>
        public override string toDataString()
        {
            return $"{mPoint.x},{mPoint.y}";
        }

        /// <summary>
        /// 要素情報を文字列に変換
        /// </summary>
        /// <returns></returns>
        public override string entityInfo()
        {
            string buf = "";
            buf += $"要素番号: {mNo}";
            buf += $"\n要素種別: 点要素";
            buf += $"\n座標: {mPoint.ToString("f4")}";
            buf += $"\nカラー: {getColorName(mColor)}";
            buf += $"\n大きさ: {mThickness}";

            return buf;
        }

        /// <summary>
        /// 要素情報の要約の取得
        /// </summary>
        /// <returns></returns>
        public override string getSummary()
        {
            return $"{mNo}:点{mPoint.ToString("f1")} {getColorName(mColor)}";
        }

        /// <summary>
        /// 要素を移動する
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        public override void translate(PointD vec)
        {
            mPoint.offset(vec);
            mArea.offset(vec);
        }

        /// <summary>
        /// 回転
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        public override void rotate(PointD cp, PointD mp)
        {
            mPoint.rotate(cp, mp);
            mArea = new Box(mPoint);
        }

        /// <summary>
        /// ミラー
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void mirror(PointD sp, PointD ep)
        {
            mPoint.mirror(sp, ep);
            mArea = new Box(mPoint);
        }

        /// <summary>
        /// トリム(動作なし)
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void trim(PointD sp, PointD ep)
        {

        }

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="dp">分割参照点</param>
        /// <returns>要素リスト</returns>
        public override List<Entity> divide(PointD dp)
        {
            return null;
        }

        /// <summary>
        /// 要素のストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        public override void stretch(PointD vec, PointD pickPos)
        {
            mPoint.translate(vec);
            mArea.offset(vec);
        }

        /// <summary>
        /// 参照点に対する垂点を求める
        /// </summary>
        /// <param name="pos">参照点</param>
        /// <returns>垂点</returns>
        public override PointD onPoint(PointD pos)
        {
            return mPoint;
        }

        /// <summary>
        /// 要素同士の交点を求める
        /// </summary>
        /// <param name="entity">要素データ</param>
        /// <returns>交点リスト</returns>
        public override List<PointD> intersection(Entity entity)
        {
            List<PointD> plist = new List<PointD>();
            PointD ip = null;
            switch (entity.mEntityId) {
                case EntityId.Point:
                    break;
                case EntityId.Line:
                    LineEntity line = (LineEntity)entity;
                    ip = line.mLine.intersection(mPoint);
                    plist.Add(ip);
                    break;
                case EntityId.Arc:
                    ArcEntity arc = (ArcEntity)entity;
                    ip = arc.mArc.intersection(mPoint);
                    plist.Add(ip);
                    break;
                case EntityId.Polyline:
                    PolylineEntity polyline = (PolylineEntity)entity;
                    ip = polyline.mPolyline.nearPoint(mPoint);
                    plist.Add(ip);
                    break;
                case EntityId.Polygon:
                    PolygonEntity polygon = (PolygonEntity)entity;
                    ip = polygon.mPolygon.nearPoint(mPoint);
                    plist.Add(ip);
                    break;
                case EntityId.Text:
                    break;
            }
            return plist;
        }

        /// <summary>
        /// 要素上の分割位置を求める
        /// </summary>
        /// <param name="divideNo">分割数</param>
        /// <param name="pos">参照点</param>
        /// <returns>分割点</returns>
        public override PointD dividePos(int divideNo, PointD pos)
        {
            return mPoint;
        }

        /// <summary>
        /// 表示範囲の更新
        /// </summary>
        public override void updateArea()
        {
            mArea = new Box(mPoint);
        }
    }

    /// <summary>
    /// 線分要素クラス
    /// </summary>
    public class LineEntity : Entity
    {
        //  座標データ
        public LineD mLine;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LineEntity()
        {
            mEntityId = EntityId.Line;
            mLine = new LineD();
        }

        /// <summary>
        /// コンストラクタ(LineD設定)
        /// </summary>
        /// <param name="line"></param>
        public LineEntity(LineD line)
        {
            mEntityId = EntityId.Line;
            mLine = line.toCopy();
            mArea = new Box(mLine);
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns></returns>
        public override Entity toCopy()
        {
            LineEntity line = new LineEntity(mLine);
            line.mColor = mColor;
            line.mThickness = mThickness;
            line.mType = mType;
            line.mRemove = mRemove;
            return line;
        }

        /// <summary>
        /// 文字データによる座標設定
        /// </summary>
        /// <param name="data"></param>
        public override void setData(string[] data)
        {
            if (3 < data.Length) {
                mLine.ps.x = double.Parse(data[0]);
                mLine.ps.y = double.Parse(data[1]);
                mLine.pe.x = double.Parse(data[2]);
                mLine.pe.y = double.Parse(data[3]);
                mArea = new Box(mLine);
            }
        }

        /// <summary>
        /// 描画処理
        /// </summary>
        /// <param name="ydraw"></param>
        public override void draw(YWorldDraw ydraw)
        {
            ydraw.mBrush = mPick ? mPickColor : mColor;
            ydraw.mThickness = mThickness;
            ydraw.mLineType = mType;
            ydraw.drawWLine(mLine);
        }

        /// <summary>
        /// 座標データの文字列変換
        /// </summary>
        /// <returns></returns>
        public override string toDataString()
        {
            return $"{mLine.ps.x},{mLine.ps.y},{mLine.pe.x},{mLine.pe.y}";
        }

        /// <summary>
        /// 要素情報を文字列に変換
        /// </summary>
        /// <param name="entNo">要素番号</param>
        /// <returns></returns>
        public override string entityInfo()
        {
            string buf = "";
            buf += $"要素番号: {mNo}";
            buf += $"\n要素種別: 線分要素";
            buf += $"\n始点 {mLine.ps.ToString("f4")} 終点 {mLine.pe.ToString("f4")}";
            buf += $"\n長さ {mLine.length().ToString("f4")}";
            buf += $"\nカラー: {getColorName(mColor)}";
            buf += $"\n太さ: {mThickness}";

            return buf;
        }

        /// <summary>
        /// 要素情報の要約の取得
        /// </summary>
        /// <returns></returns>
        public override string getSummary()
        {
            return $"{mNo}:線分{mLine.ToString("f1")} {getColorName(mColor)}";
        }

        /// <summary>
        /// 要素を移動する
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        public override void translate(PointD vec)
        {
            mLine.offset(vec);
            mArea.offset(vec);
        }

        /// <summary>
        /// 回転
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        public override void rotate(PointD cp, PointD mp)
        {
            mLine.rotate(cp, mp);
            mArea = new Box(mLine);
        }

        /// <summary>
        /// ミラー
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void mirror(PointD sp, PointD ep)
        {
            mLine.mirror(sp, ep);
            mArea = new Box(mLine);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void trim(PointD sp, PointD ep)
        {
            mLine.trim(sp, ep);
            mArea = new Box(mLine);
        }

        /// <summary>
        /// 要素のストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        public override void stretch(PointD vec, PointD pickPos)
        {
            mLine.stretch(vec, pickPos);
            mArea = new Box(mLine);
        }

        /// <summary>
        /// 要素を分割する
        /// </summary>
        /// <param name="dp">分割参照点</param>
        /// <returns>要素リスト</returns>
        public override List<Entity> divide(PointD dp)
        {
            List<Entity> entityList = new();
            PointD mp =mLine.intersection(dp);
            if (!mLine.onPoint(mp))
                return null;
            LineEntity lineEnt0 = (LineEntity)toCopy();
            lineEnt0.mLine.pe = mp;
            entityList.Add(lineEnt0);
            LineEntity lineEnt1 = (LineEntity)toCopy();
            lineEnt1.mLine.ps = mp;
            entityList.Add(lineEnt1);
            return entityList;
        }

        /// <summary>
        /// 参照点に対する垂点を求める
        /// </summary>
        /// <param name="pos">参照点</param>
        /// <returns>垂点</returns>
        public override PointD onPoint(PointD pos)
        {
            return mLine.intersection(pos);
        }

        /// <summary>
        /// 要素同士の交点を求める
        /// </summary>
        /// <param name="entity">要素データ</param>
        /// <returns>交点リスト</returns>
        public override List<PointD> intersection(Entity entity)
        {
            List<PointD> plist = new List<PointD>();
            PointD ip = null;
            switch (entity.mEntityId) {
                case EntityId.Point:
                    PointEntity point = (PointEntity)entity;
                    ip = mLine.intersection(point.mPoint);
                    plist.Add(ip);
                    break;
                case EntityId.Line:
                    LineEntity line = (LineEntity)entity;
                    ip = mLine.intersection(line.mLine);
                    if (ip != null)
                        plist.Add(ip);
                    break;
                case EntityId.Arc:
                    ArcEntity arc = (ArcEntity)entity;
                    plist = arc.mArc.intersection(mLine);
                    break;
                case EntityId.Polyline:
                    PolylineEntity polyline = (PolylineEntity)entity;
                    plist = polyline.mPolyline.intersection(mLine);
                    break;
                case EntityId.Polygon:
                    PolygonEntity polygon = (PolygonEntity)entity;
                    plist = polygon.mPolygon.intersection(mLine);
                    break;
                case EntityId.Text:
                    break;
            }
            return plist;
        }

        /// <summary>
        /// 要素上の分割位置を求める
        /// </summary>
        /// <param name="divideNo">分割数</param>
        /// <param name="pos">参照点</param>
        /// <returns>分割点</returns>
        public override PointD dividePos(int divideNo, PointD pos)
        {
            List<PointD> plist = mLine.dividePoints(divideNo);
            return plist.MinBy(p => p.length(pos));
        }

        /// <summary>
        /// 表示範囲の更新
        /// </summary>
        public override void updateArea()
        {
            mArea = new Box(mLine);
        }
    }

    /// <summary>
    /// ポリライン要素クラス
    /// </summary>
    public class PolylineEntity : Entity
    {
        //  座標データ
        public PolylineD mPolyline;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PolylineEntity()
        {
            mEntityId = EntityId.Polyline;
            mPolyline = new PolylineD();
        }

        /// <summary>
        /// コンストラクタ(LineD設定)
        /// </summary>
        /// <param name="line"></param>
        public PolylineEntity(List<PointD> polyline)
        {
            mEntityId = EntityId.Polyline;
            mPolyline = new PolylineD(polyline);
            mArea = mPolyline.getBox();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polyline"></param>
        public PolylineEntity(PolylineD polyline)
        {
            mEntityId = EntityId.Polyline;
            mPolyline = polyline.toCopy();
            mArea = mPolyline.getBox();
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns></returns>
        public override Entity toCopy()
        {
            PolylineEntity polyline = new PolylineEntity(mPolyline);
            polyline.mColor = mColor;
            polyline.mThickness = mThickness;
            polyline.mType = mType;
            polyline.mRemove = mRemove;
            return polyline;
        }

        /// <summary>
        /// 文字データによる座標設定
        /// </summary>
        /// <param name="data"></param>
        public override void setData(string[] data)
        {
            mPolyline = new PolylineD();
            if (2 < data.Length) {
                for(int i= 0; i < data.Length - 1; i+=2) {
                    PointD ps = new PointD();
                    ps.x = double.Parse(data[i]);
                    ps.y = double.Parse(data[i+1]);
                    mPolyline.Add(ps);
                }
                mArea = mPolyline.getBox();
            }
        }

        /// <summary>
        /// 描画処理
        /// </summary>
        /// <param name="ydraw"></param>
        public override void draw(YWorldDraw ydraw)
        {
            ydraw.mBrush = mPick ? mPickColor : mColor;
            ydraw.mThickness = mThickness;
            ydraw.mLineType = mType;
            ydraw.drawWPolyline(mPolyline);
        }

        /// <summary>
        /// 座標データの文字列変換
        /// </summary>
        /// <returns></returns>
        public override string toDataString()
        {
            string buf = "";
            foreach (PointD p in mPolyline.mPolyline) {
                buf += $"{p.x},{p.y},";
            }
            return buf.TrimEnd(',');
        }

        /// <summary>
        /// 要素情報を文字列に変換
        /// </summary>
        /// <returns></returns>
        public override string entityInfo()
        {
            double l = 0;
            for (int i = 0; i < mPolyline.mPolyline.Count - 1; i++)
                l += mPolyline.mPolyline[i].length(mPolyline.mPolyline[i + 1]);
            string buf = "";
            buf += $"要素番号: {mNo}";
            buf += $"\n要素種別: ポリライン要素";
            buf += $"\n始点 {mPolyline.mPolyline[0].ToString("f4")} 終点 {mPolyline.mPolyline[mPolyline.mPolyline.Count-1].ToString("f4")}";
            buf += $"\n点数 {mPolyline.mPolyline.Count.ToString("f0")}";
            buf += $"\n長さ {l.ToString("f4")}";
            buf += $"\nカラー: {getColorName(mColor)}";
            buf += $"\n太さ: {mThickness}";

            return buf;
        }

        /// <summary>
        /// 要素情報の要約の取得
        /// </summary>
        /// <returns></returns>
        public override string getSummary()
        {
            return $"{mNo}:ポリライン{mPolyline.mPolyline[0].ToString("f1")} {mPolyline.mPolyline[mPolyline.mPolyline.Count - 1].ToString("f1")} {getColorName(mColor)}";
        }

        /// <summary>
        /// 要素を移動する
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        public override void translate(PointD vec)
        {
            mPolyline.translate(vec);
            mArea.offset(vec);
        }

        /// <summary>
        /// 回転
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        public override void rotate(PointD cp, PointD mp)
        {
            mPolyline.rotate(cp, mp);
            mArea = mPolyline.getBox();
        }

        /// <summary>
        /// ミラー
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void mirror(PointD sp, PointD ep)
        {
            mPolyline.mirror(sp, ep);
            mArea = mPolyline.getBox();
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void trim(PointD sp, PointD ep)
        {
            mPolyline.trim(sp, ep);
            mArea = mPolyline.getBox();
        }

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="dp">分割参照点</param>
        /// <returns>要素リスト</returns>
        public override List<Entity> divide(PointD dp)
        {
            List<Entity> entityList = new();
            PointD mp = mPolyline.nearPoint(dp);
            if (mp == null)
                return null;
            int pos = mPolyline.nearPos(mp);
            PolylineEntity polylineEnt0 = (PolylineEntity)toCopy();
            int last = polylineEnt0.mPolyline.mPolyline.Count - 1;
            polylineEnt0.mPolyline.mPolyline.RemoveRange(pos + 1, last - pos);
            polylineEnt0.mPolyline.mPolyline.Add(mp);
            entityList.Add(polylineEnt0);
            PolylineEntity polylineEnt1 = (PolylineEntity)toCopy();
            polylineEnt1.mPolyline.mPolyline.RemoveRange(0, pos + 1);
            polylineEnt1.mPolyline.mPolyline.Insert(0, mp);
            entityList.Add(polylineEnt1);
            return entityList;
        }

        /// <summary>
        /// 要素のストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        public override void stretch(PointD vec, PointD pickPos)
        {
            mPolyline.stretch(vec, pickPos);
            mArea = mPolyline.getBox();
        }

        /// <summary>
        /// 参照点に対する垂点を求める
        /// </summary>
        /// <param name="pos">参照点</param>
        /// <returns>垂点</returns>
        public override PointD onPoint(PointD pos)
        {
            return mPolyline.nearPoint(pos);
        }

        /// <summary>
        /// 要素同士の交点を求める
        /// </summary>
        /// <param name="entity">要素データ</param>
        /// <returns>交点リスト</returns>
        public override List<PointD> intersection(Entity entity)
        {
            List<PointD> plist = new List<PointD>();
            PointD ip = null;
            switch (entity.mEntityId) {
                case EntityId.Point:
                    PointEntity point = (PointEntity)entity;
                    plist = mPolyline.intersection(point.mPoint);
                    break;
                case EntityId.Line:
                    LineEntity line = (LineEntity)entity;
                    plist = mPolyline.intersection(line.mLine);
                    break;
                case EntityId.Arc:
                    ArcEntity arc = (ArcEntity)entity;
                    plist = mPolyline.intersection(arc.mArc);
                    break;
                case EntityId.Polyline:
                    PolylineEntity polyline = (PolylineEntity)entity;
                    plist = mPolyline.intersection(polyline.mPolyline);
                    break;
                case EntityId.Polygon:
                    PolygonEntity polygon = (PolygonEntity)entity;
                    plist = mPolyline.intersection(polygon.mPolygon);
                    break;
                case EntityId.Text:
                    break;
            }
            return plist;
        }

        /// <summary>
        /// 要素上の分割位置を求める
        /// </summary>
        /// <param name="divideNo">分割数</param>
        /// <param name="pos">参照点</param>
        /// <returns>分割点</returns>
        public override PointD dividePos(int divideNo, PointD pos)
        {
            LineD line = mPolyline.nearLine(pos);
            List<PointD> plist = line.dividePoints(divideNo);
            return plist.MinBy(p => p.length(pos));
        }

        /// <summary>
        /// 表示範囲の更新
        /// </summary>
        public override void updateArea()
        {
            mArea = mPolyline.getBox();
        }
    }

    /// <summary>
    /// ポリゴン要素クラス
    /// </summary>
    public class PolygonEntity : Entity
    {
        //  座標データ
        public PolygonD mPolygon;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PolygonEntity()
        {
            mEntityId = EntityId.Polygon;
            mPolygon = new PolygonD();
        }

        /// <summary>
        /// コンストラクタ(LineD設定)
        /// </summary>
        /// <param name="line"></param>
        public PolygonEntity(List<PointD> polygon)
        {
            mEntityId = EntityId.Polygon;
            mPolygon = new PolygonD(polygon);
            mArea = new Box(mPolygon.mPolygon);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polygon"></param>
        public PolygonEntity(PolygonD polygon)
        {
            mEntityId = EntityId.Polygon;
            mPolygon = polygon.toCopy();
            mArea = new Box(mPolygon.mPolygon);
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns></returns>
        public override Entity toCopy()
        {
            PolygonEntity polygon = new PolygonEntity(mPolygon);
            polygon.mColor = mColor;
            polygon.mThickness = mThickness;
            polygon.mType = mType;
            polygon.mRemove = mRemove;
            return polygon;
        }

        /// <summary>
        /// 文字データによる座標設定
        /// </summary>
        /// <param name="data"></param>
        public override void setData(string[] data)
        {
            mPolygon = new PolygonD();
            if (2 < data.Length) {
                for (int i = 0; i < data.Length - 1; i += 2) {
                    PointD ps = new PointD();
                    ps.x = double.Parse(data[i]);
                    ps.y = double.Parse(data[i + 1]);
                    mPolygon.Add(ps);
                }
                mArea = new Box(mPolygon.mPolygon);
            }
        }

        /// <summary>
        /// 描画処理
        /// </summary>
        /// <param name="ydraw"></param>
        public override void draw(YWorldDraw ydraw)
        {
            ydraw.mBrush = mPick ? mPickColor : mColor;
            ydraw.mThickness = mThickness;
            ydraw.mLineType = mType;
            ydraw.drawWPolygon(mPolygon, false);
        }

        /// <summary>
        /// 座標データの文字列変換
        /// </summary>
        /// <returns></returns>
        public override string toDataString()
        {
            string buf = "";
            foreach (PointD p in mPolygon.mPolygon) {
                buf += $"{p.x},{p.y},";
            }
            return buf.TrimEnd(',');
        }

        /// <summary>
        /// 要素情報を文字列に変換
        /// </summary>
        /// <returns></returns>
        public override string entityInfo()
        {
            double l = 0;
            for (int i = 0; i < mPolygon.mPolygon.Count - 1; i++)
                l += mPolygon.mPolygon[i].length(mPolygon.mPolygon[i + 1]);
            string buf = "";
            buf += $"要素番号: {mNo}";
            buf += $"\n要素種別: ポリゴン要素";
            buf += $"\n始点 {mPolygon.mPolygon[0].ToString("f4")} 終点 {mPolygon.mPolygon[mPolygon.mPolygon.Count - 1].ToString("f4")}";
            buf += $"\n点数 {mPolygon.mPolygon.Count.ToString("f0")}";
            buf += $"\n長さ {l.ToString("f4")}";
            buf += $"\nカラー: {getColorName(mColor)}";
            buf += $"\n太さ: {mThickness}";

            return buf;
        }

        /// <summary>
        /// 要素情報の要約の取得
        /// </summary>
        /// <returns></returns>
        public override string getSummary()
        {
            return $"{mNo}:ポリゴン{mPolygon.mPolygon[0].ToString("f1")} {mPolygon.mPolygon[mPolygon.mPolygon.Count - 1].ToString("f1")} {getColorName(mColor)}";
        }

        /// <summary>
        /// 要素を移動する
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        public override void translate(PointD vec)
        {
            mPolygon.translate(vec);
            mArea.offset(vec);
        }

        /// <summary>
        /// 回転
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        public override void rotate(PointD cp, PointD mp)
        {
            mPolygon.rotate(cp, mp);
            mArea = new Box(mPolygon.mPolygon);
        }

        /// <summary>
        /// ミラー
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void mirror(PointD sp, PointD ep)
        {
            mPolygon.mirror(sp, ep);
            mArea = new Box(mPolygon.mPolygon);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void trim(PointD sp, PointD ep)
        {
        }

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="dp">分割参照点</param>
        /// <returns>要素リスト</returns>
        public override List<Entity> divide(PointD dp)
        {
            return null;
        }

        /// <summary>
        /// 要素のストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        public override void stretch(PointD vec, PointD pickPos)
        {
            mPolygon.stretch(vec, pickPos);
            mArea = mPolygon.getBox();
        }

        /// <summary>
        /// 参照点に対する垂点を求める
        /// </summary>
        /// <param name="pos">参照点</param>
        /// <returns>垂点</returns>
        public override PointD onPoint(PointD pos)
        {
            return mPolygon.nearPoint(pos);
        }

        /// <summary>
        /// 要素同士の交点を求める
        /// </summary>
        /// <param name="entity">要素データ</param>
        /// <returns>交点リスト</returns>
        public override List<PointD> intersection(Entity entity)
        {
            List<PointD> plist = new List<PointD>();
            PointD ip = null;
            switch (entity.mEntityId) {
                case EntityId.Point:
                    PointEntity point = (PointEntity)entity;
                    plist = mPolygon.intersection(point.mPoint);
                    break;
                case EntityId.Line:
                    LineEntity line = (LineEntity)entity;
                    plist = mPolygon.intersection(line.mLine);
                    break;
                case EntityId.Arc:
                    ArcEntity arc = (ArcEntity)entity;
                    plist = mPolygon.intersection(arc.mArc);
                    break;
                case EntityId.Polyline:
                    PolylineEntity polyline = (PolylineEntity)entity;
                    plist = mPolygon.intersection(polyline.mPolyline);
                    break;
                case EntityId.Polygon:
                    PolygonEntity polygon = (PolygonEntity)entity;
                    plist = mPolygon.intersection(polygon.mPolygon);
                    break;
                case EntityId.Text:
                    break;
            }
            return plist;
        }

        /// <summary>
        /// 要素上の分割位置を求める
        /// </summary>
        /// <param name="divideNo">分割数</param>
        /// <param name="pos">参照点</param>
        /// <returns>分割点</returns>
        public override PointD dividePos(int divideNo, PointD pos)
        {
            LineD line = mPolygon.nearLine(pos);
            List<PointD> plist = line.dividePoints(divideNo);
            return plist.MinBy(p => p.length(pos));
        }

        /// <summary>
        /// 表示範囲の更新
        /// </summary>
        public override void updateArea()
        {
            mArea = new Box(mPolygon.mPolygon);
        }
    }

    /// <summary>
    /// 円弧要素クラス
    /// </summary>
    public class ArcEntity : Entity
    {
        //  座標データ
        public ArcD mArc;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ArcEntity()
        {
            mEntityId = EntityId.Arc;
            mArc = new ArcD();
        }

        /// <summary>
        /// コンストラクタ(ArcD設定)
        /// </summary>
        /// <param name="line"></param>
        public ArcEntity(ArcD arc)
        {
            mEntityId = EntityId.Arc;
            mArc = arc.toCopy();
            mArea = new Box(mArc);
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns></returns>
        public override Entity toCopy()
        {
            ArcEntity arc = new ArcEntity(mArc);
            arc.mColor = mColor;
            arc.mThickness = mThickness;
            arc.mType = mType;
            arc.mRemove = mRemove;
            return arc;
        }

        /// <summary>
        /// 文字データによる座標設定
        /// </summary>
        /// <param name="data"></param>
        public override void setData(string[] data)
        {
            if (4 < data.Length) {
                mArc.mCp.x = double.Parse(data[0]);
                mArc.mCp.y = double.Parse(data[1]);
                mArc.mR = double.Parse(data[2]);
                mArc.mSa = double.Parse(data[3]);
                mArc.mEa = double.Parse(data[4]);
                mArea = new Box(mArc);
            }
        }

        /// <summary>
        /// 描画処理
        /// </summary>
        /// <param name="ydraw"></param>
        public override void draw(YWorldDraw ydraw)
        {
            ydraw.mBrush = mPick ? mPickColor : mColor;
            ydraw.mThickness = mThickness;
            ydraw.mLineType = mType;
            ydraw.drawWArc(mArc, false);
        }

        /// <summary>
        /// 座標データの文字列変換
        /// </summary>
        /// <returns></returns>
        public override string toDataString()
        {
            return $"{mArc.mCp.x},{mArc.mCp.y},{mArc.mR},{mArc.mSa},{mArc.mEa}";
        }

        /// <summary>
        /// 要素情報を文字列に変換
        /// </summary>
        /// <param name="entNo">要素番号</param>
        /// <returns></returns>
        public override string entityInfo()
        {
            string buf = "";
            buf += $"要素番号: {mNo}";
            buf += $"\n要素種別: 円弧要素";
            buf += $"\n中心 {mArc.mCp.ToString("f4")} 半径 {mArc.mR.ToString("f4")} 開始角 {R2D(mArc.mSa).ToString("f2")} 終了角 {R2D(mArc.mEa).ToString("f2")}";
            buf += $"\nカラー: {getColorName(mColor)}";
            buf += $"\n太さ: {mThickness}";

            return buf;
        }

        /// <summary>
        /// 要素情報の要約の取得
        /// </summary>
        /// <returns></returns>
        public override string getSummary()
        {
            return $"{mNo}:円弧{mArc.ToString("f1")} {getColorName(mColor)}";
        }

        /// <summary>
        /// 要素を移動する
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        public override void translate(PointD vec)
        {
            mArc.mCp.offset(vec);
            mArea.offset(vec);
        }

        /// <summary>
        /// 回転
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        public override void rotate(PointD cp, PointD mp)
        {
            mArc.rotate(cp, mp);
            mArea = new Box(mArc);
        }

        /// <summary>
        /// ミラー
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void mirror(PointD sp, PointD ep)
        {
            mArc.mirror(sp, ep);
            mArea = new Box(mArc);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void trim(PointD sp, PointD ep)
        {
            mArc.trim(sp, ep);
            mArea = new Box(mArc);
        }

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="dp">分割参照点</param>
        /// <returns>要素リスト</returns>
        public override List<Entity> divide(PointD dp)
        {
            List<Entity> entityList = new();
            PointD mp = mArc.intersection(dp);
            if (!mArc.onPoint(mp))
                return null;
            double ang = mArc.getAngle(mp);
            ArcEntity arcEnt0 = (ArcEntity)toCopy();
            arcEnt0.mArc.mEa = ang;
            entityList.Add(arcEnt0);
            ArcEntity arcEnt1 = (ArcEntity)toCopy();
            arcEnt1.mArc.mSa = ang;
            entityList.Add(arcEnt1);
            return entityList;
        }

        /// <summary>
        /// 要素のストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        public override void stretch(PointD vec, PointD pickPos)
        {
            mArc.stretch(vec, pickPos);
            mArea = new Box(mArc);
        }

        /// <summary>
        /// 参照点に対する垂点を求める
        /// </summary>
        /// <param name="pos">参照点</param>
        /// <returns>垂点</returns>
        public override PointD onPoint(PointD pos)
        {
            return mArc.intersection(pos);
        }

        /// <summary>
        /// 要素同士の交点を求める
        /// </summary>
        /// <param name="entity">要素データ</param>
        /// <returns>交点リスト</returns>
        public override List<PointD> intersection(Entity entity)
        {
            List<PointD> plist = new List<PointD>();
            PointD ip = null;
            switch (entity.mEntityId) {
                case EntityId.Point:
                    PointEntity point = (PointEntity)entity;
                    ip = mArc.intersection(point.mPoint);
                    plist.Add(ip);
                    break;
                case EntityId.Line:
                    LineEntity line = (LineEntity)entity;
                    plist = mArc.intersection(line.mLine);
                    break;
                case EntityId.Arc:
                    ArcEntity arc = (ArcEntity)entity;
                    plist = mArc.intersection(arc.mArc);
                    break;
                case EntityId.Polyline:
                    PolylineEntity polyline = (PolylineEntity)entity;
                    plist = mArc.intersection(polyline.mPolyline);
                    break;
                case EntityId.Polygon:
                    PolygonEntity polygon = (PolygonEntity)entity;
                    plist = mArc.intersection(polygon.mPolygon);
                    break;
                case EntityId.Text:
                    break;
            }
            return plist;
        }

        /// <summary>
        /// 要素上の分割位置を求める
        /// </summary>
        /// <param name="divideNo">分割数</param>
        /// <param name="pos">参照点</param>
        /// <returns>分割点</returns>
        public override PointD dividePos(int divideNo, PointD pos)
        {
            List<PointD> plist = mArc.dividePoints(divideNo);
            return plist.MinBy(p => p.length(pos));
        }

        /// <summary>
        /// 表示範囲の更新
        /// </summary>
        public override void updateArea()
        {
            mArea = new Box(mArc);
        }
    }

    /// <summary>
    /// テキスト要素クラス
    /// </summary>
    public class TextEntity : Entity
    {
        //  座標データ
        public TextD mText;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TextEntity()
        {
            mEntityId = EntityId.Text;
            mText = new TextD();
        }

        /// <summary>
        /// コンストラクタ(TextD設定)
        /// </summary>
        /// <param name="line"></param>
        public TextEntity(TextD text)
        {
            mEntityId = EntityId.Text;
            mText = text.toCopy();
            mArea = new Box(mText.getBox());
        }

        /// <summary>
        /// 複製を作成
        /// </summary>
        /// <returns></returns>
        public override Entity toCopy()
        {
            TextEntity text = new TextEntity(mText.toCopy());
            text.mColor = mColor;
            text.mThickness = mThickness;
            text.mType = mType;
            text.mRemove = mRemove;
            return text;
        }

        /// <summary>
        /// 文字データによる座標設定
        /// </summary>
        /// <param name="data"></param>
        public override void setData(string[] data)
        {
            if (4 < data.Length) {
                mText.mText = data[0];
                mText.mPos.x = double.Parse(data[1]);
                mText.mPos.y = double.Parse(data[2]);
                mText.mTextSize = double.Parse(data[3]);
                mText.mRotate = double.Parse(data[4]);
                mText.mHa = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), data[5]);
                mText.mVa = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), data[6]);
                mArea = new Box(mText.getBox());
            }
        }

        /// <summary>
        /// 描画処理
        /// </summary>
        /// <param name="ydraw"></param>
        public override void draw(YWorldDraw ydraw)
        {
            ydraw.mTextColor = mPick ? mPickColor : mColor;
            ydraw.mThickness = mThickness;
            ydraw.drawWText(mText);
        }

        /// <summary>
        /// 座標データの文字列変換
        /// </summary>
        /// <returns></returns>
        public override string toDataString()
        {
            return $"{mText.mText},{mText.mPos.x},{mText.mPos.y},{mText.mTextSize},{mText.mRotate},{mText.mHa},{mText.mVa}";
        }

        /// <summary>
        /// 要素情報を文字列に変換
        /// </summary>
        /// <param name="entNo">要素番号</param>
        /// <returns></returns>
        public override string entityInfo()
        {
            string buf = "";
            buf += $"要素番号: {mNo}";
            buf += $"\n要素種別: テキスト要素";
            buf += $"\n文字列　: {mText.mText}";
            buf += $"\n起点    : {mText.mPos.ToString("f4")}";
            buf += $"\n文字高さ: {mText.mTextSize.ToString("f4")} 水平位置 {mText.mHa} 垂直位置 {mText.mVa}";
            buf += $"\nカラー  : {getColorName(mColor)}";

            return buf;
        }

        /// <summary>
        /// 要素情報の要約の取得
        /// </summary>
        /// <returns></returns>
        public override string getSummary()
        {
            return $"{mNo}:テキスト{mText.mPos.ToString("f1")} {mText.mText.Substring(0, Math.Min(10, mText.mText.Length-1))} {getColorName(mColor)}";
        }

        /// <summary>
        /// 要素を移動する
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        public override void translate(PointD vec)
        {
            mText.mPos.offset(vec);
            mArea.offset(vec);
        }

        /// <summary>
        /// 文字列の回転
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        public override void rotate(PointD cp, PointD mp)
        {
            mText.rotate(cp, mp);
            mArea = mText.getBox();
        }
        /// <summary>
        /// ミラー
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void mirror(PointD sp, PointD ep)
        {
            mText.mirror(sp, ep);
            mArea = mText.getBox();
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void trim(PointD sp, PointD ep)
        {
        }

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="dp">分割参照点</param>
        /// <returns>要素リスト</returns>
        public override List<Entity> divide(PointD dp)
        {
            return null;
        }

        /// <summary>
        /// 要素のストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        public override void stretch(PointD vec, PointD pickPos)
        {

        }

        /// <summary>
        /// 参照点に対する垂点を求める
        /// </summary>
        /// <param name="pos">参照点</param>
        /// <returns>垂点</returns>
        public override PointD onPoint(PointD pos)
        {
            return mText.nearPoint(pos);
        }

        /// <summary>
        /// 交点を求める
        /// </summary>
        /// <param name="entity">要素データ</param>
        /// <returns>交点リスト</returns>
        public override List<PointD> intersection(Entity entity)
        {
            List<PointD> plist = new List<PointD>();
            PointD ip = null;
            switch (entity.mEntityId) {
                case EntityId.Point:
                    break;
                case EntityId.Line:
                    break;
                case EntityId.Arc:
                    break;
                case EntityId.Polyline:
                    break;
                case EntityId.Polygon:
                    break;
                case EntityId.Text:
                    break;
            }
            return plist;
        }

        /// <summary>
        /// 要素上の分割位置を求める
        /// </summary>
        /// <param name="divideNo">分割数</param>
        /// <param name="pos">参照点</param>
        /// <returns>分割点</returns>
        public override PointD dividePos(int divideNo, PointD pos)
        {
            LineD line = mText.nearLine(pos);
            List<PointD> plist = line.dividePoints(divideNo);
            return plist.MinBy(p => p.length(pos));
        }

        /// <summary>
        /// 表示範囲の更新
        /// </summary>
        public override void updateArea()
        {
            mArea = mText.getBox();
        }
    }

    /// <summary>
    /// 要素データクラス
    /// </summary>
    public class EntityData
    {
        public List<Entity> mEntityList;        //  要素リスト
        public Brush mColor = Brushes.Black;    //  デフォルトカラー
        public int mPointType = 0;              //  点種
        public int mLineType = 0;               //  点種/線種
        public double mThickness = 1.0;         //  デフォルト線太さ
        public double mPointSize = 1;           //  点の大きさ
        public Box mArea;                       //  要素領域
        public int mOperationCouunt = 0;

        private double mEps = 1E-8;

        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EntityData() { 
            mEntityList = new List<Entity>();
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
        /// プロパティ設定値を文字列に変換
        /// </summary>
        /// <returns></returns>
        public string propertyToString()
        {
            return $"{mColor},{mPointType},{mPointSize},{mLineType},{mThickness}";
        }

        /// <summary>
        /// 文字列配列をプロパティ設定値に変換
        /// </summary>
        /// <param name="data"></param>
        public void setPropertyData(string[] data)
        {
            if (4 < data.Length) {
                mColor = ylib.getColor(data[0]);
                mPointType = int.Parse(data[1]);
                mPointSize = double.Parse(data[2]);
                mLineType = int.Parse(data[3]);
                mThickness = double.Parse(data[4]);
            }
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
            pointEnt.setProperty(mColor, mPointSize, mPointType);
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
        /// <returns></returns>
        public int addLine(LineD pline)
        {
            if (pline == null || pline.length() < mEps)
                return -1;
            Entity lineEnt = new LineEntity(pline);
            lineEnt.setProperty(mColor, mThickness, mLineType);
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
        /// <returns></returns>
        public int addRect(PointD sp, PointD ep)
        {
            if (sp == null || ep == null || sp.length(ep) < mEps)
                return -1;
            Box b = new Box(sp, ep);
            List<PointD> plist = b.ToPointDList();
            return addPolygon(plist);
        }

        /// <summary>
        /// ポリラインの追加
        /// </summary>
        /// <param name="polyline">座標点リスト</param>
        /// <returns></returns>
        public int addPolyline(List<PointD> polyline)
        {
            if (polyline == null || polyline.Count < 2)
                return -1;
            Entity polylineEnt = new PolylineEntity(polyline);
            polylineEnt.setProperty(mColor, mThickness, mLineType);
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
        /// <returns></returns>
        public int addPolygon(List<PointD> polygon)
        {
            if (polygon == null || polygon.Count < 3) 
                return -1;
            Entity polygonEnt = new PolygonEntity(polygon);
            polygonEnt.setProperty(mColor, mThickness, mLineType);
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
        /// <returns></returns>
        public int addArc(ArcD arc)
        {
            if (arc == null || arc.mR < mEps || arc.mEa - arc.mSa < mEps)
                return -1;
            Entity arcEnt = new ArcEntity(arc);
            arcEnt.setProperty(mColor, mThickness, mLineType);
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
        /// テキスト要素の追加
        /// </summary>
        /// <param name="text">テキスト</param>
        /// <returns></returns>
        public int addText(TextD text)
        {
            if (text == null || text.mText.Length == 0 || text.mTextSize < 1)
                return -1;
            Entity textEnt = new TextEntity(text);
            textEnt.setProperty(mColor, mThickness, mLineType);
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
        /// 指定要素を移動する
        /// </summary>
        /// <param name="entNoList">要素リスト</param>
        /// <param name="offset">移動量</param>
        /// <param name="copy">コピー作成</param>
        /// <returns></returns>
        public bool translate(List<(int no, PointD pos)> entNoList, PointD offset, bool copy = false)
        {
            foreach ((int no, PointD pos) entNo in entNoList) {
                mEntityList.Add(mEntityList[entNo.no].toCopy());
                mEntityList[mEntityList.Count - 1].translate(offset);
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
        /// <param name="entNoList">要素リスト</param>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        /// <param name="copy">コピー作成</param>
        /// <returns></returns>
        public bool rotate(List<(int no, PointD pos)> entNoList, PointD cp, PointD mp, bool copy = false)
        {
            foreach ((int no, PointD pos) entNo in entNoList) {
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
        /// <param name="entNoList">要素リスト</param>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        /// <param name="copy">コピー作成</param>
        /// <returns></returns>
        public bool mirror(List<(int no, PointD pos)> entNoList, PointD sp, PointD ep, bool copy = false)
        {
            foreach ((int no, PointD pos) entNo in entNoList) {
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
        /// 要素をトリムする
        /// </summary>
        /// <param name="entNoList">要素リスト</param>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        /// <returns></returns>
        public bool trim(List<(int no, PointD pos)> entNoList, PointD sp, PointD ep)
        {
            foreach ((int no, PointD pos) entNo in entNoList) {
                mEntityList.Add(mEntityList[entNo.no].toCopy());
                mEntityList[mEntityList.Count - 1].trim(sp, ep);
                mEntityList[mEntityList.Count - 1].mOperationCount = mOperationCouunt;
                removeEnt(entNo.no);
            }
            updateData();
            return true;
        }

        /// <summary>
        /// 要素を分割する
        /// </summary>
        /// <param name="entNoList">要素リスト</param>
        /// <param name="dp">分割参照点</param>
        /// <returns></returns>
        public bool divide(List<(int no, PointD pos)> entNoList, PointD dp)
        {
            foreach ((int no, PointD pos) entNo in entNoList) {
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
        /// <param name="entNoList">要素リスト</param>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <returns></returns>
        public bool stretch(List<(int no, PointD pos)> entNoList, PointD vec)
        {
            foreach ((int no, PointD pos) entNo in entNoList) {
                mEntityList.Add(mEntityList[entNo.no].toCopy());
                mEntityList[mEntityList.Count - 1].stretch(vec, entNo.pos);
                mEntityList[mEntityList.Count - 1].mOperationCount = mOperationCouunt;
                removeEnt(entNo.no);
            }
            updateData();
            return true;
        }

        /// <summary>
        /// 指定要素を削除する
        /// </summary>
        /// <param name="entNoList">要素番号リスト</param>
        /// <returns></returns>
        public bool removeEnt(List<(int, PointD)> entNoList)
        {
            foreach ((int no, PointD pos) entNo in entNoList) {
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
                while (0 <= opeNo && opeNo == mEntityList[entNo].mOperationCount) {
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
        /// 要素領域、要素番号の更新
        /// </summary>
        public void updateData()
        {
            if (mEntityList != null && 0 < mEntityList.Count) {
                mArea = mEntityList[0].mArea.toCopy();
                for (int i = 0; i < mEntityList.Count; i++) {
                    if (!mEntityList[i].mRemove && mEntityList[i].mEntityId != EntityId.Link) {
                        mEntityList[i].updateArea();
                        mArea.extension(mEntityList[i].mArea);
                        mEntityList[i].mNo = i;
                    }
                }
            }
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
                if (mEntityList[i].mRemove || mEntityList[i].mEntityId == EntityId.Non)
                    continue;
                if (b.insideChk(mEntityList[i].mArea))      //  Boxの内側
                    picks.Add(i);
                if (b.outsideChk(mEntityList[i].mArea))     //  Boxの外側
                    continue;
                if (intersection(mEntityList[i], b))        //  Boxと交点あり
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
            return ent1.intersection(ent2);
        }

        /// <summary>
        /// 要素がBOX内かまたは交差しているか(BOX外でない)の判定
        /// </summary>
        /// <param name="ent">要素</param>
        /// <param name="b">検索Box</param>
        /// <returns>判定</returns>
        public bool intersection(Entity ent, Box b)
        {
            bool result = false;
            List<PointD> plist;
            switch (ent.mEntityId) {
                case EntityId.Point:
                    PointEntity point = (PointEntity) ent;
                    result = b.insideChk(point.mPoint);
                    break;
                case EntityId.Line:
                    LineEntity line = (LineEntity)ent;
                    plist = b.intersection(line.mLine);
                    result = 0 < plist.Count;
                    break;
                case EntityId.Polyline:
                    PolylineEntity polyline = (PolylineEntity) ent;
                    plist = b.intersection(polyline.mPolyline.mPolyline, false, true);
                    result = 0 < plist.Count;
                    break;
                case EntityId.Polygon:
                    PolygonEntity polygon = (PolygonEntity)ent;
                    plist = b.intersection(polygon.mPolygon.mPolygon, true, true);
                    result = 0 < plist.Count;
                    break;
                case EntityId.Arc:
                    ArcEntity arc = (ArcEntity)ent;
                    plist = b.intersection(arc.mArc);
                    result = 0 < plist.Count;
                    break;
                case EntityId.Text:
                    TextEntity text = (TextEntity)ent;
                    if (text.mText.insideChk(b)) {
                        result = true;
                    } else {
                        plist = b.intersection(text.mText.getArea());
                        result = 0 < plist.Count;
                    }
                    break;
            }
            return result;
        }

        /// <summary>
        /// 要素リストのファイル保存
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void saveData(string path)
        {
            string buf = "";
            buf += EntityId.Property.ToString() + "\n";
            buf += propertyToString() + "\n";
            foreach (Entity entity in mEntityList) {
                if (entity != null && !entity.mRemove && entity.mEntityId != EntityId.Non) {
                    buf += entity.toString() + "\n";
                    buf += entity.toDataString() + "\n";
                }
            }
            ylib.saveTextFile(path, buf);
        }

        /// <summary>
        /// 要素リストの読出し
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void loadData(string path)
        {
            mEntityList = new List<Entity>();
            string buf = ylib.loadTextFile(path);
            string[] dataList = buf.Split(new char[] { '\n' });
            for (int i = 0; i < dataList.Length; i++) {
                string[] entityStr = dataList[i].Split(new char[] { ',' });
                if (0 < entityStr.Length) {
                    if (0 <= entityStr[0].IndexOf(EntityId.Point.ToString())) {
                        //  点要素
                        PointEntity pointEntity = new PointEntity();
                        pointEntity.setProperty(entityStr);
                        if (i + 1 < dataList.Length) {
                            string[] dataStr = dataList[i + 1].Split(new char[] { ',' });
                            pointEntity.setData(dataStr);
                            i++;
                            mEntityList.Add(pointEntity);
                        }
                    } else if (0 <= entityStr[0].IndexOf(EntityId.Line.ToString())) {
                        //  線分要素
                        LineEntity lineEntity = new LineEntity();
                        lineEntity.setProperty(entityStr);
                        if (i+ 1 < dataList.Length) {
                            string[] dataStr = dataList[i + 1].Split(new char[] { ',' });
                            lineEntity.setData(dataStr);
                            i++;
                            mEntityList.Add(lineEntity);
                        }
                    } else if (0 <= entityStr[0].IndexOf(EntityId.Polyline.ToString())) {
                        //  ポリライン要素
                        PolylineEntity polylineEntity = new PolylineEntity();
                        polylineEntity.setProperty(entityStr);
                        if (i + 1 < dataList.Length) {
                            string[] dataStr = dataList[i + 1].Split(new char[] { ',' });
                            polylineEntity.setData(dataStr);
                            i++;
                            mEntityList.Add(polylineEntity);
                        }
                    } else if (0 <= entityStr[0].IndexOf(EntityId.Polygon.ToString())) {
                        //  ポリゴン要素
                        PolygonEntity polygonEntity = new PolygonEntity();
                        polygonEntity.setProperty(entityStr);
                        if (i + 1 < dataList.Length) {
                            string[] dataStr = dataList[i + 1].Split(new char[] { ',' });
                            polygonEntity.setData(dataStr);
                            i++;
                            mEntityList.Add(polygonEntity);
                        }
                    } else if (0 <= entityStr[0].IndexOf(EntityId.Arc.ToString())) {
                        //  円弧要素
                        ArcEntity arcEntity = new ArcEntity();
                        arcEntity.setProperty(entityStr);
                        if (i + 1 < dataList.Length) {
                            string[] dataStr = dataList[i + 1].Split(new char[] { ',' });
                            arcEntity.setData(dataStr);
                            i++;
                            mEntityList.Add(arcEntity);
                        }
                    } else if (0 <= entityStr[0].IndexOf(EntityId.Text.ToString())) {
                        //  テキスト要素
                        TextEntity textEntity = new TextEntity();
                        textEntity.setProperty(entityStr);
                        if (i + 1 < dataList.Length) {
                            string[] dataStr = dataList[i + 1].Split(new char[] { ',' });
                            textEntity.setData(dataStr);
                            i++;
                            mEntityList.Add(textEntity);
                        }
                    } else if (0 <= entityStr[0].IndexOf(EntityId.Property.ToString())) {
                        //  システムプロパティ情報
                        if (i + 1 < dataList.Length) {
                            string[] dataStr = dataList[i + 1].Split(new char[] { ',' });
                            setPropertyData(dataStr);
                            i++;
                        }
                    }
                    if (0 < mEntityList.Count)
                        mEntityList[mEntityList.Count - 1].mNo = mEntityList.Count - 1;
                }
            }
            updateData();
        }
    }
}
