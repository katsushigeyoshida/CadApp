﻿using CoreLib;
using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CadApp
{
    /// <summary>
    /// 図面データ管理
    /// </summary>
    public class FileData
    {
        public string mBaseDataFolder = "Zumen";
        public string mBackupFolder = "Zumen";
        public string mGenreName = "図面";
        public string mCategoryName = "無題";
        public string mDataName = "無題";
        public string mDataExt = ".csv";
        public string mDiffTool = "";
        public Window mMainWindow;

        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="baseDataFolder">ベースフォルダ</param>
        public FileData(Window window)
        {
            mMainWindow = window;
        }

        /// <summary>
        /// ベースフォルダの設定
        /// </summary>
        /// <param name="baseDataFolder">ベースフォルダ</param>
        public void setBaseDataFolder(string baseDataFolder = "")
        {
            try {
                if (baseDataFolder != "")
                    mBaseDataFolder = baseDataFolder;
                if (!Directory.Exists(mBaseDataFolder))
                    Directory.CreateDirectory(mBaseDataFolder);
                string genreFolder = Path.Combine(mBaseDataFolder, mGenreName);
                if (!Directory.Exists(genreFolder))
                    Directory.CreateDirectory(genreFolder);
                string categoryFolder = Path.Combine(genreFolder, mCategoryName);
                if (!Directory.Exists(categoryFolder))
                    Directory.CreateDirectory(categoryFolder);
            } catch (Exception e) {
                ylib.messageBox(mMainWindow, e.Message);
            }
        }

        /// <summary>
        /// ジャンルフォルダの設定
        /// </summary>
        /// <param name="genreName">ジャンル名</param>
        public void setGenreFolder(string genreName)
        {
            mGenreName = genreName;
            string genreFolder = getCurGenrePath();
            if (!Directory.Exists(genreFolder))
                Directory.CreateDirectory(genreFolder);
        }

        /// <summary>
        /// カテゴリフォルダの設定
        /// </summary>
        /// <param name="categoryName">カテゴリ名</param>
        public void setCategoryFolder(string categoryName)
        {
            mCategoryName = categoryName;
            string categoryFolder = getCurCategoryPath();
            if (!Directory.Exists(categoryFolder))
                Directory.CreateDirectory(categoryFolder);
        }

        /// <summary>
        /// ジャンルの追加
        /// </summary>
        /// <returns></returns>
        public string addGenre()
        {
            InputBox dlg = new InputBox();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "ジャンル追加";
            if (dlg.ShowDialog() == true) {
                string genrePath = getGenrePath(dlg.mEditText.ToString());
                if (Directory.Exists(genrePath)) {
                    ylib.messageBox(mMainWindow, "すでにジャンルフォルダが存在しています。");
                } else {
                    Directory.CreateDirectory(genrePath);
                    return dlg.mEditText.ToString();
                }
            }
            return "";
        }

        /// <summary>
        /// ジャンル名の変更
        /// </summary>
        /// <param name="genre">ジャンル名</param>
        /// <returns></returns>
        public string renameGenre(string genre)
        {
            InputBox dlg = new InputBox();
            dlg.Title = "ジャンル名変更";
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mEditText = genre;
            string oldGenrePath = getGenrePath(genre);
            if (dlg.ShowDialog() == true) {
                string newGenrePath = getGenrePath(dlg.mEditText.ToString());
                if (Directory.Exists(newGenrePath)) {
                    ylib.messageBox(mMainWindow, "すでにジャンルフォルダが存在しています。");
                } else {
                    Directory.Move(oldGenrePath, newGenrePath);
                    return dlg.mEditText.ToString();
                }
            }
            return "";
        }

        /// <summary>
        /// ジャンルの削除
        /// </summary>
        /// <param name="genre"></param>
        /// <returns></returns>
        public bool removeGenre(string genre)
        {
            string genrePath = getGenrePath(genre);
            if (ylib.messageBox(mMainWindow, genre + " を削除します", "", "項目削除", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                Directory.Delete(genrePath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// カテゴリの追加
        /// </summary>
        /// <returns></returns>
        public string addCategory()
        {
            InputBox dlg = new InputBox();
            dlg.Title = "分類追加";
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (dlg.ShowDialog() == true) {
                string categoryPath = getCategoryPath(dlg.mEditText.ToString());
                if (Directory.Exists(categoryPath)) {
                    ylib.messageBox(mMainWindow, "すでに分類フォルダが存在しています。");
                } else {
                    Directory.CreateDirectory(categoryPath);
                    return dlg.mEditText.ToString();
                }
            }
            return "";
        }

        /// <summary>
        /// カテゴリ名の変更
        /// </summary>
        /// <param name="category">カテゴリ名</param>
        /// <returns></returns>
        public string renameCategory(string category)
        {
            InputBox dlg = new InputBox();
            dlg.Title = "分類名変更";
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mEditText = category;
            string oldCategoryPath = getCategoryPath(category);
            if (dlg.ShowDialog() == true) {
                string newCategoryPath = getCategoryPath(dlg.mEditText.ToString());
                if (Directory.Exists(newCategoryPath)) {
                    ylib.messageBox(mMainWindow, "すでに分類フォルダが存在しています。");
                } else {
                    Directory.Move(oldCategoryPath, newCategoryPath);
                    return dlg.mEditText.ToString();
                }
            }
            return "";
        }

        /// <summary>
        /// カテゴリの削除
        /// </summary>
        /// <param name="category">カテゴリ名</param>
        /// <returns></returns>
        public bool removeCategory(string category)
        {
            string categoryPath = getCategoryPath(category);
            if (ylib.messageBox(mMainWindow, category + " を削除します", "", "項目削除", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                Directory.Delete(categoryPath);
                return true;
            }
            return false;
        }


        /// <summary>
        /// 分類のコピー/移動
        /// </summary>
        /// <param name="categoryName">図面名</param>
        /// <param name="move">移動の可否</param>
        /// <returns></returns>
        public bool copyCategory(string categoryName, bool move = false)
        {
            SelectMenu dlg = new SelectMenu();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mMenuList = getGenreList().ToArray();
            if (dlg.ShowDialog() == true) {
                string oldCategoryPath = getCategoryPath(categoryName);
                string newCategoryPath = getCategoryPath(categoryName, dlg.mSelectItem);
                int opt = 1;
                while (Directory.Exists(newCategoryPath)) {
                    newCategoryPath = getCategoryPath(categoryName + "(" + opt + ")", dlg.mSelectItem);
                    opt++;
                }
                if (move) {
                    Directory.Move(oldCategoryPath, newCategoryPath);
                } else {
                    ylib.copyDrectory(oldCategoryPath, newCategoryPath);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 図面の追加
        /// </summary>
        /// <returns></returns>
        public string addItem()
        {
            InputBox dlg = new InputBox();
            dlg.Title = "図面追加";
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (dlg.ShowDialog() == true) {
                string filePath = getItemFilePath(dlg.mEditText.ToString());
                if (File.Exists(filePath)) {
                    ylib.messageBox(mMainWindow, "すでにファイルが存在しています。");
                } else {
                    return dlg.mEditText.ToString();
                }
            }
            return "";
        }

        /// <summary>
        /// 図面名の変更
        /// </summary>
        /// <param name="dataName"></param>
        /// <returns></returns>
        public string renameItem(string dataName)
        {
            InputBox dlg = new InputBox();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "図面名変更";
            dlg.mEditText = dataName;
            string oldDataFilePath= getItemFilePath(dataName);
            if (dlg.ShowDialog() == true) {
                string newDataFilePath = getItemFilePath(dlg.mEditText.ToString());
                if (File.Exists(newDataFilePath)) {
                    ylib.messageBox(mMainWindow, "すでにファイルが存在しています。");
                } else {
                    File.Move(oldDataFilePath, newDataFilePath);
                    return dlg.mEditText.ToString();
                }
            }
            return "";
        }

        /// <summary>
        /// 図面ファイルの削除
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public bool removeItem(string itemName)
        {
            string filePath = getItemFilePath(itemName);
            if (ylib.messageBox(mMainWindow, itemName + " を削除します", "", "項目削除", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                File.Delete(filePath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 図面ファイルのコピー/移動
        /// </summary>
        /// <param name="itemName">図面名</param>
        /// <param name="move">移動の可否</param>
        /// <returns></returns>
        public bool copyItem(string itemName, bool move = false)
        {
            SelectCategory dlg = new SelectCategory();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mRootFolder = mBaseDataFolder;
            if (dlg.ShowDialog() == true) {
                string oldItemPath = getItemFilePath(itemName);
                string newItemPath = getItemFilePath(itemName, dlg.mSelectCategory, dlg.mSelectGenre);
                string item = Path.GetFileNameWithoutExtension(itemName);
                string ext = Path.GetExtension(itemName);
                int opt = 1;
                while (File.Exists(newItemPath)) {
                    newItemPath = getItemFilePath(item + "(" + opt + ")", dlg.mSelectCategory, dlg.mSelectGenre);
                    opt++;
                }
                if (move) {
                    File.Move(oldItemPath, newItemPath);
                } else {
                    File.Copy(oldItemPath, newItemPath);
                }
                return true;
            }
            return false;
        }


        /// <summary>
        /// ジャンルリストの取得
        /// </summary>
        /// <returns></returns>
        public List<string> getGenreList()
        {
            List<string> genreList = ylib.getDirectories(mBaseDataFolder);
            if (genreList != null) {
                genreList.Sort();
                genreList = genreList.ConvertAll(p => ylib.getLastFolder(p, 1));
            }
            return genreList;
        }

        /// <summary>
        /// カテゴリリストの取得
        /// </summary>
        /// <returns></returns>
        public List<string> getCategoryList()
        {
            List<string> categoryList = new List<string>();
            try {
                categoryList = ylib.getDirectories(getCurGenrePath());
                if (categoryList != null) {
                    categoryList.Sort();
                    categoryList = categoryList.ConvertAll(p => ylib.getLastFolder(p, 1));
                }
            } catch (Exception e) {
                ylib.messageBox(mMainWindow, e.Message);
            }
            return categoryList;
        }

        /// <summary>
        /// データファイルの一覧の取得
        /// </summary>
        /// <returns></returns>
        public List<string> getItemFileList()
        {
            List<string> fileNameList = new List<string>();
            try {
                string[] files = ylib.getFiles(getCurCategoryPath() + "\\*.csv");
                if (files != null) {
                    for (int i = 0; i < files.Length; i++) {
                        fileNameList.Add(Path.GetFileNameWithoutExtension(files[i]));
                    }
                }
            } catch (Exception e) {
                ylib.messageBox(mMainWindow, e.Message);
            }

            return fileNameList;
        }

        /// <summary>
        /// カレント状態のジャンルパスの取得
        /// </summary>
        /// <returns>ジャンルパス</returns>
        public string getCurGenrePath()
        {
            return Path.Combine(mBaseDataFolder, mGenreName);
        }

        /// <summary>
        /// カレント状態のカテゴリパスの取得
        /// </summary>
        /// <returns>カテゴリパス</returns>
        public string getCurCategoryPath()
        {
            string curCategoryPath = mGenreName + "\\" + mCategoryName;
            return Path.Combine(mBaseDataFolder, curCategoryPath);
        }

        /// <summary>
        /// カレント状態の図面ファイルパスの取得
        /// </summary>
        /// <returns>ファイルパス</returns>
        public string getCurItemFilePath()
        {
            return getItemFilePath(mDataName, mCategoryName, mGenreName);
        }

        /// <summary>
        /// 大分類(ジャンル)パスの取得
        /// </summary>
        /// <param name="genre">大分類名</param>
        /// <returns>大分類パス</returns>
        public string getGenrePath(string genre)
        {
            return Path.Combine(mBaseDataFolder, genre);
        }

        /// <summary>
        /// 分類パスの取得
        /// </summary>
        /// <param name="category">分類名</param>
        /// <returns>分類パス</returns>
        public string getCategoryPath(string category)
        {
            return Path.Combine(mBaseDataFolder, mGenreName + "\\" + category);
        }

        /// <summary>
        /// 分類パスの取得
        /// </summary>
        /// <param name="categoryName">分類名</param>
        /// <param name="genreName">大分類名</param>
        /// <returns>分類パス</returns>
        public string getCategoryPath(string categoryName, string genreName)
        {
            return Path.Combine(mBaseDataFolder, genreName + "\\" + categoryName);
        }

        /// <summary>
        /// 図面ファイル名を指定してファイルパスの取得
        /// </summary>
        /// <param name="dataName">図面ファイル名</param>
        /// <returns>ファイルパス</returns>
        public string getItemFilePath(string dataName)
        {
            return getItemFilePath(dataName, mCategoryName, mGenreName);
        }

        /// <summary>
        /// 図面ファイル名とカテゴリを指定してファイルパスを取得
        /// </summary>
        /// <param name="dataName">図面ファイル名</param>
        /// <param name="categoryName">カテゴリ名</param>
        /// <returns>ファイルパス</returns>
        public string getItemFilePath(string dataName, string categoryName)
        {
            return getItemFilePath(dataName, categoryName, mGenreName);
        }

        /// <summary>
        /// 図面ファイル名とカテゴリ、ジャンルを指定してファイルパスを取得
        /// </summary>
        /// <param name="dataName">図面ファイル名</param>
        /// <param name="categoryName">カテゴリ名param>
        /// <param name="genreName">ジャンル</param>
        /// <returns>ファイルパス</returns>
        public string getItemFilePath(string dataName, string categoryName, string genreName)
        {
            string curDataFilePath = genreName + "\\" + categoryName + "\\" + dataName + mDataExt;
            return Path.Combine(mBaseDataFolder, curDataFilePath);
        }

        /// <summary>
        /// 図面データのプロパティ
        /// </summary>
        /// <param name="itemName">図面名</param>
        /// <returns>プロパティデータ</returns>
        public string getItemFileProperty(string itemName)
        {
            string itemPath = getItemFilePath(itemName);
            EntityData entityData = new EntityData();
            entityData.loadData(itemPath);
            string buf = "■ファイルデータ\n";
            buf += "項目名: " + itemName + "\n";
            buf += "分類名: " + mCategoryName + "\n";
            buf += "大分類名: " + mGenreName + "\n";
            buf += "パス: " + itemPath + "\n";
            FileInfo fileInfo = new FileInfo(itemPath);
            buf += "ファイルサイズ: " + fileInfo.Length.ToString("#,###") + "\n";
            buf += "作成日: " + fileInfo.CreationTime + "\n";
            buf += "更新日: " + fileInfo.LastWriteTime + "\n";
            buf += "■図面データ\n";
            buf +=  entityData.getDataInfo();

            return buf;
        }

        /// <summary>
        /// データをバックアップする
        /// </summary>
        public int dataBackUp(bool messageOn = true)
        {
            int count = 0;
            if (mBaseDataFolder == null || mBackupFolder.Length == 0
                || !Directory.Exists(mBackupFolder)) {
                ylib.messageBox(mMainWindow, "図面データのバックアップのフォルダが設定されていません。");
                return count;
            }
            string backupFolder = Path.Combine(mBackupFolder, Path.GetFileName(mBaseDataFolder));
            if (Path.GetFullPath(mBaseDataFolder) != Path.GetFullPath(backupFolder)) {
                DirectoryDiff directoryDiff = new DirectoryDiff(mBaseDataFolder, backupFolder);
                List<FilesData> removeFile = directoryDiff.getNoExistFile();
                bool noExistFileRemove = true;
                if (0 < removeFile.Count) {
                    if (ylib.messageBox(mMainWindow,
                        $"ソースにないファイルがバックアップに {removeFile.Count} ファイル存在します。\nバックアップから削除しますか?",
                        "" ,"確認", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        noExistFileRemove = false;
                }
                count = directoryDiff.syncFolder(noExistFileRemove);
                if (messageOn)
                    ylib.messageBox(mMainWindow, $"{count} ファイルのバックアップを更新しました。");
            } else {
                ylib.messageBox(mMainWindow, "バックアップ先がデータフォルダと同じです");
            }
            return count;
        }

        /// <summary>
        /// バックアップデータを元に戻す
        /// </summary>
        public void dataRestor()
        {
            if (mBaseDataFolder == null || mBackupFolder.Length == 0 || !Directory.Exists(mBackupFolder)) {
                ylib.messageBox(mMainWindow, "バックアップのフォルダが設定されていません。");
                return;
            }
            string backupFolder = Path.Combine(mBackupFolder, Path.GetFileName(mBaseDataFolder));
            DiffFolder dlg = new DiffFolder();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mSrcTitle = "比較元(データフォルダ)";
            dlg.mDestTitle = "比較先(バックアップ先)";
            dlg.mSrcFolder = mBaseDataFolder;
            dlg.mDestFolder = backupFolder;
            dlg.mDiffTool = mDiffTool;
            dlg.ShowDialog();
        }

        /// <summary>
        /// 未使用イメージキャッシュファイルの削除
        /// </summary>
        /// <param name="imageCacheFolder">イメージキャッシュファイルフォルダ</param>
        public void squeezeImageCache(string imageCacheFolder)
        {
            //  図面データファイルリストの取得
            List<FileInfo> fileList = ylib.getDirectoriesInfo(mBaseDataFolder, "*.csv");
            //  図面データ中のイメージキャッシュファイルのリストの作成
            List<string> dataImageCacheFiles = new List<string>();
            foreach (FileInfo file in fileList) {
                //System.Diagnostics.Debug.WriteLine(file.FullName);
                List<string> dataImageCacheFile = getImageCacheFile(file.FullName);
                if (0 < dataImageCacheFile.Count) {
                    foreach (string imagePath in dataImageCacheFile) {
                        System.Diagnostics.Debug.WriteLine(file.Name + ": " + imagePath);
                    }
                    dataImageCacheFiles.AddRange(dataImageCacheFile);
                }
            }
            int count = 0;
            //  イメージキャッシュファイルの取得
            string[] cacheFiles = ylib.getFiles(Path.Combine(imageCacheFolder, "*.*"));
            if (0 < cacheFiles.Length) {
                //  未使用イメージキャッシュファイルの削除
                foreach (string cacheFile in cacheFiles) {
                    if (!dataImageCacheFiles.Contains(Path.GetFileName(cacheFile))) {
                        System.Diagnostics.Debug.WriteLine("Cache: " + Path.GetFileName(cacheFile));
                        File.Delete(cacheFile);
                        count++;
                    }
                }
                if (0 < count) {
                    ylib.messageBox(mMainWindow, $"{count} ファイルの未使用イメージファイルを削除");
                }
            }
        }

        /// <summary>
        /// 図面データ中のイメージキャッシュファイルの取得
        /// </summary>
        /// <param name="path">図面データファイルパス</param>
        /// <returns>イメージファイルリスト</returns>
        public List<string> getImageCacheFile(string path)
        {
            List<string > imagePaths = new List<string>();
            if (!File.Exists(path))
                return imagePaths;
            List<string[]> dataList = ylib.loadCsvData(path);
            for (int i = 0; i < dataList.Count; i++) {
                string proerty = dataList[i++][0];
                if (proerty == "Image") {
                    string imagePath = dataList[i][0];      //  イメージファイルパス(オリジナル)
                    string imageCachePath = dataList[i][7]; //  イメージファイルキャッシュパス
                    imagePaths.Add(imageCachePath);
                }
            }
            return imagePaths;
        }

        /// <summary>
        /// 選択したデータをDXFファイルに変換
        /// </summary>
        /// <param name="itemName">図面名</param>
        public void exportAsFile(string itemName)
        {
            List<string[]> filters = new List<string[]>() {
                    new string[] { "図面ファイル", "*.dxf" },
                    new string[] { "すべてのファイル", "*.*"}
                };
            string exportFilePath = ylib.fileSaveSelectDlg("データ出力", ".", filters);
            if (exportFilePath != null && 0 < exportFilePath.Length) {
                if (Path.GetExtension(exportFilePath) == "")
                    exportFilePath += ".dxf";
                string itemPath = getItemFilePath(itemName);
                DxfConv dxfConv = new DxfConv();
                dxfConv.exportDxf(exportFilePath, itemPath);
            }
        }

        /// <summary>
        /// DXFファイルの選択とインポート
        /// </summary>
        /// <returns>DXFファイル名(拡張子なし)</returns>
        public string importAsFile()
        {
            List<string[]> filters = new List<string[]>() {
                    new string[] { "図面ファイル", "*.dxf" },
                    new string[] { "すべてのファイル", "*.*"}
                };
            string importPath = ylib.fileOpenSelectDlg("データ読込", ".", filters);
            if (importPath == null || importPath.Length == 0)
                return "";
            string outPath = getItemFilePath(Path.GetFileNameWithoutExtension(importPath));
            DxfConv dxfConv = new DxfConv();
            return dxfConv.importDxf(importPath, outPath);
        }
    }
}
