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
// "FlockingAvoidance/WorldCanvas.cs" was last cleaned by Rick on 2014/10/01 at 2:39 PM
#endregion

namespace FlockingAvoidance {
    using System;
    using System.Collections.Concurrent;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
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
    using Point = System.Windows.Point;

    public class World3DCanvas : Canvas {


    }

    /// <summary>
    ///     Simple flocking container canvas
    /// </summary>
    /// <copyright>http://sachabarbs.wordpress.com/2010/03/01/wpf-a-fun-little-boids-type-thing/</copyright>
    public class WorldCanvas : Canvas, IDisposable {
        public const Int32 CANVAS_HEIGHT = 800;
        public const Int32 CANVAS_WIDTH = 800;

        //private static readonly PointF Middle = new PointF( Entity.ImageWidth / 2.0f, Entity.ImageHeight / 2.0f );

        public readonly ConcurrentDictionary<EntityType, BitmapImage> EntityImages = new ConcurrentDictionary<EntityType, BitmapImage>();
        private readonly ParallelList<Entity> _entities = new ParallelList<Entity>();
        private readonly DispatcherTimer _redrawTimer;
        private Point _mousePoint;

        /// <summary>
        ///     Ctor
        /// </summary>
        public WorldCanvas() {
            this.Width = CANVAS_WIDTH;
            this.Height = CANVAS_HEIGHT;

            this.UIThread = Thread.CurrentThread;

            //var bob = new Storyboard();
            //var jane = new PointAnimation( orig );
            //bob.Children.Add( jane  );

            this._redrawTimer = new DispatcherTimer( DispatcherPriority.Render ) {
                Interval = Hertz.Sixty
            };
            this._redrawTimer.Tick += this.OnRedrawTimerTick;
            this._redrawTimer.Start();

            Task.Run( () => this.Init() );
        }

        public Thread UIThread {
            get;
            private set;
        }

        /// <summary>
        ///     Clean up
        /// </summary>
        public void Dispose() {
            this._redrawTimer.Stop();
            this._redrawTimer.Tick -= this.OnRedrawTimerTick;
        }

        public static PointF GiveRandomSpot() {
            return new PointF( Randem.NextSingle() * CANVAS_WIDTH, Randem.NextSingle() * CANVAS_HEIGHT );
        }

        private void LoadAllEntities() {
            Report.Enter();
            for ( var i = 0 ; i < 3 ; i++ ) {
                var entity = Entity.Create();
                //this.Children.Add( entity );
                this._entities.Add( entity );
            }
            Report.Exit();
        }

        public async Task<Boolean> RunOnUI( Action action ) {
            if ( null == action ) {
                return false;
            }

            var fromThread = Dispatcher.FromThread( this.UIThread );
            if ( null == fromThread ) {
                return false;
            }

            try {
                await fromThread.InvokeAsync( action );
                return true;
            }
            catch ( Exception exception ) {
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

            foreach ( EntityType entityType in Enum.GetValues( typeof( EntityType ) ) ) {
                this.LoadImageStub( entityType );
            }

            Report.Exit();
        }

        private void LoadImageStub( EntityType entityType ) {
            try {
                this.EntityImages[ entityType ] = new BitmapImage();
                this.EntityImages[ entityType ].BeginInit();
                var imageSource = String.Format( "Images/{0}.png", entityType );
                Report.Info( String.Format( "Loading image `{0}`", imageSource ) );
                this.EntityImages[ entityType ].UriSource = new Uri( BaseUriHelper.GetBaseUri( this ), imageSource );
                this.EntityImages[ entityType ].EndInit();
                this.EntityImages[ entityType ].Freeze();
            }
            catch ( Exception exception ) {
                exception.Error();
            }
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

            foreach ( var entity in this._entities ) {
                drawingContext.PushTransform( new TranslateTransform( entity.Position.X, entity.Position.Y ) );

                drawingContext.PushTransform( new RotateTransform( entity.Heading, entity.Position.X, entity.Position.Y ) );

                drawingContext.DrawImage( this.EntityImages[ entity.EntityType ], entity.ImageBoundary );

                drawingContext.Pop(); // pop RotateTransform
                drawingContext.Pop(); // pop TranslateTransform
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

            //redraw all
            this.InvalidateVisual();

            if ( null != timer ) {
                timer.Start();
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

        }
    }
}
