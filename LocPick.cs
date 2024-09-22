using CoreLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CadApp
{
    public class LocPick
    {
        //  アプリキーによるロケイトメニュー
        private List<string> mLocMenu = new List<string>() {
            "座標入力", "相対座標入力"
        };
        //  Ctrl + マウス右ピックによるロケイトメニュー
        private List<string> mLocSelectMenu = new List<string>() {
            "端点・中間点", "3分割点", "4分割点", "5分割点", "6分割点", "8分割点", "16分割点",
            "垂点", "端点距離"
        };

        public List<PointD> mLocPos = new();                        //  マウス指定点
        public List<(int no, PointD pos)> mPickEnt = new();         //  ピックした要素リスト
        public EntityId mPickMask = EntityId.Non;
        public EntityData mEntityData;
        public Window mMainWindow;
        private YCalc ycalc = new YCalc();
        private YLib ylib = new YLib();

        public LocPick(Window mainWIndow, EntityData entityData)
        {
            mMainWindow = mainWIndow;
            mEntityData = entityData;
        }

        /// <summary>
        /// ピック要素に対しての自動ロケイト
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="picks">要素Noリスト</param>
        /// <returns>ロケイト位置</returns>
        public PointD autoLoc(PointD pickPos, List<int> picks)
        {
            PointD? wp = null;
            if (ylib.onControlKey()) {
                //  Ctrlキーでのメニュー表示で位置を選定
                wp = locSelect(pickPos, picks);
            } else {
                //  ピックされているときは位置を自動判断
                if (picks.Count == 1) {
                    //  ピックされているときは位置を自動判断
                    wp = autoLoc(pickPos, picks[0]);
                } else if (2 <= picks.Count) {
                    //  2要素の時は交点位置
                    wp = mEntityData.intersection(picks[0], picks[1], pickPos);
                    if (wp == null)
                        wp = autoLoc(pickPos, picks[0]);
                    if (wp == null)
                        wp = autoLoc(pickPos, picks[1]);
                }
            }
            if (wp != null)
                mLocPos.Add(wp);
            return wp;
        }

        /// <summary>
        /// ピックした要素上の点を取得(端点、中点,1/4点から一番近い点)
        /// </summary>
        /// <param name="p">ピック位置</param>
        /// <param name="entNo">要素No</param>
        /// <returns>座標(World座標)</returns>
        private PointD autoLoc(PointD p, int entNo = -1)
        {
            if (entNo < 0)
                return p;
            PointD pos = new PointD(p);
            Entity ent = mEntityData.mEntityList[entNo];
            switch (ent.mEntityId) {
                case EntityId.Point:
                    PointEntity point = (PointEntity)ent;
                    pos = point.mPoint;
                    break;
                case EntityId.Line:
                    LineEntity line = (LineEntity)ent;
                    pos = line.mLine.nearPoint(p, 4);
                    break;
                case EntityId.Polyline:
                    PolylineEntity polyline = (PolylineEntity)ent;
                    pos = polyline.mPolyline.nearPoint(p, 4);
                    //LineD pl = polyline.mPolyline.nearLine(p);
                    //pos = pl.nearPoint(p, 4);
                    break;
                case EntityId.Polygon:
                    PolygonEntity polygon = (PolygonEntity)ent;
                    pos = polygon.mPolygon.nearPoint(p, 4);
                    //LineD pgl = polygon.mPolygon.nearLine(p);
                    //pos = pgl.nearPoint(p, 4);
                    break;
                case EntityId.Arc:
                    ArcEntity arc = (ArcEntity)ent;
                    pos = arc.mArc.nearPoints(p, Math.PI * 2 <= arc.mArc.mOpenAngle ? 8 : 4);
                    break;
                case EntityId.Ellipse:
                    EllipseEntity ellipse = (EllipseEntity)ent;
                    pos = ellipse.mEllipse.nearPoints(p, Math.PI * 2 <= ellipse.mEllipse.mOpenAngle ? 8 : 4);
                    break;
                case EntityId.Text:
                    TextEntity text = (TextEntity)ent;
                    pos = text.mText.nearPeakPoint(p);
                    break;
                case EntityId.Parts:
                    PartsEntity parts = (PartsEntity)ent;
                    pos = parts.mParts.nearPoint(p, 4);
                    break;
                case EntityId.Image:
                    ImageEntity image = (ImageEntity)ent;
                    pos = image.mDispPosSize.nearPoint(p, 4);
                    break;
            }
            return pos;
        }

        /// <summary>
        /// 複数ピックした時の要素選択メニュー
        /// </summary>
        /// <param name="picks">ピックリスト</param>
        /// <returns>選択ピック要素番号</returns>
        public int pickSelect(List<int> picks, OPEMODE locMode)
        {
            if (picks.Count == 1)
                return picks[0];
            List<int> sqeezePicks = picks.Distinct().ToList();
            List<string> menu = new List<string>();
            for (int i = 0; i < sqeezePicks.Count; i++) {
                Entity ent = mEntityData.mEntityList[sqeezePicks[i]];
                menu.Add(ent.getSummary());
            }
            if (locMode == OPEMODE.loc)
                menu.Add("交点");
            MenuDialog dlg = new MenuDialog();
            dlg.mMainWindow = mMainWindow;
            dlg.mHorizontalAliment = 1;
            dlg.mVerticalAliment = 1;
            dlg.mOneClick = true;
            dlg.mMenuList = menu;
            dlg.ShowDialog();
            if (dlg.mResultMenu == "")
                return -1;
            else if (dlg.mResultMenu == "交点")
                return -2;
            else
                return ylib.string2int(dlg.mResultMenu);
        }

        /// <summary>
        /// ピックした要素の分割点を求めるメニュー
        /// </summary>
        /// <param name="pos">ピック点</param>
        /// <param name="picks">ピック要素リスト</param>
        /// <returns>ロケイト点</returns>
        private PointD locSelect(PointD pos, List<int> picks)
        {
            if (picks.Count == 0) return pos;
            List<string> locMenu = new();
            locMenu.AddRange(mLocSelectMenu);
            Entity ent = mEntityData.mEntityList[picks[0]];
            if (picks.Count == 1) {
                if (ent.mEntityId == EntityId.Arc || ent.mEntityId == EntityId.Ellipse) {
                    locMenu.Add("中心点");
                    locMenu.Add("頂点");
                    locMenu.Add("接点");
                } else if (ent.mEntityId == EntityId.Parts) {
                    locMenu.Add("中心点");
                }
            } else if (1 < picks.Count) {
                locMenu.Add("交点");
            }
            MenuDialog dlg = new MenuDialog();
            dlg.Title = "ロケイトメニュー";
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mMenuList = locMenu;
            dlg.ShowDialog();
            if (0 < dlg.mResultMenu.Length) {
                pos = getLocSelectPos(dlg.mResultMenu, pos, picks);
            }

            return pos;
        }

        /// <summary>
        /// メニュー選択されたロケイト点を求める
        /// </summary>
        /// <param name="selectMenu">選択メニュー</param>
        /// <param name="pos">ピック点</param>
        /// <param name="picks">ピック要素番号リスト</param>
        /// <returns>ロケイト点</returns>
        private PointD getLocSelectPos(string selectMenu, PointD pos, List<int> picks)
        {
            Entity ent = mEntityData.mEntityList[picks[0]];
            PointD lastLoc = pos;
            if (0 < mLocPos.Count)
                lastLoc = mLocPos[mLocPos.Count - 1];
            List<PointD> plist = new List<PointD>();
            switch (selectMenu) {
                case "端点・中間点": pos = ent.dividePos(2, pos); break;
                case "3分割点": pos = ent.dividePos(3, pos); break;
                case "4分割点": pos = ent.dividePos(4, pos); break;
                case "5分割点": pos = ent.dividePos(5, pos); break;
                case "6分割点": pos = ent.dividePos(6, pos); break;
                case "8分割点": pos = ent.dividePos(8, pos); break;
                case "16分割点": pos = ent.dividePos(16, pos); break;
                case "垂点":
                    pos = ent.onPoint(lastLoc);
                    break;
                case "接点":
                    if (ent.mEntityId == EntityId.Arc) {
                        ArcEntity arcEnt = (ArcEntity)ent;
                        plist = arcEnt.mArc.tangentPoint(lastLoc);
                    } else if (ent.mEntityId == EntityId.Ellipse) {
                        EllipseEntity ellipseEnt = (EllipseEntity)ent;
                        plist = ellipseEnt.mEllipse.tangentPoint(lastLoc);
                    }
                    if (plist != null && 0 < plist.Count)
                        pos = plist.MinBy(p => p.length(pos));  //  最短位置
                    break;
                case "頂点":
                    if (ent.mEntityId == EntityId.Arc) {
                        ArcEntity arcEnt = (ArcEntity)ent;
                        plist = arcEnt.mArc.toPeakList();
                    } else if (ent.mEntityId == EntityId.Ellipse) {
                        EllipseEntity ellipseEnt = (EllipseEntity)ent;
                        plist = ellipseEnt.mEllipse.toPeakList();
                    }
                    if (plist != null && 0 < plist.Count)
                        pos = plist.MinBy(p => p.length(pos));  //  最短位置
                    break;
                case "端点距離":
                    pos = getEndPoint(selectMenu, ent, pos);
                    break;
                case "中心点":
                    if (ent.mEntityId == EntityId.Arc) {
                        ArcEntity arcEnt = (ArcEntity)ent;
                        pos = arcEnt.mArc.mCp;
                    } else if (ent.mEntityId == EntityId.Ellipse) {
                        EllipseEntity ellipseEnt = (EllipseEntity)ent;
                        pos = ellipseEnt.mEllipse.mCp;
                    } else if (ent.mEntityId == EntityId.Parts) {
                        pos = ent.mArea.getCenter();
                    }
                    break;
                case "交点":
                    if (2 <= picks.Count) {
                        plist = mEntityData.intersection(picks[0], picks[1]);
                        pos = plist.MinBy(p => p.length(pos));  //  最短位置
                    }
                    break;
            }
            return pos;
        }

        /// <summary>
        /// 端点から距離指定の座標
        /// 負数だと延長線上の点
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="ent">要素</param>
        /// <param name="pos">ピック店</param>
        /// <returns>座標</returns>
        private PointD getEndPoint(string title, Entity ent, PointD pos)
        {
            double dis = getInputVal(title);
            if (!double.IsNaN(dis)) {
                LineD l = ent.getLine(pos);
                if (!l.isNaN()) {
                    if (l.ps.length(pos) > l.pe.length(pos))
                        l.inverse();
                    l.setLength(dis);
                    pos = l.pe;
                } else if (ent.mEntityId == EntityId.Arc) {
                    ArcD arc = ((ArcEntity)ent).mArc;
                    double ang = dis / arc.mR;
                    if (pos.length(arc.startPoint()) < pos.length(arc.endPoint()))
                        pos = arc.getPoint(arc.mSa + ang);
                    else
                        pos = arc.getPoint(arc.mEa - ang);
                }
            }
            return pos;
        }

        /// <summary>
        /// 数値入力ダイヤログ
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <returns>数値(cancelはNaN)</returns>
        private double getInputVal(string title)
        {
            InputBox dlg = new InputBox();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = title;
            if (dlg.ShowDialog() == true)
                return ycalc.expression(dlg.mEditText);
            else
                return double.NaN;
        }

        /// <summary>
        /// ロケイトメニューの表示(Windowsメニューキー)
        /// </summary>
        public bool locMenu(OPEMODE locMode, OPERATION operation, PointD prevPosition)
        {
            if (locMode == OPEMODE.loc) {
                List<string> locMenu = new List<string>();
                locMenu.AddRange(mLocMenu);
                if (operation == OPERATION.translate || operation == OPERATION.copyTranslate) {
                    locMenu.Add("平行距離,繰返し数");
                    locMenu.Add("スライド距離");
                } else if (operation == OPERATION.offset || operation == OPERATION.copyOffset) {
                    locMenu.Add("平行距離,繰返し数");
                } else if (operation == OPERATION.createCircle || operation == OPERATION.createTangentCircle) {
                    locMenu.Add("半径");
                } else if (operation == OPERATION.rotate || operation == OPERATION.copyRotate) {
                    locMenu.Add("回転角,繰返し数");
                } else if (operation == OPERATION.scale) {
                    locMenu.Add("スケール");
                }
                MenuDialog dlg = new MenuDialog();
                dlg.Title = "ロケイトメニュー";
                dlg.Owner = mMainWindow;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlg.mMenuList = locMenu;
                dlg.ShowDialog();
                if (0 < dlg.mResultMenu.Length)
                    return getInputLoc(dlg.mResultMenu, operation, prevPosition);
            }
            return false;
        }

        /// <summary>
        /// ロケイトメニューの処理(Windowsメニューキー)
        /// </summary>
        /// <param name="title"></param>
        private bool getInputLoc(string title, OPERATION operation, PointD prevPosition)
        {
            InputBox dlg = new InputBox();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = title;
            if (dlg.ShowDialog() == true) {
                string[] valstr;
                double val;
                int repeat = 1;
                PointD wp = new PointD();
                PointD p1, p2;
                LineD line;
                Entity entity;
                PointD lastLoc = new PointD(0, 0);
                if (0 < mLocPos.Count)
                    lastLoc = mLocPos[mLocPos.Count - 1];
                switch (title) {
                    case "座標入力":
                        //  xxx,yyy で入力
                        valstr = dlg.mEditText.Split(',');
                        if (1 < valstr.Length) {
                            wp = new PointD(ycalc.expression(valstr[0]), ycalc.expression(valstr[1]));
                            mLocPos.Add(wp);
                            return true;
                        }
                        break;
                    case "相対座標入力":
                        //  xxx,yyy で入力
                        valstr = dlg.mEditText.Split(',');
                        if (1 < valstr.Length && 0 < mLocPos.Count) {
                            wp = new PointD(ycalc.expression(valstr[0]), ycalc.expression(valstr[1]));
                            if (2 < valstr.Length)
                                repeat = (int)ycalc.expression(valstr[2]);
                            for (int i = 0; i < repeat; i++)
                                mLocPos.Add(wp + mLocPos.Last());
                            return true;
                        }
                        break;
                    case "平行距離,繰返し数":
                        //  移動またはコピー移動の時のみ
                        if (0 == mLocPos.Count)     //  方向を決めるロケイトが必要
                            break;
                        valstr = dlg.mEditText.Split(',');
                        val = ycalc.expression(valstr[0]);
                        if (1 < valstr.Length)
                            repeat = (int)ycalc.expression(valstr[1]);
                        entity = mEntityData.mEntityList[mPickEnt[mPickEnt.Count - 1].no];
                        if (entity.mEntityId == EntityId.Arc) {
                            ArcEntity arcEnt = (ArcEntity)entity;
                            LineD la = new LineD(arcEnt.mArc.mCp, lastLoc);
                            for (int i = 1; i < repeat + 1; i++) {
                                la.setLength(la.length() + val);
                                wp = la.pe.toCopy();
                                mLocPos.Add(wp);
                            }
                        } else if (entity.mEntityId == EntityId.Ellipse) {
                            EllipseEntity ellipseEnt = (EllipseEntity)entity;
                            LineD la = new LineD(ellipseEnt.mEllipse.mCp, lastLoc);
                            for (int i = 1; i < repeat + 1; i++) {
                                la.setLength(la.length() + val);
                                wp = la.pe.toCopy();
                                mLocPos.Add(wp);
                            }
                        } else {
                            line = entity.getLine(mPickEnt[mPickEnt.Count - 1].pos);
                            if (!line.isNaN()) {
                                for (int i = 1; i < repeat + 1; i++) {
                                    wp = line.offset(lastLoc, val * i);
                                    mLocPos.Add(wp);
                                }
                            }
                        }
                        if (0 < mLocPos.Count) {
                            return true;
                        }
                        break;
                    case "スライド距離":
                        //  移動またはコピー移動の時のみ
                        if (0 == mLocPos.Count)     //  方向を決めるロケイトが必要
                            break;
                        valstr = dlg.mEditText.Split(',');
                        val = ycalc.expression(valstr[0]);
                        entity = mEntityData.mEntityList[mPickEnt[mPickEnt.Count - 1].no];
                        line = entity.getLine(mPickEnt[mPickEnt.Count - 1].pos);
                        if (!line.isNaN()) {
                            LineD directLine = new LineD(line.centerPoint(), line.intersection(lastLoc));
                            wp = lastLoc + directLine.getVectorAngle(0, val);
                            mLocPos.Add(wp);
                            return true;
                        }
                        break;
                    case "半径":
                        valstr = dlg.mEditText.Split(',');
                        val = ycalc.expression(valstr[0]);
                        if (operation == OPERATION.createCircle) {
                            //  円の作成
                            wp = lastLoc + new PointD(val, 0);
                        } else if (operation == OPERATION.createTangentCircle && 2 <= mPickEnt.Count) {
                            //  接円の作成
                            mLocPos.Add(prevPosition);
                            ArcD arc = mEntityData.tangentCircle(mPickEnt, mLocPos, val);
                            wp = arc.mCp;
                            mLocPos.RemoveAt(mLocPos.Count - 1);
                        } else {
                            break;
                        }
                        if (!wp.isNaN()) {
                            mLocPos.Add(wp);
                        }
                        break;
                    case "回転角,繰返し数":
                        valstr = dlg.mEditText.Split(',');
                        val = ylib.D2R(ycalc.expression(valstr[0]));
                        if (1 < valstr.Length)
                            repeat = (int)ycalc.expression(valstr[1]);
                        PointD vec = new PointD(1, 0);
                        if (mLocPos.Count < 2)
                            mLocPos.Add(mLocPos[0] + vec);
                        vec = mLocPos[1] - mLocPos[0];
                        for (int i = 1; i < repeat + 1; i++) {
                            vec.rotate(val);
                            mLocPos.Add(mLocPos[0] + vec);
                        }
                        return true;
                    case "スケール":
                        valstr = dlg.mEditText.Split(',');
                        val = ycalc.expression(valstr[0]);
                        if (1 == mLocPos.Count) {
                            wp = lastLoc + new PointD(1, 0);
                            mLocPos.Add(wp);
                        }
                        if (2 == mLocPos.Count) {
                            double dis = mLocPos[0].length(mLocPos[1]);
                            line = new LineD(mLocPos[0], mLocPos[1]);
                            line.setLength(dis * val);
                            mLocPos.Add(line.pe);
                            return true;
                        }
                        break;
                }
            }
            return false;
        }

        /// <summary>
        /// ピック要素Noの追加
        /// すでに登録されている場合は削除する(アンピック)
        /// </summary>
        /// <param name="pick">ピックデータ</param>
        public void addPick((int no, PointD pos) pick, bool unpick = false)
        {
            if (unpick) {
                int index = mPickEnt.FindIndex(p => p.no == pick.no);
                if (0 <= index) {
                    mPickEnt.RemoveAt(index);
                } else {
                    mPickEnt.Add(pick);
                }
            } else
                mPickEnt.Add(pick);
        }

        /// <summary>
        /// ピックした要素Noを求める
        /// </summary>
        /// <param name="pickPos">ピック位置(World座標)</param>
        /// <returns>要素Noリスト</returns>
        public List<int> getPickNo(PointD pickPos, double xd)
        {
            //double xd = mDataDrawing.screen2worldXlength(mPickBoxSize);    //  ピック領域
            Box b = new Box(pickPos, xd);
            return getPickNo(b);
        }

        /// <summary>
        /// ピックした要素Noを求める
        /// </summary>
        /// <param name="b">ピック領域(World座標)</param>
        /// <returns>要素Noリスト</returns>
        public List<int> getPickNo(Box b)
        {
            return mEntityData.findIndex(b, mPickMask);
        }

        /// <summary>
        /// グループピック(ピックした要素と同じグループ要素を登録)
        /// </summary>
        /// <param name="picks">ピック要素</param>
        /// <param name="pos">ピック位置</param>
        public void getGroup(List<int> picks, PointD pos)
        {
            List<(int no, PointD pos)> groupList = new List<(int no, PointD pos)>();
            for (int i = 0; i < picks.Count; i++) {
                int group = mEntityData.mEntityList[picks[i]].mGroup;
                if (0 < group) {
                    for (int j = 0; j < mEntityData.mEntityList.Count; j++) {
                        if (group == mEntityData.mEntityList[j].mGroup &&
                            !mEntityData.mEntityList[j].mRemove)
                            groupList.Add((j, pos));
                    }
                }
            }
            mPickEnt.AddRange(groupList);
        }

        /// <summary>
        /// グループピックのメニューダイヤログを開く
        /// </summary>
        /// <param name="pos">ピック位置</param>
        /// <returns></returns>
        public bool groupSelectPick(PointD pos)
        {
            MenuDialog dlg = new MenuDialog();
            dlg.Title = "グループピックメニュー";
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mMenuList = mEntityData.mGroup.getGroupNameList();
            dlg.ShowDialog();
            if (0 < dlg.mResultMenu.Length) {
                int groupNo = mEntityData.mGroup.getGroupNo(dlg.mResultMenu);
                if (0 < groupNo) {
                    List<(int no, PointD pos)> groupList = new List<(int no, PointD pos)>();
                    for (int j = 0; j < mEntityData.mEntityList.Count; j++) {
                        if (groupNo == mEntityData.mEntityList[j].mGroup &&
                            !mEntityData.mEntityList[j].mRemove)
                            groupList.Add((j, pos));
                    }
                    mPickEnt.AddRange(groupList);
                    return true;
                }
            }
            return false;
        }
    }
}
