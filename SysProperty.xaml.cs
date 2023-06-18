using CoreLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CadApp
{
    /// <summary>
    /// SysProperty.xaml の相互作用ロジック
    /// </summary>
    public partial class SysProperty : Window
    {
        private List<string> mPointTypeMenu = new List<string>() {
            "・点", "X クロス", "+ 十字", "□ 四角", "〇 円"
        };
        private List<string> mLineTypeMenu = new List<string>() {
            "実線", "破線", "一点鎖線", "二点鎖線"
        };
        private double[] mTextSizeMenu = {
            1, 2, 4, 6, 8, 9, 10, 11, 12, 13, 14, 16, 18, 20, 24, 28, 32, 36, 40, 48, 56, 64
        };
        private double[] mTextRotateMenu = {
            0, 30, 45, 60, 90, 120, 135, 150, 180, 210, 225, 240, 270, 300, 315, 330
        };
        private double[] mEntSizeMenu = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10
        };
        private double[] mGridSizeMenu = {
            0, 0.1, 0.2, 0.5, 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000
        };
        private string[] mHorizontalAlignmentMenu = { "Left", "Center", "Right" };
        private string[] mVerticalAlignmentMenu = { "Top", "Center", "Bottum" };

        public string mColorName = "Black";
        public Brush mColor = Brushes.Black;
        public int mLineType = 0;
        public double mThickness = 1;
        public int mPointType = 0;
        public double mPointSize = 1;
        public double mTextSize = 12;
        public double mTextRotate = 0;
        public HorizontalAlignment mHa = HorizontalAlignment.Left;
        public VerticalAlignment mVa = VerticalAlignment.Top;
        public double mGridSize = 1;

        YDraw ydraw = new YDraw();
        YLib ylib = new YLib();

        public SysProperty()
        {
            InitializeComponent();

            cbColor.DataContext = ydraw.mColorList;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbLineType.ItemsSource = mLineTypeMenu;
            cbPointType.ItemsSource = mPointTypeMenu;
            cbThickness.ItemsSource = mEntSizeMenu;
            cbPointSize.ItemsSource = mEntSizeMenu;
            cbTextSize.ItemsSource = mTextSizeMenu;
            cbTextRotate.ItemsSource = mTextRotateMenu;
            cbHorizontal.ItemsSource = mHorizontalAlignmentMenu;
            cbVertical.ItemsSource = mVerticalAlignmentMenu;
            cbGridSize.ItemsSource = mGridSizeMenu;

            cbColor.SelectedIndex = ydraw.mColorList.FindIndex(p => p.brush == mColor);
            cbLineType.SelectedIndex = mLineType;
            cbPointType.SelectedIndex = mPointType;
            cbThickness.SelectedIndex = mEntSizeMenu.FindIndex(p => p  >= mThickness);
            cbPointSize.SelectedIndex = mEntSizeMenu.FindIndex(p => p >= mPointSize);
            cbTextSize.SelectedIndex = mTextSizeMenu.FindIndex(p => p >= mTextSize);
            cbTextRotate.SelectedIndex = mTextRotateMenu.FindIndex(p => p >= ylib.R2D(mTextRotate));
            cbHorizontal.SelectedIndex = mHa == HorizontalAlignment.Left ? 0 :
                                         mHa == HorizontalAlignment.Center ? 1 : 2;
            cbVertical.SelectedIndex = mVa == VerticalAlignment.Top ? 0 :
                                       mVa == VerticalAlignment.Center ? 1 : 2;
            cbGridSize.SelectedIndex = mGridSizeMenu.FindIndex(p => p >= mGridSize);

        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            if (0 <= cbColor.SelectedIndex) {
                mColor = ydraw.mColorList[cbColor.SelectedIndex].brush;
                mColorName = ydraw.mColorList[cbColor.SelectedIndex].colorTitle;
            }
            if (0 <= cbLineType.SelectedIndex)
                mLineType = cbLineType.SelectedIndex;
            if (0 <= cbThickness.SelectedIndex)
                mThickness = mEntSizeMenu[cbThickness.SelectedIndex];
            if (0 <= cbPointType.SelectedIndex)
                mPointType = cbPointType.SelectedIndex;
            if (0 <= cbPointSize.SelectedIndex)
                mPointSize = mEntSizeMenu[cbPointSize.SelectedIndex];

            if (0 <= cbTextSize.SelectedIndex)
                mTextSize = mTextSizeMenu[cbTextSize.SelectedIndex];
            if (0 <= cbTextRotate.SelectedIndex)
                mTextRotate = ylib.D2R(mTextRotateMenu[cbTextRotate.SelectedIndex]);

            mHa = cbHorizontal.SelectedIndex == 0 ? HorizontalAlignment.Left :
                          cbHorizontal.SelectedIndex == 1 ? HorizontalAlignment.Center : HorizontalAlignment.Right;
            mVa = cbVertical.SelectedIndex == 0 ? VerticalAlignment.Top :
                          cbVertical.SelectedIndex == 1 ? VerticalAlignment.Center : VerticalAlignment.Bottom;

            if (0 <= cbGridSize.SelectedIndex)
                mGridSize = mGridSizeMenu[cbGridSize.SelectedIndex];

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
