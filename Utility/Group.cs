using CoreLib;
using System.Collections.Generic;
using System.Linq;

namespace CadApp
{
    public class Group
    {
        public Dictionary<int, string> mGroupList;      //  グループ名リスト

        private YLib ylib = new YLib();

        public Group()
        {
            mGroupList = new Dictionary<int, string>();
        }

        /// <summary>
        /// グループNoを抽出する
        /// </summary>
        /// <param name="entityList">要素リスト</param>
        /// <param name="groupNo">グループNo</param>
        /// <returns>グループNoリスト</returns>
        public List<int> getGroupNoList(List<Entity> entityList, int groupNo)
        {
            List<int> groupList = new List<int>();
            if (entityList != null) {
                for (int i = 0; i < entityList.Count; i++)
                    if (entityList[i].mGroup == groupNo)
                        groupList.Add(i);
            }
            return groupList;
        }

        /// <summary>
        /// グループ名のリスト化
        /// </summary>
        /// <returns>グループ名リスト</returns>
        public List<string> getGroupNameList()
        {
            List<string> list = new List<string>();
            foreach (var item in mGroupList)
                list.Add(item.Value);
            return list;
        }

        /// <summary>
        /// グループ名の追加
        /// </summary>
        /// <param name="name">グループ名</param>
        /// <returns>グループNo</returns>
        public int add(string name)
        {
            if (!mGroupList.ContainsValue(name)) {
                int n = 1;
                while (mGroupList.ContainsKey(n))
                    n++;
                mGroupList[n] = name;
                return n;
            } else
                return getGroupNo(name);
        }

        /// <summary>
        /// グループ名の取得
        /// </summary>
        /// <param name="n">グループNo</param>
        /// <returns>グループ名</returns>
        public string getGroupName(int n)
        {
            if (mGroupList.ContainsKey(n))
                return mGroupList[n];
            else
                return "";
        }

        /// <summary>
        /// グループ名からグループNoを取得(0はグループ名登録なし)
        /// </summary>
        /// <param name="name">グループ名</param>
        /// <returns>グループNo</returns>
        public int getGroupNo(string name)
        {
            if (mGroupList.ContainsValue(name)) {
                var val = mGroupList.FirstOrDefault(x => x.Value == name);
                return val.Key;
            } else
                return 0;
        }

        /// <summary>
        /// 使用されていないグループ名を削除する
        /// </summary>
        /// <param name="entityList">要素リスト</param>
        public void squeeze(List<Entity> entityList)
        {
            List<int> noList = new List<int>();
            for (int i = 0; i < entityList.Count; i++) {
                if (0 < entityList[i].mGroup) {
                    if (!noList.Contains(entityList[i].mGroup))
                        noList.Add(entityList[i].mGroup);
                }
            }
            foreach (var item in mGroupList) {
                if (!noList.Contains(item.Key) || item.Value == "")
                    mGroupList.Remove(item.Key);
            }
        }

        /// <summary>
        /// グループデータを文字列配列リストに変換
        /// </summary>
        /// <returns>文字列配列リスト</returns>
        public string[] toDataList()
        {
            List<string> list = new List<string>();
            string buf = "GroupList";
            list.Add(buf);
            foreach (var item in mGroupList) {
                list.Add(item.Key.ToString());
                list.Add(item.Value);
            }
            return list.ToArray();
        }

        /// <summary>
        /// グループデータの設定
        /// </summary>
        /// <param name="dataList">文字列配列リスト</param>
        /// <param name="sp">リスト開始位置</param>
        /// <returns>リスト終了位置</returns>
        public void setDataList(string[] dataList)
        {
            int sp = 0;
            if (dataList[sp] == null || dataList[sp] != "GroupList")
                return;
            sp++;
            mGroupList.Clear();
            while (sp < dataList.Length) {
                int groupNo = ylib.intParse(dataList[sp++]);
                mGroupList.Add(groupNo, dataList[sp++]);
            }
            return ;
        }
    }
}
