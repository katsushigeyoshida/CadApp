using CoreLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CadApp
{
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
            arc.setProperty(this);
            arc.mArea = mArea.toCopy();
            return arc;
        }

        /// <summary>
        /// 文字データによる座標設定
        /// </summary>
        /// <param name="data"></param>
        public override void setData(string[] data)
        {
            try {
                mArc.mCp.x = double.Parse(data[0]);
                mArc.mCp.y = double.Parse(data[1]);
                mArc.mR = double.Parse(data[2]);
                mArc.mSa = double.Parse(data[3]);
                mArc.mEa = double.Parse(data[4]);
            } catch(Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            mArea = new Box(mArc);
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
        /// データを文字列リストに変換
        /// </summary>
        /// <returns></returns>
        public override List<string> toDataList()
        {
            List<string> dataList = new List<string>() {
                mArc.mCp.x.ToString(), mArc.mCp.y.ToString(), mArc.mR.ToString(),
                mArc.mSa.ToString(), mArc.mEa.ToString()
            };
            return dataList;
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
        /// 原点を指定して拡大縮小
        /// </summary>
        /// <param name="cp">原点</param>
        /// <param name="scale">拡大率</param>
        public override void scale(PointD cp, double scale)
        {
            mArc.scale(cp, scale);
            mArea = new Box(mArc);
        }

        /// <summary>
        /// 要素のオフセット
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void offset(PointD sp, PointD ep)
        {
            mArc.offset(sp, ep);
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
            switch (entity.mEntityId) {
                case EntityId.Point:
                    PointEntity point = (PointEntity)entity;
                    PointD ip = mArc.intersection(point.mPoint);
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
        /// Box内か交点有りの有無をチェック
        /// </summary>
        /// <param name="b">Box</param>
        /// <returns></returns>
        public override bool intersectionChk(Box b)
        {
            List<PointD> plist = b.intersection(mArc);
            return 0 < plist.Count;
        }

        /// <summary>
        /// ピック位置に最も近い端点を求める
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <returns>端点座標(不定値はisNaN()でチェック)</returns>
        public override PointD getEndPoint(PointD pickPos)
        {
            if (mArc.startPoint().length(pickPos) < mArc.endPoint().length(pickPos))
                return mArc.startPoint();
            else
                return mArc.endPoint();
        }

        /// <summary>
        /// 参照点からピック位置と同じ方向でピック位置に近い端点
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="cp">参照位置</param>
        /// <returns>端点座標</returns>
        public override PointD getEndPoint(PointD pickPos, PointD cp)
        {
            if (mArc.startPoint().length(pickPos) < mArc.endPoint().length(pickPos))
                return mArc.startPoint();
            else
                return mArc.endPoint();
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

}
