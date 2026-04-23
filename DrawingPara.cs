using CoreLib;
using System;
using System.Windows;
using System.Windows.Media;

namespace CadApp
{
    /// <summary>
    /// 図面のパラメータ
    /// </summary>
    public class DrawingPara
    {
        public int mPointType = 0;                                  //  点種
        public double mPointSize = 1;                               //  点の大きさ
        public int mLineType = 0;                                   //  線種
        public double mThickness = 1;                               //  線の太さ
        public double mTextSize = 12;                               //  文字サイズ
        public double mTextRotate = 0;                              //  文字列の回転角
        public double mLinePitchRate = 1.2;                         //  文字列の改行幅率
        public HorizontalAlignment mHa = HorizontalAlignment.Left;  //  水平アライメント
        public VerticalAlignment mVa = VerticalAlignment.Top;       //  垂直アライメント
        public string mFontFamily = "";                             //  フォント種別(Yu Gothic UI)
        public FontStyle mFontStyle = FontStyles.Normal;            //  斜体 Normal,Italic
        public FontWeight mFontWeight = FontWeights.Normal;         //  太字 Thin,Normal,Bold
        public double mFilletSize = 0;                              //  R面の半径
        public double mArrowAngle = Math.PI / 6;                    //  矢印の角度
        public double mArrowSize = 5;                               //  矢印の大きさ
        public Brush mColor = Brushes.Black;                        //  要素の色
        public double mGridSize = 1.0;                              //  マウス座標の丸め値
        public string mComment = "";                                //  図面のコメント
        public string mMemo = "";                                   //  メモデータ
        public ulong mDispLayerBit = 0xffffffff;                    //  表示レイヤービットフィルタ
        public string mCreateLayerName = "BaseLayer";               //  作成レイヤー名
        public bool mOneLayerDisp = false;                          //  1レイヤーのみの表示
        public int mSymbolCategoryIndex = 0;                        //  シンボル分類No
        public Brush mBackColor = Brushes.White;                    //  背景色

        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DrawingPara()
        {
            mFontFamily = SystemFonts.MessageFontFamily.Source;
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns>DrawimgPara</returns>
        public DrawingPara toCopy()
        {
            DrawingPara para = new DrawingPara();
            para.mPointType = mPointType;
            para.mPointSize = mPointSize;
            para.mLineType = mLineType;
            para.mThickness = mThickness;
            para.mTextSize = mTextSize;
            para.mTextRotate = mTextRotate;
            para.mLinePitchRate = mLinePitchRate;
            para.mHa = mHa;
            para.mVa = mVa;
            para.mFontFamily = mFontFamily;
            para.mFontStyle = mFontStyle;
            para.mFontWeight = mFontWeight;
            para.mArrowAngle = mArrowAngle;
            para.mArrowSize = mArrowSize;
            para.mColor = mColor;
            para.mGridSize = mGridSize;
            para.mComment = mComment;
            para.mMemo = mMemo;
            para.mDispLayerBit = mDispLayerBit;
            para.mCreateLayerName = mCreateLayerName;
            para.mOneLayerDisp = mOneLayerDisp;
            para.mSymbolCategoryIndex = mSymbolCategoryIndex;
            para.mBackColor = mBackColor;
            return para;
        }

        /// <summary>
        /// 初期値に設定
        /// </summary>
        public void init()
        {
            mPointType = 0;                                 //  点種
            mPointSize = 1;                                 //  点の大きさ
            mLineType = 0;                                  //  線種
            mThickness = 1;                                 //  線の太さ
            mTextSize = 12;                                 //  文字サイズ
            mTextRotate = 0;                                //  文字列の回転角
            mLinePitchRate = 1.2;                           //  文字列の改行幅率
            mHa = HorizontalAlignment.Left;                 //  水平アライメント
            mVa = VerticalAlignment.Top;                    //  垂直アライメント
            mFontFamily = SystemFonts.MessageFontFamily.Source; //  フォントファミリ
            mFontStyle = FontStyles.Normal;                 //  斜体
            mFontWeight = FontWeights.Normal;               //  太さ
            mArrowAngle = Math.PI / 6;                      //  矢印の角度
            mArrowSize = 5;                                 //  矢印の大きさ
            mColor = Brushes.Black;                         //  要素の色
            mGridSize = 1.0;                                //  マウス座標の丸め値
            mComment = "";                                  //  図面のコメント
            mMemo = "";                                     //  図面のメモ
            mDispLayerBit = 0xffffffff;                     //  表示レイヤービットフィルタ
            mCreateLayerName = "BaseLayer";                 //  作成レイヤー名
            mOneLayerDisp = false;                          //  1レイヤーのみの表示
            mSymbolCategoryIndex = 0;                       //  シンボル分類No
            mBackColor = Brushes.White;                     //  背景色
        }

        /// <summary>
        /// パラメータを文字列に変換
        /// </summary>
        /// <returns></returns>
        public string propertyToString()
        {
            return $"Prperty,Color,{ylib.getColorName(mColor)},PointType,{mPointType},PointSize,{mPointSize}," +
                $"LineType,{mLineType},Thickness,{mThickness},TextSize,{mTextSize}," +
                $"TextRotate,{mTextRotate},LinePitchRate,{mLinePitchRate},HA,{mHa},VA,{mVa}," +
                $"ArrowSize,{mArrowSize},ArrowAngle,{mArrowAngle},GridSize,{mGridSize},DispLayerBit,{mDispLayerBit}," +
                $"CreateLayer,{ylib.strControlCodeCnv(mCreateLayerName)},OneLayerDisp,{mOneLayerDisp}," +
                $"FontFamily,{mFontFamily},FontStyle,{mFontStyle},FontWeight,{mFontWeight}," +
                $"SymbolCategoryIindex,{mSymbolCategoryIndex},BackColor,{ylib.getBrushName(mBackColor)}";
        }

        /// <summary>
        /// 文字列配列をプロパティ設定値に変換
        /// </summary>
        /// <param name="data"></param>
        public void setPropertyData(string[] data)
        {
            try {
                if (1 < data.Length && data[0] == "Prperty") {
                    for (int i = 1; i < data.Length; i++) {
                        switch (data[i]) {
                            case "Color":
                                mColor = ylib.getColor(data[++i]);
                                break;
                            case "PointType":
                                mPointType = int.Parse(data[++i]);
                                break;
                            case "PointSize":
                                mPointSize = double.Parse(data[++i]);
                                break;
                            case "LineType":
                                mLineType = int.Parse(data[++i]);
                                break;
                            case "Thickness":
                                mThickness = double.Parse(data[++i]);
                                break;
                            case "TextSize":
                                mTextSize = double.Parse(data[++i]);
                                break;
                            case "TextRotate":
                                mTextRotate = double.Parse(data[++i]);
                                break;
                            case "LinePitchRate":
                                mLinePitchRate = double.Parse(data[++i]);
                                break;
                            case "HA":
                                mHa = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), data[++i]);
                                break;
                            case "VA":
                                mVa = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), data[++i]);
                                break;
                            case "FontFamily":
                                mFontFamily = data[++i];
                                break;
                            case "FontStyle":
                                mFontStyle = ylib.convFontStyle(data[++i]);
                                break;
                            case "FontWeight":
                                mFontWeight = ylib.convFontWeight(data[++i]);
                                break;
                            case "ArrowSize":
                                mArrowSize = double.Parse(data[++i]);
                                break;
                            case "ArrowAngle":
                                mArrowAngle = double.Parse(data[++i]);
                                break;
                            case "GridSize":
                                mGridSize = double.Parse(data[++i]);
                                break;
                            case "DispLayerBit":
                                mDispLayerBit = ulong.Parse(data[++i]);
                                break;
                            case "CreateLayer":
                                mCreateLayerName = ylib.strControlCodeRev(data[++i]);
                                break;
                            case "OneLayerDisp":
                                mOneLayerDisp = bool.Parse(data[++i]);
                                break;
                            case "SymbolCategoryIindex":
                                mSymbolCategoryIndex = int.Parse(data[++i]);
                                break;
                            case "BackColor":
                                mBackColor = ylib.getBrsh(data[++i]);
                                break;
                        }
                    }
                } else {
                    mColor = ylib.getColor(data[0]);
                    mPointType = int.Parse(data[1]);
                    mPointSize = double.Parse(data[2]);
                    mLineType = int.Parse(data[3]);
                    mThickness = double.Parse(data[4]);
                    mTextSize = double.Parse(data[5]);
                    mArrowSize = double.Parse(data[6]);
                    mArrowAngle = double.Parse(data[7]);
                    mGridSize = double.Parse(data[8]);
                    if (mArrowAngle == 0)
                        mArrowAngle = 30 * Math.PI / 180;
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                ylib.messageBox(null, e.Message, "SetPropertyData 例外エラー");
            }
        }

        /// <summary>
        /// 図面情報を文字列に変換
        /// </summary>
        /// <returns></returns>
        public string[] commentToString()
        {
            string[] buf = { "Comment",
                "Comment", ylib.strControlCodeCnv(mComment),
                "Memo", ylib.strControlCodeCnv(mMemo),
            };
            return buf;
        }

        /// <summary>
        /// 図面情報のコメントをパラメータに設定
        /// </summary>
        /// <param name="data"></param>
        public void setCommentData(string[] data)
        {
            try {
                if (1 < data.Length && data[0] == "Comment") {
                    for (int i = 1; i < data.Length; i++) {
                        switch (data[i]) {
                            case "Comment":
                                mComment = ylib.strControlCodeRev(data[++i]);
                                break;
                            case "Memo":
                                mMemo = ylib.strControlCodeRev(data[++i]);
                                break;
                        }
                    }
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
    }
}
