using CoreLib;
using System;
using System.Collections.Generic;
using System.Windows;

namespace CadApp
{

    /// <summary>
    /// パーツ(部品)クラス
    /// </summary>
    public class PartsEntity : Entity
    {
        //  座標データ
        public PartsD mParts;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PartsEntity()
        {
            mEntityId = EntityId.Parts;
            mParts = new PartsD();
            mEntityName = "パーツ";
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">パーツ名</param>
        /// <param name="lines">線分データ</param>
        /// <param name="arcs">円弧データ</param>
        /// <param name="texts">文字列データ</param>
        public PartsEntity(string name, List<LineD> lines, List<ArcD> arcs, List<TextD> texts)
        {
            mEntityId = EntityId.Parts;
            mParts = new PartsD(name, lines, arcs, texts);
            mEntityName = "パーツ";
            mArea = mParts.getBox();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">パーツ名</param>
        /// <param name="points">点データ</param>
        /// <param name="lines">線分データ</param>
        /// <param name="arcs">円弧データ</param>
        /// <param name="texts">文字列データ</param>
        public PartsEntity(string name, List<PointD> points, List<LineD> lines, List<ArcD> arcs, List<TextD> texts)
        {
            mEntityId = EntityId.Parts;
            mParts = new PartsD(name, points, lines, arcs, texts);
            mEntityName = "パーツ";
            mArea = mParts.getBox();
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns></returns>
        public override Entity toCopy()
        {
            PartsEntity parts = new PartsEntity();
            parts.setProperty(this);
            parts.mParts = mParts.toCopy();
            parts.mArea = mArea.toCopy();
            return parts;
        }

        /// <summary>
        /// 描画処理
        /// </summary>
        /// <param name="ydraw"></param>
        public override void draw(YWorldDraw ydraw)
        {
            ydraw.mBrush = mPick ? mPickColor : mColor;
            ydraw.mTextColor = mPick ? mPickColor : mColor;
            ydraw.mThickness = mThickness;
            ydraw.mLineType = mType;
            ydraw.mPointSize = 1;
            ydraw.mPointType = 1;
            if (mParts.mPoints != null) {
                foreach (var point in mParts.mPoints)
                    ydraw.drawWPoint(point);
            }
            if (mParts.mLines != null) {
                foreach (var line in mParts.mLines)
                    ydraw.drawWLine(line);
            }
            if (mParts.mArcs != null) {
                foreach (var arc in mParts.mArcs)
                    ydraw.drawWArc(arc, false);
            }
            if (mParts.mTexts != null) {
                ydraw.mFontFamily = mParts.mFontFamily;
                ydraw.mFontStyle  = mParts.mFontStyle;
                ydraw.mFontWeight = mParts.mFontWeight;
                foreach (var text in mParts.mTexts)
                    ydraw.drawWText(text);
            }
        }

        /// <summary>
        /// 文字データによる座標設定
        /// </summary>
        /// <param name="data"></param>
        public override void setData(string[] data)
        {
            mParts = new PartsD();
            mParts.mTextSize = 0;
            for (int i = 0; i < data.Length - 1; i++) {
                try {
                    if (data[i] == "point") {
                        PointD point = new();
                        point.x = ylib.string2double(data[++i]);
                        point.y = ylib.string2double(data[++i]);
                        mParts.mPoints.Add(point);
                    } else if (data[i] == "line") {
                        LineD line = new();
                        line.ps.x = ylib.string2double(data[++i]);
                        line.ps.y = ylib.string2double(data[++i]);
                        line.pe.x = ylib.string2double(data[++i]);
                        line.pe.y = ylib.string2double(data[++i]);
                        mParts.mLines.Add(line);
                    } else if (data[i] == "arc") {
                        ArcD arc = new();
                        arc.mCp.x = ylib.string2double(data[++i]);
                        arc.mCp.y = ylib.string2double(data[++i]);
                        arc.mR = ylib.string2double(data[++i]);
                        arc.mSa = ylib.string2double(data[++i]);
                        arc.mEa = ylib.string2double(data[++i]);
                        mParts.mArcs.Add(arc);
                    } else if (data[i] == "text") {
                        TextD text = new();
                        text.mText = ylib.strControlCodeRev(data[++i]);
                        text.mPos.x = ylib.string2double(data[++i]);
                        text.mPos.y = ylib.string2double(data[++i]);
                        text.mTextSize = ylib.string2double(data[++i]);
                        text.mRotate = ylib.string2double(data[++i]);
                        text.mHa = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), data[++i]);
                        text.mVa = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), data[++i]);
                        mParts.mTexts.Add(text);
                    } else if (data[i] == "name") {
                        mParts.mName = data[++i];
                    } else if (data[i] == "textSize") {
                        mParts.mTextSize = ylib.string2double(data[++i]);
                    } else if (data[i] == "fontFamily") {
                        mParts.mFontFamily = data[++i];
                    } else if (data[i] == "fontStyle") {
                        mParts.mFontStyle = ylib.convFontStyle(data[++i]);
                    } else if (data[i] == "fontWeight") {
                        mParts.mFontWeight = ylib.convFontWeight(data[++i]);
                    } else if (data[i] == "linePitchRate") {
                        mParts.mLinePitchRate = ylib.string2double(data[++i]);
                    } else if (data[i] == "textRotate") {
                        mParts.mTextRotate = ylib.string2double(data[++i]);
                    } else if (data[i] == "arrowAngle") {
                        mParts.mArrowAngle = ylib.string2double(data[++i]);
                    } else if (data[i] == "arrowSize") {
                        mParts.mArrowSize = ylib.string2double(data[++i]);
                    } else if (data[i] == "refPoint") {
                        PointD point = new();
                        point.x = ylib.string2double(data[++i]);
                        point.y = ylib.string2double(data[++i]);
                        mParts.mRefPoints.Add(point);
                    } else if (data[i] == "refValue") {
                        double v = ylib.string2double(data[++i]);
                        mParts.mRefValue.Add(v);
                    }
                } catch (Exception e) {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
            if (mParts.mTextSize == 0) {
                if (0 < mParts.mTexts.Count)
                    mParts.mTextSize = mParts.mTexts[0].mTextSize;
                else
                    mParts.mTextSize = 12;
            }
            mArea = mParts.getBox();
        }

        /// <summary>
        /// 座標データの文字列変換
        /// </summary>
        /// <returns></returns>
        public override string toDataString()
        {
            string buf = "";
            buf += "name," + mParts.mName + ",";
            if (mParts.mPoints != null) {
                foreach (var point in mParts.mPoints)
                    buf += $"point,{point.x},{point.y},";
            }
            if (mParts.mLines != null) {
                foreach (var line in mParts.mLines)
                    buf += $"line,{line.ps.x},{line.ps.y},{line.pe.x},{line.pe.y},";
            }
            if (mParts.mArcs != null) {
                foreach (var arc in mParts.mArcs)
                    buf += $"arc,{arc.mCp.x},{arc.mCp.y},{arc.mR},{arc.mSa},{arc.mEa},";
            }
            if (mParts.mTexts != null) {
                foreach (var text in mParts.mTexts)
                    buf += $"text,{ylib.strControlCodeCnv(text.mText)},{text.mPos.x},{text.mPos.y},{text.mTextSize},{text.mRotate},{text.mHa},{text.mVa},";
            }
            buf += $"textSize,{mParts.mTextSize},";
            buf += $"textRotate,{mParts.mTextRotate},";
            buf += $"fontFamily,{mParts.mFontFamily},";
            buf += $"fontStyle,{mParts.mFontStyle},";
            buf += $"fontWeight,{mParts.mFontWeight},";
            buf += $"linePitchRate,{mParts.mLinePitchRate},";
            buf += $"arrowSize,{mParts.mArrowSize},";
            buf += $"arrowAngle,{mParts.mArrowAngle},";
            if (mParts.mRefString != null) {
                foreach (var str in mParts.mRefString)
                    buf += $"refString,{str},";
            }
            if (mParts.mRefPoints != null) {
                foreach (var point in mParts.mRefPoints)
                    buf += $"refPoint,{point.x},{point.y},";
            }
            if (mParts.mRefValue != null) {
                foreach (var v in mParts.mRefValue)
                    buf += $"refValue,{v},";
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
            dataList.Add("name");
            dataList.Add(mParts.mName);
            if (mParts.mPoints != null) {
                foreach (var point in mParts.mPoints) {
                    dataList.Add("point");
                    dataList.Add(point.x.ToString());
                    dataList.Add(point.y.ToString());
                }
            }
            if (mParts.mLines != null) {
                foreach (var line in mParts.mLines) {
                    dataList.Add("line");
                    dataList.Add(line.ps.x.ToString());
                    dataList.Add(line.ps.y.ToString());
                    dataList.Add(line.pe.x.ToString());
                    dataList.Add(line.pe.y.ToString());
                }
            }
            if (mParts.mArcs != null) {
                foreach (var arc in mParts.mArcs) {
                    dataList.Add("arc");
                    dataList.Add(arc.mCp.x.ToString());
                    dataList.Add(arc.mCp.y.ToString());
                    dataList.Add(arc.mR.ToString());
                    dataList.Add(arc.mSa.ToString());
                    dataList.Add(arc.mEa.ToString());
                }
            }
            if (mParts.mTexts != null) {
                foreach (var text in mParts.mTexts) {
                    dataList.Add("text");
                    dataList.Add(ylib.strControlCodeCnv(text.mText));
                    dataList.Add(text.mPos.x.ToString());
                    dataList.Add(text.mPos.y.ToString());
                    dataList.Add(text.mTextSize.ToString());
                    dataList.Add(text.mRotate.ToString());
                    dataList.Add(text.mHa.ToString());
                    dataList.Add(text.mVa.ToString());
                }
            }
            dataList.Add("textSize");      dataList.Add(mParts.mTextSize.ToString());
            dataList.Add("textRotate");    dataList.Add(mParts.mTextRotate.ToString());
            dataList.Add("fontFamily");    dataList.Add(mParts.mFontFamily);
            dataList.Add("fontStyle");     dataList.Add(mParts.mFontStyle.ToString());
            dataList.Add("fontWeight");     dataList.Add(mParts.mFontWeight.ToString());
            dataList.Add("linePitchRate"); dataList.Add(mParts.mLinePitchRate.ToString());
            dataList.Add("arrowSize");     dataList.Add(mParts.mArrowSize.ToString());
            dataList.Add("arrowAngle");    dataList.Add(mParts.mArrowAngle.ToString());
            if (mParts.mRefString != null) {
                foreach (var str in mParts.mRefString) {
                    dataList.Add("refString");
                    dataList.Add(str);
                }
            }
            if (mParts.mRefPoints != null) {
                foreach (var point in mParts.mRefPoints) {
                    dataList.Add("refPoint");
                    dataList.Add(point.x.ToString());
                    dataList.Add(point.y.ToString());
                }
            }
            if (mParts.mRefValue != null) {
                foreach (var v in mParts.mRefValue) {
                    dataList.Add("refValue");
                    dataList.Add(v.ToString());
                }
            }
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
            buf += $"\n名称: " + mParts.mName;
            buf += $"\n領域 {mArea.ToString("f2")}";
            buf += $"\n文字高さ: {mParts.mTextSize.ToString("f4")}";
            buf += $"\nフォント: {mParts.mFontFamily} 斜体 {mParts.mFontStyle} 太さ {mParts.mFontWeight}";
            buf += $"\nカラー: {getColorName(mColor)}";
            buf += $"\n太さ: {mThickness}";
            buf += $"\nレイヤー: {mLayerName}";

            return buf;
        }

        /// <summary>
        /// 要素情報の要約の取得
        /// </summary>
        /// <returns></returns>
        public override string getSummary()
        {
            return $"{mNo}:{mEntityName} {mParts.mName} {mArea.ToString("f1")} {getColorName(mColor)}";
        }

        /// <summary>
        /// 要素を移動する
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        public override void translate(PointD vec)
        {
            mParts.translate(vec);
            mArea = mParts.getBox();
        }

        /// <summary>
        /// 回転
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="mp">回転角座標</param>
        public override void rotate(PointD cp, PointD mp)
        {
            mParts.rotate(cp, mp);
            mArea = mParts.getBox();
        }

        /// <summary>
        /// ミラー
        /// </summary>
        /// <param name="sp">始点座標</param>
        /// <param name="ep">終点座標</param>
        public override void mirror(PointD sp, PointD ep)
        {
            mParts.mirror(sp, ep);
            mArea = mParts.getBox();
        }

        /// <summary>
        /// 原点を指定して拡大縮小
        /// </summary>
        /// <param name="cp">原点</param>
        /// <param name="scale">拡大率</param>
        public override void scale(PointD cp, double scale)
        {
            mParts.scale(cp, scale);
            mArea = mParts.getBox();
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
        public override void stretch(PointD vec, PointD pickPos, bool arc = false)
        {
            mParts.stretch(vec, pickPos);
            mArea = mParts.getBox();
        }

        /// <summary>
        /// 参照点に対する垂点を求める
        /// </summary>
        /// <param name="pos">参照点</param>
        /// <returns>垂点</returns>
        public override PointD onPoint(PointD pos)
        {
            return mParts.onPoint(pos);
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
                    plist = mParts.intersection(point.mPoint);
                    break;
                case EntityId.Line:
                    LineEntity line = (LineEntity)entity;
                    plist = mParts.intersection(line.mLine);
                    break;
                case EntityId.Arc:
                    ArcEntity arc = (ArcEntity)entity;
                    plist = mParts.intersection(arc.mArc);
                    break;
                case EntityId.Polyline:
                    PolylineEntity polyline = (PolylineEntity)entity;
                    plist = mParts.intersection(polyline.mPolyline);
                    break;
                case EntityId.Polygon:
                    PolygonEntity polygon = (PolygonEntity)entity;
                    plist = mParts.intersection(polygon.mPolygon);
                    break;
                case EntityId.Text:
                    break;
                case EntityId.Ellipse:
                    EllipseEntity ellipse = (EllipseEntity)entity;
                    plist = mParts.intersection(ellipse.mEllipse);
                    break;
                case EntityId.Parts:
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
            List<PointD> plist = b.intersection(mParts);
            if (0 < plist.Count)
                return true;
            if (mParts.textInsideChk(b))
                return true;
            foreach (var point in mParts.mPoints)
                if (b.insideChk(point))
                    return true;
            return false;
        }

        /// <summary>
        /// ピック位置に最も近い端点を求める
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <returns>端点座標(不定値はisNaN()でチェック)</returns>
        public override PointD getEndPoint(PointD pickPos)
        {
            return mParts.nearPoint(pickPos);
        }

        /// <summary>
        /// 参照点からピック位置と同じ方向でピック位置に近い端点
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="cp">参照位置</param>
        /// <returns>端点座標</returns>
        public override PointD getEndPoint(PointD pickPos, PointD cp)
        {
            return mParts.nearPoint(pickPos);
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
            mArea = mParts.getBox();
        }
    }
}
