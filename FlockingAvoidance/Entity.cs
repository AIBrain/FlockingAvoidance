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
// "FlockingAvoidance/Entity.cs" was last cleaned by Rick on 2014/10/01 at 2:33 PM
#endregion

namespace FlockingAvoidance {
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Media.Media3D;
    using System.Windows.Threading;
    using Librainian.Annotations;
    using Librainian.Maths;
    using Librainian.Measurement.Spatial;
    using Librainian.Measurement.Time;
    using Librainian.Threading;

    /// <summary>
    ///     Flocking Item
    /// </summary>
    /// <copyright>http://sachabarbs.wordpress.com/2010/03/01/wpf-a-fun-little-boids-type-thing/</copyright>
    public class Entity {
        private float _velocityX;
        private float _velocityY;

        public enum PossibleGoal {
            FindHome,
            FindFood,
            FindSleep,
        }

        public enum PossibleStates {
            Nothing = 0,
            Tired,
            Sleeping,
            WakingUp,
            TurnTowardsHome,
            HeadingHome,
            Fleeing,
            Hungry,
            Exploring,
            FindingFood
        }

        public const Single Radius = 30;

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="entityType"></param>
        public Entity( EntityType entityType ) {
            this.EntityType = entityType;

            this.VelocityX = 3;
            this.VelocityY = 3;

            this.ImageBoundary = new Rect( 0, 0, Radius, Radius );

            this.PossibleState = PossibleStates.TurnTowardsHome;
            this.PreviousPossibleState = PossibleStates.Nothing;

            this.DoTeleport();
            this.FindNewHome();

            //var bob = new Visual3D();

            this.ReactionTime = new Milliseconds( Randem.Next( 10, 1000 ) );

            this.BrainTimer = new DispatcherTimer( DispatcherPriority.Background ) {
                Interval = this.ReactionTime,
            };
            this.BrainTimer.Tick += this.Do;
            this.BrainTimer.Start();
        }

        //TODO these dont belong here.

        /// <summary>
        ///     The direction towards <see cref="Home" />.
        /// </summary>
        public Degrees Bearing {
            get;
            set;
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
        ///     The location of home.
        /// </summary>
        public PointF Home {
            get;
            set;
        }

        public Rect ImageBoundary {
            get;
            private set;
        }

        /// <summary>
        ///     The current position
        /// </summary>
        public PointF Position {
            get;
            set;
        }

        public PossibleStates PossibleState {
            get;
            set;
        }

        //TODO
        //what we are doing
        //what we want to do
        //what we need to do
        //stats:
        //  health,
        //  energy,
        //  fatigue (or is fatigue just the lack of energy?)

        public PossibleStates PreviousPossibleState {
            get;
            set;
        }

        public Milliseconds ReactionTime {
            get;
            set;
        }

        public Single VelocityX {
            get {
                return this._velocityX;
            }
            set {
                if ( value > MaxSpeed ) {
                    value = MaxSpeed;
                }
                else if ( value < MinSpeed ) {
                    value = MinSpeed;
                }
                this._velocityX = value;
            }
        }

        public Single VelocityY {
            get {
                return this._velocityY;
            }
            set {
                if ( value > MaxSpeed ) {
                    value = MaxSpeed;
                }
                else if ( value < MinSpeed ) {
                    value = MinSpeed;
                }
                this._velocityY = value;
            }
        }

        //TODO these dont belong here.
        public PossibleGoal WantGoal {
            get;
            set;
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

        public void ChangeStateTo( PossibleStates newState ) {
            Debug.WriteLine( "Entity changing state from {0} to {1} to {2}.", this.PreviousPossibleState, this.PossibleState, newState );
            this.PreviousPossibleState = this.PossibleState;
            this.PossibleState = newState;
        }

        protected virtual void AmFleeing() {
            if ( Randem.NextBoolean() ) {
                this.DoChangeSpeed();
            }
            else {
                this.DoChangeDirection();
            }

            //TODO are we being chased any more?
            this.ChangeStateTo( PossibleStates.Tired );
        }

        private void DoChangeDirection() {
            this.AdjustBearingTowards( WorldCanvas.PickRandomSpot() );
        }

        protected virtual void AmHungry() {
            //TODO are we full?
            if ( IsFull() ) {
                this.ChangeStateTo( PossibleStates.Tired );
                return;
            }
            this.ChangeStateTo( PossibleStates.FindingFood );
        }

        protected virtual void AmSleeping() {
            this.ChangeStateTo( PossibleStates.Nothing );
        }

        protected virtual void AmTired() {
            this.ChangeStateTo( PossibleStates.Sleeping );
        }

        protected virtual void AmTurningTowardsHome() {
            //this.MoveTowardsHome();
            //TODO
            if ( this.Position.Near( this.Home ) ) {
                switch ( Randem.Next( 4 ) ) {
                    case 0:
                        this.ChangeStateTo( PossibleStates.Exploring );
                        break;

                    case 1:
                        this.ChangeStateTo( PossibleStates.Hungry );
                        break;

                    case 2:
                        this.ChangeStateTo( PossibleStates.Tired );
                        break;

                    default:
                        this.ChangeStateTo( PossibleStates.Nothing );
                        break;
                }
                return;
            }
            this.ChangeStateTo( PossibleStates.HeadingHome );
        }

        protected virtual void AmWakingUp() {
            //TODO how hungry are we?
            this.ChangeStateTo( PossibleStates.FindingFood );
        }

        protected virtual void DoChangeSpeed() {
            if ( Randem.NextBoolean() ) {
                this.IncreaseSpeed();    
            }
            else {
                this.DecreaseSpeed();
            }
       }

        private void IncreaseSpeed() {
            this.VelocityX += Randem.NextSingle();
            this.VelocityY += Randem.NextSingle();
        }

        private void DecreaseSpeed() {
            this.VelocityX *= 0.99f;
            this.VelocityY *= 0.99f;
        }

        protected virtual void DoingNothing() {
            this.DoWander();
            this.ChangeStateTo( PossibleStates.Exploring );
        }

        /// <summary>
        ///     Pick a new position on the canvas.
        /// </summary>
        protected void DoTeleport() {
            this.Position = WorldCanvas.PickRandomSpot();
        }

        protected virtual void DoWander() {
            this.FindNewHome();
        }

        private static Boolean IsFull() {
            return Randem.NextBoolean();
        }

        private void AmExploring() {
            this.FindNewHome();
            this.ChangeStateTo( PossibleStates.Exploring );
        }

        private void AmHeadingHome() {
            this.AdjustBearingTowards( this.Home );
            this.ChangeStateTo( PossibleStates.TurnTowardsHome );
        }

        private void AdjustBearingTowards( PointF target ) {
            var oldAngle = this.Bearing.Value;

            var newAngle = this.Position.FindAngle( target );

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

        /// <summary>
        ///     Move calculations
        /// </summary>
        private void MoveForward( PointF target ) {
            this.Position = new PointF( this.Position.X + this.VelocityX, this.Position.Y + this.VelocityX );
            this.CheckBoundary();
        }

        private void CheckBoundary() {
            if ( this.Position.X > WorldCanvas.CANVAS_WIDTH ) {
                this.Position = new PointF( 1, this.Position.Y );
            }
            if ( this.Position.Y > WorldCanvas.CANVAS_HEIGHT ) {
                this.Position = new PointF( this.Position.X, 1 );
            }
        }

        private void Do( object sender, EventArgs eventArgs ) {
            this.BrainTimer.Stop();
            try {
                switch ( this.PossibleState ) {
                    case PossibleStates.Nothing:
                        this.DoingNothing();
                        break;

                    case PossibleStates.Tired:
                        this.AmTired();
                        break;

                    case PossibleStates.Sleeping:
                        this.AmSleeping();
                        break;

                    case PossibleStates.WakingUp:
                        this.AmWakingUp();
                        break;

                    case PossibleStates.TurnTowardsHome:
                        this.AmTurningTowardsHome();
                        break;

                    case PossibleStates.HeadingHome:
                        this.AmHeadingHome();
                        break;

                    case PossibleStates.Fleeing:
                        this.AmFleeing();
                        break;

                    case PossibleStates.Hungry:
                        this.AmHungry();
                        break;

                    case PossibleStates.Exploring:
                        this.AmExploring();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            finally {
                this.BrainTimer.Start();
            }
        }

        private void FindNewHome() {
            this.Home = WorldCanvas.PickRandomSpot();
        }

        public const Single MaxSpeed = 5;
        public const Single MinSpeed = Single.Epsilon;

        
        

    }
}
