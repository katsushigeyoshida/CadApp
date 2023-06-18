using CoreLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CadApp
{
    /// <summary>
    /// EntProperty.xaml の相互作用ロジック
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
            1, 2, 4, 6, 8, 9, 10, 11, 12, 13, 14, 16, 18, 20, 24, 28, 32, 36, 40, 48, 56, 64
        };
        private double[] mTextRotateMenu = {
            0, 30, 45, 60, 90, 120, 135, 150, 180, 210, 225, 240, 270, 300, 315, 330
        };
        private double[] mEntSizeMenu = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10
        };
        private string[] mHorizontalAlignmentMenu = { "Left", "Center", "Right" };
        private string[] mVerticalAlignmentMenu = { "Top", "Center", "Bottum" };

        public EntityId mEntityId = EntityId.Non;
        public string mColorName = "Black";
        public Brush mColor = Brushes.Black;
        public int mType = 0;
        public double mThickness = 1;
        public double mTextSize = 12;
        public HorizontalAlignment mHa = HorizontalAlignment.Left;
        public VerticalAlignment mVa = VerticalAlignment.Top;
        public double mTextRotate = 12;

        YDraw ydraw = new YDraw();
        YLib ylib = new YLib();

        public EntProperty()
        {
            InitializeComponent();

            cbColor.DataContext = ydraw.mColorList;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbType.ItemsSource = mEntityId == EntityId.Point ? mPointTypeMenu : mLineTypeMenu;
            cbThickness.ItemsSource = mEntityId == EntityId.Text ? mTextSizeMenu : mEntSizeMenu;
            lbTypeTitle.Content = mEntityId == EntityId.Point ? "点種" : "線種";
            lbSizeTitle.Content = mEntityId == EntityId.Point ? "点サイズ" : (mEntityId == EntityId.Text ? "文字サイズ" : "太さ");
            cbHorizontal.ItemsSource = mHorizontalAlignmentMenu;
            cbVertical.ItemsSource = mVerticalAlignmentMenu;
            cbTextRotate.ItemsSource = mTextRotateMenu;

            cbColor.SelectedIndex = ydraw.mColorList.FindIndex(p => p.brush == mColor);
            cbType.SelectedIndex = mType;
            cbHorizontal.SelectedIndex = mHa == HorizontalAlignment.Left ? 0 :
                                         mHa == HorizontalAlignment.Center ? 1 : 2;
            cbVertical.SelectedIndex = mVa == VerticalAlignment.Top ? 0 :
                                       mVa == VerticalAlignment.Center ? 1 : 2;

            if (mEntityId == EntityId.Text) {
                lbTypeTitle.IsEnabled = false;
                cbType.IsEnabled = false;
                cbThickness.SelectedIndex = mTextSizeMenu.FindIndex(p => p >= mThickness);
                cbTextRotate.Text = ylib.R2D(mTextRotate).ToString();
            } else {
                cbType.IsEnabled = true;
                cbThickness.SelectedIndex = mEntSizeMenu.FindIndex(p => p >= mThickness);
                lbAlimentTitle.Visibility = Visibility.Hidden;
                cbHorizontal.Visibility   = Visibility.Hidden;
                cbVertical.Visibility     = Visibility.Hidden;
                lbRotateTitle.Visibility = Visibility.Hidden;
                cbTextRotate.Visibility   = Visibility.Hidden;
            }
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            if (0 <= cbColor.SelectedIndex) {
                mColor = ydraw.mColorList[cbColor.SelectedIndex].brush;
                mColorName = ydraw.mColorList[cbColor.SelectedIndex].colorTitle;
            }
            if (0 <= cbType.SelectedIndex)
                mType = cbType.SelectedIndex;
            if (0 <= cbThickness.SelectedIndex) {
                if (mEntityId == EntityId.Text) {
                    mThickness = mTextSizeMenu[cbThickness.SelectedIndex];
                    mHa = cbHorizontal.SelectedIndex == 0 ? HorizontalAlignment.Left :
                          cbHorizontal.SelectedIndex == 1 ? HorizontalAlignment.Center : HorizontalAlignment.Right;
                    mVa = cbVertical.SelectedIndex == 0 ? VerticalAlignment.Top :
                          cbVertical.SelectedIndex == 1 ? VerticalAlignment.Center : VerticalAlignment.Bottom;
                    mTextRotate = ylib.D2R(ylib.string2double(cbTextRotate.Text));
                } else {
                    mThickness = mEntSizeMenu[cbThickness.SelectedIndex];
                }
            }

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
