using CoreLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace CadApp
{
    public enum EntityId
    {
        Non, Link, Point, Line, Arc, Circle, Ellipse, Oval, Polyline, Polygon,
        Text, Parts, Image, Property, Comment
    }
    public enum PointType { Dot, Cross, Plus, Squre, Circle }
    public enum LineType { Solid, Center, Dash }

    /// <summary>
    /// 要素の抽象化クラス
    /// </summary>
    public abstract class Entity
    {
        //  属性
        public int mNo = -1;                        //  要素番号(非保存)
        public EntityId mEntityId = EntityId.Non;   //  要素種別
        public Brush mColor = Brushes.Black;        //  色
        public double mThickness = 1.0;             //  線の太さ/点の大きさ
        public int mType = 0;                       //  線種/店主
        public bool mRemove = false;                //  削除フラグ(非保存)
        public int mRemoveLink = -1;                //  削除要素番号(非保存)
        public int mOperationCount = -1;            //  操作番号(非保存)
        public string mLayerName = "BaseLayer";     //  レイヤー名
        public ulong mLayerBit = 0x1;               //  レイヤービット(64bit非保存)
        public string mEntityName = "";             //  要素名
        public bool mBackDisp = false;              //  背面表示

        //  表示領域
        public Box mArea = new Box();
        //  ピック
        public bool mPick = false;
        public Brush mPickColor = Brushes.Red;

        private YDraw ydraw = new YDraw();
        protected YLib ylib = new YLib();

        /// <summary>
        /// 要素属性の設定
        /// </summary>
        /// <param name="ent">Entity</param>
        public void setProperty(Entity ent)
        {
            mColor      = ent.mColor;
            mThickness  = ent.mThickness;
            mType       = ent.mType;
            mLayerName  = ent.mLayerName;
            mEntityName = ent.mEntityName;
            mBackDisp   = ent.mBackDisp;
        }

        /// <summary>
        /// 要素属性の設定
        /// </summary>
        /// <param name="para">図面パラメータ</param>
        public void setProperty(DrawingPara para)
        {
            mColor     = para.mColor;
            mThickness = para.mThickness;
            mType      = para.mLineType;
            mLayerName = para.mCreateLayerName;
        }

        /// <summary>
        /// 要素属性の設定
        /// </summary>
        /// <param name="property">文字列データ</param>
        public void setProperty(string[] property)
        {
            try {
                mColor     = ydraw.getColor(property[1].Trim());
                mThickness = double.Parse(property[2].Trim());
                mType      = int.Parse(property[3].Trim());
                mLayerName = property[4].Trim();
                if (5 < property.Length) {
                    mBackDisp = bool.Parse(property[5].Trim());
                } else {
                    if (mEntityId == EntityId.Image)
                        mBackDisp = true;
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// 要素属性を文字列に変換
        /// </summary>
        /// <returns></returns>
        public string toString()
        {
            return $"{mEntityId},{ydraw.getColorName(mColor)},{mThickness}," +
                    $"{mType},{mLayerName},{mBackDisp}";
        }

        /// <summary>
        /// 要素属性を文字列のリストに変換
        /// </summary>
        /// <returns></returns>
        public List<string> toList()
        {
            List<string> propertyLis = new List<string>() {
                mEntityId.ToString(), ydraw.getColorName(mColor), mThickness.ToString(),
                mType.ToString(), mLayerName, mBackDisp.ToString(),
            };
            return propertyLis;
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
        /// 座標データを文字列に変換
        /// </summary>
        /// <returns></returns>
        abstract public string toDataString();

        /// <summary>
        /// データを文字列リストに変換
        /// </summary>
        /// <returns></returns>
        abstract public List<string> toDataList();

        /// <summary>
        /// 座標リストに変換
        /// </summary>
        /// <returns>座標リスト</returns>
        abstract public List<PointD> toPointList();

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
        /// 原点を指定して拡大縮小
        /// </summary>
        /// <param name="cp">原点</param>
        /// <param name="scale">拡大率</param>
        abstract public void scale(PointD cp, double scale);

        /// <summary>
        /// 要素のオフセット
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        abstract public void offset(PointD sp, PointD ep);

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
        abstract public void stretch(PointD vec, PointD pickPos, bool arc);

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
        /// Boxとの交点の有無をチェック
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        abstract public bool intersectionChk(Box b);

        /// <summary>
        /// ピック位置に最も近い要素の端点座標を求める
        /// </summary>
        /// <param name="pickPos">ピック座標</param>
        /// <returns>端点座標</returns>
        abstract public PointD getEndPoint(PointD pickPos);

        /// <summary>
        /// 参照点からピック位置と同じ方向でピック位置に近い端点
        /// </summary>
        /// <param name="pickPos">ピック座標</param>
        /// <param name="cp">参照点</param>
        /// <returns>端点座標</returns>
        abstract public PointD getEndPoint(PointD pickPos, PointD cp);

        /// <summary>
        /// ピック位置に最も近い線分を求める
        /// </summary>
        /// <param name="pickPos">ピック座標</param>
        /// <returns>線分</returns>
        abstract public LineD getLine(PointD pickPos);

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
}
