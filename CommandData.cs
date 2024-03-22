using CoreLib;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace CadApp
{
    public enum ENTITY { non, all, point, line, polyline, polygon, 
                            arc, ellipse, text, parts, image, any }
    public enum OPEMODE { non, pick, loc, areaDisp, areaPick }
    public enum OPERATION { non, loc, pic,
        createPoint, createLine, createHVLine, createTangentLine,
        createRect, createPolyline, createPolygon,
        createArc, createCircle, createTangentCircle, createEllipse, createText,
        createArrow, createLabel, createSymbol, createImage,
        createLocDimension, createLinearDimension, createAngleDimension,
        createDiameterDimension, createRadiusDimension,

        translate, rotate, mirror, scale, trim, divide, stretch, offset, symbolAssemble,
        disassemble, changeText, changeRadius, changeProperty, changeProperties,

        copyTranslate, copyRotate, copyMirror, copyScale, copyTrim, copyOffset,
        copyEntity, pasteEntity, copyProperty,

        info, infoData, zumenComment,
        measure, measureDistance, measureAngle,
        remove, removeAll,
        zumenInfo, createLayer, setDispLayer, setAllDispLayer, oneLayerDisp, changeLayerName,
        setSymbol, manageSymbol,

        undo, redo,
        copyScreen, saveScreen, screenCapture, imageTrimming, print,
        cancel, close,
        back, save, saveAs, open,

        changeColor, changeThickness, changeTextSize, changePointType, changeLineType,
        color, thickness, textSize, gridSize, 
    }

    class Command
    {
        public string mainCommand;
        public string subCommand;
        public string parameter;
        public OPERATION operation;
        public ENTITY entity;

        public Command(string mainCommand, string subCommand, string parameter, OPERATION operation, ENTITY entity)
        {
            this.mainCommand = mainCommand;
            this.subCommand = subCommand;
            this.parameter = parameter;
            this.operation = operation;
            this.entity = entity;
        }
    }

    class CommandData
    {
        public List<Command> mCommandData = new List<Command>() {
            //          main,       sub,            para,  operation,                   entity
            new Command("作成",      "点",          "", OPERATION.createPoint,          ENTITY.point),
            new Command("作成",      "線分",        "", OPERATION.createLine,           ENTITY.line),
            new Command("作成",      "水平垂直線分","", OPERATION.createHVLine,         ENTITY.line),
            new Command("作成",      "接線",        "", OPERATION.createTangentLine,    ENTITY.line),
            new Command("作成",      "四角",        "", OPERATION.createRect,           ENTITY.polyline),
            new Command("作成",      "ポリライン",  "", OPERATION.createPolyline,       ENTITY.polyline),
            new Command("作成",      "ポリゴン",    "", OPERATION.createPolygon,        ENTITY.polygon),
            new Command("作成",      "円弧",        "", OPERATION.createArc,            ENTITY.arc),
            new Command("作成",      "円",          "", OPERATION.createCircle,         ENTITY.arc),
            new Command("作成",      "接円",        "", OPERATION.createTangentCircle,  ENTITY.any),
            new Command("作成",      "楕円",        "", OPERATION.createEllipse,        ENTITY.ellipse),
            new Command("作成",      "文字列",      "", OPERATION.createText,           ENTITY.text),
            new Command("作成",      "矢印",        "", OPERATION.createArrow,          ENTITY.parts),
            new Command("作成",      "ラベル",      "", OPERATION.createLabel,          ENTITY.parts),
            new Command("作成",      "シンボル",    "", OPERATION.createSymbol,         ENTITY.parts),
            new Command("作成",      "イメージ",    "", OPERATION.createImage,          ENTITY.image),
            new Command("作成",      "位置寸法線",  "", OPERATION.createLocDimension,   ENTITY.parts),
            new Command("作成",      "直線寸法線",  "", OPERATION.createLinearDimension,ENTITY.parts),
            new Command("作成",      "角度寸法線",  "", OPERATION.createAngleDimension, ENTITY.parts),
            new Command("作成",      "直径寸法線",  "", OPERATION.createDiameterDimension, ENTITY.parts),
            new Command("作成",      "半径寸法線",  "", OPERATION.createRadiusDimension, ENTITY.parts),
            new Command("作成",      "戻る",        "", OPERATION.back,                 ENTITY.non),
            new Command("編集",      "移動",        "", OPERATION.translate,            ENTITY.any),
            new Command("編集",      "回転",        "", OPERATION.rotate,               ENTITY.any),
            new Command("編集",      "反転",        "", OPERATION.mirror,               ENTITY.any),
            new Command("編集",      "拡大縮小",    "", OPERATION.scale,                ENTITY.any),
            new Command("編集",      "トリム",      "", OPERATION.trim,                 ENTITY.any),
            new Command("編集",      "分割",        "", OPERATION.divide,               ENTITY.any),
            new Command("編集",      "ストレッチ",  "", OPERATION.stretch,              ENTITY.any),
            new Command("編集",      "オフセット",  "", OPERATION.offset,               ENTITY.any),
            new Command("編集",      "シンボル変換","", OPERATION.symbolAssemble,       ENTITY.any),
            new Command("編集",      "分解",        "", OPERATION.disassemble,          ENTITY.any),
            new Command("編集",      "文字列変更",  "", OPERATION.changeText,           ENTITY.text),
            new Command("編集",      "半径変更"  ,  "", OPERATION.changeRadius,         ENTITY.arc),
            new Command("編集",      "属性変更",    "", OPERATION.changeProperty,       ENTITY.any),
            new Command("編集",      "属性一括変更","", OPERATION.changeProperties,     ENTITY.any),
            new Command("編集",      "戻る",        "", OPERATION.back,                 ENTITY.non),
            new Command("コピー",    "移動",        "", OPERATION.copyTranslate,        ENTITY.any),
            new Command("コピー",    "回転",        "", OPERATION.copyRotate,           ENTITY.any),
            new Command("コピー",    "反転",        "", OPERATION.copyMirror,           ENTITY.any),
            new Command("コピー",    "拡大縮小",    "", OPERATION.copyScale,            ENTITY.any),
            new Command("コピー",    "トリム",      "", OPERATION.copyTrim,             ENTITY.any),
            new Command("コピー",    "オフセット",  "", OPERATION.copyOffset,           ENTITY.any),
            new Command("コピー",    "属性変更",    "", OPERATION.copyProperty,         ENTITY.any),
            new Command("コピー",    "要素コピー",  "", OPERATION.copyEntity,           ENTITY.non),
            new Command("コピー",    "要素貼付け",  "", OPERATION.pasteEntity,          ENTITY.non),
            new Command("コピー",    "戻る",        "", OPERATION.back,                 ENTITY.any),
            new Command("情報",      "要素",        "", OPERATION.info,                 ENTITY.any),
            new Command("情報",      "要素データ",  "", OPERATION.infoData,             ENTITY.any),
            new Command("情報",      "図面コメント","", OPERATION.zumenComment,         ENTITY.non),
            new Command("情報",      "戻る",        "", OPERATION.back,                 ENTITY.non),
            new Command("測定",      "距離・角度",  "", OPERATION.measure,              ENTITY.any),
            new Command("測定",      "距離",        "", OPERATION.measureDistance,      ENTITY.any),
            new Command("測定",      "角度",        "", OPERATION.measureAngle,         ENTITY.any),
            new Command("測定",      "戻る",        "", OPERATION.back,                 ENTITY.any),
            new Command("削除",      "削除",        "", OPERATION.remove,               ENTITY.any),
            new Command("設定",      "図面設定",    "", OPERATION.zumenInfo,            ENTITY.non),
            new Command("設定",      "作成レイヤー","", OPERATION.createLayer,          ENTITY.non),
            new Command("設定",      "表示レイヤー","", OPERATION.setDispLayer,         ENTITY.non),
            new Command("設定",      "全レイヤー表示",   "", OPERATION.setAllDispLayer, ENTITY.non),
            new Command("設定",      "1レイヤー表示", "", OPERATION.oneLayerDisp,       ENTITY.non),
            new Command("設定",      "レイヤ名変更","", OPERATION.changeLayerName,      ENTITY.non),
            new Command("設定",      "シンボル登録","", OPERATION.setSymbol,            ENTITY.parts),
            new Command("設定",      "シンボル管理","", OPERATION.manageSymbol,         ENTITY.non),
            new Command("設定",      "戻る",        "", OPERATION.back,                 ENTITY.non),
            new Command("アンドゥ",  "アンドゥ",    "", OPERATION.undo,                 ENTITY.non),
            //new Command("リドゥ",    "リドゥ",      "", OPERATION.redo,            ENTITY.non),
            //new Command("ファイル",  "上書き保存",  "", OPERATION.save,            ENTITY.non),
            //new Command("ファイル",  "保存",        "", OPERATION.saveAs,          ENTITY.non),
            new Command("ツール",    "画面コピー",  "", OPERATION.copyScreen,           ENTITY.non),
            new Command("ツール",    "画面保存",    "", OPERATION.saveScreen,           ENTITY.non),
            new Command("ツール",    "スクリーンキャプチャ","", OPERATION.screenCapture,ENTITY.non),
            new Command("ツール",    "イメージトリミング","", OPERATION.imageTrimming,  ENTITY.non),
            new Command("ツール",    "印刷",        "", OPERATION.print,                ENTITY.non),
            new Command("ツール",    "戻る",        "", OPERATION.back,                 ENTITY.non),
            new Command("キャンセル","キャンセル",  "", OPERATION.cancel,               ENTITY.non),
            new Command("終了",      "終了",        "", OPERATION.close,                ENTITY.non),
        };

        public int mCommandLevel = 0;
        public string mMain = "";
        public string mSub = "";
        public Dictionary<Key, OPERATION> mShortCutList = new Dictionary<Key, OPERATION>() {
            { Key.A, OPERATION.createLine },
            { Key.Q, OPERATION.createText },
            { Key.S, OPERATION.save },
            { Key.Z, OPERATION.undo},
        };
        private string[] mShortCutFormat = { "Key", "Command" };
        private List<string> mShortCutComment = new List<string>() {
            "ショートカットキーの設定ファイル",
            "Command",
            "createPoint, createLine, createHVLine, createTangentLine",
            "createRect, createPolyline, createPolygon",
            "createArc, createCircle, createTangentCircle, createEllipse, createText",
            "createArrow, createLabel, createSymbol, createImage",
            "createLocDimension, createDimension, createAngleDimension",
            "createDiameterDimension, createRadiusDimension",
            "translate, rotate, mirror, scale, trim, divide, stretch, offset, symbolAssemble",
            "disassemble, changeText, changeRadius, changeProperty, changeProperties",
            "copyTranslate, copyRotate, copyMirror, copyScale, copyTrim, copyOffset",
            "copyEntity, pasteEntity, copyProperty",
            "info, infoData, zumenComment",
            "measure, measureDistance, measureAngle",
            "remove",
            "zumenInfo, createLayer, setDispLayer, setAllDispLayer, oneLayerDisp, changeLayerName",
            "setSymbol, manageSymbol",
            "undo",
            "copyScreen, saveScreen, screenCapture, print",
            "cancel, close",
            "back, save, saveAs, open, gridSize",
            "",
        };

        private YLib ylib = new YLib();

        /// <summary>
        /// メインコマンドのリスト取得
        /// </summary>
        /// <returns>コマンドリスト</returns>
        public List<string> getMainCommand()
        {
            List<string> main = new List<string>();
            foreach (var cmd in mCommandData) {
                if (!main.Contains(cmd.mainCommand) && cmd.mainCommand != "")
                    main.Add(cmd.mainCommand);
            }
            return main;
        }

        /// <summary>
        /// サブコマンドのリスト取得
        /// </summary>
        /// <param name="main">メインコマンド名</param>
        /// <returns>コマンドリスト</returns>
        public List<string> getSubCommand(string main)
        {
            List<string> sub = new List<string>();
            foreach (var cmd in mCommandData) {
                if (cmd.mainCommand == main || cmd.mainCommand == "") {
                    if (!sub.Contains(cmd.subCommand))
                        sub.Add(cmd.subCommand);
                }
            }
            return sub;
        }

        /// <summary>
        /// コマンドデータ取得
        /// </summary>
        /// <param name="main">メインコマンド名</param>
        /// <param name="sub">サブコマンド名</param>
        /// <param name="para">パラメータ名</param>
        /// <returns>コマンドデータ</returns>
        public Command getCommand(string main, string sub = "", string para = "")
        {
            if (sub == "" && para == "") {
                foreach (var cmd in mCommandData)
                    if (cmd.mainCommand == main)
                        return cmd;
            } else if (para == "") {
                foreach (var cmd in mCommandData)
                    if (cmd.mainCommand == main && cmd.subCommand == sub)
                        return cmd;
            } else {
                foreach (var cmd in mCommandData)
                    if (cmd.mainCommand == main && cmd.subCommand == sub && cmd.parameter == para)
                        return cmd;
            }
            return new Command("","","",OPERATION.non, ENTITY.non);
        }

        /// <summary>
        /// 文字列をenum型OPERATIONに変換
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public OPERATION str2Operatin(string str)
        {
            OPERATION ope;
            if (Enum.TryParse(str, out ope))
                return ope;
            else
                return OPERATION.non;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Key str2Key(string c)
        {
            Key key;
            if (Enum.TryParse(c, out key))
                return key;
            else
                return Key.None;
        }

        /// <summary>
        /// enum型OPERATIONを文字列ですべて取得
        /// </summary>
        /// <returns></returns>
        public List<string> getOperationCommandList()
        {
            List<string> list = new List<string>();
            //  enum(列挙型)のすべての値や名前を取得
            foreach (var enumval in Enum.GetValues(typeof(OPERATION))) {
                list.Add(enumval.ToString());
                //Console.WriteLine($"{enumval} {Enum.GetName(typeof(OPERATION), enumval)}");
            }
            return list;
        }

        /// <summary>
        /// ShortCutキーファイルの読込
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void loadShortCut(string path)
        {
            List<string[]> shortcutList = ylib.loadCsvData(path, mShortCutFormat, false, false);
            if (0 < shortcutList.Count) {
                mShortCutList.Clear();
                foreach (var shortcut in shortcutList) {
                    if (1 < shortcut.Length)
                        mShortCutList.Add(str2Key(shortcut[0]), str2Operatin(shortcut[1]));
                }
            }
        }

        /// <summary>
        /// ShortCutキーファイル-保存
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void saveShortCut(string path)
        {
            List<string[]> shortcutList = new List<string[]>();
            foreach (var keyvalue in mShortCutList) {
                string[] buf = new string[] {
                    keyvalue.Key.ToString(), keyvalue.Value.ToString()
                };
                shortcutList.Add(buf);
            }
            ylib.saveCsvData(path, mShortCutFormat, shortcutList, mShortCutComment);
        }
    }
}
