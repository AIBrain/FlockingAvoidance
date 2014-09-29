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
// "FlockingAvoidance/Entity.cs" was last cleaned by Rick on 2014/09/29 at 2:23 PM

#endregion License & Information

namespace FlockingAvoidance {

    using System;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Threading;
    using Librainian.Annotations;
    using Librainian.Measurement.Frequency;
    using Librainian.Measurement.Spatial;
    using Librainian.Threading;

    /// <summary>
    ///     Flocking Item
    /// </summary>
    /// <copyright>http://sachabarbs.wordpress.com/2010/03/01/wpf-a-fun-little-boids-type-thing/</copyright>
    public class Entity {
        public const Int32 ImageHeight = 30;

        public const Int32 ImageWidth = 30;

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="entityType"></param>
        public Entity( EntityType entityType ) {
            this.EntityType = entityType;
            this.Position = new PointF( Randem.NextSingle() * WorldCanvas.CANVAS_WIDTH, Randem.NextSingle() * WorldCanvas.CANVAS_HEIGHT );

            this.VelocityX = 1;
            this.VelocityY = 1;

            this.Home = new PointF( Randem.NextSingle() * WorldCanvas.CANVAS_WIDTH, Randem.NextSingle() * WorldCanvas.CANVAS_HEIGHT );

            this.Boundary = new Rect( 0, 0, ImageWidth, ImageHeight );

            this.BrainTimer = new DispatcherTimer( DispatcherPriority.ApplicationIdle ) {
                Interval = Hertz.ThreeHundredThirtyThree
            };
            this.BrainTimer.Tick += this.Think;
            this.BrainTimer.Start();
        }

        /// <summary>
        ///     The direction towards <see cref="Home" />.
        /// </summary>
        public Degrees Bearing {
            get;
            set;
        }

        public Rect Boundary {
            get;
            private set;
        }

        [NotNull]
        public DispatcherTimer BrainTimer {
            get;
            private set;
        }

        /// <summary>
        ///     Centre of item
        /// </summary>
        public PointF CenterPoint {
            get {
                return new PointF( this.Position.X + ( ImageWidth / 2 ), this.Position.Y + ( ImageHeight / 2 ) );
            }
        }

        public EntityType EntityType {
            get;
            private set;
        }

        /// <summary>
        ///     The current direction this entity is facing.
        /// </summary>
        public Degrees Heading {
            get;
            set;
        }

        public PointF Home {
            get;
            set;
        }

        public PointF Position {
            get;
            set;
        }

        public Single VelocityX {
            get;
            set;
        }

        public Single VelocityY {
            get;
            set;
        }

        private void AngleTowardsHome() {

            //we have our current position this.Position
            //we have our current this.Heading
            //and we have a this.Home
            //... so how do we calc all that?

            // turn left, or turn right?
            if ( this.Home.X < this.Position.X ) {
                this.Heading = this.Heading.RotateLeft();
            }
            else if ( this.Home.X > this.Position.X ) {
                this.Heading = this.Heading.RotateRight();
            }

            if ( this.Home.Y < this.Position.Y ) {
                this.Heading = this.Heading.RotateRight();
            }
            else if ( this.Home.Y > this.Position.Y ) {
                this.Heading = this.Heading.RotateLeft();
            }
        }

        private void LimitSpeed() {
            if ( this.VelocityX > 3 ) {
                this.VelocityX = 3;
            }
            else if ( this.VelocityX < -3 ) {
                this.VelocityX = -3;
            }

            if ( this.VelocityY > 3 ) {
                this.VelocityY = 3;
            }
            else if ( this.VelocityY < -3 ) {
                this.VelocityY = -3;
            }
        }

        /// <summary>
        ///     Move calculations
        /// </summary>
        private void MoveTowardsHome() {

            //the speed limit
            this.LimitSpeed();

            this.VelocityX *= 0.99f; //taper off speed
            this.VelocityY *= 0.99f; //taper off speed

            this.VelocityX += ( Randem.NextSingle() - 0.5f ) * 0.4f;
            this.VelocityY += ( Randem.NextSingle() - 0.5f ) * 0.4f;

            this.Position = new PointF( this.Position.X + this.VelocityX, this.Position.Y + this.VelocityX );
        }

        private void Think( object sender, EventArgs eventArgs ) {
            this.BrainTimer.Stop();
            try {
                if ( Randem.NextBoolean() ) {
                    this.AngleTowardsHome();
                }
                else {
                    this.MoveTowardsHome();
                }
            }
            finally {
                this.BrainTimer.Start();
            }
        }
    }
}