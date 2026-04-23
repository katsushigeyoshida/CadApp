using CoreLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CadApp
{
    public class FuncCad
    {
        public static string[] mFuncNames = new string[] {
            "cad.disp(); 再表示",
            "cad.dispFull(); 全体表示",
            "cad.setColor(\"Blue\"); 色の設定",
            "cad.setLineType(\"dash\"); 線種の設定(\"solid\", \"dash\", \"center\", \"phantom\")",
            "cad.setLineThickness(2); 線の太さの設定",
            "cad.setTextPara(size,rotate,HorizontalAlignment,VerticalAlignment,LinePitchRate,FontStyle,FontWeight): 文字パラメータの設定",
            "cad.line(sx,sy,ex,ey/sp[],ep[]/plist[,]); 線分を作成",
            "cad.arc(cp[],r[,sa[,ea]]); 円/円弧の作成",
            "cad.arc(sp[],mp[],ep[]/plist[]); 三点円弧の作成",
            "cad.circle(cp[],r); 半径指定円の作成",
            "cad.circle(x0,y0,x1,y1,x2,y2/sp[],mp[],ep[]/plist[,]); 三点円の作成",
            "cad.ellipse(cp[],rx,ry,rotate,sa,ea): 楕円子の作成",
            "cad.oval(plist[,]/p0[],p1[].../x0,y0,x1,y1...): 楕円の作成",
            "cad.polyline(p[,]/p0[],p1[],p2[]..../x0.y0,x1,y1...); ポリラインの作成",
            "cad.polygon(p[,]/p0[],p1[],p2[]..../x0.y0,x1,y1...); ポリゴンの作成",
            "cad.text([pos[]/x,y],text,size,rotate,horizontal,vertical): 文字列の作成",
            "cad.arrow(plist[,],ArrowSize/p0[],p1[],p2[]...,ArrowSize/x0,y0,x1,y1...,ArrowSize): 矢印の作成",
            "cad.label(plist[,],TextSize/p0[],p1[],p2[]...,TextSize/x0,y0,x1,y1...,TextSize): ラベルの作成",
        };

        public KScript mScript;
        public EntityData mEntityData;          //  要素データ
        public DataDrawing mDataDrawing;        //  表示操作
        public LocPick mLocPick;                //  ロックピックデータ

        private KParse mParse;
        private Variable mVar;
        private KLexer mLexer = new KLexer();
        private YLib ylib = new YLib();
        private YDraw ydraw = new YDraw();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="script">スクリプトクラス</param>
        /// <param name="entityData">要素データ</param>
        /// <param name="dataDrawing">標示操作</param>
        /// <param name="locPick">ロックピックデータ</param>
        public FuncCad(KScript script,  EntityData entityData, DataDrawing dataDrawing, LocPick locPick)
        {
            mScript = script;
            mParse = script.mParse;
            mVar = script.mVar;
            mEntityData = entityData;
            mDataDrawing = dataDrawing;
            mLocPick = locPick;
        }

        /// <summary>
        /// コマンドの割り振り
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="arg"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public Token cadFunc(Token funcName, Token arg, Token ret)
        {
            List<Token> args = mScript.getFuncArgs(arg.mValue);
            switch (funcName.mValue) {
                case "cad.init": init(); break;
                case "cad.disp": disp(); break;
                case "cad.dispFull": dispFull(); break;
                case "cad.dispArea": dispArea(args); break;
                case "cad.setColor": setColor(args); break;
                case "cad.setLineType": setLineType(args); break;
                case "cad.setLineThickness": setLineThickness(args); break;
                case "cad.setTextPara": setTextPara(args); break;
                case "cad.line": line(args); break;
                case "cad.arc": arc(args); break;
                case "cad.circle": circle(args); break;
                case "cad.ellipse": ellipse(args); break;
                case "cad.oval": oval(args); break;
                case "cad.polyline": polyline(args); break;
                case "cad.polygon": polygon(args); break;
                case "cad.text": text(args); break;
                case "cad.arrow": arrow(args); break;
                case "cad.label": label(args); break;
                case "cad.dimension": dimension(args); break;
                default: return new Token("not found func", TokenType.ERROR);
            }
            return new Token("", TokenType.EMPTY);
        }

        private void init()
        {

        }

        /// <summary>
        /// 再表示
        /// </summary>
        private void disp()
        {
            mDataDrawing.disp(mEntityData, mLocPick.mPickEnt);
        }

        /// <summary>
        /// 全体表示
        /// </summary>
        private void dispFull()
        {
            mDataDrawing.dispFit(mEntityData, mLocPick.mPickEnt);
        }

        /// <summary>
        /// 領域表示
        /// </summary>
        /// <param name="args"></param>
        private void dispArea(List<Token> args)
        {
            mDataDrawing.dispFit(mEntityData, mLocPick.mPickEnt);
        }

        /// <summary>
        /// 色の設定
        /// </summary>
        /// <param name="args"></param>
        private void setColor(List<Token> args)
        {
            if (0 < args.Count) {
                string colorName = ylib.stripBracketString(args[0].mValue, '"');
                mEntityData.mPara.mColor = ylib.getColor(colorName);
            }
        }

        /// <summary>
        /// 線種の設定
        /// </summary>
        /// <param name="args"></param>
        private void setLineType(List<Token> args)
        {
            if (0 < args.Count) {
                string lineType = ylib.stripBracketString(args[0].mValue, '"');
                mEntityData.mPara.mLineType = ydraw.mLineTypeName.FindIndex(lineType);
            }
        }

        /// <summary>
        /// 線分の太さ設定
        /// </summary>
        /// <param name="args"></param>
        private void setLineThickness(List<Token> args)
        {
            if (0 < args.Count) {
                string thickness = ylib.stripBracketString(args[0].mValue, '"');
                mEntityData.mPara.mThickness = ylib.doubleParse(thickness);
            }
        }

        /// <summary>
        /// 文字パラメータの設定
        /// setTextPara(size,rotate,HorizontalAlignment,VerticalAlignment,LinePitchRate,FontStyle,FontWeight)
        /// size = 1～96                                     文字サイズ
        /// rotate = 0～2π                                  文字列回転角
        /// HorizontalAlignment = "Left"/"Center"/"Right"    水平アライメント
        /// VerticalAlignment = "Top"/"Center"/"Bottom       垂直アライメント
        /// LinePitchRate = 1～                              行ピッチ比率(文字サイズに対して)
        /// FontStyle = "Normal"/"Oblique"/"Italic"          文字スタイル
        /// FontWeight = "Normal!/"Thin"/"Bold"              文字太さ
        /// </summary>
        /// <param name="args"></param>
        private void setTextPara(List<Token> args)
        {
            if (0 < args.Count && args[0].mValue != "")
                mEntityData.mPara.mTextSize = ylib.doubleParse(ylib.stripBracketString(args[0].mValue, '"'));
            if (1 < args.Count && args[1].mValue != "")
                mEntityData.mPara.mTextRotate = ylib.doubleParse(ylib.stripBracketString(args[1].mValue, '"'));
            if (2 < args.Count && args[2].mValue != "") {
                string ha = ylib.stripBracketString(args[2].mValue, '"').ToUpper();
                mEntityData.mPara.mHa = ha == "RIGHT" ? HorizontalAlignment.Right :
                    ha == "CENTER" ? HorizontalAlignment.Center : HorizontalAlignment.Left;
            }
            if (3 < args.Count && args[3].mValue != "") {
                string va = ylib.stripBracketString(args[3].mValue, '"').ToUpper();
                mEntityData.mPara.mVa = va == "BOTTOM" ? VerticalAlignment.Bottom :
                    va == "CENTER" ? VerticalAlignment.Center : VerticalAlignment.Top;
            }
            if (4 < args.Count && args[4].mValue != "")
                mEntityData.mPara.mLinePitchRate = ylib.doubleParse(ylib.stripBracketString(args[4].mValue, '"'));
            if (5 < args.Count && args[5].mValue != "")
                mEntityData.mPara.mFontStyle = args[5].mValue.ToUpper() == "OBLIQUE" ? FontStyles.Oblique :
                    args[5].mValue.ToUpper() == "ITALIC" ? FontStyles.Italic : FontStyles.Normal;
            if (6 < args.Count && args[6].mValue != "")
                mEntityData.mPara.mFontWeight = args[6].mValue.ToUpper() == "BOLD" ? FontWeights.Bold :
                    args[6].mValue.ToUpper() == "THIN" ? FontWeights.Thin : FontWeights.Normal;
        }

        /// <summary>
        /// 線分の作成
        /// line(sx,sy,ex,ey),line(sp[],ep[]),line(plist[,])
        /// </summary>
        /// <param name="args"></param>
        private void line(List<Token> args)
        {
            List<PointD> plist = args2PointList(args);
            if (plist != null && 1 < plist.Count) {
                mEntityData.addLine(plist[0], plist[1]);
            }
        }

        /// <summary>
        /// 円弧の作成
        /// arc(cp[],r,sa,ea)/arc(x0,y0,x1,y1,x2,y2)/arc(p0[],p1[],p2[])/arc(plist[,])
        /// </summary>
        /// <param name="args"></param>
        private void arc(List<Token> args)
        {
            if (1 < args.Count && mVar.getArrayOder(args[0]) == 1 && mVar.getArrayOder(args[1]) == 0) {
                //  arc(cp[],r[sa[.ea]])
                PointD cp = null;
                double r = 1, sa = 0, ea = Math.PI * 2;
                List<double> cpList = mVar.cnvListDouble(args[0]);
                if (1 < cpList.Count) {
                    cp = new PointD(cpList[0], cpList[1]);
                    r = ylib.doubleParse(args[1].mValue);
                    if (2 < args.Count && mVar.getArrayOder(args[2]) == 0)
                        sa = ylib.doubleParse(args[2].mValue);
                    if (3 < args.Count && mVar.getArrayOder(args[3]) == 0)
                        ea = ylib.doubleParse(args[3].mValue);
                    ArcD arc = new ArcD(cp, r, sa, ea);
                    mEntityData.addArc(arc);
                }
            } else {
                //  三点円弧
                List<PointD> plist = args2PointList(args);
                if (plist != null && 2 < plist.Count) {
                    ArcD arc = new ArcD(plist[0], plist[1], plist[2]);
                    mEntityData.addArc(arc);
                }
            }
        }

        /// <summary>
        /// 円の作成
        /// circle(cp[],r)/circle(x0,y0,x1,y1,x2,y2)/circle(p0[],p1[],p2[])/circle(plist[,])
        /// </summary>
        /// <param name="args"></param>
        private void circle(List<Token> args)
        {
            if (1 < args.Count && mVar.getArrayOder(args[0]) == 1 && mVar.getArrayOder(args[1]) == 0) {
                //  arc(cp[],r[sa[.ea]])
                PointD cp = null;
                double r = 1, sa = 0, ea = Math.PI * 2;
                List<double> cpList = mVar.cnvListDouble(args[0]);
                if (1 < cpList.Count) {
                    cp = new PointD(cpList[0], cpList[1]);
                    r = ylib.doubleParse(args[1].mValue);
                    ArcD arc = new ArcD(cp, r, sa, ea);
                    mEntityData.addArc(arc);
                }
            } else {
                //  三点円弧
                List<PointD> plist = args2PointList(args);
                if (plist != null && 2 < plist.Count) {
                    ArcD arc = new ArcD(plist[0], plist[1], plist[2]);
                    arc.mSa = 0;
                    arc.mEa = Math.PI * 2;
                    mEntityData.addArc(arc);
                }
            }
        }

        /// <summary>
        /// 楕円弧の作成
        /// ellispe(cp[],rx,ry,rotate,sa,ea)
        /// </summary>
        /// <param name="args"></param>
        private void ellipse(List<Token> args)
        {
            List<PointD> plist = getPointListFromArgs(args);
            List<double> dlist = getDoubleListFromArgs(args);
            EllipseD ellipse = new EllipseD();
            if (0 < plist.Count)
                ellipse.mCp = plist[0];
            if (0 < dlist.Count)
                ellipse.mRx = dlist[0];
            if (1 < dlist.Count)
                ellipse.mRy = dlist[1];
            if (2 < dlist.Count)
                ellipse.mRotate = dlist[2];
            if (3 < dlist.Count)
                ellipse.mSa = dlist[3];
            if (5 < dlist.Count)
                ellipse.mEa = dlist[4];
            mEntityData.addEllipse(ellipse);
        }

        /// <summary>
        /// 楕円の作成(2点で指定された矩形領域)
        /// oval(plist[,])/oval(p0[],p1[]...)/oval(x0,y0,x1,y1...)
        /// </summary>
        /// <param name="args"></param>
        private void oval(List<Token> args)
        {
            List<PointD> plist = args2PointList(args);
            if (1 < plist.Count) {
                EllipseD ellipse = new EllipseD(plist[0], plist[1]);
                mEntityData.addEllipse(ellipse);
            }
        }

        /// <summary>
        /// ポリラインの作成
        /// polyline(plist[,])/polyline(p0[],p1[],p2[]....)/polyline(x0.y0,x1,y1...)
        /// </summary>
        /// <param name="args"></param>
        private void polyline(List<Token> args)
        {
            List<PointD> plist = args2PointList(args);
            if (plist != null && 1 < plist.Count) {
                mEntityData.addPolyline(plist);
            }
        }

        /// <summary>
        /// ポリゴンの作成
        /// polygon(plist[,])/polygon(p9[],p1[],p2[]....)/polygon(x0.y0,x1,y1...)
        /// </summary>
        /// <param name="args"></param>
        private void polygon(List<Token> args)
        {
            List<PointD> plist = args2PointList(args);
            if (plist != null && 1 < plist.Count) {
                mEntityData.addPolygon(plist);
            }
        }

        /// <summary>
        /// 文字列の作成
        /// text(pos[],text,size,rotate,horizontal,vertical)
        /// text(x,y,text,size,rotate,horizontal,vertical)
        /// </summary>
        /// <param name="args"></param>
        private void text(List<Token> args)
        {
            PointD pos = new PointD();
            string text = "";
            int n = 0;
            if (n < args.Count && mVar.getArrayOder(args[n]) == 1) {
                List<double> spList = mVar.cnvListDouble(args[n]);
                if (1 < spList.Count)
                    pos = new PointD(spList[0], spList[1]);
                n++;
            } else if (n + 1 < args.Count && mVar.getArrayOder(args[n]) == 0 && mVar.getArrayOder(args[n + 1]) == 0) {
                pos = new PointD(ylib.doubleParse(args[n].mValue), ylib.doubleParse(args[n + 1].mValue));
                n += 2;
            } else
                return;
            if (n < args.Count) {
                text = ylib.stripBracketString(mVar.getVariable(args[n++]).mValue, '"');
                if (text != "") {
                    TextD textEnt = new TextD(text, pos);
                    textEnt.mTextSize = mEntityData.mPara.mTextSize;
                    textEnt.mRotate = mEntityData.mPara.mTextRotate;
                    textEnt.mHa = mEntityData.mPara.mHa;
                    textEnt.mVa = mEntityData.mPara.mVa;
                    textEnt.mLinePitchRate = mEntityData.mPara.mLinePitchRate;
                    textEnt.mFontStyle = mEntityData.mPara.mFontStyle;
                    textEnt.mFontWeight = mEntityData.mPara.mFontWeight;
                    if (n < args.Count && args[n].mValue != "")
                        textEnt.mTextSize = ylib.doubleParse(ylib.stripBracketString(args[n].mValue, '"'));
                    n++;
                    if (n < args.Count && args[n].mValue != "")
                        textEnt.mRotate = ylib.doubleParse(ylib.stripBracketString(args[n].mValue, '"'));
                    n++;
                    if (n < args.Count && args[n].mValue != "") {
                        string ha = ylib.stripBracketString(args[n].mValue, '"').ToUpper();
                        textEnt.mHa = ha == "RIGHT" ? HorizontalAlignment.Right : ha == "CENTER" ? HorizontalAlignment.Center : HorizontalAlignment.Left;
                    }
                    n++;
                    if (n < args.Count && args[n].mValue != "") {
                        string va = ylib.stripBracketString(args[n].mValue, '"').ToUpper();
                        textEnt.mVa = va == "BOTTOM" ? VerticalAlignment.Bottom : va == "CENTER" ? VerticalAlignment.Center : VerticalAlignment.Top;
                    }
                    n++;
                    mEntityData.addText(textEnt);
                }
            }

        }

        /// <summary>
        /// 矢印の作成
        /// arrow(plist[,],arrowSize)/arrow(p0[],p1[],p2[]...,arrowSize)/arrow(x0,y0,x1,y1...,arrowSize)
        /// </summary>
        /// <param name="args"></param>
        private void arrow(List<Token> args)
        {
            List<PointD> plist = args2PointList(args);
            List<double> dlist = getDoubleListFromArgs(args);
            if (plist != null && 1 < plist.Count) {
                if (0 < dlist.Count && dlist.Count % 2 == 1)
                    mEntityData.addArrow(plist, dlist.Last());
                else
                    mEntityData.addArrow(plist);
            }
        }

        /// <summary>
        /// ラベルの作成
        /// </summary>
        /// <param name="args"></param>
        private void label(List<Token> args)
        {
            List<PointD> plist = getPointListFromArgs(args);
            List<string> tlist = getStringListFromArgs(args);
            List<double> dlist = getDoubleListFromArgs(args);
            if (1 < plist.Count && 0 < tlist.Count) {
                if (0 < dlist.Count && dlist.Count % 2 == 1)
                    mEntityData.addLabel(plist, tlist[0], dlist.Last());
                else
                mEntityData.addLabel(plist, tlist[0]);
            }
        }

        private void dimension(List<Token> args)
        {

        }

        /// <summary>
        /// 引数からPointDリストを作成(plist[,]/p0[],p1[].../x0,y0,x1,y1...  → List<PointD>)</PointD>
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private List<PointD> args2PointList(List<Token> args)
        {
            List<PointD> pointList = new List<PointD>();
            if (0 < args.Count && mVar.getArrayOder(args[0]) == 2) {
                //  plist[,] → List<PointD>
                double[,] plist = mVar.cnvArrayDouble2(args[0]);
                if (1 < plist.GetLength(1)) {
                    for (int i = 0; i < plist.GetLength(0); i++) {
                        PointD p = new PointD(plist[i, 0], plist[i, 1]);
                        pointList.Add(p);
                    }
                }
            } else if (0 < args.Count && mVar.getArrayOder(args[0]) == 1) {
                //  p0[],p1[]... → List<PointD>
                for (int i = 0; i < args.Count; i++) {
                    List<double> spList = mVar.cnvListDouble(args[i]);
                    if (1 < spList.Count) {
                        PointD p = new PointD(spList[0], spList[1]);
                        pointList.Add(p);
                    }
                }
            } else if (1 < args.Count && mVar.getArrayOder(args[0]) == 0) {
                //  x0,y0,x1,y1... → List<PointD>
                for (int i = 0; i < args.Count - 1; i += 2) {
                    if (mVar.getArrayOder(args[i]) == 0 && mVar.getArrayOder(args[i+1]) == 0) {
                        if (mVar.getVariable(args[i]).mType == TokenType.LITERAL && mVar.getVariable(args[i + 1]).mType == TokenType.LITERAL) {
                            PointD p = new PointD(ylib.doubleParse(args[i].mValue), ylib.doubleParse(args[i + 1].mValue));
                            pointList.Add(p);
                        }
                    }
                }
            }
            return pointList;
        }

        /// <summary>
        /// 引数からdoubleリストを取得(配列を除く)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private List<double> getDoubleListFromArgs(List<Token> args)
        {
            List<double> doubleList = new List<double>();
            //  x0,x1,x2....  →  List<double>
            for (int i = 0; i < args.Count; i++) {
                if (mVar.getArrayOder(args[i]) == 0) {
                    if (mVar.getVariable(args[i]).mType == TokenType.LITERAL)
                        doubleList.Add(ylib.doubleParse(args[i].mValue));
                } 
            }
            return doubleList;
        }

        /// <summary>
        /// 引数からstringリストを取得
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private List<string> getStringListFromArgs(List<Token> args)
        {
            List<string> stringList = new List<string>();
            //  x0,x1,x2....  →  List<double>
            for (int i = 0; i < args.Count; i++) {
                if (mVar.getArrayOder(args[i]) == 0) {
                    if (mVar.getVariable(args[i]).mType == TokenType.STRING)
                        stringList.Add(ylib.stripBracketString(mVar.getVariable(args[i]).mValue, '"'));
                }
            }
            return stringList;
        }


        /// <summary>
        /// 引数からPointDリストを取得(配列のみ)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private List<PointD> getPointListFromArgs(List<Token> args)
        {
            List<PointD> pointList = new List<PointD>();
            for (int i = 0; i < args.Count; i++) {
                if (0 < args.Count && mVar.getArrayOder(args[i]) == 2) {
                    //  plist[,] → List<PointD>
                    double[,] plist = mVar.cnvArrayDouble2(args[i]);
                    if (1 < plist.GetLength(1)) {
                        for (int j = 0; j < plist.GetLength(0); j++) {
                            PointD p = new PointD(plist[j, 0], plist[j, 1]);
                            pointList.Add(p);
                        }
                    }
                } else if (mVar.getArrayOder(args[i]) == 1) {
                    //  p0[],p1[]... → List<PointD>
                    List<double> spList = mVar.cnvListDouble(args[i]);
                    if (1 < spList.Count) {
                        PointD p = new PointD(spList[0], spList[1]);
                        pointList.Add(p);
                    }
                } else if (i < args.Count - 1 && mVar.getArrayOder(args[i]) == 0 && mVar.getArrayOder(args[i+1]) == 0) {
                    if (mVar.getVariable(args[i]).mType == TokenType.LITERAL && mVar.getVariable(args[i + 1]).mType == TokenType.LITERAL) {
                        //  x0,y0,x1,y1... → List<PointD>
                        PointD p = new PointD(ylib.doubleParse(args[i].mValue), ylib.doubleParse(args[i + 1].mValue));
                        pointList.Add(p);
                        i++;
                    }
                }
            }
            return pointList;
        }
    }

}
