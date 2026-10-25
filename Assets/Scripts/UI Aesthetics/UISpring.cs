using UnityEngine;

/// Small spring helper used by ButtonPop and VirtualJoystick.
/// Call Step() every frame: the value gets pulled toward the target and overshoots a little (the "bounce").
/// Higher stiffness = snappier. Lower damping = bouncier.
public static class UISpring
{
    public static void Step(ref float value, ref float velocity, float target,
                            float stiffness, float damping, float dt)
    {
        dt = Mathf.Min(dt, 1f / 30f); // keeps the spring stable if a frame hitches
        velocity += ((target - value) * stiffness - velocity * damping) * dt;
        value += velocity * dt;
    }

    public static void Step(ref Vector2 value, ref Vector2 velocity, Vector2 target,
                            float stiffness, float damping, float dt)
    {
        dt = Mathf.Min(dt, 1f / 30f);
        velocity += ((target - value) * stiffness - velocity * damping) * dt;
        value += velocity * dt;
    }
}
