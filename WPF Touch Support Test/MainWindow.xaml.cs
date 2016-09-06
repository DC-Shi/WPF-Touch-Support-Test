using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_Touch_Support_Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Step: 3
        /// Utilize the ManipulationStarting event to set values for ManipulationContainer and Handled property.
        /// ManipulationContainer allows specifying that the position should be relative to another element.
        /// Handled allows specifying how the event is handled by the class handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void centerImageView_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            e.Handled = true;
        }

        /// <summary>
        /// Step: 4
        /// ManipulationDelta event occurs multiple times when the user drags a finger over the screen during manipulation.
        /// CumulativeManipulation property contains the total changes that occurred for the current manipulation.
        /// It further provides more details of the type of manipulation, e.g., translation, scaling, rotation, etc.
        /// The Velocities property give details on the current speed of manipulation and direction.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void centerImageView_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            //store values of horizontal & vertical cumulative translation
            cumulativeDeltaX = e.CumulativeManipulation.Translation.X;
            cumulativeDeltaY = e.CumulativeManipulation.Translation.Y;
            //store value of linear velocity into horizontal direction  
            linearVelocity = e.Velocities.LinearVelocity.X;

            /// Added from part 2. Scale part.
            // get current matrix of the element.
            Matrix borderMatrix = ((MatrixTransform)centerImageView.RenderTransform).Matrix;
            //determine if action is zoom or pinch
            var maxScale = Math.Max(e.DeltaManipulation.Scale.X, e.DeltaManipulation.Scale.Y);
            //check if not crossing minimum and maximum zoom limit
            if ((maxScale < 1 && borderMatrix.M11 * maxScale > MinimumZoom) ||
            (maxScale > 1 && borderMatrix.M11 * maxScale < MaximumZoom))
            {
                //scale to most recent change (delta) in X & Y 
                borderMatrix.ScaleAt(e.DeltaManipulation.Scale.X,
                        e.DeltaManipulation.Scale.Y,
                        centerImageView.ActualWidth / 2,
                        centerImageView.ActualHeight / 2);
                //render new matrix
                centerImageView.RenderTransform = new MatrixTransform(borderMatrix);
            }
        }

        /// <summary>
        /// Variables that Step 4 lacks.
        /// </summary>
        double cumulativeDeltaX, cumulativeDeltaY, linearVelocity;

        /// <summary>
        /// Step: 5
        /// Use the ManipulationInertiaStarting event to set the desired deceleration value for the given manipulation behavior,
        /// e.g., ExpansionBehaviour and RotationBehaviour.
        /// After the ManipulationInertiaStarting is called,
        /// it will call ManipulationDelta until velocity becomes zero.
        /// Set initial velocity of the expansion behavior and desired deceleration here.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void centerImageView_ManipulationDelta(object sender, System.Windows.Input.ManipulationInertiaStartingEventArgs e)
        {
            e.ExpansionBehavior = new InertiaExpansionBehavior()
            {
                InitialVelocity = e.InitialVelocities.ExpansionVelocity,
                DesiredDeceleration = 10.0 * 96.0 / 1000000.0
            };
        }

        /// <summary>
        /// Step: 6
        /// Use the ManipulationCompleted event to determine the total amount the position of the manipulation changed.
        /// Use the stored value cumulativeDeltaX to determine if the movement is from left-to-right or right-to-left.
        /// Also call the isSwipeGesture function to determine if the manipulation formed a Swipe gesture or not.
        /// We need to show the next image when the photo app recognizes a swipe from right-to-left.
        /// So set the current index of the list view to next image.
        /// Similarly, when the photo app recognizes a swipe from left-to-right,
        /// it should set the current index of the list view to the previous image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void centerImageView_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            bool isRightToLeftSwipe = false; //to determine swipe direction
            if (cumulativeDeltaX < 0)
            {
                //moving from right to left
                isRightToLeftSwipe = true;
            }
            //check if this is swipe gesture
            if (isSwipeGesture(cumulativeDeltaX, cumulativeDeltaY, linearVelocity))
            {
                if (isRightToLeftSwipe)
                {
                    //show previous image
                    this.imagesListBox.SelectedIndex = imagesListBox.SelectedIndex - 1;
                }
                else
                {
                    //show next image
                    this.imagesListBox.SelectedIndex = imagesListBox.SelectedIndex + 1;
                }
            }
        }



        /// <summary>
        /// Step: 7
        /// isSwipeGesture is a method to determine the characteristics of a swipe gesture.
        /// Consider deltaX, deltaY, and velocity to decide the swipe gesture’s characteristics.
        /// Here, AppConstants.DeltaX and AppConstants.DeltaY are defined as constant values that define the maximum allowed horizontal and vertical movement,
        /// respectively.AppConstants.LinearVelocityX is defined as a constant value that defines the maximum allowed velocity.
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        /// <param name="linearVelocity"></param>
        /// <returns></returns>
        private bool isSwipeGesture(double deltaX, double deltaY, double linearVelocity)
        {
            //imagesListBox.Items.Add(linearVelocity);
            bool result = false;
            if (Math.Abs(deltaY) <= DeltaY && Math.Abs(deltaX) >= DeltaX && Math.Abs(linearVelocity) >= LinearVelocityX)
                result = true;
            return result;
        }


        /// <summary>
        /// Added constants that Step 7 used.
        /// </summary>
        const double DeltaX = 50, DeltaY = 50, LinearVelocityX = 0.04, MinimumZoom = 0.1, MaximumZoom = 10;


        private void imagesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb != null)
            {
                Image i = lb.SelectedItem as Image;
                if (i != null)
                {
                    centerImageView.Source = i.Source;
                }
            }
        }

        private void centerImageView_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            e.Handled = true;
        }
    }
}
