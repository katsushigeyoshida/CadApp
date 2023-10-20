using CoreLib;
using System.Collections.Generic;
using System.Linq;

namespace CadApp
{
    /// <summary>
    /// ポリゴン要素クラス
    /// 
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
            mEntityName = "ポリゴン";
        }

        /// <summary>
        /// コンストラクタ(LineD設定)
        /// </summary>
        /// <param name="line"></param>
        public PolygonEntity(List<PointD> polygon)
        {
            mEntityId = EntityId.Polygon;
            mPolygon = new PolygonD(polygon);
            mPolygon.squeeze();
            mEntityName = "ポリゴン";
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
            polygon.setProperty(this);
            polygon.mArea = mArea.toCopy();
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
                mPolygon.squeeze();
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
        /// データを文字列リストに変換
        /// </summary>
        /// <returns></returns>
        public override List<string> toDataList()
        {
            List<string> dataList = new List<string>();
            foreach (PointD p in mPolygon.mPolygon) {
                dataList.Add(p.x.ToString());
                dataList.Add(p.y.ToString());
            }
            return dataList;
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
            buf += $"\n要素種別: {mEntityName}要素";
            buf += $"\n始点 {mPolygon.mPolygon[0].ToString("f4")} 終点 {mPolygon.mPolygon[mPolygon.mPolygon.Count - 1].ToString("f4")}";
            buf += $"\n点数 {mPolygon.mPolygon.Count.ToString("f0")}";
            buf += $"\n長さ {l.ToString("f4")}";
            buf += $"\nカラー　: {getColorName(mColor)}";
            buf += $"\n太さ　　: {mThickness}";
            buf += $"\nレイヤー: {mLayerName}";

            return buf;
        }

        /// <summary>
        /// 要素情報の要約の取得
        /// </summary>
        /// <returns></returns>
        public override string getSummary()
        {
            return $"{mNo}:{mEntityName} {mPolygon.mPolygon[0].ToString("f1")} {mPolygon.mPolygon[mPolygon.mPolygon.Count - 1].ToString("f1")} {getColorName(mColor)}";
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
        /// 原点を指定して拡大縮小
        /// </summary>
        /// <param name="cp">原点</param>
        /// <param name="scale">拡大率</param>
        public override void scale(PointD cp, double scale)
        {
            mPolygon.scale(cp, scale);
            mArea = new Box(mPolygon.mPolygon);
        }

        /// <summary>
        /// 要素のオフセット
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void offset(PointD sp, PointD ep)
        {
            mPolygon.offset(sp, ep);
            mArea = mPolygon.getBox();
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
            List<Entity> entityList = new();
            PolylineEntity polylineEnt = new PolylineEntity();
            polylineEnt.mPolyline = mPolygon.divide(dp);
            polylineEnt.setProperty(this);
            entityList.Add(polylineEnt);
            return entityList;
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
            return mPolygon.nearCrossPoint(pos);
        }

        /// <summary>
        /// 要素同士の交点を求める
        /// </summary>
        /// <param name="entity">要素データ</param>
        /// <returns>交点リスト</returns>
        public override List<PointD> intersection(Entity entity)
        {
            List<PointD> plist = new List<PointD>();
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
        /// Box内か交点有りの有無をチェック
        /// </summary>
        /// <param name="b">Box</param>
        /// <returns></returns>
        public override bool intersectionChk(Box b)
        {
            List<PointD> plist = b.intersection(mPolygon.mPolygon, true, true);
            return 0 < plist.Count;
        }

        /// <summary>
        /// ピック位置に最も近い端点を求める
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <returns>端点座標(不定値はisNaN()でチェック)</returns>
        public override PointD getEndPoint(PointD pickPos)
        {
            return mPolygon.nearPeackPoint(pickPos);
        }

        /// <summary>
        /// 参照点からピック位置と同じ方向でピック位置に近い端点
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="cp">参照位置</param>
        /// <returns>端点座標</returns>
        public override PointD getEndPoint(PointD pickPos, PointD cp)
        {
            LineD line = mPolygon.getLine(pickPos);
            return line.getEndPointLine(pickPos, cp);
        }

        /// <summary>
        /// 線分を取り出す(線分が複数の場合はピック位置に最も近い線分)
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <returns>線分(不定値は isNaN()でチェック)</returns>
        public override LineD getLine(PointD pickPos)
        {
            return mPolygon.nearLine(pickPos).toCopy();
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

}
