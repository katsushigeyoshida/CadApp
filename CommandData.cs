using System.Collections.Generic;

namespace CadApp
{
    public enum ENTITY { non, all, point, line, polyline, polygon, arc, ellipse, text, parts, any }
    public enum OPERATION { non, loc, pic,
        createPoint, createLine, createRect, createPolyline, createPolygon, 
        createArc, createCircle, createTangentCircle, createEllipse, createText, createArrow,
        createLabel, createLocDimension, createDimension, createAngleDimension,
        createDiameterDimension, createRadiusDimension,

        translate, rotate, mirror, trim, divide, stretch, offset,
        copyTranslate, copyRotate, copyMirror, copyOffset,
        disassemble, textChange, radiusChange, changeProperty, changeProperties,

        info, infoData,
        measureDistance, measureAngle,
        remove, removeAll,
        zumenInfo,
        undo, redo,
        back, cancel, close,
        save, saveAs, open, screenCopy, entityCopy, entityPaste,
        colorChange, thicknessChange, textSizeChange, pointTypeChange, lineTypeChange,
        color, thickness, textSize, gridSize,
        allClear,
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
            new Command("作成",      "四角",        "", OPERATION.createRect,           ENTITY.line),
            new Command("作成",      "ポリライン",  "", OPERATION.createPolyline,       ENTITY.polyline),
            new Command("作成",      "ポリゴン",    "", OPERATION.createPolygon,        ENTITY.polygon),
            new Command("作成",      "円弧",        "", OPERATION.createArc,            ENTITY.arc),
            new Command("作成",      "円",          "", OPERATION.createCircle,         ENTITY.arc),
            new Command("作成",      "接円",        "", OPERATION.createTangentCircle,  ENTITY.any),
            //new Command("作成",      "楕円",        "", OPERATION.createEllipse,   ENTITY.ellipse),
            new Command("作成",      "文字列",      "", OPERATION.createText,           ENTITY.text),
            new Command("作成",      "矢印",        "", OPERATION.createArrow,          ENTITY.parts),
            new Command("作成",      "ラベル",      "", OPERATION.createLabel,          ENTITY.parts),
            new Command("作成",      "位置寸法線",  "", OPERATION.createLocDimension,   ENTITY.parts),
            new Command("作成",      "寸法線",      "", OPERATION.createDimension,      ENTITY.parts),
            new Command("作成",      "角度寸法線",  "", OPERATION.createAngleDimension, ENTITY.parts),
            new Command("作成",      "直径寸法線",  "", OPERATION.createDiameterDimension, ENTITY.parts),
            new Command("作成",      "半径寸法線",  "", OPERATION.createRadiusDimension, ENTITY.parts),
            new Command("作成",      "戻る",        "", OPERATION.back,                 ENTITY.non),
            new Command("編集",      "移動",        "", OPERATION.translate,            ENTITY.any),
            new Command("編集",      "回転",        "", OPERATION.rotate,               ENTITY.any),
            new Command("編集",      "反転",        "", OPERATION.mirror,               ENTITY.any),
            new Command("編集",      "トリム",      "", OPERATION.trim,                 ENTITY.any),
            new Command("編集",      "分割",        "", OPERATION.divide,               ENTITY.any),
            new Command("編集",      "ストレッチ",  "", OPERATION.stretch,              ENTITY.any),
            new Command("編集",      "オフセット",  "", OPERATION.offset,               ENTITY.any),
            new Command("編集",      "分解",        "", OPERATION.disassemble,          ENTITY.any),
            new Command("編集",      "文字列変更",  "", OPERATION.textChange,           ENTITY.text),
            new Command("編集",      "半径変更"  ,  "", OPERATION.radiusChange,         ENTITY.arc),
            new Command("編集",      "属性変更",    "", OPERATION.changeProperty,       ENTITY.any),
            new Command("編集",      "属性一括変更","", OPERATION.changeProperties,     ENTITY.any),
            new Command("編集",      "戻る",        "", OPERATION.back,                 ENTITY.non),
            new Command("コピー",    "移動",        "", OPERATION.copyTranslate,        ENTITY.any),
            new Command("コピー",    "回転",        "", OPERATION.copyRotate,           ENTITY.any),
            new Command("コピー",    "反転",        "", OPERATION.copyMirror,           ENTITY.any),
            new Command("コピー",    "オフセット",  "", OPERATION.copyOffset,           ENTITY.any),
            new Command("コピー",    "画面コピー",  "", OPERATION.screenCopy,           ENTITY.non),
            new Command("コピー",    "要素コピー",  "", OPERATION.entityCopy,           ENTITY.non),
            new Command("コピー",    "要素貼付け",  "", OPERATION.entityPaste,           ENTITY.non),
            new Command("コピー",    "戻る",        "", OPERATION.back,                 ENTITY.any),
            new Command("情報",      "要素",        "", OPERATION.info,                 ENTITY.any),
            new Command("情報",      "要素データ",  "", OPERATION.infoData,             ENTITY.any),
            new Command("情報",      "戻る",        "", OPERATION.back,                 ENTITY.non),
            new Command("測定",      "距離・角度",  "", OPERATION.measureDistance,      ENTITY.any),
            //new Command("測定",      "角度",        "", OPERATION.measureAngle,    ENTITY.line),
            new Command("測定",      "戻る",        "", OPERATION.back,                 ENTITY.non),
            new Command("削除",      "削除",        "", OPERATION.remove,               ENTITY.any),
            new Command("設定",      "図面設定",    "", OPERATION.zumenInfo,            ENTITY.non),
            new Command("設定",      "戻る",        "", OPERATION.back,                 ENTITY.non),
            new Command("アンドゥ",  "アンドゥ",    "", OPERATION.undo,                 ENTITY.non),
            //new Command("リドゥ",    "リドゥ",      "", OPERATION.redo,            ENTITY.non),
            //new Command("ファイル",  "上書き保存",  "", OPERATION.save,            ENTITY.non),
            //new Command("ファイル",  "保存",        "", OPERATION.saveAs,          ENTITY.non),
            new Command("キャンセル","キャンセル",  "", OPERATION.cancel,               ENTITY.non),
            new Command("終了",      "終了",        "", OPERATION.close,                ENTITY.non),
        };

        public int mCommandLevel = 0;
        public string mMain = "";
        public string mSub = "";

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
    }
}
