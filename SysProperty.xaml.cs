using CoreLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CadApp
{
    /// <summary>
    /// SysProperty.xaml の相互作用ロジック
    /// 
    /// 図面パラメータ設定ダイヤログ
    /// </summary>
    public partial class SysProperty : Window
    {
        private List<string> mPointTypeMenu = new List<string>() {
            "・点", "X クロス", "+ 十字", "□ 四角", "〇 円", "△ 三角"
        };
        private List<string> mLineTypeMenu = new List<string>() {
            "実線", "破線", "一点鎖線", "二点鎖線"
        };
        private double[] mTextSizeMenu = {
            0.1, 0.2, 0.5, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16,
            18, 20, 24, 28, 32, 36, 40, 48, 56, 64, 96, 128, 196, 256, 384,
            512, 768, 1024, 1536, 2048, 3072, 4096, 6144, 8192, 12288, 16384
        };
        private double[] mTextRotateMenu = {
            0, 30, 45, 60, 90, 120, 135, 150, 180, 210, 225, 240, 270, 300, 315, 330
        };
        private double[] mEntSizeMenu = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10
        };
        private double[] mArrowAngleMenu = {
            5, 10 , 15, 20, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90
        };
        private double[] mGridSizeMenu = {
            0, 0.1, 0.2, 0.5, 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000
        };
        private string[] mHorizontalAlignmentMenu = { "Left", "Center", "Right" };
        private string[] mVerticalAlignmentMenu = { "Top", "Center", "Bottum" };
        private List<string> mFontFamilyMenu;
        private string[] mFontStyleMenu = { "Normal", "Italic", "Oblique" };
        private string[] mFontWeightMenu = { "Normal", "Thin", "Bold" };

        public string mColorName = "Black";
        public Brush mColor = Brushes.Black;
        public bool mBackDisp = false;
        public int mLineType = 0;
        public double mThickness = 1;
        public int mPointType = 0;
        public double mPointSize = 1;
        public double mTextSize = 12;
        public double mTextRotate = 0;
        public double mLinePitchRate = 1.2;
        public HorizontalAlignment mHa = HorizontalAlignment.Left;
        public VerticalAlignment mVa = VerticalAlignment.Top;
        public double mArrowSize = 6;
        public double mArrowAngle = 30 * Math.PI / 180;
        public string mLayerName = "";
        public List<string> mLayerNameList;
        public string mFontFamily = "";
        public string mFontStyle = "";
        public string mFontWeight = "";
        public string mFillColorName = "Beige";
        public Brush mFillColor = Brushes.Beige;
        public bool mFillColorOn = false;
        public double mGridSize = 1;
        public Brush mBackColor = Brushes.White;

        public bool mShowCheckBox     = false;
        public bool mColorChk         = false;
        public bool mBackDispChk      = false;
        public bool mLineTypeChk      = false;
        public bool mThicknessChk     = false;
        public bool mPointTypeChk     = false;
        public bool mPointSizeChk     = false;
        public bool mTextSizeChk      = false;
        public bool mTextRotateChk    = false;
        public bool mLinePitchRateChk = false;
        public bool mHaChk            = false;
        public bool mVaChk            = false;
        public bool mArrowSizeChk     = false;
        public bool mArrowAngleChk    = false;
        public bool mLayerNameChk     = false;
        public bool mFontFamilyChk    = false;
        public bool mFontStyleChk     = false;
        public bool mFontWeightChk    = false;
        public bool mFillColorChk     = false;


        YDraw ydraw = new YDraw();
        YLib ylib = new YLib();

        public SysProperty()
        {
            InitializeComponent();

            cbColor.DataContext = ydraw.mColorList;
            cbFillColor.DataContext = ylib.mBrushList;
            cbBackColor.DataContext = ylib.mBrushList;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbLineType.ItemsSource   = mLineTypeMenu;
            cbPointType.ItemsSource  = mPointTypeMenu;
            cbThickness.ItemsSource  = mEntSizeMenu;
            cbPointSize.ItemsSource  = mEntSizeMenu;
            cbTextSize.ItemsSource   = mTextSizeMenu;
            cbTextRotate.ItemsSource = mTextRotateMenu;
            cbHorizontal.ItemsSource = mHorizontalAlignmentMenu;
            cbVertical.ItemsSource   = mVerticalAlignmentMenu;
            cbArrowSize.ItemsSource  = mTextSizeMenu;
            cbArrowAngle.ItemsSource = mArrowAngleMenu;
            cbLayerName.ItemsSource  = mLayerNameList;
            cbGridSize.ItemsSource   = mGridSizeMenu;

            cbColor.SelectedIndex      = ydraw.mColorList.FindIndex(p => p.brush == mColor);
            chBackDisp.IsChecked       = mBackDisp;
            cbLineType.SelectedIndex   = mLineType;
            cbPointType.SelectedIndex  = mPointType;
            cbThickness.Text           = mThickness.ToString();
            cbPointSize.Text           = mPointSize.ToString();
            cbTextSize.Text            = mTextSize.ToString();
            cbTextRotate.Text          = ylib.R2D(mTextRotate).ToString();
            tbLinerPitchRate.Text      = mLinePitchRate.ToString();
            cbHorizontal.SelectedIndex = mHa == HorizontalAlignment.Left ? 0 :
                                         mHa == HorizontalAlignment.Center ? 1 : 2;
            cbVertical.SelectedIndex   = mVa == VerticalAlignment.Top ? 0 :
                                         mVa == VerticalAlignment.Center ? 1 : 2;
            mFontFamilyMenu            = ylib.getSystemFontFamilyName();
            cbFontFamily.ItemsSource   = mFontFamilyMenu;
            cbFontStyle.ItemsSource    = mFontStyleMenu;
            cbFontWeight.ItemsSource   = mFontWeightMenu;

            cbArrowSize.Text           = mArrowSize.ToString();
            cbArrowAngle.Text          = ylib.double2StrZeroSup(ylib.R2D(mArrowAngle));
            cbGridSize.SelectedIndex   = mGridSizeMenu.FindIndex(p => p >= mGridSize);
            cbFontFamily.SelectedIndex = mFontFamilyMenu.IndexOf(mFontFamily == "" ? SystemFonts.MessageFontFamily.Source : mFontFamily);
            cbFontStyle.SelectedIndex  = mFontStyleMenu.FindIndex(p =>p == mFontStyle);
            cbFontWeight.SelectedIndex = mFontWeightMenu.FindIndex(p => p == mFontWeight);
            cbFillColor.SelectedIndex  = ylib.mBrushList.FindIndex(p => p.brush.ToString() == mFillColor.ToString());
            chFillColorOn.IsChecked    = mFillColorOn;
            cbBackColor.SelectedIndex  = ylib.mBrushList.FindIndex(p => p.brush.ToString() == mBackColor.ToString());

            chColor.Visibility         = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            lbBackDispTitle.Visibility = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chBackDisp.Visibility      = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chBackDispChk.Visibility   = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chLineType.Visibility      = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chPointType.Visibility     = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chThickness.Visibility     = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chPointSize.Visibility     = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chTextSize.Visibility      = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chTextRotate.Visibility    = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chLinePitchRate.Visibility = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chHorizontal.Visibility    = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chVertical.Visibility      = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chArrowSize.Visibility     = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chArrowAngle.Visibility    = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            lbLayerTitle.Visibility    = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            cbLayerName.Visibility     = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chLayerName.Visibility     = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            lbGridTitle.Visibility     = mShowCheckBox ? Visibility.Hidden : Visibility.Visible;
            cbGridSize.Visibility      = mShowCheckBox ? Visibility.Hidden : Visibility.Visible;
            chFontFamily.Visibility    = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chFontStyle.Visibility     = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chFontWeight.Visibility    = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            lbFillColorTitle.Visibility = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            cbFillColor.Visibility      = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chFillColorOn.Visibility    = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            chFillColor.Visibility      = mShowCheckBox ? Visibility.Visible : Visibility.Hidden;
            lbBackColorTitle.Visibility = mShowCheckBox ? Visibility.Hidden : Visibility.Visible;
            cbBackColor.Visibility      = mShowCheckBox ? Visibility.Hidden : Visibility.Visible;

            chColor.IsChecked         = mColorChk;
            chLineType.IsChecked      = mLineTypeChk;
            chPointType.IsChecked     = mPointTypeChk;
            chThickness.IsChecked     = mThicknessChk;
            chPointSize.IsChecked     = mPointSizeChk;
            chTextSize.IsChecked      = mTextSizeChk;
            chTextRotate.IsChecked    = mTextRotateChk;
            chLinePitchRate.IsChecked = mLinePitchRateChk;
            chHorizontal.IsChecked    = mHaChk;
            chVertical.IsChecked      = mVaChk;
            chFontFamily.IsChecked    = mFontFamilyChk;
            chFontStyle.IsChecked     = mFontStyleChk;
            chFontWeight.IsChecked    = mFontWeightChk;
            chFillColor.IsChecked     = mFillColorChk;
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            if (0 <= cbColor.SelectedIndex) {
                mColor = ydraw.mColorList[cbColor.SelectedIndex].brush;
                mColorName = ydraw.mColorList[cbColor.SelectedIndex].colorTitle;
            }
            mBackDisp = chBackDisp.IsChecked == true;
            if (0 <= cbLineType.SelectedIndex)
                mLineType = cbLineType.SelectedIndex;
            if (0 < cbThickness.Text.Length)
                mThickness = ylib.string2double(cbThickness.Text);
            if (0 <= cbPointType.SelectedIndex)
                mPointType = cbPointType.SelectedIndex;
            if (0 < cbPointSize.Text.Length)
                mPointSize = ylib.string2double(cbPointSize.Text);

            if (0 < cbTextSize.Text.Length)
                mTextSize = ylib.string2double(cbTextSize.Text);
            if (0 < cbTextRotate.Text.Length)
                mTextRotate = ylib.D2R(ylib.string2double(cbTextRotate.Text));
            mLinePitchRate = ylib.string2double(tbLinerPitchRate.Text);

            mHa = cbHorizontal.SelectedIndex == 0 ? HorizontalAlignment.Left :
                    cbHorizontal.SelectedIndex == 1 ? HorizontalAlignment.Center : HorizontalAlignment.Right;
            mVa = cbVertical.SelectedIndex == 0 ? VerticalAlignment.Top :
                    cbVertical.SelectedIndex == 1 ? VerticalAlignment.Center : VerticalAlignment.Bottom;
            if (0 < cbArrowSize.Text.Length)
                mArrowSize = ylib.string2double(cbArrowSize.Text);
            if (0 < cbArrowAngle.Text.Length)
                mArrowAngle = ylib.D2R(ylib.string2double(cbArrowAngle.Text));
            mLayerName = cbLayerName.Text;
            if (0 <= cbGridSize.SelectedIndex)
                mGridSize = mGridSizeMenu[cbGridSize.SelectedIndex];
            mFontFamily = cbFontFamily.Text;
            mFontStyle = cbFontStyle.Text;
            mFontWeight = cbFontWeight.Text;
            if (0 <= cbFillColor.SelectedIndex) {
                mFillColor = ylib.mBrushList[cbFillColor.SelectedIndex].brush;
                mFillColorName = ylib.mBrushList[cbFillColor.SelectedIndex].colorTitle;
            }
            mFillColorOn = chFillColorOn.IsChecked == true;
            if (0 <= cbBackColor.SelectedIndex) {
                mBackColor = ylib.mBrushList[cbBackColor.SelectedIndex].brush;
                //mBackColorName = ylib.mBrushList[cbBackColor.SelectedIndex].colorTitle;
            }

            mBackDispChk = chBackDispChk.IsChecked   == true;
            mColorChk         = chColor.IsChecked         == true;
            mLineTypeChk      = chLineType.IsChecked      == true;
            mPointTypeChk     = chPointType.IsChecked     == true;
            mThicknessChk     = chThickness.IsChecked     == true;
            mPointSizeChk     = chPointSize.IsChecked     == true;
            mTextSizeChk      = chTextSize.IsChecked      == true;
            mTextRotateChk    = chTextRotate.IsChecked    == true;
            mLinePitchRateChk = chLinePitchRate.IsChecked == true;
            mHaChk            = chHorizontal.IsChecked    == true;
            mVaChk            = chVertical.IsChecked      == true;
            mArrowSizeChk     = chArrowSize.IsChecked     == true;
            mArrowAngleChk    = chArrowAngle.IsChecked    == true;
            mLayerNameChk     = chLayerName.IsChecked     == true;
            mFontFamilyChk    = chFontFamily.IsChecked    == true;
            mFontStyleChk     = chFontStyle.IsChecked     == true;
            mFontWeightChk    = chFontWeight.IsChecked    == true;
            mFillColorChk     = chFillColor.IsChecked     == true;

            DialogResult = true;
            Close();
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
