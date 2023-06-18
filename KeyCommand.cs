using CoreLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CadApp
{
    /// <summary>
    /// キー入力でのコマンド処理
    /// </summary>
    public class KeyCommand
    {
        public int mPointType = 0;                                  //  点種
        public int mLineType = 0;                                   //  線種
        public double mEntSize = 1;                                 //  線の太さ
        public double mPointSize = 1;                               //  点の大きさ
        public double mTextSize = 12;                               //  文字サイズ
        public double mTextRotate = 0;                              //  文字列の回転角
        public string mTextString = "";                             //  文字列データ
        public HorizontalAlignment mHa = HorizontalAlignment.Left;  //  水平アライメント
        public VerticalAlignment mVa = VerticalAlignment.Top;       //  垂直アライメント
        public Brush mCreateColor = Brushes.Black;                  //  要素の色

        private List<string> mMainCmd = new List<string>() {
            "point", "line", "rect", "polyline", "polygon", "arc", "circle", "text",
            "color", "linetype", "thickness", "pointtype", "pointsize", "textsize", "ha", "va"
        };

        public string mCommandStr = "";

        private List<PointD> mPoints = new List<PointD>();
        private double mRadius = 0;
        private double mSa = 0;
        private double mEa = Math.PI * 2;
        private double mValue = 0;
        private string mValString = "";
        private int mCommandNo = -1;

        private YLib ylib = new YLib();

        public KeyCommand() { 
        }

        /// <summary>
        /// コマンド文字列の設定
        /// </summary>
        /// <param name="command"></param>
        public void setCommand(string command)
        {
            mCommandStr = command;
        }

        /// <summary>
        /// コマンド文字列から要素を作成
        /// </summary>
        /// <returns></returns>
        public Entity getEntity()
        {
            getCommandParameter(mCommandStr);
            Entity entity = null;
            switch (mCommandNo) {
                case 0:         //  point
                    if (0 < mPoints.Count) {
                        entity = new PointEntity(mPoints[0]);
                    }
                    break;
                case 1:         //  line
                    if (1 < mPoints.Count) {
                        LineD line = new LineD(mPoints[0], mPoints[1]);
                        entity = new LineEntity(line);
                    }
                    break;
                case 2:         //  rect
                    if (1 < mPoints.Count) {
                        Box b = new Box(mPoints[0], mPoints[1]);
                        entity = new PolygonEntity(b.ToPointDList());
                    }
                    break;
                case 3:         //  polyline
                    if (1 < mPoints.Count) {
                        entity = new PolylineEntity(mPoints);
                    }
                    break;
                case 4:         //  polygon
                    if (1 < mPoints.Count) {
                        entity = new PolygonEntity(mPoints);
                    }
                    break;
                case 5:         //  arc
                case 6:         //  circle
                    if (0 < mPoints.Count) {
                        ArcD arc = new ArcD(mPoints[0], mRadius, mSa, mEa);
                        entity = new ArcEntity(arc);
                    }
                    break;
                case 7:         //  text
                    if (0 < mValString.Length && 0 < mPoints.Count) {
                        TextD text = new TextD(mValString, mPoints[0]);
                        entity = new TextEntity(text);
                    }
                    break;
                case 8:         //  color
                    mCreateColor = ylib.mColorList.Find(p => p.colorTitle == mValString).brush;
                    break;
                case 9:         //  linetype
                    mLineType = (int)mValue;
                    break;
                case 10:        //  thickness
                    mEntSize = mValue;
                    break;
                case 11:        //  pointtype
                    mPointType = (int)mValue;
                    break;
                case 12:        //  pointsize
                    mPointSize = mValue;
                    break;
                case 13:        //  textsize
                    mTextSize = mValue;
                    break;
            }
            return entity;
        }

        /// <summary>
        /// コマンド文字列からコマンドやパラメータ値を求める
        /// </summary>
        /// <param name="command"></param>
        private void getCommandParameter(string command)
        {
            mCommandNo = -1;
            mRadius = 0;
            mSa = 0;
            mEa = 0;
            mValue = 0;
            mValString = "";
            mPoints.Clear();
            List<string> cmd = commandSplit(command);
            for (int i = 0; i < cmd.Count; i++) {
                cmd[i] = cmd[i].ToLower();
                if (mCommandNo < 0) {
                    mCommandNo = mMainCmd.FindIndex(p => 0 <= p.IndexOf(cmd[i]));
                    continue;
                }
                if (0 == cmd[i].IndexOf("dx") && 0 < mPoints.Count) {
                    PointD dp = getPoint(cmd[i], "dx", "dy");       //  相対座標
                    dp.offset(mPoints[mPoints.Count - 1]);
                    mPoints.Add(dp);
                } else if (0 == cmd[i].IndexOf("x")) {
                    mPoints.Add(getPoint(cmd[i]));                  //  座標
                } else if (0 == cmd[i].IndexOf("r")) {
                    mRadius = getPara(cmd[i], "r");                 //  半径
                } else if (0 == cmd[i].IndexOf("sa")) {
                    mSa = getPara(cmd[i], "sa");                    //  始角
                } else if (0 == cmd[i].IndexOf("ea")) {
                    mEa = getPara(cmd[i], "ea");                    //  終角
                }else if (char.IsDigit(cmd[i][0]) || cmd[i][0]== '-') {
                    mValue = ylib.string2double(cmd[i]);            //  数値
                } else if (cmd[i][0] == '"') {
                    mValString = cmd[i].Trim('"');                  //  文字列
                } else {
                    mValString = cmd[i];                            //  文字列
                }
            }
        }

        /// <summary>
        /// パラメータ文字列からPointDの値に変換
        /// </summary>
        /// <param name="xy">パラメータ文字列</param>
        /// <param name="a">パラメータ名</param>
        /// <param name="b">パラメータ名</param>
        /// <returns>パラメータ値</returns>
        private PointD getPoint(string xy, string a = "x", string b = "y")
        {
            double x = 0, y = 0;
            int xn =xy.IndexOf(a);
            int yn =xy.IndexOf(b);
            x = ylib.string2double(xy.Substring(xn + a.Length));
            y = ylib.string2double(xy.Substring(yn + b.Length));
            return new PointD(x, y);
        }

        /// <summary>
        /// パラメータ文字列から値に変換
        /// </summary>
        /// <param name="paraStr">パラメータ文字列</param>
        /// <param name="para">パラメータの種類</param>
        /// <returns>パラメータ値</returns>
        private double getPara(string paraStr, string para)
        {
            int rn = paraStr.IndexOf(para);
            double r = ylib.string2double(paraStr.Substring(rn + 1));
            return r;
        }

        /// <summary>
        /// コマンド文字列をコマンドやパラメータなどに分解
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <returns>パラメータリスト</returns>
        private List<string> commandSplit(string command)
        {
            List<string> cmd = new List<string>();
            string buf = "";
            for (int i = 0; i < command.Length; i++) {
                if (command[i] == ' ' || command[i] == ',') {
                    if (0 < buf.Length) {
                        cmd.Add(buf);
                        buf = "";
                    } 
                } else if (command[i] == '"') {
                    if (0 < buf.Length) {
                        cmd.Add(buf);
                        buf = "";
                    }
                    buf += command[i++];
                    do {
                        buf += command[i];
                    } while (i < command.Length - 1 && command[i++] != '"');
                    cmd.Add(buf);
                    buf = "";
                } else {
                    buf += command[i];
                }
            }
            if (0 < buf.Length) {
                cmd.Add(buf);
                buf = "";
            }
            return cmd;
        }
    }
}
