using CoreLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CadApp
{
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
            text.mArea = mArea.toCopy();
            return text;
        }

        /// <summary>
        /// 文字データによる座標設定
        /// </summary>
        /// <param name="data"></param>
        public override void setData(string[] data)
        {
            try {
                mText.mText = ylib.strControlCodeRev(data[0]);
                mText.mPos.x = double.Parse(data[1]);
                mText.mPos.y = double.Parse(data[2]);
                mText.mTextSize = double.Parse(data[3]);
                mText.mRotate = double.Parse(data[4]);
                mText.mHa = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), data[5]);
                mText.mVa = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), data[6]);
                mText.mLinePitchRate = double.Parse(data[7]);
            } catch (Exception e) {

            }
            mArea = new Box(mText.getBox());
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
            return $"{ylib.strControlCodeCnv(mText.mText)},{mText.mPos.x},{mText.mPos.y}," +
                $"{mText.mTextSize},{mText.mRotate},{mText.mHa},{mText.mVa},{mText.mLinePitchRate}";
        }

        /// <summary>
        /// データを文字列リストに変換
        /// </summary>
        /// <returns></returns>
        public override List<string> toDataList()
        {
            List<string> dataList = new List<string>() {
                ylib.strControlCodeCnv(mText.mText), mText.mPos.x.ToString(), mText.mPos.y.ToString(), mText.mTextSize.ToString(),
                mText.mRotate.ToString(), mText.mHa.ToString(), mText.mVa.ToString(), mText.mLinePitchRate.ToString()
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
            buf += $"\n要素種別: テキスト要素";
            buf += $"\n文字列　: {mText.mText}";
            buf += $"\n起点    : {mText.mPos.ToString("f4")}";
            buf += $"\n文字高さ: {mText.mTextSize.ToString("f4")} 水平位置 {mText.mHa} 垂直位置 {mText.mVa}";
            buf += $"\nカラー  : {getColorName(mColor)}";
            buf += $"\nレイヤー: {mLayerName}";

            return buf;
        }

        /// <summary>
        /// 要素情報の要約の取得
        /// </summary>
        /// <returns></returns>
        public override string getSummary()
        {
            return $"{mNo}:テキスト{mText.mPos.ToString("f1")} {mText.mText.Substring(0, Math.Min(10, mText.mText.Length))} {getColorName(mColor)}";
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
        /// 要素のオフセット
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void offset(PointD sp, PointD ep)
        {
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
            mText.mPos.offset(vec);
            mArea.offset(vec);
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
        /// Box内か交点有りの有無をチェック
        /// </summary>
        /// <param name="b">Box</param>
        /// <returns></returns>
        public override bool intersectionChk(Box b)
        {
            if (mText.insideChk(b)) {
                return true;
            } else {
                List<PointD> plist = b.intersection(mText.getArea());
                return 0 < plist.Count;
            }
        }

        /// <summary>
        /// ピック位置に最も近い端点を求める
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <returns>端点座標(不定値はisNaN()でチェック)</returns>
        public override PointD getEndPoint(PointD pickPos)
        {
            return mText.nearPoint(pickPos);
        }

        /// <summary>
        /// 参照点からピック位置と同じ方向でピック位置に近い端点
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="cp">参照位置</param>
        /// <returns>端点座標</returns>
        public override PointD getEndPoint(PointD pickPos, PointD cp)
        {
            return mText.nearPoint(pickPos);
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

}
