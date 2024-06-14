using CoreLib;
using System.Collections.Generic;
using System.Linq;

namespace CadApp
{
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
            mEntityName = "線分";
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
            mEntityName = "線分";
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns></returns>
        public override Entity toCopy()
        {
            LineEntity line = new LineEntity(mLine);
            line.setProperty(this);
            line.mArea = mArea.toCopy();
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
        /// データを文字列リストに変換
        /// </summary>
        /// <returns></returns>
        public override List<string> toDataList()
        {
            List<string> dataList = new List<string> {
                mLine.ps.x.ToString(), mLine.ps.y.ToString(), mLine.pe.x.ToString(), mLine.pe.y.ToString()
            };
            return dataList;
        }

        /// <summary>
        /// 座標リストに変換
        /// </summary>
        /// <returns>座標リスト</returns>
        public override List<PointD> toPointList()
        {
            return mLine.toPointList();
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
            buf += $"\n要素種別: {mEntityId}要素";
            buf += $"\n始点 {mLine.ps.ToString("f4")} 終点 {mLine.pe.ToString("f4")}";
            buf += $"\n長さ {mLine.length().ToString("f4")}";
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
            return $"{mNo}:{mEntityName} {mLine.ToString("f1")} {getColorName(mColor)}";
        }

        /// <summary>
        /// 要素を移動する
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        public override void translate(PointD vec)
        {
            mLine.translate(vec);
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
        /// 要素のオフセット
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void offset(PointD sp, PointD ep)
        {
            mLine.offset(sp, ep);
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
        /// 原点を指定して拡大縮小
        /// </summary>
        /// <param name="cp">原点</param>
        /// <param name="scale">拡大率</param>
        public override void scale(PointD cp, double scale)
        {
            mLine.scale(cp, scale);
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
        /// <param name="arc">Polyline/Polygonの円弧ストレッチ</param>
        public override void stretch(PointD vec, PointD pickPos, bool arc = false)
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
            PointD mp = mLine.intersection(dp);
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
        /// Box内か交点有りの有無をチェック
        /// </summary>
        /// <param name="b">Box</param>
        /// <returns></returns>
        public override bool intersectionChk(Box b)
        {
            List<PointD> plist = b.intersection(mLine);
            return 0 < plist.Count;
        }

        /// <summary>
        /// ピック位置に最も近い端点を求める
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <returns>端点座標(不定値はisNaN()でチェック)</returns>
        public override PointD getEndPoint(PointD pickPos)
        {
            if (mLine.ps.length(pickPos) < mLine.pe.length(pickPos))
                return mLine.ps;
            else
                return mLine.pe;
        }

        /// <summary>
        /// 参照点からピック位置と同じ方向でピック位置に近い端点
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="cp">参照位置</param>
        /// <returns>端点座標</returns>
        public override PointD getEndPoint(PointD pickPos, PointD cp)
        {
            return mLine.getEndPointLine(pickPos, cp);
        }

        /// <summary>
        /// 線分を取り出す(線分が複数の場合はピック位置に最も近い線分)
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <returns>線分(不定値は isNaN()でチェック)</returns>
        public override LineD getLine(PointD pickPos)
        {
            return mLine.toCopy();
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

}
