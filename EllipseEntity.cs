using CoreLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CadApp
{
    public class EllipseEntity : Entity
    {
        public EllipseD mEllipse = new EllipseD();

        public EllipseEntity()
        {
            mEntityId = EntityId.Ellipse;
        }

        public EllipseEntity(EllipseD ellipse)
        {
            mEntityId = EntityId.Ellipse;
            mEllipse = ellipse;
            mArea = ellipse.getArea();
        }

        /// <summary>
        /// 座標データの設定
        /// </summary>
        /// <param name="data">文字配列</param>
        public override void setData(string[] data)
        {
            try {
                mEllipse.mCp.x = double.Parse(data[0]);
                mEllipse.mCp.y = double.Parse(data[1]);
                mEllipse.mRx = double.Parse(data[2]);
                mEllipse.mRy = double.Parse(data[3]);
                mEllipse.mSa = double.Parse(data[4]);
                mEllipse.mEa = double.Parse(data[5]);
                mEllipse.mRotate = double.Parse(data[6]);
            } catch(Exception e) {

            }
            mArea = mEllipse.getArea();

        }

        /// <summary>
        /// 要素の描画
        /// </summary>
        /// <param name="ydraw"></param>
        public override void draw(YWorldDraw ydraw)
        {
            ydraw.mBrush = mPick ? mPickColor : mColor;
            ydraw.mThickness = mThickness;
            ydraw.mLineType = mType;
            ydraw.drawWEllipse(mEllipse);
        }

        /// <summary>
        /// 座標データを文字列に変換
        /// </summary>
        /// <returns></returns>
        public override string toDataString()
        {
            return $"{mEllipse.mCp.x},{mEllipse.mCp.y},{mEllipse.mRx},{mEllipse.mRy},{mEllipse.mSa},{mEllipse.mEa},{mEllipse.mRotate}";
        }

        /// <summary>
        /// データを文字列リストに変換
        /// </summary>
        /// <returns></returns>
        public override List<string> toDataList()
        {
            List<string> dataList = new List<string>() {
                mEllipse.mCp.x.ToString(), mEllipse.mCp.y.ToString(), mEllipse.mRx.ToString(), mEllipse.mRy.ToString(),
                mEllipse.mSa.ToString(), mEllipse.mEa.ToString(), mEllipse.mRotate.ToString(),
            };
            return dataList;
        }

        /// <summary>
        /// 要素のコピーを作成
        /// </summary>
        /// <returns></returns>
        public override Entity toCopy()
        {
            EllipseEntity entity = new EllipseEntity();
            entity.mColor = mColor;
            entity.mThickness = mThickness;
            entity.mType = mType;
            entity.mRemove = mRemove;
            entity.mEllipse = mEllipse.toCopy();
            entity.mArea = mArea.toCopy();
            return entity;
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
            buf += $"\n要素種別: 楕円要素";
            buf += $"\n中心 {mEllipse.mCp.ToString("f4")} 半径 {mEllipse.mRx.ToString("f4")},{mEllipse.mRy.ToString("f4")}";
            buf += $"\n開始角 {R2D(mEllipse.mSa).ToString("f2")} 終了角 {R2D(mEllipse.mEa).ToString("f2")} 回転角 {R2D(mEllipse.mRotate).ToString("f2")}";
            buf += $"\nカラー: {getColorName(mColor)}";
            buf += $"\n太さ: {mThickness}";
            buf += $"\nレイヤー: {mLayerName}";
            return buf;
        }

        /// <summary>
        /// 要素情報の要約を取得
        /// </summary>
        /// <returns></returns>
        public override string getSummary()
        {
            return $"{mNo}:楕円{mEllipse.ToString("f1")} {getColorName(mColor)}";
        }

        /// <summary>
        /// 要素を移動する
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        public override void translate(PointD vec)
        {
            mEllipse.mCp.offset(vec);
            mArea.offset(vec);
        }

        /// <summary>
        /// 要素の回転
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        public override void rotate(PointD cp, PointD mp)
        {
            mEllipse.rotate(cp, mp);
            mArea = mEllipse.getArea();
        }

        /// <summary>
        /// 要素のミラー
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void mirror(PointD sp, PointD ep)
        {
            mEllipse.mirror(sp, ep);
            mArea = mEllipse.getArea();
        }

        /// <summary>
        /// 要素のオフセット
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void offset(PointD sp, PointD ep)
        {
            mEllipse.offset(sp, ep);
            mArea = mEllipse.getArea();
        }

        /// <summary>
        /// 要素のトリム
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void trim(PointD sp, PointD ep)
        {
            mEllipse.trim(sp, ep);
            mArea = mEllipse.getArea();
        }

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="dp">分割参照点</param>
        /// <returns>要素リスト</returns>
        public override List<Entity> divide(PointD dp)
        {
            List<Entity> entityList = new();
            PointD mp = mEllipse.intersection(dp);
            if (!mEllipse.onPoint(mp))
                return null;
            double ang = mEllipse.getAngle(mp);
            EllipseEntity ellipseEnt0 = (EllipseEntity)toCopy();
            ellipseEnt0.mEllipse.mEa = ang;
            ellipseEnt0.mEllipse.normalize();
            entityList.Add(ellipseEnt0);
            EllipseEntity ellipseEnt1 = (EllipseEntity)toCopy();
            ellipseEnt1.mEllipse.mSa = ang;
            ellipseEnt1.mEllipse.normalize();
            entityList.Add(ellipseEnt1);
            return entityList;
        }

        /// <summary>
        /// 要素のストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピックした位置</param>
        public override void stretch(PointD vec, PointD pickPos)
        {
            mEllipse.stretch(vec, pickPos);
            mArea = mEllipse.getArea();
        }

        /// <summary>
        /// 参照点に対する垂点を求める
        /// </summary>
        /// <param name="pos">参照点</param>
        /// <returns>垂点</returns>
        public override PointD onPoint(PointD pos)
        {
            return mEllipse.intersection(pos);
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
                    ip = mEllipse.intersection(point.mPoint);
                    plist.Add(ip);
                    break;
                case EntityId.Line:
                    LineEntity line = (LineEntity)entity;
                    plist = mEllipse.intersection(line.mLine);
                    break;
                case EntityId.Arc:
                    ArcEntity arc = (ArcEntity)entity;
                    plist = mEllipse.intersection(arc.mArc);
                    break;
                case EntityId.Polyline:
                    PolylineEntity polyline = (PolylineEntity)entity;
                    plist = mEllipse.intersection(polyline.mPolyline);
                    break;
                case EntityId.Polygon:
                    PolygonEntity polygon = (PolygonEntity)entity;
                    plist = mEllipse.intersection(polygon.mPolygon);
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
            List<PointD> plist = mEllipse.intersection(b);
            return 0 < plist.Count;
        }

        /// <summary>
        /// ピック位置に最も近い端点を求める
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <returns>端点座標(不定値はisNaN()でチェック)</returns>
        public override PointD getEndPoint(PointD pickPos)
        {
            if (mEllipse.startPoint().length(pickPos) < mEllipse.endPoint().length(pickPos))
                return mEllipse.startPoint();
            else
                return mEllipse.endPoint();
        }

        /// <summary>
        /// 参照点からピック位置と同じ方向でピック位置に近い端点
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="cp">参照位置</param>
        /// <returns>端点座標</returns>
        public override PointD getEndPoint(PointD pickPos, PointD cp)
        {
            if (mEllipse.startPoint().length(pickPos) < mEllipse.endPoint().length(pickPos))
                return mEllipse.startPoint();
            else
                return mEllipse.endPoint();
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
            List<PointD> plist = mEllipse.dividePoints(divideNo);
            return plist.MinBy(p => p.length(pos));
        }

        /// <summary>
        /// 表示範囲の更新
        /// </summary>
        public override void updateArea()
        {
            mArea = mEllipse.getArea();
        }
    }
}
