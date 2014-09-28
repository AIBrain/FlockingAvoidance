#region License & Information
// This notice must be kept visible in the source.
// 
// This section of source code belongs to Rick@AIBrain.Org unless otherwise specified,
// or the original license has been overwritten by the automatic formatting of this code.
// Any unmodified sections of source code borrowed from other projects retain their original license and thanks goes to the Authors.
// 
// Donations and Royalties can be paid via
// PayPal: paypal@aibrain.org
// bitcoin:1Mad8TxTqxKnMiHuZxArFvX8BuFEB9nqX2
// bitcoin:1NzEsF7eegeEWDr5Vr9sSSgtUC4aL6axJu
// litecoin:LeUxdU2w3o6pLZGVys5xpDZvvo8DUrjBp9
// 
// Usage of the source code or compiled binaries is AS-IS.
// I am not responsible for Anything You Do.
// 
// Contact me by email if you have any questions or helpful criticism.
// 
// "FlockingAvoidance/FlockingAvoidanceCanvas.cs" was last cleaned by Rick on 2014/09/28 at 3:13 AM
#endregion

namespace FlockingAvoidance {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Threading;
    using Librainian.Threading;

    /// <summary>
    ///     Simple flocking container canvas
    /// </summary>
    /// <copyright>http://sachabarbs.wordpress.com/2010/03/01/wpf-a-fun-little-boids-type-thing/</copyright>
    public class FlockingAvoidanceCanvas : Canvas, IDisposable {
        public const Int32 CANVAS_HEIGHT = 800;
        public const Int32 CANVAS_WIDTH = 800;

        //public static Random rand = new Random();

        private readonly List<FlockItem> flockItems = new List<FlockItem>();

        private readonly BitmapImage imgSource;

        private const Double offsetX = FlockItem.ITEM_WIDTH / 2.0;

        private const Double offsetY = FlockItem.ITEM_HEIGHT / 2.0;

        private readonly DispatcherTimer timer;

        private const AnimalType currentAnimalType = AnimalType.Butterfly;

        private Point _mousePoint;

        /// <summary>
        ///     Ctor
        /// </summary>
        public FlockingAvoidanceCanvas() {
            this.Width = CANVAS_WIDTH;
            this.Height = CANVAS_HEIGHT;

            for ( var i = 0 ; i < 100 ; i++ ) {
                this.flockItems.Add( new FlockItem() );
            }

            this.timer = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds( 1 )
            };
            this.timer.Tick += this.OnTimerTick;
            this.timer.Start();

            string imagePath;
            switch ( currentAnimalType ) {
                case AnimalType.Butterfly:
                    imagePath = @"Images\butterfly.png";
                    break;

                case AnimalType.Fish:
                    imagePath = @"Images\fish.png";
                    break;

                default:
                    imagePath = @"Images\butterfly.png";
                    break;
            }

            this.imgSource = new BitmapImage();
            this.imgSource.BeginInit();
            this.imgSource.UriSource = new Uri( BaseUriHelper.GetBaseUri( this ), imagePath );
            this.imgSource.EndInit();
            this.imgSource.Freeze();
        }

        /// <summary>
        ///     Store Mouse Point to allow flocking
        ///     items to avoid the Mouse
        /// </summary>
        protected override void OnMouseMove( MouseEventArgs e ) {
            base.OnMouseMove( e );
            this._mousePoint = e.GetPosition( this );
        }

        //Asked to ReDraw so draw all
        protected override void OnRender( DrawingContext dc ) {
            base.OnRender( dc );

            //draw flocking items
            foreach ( var item in this.flockItems ) {
                Double angle = FlockItem.AngleItem( item.VX, item.VY );

                dc.PushTransform( new TranslateTransform( item.CentrePoint.X, item.CentrePoint.Y ) );

                dc.PushTransform( new RotateTransform( angle, offsetX, offsetY ) );

                dc.DrawImage( this.imgSource, new Rect( 0, 0, FlockItem.ITEM_WIDTH, FlockItem.ITEM_HEIGHT ) );

                dc.Pop(); // pop RotateTransform
                dc.Pop(); // pop TranslateTransform
            }
        }

        /// <summary>
        ///     Update flocking items
        /// </summary>
        private void OnTimerTick( object sender, EventArgs e ) {

            var dispatcherTimer = sender as DispatcherTimer;
            if ( null != dispatcherTimer ) {
                dispatcherTimer.Stop();
            }

            Parallel.ForEach( this.flockItems, ThreadingExtensions.Parallelism, itemX => {

                foreach ( var itemY in this.flockItems.Where( item => !ReferenceEquals( itemX, item ) ) ) {
                    var dx = itemY.X - itemX.X;
                    var dy = itemY.Y - itemX.Y;
                    var d = Math.Sqrt( dx * dx + dy * dy );
                    if ( d < 40 ) {
                        itemX.VX += 20 * ( -dx / ( d * d ) );
                        itemX.VY += 20 * ( -dy / ( d * d ) );
                    }
                    else if ( d < 100 ) {
                        itemX.VX += 0.07 * ( dx / d );
                        itemX.VY += 0.07 * ( dy / d );
                    }
                }

                var dxMouse = this._mousePoint.X - itemX.X;
                var dyMouse = this._mousePoint.Y - itemX.Y;
                var dSqrt = Math.Sqrt( dxMouse * dxMouse + dyMouse * dyMouse );
                if ( dSqrt < 100 ) {
                    itemX.VX += 1 * ( -dxMouse / ( dSqrt ) );
                    itemX.VY += 1 * ( -dyMouse / ( dSqrt ) );
                }


                itemX.Move();


            } );

            //redraw all
            this.InvalidateVisual();

            if ( null != dispatcherTimer ) {
                dispatcherTimer.Start();
            }


        }

        /// <summary>
        ///     Clean up
        /// </summary>
        public void Dispose() {
            this.timer.Tick -= this.OnTimerTick;
        }
    }
}
