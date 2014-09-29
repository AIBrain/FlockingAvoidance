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
// "FlockingAvoidance/FlockItem.cs" was last cleaned by Rick on 2014/09/28 at 3:16 AM
#endregion

namespace FlockingAvoidance {
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using Librainian.Measurement.Frequency;
    using Librainian.Threading;
    using Timer = System.Timers.Timer;

    /// <summary>
    ///     Flocking Item
    /// </summary>
    /// <copyright>http://sachabarbs.wordpress.com/2010/03/01/wpf-a-fun-little-boids-type-thing/</copyright>
    public class Entity {

        public EntityType EntityType {
            get;
            private set;
        }
        public const Int32 ItemHeight = 30;

        public const Int32 ItemWidth = 30;

        public Stopwatch BrainStopwatch = new Stopwatch();

        public double angle = Randem.Next(0, 360);

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="entityType"></param>
        public Entity( EntityType entityType ) {
            this.EntityType = entityType;
            this.Position = new Point( Randem.NextDouble() * WorldCanvas.CANVAS_WIDTH, Randem.NextDouble() * WorldCanvas.CANVAS_HEIGHT );

            this.VelocityX = 1;
            this.VelocityY = 1;

            this.Home = new Point( Randem.NextDouble() * WorldCanvas.CANVAS_WIDTH, Randem.NextDouble() * WorldCanvas.CANVAS_HEIGHT );

            this.BrainTimer = FluentTimers.Create( Hertz.ThreeHundredThirtyThree, this.Move ).AutoResetting().AndStart();
        }

        public Timer BrainTimer {
            get;
            private set;
        }

        /// <summary>
        ///     Centre of item
        /// </summary>
        public Point CenterPoint {
            get {
                return new Point( this.Position.X + ( ItemWidth / 2 ), this.Position.Y + ( ItemHeight / 2 ) );
            }
        }

        public Double VelocityX {
            get;
            set;
        }

        public Double VelocityY {
            get;
            set;
        }

        public Point Home {
            get;
            set;
        }

        public Point Position {
            get;
            set;
        }

        //public Double Y { get; set; }

        ///// <summary>
        /////     Work out an angle
        ///// </summary>
        //public static Int32 AngleItem( Double vx, Double vy ) {
        //    return ( int )( Randem.NextDouble() * 360 );
        //}

        /// <summary>
        ///     Move calculations
        /// </summary>
        public void Move() {
            //the speed limit
            if ( this.VelocityX > 3 ) {
                this.VelocityX = 3;
            }
            if ( this.VelocityX < -3 ) {
                this.VelocityX = -3;
            }
            if ( this.VelocityY > 3 ) {
                this.VelocityY = 3;
            }
            if ( this.VelocityY < -3 ) {
                this.VelocityY = -3;
            }

            //this.Position = new Point( this.Position.X + this.VelocityX, this.Position.Y + VelocityY );

            //this.VelocityX *= 0.9;
            //this.VelocityY *= 0.9;
            this.VelocityX += ( Randem.NextDouble() - 0.5 ) * 0.4;
            this.VelocityY += ( Randem.NextDouble() - 0.5 ) * 0.4;

            switch ( Randem.Next( 0, 4 ) ) {
                case 0:
                    if ( this.Position.X < this.Home.X ) {
                        this.angle++;
                    }
                    break;
                case 1:
                    if ( this.Position.X > this.Home.X ) {
                        this.angle--;
                    }
                    break;
                case 2:
                    if ( this.Position.Y > this.Home.Y ) {
                        this.angle++;
                    }
                    break;
                case 3:
                    if ( this.Position.Y > this.Home.Y ) {
                        this.angle--;
                    }
                    break;
            }


            //this.X = ( this.X * 500 + WorldCanvas.CANVAS_WIDTH / 2 ) / 501;
            //this.Y = ( this.Y * 500 + WorldCanvas.CANVAS_HEIGHT / 2 ) / 501;

            this.Position = new Point( this.Position.X + this.VelocityX, this.Position.Y + this.VelocityX );
        }
    }
}
