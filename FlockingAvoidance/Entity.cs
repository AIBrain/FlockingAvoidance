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
// "FlockingAvoidance/Entity.cs" was last cleaned by Rick on 2014/10/02 at 4:01 PM

#endregion License & Information

namespace FlockingAvoidance {

    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Dynamic;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Windows;
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
    [DataContract( IsReference = true )]
    public class Entity : UIElement, IEquatable<Entity> {

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
        public const Single MaxSpeed = 5;
        public const Single MinSpeed = Single.Epsilon;
        public static readonly AutoNumber Identities = new AutoNumber();

        public readonly UInt64 ID;
        private float _velocityX;
        private float _velocityY;

        public long NumberOfChanges;

        public void NotifyThereAreChanges( long changes = 1 ) {
            //Interlocked.Add( ref NumberOfChanges, changes );
        }

        public void AcknowledgeChanges( long changes = 1 ) {
            //Interlocked.Add( ref NumberOfChanges, -changes );
        }

        private Entity() {
            this.ID = Identities.Next();

            this.EntityType = Randem.RandomEnum<EntityType>();

            this.VelocityX = 3;
            this.VelocityY = 3;

            this.ImageBoundary = new Rect( 0, 0, Radius, Radius );

            this.PossibleState = PossibleStates.TurnTowardsHome;
            this.PreviousPossibleState = PossibleStates.Nothing;

            //this.DoTeleport();
            //this.FindNewHome();
            this.Home = new PointF( 500, 500 );
            this.Position = new PointF( 100, 100 );
            this.Heading = new Degrees( Randem.Next( 1, 360 ) );

            //var bob = new Visual3D();

            //this.PersonalThoughts.Question = "Hello?";
            //this.PersonalThoughts.What = "what?";
            //Console.WriteLine( this.PersonalThoughts.Question );

            this.ReactionTime = new Milliseconds( Randem.Next( 10, 1000 ) );

            this.BrainTimer = new DispatcherTimer( DispatcherPriority.Background ) {
                Interval = this.ReactionTime,
            };
            this.BrainTimer.Tick += this.Do;
            this.BrainTimer.Start();

            this.NotifyThereAreChanges();
        }

        internal readonly dynamic PersonalThoughts = new ExpandoObject();
        private Degrees _heading;

        //TODO these dont belong here.

        ///// <summary>
        /////     The direction towards <see cref="Home" />.
        ///// </summary>
        //public Degrees Bearing {
        //    get;
        //    set;
        //}

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
        ///     The current direction (<see cref="Degrees"/>) this entity is facing.
        /// </summary>
        public Degrees Heading {
            get {
                return this._heading;
            }
            set {
                Debug.WriteLine( "Heading is now {0}", value );
                this._heading = value;
            }
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
        ///     The entity's current position
        /// </summary>
        public PointF Position {
            get;
            set;
        }

        public PossibleStates PossibleState {
            get;
            set;
        }

        ///// <summary>
        ///// Participates in rendering operations when overridden in a derived class.
        ///// </summary>
        //protected override void OnUpdateModel() {
        //    base.OnUpdateModel();

        //    this.
        //}

        //
        //TODO what we are doing
        //TODO what we want to do
        //TODO what we need to do
        //TODO stats:
        //TODO  health,
        //TODO   energy,
        //TODO   fatigue (or is fatigue just the lack of energy?)

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

        public PossibleGoal WantGoal {
            get;
            set;
        }

        public static Entity Create() {
            return new Entity();
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
            Debug.WriteLine( "Entity {0} changing state from {1} to {2}.", this.ID, this.PossibleState, newState );
            this.PreviousPossibleState = this.PossibleState;
            this.PossibleState = newState;
            this.NotifyThereAreChanges();
        }

        protected virtual void AmFleeing() {
            if ( Randem.NextBoolean() ) {
                this.RandomlyChangeSpeed();
            }
            else {
                this.RandomlyChangeDirection();
            }

            //TODO are we being chased any more?
            this.ChangeStateTo( PossibleStates.Tired );
        }

        private void RandomlyChangeDirection() {
            this.AdjustHeadingTowards( WorldCanvas.GiveRandomSpot() );
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
            this.MoveForward();
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

        protected virtual void RandomlyChangeSpeed() {
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
            this.VelocityX -= Randem.NextSingle();
            this.VelocityY -= Randem.NextSingle();
        }

        protected virtual void DoingNothing() {
            this.ChangeStateTo( PossibleStates.Exploring );
        }

        /// <summary>
        ///     <para>Pick a new position on the canvas.</para>
        /// <para>this.Position = WorldCanvas.GiveRandomSpot();</para>
        /// </summary>
        protected void DoTeleport() {
            this.Position = WorldCanvas.GiveRandomSpot();
        }

        /// <summary>
        /// <para>this.FindNewHome();</para>
        /// </summary>
        protected virtual void DoWander() {
            this.FindNewHome();
        }

        private static Boolean IsFull() {
            return Randem.NextBoolean();
        }

        private void AmExploring() {
            this.FindNewHome();
            this.ChangeStateTo( PossibleStates.HeadingHome );
        }

        private void AmHeadingHome() {
            this.AdjustHeadingTowards( this.Home );
            this.ChangeStateTo( PossibleStates.TurnTowardsHome );
        }

        private void AdjustHeadingTowards( PointF target ) {
            var oldAngle = this.Heading.Value;

            var newAngle = this.Position.FindAngle( target );

            if ( oldAngle.Near( newAngle ) ) {
                return;
            }

            this.Heading = new Degrees( oldAngle.CompassAngleLerp( newAngle, 1 ) );

            //if ( newAngle < oldAngle ) {
            //    this.Heading = new Degrees( oldAngle - 1 );
            //}
            //else if ( newAngle > oldAngle ) {
            //    this.Heading = new Degrees( oldAngle + 1 );
            //}
        }

        /// <summary>
        ///     Move calculations
        /// </summary>
        private void MoveForward( Single distance = 1 ) {
            //this.Position = new PointF( this.Position.X + this.VelocityX, this.Position.Y + this.VelocityX );
            var past = this.Position;

            this.Position = new PointF(
                 ( float )( past.X + Math.Sin( this.Heading ) * distance ),
                 ( float )( past.Y + Math.Cos( this.Heading ) * distance )
                 );

            this.CheckBoundary();
        }

        private void CheckBoundary() {
            if ( this.Position.X >= WorldCanvas.CANVAS_WIDTH ) {
                this.Position = new PointF( 1, this.Position.Y );
            }
            else if ( this.Position.X <= 0 ) {
                this.Position = new PointF( WorldCanvas.CANVAS_WIDTH, this.Position.Y );
            }

            if ( this.Position.Y > WorldCanvas.CANVAS_HEIGHT ) {
                this.Position = new PointF( this.Position.X, 1 );
            }
            else if ( this.Position.Y <= 0 ) {
                this.Position = new PointF( this.Position.X, WorldCanvas.CANVAS_HEIGHT );
            }
        }

        private void Do( object sender, EventArgs eventArgs ) {
            this.BrainTimer.Stop();
            try {
                this.Do();
            }
            finally {
                this.BrainTimer.Start();
            }
        }

        private void Do() {
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

        private void FindNewHome() {
            this.Home = WorldCanvas.GiveRandomSpot();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals( Entity other ) {
            return Equals( this, other );
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        bool IEquatable<Entity>.Equals( Entity other ) {
            return Equals( this, other );
        }

        ///// <summary>
        ///// Serves as a hash function for a particular type. 
        ///// </summary>
        ///// <returns>
        ///// A hash code for the current object.
        ///// </returns>
        //[Pure]
        //public override int GetHashCode() {
        //    unchecked {
        //        return ( int )this.ID;
        //    }
        //}

        /// <summary>
        /// static equality test
        /// </summary>
        /// <param name="this"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        [Pure]
        public static Boolean Equals( Entity @this, Entity @that ) {
            if ( ReferenceEquals( @this, @that ) ) {
                return true;
            }
            if ( null == @this || null == @that ) {
                return false;
            }
            return @this.ID == @that.ID;
        }
    }
}