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
    using System.Windows;
    using Librainian.Threading;

    /// <summary>
    ///     Flocking Item
    /// </summary>
    /// <copyright>http://sachabarbs.wordpress.com/2010/03/01/wpf-a-fun-little-boids-type-thing/</copyright>
    public class FlockItem {
        public const Int32 ITEM_HEIGHT = 30;

        public const Int32 ITEM_WIDTH = 30;

        /// <summary>
        ///     Ctor
        /// </summary>
        public FlockItem() {
            this.X = Randem.NextDouble() * FlockingAvoidanceCanvas.CANVAS_WIDTH;
            this.Y = Randem.NextDouble() * FlockingAvoidanceCanvas.CANVAS_HEIGHT;
            this.VX = 0;
            this.VY = 0;
            this.Move();
        }

        /// <summary>
        ///     Centre of item
        /// </summary>
        public Point CentrePoint { get { return new Point( this.X + ( ITEM_WIDTH / 2 ), this.Y + ( ITEM_HEIGHT / 2 ) ); } }

        public Double VX { get; set; }

        public Double VY { get; set; }

        public Double X { get; set; }

        public Double Y { get; set; }

        /// <summary>
        ///     Work out an angle
        /// </summary>
        public static Int32 AngleItem( Double vx, Double vy ) {
            return ( int )( Randem.NextDouble() * 30 );
        }

        /// <summary>
        ///     Move calculations
        /// </summary>
        public void Move() {
            //the speed limit
            if ( this.VX > 3 ) {
                this.VX = 3;
            }
            if ( this.VX < -3 ) {
                this.VX = -3;
            }
            if ( this.VY > 3 ) {
                this.VY = 3;
            }
            if ( this.VY < -3 ) {
                this.VY = -3;
            }

            this.X += this.VX;
            this.Y += this.VY;
            this.VX *= 0.9;
            this.VY *= 0.9;
            this.VX += ( Randem.NextDouble() - 0.5 ) * 0.4;
            this.VY += ( Randem.NextDouble() - 0.5 ) * 0.4;

            //go towards center
            this.X = ( this.X * 500 + FlockingAvoidanceCanvas.CANVAS_WIDTH / 2 ) / 501;
            this.Y = ( this.Y * 500 + FlockingAvoidanceCanvas.CANVAS_HEIGHT / 2 ) / 501;
        }
    }
}
