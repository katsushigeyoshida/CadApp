using CoreLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CadApp
{
    /// <summary>
    /// EntProperty.xaml の相互作用ロジック
    /// 
    /// 要素プロパティ変更ダイヤログ
    /// </summary>
    public partial class EntProperty : Window
    {
        private List<string> mPointTypeMenu = new List<string>() {
            "・点", "X クロス", "+ 十字", "□ 四角", "〇 円"
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
        private string[] mHorizontalAlignmentMenu = { "Left", "Center", "Right" };
        private string[] mVerticalAlignmentMenu = { "Top", "Center", "Bottum" };
        private List<string> mFontFamilyMemu;
        private string[] mFontSTyleMenu = { "Normal", "Italic", "Oblique" };
        private string[] mFontWeightMenu = { "Normal", "Thin", "Bold" };

        public EntityId mEntityId = EntityId.Non;
        public string mColorName = "Black";
        public Brush mColor = Brushes.Black;
        public bool mBackDisp = false;
        public int mLineType = 0;
        public double mThickness = 1;
        public double mTextSize = 12;
        public HorizontalAlignment mHa = HorizontalAlignment.Left;
        public VerticalAlignment mVa = VerticalAlignment.Top;
        public double mTextRotate = 0;
        public string mFontFamily = "";
        public string mFontStyle = "Normal";
        public string mFontWeight = "Normal";
        public double mArrowSize = 5;
        public double mArrowAngle = 0.523598775599;
        public double mLinePitchRate = 1.2;
        public List<string> mLayerNameList = new List<string>();
        public string mLayerName = "";
        public string mPartsName = "";
        public bool mFillOn = false;
        public Brush mFillColor = Brushes.Aqua;
        public bool mFillEntity = false;

        YDraw ydraw = new YDraw();
        YLib ylib = new YLib();

        public EntProperty()
        {
            InitializeComponent();

            cbColor.DataContext     = ydraw.mColorList;
            cbFillColor.DataContext = ydraw.mColorList;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbColor.SelectedIndex    = ydraw.mColorList.FindIndex(p => p.brush == mColor);
            cbType.ItemsSource       = mEntityId == EntityId.Point ? mPointTypeMenu : mLineTypeMenu;
            chBackDisp.IsChecked     = mBackDisp;
            cbThickness.ItemsSource  = mEntSizeMenu;
            lbTypeTitle.Content      = mEntityId == EntityId.Point ? "点種" : "線種";
            lbSizeTitle.Content      = mEntityId == EntityId.Point ? "点サイズ" : "太さ";
            cbHorizontal.ItemsSource = mHorizontalAlignmentMenu;
            cbVertical.ItemsSource   = mVerticalAlignmentMenu;

            cbType.SelectedIndex       = mLineType;
            cbThickness.SelectedIndex  = mEntSizeMenu.FindIndex(p => p >= mThickness);
            tbTextSize.Text            = mTextSize.ToString();
            tbTextRotate.Text          = ylib.double2StrZeroSup(ylib.R2D(mTextRotate), "F8");
            tbLinePitchRate.Text       = mLinePitchRate.ToString();
            cbHorizontal.SelectedIndex = mHa == HorizontalAlignment.Left ? 0 :
                                         mHa == HorizontalAlignment.Center ? 1 : 2;
            cbVertical.SelectedIndex = mVa == VerticalAlignment.Top ? 0 :
                                       mVa == VerticalAlignment.Center ? 1 : 2;
            mFontFamilyMemu = ylib.getSystemFontFamilyName();
            cbFontFamily.ItemsSource   = mFontFamilyMemu;
            cbFontStyle.ItemsSource    = mFontSTyleMenu;
            cbFontWeight.ItemsSource   = mFontWeightMenu;
            cbFontFamily.SelectedIndex = mFontFamilyMemu.IndexOf(mFontFamily);
            cbFontStyle.SelectedIndex  = mFontSTyleMenu.FindIndex(p => p == mFontStyle);
            cbFontWeight.SelectedIndex = mFontWeightMenu.FindIndex(p => p == mFontWeight);

            tbArrowSize.Text  = mArrowSize.ToString();
            tbArrowAngle.Text = ylib.double2StrZeroSup(ylib.R2D(mArrowAngle),"F8");
            tbPartsName.Text  = mPartsName.TrimStart('_');

            lbTextSizeTitle.IsEnabled = false;
            tbTextSize.IsEnabled      = false;
            lbRotateTitle.IsEnabled   = false;
            tbTextRotate.IsEnabled    = false;
            lbLinePitchRateTitle.IsEnabled = false;
            tbLinePitchRate.IsEnabled = false;
            lbAlimentTitle.IsEnabled  = false;
            cbHorizontal.IsEnabled    = false;
            lbVATitle.IsEnabled       = false;
            cbVertical.IsEnabled      = false;
            lbFontFamily.IsEnabled    = false;
            cbFontFamily.IsEnabled    = false;
            lbFontStyle.IsEnabled     = false;
            cbFontStyle.IsEnabled     = false;
            lbFontWeight.IsEnabled    = false;
            cbFontWeight.IsEnabled    = false;

            lbArrowSizeTitle.IsEnabled  = false;
            tbArrowSize.IsEnabled       = false;
            lbArrowAngleTitle.IsEnabled = false;
            tbArrowAngle.IsEnabled      = false;
            lbPartsNameTitle.IsEnabled  = false;
            tbPartsName.IsEnabled       = false;

            lbFillTitle.IsEnabled = false;
            cbFillColor.IsEnabled = false;
            chFillColor.IsEnabled = false;

            if (mEntityId == EntityId.Text || mEntityId == EntityId.Parts) {
                lbTextSizeTitle.IsEnabled = true;
                tbTextSize.IsEnabled      = true;
                lbFontFamily.IsEnabled    = true;
                cbFontFamily.IsEnabled    = true;
                lbFontStyle.IsEnabled     = true;
                cbFontStyle.IsEnabled     = true;
                lbFontWeight.IsEnabled    = true;
                cbFontWeight.IsEnabled    = true;
            }
            if (mEntityId == EntityId.Text) {
                lbTypeTitle.IsEnabled     = false;
                cbType.IsEnabled          = false;
                lbSizeTitle.IsEnabled     = false;
                cbThickness.IsEnabled     = false;
                lbRotateTitle.IsEnabled   = true;
                tbTextRotate.IsEnabled    = true;
                lbLinePitchRateTitle.IsEnabled = true;
                tbLinePitchRate.IsEnabled = true;
                lbAlimentTitle.IsEnabled  = true;
                cbHorizontal.IsEnabled    = true;
                lbVATitle.IsEnabled       = true;
                cbVertical.IsEnabled      = true;
            } else if (mEntityId == EntityId.Parts) {
                lbTypeTitle.IsEnabled      = true;
                cbType.IsEnabled           = true;
                lbSizeTitle.IsEnabled      = true;
                cbThickness.IsEnabled      = true;
                cbThickness.SelectedIndex  = mEntSizeMenu.FindIndex(p => p >= mThickness);
                lbRotateTitle.IsEnabled    = true;
                tbTextRotate.IsEnabled     = true;
                lbLinePitchRateTitle.IsEnabled = true;
                tbLinePitchRate.IsEnabled   = true;
                lbArrowSizeTitle.IsEnabled  = true;
                tbArrowSize.IsEnabled       = true;
                lbArrowAngleTitle.IsEnabled = true;
                tbArrowAngle.IsEnabled      = true;
                if (mPartsName.Length == 0 || (2 < mPartsName.Length && mPartsName.Substring(0, 2) == "__")) {
                    lbPartsNameTitle.IsEnabled = true;
                    tbPartsName.IsEnabled = true;
                }
            }
            cbLayerName.ItemsSource = mLayerNameList;
            cbLayerName.Text = mLayerName;
            if (mFillEntity) {
                lbFillTitle.IsEnabled = true;
                cbFillColor.IsEnabled = true;
                chFillColor.IsEnabled = true;

                cbFillColor.SelectedIndex = ydraw.mColorList.FindIndex(p => p.brush == mFillColor);
                chFillColor.IsChecked = mFillOn;
            }
        }

        /// <summary>
        /// [OK]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            if (0 <= cbColor.SelectedIndex) {
                mColor = ydraw.mColorList[cbColor.SelectedIndex].brush;
                mColorName = ydraw.mColorList[cbColor.SelectedIndex].colorTitle;
            }
            mBackDisp = chBackDisp.IsChecked == true;
            if (0 <= cbType.SelectedIndex)
                mLineType = cbType.SelectedIndex;
            if (0 <= cbThickness.SelectedIndex) {
                if (mEntityId == EntityId.Text) {
                    mHa = cbHorizontal.SelectedIndex == 0 ? HorizontalAlignment.Left :
                          cbHorizontal.SelectedIndex == 1 ? HorizontalAlignment.Center : HorizontalAlignment.Right;
                    mVa = cbVertical.SelectedIndex == 0 ? VerticalAlignment.Top :
                          cbVertical.SelectedIndex == 1 ? VerticalAlignment.Center : VerticalAlignment.Bottom;
                } else {
                    mThickness = mEntSizeMenu[cbThickness.SelectedIndex];
                }
            }
            mTextSize      = ylib.string2double(tbTextSize.Text);
            mTextRotate    = ylib.D2R(ylib.string2double(tbTextRotate.Text));
            mLinePitchRate = ylib.string2double(tbLinePitchRate.Text);
            mFontFamily    = cbFontFamily.Text;
            mFontStyle     = cbFontStyle.Text;
            mFontWeight    = cbFontWeight.Text;
            mArrowSize     = ylib.string2double(tbArrowSize.Text);
            mArrowAngle    = ylib.D2R(ylib.string2double(tbArrowAngle.Text));
            mLayerName     = cbLayerName.Text;
            if (mPartsName.Length == 0 || (2 < mPartsName.Length && mPartsName.Substring(0, 2) == "__"))
                mPartsName = "__" + tbPartsName.Text;
            mFillOn = chFillColor.IsChecked == true;
            if (0 <= cbFillColor.SelectedIndex) {
                mFillColor = ydraw.mColorList[cbFillColor.SelectedIndex].brush;
            }

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// [キャンセル]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
