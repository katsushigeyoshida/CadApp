using CoreLib;
using System.Collections.Generic;

namespace CadApp
{
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
            mEntityName = "点";
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="p">座標データ</param>
        public PointEntity(PointD p)
        {
            mEntityId = EntityId.Point;
            mPoint = p.toCopy();
            mEntityName = "点";
            mArea = new Box(mPoint);
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns></returns>
        public override Entity toCopy()
        {
            PointEntity point = new PointEntity(mPoint.toCopy());
            point.setProperty(this);
            point.mArea = mArea.toCopy();
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
        /// データを文字列リストに変換
        /// </summary>
        /// <returns></returns>
        public override List<string> toDataList()
        {
            List<string> dataList = new List<string> {
                mPoint.x.ToString(), mPoint.y.ToString()
            };
            return dataList;
        }

        /// <summary>
        /// 要素情報を文字列に変換
        /// </summary>
        /// <returns></returns>
        public override string entityInfo()
        {
            string buf = "";
            buf += $"要素番号: {mNo}";
            buf += $"\n要素種別: {mEntityName}要素";
            buf += $"\n座標: {mPoint.ToString("f4")}";
            buf += $"\nカラー　: {getColorName(mColor)}";
            buf += $"\n大きさ　: {mThickness}";
            buf += $"\nレイヤー: {mLayerName}";

            return buf;
        }

        /// <summary>
        /// 要素情報の要約の取得
        /// </summary>
        /// <returns></returns>
        public override string getSummary()
        {
            return $"{mNo}:{mEntityName} {mPoint.ToString("f1")} {getColorName(mColor)}";
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
        /// 原点を指定して拡大縮小
        /// </summary>
        /// <param name="cp">原点</param>
        /// <param name="scale">拡大率</param>
        public override void scale(PointD cp, double scale)
        {
            mPoint.scale(cp, scale);
            mArea = new Box(mPoint);
        }

        /// <summary>
        /// 要素のオフセット
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void offset(PointD sp, PointD ep)
        {
            mPoint.translate(sp, ep);
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
                    ip = polyline.mPolyline.nearCrossPoint(mPoint);
                    plist.Add(ip);
                    break;
                case EntityId.Polygon:
                    PolygonEntity polygon = (PolygonEntity)entity;
                    ip = polygon.mPolygon.nearCrossPoint(mPoint);
                    plist.Add(ip);
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
            return b.insideChk(mPoint);
        }

        /// <summary>
        /// ピック位置に最も近い端点を求める
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <returns>端点座標(不定値はisNaN()でチェック)</returns>
        public override PointD getEndPoint(PointD pickPos)
        {
            return mPoint;
        }

        /// <summary>
        /// 参照点からピック位置と同じ方向でピック位置に近い端点
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="cp">参照位置</param>
        /// <returns>端点座標</returns>
        public override PointD getEndPoint(PointD pickPos, PointD cp)
        {
            return mPoint;
        }

        /// <summary>
        /// 線分を取り出す(線分が複数の場合はピック位置に最も近い線分)
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <returns>線分(不定値は isNaN()でチェック)</returns>
        public override LineD getLine(PointD pickPos)
        {
            return new LineD();
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

}
