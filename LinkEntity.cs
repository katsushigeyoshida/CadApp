using CoreLib;
using System.Collections.Generic;

namespace CadApp
{
    /// <summary>
    /// 参照用のEntityクラス
    /// Undo 処理用に作成
    /// </summary>
    public class LinkEntity : Entity
    {
        public int mLinkNo = -1;

        public LinkEntity()
        {
            mEntityId = EntityId.Link;
            mEntityName = "リンク";
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
        /// データを文字列リストに変換
        /// </summary>
        /// <returns></returns>
        public override List<string> toDataList()
        {
            List<string> dataList = new List<string>();
            return dataList;
        }

        /// <summary>
        /// 座標リストに変換
        /// </summary>
        /// <returns>座標リスト</returns>
        public override List<PointD> toPointList()
        {
            return new List<PointD>();
        }

        /// <summary>
        /// 要素のコピーを作成
        /// </summary>
        /// <returns></returns>
        public override Entity toCopy()
        {
            LinkEntity entity = new LinkEntity();
            entity.setProperty(this);
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
        /// 原点を指定して拡大縮小
        /// </summary>
        /// <param name="cp">原点</param>
        /// <param name="scale">拡大率</param>
        public override void scale(PointD cp, double scale)
        {
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
        public override void stretch(PointD vec, PointD pickPos, bool arc = false)
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
        /// Box内か交点有りの有無をチェック
        /// </summary>
        /// <param name="b">Box</param>
        /// <returns></returns>
        public override bool intersectionChk(Box b)
        {
            return false;
        }

        /// <summary>
        /// ピック位置に最も近い端点を求める
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <returns>端点座標(不定値はisNaN()でチェック)</returns>
        public override PointD getEndPoint(PointD pickPos)
        {
            return new PointD();
        }

        /// <summary>
        /// 参照点からピック位置と同じ方向でピック位置に近い端点
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="cp">参照位置</param>
        /// <returns>端点座標</returns>
        public override PointD getEndPoint(PointD pickPos, PointD cp)
        {
            return new PointD();
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
            return null;
        }

        /// <summary>
        /// 表示範囲の更新
        /// </summary>
        public override void updateArea()
        {

        }
    }
}
