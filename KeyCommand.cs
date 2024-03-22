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
        public EntityData mEntityData;                              //  要素データ

        public string mTextString = "";                             //  文字列データ
        public DrawingPara mPara = new DrawingPara();               //  要素属性

        private List<string> mMainCmd = new List<string>() {
            "point", "line", "rect", "polyline", "polygon", "arc", "circle", "text",
            "translate", "rotate", "mirror", "trim", "stretch", "copy", "scaling",
            "color", "linetype", "thickness", "pointtype", "pointsize", "textsize", "ha", "va"
        };

        public string mCommandStr = "";

        private List<PointD> mPoints = new List<PointD>();
        private List<int> mPickEnt = new List<int>();
        private double mRadius = 0;
        private double mSa = 0;
        private double mEa = Math.PI * 2;
        private double mValue = 0;
        private string mValString = "";
        private int mCommandNo = -1;

        private YCalc ycalc = new YCalc();
        private YLib ylib = new YLib();

        public KeyCommand() { 
        }

        /// <summary>
        /// コマンド文字列の設定
        /// </summary>
        /// <param name="command"></param>
        public bool setCommand(string command, DrawingPara para)
        {
            if (0 < command.Length) {
                mCommandStr = command;
                mPara = para;
                return getEntity();
            }
            return false;
        }

        /// <summary>
        /// 作成要素の登録
        /// </summary>
        /// <param name="entity">要素データ</param>
        /// <returns></returns>
        private bool createEntity(Entity entity)
        {
            if (entity != null) {
                entity.mOperationCount = mEntityData.mOperationCount;
                entity.mColor = mPara.mColor;
                entity.mThickness = mPara.mThickness;
                entity.mType = mPara.mLineType;
                if (entity.mEntityId == EntityId.Point) {
                    entity.mThickness = mPara.mPointSize;
                    entity.mType = mPara.mPointType;
                }
                entity.mLayerName = mPara.mCreateLayerName;
                mEntityData.mEntityList.Add(entity);
                mEntityData.updateData();
                return true;
            }
            return false;
        }

        /// <summary>
        /// コマンド文字列から要素を作成
        /// </summary>
        /// <returns></returns>
        public bool getEntity()
        {
            getCommandParameter(mCommandStr);
            Entity entity = null;
            switch (mMainCmd[mCommandNo]) {
                case "point":           //  point
                    if (0 < mPoints.Count) {
                        entity = new PointEntity(mPoints[0]);
                        return createEntity(entity);
                    }
                    break;
                case "line":            //  line
                    if (1 < mPoints.Count&& 0 < mPoints[0].length(mPoints[1])) {
                        LineD line = new LineD(mPoints[0], mPoints[1]);
                        entity = new LineEntity(line);
                        return createEntity(entity);
                    }
                    break;
                case "rect":            //  rect
                    if (1 < mPoints.Count) {
                        Box b = new Box(mPoints[0], mPoints[1]);
                        entity = new PolygonEntity(b.ToPointList());
                        return createEntity(entity);
                    }
                    break;
                case "polyline":        //  polyline
                    if (1 < mPoints.Count) {
                        entity = new PolylineEntity(mPoints);
                        return createEntity(entity);
                    }
                    break;
                case "polygon":         //  polygon
                    if (1 < mPoints.Count) {
                        entity = new PolygonEntity(mPoints);
                        return createEntity(entity);
                    }
                    break;
                case "arc":             //  arc
                    if (0 < mPoints.Count && 0 < mRadius) {
                        if (mEa <= mSa) mEa += Math.PI * 2;
                        ArcD arc = new ArcD(mPoints[0], mRadius, mSa, mEa);
                        entity = new ArcEntity(arc);
                        return createEntity(entity);
                    }
                    break;
                case "circle":          //  circle
                    if (0 < mPoints.Count && 0 < mRadius) {
                        ArcD arc = new ArcD(mPoints[0], mRadius, 0, Math.PI * 2);
                        entity = new ArcEntity(arc);
                        return createEntity(entity);
                    }
                    break;
                case "text":         //  text
                    if (0 < mTextString.Length && 0 < mPoints.Count) {
                        TextD text = new TextD(mTextString, mPoints[0], mPara.mTextSize, mPara.mTextRotate, mPara.mHa, mPara.mVa, mPara.mLinePitchRate);
                        entity = new TextEntity(text);
                        return createEntity(entity);
                    }
                    break;
                case "translate":
                    if (0 < mPickEnt.Count && 1 < mPoints.Count) {
                        PointD vec = mPoints[0].vector(mPoints[1]);
                        foreach (int entNo in mPickEnt) {
                            mEntityData.mEntityList[entNo].translate(vec);
                        }
                        mEntityData.updateData();
                        return true;
                    }
                    break;
                case "rotate":
                    if (0 < mPickEnt.Count && 1 < mPoints.Count) {
                        foreach (int entNo in mPickEnt) {
                            mEntityData.mEntityList[entNo].rotate(mPoints[0], mPoints[1]);
                        }
                        mEntityData.updateData();
                        return true;
                    }
                    break;
                case "mirror":
                    if (0 < mPickEnt.Count && 1 < mPoints.Count) {
                        foreach (int entNo in mPickEnt) {
                            mEntityData.mEntityList[entNo].rotate(mPoints[0], mPoints[1]);
                        }
                        mEntityData.updateData();
                        return true;
                    }
                    break;
                case "trim":
                    if (0 < mPickEnt.Count && 1 < mPoints.Count) {
                        foreach (int entNo in mPickEnt) {
                            mEntityData.mEntityList[entNo].trim(mPoints[0], mPoints[1]);
                        }
                        mEntityData.updateData();
                        return true;
                    }
                    break;
                case "stretch":
                    break;
                case "copy":
                    break;
                case "scaling":
                    break;
                case "color":         //  color
                    mPara.mColor = ylib.mColorList.Find(p => p.colorTitle == mValString).brush;
                    break;
                case "linetype":         //  linetype
                    mPara.mLineType = (int)mValue;
                    break;
                case "thickness":        //  thickness
                    mPara.mThickness = mValue;
                    break;
                case "pointtype":        //  pointtype
                    mPara.mPointType = (int)mValue;
                    break;
                case "pointsize":        //  pointsize
                    mPara.mPointSize = mValue;
                    break;
                case "textsize":        //  textsize
                    mPara.mTextSize = mValue;
                    break;
            }
            return false;
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
            mPickEnt.Clear();
            mPoints.Clear();
            List<string> cmd = commandSplit(command);
            for (int i = 0; i < cmd.Count; i++) {
                if (0 > cmd[i].IndexOf("\""))
                    cmd[i] = cmd[i].ToLower();
                if (mCommandNo < 0) {
                    mCommandNo = mMainCmd.FindIndex(p => 0 <= p.IndexOf(cmd[i]));
                    continue;
                }
                if (0 == cmd[i].IndexOf("x") || 0 == cmd[i].IndexOf("y") ||
                    0 == cmd[i].IndexOf("dx") || 0 == cmd[i].IndexOf("dy")) {
                    //  座標/相対座標
                    PointD dp = getPoint(cmd[i],
                        mPoints.Count < 1 ? new PointD(0, 0) : mPoints[mPoints.Count - 1]);
                    mPoints.Add(dp);
                } else if (0 == cmd[i].IndexOf("p")) {
                    //  要素番号
                    mPickEnt.Add(getIntPara(cmd[i], "p"));
                } else if (0 == cmd[i].IndexOf("r")) {
                    //  半径
                    mRadius = getPara(cmd[i], "r");
                } else if (0 == cmd[i].IndexOf("sa")) {
                    //  始角
                    mSa = getPara(cmd[i], "sa");
                } else if (0 == cmd[i].IndexOf("ea")) {
                    //  終角
                    mEa = getPara(cmd[i], "ea");
                }else if (char.IsDigit(cmd[i][0]) || cmd[i][0]== '-') {
                    //  数値
                    mValue = ylib.string2double(cmd[i]);
                } else if (cmd[i][0] == '"') {
                    //  文字列
                    mTextString = cmd[i].Trim('"');
                } else {
                    //  その他の文字列
                    mValString = cmd[i];
                }
            }
        }

        /// <summary>
        /// パラメータ文字列からPointDの値に変換(計算式可)
        /// </summary>
        /// <param name="xy">パラメータ文字列</param>
        /// <param name="a">パラメータ名</param>
        /// <param name="b">パラメータ名</param>
        /// <returns>パラメータ値</returns>
        private PointD getPoint(string xy, string a = "x", string b = "y")
        {
            int xn =xy.IndexOf(a);
            int yn =xy.IndexOf(b);
            double x = ycalc.expression(xy.Substring(xn + a.Length,yn - a.Length));
            double y = ycalc.expression(xy.Substring(yn + b.Length));
            return new PointD(x, y);
        }

        /// <summary>
        /// パラメータ文字列(座標/相対座標)からPointDの値に変換(計算式可)
        /// </summary>
        /// <param name="xy">パラメータ文字列</param>
        /// <param name="prev">前座標</param>
        /// <returns>座標</returns>
        private PointD getPoint(string xy, PointD prev)
        {
            PointD p = new PointD();
            string[] sep = { "x", "y", "dx", "dy", ",", " " };
            List<string> list = ylib.splitString(xy, sep);
            for (int i = 0; i < list.Count; i++) {
                if (list[i] == "x" && i + 1 < list.Count) {
                    p.x = ycalc.expression(list[++i]);
                } else if (list[i] == "y" && i + 1 < list.Count) {
                    p.y = ycalc.expression(list[++i]);
                } else if (list[i] == "dx" && i + 1 < list.Count) {
                    p.x = ycalc.expression(list[++i]) + prev.x;
                } else if (list[i] == "dy" && i + 1 < list.Count) {
                    p.y = ycalc.expression(list[++i]) + prev.y;
                }
            }

            return p;
        }

        /// <summary>
        /// パラメータ文字列から値に変換(計算式可)
        /// </summary>
        /// <param name="paraStr">パラメータ文字列</param>
        /// <param name="para">パラメータの種類</param>
        /// <returns>パラメータ値</returns>
        private double getPara(string paraStr, string para)
        {
            int rn = paraStr.IndexOf(para);
            double r = ycalc.expression(paraStr.Substring(rn + 1));
            return r;
        }

        /// <summary>
        /// パラメータ文字列から整数値に変換
        /// </summary>
        /// <param name="paraStr">パラメータ文字列</param>
        /// <param name="para">パラメータの種類</param>
        /// <returns>パラメータ値</returns>
        private int getIntPara(string paraStr, string para)
        {
            int pn = paraStr.IndexOf(para);
            int n = (int)ycalc.expression(paraStr.Substring(pn + 1));
            return n;
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
