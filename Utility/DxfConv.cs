using CoreLib;
using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace CadApp
{
    /// <summary>
    /// DXF 変換
    /// DXF Import : CoreLib\DxfReader を使ってDXFデータを読み込む
    /// DXF Export : netDxf を使って DXFファイルを出力する
    /// </summary>
    public class DxfConv
    {
        private double mEps = 1e-8;
        private YLib ylib = new YLib();

        /// <summary>
        /// CADファイルデータをDXFファイルに変換する
        /// (netDxfを使用して図面データをDXFファイルに変換する)
        /// netDxf : https://github.com/haplokuon/netDxf
        /// </summary>
        /// <param name="filePath">CADファイルパス</param>
        /// <param name="itemPath">DXFファイルパス</param>
        public void exportDxf(string filePath, string itemPath)
        {
            EntityData entityData = new EntityData();
            entityData.loadData(itemPath);
            //  create a new document, by default it will create an AutoCad2000 DXF version
            DxfDocument doc = new DxfDocument();
            //  線種の登録
            doc.Linetypes.Add(netDxf.Tables.Linetype.Dashed);
            doc.Linetypes.Add(netDxf.Tables.Linetype.Center);
            doc.Linetypes.Add(netDxf.Tables.Linetype.DashDot);
            //  データの変換
            doc = setDxfEnt(doc, entityData);
            if (File.Exists(filePath)) {
                //if (ylib.messageBox(null, "ファイルが既に存在します。上書きしてもよいですか?", "", "確認", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                //    return ;
                File.Delete(filePath);
            }
            doc.Save(filePath);
        }

        /// <summary>
        /// netDXFを使ってDXFデータに変換
        /// </summary>
        /// <param name="doc">netDXF Document</param>
        /// <param name="entityData">要素データリスト</param>
        /// <returns>netDXF Document</returns>
        private DxfDocument setDxfEnt(DxfDocument doc, EntityData entityData)
        {
            for (int i = 0; i < entityData.mEntityList.Count; i++) {
                switch (entityData.mEntityList[i].mEntityId) {
                    case EntityId.Line: {
                            LineEntity line = (LineEntity)entityData.mEntityList[i];
                            Line entityLine = setDxfLine(line.mLine);
                            entityLine.Color = brushes2AciColor(line.mColor);
                            entityLine.Linetype = linetype2netDxf(line.mType);
                            doc.Entities.Add(entityLine);
                        }
                        break;
                    case EntityId.Arc: {
                            ArcEntity arc = (ArcEntity)entityData.mEntityList[i];
                            Arc entityArc = setDxfArc(arc.mArc);
                            entityArc.Color = brushes2AciColor(arc.mColor);
                            entityArc.Linetype = linetype2netDxf(arc.mType);
                            doc.Entities.Add(entityArc);
                        }
                        break;
                    case EntityId.Ellipse: {
                            EllipseEntity ellipse = (EllipseEntity)entityData.mEntityList[i];
                            Ellipse entityEllipse = setDxfEllipse(ellipse.mEllipse);
                            entityEllipse.Color = brushes2AciColor(ellipse.mColor);
                            entityEllipse.Linetype = linetype2netDxf(ellipse.mType);
                            doc.Entities.Add(entityEllipse);
                        }
                        break;
                    case EntityId.Polyline: {
                            PolylineEntity polyline = (PolylineEntity)entityData.mEntityList[i];
                            Polyline2D entityPolyline = setDxfPolyline(polyline.mPolyline);
                            entityPolyline.Color = brushes2AciColor(polyline.mColor);
                            entityPolyline.Linetype = linetype2netDxf(polyline.mType);
                            doc.Entities.Add(entityPolyline);
                        }
                        break;
                    case EntityId.Polygon: {
                            PolygonEntity polygon = (PolygonEntity)entityData.mEntityList[i];
                            Polyline2D entityPolyline = setDxfPolygon(polygon.mPolygon);
                            entityPolyline.Color = brushes2AciColor(polygon.mColor);
                            entityPolyline.Linetype = linetype2netDxf(polygon.mType);
                            doc.Entities.Add(entityPolyline);
                        }
                        break;
                    case EntityId.Text: {
                            TextEntity text = (TextEntity)entityData.mEntityList[i];
                            string[] mtext = text.mText.mText.Split('\n');
                            for (int j = 0; j < mtext.Length; j++) {
                                mtext[j] = mtext[j].Replace("\r", "");
                                Text entityText = setDxfText(mtext[j], text.mText);
                                entityText.Color = brushes2AciColor(text.mColor);
                                doc.Entities.Add(entityText);
                                text.mText.mPos.y -= text.mText.mTextSize * text.mText.mLinePitchRate;
                            }
                        }
                        break;
                    case EntityId.Parts: {
                            PartsEntity parts = (PartsEntity)entityData.mEntityList[i];
                            doc = setDxfParts(doc, parts.mParts, parts.mColor, parts.mType);
                        }
                        break;
                }
            }
            return doc;
        }

        /// <summary>
        /// 線分を netDXFのLineに変換
        /// </summary>
        /// <param name="line">LineD</param>
        /// <returns>Line</returns>
        private Line setDxfLine(LineD line)
        {
            Vector2 sp = new Vector2(line.ps.x, line.ps.y);
            Vector2 ep = new Vector2(line.pe.x, line.pe.y);
            return new Line(sp, ep);
        }

        /// <summary>
        /// 円弧を netDXFのArcに変換
        /// </summary>
        /// <param name="arc">ArcD</param>
        /// <returns>Arc</returns>
        private Arc setDxfArc(ArcD arc)
        {
            Vector2 cp = new Vector2(arc.mCp.x, arc.mCp.y);
            return new Arc(cp, arc.mR, ylib.R2D(arc.mSa), ylib.R2D(arc.mEa));
        }

        /// <summary>
        /// 楕円を netDXFのEllipseに変換
        /// </summary>
        /// <param name="ellipse">EllipseD</param>
        /// <returns>Ellipse</returns>
        private Ellipse setDxfEllipse(EllipseD ellipse)
        {
            Vector2 cp = new Vector2(ellipse.mCp.x, ellipse.mCp.y);
            Ellipse elli;
            if(ellipse.mRx > ellipse.mRy) {
                elli = new Ellipse(cp, ellipse.mRx * 2, ellipse.mRy * 2);
                elli.Rotation = ylib.R2D(ellipse.mRotate);
            } else {
                elli = new Ellipse(cp, ellipse.mRy * 2, ellipse.mRx * 2);
                elli.Rotation = ylib.R2D(ellipse.mRotate + Math.PI / 2);
                ellipse.mSa -= Math.PI / 2;
                ellipse.mEa -= Math.PI / 2;
            }
            elli.StartAngle = ylib.R2D(ellipse.mSa);
            elli.EndAngle = ylib.R2D(ellipse.mEa);
            return elli;
        }

        /// <summary>
        /// ポリラインを netDXFのPolyline2Dに変換
        /// </summary>
        /// <param name="polyline">PolylineD</param>
        /// <returns>Polyline2D</returns>
        private Polyline2D setDxfPolyline(PolylineD polyline)
        {
            List<Vector2> plist = new List<Vector2>();
            foreach (var p in polyline.mPolyline)
                plist.Add(new Vector2(p.x, p.y));
            return new Polyline2D(plist);
        }

        /// <summary>
        /// ポリゴンを netDXFのPolyline2Dに変換
        /// </summary>
        /// <param name="polygon">PolygonD</param>
        /// <returns>Polyline2D</returns>
        private Polyline2D setDxfPolygon(PolygonD polygon)
        {
            List<Vector2> plist = new List<Vector2>();
            foreach (var p in polygon.mPolygon)
                plist.Add(new Vector2(p.x, p.y));
            return new Polyline2D(plist, true);
        }

        /// <summary>
        /// テキストを netDXFのTextに変換
        /// </summary>
        /// <param name="text">テキスト</param>
        /// <param name="textPara">TextD(パラメータ)</param>
        /// <returns>Text</returns>
        private Text setDxfText(string text,　TextD textPara)
        {
            text = text.Replace("\r", "");
            Text entText = new Text(text, new Vector2(textPara.mPos.x, textPara.mPos.y), textPara.mTextSize);
            entText.Rotation = ylib.R2D(textPara.mRotate);
            entText.Alignment = setTextAliment(textPara.mHa, textPara.mVa);
            return entText;
        }

        /// <summary>
        /// Parts要素の変換
        /// </summary>
        /// <param name="doc">netDXF Document</param>
        /// <param name="parts">Parts要素</param>
        /// <param name="color">色</param>
        /// <param name="type">線種</param>
        /// <returns>netDXF Document</returns>
        private DxfDocument setDxfParts(DxfDocument doc, PartsD parts, Brush color, int type)
        {
            foreach (var line in parts.mLines) {
                Line entityLine = setDxfLine(line);
                entityLine.Color = brushes2AciColor(color);
                entityLine.Linetype = linetype2netDxf(type);
                doc.Entities.Add(entityLine);
            }
            foreach (var arc in parts.mArcs) {
                Arc entityArc = setDxfArc(arc);
                entityArc.Color = brushes2AciColor(color);
                entityArc.Linetype = linetype2netDxf(type);
                doc.Entities.Add(entityArc);
            }
            foreach (var text in parts.mTexts) {
                string[] mtext = text.mText.Split('\n');
                for (int j = 0; j < mtext.Length; j++) {
                    mtext[j] = mtext[j].Replace("\r", "");
                    Text entityText = setDxfText(mtext[j], text);
                    entityText.Color = brushes2AciColor(color);
                    doc.Entities.Add(entityText);
                    text.mPos.y -= text.mTextSize * text.mLinePitchRate;
                }
            }
            return doc;
        }

        /// <summary>
        /// TEXTアライメントをDXFのアライメントに変換
        /// </summary>
        /// <param name="ha">水平アライメント</param>
        /// <param name="va">垂直アライメント</param>
        /// <returns>DXFアライメント</returns>
        private netDxf.Entities.TextAlignment setTextAliment(HorizontalAlignment ha, VerticalAlignment va)
        {
            if (ha == HorizontalAlignment.Left) {
                if (va == VerticalAlignment.Top)
                    return netDxf.Entities.TextAlignment.TopLeft;
                else if (va == VerticalAlignment.Center)
                    return netDxf.Entities.TextAlignment.MiddleLeft;
                else if (va == VerticalAlignment.Bottom)
                    return netDxf.Entities.TextAlignment.BottomLeft;
            } else if (ha == HorizontalAlignment.Center) {
                if (va == VerticalAlignment.Top)
                    return netDxf.Entities.TextAlignment.TopCenter;
                else if (va == VerticalAlignment.Center)
                    return netDxf.Entities.TextAlignment.MiddleCenter;
                else if (va == VerticalAlignment.Bottom)
                    return netDxf.Entities.TextAlignment.BottomCenter;
            } else if (ha == HorizontalAlignment.Right) {
                if (va == VerticalAlignment.Top)
                    return netDxf.Entities.TextAlignment.TopRight;
                else if (va == VerticalAlignment.Center)
                    return netDxf.Entities.TextAlignment.MiddleRight;
                else if (va == VerticalAlignment.Bottom)
                    return netDxf.Entities.TextAlignment.BottomRight;
            }
            return netDxf.Entities.TextAlignment.TopLeft;
        }

        /// <summary>
        /// 色変換(Brush → AciColor)
        /// </summary>
        /// <param name="color">Brush</param>
        /// <returns>AciColor</returns>
        private AciColor brushes2AciColor(System.Windows.Media.Brush color)
        {
            SolidColorBrush solid = color as SolidColorBrush;
            return new AciColor(solid.Color.R, solid.Color.G, solid.Color.B);
        }

        /// <summary>
        /// 線種の変換
        /// </summary>
        /// <param name="linetype">線種</param>
        /// <returns>LineType</returns>
        private netDxf.Tables.Linetype linetype2netDxf(int linetype)
        {
            switch (linetype) {
                case 0: return netDxf.Tables.Linetype.Continuous;
                case 1: return netDxf.Tables.Linetype.Dashed;
                case 2: return netDxf.Tables.Linetype.Center;
                case 3: return netDxf.Tables.Linetype.DashDot;
                default: return netDxf.Tables.Linetype.Continuous;
            }
        }


        /// <summary>
        /// DXFファイルのインポート
        /// CoreLibの DxfReader を使ってDXFファイルを読み込む
        /// </summary>
        /// <param name="dxfPath">DXFファイルパス</param>
        /// <param name="outPath">出力ファイルパス</param>
        /// <returns>DXFファイル名(拡張子なし)</returns>
        public string importDxf(string dxfPath, string outPath)
        {
            //  DXFファイルの読込
            DxfReader dxfReader = new DxfReader(dxfPath);
            //  DXFデータをEntityDataに変換
            EntityData entityData = new EntityData();
            foreach (var ent in dxfReader.mEntityList) {
                switch (ent.mEntityName) {
                    case "LINE":
                        LineD line = ent.getLine();
                        Entity lineEntity = new LineEntity(line);
                        lineEntity.mColor = dxfColorNo2Brsh(ent.mColor);
                        lineEntity.mType = dxfLineType2No(ent.mLineType);
                        entityData.mEntityList.Add(lineEntity);
                        break;
                    case "ARC":
                    case "CIRCLE":
                        ArcD arc = ent.getArc();
                        Entity arcEntity = new ArcEntity(arc);
                        arcEntity.mColor = dxfColorNo2Brsh(ent.mColor);
                        arcEntity.mType = dxfLineType2No(ent.mLineType);
                        entityData.mEntityList.Add(arcEntity);
                        break;
                    case "LWPOLYLINE":
                    case "POLYLINE":
                        PolylineD polyline = ent.getPolyline();
                        Entity polylineEntity = new PolylineEntity(polyline);
                        polylineEntity.mColor = dxfColorNo2Brsh(ent.mColor);
                        polylineEntity.mType = dxfLineType2No(ent.mLineType);
                        entityData.mEntityList.Add(polylineEntity);
                        break;
                    case "ELLIPSE":
                        EllipseD ellipse = ent.getEllipse();
                        Entity ellipseEntity = new EllipseEntity(ellipse);
                        ellipseEntity.mColor = dxfColorNo2Brsh(ent.mColor);
                        ellipseEntity.mType = dxfLineType2No(ent.mLineType);
                        entityData.mEntityList.Add(ellipseEntity);
                        break;
                    case "DIMENSION":
                        break;
                    case "MTEXT":
                    case "TEXT":
                        TextD text = ent.getText();
                        Entity textEntity = new TextEntity(text);
                        textEntity.mColor = dxfColorNo2Brsh(ent.mColor);
                        entityData.mEntityList.Add(textEntity);
                        break;
                }
            }
            entityData.updateData();
            //  CSVファイルに保存
            if (File.Exists(outPath)) {
                if (ylib.messageBox(null, "ファイルが既に存在します。上書きしてもよいですか?", "", "確認", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                    return "";
            }
            entityData.saveData(outPath);

            return Path.GetFileNameWithoutExtension(dxfPath);
        }

        /// <summary>
        /// DXFの色番号をBrushに変換
        /// </summary>
        /// <param name="color">DXF色番号</param>
        /// <returns>Brush</returns>
        private Brush dxfColorNo2Brsh(int color)
        {
            Brush[] brush = new Brush[] {
                Brushes.Black, Brushes.Red, Brushes.Yellow, Brushes.Green, Brushes.Cyan, Brushes.Blue, Brushes.Magenta, Brushes.White,
                Brushes.Gray, Brushes.DarkRed, Brushes.GreenYellow, Brushes.DarkGreen, Brushes.DarkBlue, Brushes.DarkMagenta, Brushes.WhiteSmoke,
            };
            if (color < brush.Length)
                return brush[color];
            else
                return Brushes.Black;
        }

        /// <summary>
        /// DXFの線種を線種番号に変換
        /// </summary>
        /// <param name="linetype">DXF線種</param>
        /// <returns>線種No</returns>
        private int dxfLineType2No(string linetype)
        {
            switch (linetype) {
                case "Continuous": return 0;
                case "Dashed": return 1;
                case "Center": return 2;
                case "DashDot": return 3;
                default: return 0;
            }
        }
    }
}
