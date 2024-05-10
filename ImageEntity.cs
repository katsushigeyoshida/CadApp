using CoreLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace CadApp
{
    public class ImageEntity : Entity
    {
        public string mImagePath = "";
        public string mCacheName = "";
        public string mCacheFolder = "";
        public System.Drawing.Bitmap mBitmap;
        public Size mOrgSize = new Size();
        public Box mDispPosSize = new Box();
        public ImageData mImageData;
        private YDraw ydraw = new YDraw();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="imageData">イメージ管理クラス</param>
        public ImageEntity(ImageData imageData)
        {
            mEntityId = EntityId.Image;
            mEntityName = "イメージ";
            mImageData = imageData;
            mCacheFolder = mImageData.mImageFolder;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="imageData">イメージ管理クラス</param>
        /// <param name="filePath">イメージファイルパス</param>
        public ImageEntity(ImageData imageData, string filePath)
        {
            mEntityId = EntityId.Image;
            mEntityName = "イメージ";
            mImageData = imageData;
            mCacheFolder = mImageData.mImageFolder;
            mImagePath = filePath;

            string cachePath = mImageData.hashCopyFile(filePath);
            if (0 < cachePath.Length) {
                mCacheFolder = Path.GetDirectoryName(cachePath);
                mCacheName = Path.GetFileName(cachePath);
            }

            if (File.Exists(cachePath))
                mBitmap = ydraw.getBitmapFile(cachePath);
            else if (File.Exists(mImagePath))
                mBitmap = ydraw.getBitmapFile(mImagePath);
            else
                mBitmap = null;

            if (mBitmap != null) {
                mOrgSize = new Size(mBitmap.Width, mBitmap.Height);
                mDispPosSize = new Box(mOrgSize);
                mArea = mDispPosSize.toCopy();
            }
        }

        /// <summary>
        /// 座標データの設定
        /// </summary>
        /// <param name="data">文字配列</param>
        public override void setData(string[] data)
        {
            string cachePath = "";
            try {
                mImagePath = ylib.strControlCodeRev(data[0]);
                mOrgSize = new Size(double.Parse(data[1]), double.Parse(data[2]));
                PointD pos = new PointD(double.Parse(data[3]), double.Parse(data[4]));
                Size size = new Size(double.Parse(data[5]), double.Parse(data[6]));
                if (7 < data.Length) {
                    mCacheName = ylib.strControlCodeRev(data[7]);
                    cachePath = Path.Combine(mCacheFolder, mCacheName);
                }
                if (File.Exists(cachePath)) {
                    mBitmap = ydraw.getBitmapFile(cachePath);
                } else if (File.Exists(mImagePath)) {
                    mBitmap = ydraw.getBitmapFile(mImagePath);
                } else {
                    //  ファイルが存在しない時、仮のBitmapを使う
                    //  (仮のBitmapのプロパティには ビルドアクション=コンテンツ、出力ディレクトリ=新しい場合はコピーする を設定)
                    mBitmap = ydraw.getBitmapFile(@"Image/NoData.png");
                }
                mOrgSize = new Size(mBitmap.Width, mBitmap.Height);
                mDispPosSize = new Box(pos, size);
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            mArea = new Box(mDispPosSize);
        }

        /// <summary>
        /// イメージファイルを更新する
        /// </summary>
        /// <param name="filePath">イメージファイルパス</param>
        /// <param name="cashPath">キャッシュファイルパス</param>
        public void fileUpdate(string filePath = "")
        {
            string cachePath;
            if (0 < filePath.Length && File.Exists(filePath)) {
                mImagePath = filePath;
                cachePath = mImageData.hashCopyFile(mImagePath);
                mCacheFolder = Path.GetDirectoryName(cachePath);
                mCacheName = Path.GetFileName(cachePath);
            } else {
                if (File.Exists(mImagePath)) {
                    if (mCacheName.Length == 0 || !File.Exists(getCashPath())) {
                        cachePath = mImageData.hashCopyFile(mImagePath);
                        mCacheFolder = Path.GetDirectoryName(cachePath);
                        mCacheName = Path.GetFileName(cachePath);
                    } 
                }
            }

            cachePath = getCashPath();
            if (File.Exists(cachePath))
                mBitmap = ydraw.getBitmapFile(cachePath);
            else if (File.Exists(mImagePath))
                mBitmap = ydraw.getBitmapFile(mImagePath);
            mOrgSize = new Size(mBitmap.Width, mBitmap.Height);
        }

        /// <summary>
        /// キャッシュファイルのフルパス取得
        /// </summary>
        /// <returns></returns>
        public string getCashPath()
        {
            return Path.Combine(mCacheFolder, mCacheName);
        }

        /// <summary>
        /// 要素の描画
        /// </summary>
        /// <param name="ydraw"></param>
        public override void draw(YWorldDraw ydraw)
        {
            if (mPick) {
                ydraw.mBrush = mPickColor;
                ydraw.drawWRectangle(mDispPosSize);
            }
            ydraw.drawWBitmap(mBitmap, mDispPosSize, false);
        }

        /// <summary>
        /// 座標データを文字列に変換
        /// </summary>
        /// <returns></returns>
        public override string toDataString()
        {
            return $"{ylib.strControlCodeCnv(mImagePath)},{mOrgSize.Width},{mOrgSize.Height}," +
                $"{mDispPosSize.Left},{mDispPosSize.Top},{mDispPosSize.Width},{mDispPosSize.Height}," +
                $"{ylib.strControlCodeCnv(mCacheName)}";
        }

        /// <summary>
        /// データを文字列リストに変換
        /// </summary>
        /// <returns></returns>
        public override List<string> toDataList()
        {
            List<string> dataList = new List<string>() {
                ylib.strControlCodeCnv(mImagePath), mOrgSize.Width.ToString(), mOrgSize.Height.ToString(),
                mDispPosSize.Left.ToString(), mDispPosSize.Top.ToString(),
                mDispPosSize.Width.ToString(), mDispPosSize.Height.ToString(),
                ylib.strControlCodeCnv(mCacheName)
            };
            return dataList;
        }

        /// <summary>
        /// 要素のコピーを作成
        /// </summary>
        /// <returns></returns>
        public override Entity toCopy()
        {
            ImageEntity entity = new ImageEntity(mImageData);
            entity.setProperty(this);
            entity.mImagePath = mImagePath;
            entity.mCacheFolder = mCacheFolder;
            entity.mCacheName = mCacheName;
            entity.mBitmap = mBitmap;
            entity.mOrgSize = mOrgSize;
            entity.mDispPosSize = mDispPosSize.toCopy();
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
            buf += $"\n要素種別: {mEntityName}要素";
            buf += $"\nファイルパス: {mImagePath}";
            buf += $"\nキャッシュ名: {mCacheName}";
            buf += $"\n元サイズ　: {mOrgSize.Width.ToString("f4")} x {mOrgSize.Height.ToString("f4")}";
            buf += $"\n位置    　: {mDispPosSize.Left.ToString("f4")} , {mDispPosSize.Top.ToString("f4")}";
            buf += $"\n表示サイズ: {mDispPosSize.Width.ToString("f4")} , {mDispPosSize.Height.ToString("f4")}";
            buf += $"\nカラー    : {getColorName(mColor)}";
            buf += $"\nレイヤー  : {mLayerName}";

            return buf;
        }

        /// <summary>
        /// 要素情報の要約を取得
        /// </summary>
        /// <returns></returns>
        public override string getSummary()
        {
            return $"{mNo}:{mEntityName} {mDispPosSize.Left.ToString("f1")},{mDispPosSize.Top.ToString("f1")} " + 
                    $"{Path.GetFileNameWithoutExtension(mImagePath)}";
        }

        /// <summary>
        /// 要素を移動する
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        public override void translate(PointD vec)
        {
            mDispPosSize.offset(vec);
            mArea = new Box(mDispPosSize);
        }

        /// <summary>
        /// 要素の回転
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        public override void rotate(PointD cp, PointD mp)
        {
            mDispPosSize.rotate(cp, mp); 
            mArea = new Box(mDispPosSize);
        }

        /// <summary>
        /// 要素のミラー
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void mirror(PointD sp, PointD ep)
        {
            mDispPosSize.mirror(sp, ep);
            mArea = new Box(mDispPosSize);
        }

        /// <summary>
        /// 原点を指定して拡大縮小
        /// </summary>
        /// <param name="cp">原点</param>
        /// <param name="scale">拡大率</param>
        public override void scale(PointD cp, double scale)
        {
            mDispPosSize.scale(cp, scale);
            mArea = new Box(mDispPosSize);
        }

        /// <summary>
        /// 要素のオフセット
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void offset(PointD sp, PointD ep)
        {
            mDispPosSize.translate(sp, ep);
            mArea = new Box(mDispPosSize);
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
            List<PointD> plist = mDispPosSize.ToPointList();
            double dis = double.MaxValue;
            int n = 0;
            for (int i = 0; i < plist.Count; i++) {
                if (dis > plist[i].length(pickPos)) {
                    dis = plist[i].length(pickPos);
                    n = i;
                }
            }
            plist[n].translate(vec);
            int m = (n + 2) % 4;
            double width = Math.Abs(plist[m].x - plist[n].x);
            double height = width * mOrgSize.Height / mOrgSize.Width;
            if (plist[m].y < plist[n].y)
                plist[m].y = plist[n].y - height;
            else
                plist[m].y = plist[n].y + height;
            mDispPosSize = new Box(plist[n], plist[m]);
        }

        /// <summary>
        /// 参照点に対する垂点を求める
        /// </summary>
        /// <param name="pos">参照点</param>
        /// <returns>垂点</returns>
        public override PointD onPoint(PointD pos)
        {
            return mArea.onPoint(pos);
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
                    PointD ip = mArea.onPoint(point.mPoint);
                    plist.Add(ip);
                    break;
                case EntityId.Line:
                    LineEntity line = (LineEntity)entity;
                    plist = mArea.intersection(line.mLine);
                    break;
                case EntityId.Arc:
                    ArcEntity arc = (ArcEntity)entity;
                    plist = mArea.intersection(arc.mArc);
                    break;
                case EntityId.Polyline:
                    PolylineEntity polyline = (PolylineEntity)entity;
                    plist = mArea.intersection(polyline.mPolyline.mPolyline);
                    break;
                case EntityId.Polygon:
                    PolygonEntity polygon = (PolygonEntity)entity;
                    plist = mArea.intersection(polygon.mPolygon.mPolygon);
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
            if (mArea.insideChk(b)) {
                return true; 
            } else {
                List<PointD> plist = b.intersection(mArea);
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
            return mArea.nearPoint(pickPos);
        }

        /// <summary>
        /// 参照点からピック位置と同じ方向でピック位置に近い端点
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="cp">参照位置</param>
        /// <returns>端点座標</returns>
        public override PointD getEndPoint(PointD pickPos, PointD cp)
        {
            LineD line = mArea.nearLine(pickPos);
            return line.getEndPointLine(pickPos, cp);
        }

        /// <summary>
        /// 線分を取り出す(線分が複数の場合はピック位置に最も近い線分)
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <returns>線分(不定値は isNaN()でチェック)</returns>
        public override LineD getLine(PointD pickPos)
        {
            return mArea.nearLine(pickPos);
        }

        /// <summary>
        /// 要素上の分割位置を求める
        /// </summary>
        /// <param name="divideNo">分割数</param>
        /// <param name="pos">参照点</param>
        /// <returns>分割点</returns>
        public override PointD dividePos(int divideNo, PointD pos)
        {
            return mDispPosSize.nearPoint(pos, divideNo);
        }

        /// <summary>
        /// 2点から表示位置を設定する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public void setPostion(PointD sp, PointD ep)
        {
            mDispPosSize = new Box(sp, ep);
            mDispPosSize.normalize();
            mDispPosSize.Height = mDispPosSize.Width * mOrgSize.Height / mOrgSize.Width;
            mArea = new Box(mDispPosSize);
        }

        /// <summary>
        /// 表示範囲の更新
        /// </summary>
        public override void updateArea()
        {
            mArea = new Box(mDispPosSize);
        }
    }
}
