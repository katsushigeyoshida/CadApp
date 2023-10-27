using CoreLib;
using System;
using System.IO;
using System.Windows;

namespace CadApp
{
    /// <summary>
    /// イメージデータ管理
    /// </summary>
    public class ImageData
    {
        public string mImageFolder = "ImageCache";
        public string mBackupFolder = "";
        public Window mMainWindow;

        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="window"></param>
        public ImageData(Window window)
        {
            mMainWindow = window;
            //  イメージファイルフォルダ
            mImageFolder = Path.GetFullPath(mImageFolder);
            if (!Directory.Exists(mImageFolder))
                Directory.CreateDirectory(mImageFolder);
        }

        /// <summary>
        /// イメージキャッシュフォルダにコピーしてキャッシュファイル名を返す
        /// 同一ファイル名の重複を避けるためにファイルのハッシュコードをつけてコピー
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>キャッシュファイル名</returns>
        public string hashCopyFile(string filePath)
        {
            if (File.Exists(filePath)) {
                string hashString = getHashFile(filePath);
                string destName = hashString + "_" + Path.GetFileName(filePath);
                string destPath = Path.Combine(mImageFolder, destName);
                try {
                    File.Copy(filePath, destPath, true);
                } catch (Exception e) {
                    ylib.messageBox(mMainWindow, e.Message);
                    return "";
                }
                return destPath;
            }
            return "";
        } 

        /// <summary>
        /// Bitmapをファイルに保存する
        /// ファイルが存在する場合には末尾の番号をインクリメントする
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public string saveBitmapCash(System.Drawing.Bitmap bitmap)
        {
            string imageFolder = Path.Combine(ylib.getAppFolderPath(), "ClipImage");
            if (!Directory.Exists(imageFolder)) {
                Directory.CreateDirectory(imageFolder);
            }
            string fileName = "ClipImage";
            string ext = ".png";
            string filePath = Path.Combine(imageFolder, fileName + ext);
            int opt = 1;
            while (File.Exists(filePath)) {
                filePath = Path.Combine(imageFolder, fileName + "(" + opt + ")" + ext);
                opt++;
            }
            ylib.saveBitmapImage(ylib.bitmap2BitmapSource(bitmap), filePath);
            return filePath;
        }

        /// <summary>
        /// 文字列からハッシュコード(CRC32)を求める
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns>ハッシュコード</returns>
        public string getHashString(string str)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            CoreLib.HashCode hashCode = new CoreLib.HashCode(CoreLib.HashCode.HashType.CRC32L);
            ulong hash = hashCode.GetCrc32L(bytes);

            return hash.ToString("X");
        }

        /// <summary>
        /// ファイルのハッシュコードを作成
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string getHashFile(string filePath)
        {
            if (!File.Exists(filePath))
                return "";
            CoreLib.HashCode hashCode = new CoreLib.HashCode(CoreLib.HashCode.HashType.CRC32L);
            ulong hash = hashCode.GetCrc32L(filePath);

            return hash.ToString("X");
        }

        /// <summary>
        /// イメージのキャッシュファイルをバックアップする
        /// </summary>
        public void dataBackUp()
        {
            if (mImageFolder == null || mBackupFolder.Length == 0 || !Directory.Exists(mBackupFolder)) {
                ylib.messageBox(mMainWindow, "バックアップのフォルダが設定されていません。");
                return;
            }
            string backupFolder = Path.Combine(mBackupFolder, Path.GetFileName(mImageFolder));
            if (Path.GetFullPath(mImageFolder) != Path.GetFullPath(backupFolder)) {
                DirectoryDiff directoryDiff = new DirectoryDiff(mImageFolder, backupFolder);
                int count = directoryDiff.syncFolder();
                ylib.messageBox(mMainWindow, $"{count} ファイルのバックアップを更新しました。");
            } else {
                ylib.messageBox(mMainWindow, "バックアップ先がデータフォルダと同じです");
            }
        }

        /// <summary>
        /// バックアップ先とファイル比較ダイヤログを表示
        /// </summary>
        public void dataRestor()
        {
            if (mImageFolder == null || mBackupFolder.Length == 0 || !Directory.Exists(mBackupFolder)) {
                ylib.messageBox(mMainWindow, "バックアップのフォルダが設定されていません。");
                return;
            }
            string backupFolder = Path.Combine(mBackupFolder, Path.GetFileName(mImageFolder));
            DiffFolder dlg = new DiffFolder();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mSrcTitle = "比較元(データフォルダ)";
            dlg.mDestTitle = "比較先(バックアップ先)";
            dlg.mSrcFolder = mImageFolder;
            dlg.mDestFolder = backupFolder;
            dlg.mDiffTool = "";
            dlg.ShowDialog();
        }
    }
}
