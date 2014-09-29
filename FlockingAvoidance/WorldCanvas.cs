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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Threading;
    using Librainian;
    using Librainian.Collections;
    using Librainian.Measurement.Frequency;
    using Librainian.Threading;




    /// <summary>
    ///     Simple flocking container canvas
    /// </summary>
    /// <copyright>http://sachabarbs.wordpress.com/2010/03/01/wpf-a-fun-little-boids-type-thing/</copyright>
    public class WorldCanvas : Canvas, IDisposable {
        public const Int32 CANVAS_HEIGHT = 800;
        public const Int32 CANVAS_WIDTH = 800;

        //public static Random rand = new Random();

        private readonly ParallelList<Entity> _entities = new ParallelList<Entity>();

        //private readonly BitmapImage imgSource;

        private const Double OffsetX = Entity.ItemWidth / 2.0;

        private const Double OffsetY = Entity.ItemHeight / 2.0;

        private readonly DispatcherTimer _redrawTimer;

        private const EntityType CurrentAnimalType = EntityType.Butterfly;

        private Point _mousePoint;

        public readonly ConcurrentDictionary<EntityType, BitmapImage> EntityImages = new ConcurrentDictionary<EntityType, BitmapImage>();

        /// <summary>
        ///     Ctor
        /// </summary>
        public WorldCanvas() {
            this.Width = CANVAS_WIDTH;
            this.Height = CANVAS_HEIGHT;

            this.UIThread = Thread.CurrentThread;

            this._redrawTimer = new DispatcherTimer( DispatcherPriority.Render ) {
                Interval = Hertz.OneHundredTwenty  //twice the most common refresh rate?
            };
            this._redrawTimer.Tick += this.OnRedrawTimerTick;
            this._redrawTimer.Start();

            Task.Run( () => this.Init() );
        }

        public Thread UIThread {
            get;
            private set;
        }

        private void LoadAllEntities() {
            Report.Enter();
            Parallel.For( 1, 200, ( l, state ) => this._entities.Add( new Entity( Randem.RandomEnum<EntityType>() ) ) );
            Report.Exit();
        }

        public async Task<Boolean> RunOnUI( Action action ) {
            if ( null == action ) {
                return false;
            }

            var fromThread = Dispatcher.FromThread( UIThread );
            if ( null == fromThread ) {
                return false;
            }

            try {
                await fromThread.InvokeAsync( action );
                return true;
            }
            catch ( Exception exception) {
                exception.Error();
            }
            return false;
        }

        private async void Init() {
            await this.RunOnUI( this.LoadImages );
            await this.RunOnUI( this.LoadAllEntities );
        }

        private void LoadImages() {
            Report.Enter();

            foreach ( EntityType animalType in Enum.GetValues( typeof( EntityType ) ) ) {
                this.EntityImages[ animalType ] = new BitmapImage();
                this.EntityImages[ animalType ].BeginInit();
                var imageSource = String.Format( "Images/{0}.png", animalType );
                Report.Info( String.Format( "Loading image `{0}`", imageSource ) );
                this.EntityImages[ animalType ].UriSource = new Uri( BaseUriHelper.GetBaseUri( this ), imageSource );
                this.EntityImages[ animalType ].EndInit();
                this.EntityImages[ animalType ].Freeze();
            }

            Report.Exit();
        }

        /// <summary>
        ///     Store Mouse Point to allow flocking
        ///     items to avoid the Mouse
        /// </summary>
        protected override void OnMouseMove( MouseEventArgs e ) {
            base.OnMouseMove( e );
            this._mousePoint = e.GetPosition( this );
        }

        //Asked to ReDraw so draw entities
        protected override void OnRender( DrawingContext drawingContext ) {
            base.OnRender( drawingContext );


            //draw flocking items
            foreach ( var animal in this._entities ) {
                //FlockItem animal;
                //if ( !this._animals.TryDequeue( out animal ) ) {
                //    return;
                //}

                //Double angle = Entity.AngleItem( animal.VelocityX, animal.VelocityY );

                drawingContext.PushTransform( new TranslateTransform( animal.CenterPoint.X, animal.CenterPoint.Y ) );

                drawingContext.PushTransform( new RotateTransform( animal.angle, OffsetX, OffsetY ) );

                drawingContext.DrawImage( EntityImages[ animal.EntityType ], new Rect( 0, 0, Entity.ItemWidth, Entity.ItemHeight ) );

                drawingContext.Pop(); // pop RotateTransform
                drawingContext.Pop(); // pop TranslateTransform


                //this._animals.Enqueue( animal );
            }
        }

        /// <summary>
        ///     Update flocking items
        /// </summary>
        private void OnRedrawTimerTick( object sender, EventArgs e ) {

            var timer = sender as DispatcherTimer;
            if ( null != timer ) {
                timer.Stop();
            }

            //Parallel.ForEach( this._entities, ThreadingExtensions.Parallelism, itemX => {

            //    foreach ( var itemY in this._entities.Where( item => !ReferenceEquals( itemX, item ) ) ) {
            //        var dx = itemY.Position.X - itemX.Position.X;
            //        var dy = itemY.Position.Y - itemX.Position.Y;
            //        var d = Math.Sqrt( dx * dx + dy * dy );
            //        if ( d < 40 ) {
            //            itemX.VelocityX += 20 * ( -dx / ( d * d ) );
            //            itemX.VelocityY += 20 * ( -dy / ( d * d ) );
            //        }
            //        else if ( d < 100 ) {
            //            itemX.VelocityX += 0.07 * ( dx / d );
            //            itemX.VelocityY += 0.07 * ( dy / d );
            //        }
            //    }

            //    var dxMouse = this._mousePoint.X - itemX.Position.X;
            //    var dyMouse = this._mousePoint.Y - itemX.Position.Y;
            //    var dSqrt = Math.Sqrt( dxMouse * dxMouse + dyMouse * dyMouse );
            //    if ( dSqrt < 100 ) {
            //        itemX.VelocityX += 1 * ( -dxMouse / ( dSqrt ) );
            //        itemX.VelocityY += 1 * ( -dyMouse / ( dSqrt ) );
            //    }


            //    itemX.Move();


            //} );

            //redraw all
            this.InvalidateVisual();

            if ( null != timer ) {
                timer.Start();
            }


        }

        /// <summary>
        ///     Clean up
        /// </summary>
        public void Dispose() {
            this._redrawTimer.Stop();
            this._redrawTimer.Tick -= this.OnRedrawTimerTick;
        }
    }
}
