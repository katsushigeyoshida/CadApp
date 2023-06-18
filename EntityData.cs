using CoreLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CadApp
{
    public enum EntityId { 
        Non, Point, Line, Arc, Circle, Ellipse, Oval, Polyline, Polygon, Text
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
        /// 表示範囲の更新
        /// </summary>
        abstract public void updateArea();
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
        public PointEntity toCopy()
        {
            PointEntity point = new PointEntity(mPoint.toCopy());
            point.mColor = mColor;
            point.mThickness = mThickness;
            point.mType = mType;
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
        public LineEntity toCopy()
        {
            LineEntity line = new LineEntity(mLine);
            line.mColor = mColor;
            line.mThickness = mThickness;
            line.mType = mType;
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
        public List<PointD> mPolyline;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PolylineEntity()
        {
            mEntityId = EntityId.Polyline;
            mPolyline = new List<PointD>();
        }

        /// <summary>
        /// コンストラクタ(LineD設定)
        /// </summary>
        /// <param name="line"></param>
        public PolylineEntity(List<PointD> polylines)
        {
            mEntityId = EntityId.Polyline;
            mPolyline = new List<PointD>();
            polylines.ForEach(p => mPolyline.Add(p));
            mArea = new Box(mPolyline);
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns></returns>
        public PolylineEntity toCopy()
        {
            PolylineEntity polyline = new PolylineEntity(mPolyline);
            polyline.mColor = mColor;
            polyline.mThickness = mThickness;
            polyline.mType = mType;
            return polyline;
        }

        /// <summary>
        /// 文字データによる座標設定
        /// </summary>
        /// <param name="data"></param>
        public override void setData(string[] data)
        {
            mPolyline = new List<PointD>();
            if (2 < data.Length) {
                for(int i= 0; i < data.Length - 1; i+=2) {
                    PointD ps = new PointD();
                    ps.x = double.Parse(data[i]);
                    ps.y = double.Parse(data[i+1]);
                    mPolyline.Add(ps);
                }
                mArea = new Box(mPolyline);
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
            foreach (PointD p in mPolyline) {
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
            for (int i = 0; i < mPolyline.Count - 1; i++)
                l += mPolyline[i].length(mPolyline[i + 1]);
            string buf = "";
            buf += $"要素番号: {mNo}";
            buf += $"\n要素種別: ポリライン要素";
            buf += $"\n始点 {mPolyline[0].ToString("f4")} 終点 {mPolyline[mPolyline.Count-1].ToString("f4")}";
            buf += $"\n点数 {mPolyline.Count.ToString("f0")}";
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
            return $"{mNo}:ポリライン{mPolyline[0].ToString("f1")} {mPolyline[mPolyline.Count - 1].ToString("f1")} {getColorName(mColor)}";
        }

        /// <summary>
        /// 要素を移動する
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        public override void translate(PointD vec)
        {
            mPolyline.ForEach(p => p.offset(vec));
            mArea.offset(vec);
        }

        /// <summary>
        /// 回転
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        public override void rotate(PointD cp, PointD mp)
        {
            mPolyline.ForEach(p => p.rotate(cp, mp));
            mArea = new Box(mPolyline);
        }

        /// <summary>
        /// 表示範囲の更新
        /// </summary>
        public override void updateArea()
        {
            mArea = new Box(mPolyline);
        }
    }

    /// <summary>
    /// ポリゴン要素クラス
    /// </summary>
    public class PolygonEntity : Entity
    {
        //  座標データ
        public List<PointD> mPolygon;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PolygonEntity()
        {
            mEntityId = EntityId.Polygon;
            mPolygon = new List<PointD>();
        }

        /// <summary>
        /// コンストラクタ(LineD設定)
        /// </summary>
        /// <param name="line"></param>
        public PolygonEntity(List<PointD> polylines)
        {
            mEntityId = EntityId.Polygon;
            mPolygon = new List<PointD>();
            polylines.ForEach(p => mPolygon.Add(p));
            mArea = new Box(mPolygon);
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns></returns>
        public PolygonEntity toCopy()
        {
            PolygonEntity polyline = new PolygonEntity(mPolygon);
            polyline.mColor = mColor;
            polyline.mThickness = mThickness;
            polyline.mType = mType;
            return polyline;
        }

        /// <summary>
        /// 文字データによる座標設定
        /// </summary>
        /// <param name="data"></param>
        public override void setData(string[] data)
        {
            mPolygon = new List<PointD>();
            if (2 < data.Length) {
                for (int i = 0; i < data.Length - 1; i += 2) {
                    PointD ps = new PointD();
                    ps.x = double.Parse(data[i]);
                    ps.y = double.Parse(data[i + 1]);
                    mPolygon.Add(ps);
                }
                mArea = new Box(mPolygon);
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
            foreach (PointD p in mPolygon) {
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
            for (int i = 0; i < mPolygon.Count - 1; i++)
                l += mPolygon[i].length(mPolygon[i + 1]);
            string buf = "";
            buf += $"要素番号: {mNo}";
            buf += $"\n要素種別: ポリゴン要素";
            buf += $"\n始点 {mPolygon[0].ToString("f4")} 終点 {mPolygon[mPolygon.Count - 1].ToString("f4")}";
            buf += $"\n点数 {mPolygon.Count.ToString("f0")}";
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
            return $"{mNo}:ポリゴン{mPolygon[0].ToString("f1")} {mPolygon[mPolygon.Count - 1].ToString("f1")} {getColorName(mColor)}";
        }

        /// <summary>
        /// 要素を移動する
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        public override void translate(PointD vec)
        {
            mPolygon.ForEach(p => p.offset(vec));
            mArea.offset(vec);
        }

        /// <summary>
        /// 回転
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        public override void rotate(PointD cp, PointD mp)
        {
            mPolygon.ForEach(p => p.rotate(cp, mp));
            mArea = new Box(mPolygon);
        }

        /// <summary>
        /// 表示範囲の更新
        /// </summary>
        public override void updateArea()
        {
            mArea = new Box(mPolygon);
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
        public ArcEntity toCopy()
        {
            ArcEntity arc = new ArcEntity(mArc);
            arc.mColor = mColor;
            arc.mThickness = mThickness;
            arc.mType = mType;
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
        public TextEntity toCopy()
        {
            TextEntity text = new TextEntity(mText.toCopy());
            text.mColor = mColor;
            text.mThickness = mThickness;
            text.mType = mType;
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
        /// 表示範囲の更新
        /// </summary>
        public override void updateArea()
        {
            mArea = mText.getBox();
        }
    }

    public class EntityData
    {
        public List<Entity> mEntityList;        //  要素リスト
        public Brush mColor = Brushes.Black;    //  デフォルトカラー
        public double mThickness = 1.0;         //  デフォルト線太さ
        public int mType = 0;                   //  点種/線種
        public Box mArea;                       //  要素領域

        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EntityData() { 
            mEntityList = new List<Entity>();
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
            pointEnt.setProperty(mColor, mThickness, mType);
            mEntityList.Add(pointEnt);
            if (mArea == null) {
                mArea = pointEnt.mArea;
            } else {
                mArea.extension(pointEnt.mArea);
            }
            pointEnt.mNo = mEntityList.Count - 1;
            return pointEnt.mNo;
        }

        /// <summary>
        /// 線分要素追加
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <returns></returns>
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
            Entity lineEnt = new LineEntity(pline);
            lineEnt.setProperty(mColor, mThickness, mType);
            mEntityList.Add(lineEnt);
            if (mArea == null) {
                mArea = lineEnt.mArea.toCopy();
            } else {
                mArea.extension(lineEnt.mArea);
            }
            lineEnt.mNo = mEntityList.Count - 1;
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
            Entity polylineEnt = new PolylineEntity(polyline);
            polylineEnt.setProperty(mColor, mThickness, mType);
            mEntityList.Add(polylineEnt);
            if (mArea == null) {
                mArea = polylineEnt.mArea.toCopy();
            } else {
                mArea.extension(polylineEnt.mArea);
            }
            polylineEnt.mNo = mEntityList.Count - 1;
            return polylineEnt.mNo;
        }

        /// <summary>
        /// ポリゴンの追加
        /// </summary>
        /// <param name="polygon">座標点リスト</param>
        /// <returns></returns>
        public int addPolygon(List<PointD> polygon)
        {
            Entity polygonEnt = new PolygonEntity(polygon);
            polygonEnt.setProperty(mColor, mThickness, mType);
            mEntityList.Add(polygonEnt);
            if (mArea == null) {
                mArea = polygonEnt.mArea.toCopy();
            } else {
                mArea.extension(polygonEnt.mArea);
            }
            polygonEnt.mNo = mEntityList.Count - 1;
            return polygonEnt.mNo;
        }

        /// <summary>
        /// 円弧要素の追加
        /// </summary>
        /// <param name="arc">円弧データ</param>
        /// <returns></returns>
        public int addArc(ArcD arc)
        {
            Entity arcEnt = new ArcEntity(arc);
            arcEnt.setProperty(mColor, mThickness, mType);
            mEntityList.Add(arcEnt);
            if (mArea == null) {
                mArea = arcEnt.mArea;
            } else {
                mArea.extension(arcEnt.mArea);
            }
            arcEnt.mNo = mEntityList.Count - 1;
            return arcEnt.mNo;
        }

        /// <summary>
        /// テキスト要素の追加
        /// </summary>
        /// <param name="text">テキスト</param>
        /// <returns></returns>
        public int addText(TextD text)
        {
            Entity textEnt = new TextEntity(text);
            textEnt.setProperty(mColor, mThickness, mType);
            mEntityList.Add(textEnt);
            if (mArea == null) {
                mArea = textEnt.mArea;
            } else {
                mArea.extension(textEnt.mArea);
            }
            textEnt.mNo = mEntityList.Count - 1;
            return textEnt.mNo;
        }

        /// <summary>
        /// 指定要素を削除する
        /// </summary>
        /// <param name="entNoList">要素番号リスト</param>
        /// <returns></returns>
        public bool removeEnt(List<int> entNoList)
        {
            entNoList.Sort((x, y) => y - x);
            foreach (int entNo in entNoList) {
                mEntityList.RemoveAt(entNo);
            }
            updateData();
            return true;
        }

        /// <summary>
        /// 指定要素を移動する
        /// </summary>
        /// <param name="entNoList">要素リスト</param>
        /// <param name="offset">移動量</param>
        /// <returns></returns>
        public bool translate(List<int> entNoList, PointD offset)
        {
            foreach (int entNo in entNoList) {
                mEntityList[entNo].translate(offset);
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
        /// <returns></returns>
        public bool rotate(List<int> entNoList, PointD cp, PointD mp)
        {
            foreach (int entNo in entNoList) {
                mEntityList[entNo].rotate(cp, mp);
            }
            updateData();
            return true;
        }

        /// <summary>
        /// 要素領域、要素番号の更新
        /// </summary>
        public void updateData()
        {
            mArea = mEntityList[0].mArea.toCopy();
            for (int i = 0; i < mEntityList.Count; i++) {
                mEntityList[i].updateArea();
                mArea.extension(mEntityList[i].mArea);
                mEntityList[i].mNo = i;
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
                if (b.insideChk(mEntityList[i].mArea))      //  Boxの内側
                    picks.Add(i);
                if (b.outsideChk(mEntityList[i].mArea))     //  Boxの外側
                    continue;
                if (interSection(mEntityList[i], b))        //  Boxと交点あり
                    picks.Add(i);
            }
            return picks;
        }

        /// <summary>
        /// 要素がBOX内かまたは交差しているか(BOX外でない)の判定
        /// </summary>
        /// <param name="ent">要素</param>
        /// <param name="b">検索Box</param>
        /// <returns>判定</returns>
        public bool interSection(Entity ent, Box b)
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
                    plist = b.intersection(polyline.mPolyline, false, true);
                    result = 0 < plist.Count;
                    break;
                case EntityId.Polygon:
                    PolygonEntity polygon = (PolygonEntity)ent;
                    plist = b.intersection(polygon.mPolygon, true, true);
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
            foreach (Entity entity in mEntityList) {
                if (entity != null) {
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
                    }
                    if (0 < mEntityList.Count)
                        mEntityList[mEntityList.Count - 1].mNo = mEntityList.Count - 1;
                }
            }
            updateData();
        }
    }
}
