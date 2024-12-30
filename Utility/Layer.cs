using CoreLib;
using System.Collections.Generic;

namespace CadApp
{
    /// <summary>
    /// レイヤー管理
    /// </summary>
    public class Layer
    {
        public Dictionary<string, ulong> mLayerList;    //  レイヤーリスト

        public List<Entity> mEntityList;                //  要素リスト
        public DrawingPara mPara;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="entityList">要素リスト</param>
        /// <param name="para">要素プロパティ</param>
        public Layer(List<Entity> entityList, DrawingPara para)
        {
            mLayerList = new Dictionary<string, ulong>();
            mEntityList = entityList;
            mPara = para;
        }

        /// <summary>
        /// レイヤーリストクリア
        /// </summary>
        public void clear()
        {
            mLayerList.Clear();
        }

        /// <summary>
        /// レイヤー名から表示ビットを取得
        /// </summary>
        /// <param name="layerName">レイヤー名</param>
        /// <returns>表示ビット</returns>
        public ulong getLayerBit(string layerName)
        {
            if (mLayerList.ContainsKey(layerName))
                return mLayerList[layerName];
            else
                return 0;
        }

        /// <summary>
        /// 表示ビットからレイヤー名を取得
        /// </summary>
        /// <param name="bit">表示ビット</param>
        /// <returns>レイヤー名</returns>
        public string getLayerName(ulong bit)
        {
            if (mLayerList.ContainsValue(bit)) {
                foreach (KeyValuePair<string, ulong> item in mLayerList) {
                    if (item.Value == bit)
                        return item.Key;
                }
            }
            return "";
        }

        /// <summary>
        /// 新規レイヤー名に表示ビットを割り当てる
        /// </summary>
        /// <param name="layerName">レイヤー名</param>
        /// <returns>表示ビット</returns>
        public ulong setLayerBit(string layerName)
        {
            if (mLayerList.ContainsKey(layerName))
                return mLayerList[layerName];
            ulong layerBit = 0x1;
            for (int i = 0; i < 64; i++) {
                if (getLayerName(layerBit).Length == 0) {
                    mLayerList.Add(layerName, layerBit);
                    return layerBit;
                }
                layerBit <<= 1;
            }
            return 0;
        }

        /// <summary>
        /// レイヤー名のリストを取得
        /// </summary>
        /// <returns>レイヤー名リスト</returns>
        public List<string> getLayerNameList()
        {
            List<string> layerList = new List<string>();
            updateLayerList();
            foreach (string key in mLayerList.Keys) {
                layerList.Add(key);
            }
            layerList.Sort();
            return layerList;
        }

        /// <summary>
        /// チェックリスト形式でレイヤー名リストを取得
        /// </summary>
        /// <returns>レイヤー名リスト</returns>
        public List<CheckBoxListItem> getLayerChkList()
        {
            updateLayerList();
            addDispLayer(mPara.mCreateLayerName);
            List<CheckBoxListItem> chkList = new List<CheckBoxListItem>();
            foreach (KeyValuePair<string, ulong> item in mLayerList) {
                CheckBoxListItem chkItem = new CheckBoxListItem((mPara.mDispLayerBit & item.Value) != 0, item.Key);
                chkList.Add(chkItem);
            }
            chkList.Sort((a, b) => a.Text.CompareTo(b.Text));
            return chkList;
        }

        /// <summary>
        /// 要素全体のレイヤー名と表示ビットを更新
        /// </summary>
        public void updateLayerList()
        {
            mLayerList.Clear();
            foreach (Entity ent in mEntityList) {
                if (ent.mRemove == false && ent.mEntityId != EntityId.Link) {
                    if (ent.mLayerName.Length == 0) {
                        ent.mLayerName = mPara.mCreateLayerName;
                    }
                    ent.mLayerBit = setLayerBit(ent.mLayerName);
                }
            }
            if (!mLayerList.ContainsKey(mPara.mCreateLayerName))
                setLayerBit(mPara.mCreateLayerName);
        }

        /// <summary>
        /// 表示レイヤーの追加
        /// </summary>
        /// <param name="layerBit"></param>
        public void addDispLayer(ulong layerBit)
        {
            mPara.mDispLayerBit |= layerBit;
        }

        /// <summary>
        /// 表示レイヤーの追加
        /// </summary>
        /// <param name="layerName"></param>
        public void addDispLayer(string layerName)
        {
            ulong layerbit = setLayerBit(layerName);
            addDispLayer(layerbit);
        }

        /// <summary>
        /// チェックリスト形式でレイヤー名リストから表示ビットフィルタを作成
        /// </summary>
        /// <param name="chkList"></param>
        public void setDispLayerBit(List<CheckBoxListItem> chkList)
        {
            mPara.mDispLayerBit = 0;
            foreach (CheckBoxListItem chkItem in chkList) {
                if (chkItem.Checked) {
                    mPara.mDispLayerBit |= mLayerList[chkItem.Text];
                }
            }
        }
    }
}
