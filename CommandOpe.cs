using CoreLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CadApp
{
    class CommandOpe
    {
        public EntityData mEntityData;

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
        public double mGridSize = 1.0;                     //  マウス座標の丸め値

        private KeyCommand mKeyCommand = new KeyCommand();

        public Window mWindow;

        private YLib ylib = new YLib();

        public CommandOpe(EntityData entityData) {
            mEntityData = entityData;
        }

        /// <summary>
        /// 要素作成データ追加
        /// </summary>
        /// <param name="points">ロケイト点はいれつ</param>
        /// <param name="operation">操作</param>
        /// <returns></returns>
        public bool createData(List<PointD> points, OPERATION operation)
        {
            mEntityData.mColor = mCreateColor;
            mEntityData.mThickness = mEntSize;
            mEntityData.mType = mLineType;
            if (operation == OPERATION.createPoint && points.Count == 1) {
                mEntityData.mType = mPointType;
                mEntityData.mThickness = mPointSize;
                mEntityData.addPoint(points[0]);
            } else if (operation == OPERATION.createLine && points.Count == 2) {
                mEntityData.addLine(points[0], points[1]);
            } else if (operation == OPERATION.createRect && points.Count == 2) {
                mEntityData.addRect(points[0], points[1]);
            } else if (operation == OPERATION.createArc && points.Count == 3) {
                mEntityData.addArc(new ArcD(points[0], points[1], points[2]));
            } else if (operation == OPERATION.createCircle && points.Count == 2) {
                mEntityData.addArc(new ArcD(points[0], points[0].length(points[1])));
            } else if (operation == OPERATION.createText && points.Count == 1) {
                TextD text = new TextD(mTextString, points[0], mTextSize, mTextRotate, mHa, mVa);
                mEntityData.addText(text);
            } else if (operation == OPERATION.createPolyline) {
                if (1 < points.Count)
                    mEntityData.addPolyline(points);
            } else if (operation == OPERATION.createPolygon) {
                if (1 < points.Count)
                    mEntityData.addPolygon(points);
            } else {
                return false;
            }
            return true;
        }

        /// <summary>
        /// ピックした要素のデータ更新(移動,回転,ミラー)
        /// </summary>
        /// <param name="loc">ロケイト点リスト</param>
        /// <returns></returns>
        public bool changeData(List<PointD> loc, List<int> pickEnt, OPERATION operation)
        {
            if (loc.Count < 2) return false;
            if (operation == OPERATION.translate) {
                PointD vec = loc[0].vector(loc[1]);
                mEntityData.translate(pickEnt, vec);
            } else if (operation == OPERATION.rotate) {
                mEntityData.rotate(pickEnt, loc[0], loc[1]);
            } else if (operation == OPERATION.mirror) {
            }
            return true;
        }

        public bool keyCommand(string command)
        {
            mKeyCommand.setCommand(command);
            Entity entity = mKeyCommand.getEntity();
            if (entity != null) {
                mEntityData.mEntityList.Add(entity);
                mEntityData.updateData();
                return true;
            }
            return false;
        }

        /// <summary>
        /// ピックしたテキスト要素の文字列を変更する
        /// </summary>
        /// <returns></returns>
        public bool changeText(List<int> pickEnt)
        {
            foreach (int pickNo in pickEnt) {
                InputBox dlg = new InputBox();
                if (mEntityData.mEntityList[pickNo].mEntityId == EntityId.Text) {
                    TextEntity text = (TextEntity)mEntityData.mEntityList[pickNo];
                    dlg.mEditText = text.mText.mText;
                    if (dlg.ShowDialog() == true) {
                        text.mText.mText = dlg.mEditText;
                    }
                    dlg.Close();
                }
            }
            mEntityData.updateData();
            return true;
        }

        /// <summary>
        /// ピックした要素の属性変更
        /// </summary>
        /// <returns></returns>
        public bool changeProperty(List<int> pickEnt)
        {
            foreach (int pickNo in pickEnt) {
                EntProperty dlg = new EntProperty();
                dlg.Owner = mWindow;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlg.mEntityId = mEntityData.mEntityList[pickNo].mEntityId;
                dlg.mColor = mEntityData.mEntityList[pickNo].mColor;
                dlg.mType = mEntityData.mEntityList[pickNo].mType;
                dlg.mThickness = mEntityData.mEntityList[pickNo].mThickness;
                if (mEntityData.mEntityList[pickNo].mEntityId == EntityId.Text) {
                    TextEntity text = (TextEntity)mEntityData.mEntityList[pickNo];
                    dlg.mThickness = text.mText.mTextSize;
                    dlg.mHa = text.mText.mHa;
                    dlg.mVa = text.mText.mVa;
                    dlg.mTextRotate = text.mText.mRotate;
                }
                if (dlg.ShowDialog() == true) {
                    mEntityData.mEntityList[pickNo].mColor = dlg.mColor;
                    mEntityData.mEntityList[pickNo].mType = dlg.mType;
                    mEntityData.mEntityList[pickNo].mThickness = dlg.mThickness;
                    if (mEntityData.mEntityList[pickNo].mEntityId == EntityId.Text) {
                        TextEntity text = (TextEntity)mEntityData.mEntityList[pickNo];
                        text.mText.mTextSize = dlg.mThickness;
                        text.mText.mHa = dlg.mHa;
                        text.mText.mVa = dlg.mVa;
                        text.mText.mRotate = dlg.mTextRotate;
                    }
                }
                dlg.Close();
            }
            mEntityData.updateData();
            return true;
        }

        /// <summary>
        /// 要素の属性値の設定
        /// </summary>
        /// <returns></returns>
        public bool systemProperty()
        {
            SysProperty dlg = new SysProperty();
            dlg.Owner = mWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mColor = mCreateColor;
            dlg.mLineType = mLineType;
            dlg.mThickness = mEntSize;
            dlg.mPointType = mPointType;
            dlg.mPointSize = mPointSize;
            dlg.mTextSize = mTextSize;
            dlg.mHa = mHa;
            dlg.mVa = mVa;
            dlg.mGridSize = mGridSize;
            if (dlg.ShowDialog() == true) {
                mCreateColor = dlg.mColor;
                mLineType = dlg.mLineType;
                mEntSize = dlg.mThickness;
                mPointType = dlg.mPointType;
                mPointSize = dlg.mPointSize;
                mTextSize = dlg.mTextSize;
                mHa = dlg.mHa;
                mVa = dlg.mVa;
                mGridSize = dlg.mGridSize;
                dlg.Close();
                return true;
            } else {
                dlg.Close();
                return false;
            }
        }

        /// <summary>
        /// ピックした要素の情報表示
        /// </summary>
        public bool infoEntity(List<int> pickEnt)
        {
            foreach (int entNo in pickEnt) {
                MessageBox.Show(mEntityData.mEntityList[entNo].entityInfo(), "要素情報");
            }
            return true;
        }

        /// <summary>
        /// 図面ファイルを選択して読みだす
        /// </summary>
        public bool openFile()
        {
            List<string[]> filters = new List<string[]>() {
                    new string[] { "図面ファイル", "*.csv" },
                    new string[] { "すべてのファイル", "*.*"}
                };
            string filePath = ylib.fileOpenSelectDlg("データ読込", ".", filters);
            if (0 < filePath.Length) {
                mEntityData.loadData(filePath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// ファイル名を指定して保存する
        /// </summary>
        public void saveAsFile()
        {
            List<string[]> filters = new List<string[]>() {
                    new string[] { "CSVファイル", "*.csv" },
                    new string[] { "すべてのファイル", "*.*"}
                };
            string filePath = ylib.fileSaveSelectDlg("データ保存", ".", filters);
            if (0 < filePath.Length) {
                if (filePath.IndexOf(".csv") < 0)
                    filePath = filePath + ".csv";
                mEntityData.saveData(filePath);
            }
        }
    }
}
