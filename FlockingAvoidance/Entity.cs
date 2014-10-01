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
// "FlockingAvoidance/Entity.cs" was last cleaned by Rick on 2014/10/01 at 12:37 PM

#endregion License & Information

namespace FlockingAvoidance {

    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Threading;
    using Librainian.Annotations;
    using Librainian.Maths;
    using Librainian.Measurement.Frequency;
    using Librainian.Measurement.Spatial;
    using Librainian.Threading;

    /// <summary>
    ///     Flocking Item
    /// </summary>
    /// <copyright>http://sachabarbs.wordpress.com/2010/03/01/wpf-a-fun-little-boids-type-thing/</copyright>
    public class Entity {

        public enum ThinkingStates {
            Nothing = 0,
            Tired,
            Sleeping,
            WakingUp,
            AngleTowardsHome,
            HeadingHome,
            Fleeing,
            Hungry,
            Exploring,
            FindingFood
        }

        public enum PossibleGoal {
            FindHome,
            FindFood,
            FindSleep,
        }

        public PossibleGoal WantGoal { get; set; }

        public const Int32 ImageHeight = 30;    //TODO these dont belong here.

        public const Int32 ImageWidth = 30;     //TODO these dont belong here.

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="entityType"></param>
        public Entity( EntityType entityType ) {
            this.EntityType = entityType;

            this.DoTeleport();

            this.VelocityX = 3;
            this.VelocityY = 3;

            this.ThinkingState = ThinkingStates.AngleTowardsHome;
            this.PreviousThinkingState = ThinkingStates.Nothing;

            this.FindNewHome();

            this.ImageBoundary = new Rect( 0, 0, ImageWidth, ImageHeight );

            this.BrainTimer = new DispatcherTimer( DispatcherPriority.Background ) {
                Interval = Hertz.ThreeHundredThirtyThree, 
            };
            this.BrainTimer.Tick += this.Think;
            this.BrainTimer.Start();
        }

        /// <summary>
        /// Pick a new position on the canvas.
        /// </summary>
        protected void DoTeleport() {
            this.Position = new PointF( Randem.NextSingle() * WorldCanvas.CANVAS_WIDTH, Randem.NextSingle() * WorldCanvas.CANVAS_HEIGHT );
        }

        /// <summary>
        ///     The direction towards <see cref="Home" />.
        /// </summary>
        public Degrees Bearing {
            get;
            set;
        }

        public Rect ImageBoundary {
            get;
            private set;
        }

        public Rect BoundaryBorder {
            get;
            private set;
        }

        [NotNull]
        public DispatcherTimer BrainTimer {
            get;
            private set;
        }

        /*
                /// <summary>
                ///     Centre of item
                /// </summary>
                public PointF CenterPoint {
                    get {
                        return new PointF( this.Position.X + ( ImageWidth / 2 ), this.Position.Y + ( ImageHeight / 2 ) );
                    }
                }
        */

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

        /// <summary>
        /// The location of home.
        /// </summary>
        public PointF Home {
            get;
            set;
        }

        /// <summary>
        /// The current position
        /// </summary>
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

        //what we are doing
        //what we want to do
        //what we need to do
        //stats:
        //  health,
        //  energy,
        //  fatigue (or is fatigue just the lack of energy?)

        public ThinkingStates ThinkingState {
            get;
            set;
        }

        public ThinkingStates PreviousThinkingState {
            get;
            set;
        }

        private void AngleTowardsHome() {
            //we have our current position this.Position
            //we have our current this.Heading
            //and we have a this.Home
            //... so how do we calc all that?

            var oldAngle = this.Bearing.Value;

            var newAngle = this.Position.FindAngle( this.Home );

            if ( oldAngle.Near( newAngle ) ) {
                return;
            }
            if ( newAngle < oldAngle ) {
                this.Bearing = new Degrees( oldAngle - 1 );
            }
            else if ( newAngle > oldAngle ) {
                this.Bearing = new Degrees( oldAngle + 1 );
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
            this.Position = new PointF( this.Position.X + this.VelocityX, this.Position.Y + this.VelocityX );
            CheckBoundary();
        }

        private void CheckBoundary() {
            if ( this.Position.X > WorldCanvas.CANVAS_WIDTH  ) {
                this.Position = new PointF( 1, this.Position.Y );
            }
            if ( this.Position.Y > WorldCanvas.CANVAS_HEIGHT ) {
                this.Position = new PointF( this.Position.X, 1 );
            }
        }

        protected virtual void DoChangeSpeed() {
            //the speed limit
            this.LimitSpeed();

            this.VelocityX *= 0.99f; //taper off speed
            this.VelocityY *= 0.99f; //taper off speed

            this.VelocityX += ( Randem.NextSingle() - 0.5f ) * 0.4f;
            this.VelocityY += ( Randem.NextSingle() - 0.5f ) * 0.4f;
        }

        private void Think( object sender, EventArgs eventArgs ) {
            this.BrainTimer.Stop();
            try {
                switch ( this.ThinkingState ) {
                    case ThinkingStates.Nothing:
                        this.DoNothing();
                        break;

                    case ThinkingStates.Tired:
                        this.DoTired();
                        break;

                    case ThinkingStates.Sleeping:
                        this.DoWakingUp();
                        break;

                    case ThinkingStates.WakingUp:
                        this.DoHungry();
                        break;

                    case ThinkingStates.AngleTowardsHome:
                        this.DoHeadingHome();
                        break;

                    case ThinkingStates.HeadingHome:
                        this.DoTurnTowardsHome();
                        break;

                    case ThinkingStates.Fleeing:
                        this.DoFleeing();
                        break;

                    case ThinkingStates.Hungry:
                        this.DoFindingFood();
                        break;

                    case ThinkingStates.Exploring:
                        this.DoExploring();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }



            }
            finally {
                this.BrainTimer.Start();
            }
        }

        protected virtual void DoFindingFood() {
            //TODO are we full?
            if ( IsFull() ) {
                this.ChangeStateTo( ThinkingStates.Tired );
                return;
            }
            this.ChangeStateTo( ThinkingStates.FindingFood );
        }

        private static Boolean IsFull() {
            return Randem.NextBoolean();
        }

        protected virtual void DoFleeing() {
            this.DoChangeSpeed();
            //TODO are we being chased any more?
            this.ChangeStateTo( ThinkingStates.Tired );
        }

        private void DoTurnTowardsHome() {
            this.AngleTowardsHome();
            this.ChangeStateTo( ThinkingStates.HeadingHome );
        }

        protected virtual void DoHeadingHome() {
            this.MoveTowardsHome();
            if ( this.Position.Near( this.Home ) ) {
                switch ( Randem.Next( 4 ) ) {
                    case 0:
                        this.ChangeStateTo( ThinkingStates.Exploring );
                        break;
                    case 1:
                        this.ChangeStateTo( ThinkingStates.Hungry );
                        break;
                    case 2:
                        this.ChangeStateTo( ThinkingStates.Tired );
                        break;
                    default:
                        this.ChangeStateTo( ThinkingStates.Nothing );
                        break;
                }
                return;
            }
            this.ChangeStateTo( ThinkingStates.AngleTowardsHome );
        }

        protected virtual void DoHungry() {
            //TODO how hungry are we?
            this.ChangeStateTo( ThinkingStates.FindingFood);
        }

        protected virtual void DoWakingUp() {
            this.ChangeStateTo( ThinkingStates.Nothing );
        }

        public void ChangeStateTo( ThinkingStates newState ) {
            Debug.WriteLine( "Entity changing from {0} to {1} to {2}.", this.PreviousThinkingState, this.ThinkingState, newState );
            this.PreviousThinkingState = this.ThinkingState;
            this.ThinkingState = newState;
        }

        protected virtual void DoTired() {
            this.ChangeStateTo( ThinkingStates.Sleeping );
        }

        protected virtual void DoNothing() {
            this.DoWander();
            this.ChangeStateTo( ThinkingStates.Exploring );
        }

        protected virtual void DoWander() {
            this.FindNewHome();
        }

        private void DoExploring() {
            this.FindNewHome();
            this.ChangeStateTo( ThinkingStates.Exploring );
        }

        private void FindNewHome() {
            this.Home = new PointF( Randem.NextSingle() * WorldCanvas.CANVAS_WIDTH, Randem.NextSingle() * WorldCanvas.CANVAS_HEIGHT );
        }
    }
}