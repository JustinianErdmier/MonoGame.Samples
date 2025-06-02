using BoxCollider;

using Microsoft.Xna.Framework;

namespace ShipGame.Core.Game;

/// <remarks>This class was first seen in the Chase Camera sample.</remarks>
public class ChaseCamera
{
    #region Matrix properties

    /// <summary>View transform matrix.</summary>
    public Matrix View { get; private set; }

    #endregion

    #region Chased object properties (set externally each frame)

    /// <summary>Position of the object being chased.</summary>
    public Vector3 ChasePosition { get; set; }

    /// <summary>Direction the chased object is facing.</summary>
    public Vector3 ChaseDirection { get; set; }

    /// <summary>Chased object's Up vector.</summary>
    public Vector3 Up { get; set; } = Vector3.Up;

    #endregion

    #region Desired camera positioning (set when creating camera or changing view)

    /// <summary>Desired camera position in the chased object's coordinate system.</summary>
    public Vector3 DesiredPositionOffset { get; set; } = new(x: 0, y: 2.0f, z: 2.0f);

    // ReSharper disable once UnusedMember.Global
    /// <summary>Desired camera position in world space.</summary>
    public Vector3 DesiredPosition
    {
        get
        {
            // Ensure correct value even if update has not been called this frame
            UpdateWorldPositions();

            return _desiredPosition;
        }
    }

    private Vector3 _desiredPosition;

    /// <summary>Look at the point in the chased object's coordinate system.</summary>
    public Vector3 LookAtOffset { get; set; } = new(x: 0, y: 2.8f, z: 0);

    /// <summary>Look at the point in world space.</summary>
    private Vector3 LookAt
    {
        get
        {
            // Ensure correct value even if update has not been called this frame
            UpdateWorldPositions();

            return _lookAt;
        }
    }

    private Vector3 _lookAt;

    #endregion

    #region Camera physics (typically set when creating camera)

    /// <summary>Physics coefficient which controls the influence of the camera's position over the spring force. The stiffer the spring, the closer it will stay to the chased object.</summary>
    public float Stiffness { get; set; } = 1800.0f;

    /// <summary>Physics coefficient which approximates internal friction of the spring. Sufficient damping will prevent the spring from oscillating infinitely.</summary>
    public float Damping { get; set; } = 600.0f;

    /// <summary>Mass of the camera body. Heaver objects require stiffer springs with less damping to move at the same rate as lighter objects.</summary>
    public float Mass { get; set; } = 50.0f;

    #endregion

    #region Current camera properties (updated by camera physics)

    /// <summary>Position of the camera in world space.</summary>
    public Vector3 Position { get; private set; }

    private Vector3 _position;

    /// <summary>Velocity of camera.</summary>
    private Vector3 Velocity { get; set; }

    #endregion

    #region Methods

    /// <summary>Rebuilds object space values in world space. Invoke before publicly returning or privately accessing world space values.</summary>
    private void UpdateWorldPositions()
    {
        // Construct a matrix to transform from object space to world space
        Matrix transform = Matrix.Identity;

        transform.Forward = ChaseDirection;
        transform.Up      = Up;
        transform.Right   = Vector3.Cross(Up, ChaseDirection);

        // Calculate desired camera properties in world space
        _desiredPosition = ChasePosition + Vector3.TransformNormal(DesiredPositionOffset, transform);
        _lookAt          = ChasePosition + Vector3.TransformNormal(LookAtOffset, transform);
    }

    /// <summary>Rebuilds camera's view and projection matrices.</summary>
    private void UpdateMatrices() => View = Matrix.CreateLookAt(Position, LookAt, Up);

    /// <summary>
    ///     Forces the camera to be at the desired position and to stop moving. This is useful when the chased object is first created as well as after it has been teleported.
    ///     Failing to call this after a large change to the chased object's position will result in the camera quickly flying across the world.
    /// </summary>
    public void Reset()
    {
        UpdateWorldPositions();

        // Stop the motion
        Velocity = Vector3.Zero;

        // Force the desired position
        _position = _desiredPosition;

        UpdateMatrices();
    }

    /// <summary>
    ///     Animates the camera from its current position towards the desired offset behind the chased object. The camera's animation is controlled by a simple physical spring
    ///     attached to the camera and anchored to the desired position.
    /// </summary>
    public void Update(float elapsedTime, CollisionMesh? collision)
    {
        UpdateWorldPositions();

        // Calculate spring force
        Vector3 stretch = _position            - _desiredPosition;
        Vector3 force   = -Stiffness * stretch - Damping * Velocity;

        // Apply acceleration
        Vector3 acceleration = force / Mass;

        Velocity += acceleration * elapsedTime;

        // Apply velocity
        _position += Velocity * elapsedTime;
        Position  =  _position;

        // test camera for collision with the world
        if (collision is not null
            && collision.PointIntersect(_lookAt, _position, out float _, out Vector3 collisionPoint, out Vector3 _))
        {
            Vector3 dir = Vector3.Normalize(collisionPoint - _lookAt);

            Position = collisionPoint - 10 * dir;
        }

        UpdateMatrices();
    }

    #endregion
}
