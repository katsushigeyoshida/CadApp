using CoreLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace CadApp
{
    public class SymbolData
    {
        public string mSymbolFolder = "Symbol";
        public string mBackupFolder = "";
        public List<string> mSymbolFiles = new List<string>();
        public Dictionary<string, Entity> mSymbolList = new Dictionary<string, Entity>();
        public string mDiffTool = "";

        public Window mMainWindow;
        private EntityData mEntityData;
        private YLib ylib = new YLib();

        public SymbolData(Window window)
        {
            mMainWindow = window;
            mEntityData = new EntityData();
            //  シンボルフォルダ
            mSymbolFolder = Path.GetFullPath(mSymbolFolder);
            if (!Directory.Exists(mSymbolFolder))
                Directory.CreateDirectory(mSymbolFolder);
            //  シンボルデータの取得
            mSymbolFiles = getSymbolFileList(mSymbolFolder);
        }

        /// <summary>
        /// シンボルの登録
        /// </summary>
        /// <param name="category">分類名</param>
        /// <param name="parts">パーツデータ</param>
        public void registSymbol(string category, PartsEntity parts)
        {
            string path = getSymbolFilePath(category);
            Dictionary<string, Entity> symbolList = new Dictionary<string, Entity>();
            if (File.Exists(path)) {
                symbolList = loadSymbolList(path);
            }
            if (symbolList.ContainsKey(parts.mParts.mName)) {
                symbolList[parts.mParts.mName] = parts;
            } else {
                symbolList.Add(parts.mParts.mName, parts);
            }
            saveSymbolList(symbolList, path);
        }

        /// <summary>
        /// シンボルデータを取得する
        /// </summary>
        /// <param name="category">分類</param>
        /// <param name="symbolname">シンボル名</param>
        /// <returns>Entityデータ</returns>
        public Entity getSymbolData(string category, string symbolname)
        {
            string path = getSymbolFilePath(category);
            Dictionary<string, Entity> symbolList = loadSymbolList(path);
            return symbolList["__" + symbolname];
        }

        /// <summary>
        /// シンボルをリストから削除する
        /// </summary>
        /// <param name="category">分類</param>
        /// <param name="symbolname">シンボル名</param>
        /// <returns></returns>
        public bool removeSymbolData(string category, string symbolname)
        {
            string path = getSymbolFilePath(category);
            Dictionary<string, Entity> symbolList = loadSymbolList(path);
            if (symbolList.ContainsKey("__" + symbolname)) {
                symbolList.Remove("__" + symbolname);
                saveSymbolList(symbolList, path);
                return true;
            }
            return false;
        }

        /// <summary>
        /// シンボル名を変更する
        /// </summary>
        /// <param name="category">分類</param>
        /// <param name="oldName">シンボル名</param>
        /// <param name="newName">新シンボル名</param>
        /// <returns></returns>
        public bool renameSymbolData(string category, string oldName, string newName)
        {
            string path = getSymbolFilePath(category);
            Dictionary<string, Entity> symbolList = loadSymbolList(path);
            if (symbolList.ContainsKey("__" + oldName) && !symbolList.ContainsKey("__" + newName)) {
                PartsEntity ent = (PartsEntity)symbolList["__" + oldName];
                ent.mParts.mName = "__" + newName;
                saveSymbolList(symbolList, path);
                return true;
            }
            return false;
        }

        /// <summary>
        /// シンボルデータを他の分類にコピーする
        /// </summary>
        /// <param name="category">分類</param>
        /// <param name="symbolName">シンボル名</param>
        /// <param name="newCategory">新分類</param>
        /// <param name="move">移動</param>
        /// <returns></returns>
        public bool copySymbolData(string category, string symbolName, string newCategory, bool move = false)
        {
            string path = getSymbolFilePath(category);
            Dictionary<string, Entity> symbolList = loadSymbolList(path);
            PartsEntity ent = (PartsEntity)symbolList["__" + symbolName];
            string newPath = getSymbolFilePath(newCategory);
            Dictionary<string, Entity> newSymbolList;
            if (File.Exists(newPath)) {
                newSymbolList = loadSymbolList(newPath);
            } else {
                newSymbolList = new Dictionary<string, Entity>();
            }
            if (!newSymbolList.ContainsKey("__" + symbolName)) {
                newSymbolList.Add(symbolName, ent);
                saveSymbolList(newSymbolList, newPath);
            } else {
                return false;
            }
            if (move) {
                symbolList.Remove(symbolName);
                saveSymbolList(symbolList, path);
            }
            return true;
        }

        /// <summary>
        /// 分類名からファイルのパスを取得
        /// </summary>
        /// <param name="category">分類名</param>
        /// <returns>パス</returns>
        public string getSymbolFilePath(string category)
        {
            return mSymbolFolder + "/" + category + ".csv";
        }

        /// <summary>
        /// シンボルリストからシンボル名リストを取得
        /// </summary>
        /// <param name="symbolList">シンボルリスト</param>
        /// <param name="sort">ソート</param>
        /// <returns>シンボル名リスト</returns>
        public List<string> getSymbolNameList(Dictionary<string, Entity> symbolList, bool sort = false)
        {
            List<string> nameList = new List<string>();
            foreach (string s in symbolList.Keys) {
                nameList.Add(s.TrimStart('_'));
            }
            if (sort)
                nameList.Sort();
            return nameList;
        }

        /// <summary>
        /// 分類名リストの取得
        /// </summary>
        /// <returns>分類名リスト</returns>
        public List<string> getCategoryList()
        {
            List<string> categoryList = new List<string>();
            List<string> files = getSymbolFileList(mSymbolFolder);
            foreach (string file in files) {
                categoryList.Add(Path.GetFileNameWithoutExtension(file));
            }
            return categoryList;
        }

        /// <summary>
        /// 全のシンボル名のリストを取得
        /// </summary>
        /// <returns>シンボル名リスト</returns>
        public List<List<string>> getSymbolList()
        {
            List<string> categoryList = getCategoryList();
            List<List<string>> symbolList = new List<List<string>>();
            foreach (string category in categoryList) {
                string path = getSymbolFilePath(category);
                symbolList.Add(getSymbolList(path, true));
            }
            return symbolList;
        }

        /// <summary>
        /// シンボルファイルからシンボル名のリストに変換
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="sort">ソート</param>
        /// <returns>シンボル名リスト</returns>
        public List<string> getSymbolList(string path, bool sort = false)
        {
            Dictionary<string, Entity> symbolList = loadSymbolList(path);
            return getSymbolNameList(symbolList, sort);
        }

        /// <summary>
        /// シンボルデータの保存
        /// </summary>
        /// <param name="symbolList">シンボルリスト</param>
        /// <param name="path">ファイルパス</param>
        public void saveSymbolList(Dictionary<string, Entity> symbolList, string path)
        {
            List<string[]> listData = new List<string[]>();
            //  データが存在しない時はファイルを削除
            if (symbolList.Count == 0) {
                if (File.Exists(path))
                    File.Delete(path);
            }

            //  要素データ
            foreach (Entity entity in symbolList.Values) {
                if (entity != null && !entity.mRemove && entity.mEntityId != EntityId.Non) {
                    listData.Add(entity.toList().ToArray());
                    listData.Add(entity.toDataList().ToArray());
                }
            }
            ylib.saveCsvData(path, listData);
        }

        /// <summary>
        /// シンボルファイルからシンボル要素を取得
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>シンボルリスト</returns>
        public Dictionary<string, Entity> loadSymbolList(string path)
        {
            Dictionary<string, Entity> symbolList = new Dictionary<string, Entity>();
            List<string[]> dataList = ylib.loadCsvData(path);
            for (int i = 0; i < dataList.Count - 1; i++) {
                Entity entity = setStringEntityData(dataList[i], dataList[i + 1]);
                if (entity != null) {
                    PartsEntity parts = (PartsEntity)entity;
                    if (parts.mParts.mName.Substring(0, 2).CompareTo("__") == 0 &&
                        !symbolList.ContainsKey(parts.mParts.mName)) {
                        symbolList.Add(parts.mParts.mName, parts);
                    }
                }
            }
            return symbolList;
        }

        /// <summary>
        /// 要素ファイルデータのからEnturyデータに変換
        /// </summary>
        /// <param name="property">属性データ</param>
        /// <param name="dataStr">要素データ</param>
        /// <returns>Entityデータ</returns>
        public Entity setStringEntityData(string[] property, string[] dataStr)
        {
            if (0 < property.Length) {
                if (0 <= property[0].IndexOf(EntityId.Parts.ToString())) {
                    //  パーツ要素
                    PartsEntity partsEntity = new PartsEntity();
                    partsEntity.setProperty(property);
                    partsEntity.setData(dataStr);
                    return partsEntity;
                }
            }
            return null;
        }

        /// <summary>
        /// シンボルのファイルを検索リスト化
        /// </summary>
        /// <param name="folder">シンボルフォルダ</param>
        /// <returns>シンボルファイル</returns>
        public List<string> getSymbolFileList(string folder)
        {
            List<string>  symbolFiles = new List<string>();
            try {
                    string[] files = ylib.getFiles(folder + "\\*.csv");
                    for (int i = 0; i < files.Length; i++) {
                        symbolFiles.Add(files[i]);
                    }
            } catch (Exception e) {
                ylib.messageBox(mMainWindow, e.Message);
            }
            return symbolFiles;
        }


        /// <summary>
        /// データをバックアップする
        /// </summary>
        public void dataBackUp()
        {
            if (mSymbolFolder == null || mBackupFolder.Length == 0 || !Directory.Exists(mBackupFolder)) {
                ylib.messageBox(mMainWindow, "バックアップのフォルダが設定されていません。");
                return;
            }
            string backupFolder = Path.Combine(mBackupFolder, Path.GetFileName(mSymbolFolder));
            if (Path.GetFullPath(mSymbolFolder) != Path.GetFullPath(backupFolder)) {
                DirectoryDiff directoryDiff = new DirectoryDiff(mSymbolFolder, backupFolder);
                int count = directoryDiff.syncFolder();
                ylib.messageBox(mMainWindow, $"{count} ファイルのバックアップを更新しました。");
            } else {
                ylib.messageBox(mMainWindow, "バックアップ先がデータフォルダと同じです");
            }
        }

        /// <summary>
        /// バックアップデータを元に戻す
        /// </summary>
        public void dataRestor()
        {
            if (mSymbolFolder == null || mBackupFolder.Length == 0 || !Directory.Exists(mBackupFolder)) {
                ylib.messageBox(mMainWindow, "バックアップのフォルダが設定されていません。");
                return;
            }
            string backupFolder = Path.Combine(mBackupFolder, Path.GetFileName(mSymbolFolder));
            DiffFolder dlg = new DiffFolder();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mSrcTitle = "比較元(データフォルダ)";
            dlg.mDestTitle = "比較先(バックアップ先)";
            dlg.mSrcFolder = mSymbolFolder;
            dlg.mDestFolder = backupFolder;
            dlg.mDiffTool = mDiffTool;
            dlg.ShowDialog();
        }
    }
}
